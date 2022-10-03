// Program: FN_BPP1_FOUR_REC, ID: 373024155, model: 746.
// Short name: SWEFPP1B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_BPP1_FOUR_REC.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBpp1FourRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BPP1_FOUR_REC program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBpp1FourRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBpp1FourRec.
  /// </summary>
  public FnBpp1FourRec(IContext context, Import import, Export export):
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
    // 09/13/99     PR0127041  pphinney Convert selected 4 records to 1 records
    // One time run
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ReportProcessing.Action = "OPEN";
      local.ReportHandling.ProgramName = "SWEFBPP1";
      local.ReportHandling.ProcessDate = Now().Date;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

        return;
      }

      // Print ERROR Report
      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFBPP1";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandling.RptDetail = "  ERROR Checkpoint Program Name : " + local
        .ProgramCheckpointRestart.ProgramName + "   ERROR Message = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFBPP1";
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
      local.ReportHandling.ProgramName = "SWEFBPP1";
      local.ReportHandling.ProcessDate = Now().Date;
      UseCabErrorReport2();

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

    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeRead.FileInstruction = "READ";

    // *************************************
    // CALL EXTERNAL TO OPEN THE DRIVER FILE
    // *************************************
    local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
    UseEabFourRecDriver2();

    if (!Equal(local.PassArea.TextReturnCode, "00"))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";
      local.ReportProcessing.Action = "OPEN";
      local.ReportHandling.ProgramName = "SWEFBPP1";
      local.ReportHandling.ProcessDate = Now().Date;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

        return;
      }

      // Print ERROR Report
      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFBPP1";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ReportHandling.RptDetail = "  ERROR Return Code from OPEN: " + local
        .PassArea.TextReturnCode + "   ERROR Message = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFBPP1";
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
      local.ReportHandling.ProgramName = "SWEFBPP1";
      local.ReportHandling.ProcessDate = Now().Date;
      UseCabErrorReport2();

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

    // ********************************
    // PROCESS DRIVER RECORDS UNTIL EOF
    // ********************************
    local.LastUpdate.Timestamp = Now();
    local.LastUpdate.Timestamp =
      AddMicroseconds(local.LastUpdate.Timestamp, -
      Microsecond(local.LastUpdate.Timestamp));
    local.ForUpdate.LastUpdatedBy = global.UserId;

    do
    {
      global.Command = "";

      // *************************************
      // CALL EXTERNAL TO READ THE DRIVER FILE
      // *************************************
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      local.CauseAbend.Flag = "N";
      UseEabFourRecDriver1();

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "00":
          ++local.Swefbpp1Read.Count;

          break;
        case "EF":
          // *****************************************************************
          // End Of File.  The EAB closed the file.  Complete processing.
          // *****************************************************************
          goto AfterCycle;
        default:
          ExitState = "FILE_READ_ERROR_WITH_RB";

          // Print ERROR Report
          // IS ERROR file open?
          if (AsChar(local.ErrorOpen.Flag) != 'Y')
          {
            local.ReportProcessing.Action = "OPEN";
            local.ReportHandling.ProgramName = "SWEFBPP1";
            local.ReportHandling.ProcessDate = Now().Date;
            UseCabErrorReport2();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

              return;
            }

            local.ErrorOpen.Flag = "Y";
          }

          // Print ERROR Report
          // SPACING
          local.ReportHandling.RptDetail = "";
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFBPP1";
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.ReportHandling.RptDetail = "  ERROR Return Code from READ: " + local
            .PassArea.TextReturnCode + "   ERROR Message = " + local
            .ExitStateWorkArea.Message;
          local.ReportProcessing.Action = "WRITE";
          local.ReportHandling.ProgramName = "SWEFBPP1";
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          ++local.Swefbpp1ErrorCount.Count;

          if (local.Swefbpp1ErrorCount.Count > 50)
          {
            // SPACING
            local.ReportHandling.RptDetail = "";
            local.ReportProcessing.Action = "WRITE";
            local.ReportHandling.ProgramName = "SWEFBPP1";
            UseCabErrorReport1();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

              return;
            }

            local.ReportHandling.RptDetail =
              "EXCESSIVE ERRORS (> 50) -- the Program has Ended";
            local.ReportProcessing.Action = "WRITE";
            local.ReportHandling.ProgramName = "SWEFBPP1";
            UseCabErrorReport1();

            if (Equal(local.ReportProcessing.Status, "OK"))
            {
            }
            else
            {
              ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

              return;
            }

            local.CauseAbend.Flag = "Y";

            goto AfterCycle;
          }

          ExitState = "ACO_NN0000_ALL_OK";

          continue;
      }

      if (ReadCashReceiptDetail())
      {
        local.LastUpdate.Timestamp =
          AddMicroseconds(local.LastUpdate.Timestamp, 1);

        try
        {
          UpdateCashReceiptDetail();
          ++local.Swefbpp1MatchedUpdated.Count;

          if (local.Swefbpp1CommitCount.Count > local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            local.Swefbpp1CommitCount.Count = 0;
            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              local.CauseAbend.Flag = "Y";
              ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

              goto Read;
            }
          }

          ++local.Swefbpp1CommitCount.Count;

          continue;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

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
        ++local.Swefbpp1NotMatched.Count;

        continue;
      }

Read:

      // **************
      // PROCESS ERRORS
      // **************
      // IS ERROR file open?
      if (AsChar(local.ErrorOpen.Flag) != 'Y')
      {
        local.ReportProcessing.Action = "OPEN";
        local.ReportHandling.ProgramName = "SWEFBPP1";
        local.ReportHandling.ProcessDate = Now().Date;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        local.ErrorOpen.Flag = "Y";
      }

      // Print ERROR Report
      // SPACING
      local.ReportHandling.RptDetail = "";
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFBPP1";
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

      if (IsExitState("FN0000_INVALID_EXT_COMMIT_RTN_CD") && local
        .PassArea.NumericReturnCode > 0)
      {
        local.ExitStateWorkArea.Message =
          TrimEnd(local.ExitStateWorkArea.Message) + " = " + NumberToString
          (local.PassArea.NumericReturnCode, 15);
        local.PassArea.NumericReturnCode = 0;
      }

      local.ReportHandling.RptDetail = "  ERROR for Tran ID:  " + (
        local.From.InterfaceTransId ?? "") + "   ERROR Message = " + local
        .ExitStateWorkArea.Message;
      local.ReportProcessing.Action = "WRITE";
      local.ReportHandling.ProgramName = "SWEFBPP1";
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
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

    // *****************************************************************
    // The external hit the end of the driver file, closed the file and
    // returned an EF (EOF) indicator.
    // *****************************************************************
    // IS ERROR Report open?
    if (AsChar(local.ErrorOpen.Flag) == 'Y')
    {
      local.ReportProcessing.Action = "CLOSE";
      local.ReportHandling.ProgramName = "SWEFBPP1";
      local.ReportHandling.ProcessDate = Now().Date;
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

    // *******************************************
    // PRINT THE PROGRAM CONTROL - TOTALS
    // *******************************************
    // OPEN  CONTROL  Report * *
    local.ReportProcessing.Action = "OPEN";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    local.ReportHandling.ProcessDate = Now().Date;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // **********************************
    // WRITE OUT A CONTROL REPORT
    // **********************************
    local.ReportHandling.RptDetail = "Input Records READ.................." + NumberToString
      (local.Swefbpp1Read.Count, 7, 9);
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    local.ReportHandling.RptDetail = "Input Records Matched - Updated....." + NumberToString
      (local.Swefbpp1MatchedUpdated.Count, 7, 9);
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    local.ReportHandling.RptDetail = "Input Records - Not Matched........." + NumberToString
      (local.Swefbpp1NotMatched.Count, 7, 9);
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // Completion message
    if (AsChar(local.ErrorOpen.Flag) == 'Y')
    {
      local.ReportHandling.RptDetail =
        "* * * * ERROR REPORT Created - End of Job * * * *";
    }
    else if (local.Swefbpp1Read.Count == 0)
    {
      local.ReportHandling.RptDetail =
        "* * * * Succesful End of Job - INPUT File is EMPTY * * * *";
    }
    else
    {
      local.ReportHandling.RptDetail = "* * * * Succesful End of Job * * * *";
    }

    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // SPACING
    local.ReportHandling.RptDetail = "";
    local.ReportProcessing.Action = "WRITE";
    local.ReportHandling.ProgramName = "SWEFBPP1";
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
    local.ReportHandling.ProgramName = "SWEFBPP1";
    local.ReportHandling.ProcessDate = Now().Date;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

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

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabFourRecDriver1()
  {
    var useImport = new EabFourRecDriver.Import();
    var useExport = new EabFourRecDriver.Export();

    MoveExternal1(local.PassArea, useImport.External);
    useExport.To.InterfaceTransId = local.To.InterfaceTransId;
    useExport.From.InterfaceTransId = local.From.InterfaceTransId;
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabFourRecDriver.Execute, useImport, useExport);

    local.To.InterfaceTransId = useExport.To.InterfaceTransId;
    local.From.InterfaceTransId = useExport.From.InterfaceTransId;
    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseEabFourRecDriver2()
  {
    var useImport = new EabFourRecDriver.Import();
    var useExport = new EabFourRecDriver.Export();

    MoveExternal1(local.PassArea, useImport.External);
    MoveExternal2(local.PassArea, useExport.External);

    Call(EabFourRecDriver.Execute, useImport, useExport);

    MoveExternal2(useExport.External, local.PassArea);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "interfaceTranId", local.From.InterfaceTransId ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var interfaceTransId = local.To.InterfaceTransId ?? "";
    var lastUpdatedBy = local.ForUpdate.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.LastUpdate.Timestamp;

    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "interfaceTranId", interfaceTransId);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.InterfaceTransId = interfaceTransId;
    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.Populated = true;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of LastUpdate.
    /// </summary>
    [JsonPropertyName("lastUpdate")]
    public DateWorkArea LastUpdate
    {
      get => lastUpdate ??= new();
      set => lastUpdate = value;
    }

    /// <summary>
    /// A value of Swefbpp1ErrorCount.
    /// </summary>
    [JsonPropertyName("swefbpp1ErrorCount")]
    public Common Swefbpp1ErrorCount
    {
      get => swefbpp1ErrorCount ??= new();
      set => swefbpp1ErrorCount = value;
    }

    /// <summary>
    /// A value of Swefbpp1MatchedUpdated.
    /// </summary>
    [JsonPropertyName("swefbpp1MatchedUpdated")]
    public Common Swefbpp1MatchedUpdated
    {
      get => swefbpp1MatchedUpdated ??= new();
      set => swefbpp1MatchedUpdated = value;
    }

    /// <summary>
    /// A value of Swefbpp1NotMatched.
    /// </summary>
    [JsonPropertyName("swefbpp1NotMatched")]
    public Common Swefbpp1NotMatched
    {
      get => swefbpp1NotMatched ??= new();
      set => swefbpp1NotMatched = value;
    }

    /// <summary>
    /// A value of Swefbpp1Read.
    /// </summary>
    [JsonPropertyName("swefbpp1Read")]
    public Common Swefbpp1Read
    {
      get => swefbpp1Read ??= new();
      set => swefbpp1Read = value;
    }

    /// <summary>
    /// A value of Swefbpp1CommitCount.
    /// </summary>
    [JsonPropertyName("swefbpp1CommitCount")]
    public Common Swefbpp1CommitCount
    {
      get => swefbpp1CommitCount ??= new();
      set => swefbpp1CommitCount = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public CashReceiptDetail To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public CashReceiptDetail From
    {
      get => from ??= new();
      set => from = value;
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
    /// A value of ErrorAbend.
    /// </summary>
    [JsonPropertyName("errorAbend")]
    public Common ErrorAbend
    {
      get => errorAbend ??= new();
      set => errorAbend = value;
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
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public CashReceiptDetail ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ProgramProcessingInfo Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea lastUpdate;
    private Common swefbpp1ErrorCount;
    private Common swefbpp1MatchedUpdated;
    private Common swefbpp1NotMatched;
    private Common swefbpp1Read;
    private Common swefbpp1CommitCount;
    private CashReceiptDetail to;
    private CashReceiptDetail from;
    private External hardcodeOpen;
    private External hardcodeRead;
    private Common errorAbend;
    private EabReportSend reportHandling;
    private EabFileHandling reportProcessing;
    private Common causeAbend;
    private Common errorOpen;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private CashReceiptDetail forUpdate;
    private ProgramProcessingInfo zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
  }
#endregion
}
