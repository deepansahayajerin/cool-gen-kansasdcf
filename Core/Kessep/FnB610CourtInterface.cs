// Program: FN_B610_COURT_INTERFACE, ID: 372565354, model: 746.
// Short name: SWEF610B
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
/// A program: FN_B610_COURT_INTERFACE.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB610CourtInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B610_COURT_INTERFACE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB610CourtInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB610CourtInterface.
  /// </summary>
  public FnB610CourtInterface(IContext context, Import import, Export export):
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
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------  --------------------
    // ??/??/95  Leslie Flagler - MTW               Initial Development
    // 02/14/96  Bryan Fristrup - MTW               Initial Standards
    // Retrofit and Collection
    //  Adjustment Processing.
    // 08/14/96  Holly Kennedy - MTW		     Test and make minor
    // fixes due to data model changes.
    // 12/98     Rich Galichon                      Test and make fixes.
    // *****************************************************************
    // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
    // to ERROR Report
    // 10/25/01     I00129648  pphinney  Cause ABEND on ERROR condition
    // 12/03/01     WR010504 - pphinney  RETRO Processing  - Added Exit States
    // *****************************************************************
    // 12/03/01     WR010504 - RETRO -12/03/01  - Added Exit States
    // cse_person_nf_rb    and    fn0000_error_on_event_creation_with_Rollback
    // *****************************************************************
    // *************************************************************************************************
    // 06/14/2004  PR 002020821 Added exit state checking to see if ADABAS was 
    // unavailable.
    // **********************************************************************************************
    // ******************************************************************************************************************
    // 02/10/2005   M J Quinn   Archive changes that will be activated in the
    // future are included in                        this code, but have been
    // disabled.   It will be activated when archiving is ready to be
    // implemented.     There are two case statements to
    // be activated.  They are marked 1 of 2 and 2 of 2.
    // ******************************************************************************************************************
    // -------------------------------------------------------------------------------------
    // 06/17/2009     CQ 7811   J Harden  add si_close_adabas at end of program
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ****************************
    // CHECK IF ADABAS IS AVAILABLE
    // ****************************
    UseCabReadAdabasPersonBatch();

    if (Equal(local.AbendData.AdabasResponseCd, "0148"))
    {
      ExitState = "ADABAS_UNAVAILABLE_RB";

      // *****************************************************************
      // 10/25/01     I00129648  pphinney  Cause ABEND on ERROR condition
      // *****************************************************************
      local.ReportProcessing.Action = "OPEN";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

        return;
      }

      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // PRINT the ERROR Message
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandling.RptDetail =
        TrimEnd("Program has ABENDED - ADABAS NOT Availiable") + " Exit State = " +
        local.ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // CLOSE the ERROR Report
      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "ERROR_CLOSING_FILE_AB";

        return;
      }

      // *****************************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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
      // *****************************************************************
      // 10/25/01     I00129648  pphinney  Cause ABEND on ERROR condition
      // *****************************************************************
      local.ReportProcessing.Action = "OPEN";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate = Now().Date;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

        return;
      }

      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // PRINT the ERROR Message
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandling.RptDetail =
        TrimEnd("Program has ABENDED - Program Processing Information") + " Exit State = " +
        local.ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // CLOSE the ERROR Report
      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // *****************************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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
        local.RestartCashReceiptSourceType.Code =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
        local.RestartCashReceiptEvent.SourceCreationDate =
          IntToDate((int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 11, 15)));
      }
    }
    else
    {
      // *****************************************************************
      // 10/25/01     I00129648  pphinney  Cause ABEND on ERROR condition
      // *****************************************************************
      local.ReportProcessing.Action = "OPEN";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

        return;
      }

      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // PRINT the ERROR Message
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandling.RptDetail =
        TrimEnd("Program has ABENDED - CheckPoint Restart Record") + " Exit State = " +
        local.ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFB610";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // CLOSE the ERROR Report
      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      // *****************************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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
      UseEabCourtInterfaceDrvr1();

      if (!Equal(local.PassArea.TextReturnCode, "00") && !
        Equal(local.PassArea.TextReturnCode, "10"))
      {
        ExitState = "FILE_POSITION_ERROR_WITH_RB";

        // *****************************************************************
        // 10/25/01     I00129648  pphinney  Cause ABEND on ERROR condition
        // *****************************************************************
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandling.ProgramName = "SWEFB610";
        local.ReportHandling.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

          return;
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // PRINT the ERROR Message
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.ReportHandling.RptDetail =
          TrimEnd("Program has ABENDED - EXTERNAL Position Input File") + " Exit State = " +
          local.ExitStateWorkArea.Message;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // CLOSE the ERROR Report
        local.ReportProcessing.Action = "CLOSE";
        local.ReportHandling.ProgramName = "SWEFB610";
        local.ReportHandling.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // *****************************************************************
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.ProgramCheckpointRestart.RestartInd = "N";
    }
    else
    {
      // *************************************
      // CALL EXTERNAL TO OPEN THE DRIVER FILE
      // *************************************
      local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
      UseEabCourtInterfaceDrvr2();

      if (!Equal(local.PassArea.TextReturnCode, "00"))
      {
        ExitState = "FILE_OPEN_ERROR_AB";

        // *****************************************************************
        // 10/25/01     I00129648  pphinney  Cause ABEND on ERROR condition
        // *****************************************************************
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandling.ProgramName = "SWEFB610";
        local.ReportHandling.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

          return;
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // PRINT the ERROR Message
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.ReportHandling.RptDetail =
          TrimEnd("Program has ABENDED - EXTERNAL Open Input File") + " Exit State = " +
          local.ExitStateWorkArea.Message;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // CLOSE the ERROR Report
        local.ReportProcessing.Action = "CLOSE";
        local.ReportHandling.ProgramName = "SWEFB610";
        local.ReportHandling.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // *****************************************************************
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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

    do
    {
      global.Command = "";

      // *************************************
      // CALL EXTERNAL TO READ THE DRIVER FILE
      // *************************************
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      local.CauseAbend.Flag = "N";
      UseEabCourtInterfaceDrvr3();

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          switch(local.RecordTypeReturned.Count)
          {
            case 1:
              break;
            case 9:
              break;
            default:
              local.Index.Count = 1;

              for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
                .ControlTotals.Count; ++local.ControlTotals.Index)
              {
                switch(local.Index.Count)
                {
                  case 1:
                    break;
                  case 2:
                    break;
                  case 3:
                    break;
                  case 4:
                    break;
                  case 5:
                    local.ControlTotals.Update.ProgramControlTotal.Value =
                      local.ControlTotals.Item.ProgramControlTotal.Value.
                        GetValueOrDefault() + 1;
                    local.ControlTotals.Update.Amount.TotalCurrency =
                      local.ControlTotals.Item.Amount.TotalCurrency + local
                      .CollectionRecord.CollectionInputCashReceiptDetail.
                        ReceivedAmount;

                    break;
                  default:
                    goto AfterCycle1;
                }

                ++local.Index.Count;
              }

AfterCycle1:

              break;
          }

          if (AsChar(local.SkipToNextHeaderRecord.Flag) == 'Y')
          {
            if (local.RecordTypeReturned.Count < 9)
            {
              continue;
            }

            if (local.RecordTypeReturned.Count == 9)
            {
              local.SkipToNextHeaderRecord.Flag = "N";

              continue;
            }
          }

          ++local.ReadSinceLastCommit.Count;

          break;
        case "EF":
          // *****************************************************************
          // End Of File.  The EAB closed the file.  Complete processing.
          // *****************************************************************
          goto AfterCycle5;
        default:
          ExitState = "FILE_READ_ERROR_WITH_RB";
          local.CauseAbend.Flag = "Y";

          // *****************************************************************
          // If any type of error occurs (critical or non-critical), rollback
          // all changes since the last commit (i.e. since the last court
          // successfully processed) and write out an error record.
          // Continue processing with the next court's header record.
          // *****************************************************************
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
            AaaGroupLocalErrorsDetailProgramError.KeyInfo = "Court = " + local
            .HeaderRecord.HeaderInputCashReceiptSourceType.Code;
          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.KeyInfo =
              TrimEnd(local.AaaGroupLocalErrors.Item.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Interface Date = " +
            local.SourceCreation.TextDate;
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

          break;
      }

      // *****************************************************************
      // Re-Initialize the error group view.  Set all of the attributes to
      // spaces in case the new error does not overwrite the
      // previous data.  Execute a "Repeat Targeting" to reset the
      // cardinality to zero.
      // *****************************************************************
      if (AsChar(local.CauseAbend.Flag) == 'N')
      {
        for(local.AaaGroupLocalErrors.Index = 0; local
          .AaaGroupLocalErrors.Index < local.AaaGroupLocalErrors.Count; ++
          local.AaaGroupLocalErrors.Index)
        {
          if (!local.AaaGroupLocalErrors.CheckSize())
          {
            break;
          }

          local.AaaGroupLocalErrors.Update.
            AaaGroupLocalErrorsDetailProgramError.
              Assign(local.NullProgramError);
          local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
            Command = "";
        }

        local.AaaGroupLocalErrors.CheckIndex();
      }

      local.AaaGroupLocalErrors.Index = -1;

      switch(local.RecordTypeReturned.Count)
      {
        case 1:
          // ********************************
          // RECORD TYPE 1 IS A HEADER RECORD
          // ********************************
          if (AsChar(local.CauseAbend.Flag) == 'Y')
          {
            break;
          }

          local.SkipToNextHeaderRecord.Flag = "N";
          local.HeaderPrinted.Flag = "N";
          local.ErrorAbend.Flag = "N";
          local.ErrorInfo.Flag = "N";
          local.AlreadyProcessed.Flag = "N";

          // *************************************************************
          // Initialize the CONTROL Report Table
          // *************************************************************
          local.ControlTotals.Update.ProgramControlTotal.
            SystemGeneratedIdentifier = 0;
          local.Index.Count = 0;

          local.ControlTotals.Index = 0;
          local.ControlTotals.Clear();

          while(local.Index.Count < 10)
          {
            if (local.ControlTotals.IsFull)
            {
              break;
            }

            ++local.Index.Count;
            local.ControlTotals.Update.ProgramControlTotal.
              SystemGeneratedIdentifier = local.Index.Count;

            switch(local.ControlTotals.Item.ProgramControlTotal.
              SystemGeneratedIdentifier)
            {
              case 1:
                local.ControlTotals.Update.ProgramControlTotal.Value = 0;
                local.ControlTotals.Update.Amount.TotalCurrency = 0;
                local.ControlTotals.Update.ProgramControlTotal.Name =
                  "Number of Detail Records....................................";
                  

                break;
              case 2:
                local.ControlTotals.Update.ProgramControlTotal.Value = 0;
                local.ControlTotals.Update.Amount.TotalCurrency = 0;
                local.ControlTotals.Update.ProgramControlTotal.Name =
                  "Number of Detail Records - Suspended........................";
                  

                break;
              case 3:
                local.ControlTotals.Update.ProgramControlTotal.Value = 0;
                local.ControlTotals.Update.Amount.TotalCurrency = 0;
                local.ControlTotals.Update.ProgramControlTotal.Name =
                  "Number of Detail Records - Released.........................";
                  

                break;
              case 4:
                local.ControlTotals.Update.ProgramControlTotal.Value = 0;
                local.ControlTotals.Update.Amount.TotalCurrency = 0;
                local.ControlTotals.Update.ProgramControlTotal.Name =
                  "Number of Adjustment Records................................";
                  

                break;
              case 5:
                local.ControlTotals.Update.ProgramControlTotal.Value = 0;
                local.ControlTotals.Update.Amount.TotalCurrency = 0;
                local.ControlTotals.Update.ProgramControlTotal.Name =
                  "Number of Records Read......................................";
                  

                break;
              case 6:
                local.ControlTotals.Update.ProgramControlTotal.Value = 0;
                local.ControlTotals.Update.Amount.TotalCurrency = 0;
                local.ControlTotals.Update.ProgramControlTotal.Name =
                  "Number of Records - Net.....................................";
                  

                break;
              case 7:
                local.ControlTotals.Update.ProgramControlTotal.Value = 0;
                local.ControlTotals.Update.Amount.TotalCurrency = 0;
                local.ControlTotals.Update.ProgramControlTotal.Name =
                  "Number of Records - from Total Record.......................";
                  
                local.ControlTotals.Next();

                goto AfterCycle2;
              case 8:
                break;
              case 9:
                break;
              case 10:
                break;
              default:
                local.ControlTotals.Next();

                goto AfterCycle2;
            }

            local.ControlTotals.Next();
          }

AfterCycle2:

          // *****************************************************************
          // Clear the local views related to the Cash Receipt Detail.
          // *****************************************************************
          local.Check.Assign(local.NullCashReceipt);
          local.NextCheckId.SequentialIdentifier =
            local.NullCashReceiptDetail.SequentialIdentifier;

          // *****************************************************************
          // Set the Source Creation text date.  This date will be used to
          // write out any errors found while processing this court.
          // *****************************************************************
          local.SourceCreation.TextDate =
            NumberToString(DateToInt(
              local.HeaderRecord.HeaderInputCashReceiptEvent.
              SourceCreationDate), 8, 8);
          local.SaveCourt.Code =
            local.HeaderRecord.HeaderInputCashReceiptSourceType.Code;

          // *************************
          // PROCESS THE HEADER RECORD
          // *************************
          UseFnProcessCtIntHeaderRecord();

          // *****************************************************************
          // Check for errors processing the header.  If an error was
          // encountered must skip entire file and proceed with next
          // header record after logging the error.
          // *****************************************************************
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // *******************
            // REINITIALIZE COUNTS
            // *******************
            local.Swefb610CashAmt.TotalCurrency = 0;
            local.Swefb610CashCount.Count = 0;
            local.Swefb610NonCashAmt.TotalCurrency = 0;
            local.Swefb610NonCashCount.Count = 0;
            local.Swefb610AdjustmentCount.Count = 0;
          }
          else
          {
            // Do NOT Print control Totals
            // IF The County has already been Processed (They will be all ZEROs
            // ).
            if (IsExitState("INTERFACE_ALREADY_PROCESSED_RB"))
            {
              local.AlreadyProcessed.Flag = "Y";
            }
            else
            {
            }

            // *****************************************************************
            // If any type of error occurs (critical or non-critical), rollback
            // all changes since the last commit (i.e. since the last court
            // successfully processed) and write out an error record.
            // Continue processing with the next court's header record.
            // *****************************************************************
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
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "Court = " + local
              .HeaderRecord.HeaderInputCashReceiptSourceType.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Interface Date = " +
              local.SourceCreation.TextDate;
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

            // * * Someone had a change in Dev on 01/06/2003 that commented out 
            // the Exit State re-set below.                       It was
            // allegedy done to fix printing records after ERROR in Header
            // found.  The person who made                        the change did
            // not note their name or the PR number.      Feb 2005  M J Quinn
            ExitState = "ACO_NN0000_ALL_OK";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            break;
          }

          // *****************************************************************
          // Read and establish currency on the Cash Receipts.  This
          // will allow matching to persistent views in called CABs.
          // *****************************************************************
          if (ReadCashReceipt())
          {
            continue;
          }
          else
          {
            ExitState = "FN0086_CASH_RCPT_NF_RB";

            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number and the creation date
            // of the transaction file.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "Court = " + local
              .CashReceiptSourceType.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Interface Date = " +
              local.SourceCreation.TextDate;
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
                "Error in read check cash receipt";
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

            // * * Someone had a change in Dev on 01/06/2003 that commented out 
            // the Exit State re-set below.                       It was
            // allegedy done to fix printing records after ERROR in Header
            // found.  The person who made                        the change did
            // not note their name or the PR number.      Feb 2005  M J Quinn
            ExitState = "ACO_NN0000_ALL_OK";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
          }

          break;
        case 2:
          // *******************************************
          // PDP - - This CODE was added to TEST RESTART
          // *******************************************
          if (Equal(local.CollectionRecord.CollectionInputCashReceiptDetail.
            InterfaceTransId, "JOB WILL ABEND"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *******************************************
          // RECORD TYPE 2 IS A COLLECTION DETAIL RECORD
          // *******************************************
          if (AsChar(local.CauseAbend.Flag) == 'Y')
          {
            break;
          }

          ExitState = "ACO_NN0000_ALL_OK";
          UseFnProcessCtIntCollDtlRec();

          // *******************************************************************************************************************
          // Added a check to see if ADABAS was unavailable while creating the
          // cash reciept detail.  If not abend the job.  MJQuinn June 2004
          // 00208021
          // ******************************************************************************************************
          if (IsExitState("ADABAS_UNAVAILABLE_RB"))
          {
            ExitState = "LE0000_ADABAS_UNAVAILABLE_ABORT";

            return;
          }
          else if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.Index.Count = 1;

            for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
              .ControlTotals.Count; ++local.ControlTotals.Index)
            {
              switch(local.Index.Count)
              {
                case 1:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency + local
                    .CollectionRecord.CollectionInputCashReceiptDetail.
                      ReceivedAmount;

                  break;
                case 2:
                  if (AsChar(local.SuspendedDetail.Flag) == 'Y')
                  {
                    local.ControlTotals.Update.ProgramControlTotal.Value =
                      local.ControlTotals.Item.ProgramControlTotal.Value.
                        GetValueOrDefault() + 1;
                    local.ControlTotals.Update.Amount.TotalCurrency =
                      local.ControlTotals.Item.Amount.TotalCurrency + local
                      .CollectionRecord.CollectionInputCashReceiptDetail.
                        ReceivedAmount;
                  }

                  break;
                case 3:
                  if (AsChar(local.ReleasedDetail.Flag) == 'Y')
                  {
                    local.ControlTotals.Update.ProgramControlTotal.Value =
                      local.ControlTotals.Item.ProgramControlTotal.Value.
                        GetValueOrDefault() + 1;
                    local.ControlTotals.Update.Amount.TotalCurrency =
                      local.ControlTotals.Item.Amount.TotalCurrency + local
                      .CollectionRecord.CollectionInputCashReceiptDetail.
                        ReceivedAmount;
                  }

                  break;
                case 4:
                  break;
                case 5:
                  break;
                case 6:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency + local
                    .CollectionRecord.CollectionInputCashReceiptDetail.
                      ReceivedAmount;

                  goto AfterCycle3;
                default:
                  break;
              }

              ++local.Index.Count;
            }

AfterCycle3:
            ;
          }
          else
          {
            // *****************************************************************
            // If any type of error occurs (critical or non-critical), rollback
            // all changes since the last commit (i.e. since the last court
            // successfully processed) and write out an error record.
            // Continue processing with the next court's header record.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, and the court system identifier.
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "Court = " + local
              .CashReceiptSourceType.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Interface Date = " +
              local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court System Identifier  = " +
              (
                local.CollectionRecord.CollectionInputCashReceiptDetail.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";

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
                "Error in processing detail record";
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
              AaaGroupLocalErrorsDetailProgramError.ProgramError1 = "SPACING";
            ExitState = "ACO_NN0000_ALL_OK";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
          }

          break;
        case 5:
          // *******************************************
          // PDP - - This CODE was added to TEST RESTART
          // *******************************************
          if (Equal(local.CollectionRecord.CollectionInputCashReceiptDetail.
            InterfaceTransId, "JOB WILL ABEND"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *************************************
          // RECORD TYPE 5 IS AN ADJUSTMENT RECORD
          // *************************************
          if (AsChar(local.CauseAbend.Flag) == 'Y')
          {
            break;
          }

          ExitState = "ACO_NN0000_ALL_OK";
          UseFnProcessCiIntAdjustmentRec();

          // *****************************************************************
          // 12/03/01     WR010504 - RETRO Processing - Added Exit States
          // cse_person_nf_rb    and    
          // fn0000_error_on_event_creation_with_Rollback
          // *****************************************************************
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.Index.Count = 1;

            for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
              .ControlTotals.Count; ++local.ControlTotals.Index)
            {
              switch(local.Index.Count)
              {
                case 4:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency + local
                    .Swefb610AdjustmentCount.TotalCurrency;

                  break;
                case 6:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency - local
                    .CollectionRecord.CollectionInputCashReceiptDetail.
                      ReceivedAmount;

                  break;
                default:
                  break;
              }

              ++local.Index.Count;
            }
          }
          else if (IsExitState("FN0124_ORIGINAL_CASH_RCPT_ADJ_NF"))
          {
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court Order # = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "  Original Court System # = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "  Adjusting Court System # = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 2
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                " Original Court System Number not found for Court = " + local
              .CashReceiptSourceType.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Interface Date = " +
              local.SourceCreation.TextDate;

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "'" + " ";

            // *****************************************************************
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
          }
          else if (IsExitState("FN0000_ADJ_COURT_ORDER_MISMATCH"))
          {
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Adjusting Court Order # = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "  Adjusting Court System # = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "  Original Court System # = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 2
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court Order # = " +
              (local.OriginalPassed.CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "  " + "Adjusting Court Order does not match Original Court Order.";
              

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "'" + " ";

            // *****************************************************************
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
          }
          else if (IsExitState("FN0000_CR_DTL_ALREADY_ADJ"))
          {
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court Order Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " New Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 2
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Interface Date = " +
              local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Cash Receipt ALREADY adjusted for Court = " +
              local.CashReceiptSourceType.Code;

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "'" + " ";

            // *****************************************************************
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
          }
          else if (IsExitState("FN0000_ORIG_DTL_ADJ_IND_IS_YES"))
          {
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court Order Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " New Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 2
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Interface Date = " +
              local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " The original detail being adjusted has adjustment indicator of Y." +
              " ";

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "'" + " ";

            // *****************************************************************
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
          }
          else if (IsExitState("CASE_NF"))
          {
            // *****************************************************************
            // 12/03/01     WR010504 - RETRO Processing - Added Exit States
            // cse_person_nf_rb     case_nf   and    sp0000_event_nf
            // *****************************************************************
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court Order Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " New Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 2
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Interface Date = " +
              local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " INFO - Prot Col Alert Not Sent - AP/CASE Match Not Found - " +
              " ";

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
            local.Index.Count = 1;

            for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
              .ControlTotals.Count; ++local.ControlTotals.Index)
            {
              switch(local.Index.Count)
              {
                case 4:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency + local
                    .Swefb610AdjustmentCount.TotalCurrency;

                  break;
                case 6:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency - local
                    .CollectionRecord.CollectionInputCashReceiptDetail.
                      ReceivedAmount;

                  break;
                default:
                  break;
              }

              ++local.Index.Count;
            }
          }
          else if (IsExitState("CSE_PERSON_NF_RB"))
          {
            // *****************************************************************
            // 12/03/01     WR010504 - RETRO Processing - Added Exit States
            // cse_person_nf_rb     case_nf   and    sp0000_event_nf
            // *****************************************************************
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court Order Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " New Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 2
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Interface Date = " +
              local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "INFO - Prot Col Alert Not Sent - AP/Court Order Not Found - " +
              " ";

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
            local.Index.Count = 1;

            for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
              .ControlTotals.Count; ++local.ControlTotals.Index)
            {
              switch(local.Index.Count)
              {
                case 4:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency + local
                    .Swefb610AdjustmentCount.TotalCurrency;

                  break;
                case 6:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency - local
                    .CollectionRecord.CollectionInputCashReceiptDetail.
                      ReceivedAmount;

                  break;
                default:
                  break;
              }

              ++local.Index.Count;
            }
          }
          else if (IsExitState("SP0000_EVENT_NF"))
          {
            // *****************************************************************
            // 12/03/01     WR010504 - RETRO Processing - Added Exit States
            // cse_person_nf_rb     case_nf   and    sp0000_event_nf
            // *****************************************************************
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court Order Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " New Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 2
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Interface Date = " +
              local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "INFO - Prot Col Alert Not Sent - Creation ERROR - " +
              " ";

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
            local.Index.Count = 1;

            for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
              .ControlTotals.Count; ++local.ControlTotals.Index)
            {
              switch(local.Index.Count)
              {
                case 4:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency + local
                    .Swefb610AdjustmentCount.TotalCurrency;

                  break;
                case 6:
                  local.ControlTotals.Update.ProgramControlTotal.Value =
                    local.ControlTotals.Item.ProgramControlTotal.Value.
                      GetValueOrDefault() + 1;
                  local.ControlTotals.Update.Amount.TotalCurrency =
                    local.ControlTotals.Item.Amount.TotalCurrency - local
                    .CollectionRecord.CollectionInputCashReceiptDetail.
                      ReceivedAmount;

                  break;
                default:
                  break;
              }

              ++local.Index.Count;
            }
          }
          else
          {
            // * * * * *
            // Print LINE 1
            // * * * * *
            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, the original court system identifier, and
            // the new court system identifier.
            // *****************************************************************
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court Order Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                CourtOrderNumber ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Original Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputOriginal.
                InterfaceTransId ?? "");
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " New Court System Number = " +
              (
                local.AdjustmentRecord.AdjustmentInputNew.InterfaceTransId ?? ""
              );

            // **************************
            // SET THE TERMINATION ACTION
            // **************************
            local.SkipToNextHeaderRecord.Flag = "Y";
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

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Interface Date = " +
              local.SourceCreation.TextDate;
            local.ExitStateWorkArea.Message = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Error in Adjustment Record Processing " +
              local.ExitStateWorkArea.Message;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.ProgramError1 =
                "Error in processing adjustment records";

            // 09/13/99     H00073251  pphinney  Add Adjustment Reason Code
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Adjustment Reason Code = " +
              " ";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " '" + local
              .AdjustmentRecord.AdjustmentInput.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "'" + " ";

            // *****************************************************************
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";

            // * * * * *
            // Print LINE 3
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
                "Error in processing adjustment records";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";

            // * * * * *
            // Print LINE 3 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
          }

          break;
        case 9:
          // *********************************
          // RECORD TYPE 9 IS THE TOTAL RECORD
          // *********************************
          if (AsChar(local.CauseAbend.Flag) == 'Y')
          {
            break;
          }

          UseFnProcessCtIntTotalsRecord();
          local.Index.Count = 0;

          for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
            .ControlTotals.Count; ++local.ControlTotals.Index)
          {
            ++local.Index.Count;

            switch(local.Index.Count)
            {
              case 7:
                local.ControlTotals.Update.ProgramControlTotal.Value =
                  local.TotalRecord.TotalInput.TotalAdjustmentCount.
                    GetValueOrDefault() + local
                  .TotalRecord.TotalInput.TotalCashTransactionCount.
                    GetValueOrDefault() + local
                  .TotalRecord.TotalInput.TotalNonCashTransactionCount.
                    GetValueOrDefault();
                local.ControlTotals.Update.Amount.TotalCurrency =
                  local.TotalRecord.TotalInput.AnticipatedCheckAmt.
                    GetValueOrDefault();

                break;
              case 8:
                goto AfterCycle4;
              default:
                continue;
            }
          }

AfterCycle4:

          if (IsExitState("FN0000_OUT_OF_BALANCE"))
          {
            local.AaaGroupLocalErrors.Index = local.AaaGroupLocalErrors.Count;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Court = " + local
              .CashReceiptSourceType.Code;
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Exit State = " +
              local.ExitStateWorkArea.Message;
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            local.AaaGroupLocalErrors.Index = local.AaaGroupLocalErrors.Count;
            local.AaaGroupLocalErrors.CheckSize();

            // *****************************************************************
            // Set the Key Info with the Court number, the creation date of
            // the transaction file, and the court system identifier.
            // *****************************************************************
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = " Court = " + local
              .CashReceiptSourceType.Code;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + " Interface Date = " +
              local.SourceCreation.TextDate;
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";

            // * * * * *
            // Print LINE 2
            // * * * * *
            // *****************************************************************
            // Set the Error Message Text to the exit state description.
            // *****************************************************************
            local.AaaGroupLocalErrors.Index = local.AaaGroupLocalErrors.Count;
            local.AaaGroupLocalErrors.CheckSize();

            local.ExitStateWorkArea.Message = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "ERROR Processing TOTAL Record" +
              local.ExitStateWorkArea.Message;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.ProgramError1 =
                "Error in processing total records";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";

            // * * * * *
            // Print LINE 3
            // * * * * *
            // *****************************************************************
            // Set the Error Message Text to the exit state description.
            // *****************************************************************
            local.AaaGroupLocalErrors.Index = local.AaaGroupLocalErrors.Count;
            local.AaaGroupLocalErrors.CheckSize();

            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo =
                TrimEnd(local.AaaGroupLocalErrors.Item.
                AaaGroupLocalErrorsDetailProgramError.KeyInfo) + "Exit State = " +
              local.ExitStateWorkArea.Message;
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.ProgramError1 =
                "Error in processing total records";
            ExitState = "ACO_NN0000_ALL_OK";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "ROLLBACK";

            // * * * * *
            // Print LINE 4 (spacing)
            // * * * * *
            ++local.AaaGroupLocalErrors.Index;
            local.AaaGroupLocalErrors.CheckSize();

            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.KeyInfo = "";
            local.AaaGroupLocalErrors.Update.AaaGroupLocalErrorsDetailStandard.
              Command = "";
            local.AaaGroupLocalErrors.Update.
              AaaGroupLocalErrorsDetailProgramError.Code = "SPACING";
            ExitState = "ACO_NN0000_ALL_OK";
          }

          // ****************************************
          // COMMIT THE INTERFACE FILE FOR THIS COURT
          // ****************************************
          global.Command = "COMMIT";

          break;
        default:
          if (AsChar(local.CauseAbend.Flag) == 'Y')
          {
            break;
          }

          ExitState = "ACO_RE0000_INPUT_RECORD_TYPE_INV";

          return;
      }

      // **************
      // PROCESS ERRORS
      // **************
      // Print ERROR HEADER and Determine if ROLLBACK occurred
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

        // IS ERROR file open?
        if (AsChar(local.ErrorOpen.Flag) != 'Y')
        {
          local.ReportProcessing.Action = "OPEN";
          local.ReportHandling.ProgramName = "SWEFB610";
          local.ReportHandling.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          UseCabErrorReport2();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

            return;
          }

          local.ErrorOpen.Flag = "Y";
        }

        // HEADER
        if (AsChar(local.HeaderPrinted.Flag) == 'Y')
        {
        }
        else
        {
          // SPACING
          local.ReportHandling.RptDetail = "";
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFB610";
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }

          local.ReportHandling.RptDetail = "  ERRORS from Court " + local
            .SaveCourt.Code + " for Date = " + local.SourceCreation.TextDate;
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFB610";
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }

          // SPACING
          local.ReportHandling.RptDetail = "";
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFB610";
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }

          local.HeaderPrinted.Flag = "Y";
        }

        if (Equal(local.AaaGroupLocalErrors.Item.
          AaaGroupLocalErrorsDetailStandard.Command, "ROLLBACK"))
        {
          // *****************************************************************
          // Roll back all updates made since the last successful header.
          // *****************************************************************
          local.ErrorAbend.Flag = "Y";
          UseEabRollbackSql();

          if (local.PassArea.NumericReturnCode != 0)
          {
            // HERE PDP
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            // IS ERROR file open?
            if (AsChar(local.ErrorOpen.Flag) != 'Y')
            {
              local.ReportProcessing.Action = "OPEN";
              local.ReportHandling.ProgramName = "SWEFB610";
              local.ReportHandling.ProcessDate =
                local.ProgramProcessingInfo.ProcessDate;
              UseCabErrorReport2();

              if (Equal(local.ReportProcessing.Status, "OK"))
              {
              }
              else
              {
                ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

                return;
              }

              local.ErrorOpen.Flag = "Y";
            }

            // *****************************************************************
            // 10/25/01     I00129648  pphinney  Cause ABEND on ERROR condition
            // *****************************************************************
            // SPACING
            local.ReportHandling.RptDetail = "";
            local.ReportProcessing.Action = "WRITE";
            local.ReportHandling.ProgramName = "SWEFB610";
            UseCabErrorReport1();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }

            // PRINT the ERROR Message
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.ReportHandling.RptDetail =
              TrimEnd("Program has ABENDED - INVALID DB2 Rollback") + " Exit State = " +
              local.ExitStateWorkArea.Message;
            local.ReportProcessing.Action = "WRITE";
            local.ReportHandling.ProgramName = "SWEFB610";
            UseCabErrorReport1();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }

            // SPACING
            local.ReportHandling.RptDetail = "";
            local.ReportProcessing.Action = "WRITE";
            local.ReportHandling.ProgramName = "SWEFB610";
            UseCabErrorReport1();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }

            // CLOSE the ERROR Report
            local.ReportProcessing.Action = "CLOSE";
            local.ReportHandling.ProgramName = "SWEFB610";
            local.ReportHandling.ProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
            UseCabErrorReport2();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "ERROR_CLOSING_FILE_AB";

              return;
            }

            // *****************************************************************
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // *******************************************
          // BYPASS selected EXIT STATES - DO NOT commit
          // Continue with PROCESSING until EOF or major Error
          // *******************************************
          global.Command = "COMMIT";

          break;
        }
      }

      local.AaaGroupLocalErrors.CheckIndex();

      for(local.AaaGroupLocalErrors.Index = 0; local
        .AaaGroupLocalErrors.Index < local.AaaGroupLocalErrors.Count; ++
        local.AaaGroupLocalErrors.Index)
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

        local.ErrorInfo.Flag = "Y";

        // **********************************
        // WRITE OUT AN ERROR/WARNING MESSAGE
        // **********************************
        local.ReportHandling.RptDetail =
          local.AaaGroupLocalErrors.Item.AaaGroupLocalErrorsDetailProgramError.
            KeyInfo ?? Spaces(132);
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // *****************************************************************
        // If all of the changes were rolled back (i.e. a severe error
        // occurred), commit the error record to the database so the
        // error can be traced if a rollback occurs while processing the
        // next header record.  If the changes were not rolled back (i.e.
        // a warning message was issued), do not perform a commit
        // until the Logical Unit of Work is completed.
        // *****************************************************************
        // *****************************************************************
        // 12/03/01     WR010504 - RETRO Processing - Added Exit States
        // cse_person_nf_rb     case_nf   and    sp0000_event_nf
        // *****************************************************************
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.AlreadyProcessed.Flag) == 'Y')
          {
            local.SkipToNextHeaderRecord.Flag = "Y";

            goto Test1;
          }

          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else if (IsExitState("FN0124_ORIGINAL_CASH_RCPT_ADJ_NF"))
        {
          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else if (IsExitState("FN0000_ADJ_COURT_ORDER_MISMATCH"))
        {
          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else if (IsExitState("FN0000_CR_DTL_ALREADY_ADJ"))
        {
          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else if (IsExitState("FN0000_ORIG_DTL_ADJ_IND_IS_YES"))
        {
          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else if (IsExitState("CSE_PERSON_NF_RB"))
        {
          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else if (IsExitState("CASE_NF"))
        {
          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else if (IsExitState("SP0000_EVENT_NF"))
        {
          local.SkipToNextHeaderRecord.Flag = "N";
        }
        else
        {
          if (local.RecordTypeReturned.Count == 9)
          {
            local.SkipToNextHeaderRecord.Flag = "N";
          }
          else
          {
            local.SkipToNextHeaderRecord.Flag = "Y";
          }
        }

Test1:
        ;
      }

      local.AaaGroupLocalErrors.CheckIndex();
      ExitState = "ACO_NN0000_ALL_OK";

      if (Equal(global.Command, "COMMIT"))
      {
        // Do NOT Print control Totals
        // IF The County has already been Processed (They will be all ZEROs).
        if (AsChar(local.AlreadyProcessed.Flag) == 'Y')
        {
          goto Test3;
        }

        // *******************************************
        // PRINT THE PROGRAM CONTROL - TOTALS
        // *******************************************
        // IS CONTROL file open?
        if (AsChar(local.ControlOpen.Flag) != 'Y')
        {
          local.ReportProcessing.Action = "OPEN";
          local.ReportHandling.ProgramName = "SWEFB610";
          local.ReportHandling.ProcessDate =
            local.ProgramProcessingInfo.ProcessDate;
          UseCabControlReport2();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

            return;
          }

          local.ControlOpen.Flag = "Y";
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }

        // HEADER
        local.ReportHandling.RptDetail = "  CONTROL REPORT for court " + local
          .SaveCourt.Code + " for Date = " + local.SourceCreation.TextDate;
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }

        // 1 = Header Record -- bypass it.
        // 6 = NET Records -- bypass it.
        // 8 = NOT USED -- bypass it.
        // 9 = NOT USED -- bypass it.
        // 10 = NOT USED -- bypass it.
        for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
          .ControlTotals.Count; ++local.ControlTotals.Index)
        {
          if (local.ControlTotals.Item.ProgramControlTotal.
            SystemGeneratedIdentifier == 0 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              9 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              10 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              8 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              6)
          {
            continue;
          }

          // **********************************
          // WRITE OUT A CONTROL REPORT
          // **********************************
          local.ReportHandling.RptDetail =
            (local.ControlTotals.Item.ProgramControlTotal.Name ?? "") + NumberToString
            ((long)local.ControlTotals.Item.ProgramControlTotal.Value.
              GetValueOrDefault(), 7, 9);

          if (local.ControlTotals.Item.ProgramControlTotal.
            SystemGeneratedIdentifier == 4)
          {
            local.ReportHandling.RptDetail =
              (local.ControlTotals.Item.ProgramControlTotal.Name ?? "") + NumberToString
              (local.Swefb610AdjustmentCount.Count, 7, 9);
          }

          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFB610";
          UseCabControlReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }

        // *******************************************
        // PRINT THE PROGRAM CONTROL - AMOUNTS
        // *******************************************
        // 1 = Header Record -- bypass it.
        for(local.ControlTotals.Index = 0; local.ControlTotals.Index < local
          .ControlTotals.Count; ++local.ControlTotals.Index)
        {
          if (local.ControlTotals.Item.ProgramControlTotal.
            SystemGeneratedIdentifier == 0 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              8 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              9 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              10 || local
            .ControlTotals.Item.ProgramControlTotal.SystemGeneratedIdentifier ==
              6)
          {
            continue;
          }

          // **********************************
          // WRITE OUT A CONTROL REPORT
          // **********************************
          // Change the Label
          local.ReportHandling.RptDetail = "Amount" + Substring
            (local.ControlTotals.Item.ProgramControlTotal.Name, 7, 54);

          // **************************************************************
          // ADD the Amount to the REPORT LINE with Decimal point
          local.ReportHandling.RptDetail =
            Substring(local.ReportHandling.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 60) + NumberToString
            ((long)local.ControlTotals.Item.Amount.TotalCurrency, 10, 6) + "."
            + NumberToString
            ((long)(local.ControlTotals.Item.Amount.TotalCurrency * 100), 14, 2);
            
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFB610";
          UseCabControlReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }
        }

        // SPACING
        local.ReportHandling.RptDetail = "";
        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }

        // Completion message
        if (AsChar(local.ErrorInfo.Flag) == 'Y')
        {
          if (AsChar(local.CauseAbend.Flag) == 'Y')
          {
            local.ReportHandling.RptDetail =
              "* * * * Court NOT Processed - MAJOR FILE ERROR - See ERROR Report * * * *";
              

            goto Test2;
          }

          if (AsChar(local.ErrorAbend.Flag) == 'Y')
          {
            local.ReportHandling.RptDetail =
              "* * * * Court NOT Processed - See ERROR Report * * * *";
          }
          else
          {
            local.ReportHandling.RptDetail =
              "* * * * Court Processed BUT Errors occurred - See ERROR Report * * * *";
              
          }
        }
        else
        {
          local.ReportHandling.RptDetail =
            "* * * * Court Processed Successfully * * * *";
        }

Test2:

        local.ReportProcessing.Action = "WRITE";
        local.ReportHandling.ProgramName = "SWEFB610";
        UseCabControlReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }

        local.Index.Count = 0;

        while(local.Index.Count < 33)
        {
          // SPACING
          local.ReportHandling.RptDetail = "";
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFB610";
          UseCabControlReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }

          ++local.Index.Count;
        }

        if (AsChar(local.CauseAbend.Flag) == 'Y')
        {
          break;
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          return;
        }
      }

Test3:
      ;
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle5:

    // *****************************************************************
    // The external hit the end of the driver file, closed the file and
    // returned an EF (EOF) indicator.
    // *****************************************************************
    // IS ERROR Report open?
    if (AsChar(local.ErrorOpen.Flag) == 'Y')
    {
      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "ERROR_CLOSING_FILE_AB";

        return;
      }
    }

    // IS CONTROL Report open?
    if (AsChar(local.ControlOpen.Flag) == 'Y')
    {
      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandling.ProgramName = "SWEFB610";
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      UseCabControlReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "ERROR_CLOSING_FILE_AB";

        return;
      }
    }

    // *****************************************************************
    // Set restart indicator to no because we successfully finished
    // this program.
    // *****************************************************************
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdatePgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (AsChar(local.CauseAbend.Flag) == 'Y')
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    UseSiCloseAdabas();
  }

  private static void MoveAdjustmentRecord1(Local.AdjustmentRecordGroup source,
    EabCourtInterfaceDrvr.Export.AdjustmentRecordGroup target)
  {
    target.AdjustmentInput.Code = source.AdjustmentInput.Code;
    MoveCashReceiptDetail(source.AdjustmentInputOriginal,
      target.AdjustmentInputOriginal);
    target.AdjustmentInputNew.InterfaceTransId =
      source.AdjustmentInputNew.InterfaceTransId;
  }

  private static void MoveAdjustmentRecord2(Local.AdjustmentRecordGroup source,
    FnProcessCiIntAdjustmentRec.Import.AdjustmentRecordGroup target)
  {
    target.Adjustment.Code = source.AdjustmentInput.Code;
    MoveCashReceiptDetail(source.AdjustmentInputOriginal,
      target.AdjustmentInputOriginal);
    target.AdjustmentInputNew.InterfaceTransId =
      source.AdjustmentInputNew.InterfaceTransId;
  }

  private static void MoveAdjustmentRecord3(EabCourtInterfaceDrvr.Export.
    AdjustmentRecordGroup source, Local.AdjustmentRecordGroup target)
  {
    target.AdjustmentInput.Code = source.AdjustmentInput.Code;
    MoveCashReceiptDetail(source.AdjustmentInputOriginal,
      target.AdjustmentInputOriginal);
    target.AdjustmentInputNew.InterfaceTransId =
      source.AdjustmentInputNew.InterfaceTransId;
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.TotalNonCashFeeAmount = source.TotalNonCashFeeAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
    target.CashDue = source.CashDue;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.CourtOrderNumber = source.CourtOrderNumber;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SourceCreationDate = source.SourceCreationDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveErrors(FnProcessCtIntTotalsRecord.Export.
    ErrorsGroup source, Local.AaaGroupLocalErrorsGroup target)
  {
    MoveProgramError(source.ErrorsDetailProgramError,
      target.AaaGroupLocalErrorsDetailProgramError);
    target.AaaGroupLocalErrorsDetailStandard.Command =
      source.ErrorsDetailStandard.Command;
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
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramError(ProgramError source, ProgramError target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.KeyInfo = source.KeyInfo;
    target.Code = source.Code;
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

    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportProcessing.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabReadAdabasPersonBatch()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.AdabasResponseCd = useExport.AbendData.AdabasResponseCd;
  }

  private void UseEabCourtInterfaceDrvr1()
  {
    var useImport = new EabCourtInterfaceDrvr.Import();
    var useExport = new EabCourtInterfaceDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    useImport.RestartCashReceiptSourceType.Code =
      local.RestartCashReceiptSourceType.Code;
    useImport.RestartCashReceiptEvent.SourceCreationDate =
      local.RestartCashReceiptEvent.SourceCreationDate;
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabCourtInterfaceDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabCourtInterfaceDrvr2()
  {
    var useImport = new EabCourtInterfaceDrvr.Import();
    var useExport = new EabCourtInterfaceDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabCourtInterfaceDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabCourtInterfaceDrvr3()
  {
    var useImport = new EabCourtInterfaceDrvr.Import();
    var useExport = new EabCourtInterfaceDrvr.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);
    useExport.RecordTypeReturned.Count = local.RecordTypeReturned.Count;
    useExport.HeaderRecord.HeaderInputCashReceiptSourceType.Code =
      local.HeaderRecord.HeaderInputCashReceiptSourceType.Code;
    useExport.HeaderRecord.HeaderInputCashReceiptEvent.SourceCreationDate =
      local.HeaderRecord.HeaderInputCashReceiptEvent.SourceCreationDate;
    useExport.CollectionRecord.CollectionInputCollectionType.Code =
      local.CollectionRecord.CollectionInputCollectionType.Code;
    useExport.CollectionRecord.CollectionInputCashReceiptDetail.Assign(
      local.CollectionRecord.CollectionInputCashReceiptDetail);
    MoveAdjustmentRecord1(local.AdjustmentRecord, useExport.AdjustmentRecord);
    useExport.TotalRecord.TotalInput.Assign(local.TotalRecord.TotalInput);
    useExport.FeesRecord.FeesInputSrs.Amount =
      local.FeesRecord.FeesInputSrs.Amount;
    useExport.FeesRecord.FeesInputSendingState.Amount =
      local.FeesRecord.FeesInputSendingState.Amount;
    useExport.FeesRecord.FeesInputCourt.Amount =
      local.FeesRecord.FeesInputCourt.Amount;
    useExport.FeesRecord.FeesInputMiscellaneous.Amount =
      local.FeesRecord.FeesInputMiscellaneous.Amount;

    Call(EabCourtInterfaceDrvr.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
    local.RecordTypeReturned.Count = useExport.RecordTypeReturned.Count;
    local.HeaderRecord.HeaderInputCashReceiptSourceType.Code =
      useExport.HeaderRecord.HeaderInputCashReceiptSourceType.Code;
    local.HeaderRecord.HeaderInputCashReceiptEvent.SourceCreationDate =
      useExport.HeaderRecord.HeaderInputCashReceiptEvent.SourceCreationDate;
    local.CollectionRecord.CollectionInputCollectionType.Code =
      useExport.CollectionRecord.CollectionInputCollectionType.Code;
    local.CollectionRecord.CollectionInputCashReceiptDetail.Assign(
      useExport.CollectionRecord.CollectionInputCashReceiptDetail);
    MoveAdjustmentRecord3(useExport.AdjustmentRecord, local.AdjustmentRecord);
    local.TotalRecord.TotalInput.Assign(useExport.TotalRecord.TotalInput);
    local.FeesRecord.FeesInputSrs.Amount =
      useExport.FeesRecord.FeesInputSrs.Amount;
    local.FeesRecord.FeesInputSendingState.Amount =
      useExport.FeesRecord.FeesInputSendingState.Amount;
    local.FeesRecord.FeesInputCourt.Amount =
      useExport.FeesRecord.FeesInputCourt.Amount;
    local.FeesRecord.FeesInputMiscellaneous.Amount =
      useExport.FeesRecord.FeesInputMiscellaneous.Amount;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
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

  private void UseFnProcessCiIntAdjustmentRec()
  {
    var useImport = new FnProcessCiIntAdjustmentRec.Import();
    var useExport = new FnProcessCiIntAdjustmentRec.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Swefb610AdjustmentCount.Count =
      local.Swefb610AdjustmentCount.Count;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.CashReceiptSourceType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(local.CashReceiptEvent, useImport.CashReceiptEvent);
    MoveAdjustmentRecord2(local.AdjustmentRecord, useImport.AdjustmentRecord);
    useExport.ImportCheck.Assign(local.Check);
    useExport.ImportNextCheckId.SequentialIdentifier =
      local.NextCheckId.SequentialIdentifier;

    Call(FnProcessCiIntAdjustmentRec.Execute, useImport, useExport);

    MoveCommon(useExport.Swefb610AdjustmentCount, local.Swefb610AdjustmentCount);
      
    local.Check.Assign(useExport.ImportCheck);
    local.NextCheckId.SequentialIdentifier =
      useExport.ImportNextCheckId.SequentialIdentifier;
    local.OriginalPassed.CourtOrderNumber = useExport.Original.CourtOrderNumber;
  }

  private void UseFnProcessCtIntCollDtlRec()
  {
    var useImport = new FnProcessCtIntCollDtlRec.Import();
    var useExport = new FnProcessCtIntCollDtlRec.Export();

    useImport.Swefb610NonCashCount.Count = local.Swefb610NonCashCount.Count;
    useImport.Swefb610CashCount.Count = local.Swefb610CashCount.Count;
    useImport.Swefb610NonCashAmt.TotalCurrency =
      local.Swefb610NonCashAmt.TotalCurrency;
    useImport.Swefb610CashAmt.TotalCurrency =
      local.Swefb610CashAmt.TotalCurrency;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.Pcheck.Assign(entities.Check);
    useImport.CollectionRecord.CollectionCollectionType.Code =
      local.CollectionRecord.CollectionInputCollectionType.Code;
    useImport.CollectionRecord.CollectionCashReceiptDetail.Assign(
      local.CollectionRecord.CollectionInputCashReceiptDetail);
    MoveCashReceipt(local.Check, useExport.ImportCheck);
    useImport.FeesRecord.FeesInputCourt.Amount =
      local.FeesRecord.FeesInputCourt.Amount;
    useImport.FeesRecord.FeesInputSendingState.Amount =
      local.FeesRecord.FeesInputSendingState.Amount;
    useImport.FeesRecord.FeesInputSrs.Amount =
      local.FeesRecord.FeesInputSrs.Amount;
    useImport.FeesRecord.FeesInputMiscellaneous.Amount =
      local.FeesRecord.FeesInputMiscellaneous.Amount;
    MoveCashReceipt(local.Check, useExport.ImportCheck);
    useExport.ImportNextCheckId.SequentialIdentifier =
      local.NextCheckId.SequentialIdentifier;

    Call(FnProcessCtIntCollDtlRec.Execute, useImport, useExport);

    entities.Check.SequentialNumber = useImport.Pcheck.SequentialNumber;
    local.Check.Assign(useExport.ImportCheck);
    local.Swefb610CashAmt.TotalCurrency =
      useExport.Swefb610CashAmt.TotalCurrency;
    local.Swefb610NonCashAmt.TotalCurrency =
      useExport.Swefb610NonCashAmt.TotalCurrency;
    local.Swefb610CashCount.Count = useExport.Swefb610CashCount.Count;
    local.Swefb610NonCashCount.Count = useExport.Swefb610NonCashCount.Count;
    local.Check.Assign(useExport.ImportCheck);
    local.NextCheckId.SequentialIdentifier =
      useExport.ImportNextCheckId.SequentialIdentifier;
    local.ReleasedDetail.Flag = useExport.ReleasedDetail.Flag;
    local.SuspendedDetail.Flag = useExport.SuspendedDetail.Flag;
  }

  private void UseFnProcessCtIntHeaderRecord()
  {
    var useImport = new FnProcessCtIntHeaderRecord.Import();
    var useExport = new FnProcessCtIntHeaderRecord.Export();

    useImport.HeaderRecord.HeaderCashReceiptSourceType.Code =
      local.HeaderRecord.HeaderInputCashReceiptSourceType.Code;
    useImport.HeaderRecord.HeaderCashReceiptEvent.SourceCreationDate =
      local.HeaderRecord.HeaderInputCashReceiptEvent.SourceCreationDate;

    Call(FnProcessCtIntHeaderRecord.Execute, useImport, useExport);

    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      local.CashReceiptSourceType);
    local.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
    local.CashReceiptType.SystemGeneratedIdentifier =
      useExport.CashReceiptType.SystemGeneratedIdentifier;
    local.Check.Assign(useExport.Check);
  }

  private void UseFnProcessCtIntTotalsRecord()
  {
    var useImport = new FnProcessCtIntTotalsRecord.Import();
    var useExport = new FnProcessCtIntTotalsRecord.Export();

    useImport.Swefb610AdjustmentCount.Count =
      local.Swefb610AdjustmentCount.Count;
    useImport.Swefb610NonCash.Count = local.Swefb610NonCashCount.Count;
    useImport.Swefb610CashCount.Count = local.Swefb610CashCount.Count;
    useImport.Swefb610NonCashAmt.TotalCurrency =
      local.Swefb610NonCashAmt.TotalCurrency;
    useImport.Swefb610CashAmt.TotalCurrency =
      local.Swefb610CashAmt.TotalCurrency;
    useImport.CashReceiptEvent.Assign(local.CashReceiptEvent);
    MoveCashReceiptSourceType(local.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.Check.Assign(local.Check);
    useImport.SourceCreation.TextDate = local.SourceCreation.TextDate;
    useImport.TotalRecord.Total.Assign(local.TotalRecord.TotalInput);

    Call(FnProcessCtIntTotalsRecord.Execute, useImport, useExport);

    local.IsCashReceiptDeleted.Flag = useExport.IsCashReceiptDeleted.Flag;
    useExport.Errors.CopyTo(local.AaaGroupLocalErrors, MoveErrors);
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
    entities.Check.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", local.Check.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.Check.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Check.CstIdentifier = db.GetInt32(reader, 1);
        entities.Check.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Check.SequentialNumber = db.GetInt32(reader, 3);
        entities.Check.Populated = true;
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
    /// <summary>A HeaderRecordGroup group.</summary>
    [Serializable]
    public class HeaderRecordGroup
    {
      /// <summary>
      /// A value of HeaderInputCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("headerInputCashReceiptSourceType")]
      public CashReceiptSourceType HeaderInputCashReceiptSourceType
      {
        get => headerInputCashReceiptSourceType ??= new();
        set => headerInputCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of HeaderInputCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("headerInputCashReceiptEvent")]
      public CashReceiptEvent HeaderInputCashReceiptEvent
      {
        get => headerInputCashReceiptEvent ??= new();
        set => headerInputCashReceiptEvent = value;
      }

      private CashReceiptSourceType headerInputCashReceiptSourceType;
      private CashReceiptEvent headerInputCashReceiptEvent;
    }

    /// <summary>A CollectionRecordGroup group.</summary>
    [Serializable]
    public class CollectionRecordGroup
    {
      /// <summary>
      /// A value of CollectionInputCollectionType.
      /// </summary>
      [JsonPropertyName("collectionInputCollectionType")]
      public CollectionType CollectionInputCollectionType
      {
        get => collectionInputCollectionType ??= new();
        set => collectionInputCollectionType = value;
      }

      /// <summary>
      /// A value of CollectionInputCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("collectionInputCashReceiptDetail")]
      public CashReceiptDetail CollectionInputCashReceiptDetail
      {
        get => collectionInputCashReceiptDetail ??= new();
        set => collectionInputCashReceiptDetail = value;
      }

      private CollectionType collectionInputCollectionType;
      private CashReceiptDetail collectionInputCashReceiptDetail;
    }

    /// <summary>A FeesRecordGroup group.</summary>
    [Serializable]
    public class FeesRecordGroup
    {
      /// <summary>
      /// A value of FeesInputSrs.
      /// </summary>
      [JsonPropertyName("feesInputSrs")]
      public CashReceiptDetailFee FeesInputSrs
      {
        get => feesInputSrs ??= new();
        set => feesInputSrs = value;
      }

      /// <summary>
      /// A value of FeesInputSendingState.
      /// </summary>
      [JsonPropertyName("feesInputSendingState")]
      public CashReceiptDetailFee FeesInputSendingState
      {
        get => feesInputSendingState ??= new();
        set => feesInputSendingState = value;
      }

      /// <summary>
      /// A value of FeesInputCourt.
      /// </summary>
      [JsonPropertyName("feesInputCourt")]
      public CashReceiptDetailFee FeesInputCourt
      {
        get => feesInputCourt ??= new();
        set => feesInputCourt = value;
      }

      /// <summary>
      /// A value of FeesInputMiscellaneous.
      /// </summary>
      [JsonPropertyName("feesInputMiscellaneous")]
      public CashReceiptDetailFee FeesInputMiscellaneous
      {
        get => feesInputMiscellaneous ??= new();
        set => feesInputMiscellaneous = value;
      }

      private CashReceiptDetailFee feesInputSrs;
      private CashReceiptDetailFee feesInputSendingState;
      private CashReceiptDetailFee feesInputCourt;
      private CashReceiptDetailFee feesInputMiscellaneous;
    }

    /// <summary>A AdjustmentRecordGroup group.</summary>
    [Serializable]
    public class AdjustmentRecordGroup
    {
      /// <summary>
      /// A value of AdjustmentInput.
      /// </summary>
      [JsonPropertyName("adjustmentInput")]
      public CashReceiptDetailRlnRsn AdjustmentInput
      {
        get => adjustmentInput ??= new();
        set => adjustmentInput = value;
      }

      /// <summary>
      /// A value of AdjustmentInputOriginal.
      /// </summary>
      [JsonPropertyName("adjustmentInputOriginal")]
      public CashReceiptDetail AdjustmentInputOriginal
      {
        get => adjustmentInputOriginal ??= new();
        set => adjustmentInputOriginal = value;
      }

      /// <summary>
      /// A value of AdjustmentInputNew.
      /// </summary>
      [JsonPropertyName("adjustmentInputNew")]
      public CashReceiptDetail AdjustmentInputNew
      {
        get => adjustmentInputNew ??= new();
        set => adjustmentInputNew = value;
      }

      private CashReceiptDetailRlnRsn adjustmentInput;
      private CashReceiptDetail adjustmentInputOriginal;
      private CashReceiptDetail adjustmentInputNew;
    }

    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of TotalInput.
      /// </summary>
      [JsonPropertyName("totalInput")]
      public CashReceiptEvent TotalInput
      {
        get => totalInput ??= new();
        set => totalInput = value;
      }

      private CashReceiptEvent totalInput;
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

      /// <summary>
      /// A value of Amount.
      /// </summary>
      [JsonPropertyName("amount")]
      public Common Amount
      {
        get => amount ??= new();
        set => amount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private ProgramControlTotal programControlTotal;
      private Common amount;
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

    /// <summary>
    /// A value of OriginalPassed.
    /// </summary>
    [JsonPropertyName("originalPassed")]
    public CashReceiptDetail OriginalPassed
    {
      get => originalPassed ??= new();
      set => originalPassed = value;
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
    /// A value of ErrorInfo.
    /// </summary>
    [JsonPropertyName("errorInfo")]
    public Common ErrorInfo
    {
      get => errorInfo ??= new();
      set => errorInfo = value;
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
    /// A value of SuspendedDetail.
    /// </summary>
    [JsonPropertyName("suspendedDetail")]
    public Common SuspendedDetail
    {
      get => suspendedDetail ??= new();
      set => suspendedDetail = value;
    }

    /// <summary>
    /// A value of ReleasedDetail.
    /// </summary>
    [JsonPropertyName("releasedDetail")]
    public Common ReleasedDetail
    {
      get => releasedDetail ??= new();
      set => releasedDetail = value;
    }

    /// <summary>
    /// A value of SaveCourt.
    /// </summary>
    [JsonPropertyName("saveCourt")]
    public CashReceiptSourceType SaveCourt
    {
      get => saveCourt ??= new();
      set => saveCourt = value;
    }

    /// <summary>
    /// A value of ReportHandling.
    /// </summary>
    [JsonPropertyName("reportHandling")]
    public EabReportSend ReportHandling
    {
      get => reportHandling ??= new();
      set => reportHandling = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of IsCashReceiptDeleted.
    /// </summary>
    [JsonPropertyName("isCashReceiptDeleted")]
    public Common IsCashReceiptDeleted
    {
      get => isCashReceiptDeleted ??= new();
      set => isCashReceiptDeleted = value;
    }

    /// <summary>
    /// A value of Swefb610AdjustmentCount.
    /// </summary>
    [JsonPropertyName("swefb610AdjustmentCount")]
    public Common Swefb610AdjustmentCount
    {
      get => swefb610AdjustmentCount ??= new();
      set => swefb610AdjustmentCount = value;
    }

    /// <summary>
    /// A value of Swefb610NonCashAmt.
    /// </summary>
    [JsonPropertyName("swefb610NonCashAmt")]
    public Common Swefb610NonCashAmt
    {
      get => swefb610NonCashAmt ??= new();
      set => swefb610NonCashAmt = value;
    }

    /// <summary>
    /// A value of Swefb610CashAmt.
    /// </summary>
    [JsonPropertyName("swefb610CashAmt")]
    public Common Swefb610CashAmt
    {
      get => swefb610CashAmt ??= new();
      set => swefb610CashAmt = value;
    }

    /// <summary>
    /// A value of Swefb610NonCashCount.
    /// </summary>
    [JsonPropertyName("swefb610NonCashCount")]
    public Common Swefb610NonCashCount
    {
      get => swefb610NonCashCount ??= new();
      set => swefb610NonCashCount = value;
    }

    /// <summary>
    /// A value of Swefb610CashCount.
    /// </summary>
    [JsonPropertyName("swefb610CashCount")]
    public Common Swefb610CashCount
    {
      get => swefb610CashCount ??= new();
      set => swefb610CashCount = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("restartCashReceiptSourceType")]
    public CashReceiptSourceType RestartCashReceiptSourceType
    {
      get => restartCashReceiptSourceType ??= new();
      set => restartCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("restartCashReceiptEvent")]
    public CashReceiptEvent RestartCashReceiptEvent
    {
      get => restartCashReceiptEvent ??= new();
      set => restartCashReceiptEvent = value;
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
    /// A value of Index.
    /// </summary>
    [JsonPropertyName("index")]
    public Common Index
    {
      get => index ??= new();
      set => index = value;
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
    /// A value of NullProgramError.
    /// </summary>
    [JsonPropertyName("nullProgramError")]
    public ProgramError NullProgramError
    {
      get => nullProgramError ??= new();
      set => nullProgramError = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of NullCashReceipt.
    /// </summary>
    [JsonPropertyName("nullCashReceipt")]
    public CashReceipt NullCashReceipt
    {
      get => nullCashReceipt ??= new();
      set => nullCashReceipt = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public CashReceipt Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of NullCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("nullCashReceiptDetail")]
    public CashReceiptDetail NullCashReceiptDetail
    {
      get => nullCashReceiptDetail ??= new();
      set => nullCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of NextCheckId.
    /// </summary>
    [JsonPropertyName("nextCheckId")]
    public CashReceiptDetail NextCheckId
    {
      get => nextCheckId ??= new();
      set => nextCheckId = value;
    }

    /// <summary>
    /// A value of NextId.
    /// </summary>
    [JsonPropertyName("nextId")]
    public ProgramError NextId
    {
      get => nextId ??= new();
      set => nextId = value;
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
    /// A value of SourceCreation.
    /// </summary>
    [JsonPropertyName("sourceCreation")]
    public DateWorkArea SourceCreation
    {
      get => sourceCreation ??= new();
      set => sourceCreation = value;
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
    /// Gets a value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecordGroup HeaderRecord
    {
      get => headerRecord ?? (headerRecord = new());
      set => headerRecord = value;
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
    /// Gets a value of FeesRecord.
    /// </summary>
    [JsonPropertyName("feesRecord")]
    public FeesRecordGroup FeesRecord
    {
      get => feesRecord ?? (feesRecord = new());
      set => feesRecord = value;
    }

    /// <summary>
    /// Gets a value of AdjustmentRecord.
    /// </summary>
    [JsonPropertyName("adjustmentRecord")]
    public AdjustmentRecordGroup AdjustmentRecord
    {
      get => adjustmentRecord ?? (adjustmentRecord = new());
      set => adjustmentRecord = value;
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
    /// Gets a value of ControlTotals.
    /// </summary>
    [JsonIgnore]
    public Array<ControlTotalsGroup> ControlTotals => controlTotals ??= new(
      ControlTotalsGroup.Capacity);

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
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public Common AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    private CashReceiptDetail originalPassed;
    private Common errorAbend;
    private Common errorInfo;
    private Common headerPrinted;
    private Common suspendedDetail;
    private Common releasedDetail;
    private CashReceiptSourceType saveCourt;
    private EabReportSend reportHandling;
    private EabFileHandling reportProcessing;
    private Common causeAbend;
    private Common errorOpen;
    private Common controlOpen;
    private CashReceiptType cashReceiptType;
    private AbendData abendData;
    private Common isCashReceiptDeleted;
    private Common swefb610AdjustmentCount;
    private Common swefb610NonCashAmt;
    private Common swefb610CashAmt;
    private Common swefb610NonCashCount;
    private Common swefb610CashCount;
    private CashReceiptSourceType restartCashReceiptSourceType;
    private CashReceiptEvent restartCashReceiptEvent;
    private Common firstTime;
    private Common index;
    private Common readSinceLastCommit;
    private ProgramError nullProgramError;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramRun programRun;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt nullCashReceipt;
    private CashReceipt check;
    private CashReceiptDetail nullCashReceiptDetail;
    private CashReceiptDetail nextCheckId;
    private ProgramError nextId;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea sourceCreation;
    private Common skipToNextHeaderRecord;
    private Common recordTypeReturned;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External hardcodePosition;
    private HeaderRecordGroup headerRecord;
    private CollectionRecordGroup collectionRecord;
    private FeesRecordGroup feesRecord;
    private AdjustmentRecordGroup adjustmentRecord;
    private TotalRecordGroup totalRecord;
    private Array<ControlTotalsGroup> controlTotals;
    private Array<AaaGroupLocalErrorsGroup> aaaGroupLocalErrors;
    private Common alreadyProcessed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public CashReceipt Check
    {
      get => check ??= new();
      set => check = value;
    }

    private CashReceipt check;
  }
#endregion
}
