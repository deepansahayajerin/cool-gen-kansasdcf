// Program: FN_B614_DELETE_REIP_CRDS, ID: 373301641, model: 746.
// Short name: SWEF614B
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
/// A program: FN_B614_DELETE_REIP_CRDS.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB614DeleteReipCrds: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B614_DELETE_REIP_CRDS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB614DeleteReipCrds(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB614DeleteReipCrds.
  /// </summary>
  public FnB614DeleteReipCrds(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************************************
    // **                     M A I N T E N A N C E    L O G
    // ********************************************************************************
    // **  Date	PR/WR #		UserID		Description
    // ********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeClose.FileInstruction = "CLOSE";
    local.HardcodeWrite.FileInstruction = "WRITE";
    local.HardcodeForRead.Code = "REIPDELETE";
    local.ErrorFound.Flag = "N";
    local.ErrorOpen.Flag = "N";

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      // * * * * * * * * * *
      // SAVE the Exit_State message
      // * * * * * * * * * *
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

      // * * * * * * * * * *
      // OPEN the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // WRITE the ERROR REPORT - SPACING
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // WRITE the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "Program has ABENDED - Exit State = " + local
        .ExitStateWorkArea.Message;
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // WRITE the ERROR REPORT - SPACING
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // CLOSE the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeClose.FileInstruction;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // A Program ERROR Occured to get HERE
      // -- ALL Errors have been written to Reports and files closed
      // * * * * * * * * * *
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ******************************************************************
    // If NO Error found -- Process the Records
    // ******************************************************************
    if (AsChar(local.ErrorFound.Flag) == 'N')
    {
      if (ReadCashReceiptDetailStatus())
      {
        // Status is Valid -- Continue
      }
      else
      {
        ExitState = "FN0000_CASH_RECEIPT_STATUS_NF";

        // * * * * * * * * * *
        // SAVE the Exit_State message
        // * * * * * * * * * *
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

        // * * * * * * * * * *
        // OPEN the ERROR REPORT
        // * * * * * * * * * *
        local.ReportHandling.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.ReportHandling.ProgramName = "SWEFB614";
        local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_OPENING_ERR_RPT_FILE";

          return;
        }

        // * * * * * * * * * *
        // WRITE the ERROR REPORT - SPACING
        // * * * * * * * * * *
        local.ReportHandling.RptDetail = "";
        local.ReportHandling.ProgramName = "SWEFB614";
        local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // * * * * * * * * * *
        // WRITE the ERROR REPORT
        // * * * * * * * * * *
        local.ReportHandling.RptDetail =
          "Program has ABENDED - Exit State = " + local
          .ExitStateWorkArea.Message;
        local.ReportHandling.ProgramName = "SWEFB614";
        local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // * * * * * * * * * *
        // WRITE the ERROR REPORT - SPACING
        // * * * * * * * * * *
        local.ReportHandling.RptDetail = "";
        local.ReportHandling.ProgramName = "SWEFB614";
        local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
        UseCabErrorReport1();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

          return;
        }

        // * * * * * * * * * *
        // CLOSE the ERROR REPORT
        // * * * * * * * * * *
        local.ReportHandling.ProgramName = "SWEFB614";
        local.ReportProcessing.Action = local.HardcodeClose.FileInstruction;
        UseCabErrorReport2();

        if (Equal(local.ReportProcessing.Status, "OK"))
        {
        }
        else
        {
          ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

          return;
        }

        // * * * * * * * * * *
        // A Program ERROR Occured to get HERE
        // -- ALL Errors have been written to Reports and files closed
        // * * * * * * * * * *
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.TotalNumRecordsDeleted.Count = 0;
      local.TotalErrorRecords.Count = 0;

      foreach(var item in ReadCashReceiptDetailCashReceipt())
      {
        foreach(var item1 in ReadCollection())
        {
          if (AsChar(local.ErrorOpen.Flag) != 'Y')
          {
            // * * * * * * * * * *
            // OPEN the ERROR REPORT
            // * * * * * * * * * *
            local.ReportHandling.ProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
            local.ReportHandling.ProgramName = "SWEFB614";
            local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
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

          // * * * * * * * * * *
          // WRITE the ERROR REPORT - SPACING
          // * * * * * * * * * *
          local.ReportHandling.RptDetail = "";
          local.ReportHandling.ProgramName = "SWEFB614";
          local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          // * * * * * * * * * *
          // WRITE the ERROR REPORT
          // * * * * * * * * * *
          local.ReportHandling.RptDetail = "Cash Receipt = " + NumberToString
            (entities.CashReceipt.SequentialNumber, 7, 9) + " Cash Receipt Detail = " +
            NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 7, 9) + " was NOT Deleted - Collection Found";
            
          local.ReportHandling.ProgramName = "SWEFB614";
          local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          local.ErrorFound.Flag = "Y";
          ++local.TotalErrorRecords.Count;

          goto ReadEach;
        }

        foreach(var item1 in ReadReceiptRefund())
        {
          if (AsChar(local.ErrorOpen.Flag) != 'Y')
          {
            // * * * * * * * * * *
            // OPEN the ERROR REPORT
            // * * * * * * * * * *
            local.ReportHandling.ProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
            local.ReportHandling.ProgramName = "SWEFB614";
            local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
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

          // * * * * * * * * * *
          // WRITE the ERROR REPORT - SPACING
          // * * * * * * * * * *
          local.ReportHandling.RptDetail = "";
          local.ReportHandling.ProgramName = "SWEFB614";
          local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          // * * * * * * * * * *
          // WRITE the ERROR REPORT
          // * * * * * * * * * *
          local.ReportHandling.RptDetail = "Cash Receipt = " + NumberToString
            (entities.CashReceipt.SequentialNumber, 7, 9) + " Cash Receipt Detail = " +
            NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 7, 9) + " was NOT Deleted - Refund Found";
            
          local.ReportHandling.ProgramName = "SWEFB614";
          local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          local.ErrorFound.Flag = "Y";
          ++local.TotalErrorRecords.Count;

          goto ReadEach;
        }

        foreach(var item1 in ReadCashReceiptDetailFee())
        {
          if (AsChar(local.ErrorOpen.Flag) != 'Y')
          {
            // * * * * * * * * * *
            // OPEN the ERROR REPORT
            // * * * * * * * * * *
            local.ReportHandling.ProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
            local.ReportHandling.ProgramName = "SWEFB614";
            local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
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

          // * * * * * * * * * *
          // WRITE the ERROR REPORT - SPACING
          // * * * * * * * * * *
          local.ReportHandling.RptDetail = "";
          local.ReportHandling.ProgramName = "SWEFB614";
          local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          // * * * * * * * * * *
          // WRITE the ERROR REPORT
          // * * * * * * * * * *
          local.ReportHandling.RptDetail = "Cash Receipt = " + NumberToString
            (entities.CashReceipt.SequentialNumber, 7, 9) + " Cash Receipt Detail = " +
            NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 7, 9) + " was NOT Deleted - Fees Found";
            
          local.ReportHandling.ProgramName = "SWEFB614";
          local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
          UseCabErrorReport1();

          if (Equal(local.ReportProcessing.Status, "OK"))
          {
          }
          else
          {
            ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

            return;
          }

          local.ErrorFound.Flag = "Y";
          ++local.TotalErrorRecords.Count;

          goto ReadEach;
        }

        foreach(var item1 in ReadCashReceiptDetailAddress())
        {
          DeleteCashReceiptDetailAddress();
        }

        DeleteCashReceiptDetail();
        ++local.TotalNumRecordsDeleted.Count;

ReadEach:
        ;
      }
    }

    // ---------------------------------------------
    // After all processing has completed
    // Print the control total Report which
    // will reflect the total deletes.
    // ---------------------------------------------
    // * * * * * * * * * *
    // OPEN the CONTROL REPORT
    // * * * * * * * * * *
    local.ReportHandling.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandling.ProgramName = "SWEFB614";
    local.ReportProcessing.Action = local.HardcodeOpen.FileInstruction;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_OPENING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * *
    // WRITE the CONTROL REPORT -- Total Count of Records Processed
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Number of Cash Receipt Detail Records Deleted....................." +
      NumberToString(local.TotalNumRecordsDeleted.Count, 15);
    local.ReportHandling.ProgramName = "SWEFB614";
    local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * *
    // WRITE the CONTROL REPORT - SPACING
    // * * * * * * * * * *
    local.ReportHandling.RptDetail = "";
    local.ReportHandling.ProgramName = "SWEFB614";
    local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * *
    // WRITE the CONTROL REPORT -- Total Count of ERROR Records Bypassed
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Number of REIP Cash Receipt Detail Records NOT Deleted............" +
      NumberToString(local.TotalErrorRecords.Count, 15);
    local.ReportHandling.ProgramName = "SWEFB614";
    local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * *
    // WRITE the CONTROL REPORT - SPACING
    // * * * * * * * * * *
    local.ReportHandling.RptDetail = "";
    local.ReportHandling.ProgramName = "SWEFB614";
    local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
    UseCabControlReport1();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

      return;
    }

    // * * * * * * * * * *
    // Error found - write message on CONTROL Report
    // * * * * * * * * * *
    if (AsChar(local.ErrorFound.Flag) == 'Y')
    {
      // * * * * * * * * * *
      // WRITE the CONTROL REPORT -- ERROR Message
      // * * * * * * * * * *
      local.ReportHandling.RptDetail =
        "* * *  ERRORs FOUND - SEE ERROR REPORT * * *";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabControlReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // WRITE the CONTROL REPORT - SPACING
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
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
    else if (local.TotalNumRecordsDeleted.Count == 0)
    {
      // * * * * * * * * * *
      // WRITE the CONTROL REPORT -- NO RECORDS PROCESSED
      // * * * * * * * * * *
      local.ReportHandling.RptDetail =
        "* * *  NO Records were Deleted this Run * * *";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabControlReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // WRITE the CONTROL REPORT - SPACING
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
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
    else
    {
      // * * * * * * * * * *
      // WRITE the CONTROL REPORT -- SUCCESSFUL END OF JOB
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "* * *  Successful End of Job * * *";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabControlReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // WRITE the CONTROL REPORT - SPACING
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
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

    // * * * * * * * * * *
    // CLOSE the CONTROL REPORT
    // * * * * * * * * * *
    local.ReportHandling.ProgramName = "SWEFB614";
    local.ReportProcessing.Action = local.HardcodeClose.FileInstruction;
    UseCabControlReport2();

    if (Equal(local.ReportProcessing.Status, "OK"))
    {
    }
    else
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      return;
    }

    // Process ERROR Report
    if (AsChar(local.ErrorOpen.Flag) == 'Y')
    {
      // * * * * * * * * * *
      // WRITE the ERROR REPORT - SPACING
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = "";
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeWrite.FileInstruction;
      UseCabErrorReport1();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
      }
      else
      {
        ExitState = "SI0000_ERR_WRITING_ERR_RPT_FILE";

        return;
      }

      // * * * * * * * * * *
      // CLOSE the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.ProgramName = "SWEFB614";
      local.ReportProcessing.Action = local.HardcodeClose.FileInstruction;
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

    // * * * * * * * * * *
    // Successful EOJ
    // * * * * * * * * * *
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    useImport.NeededToWrite.RptDetail = local.ReportHandling.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportProcessing.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.ReportProcessing.Action;
    MoveEabReportSend(local.ReportHandling, useImport.NeededToOpen);

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

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void DeleteCashReceiptDetail()
  {
    Update("DeleteCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });
  }

  private void DeleteCashReceiptDetailAddress()
  {
    Update("DeleteCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.CashReceiptDetailAddress.SystemGeneratedIdentifier.
            GetValueOrDefault());
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailAddress.Populated = false;

    return ReadEach("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.CashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.CashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.CashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.CashReceiptDetailAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceipt()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cdsIdentifier",
          entities.ReipdeleteCashReceiptDetailStatus.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 4);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 5);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 6);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailFee.Populated = false;

    return ReadEach("ReadCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFee.CrdIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailFee.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailFee.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailFee.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 4);
        entities.CashReceiptDetailFee.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.ReipdeleteCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetString(command, "code", local.HardcodeForRead.Code);
      },
      (db, reader) =>
      {
        entities.ReipdeleteCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReipdeleteCashReceiptDetailStatus.Code =
          db.GetString(reader, 1);
        entities.ReipdeleteCashReceiptDetailStatus.Name =
          db.GetString(reader, 2);
        entities.ReipdeleteCashReceiptDetailStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ReipdeleteReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ReipdeleteReceiptRefund.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ReipdeleteReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReipdeleteReceiptRefund.CrvIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ReipdeleteReceiptRefund.CrdIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.ReipdeleteReceiptRefund.CrtIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ReipdeleteReceiptRefund.CstIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ReipdeleteReceiptRefund.Populated = true;

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
    /// A value of ErrorOpen.
    /// </summary>
    [JsonPropertyName("errorOpen")]
    public Common ErrorOpen
    {
      get => errorOpen ??= new();
      set => errorOpen = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of TotalNumRecordsDeleted.
    /// </summary>
    [JsonPropertyName("totalNumRecordsDeleted")]
    public Common TotalNumRecordsDeleted
    {
      get => totalNumRecordsDeleted ??= new();
      set => totalNumRecordsDeleted = value;
    }

    /// <summary>
    /// A value of TotalErrorRecords.
    /// </summary>
    [JsonPropertyName("totalErrorRecords")]
    public Common TotalErrorRecords
    {
      get => totalErrorRecords ??= new();
      set => totalErrorRecords = value;
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
    /// A value of HardcodeClose.
    /// </summary>
    [JsonPropertyName("hardcodeClose")]
    public External HardcodeClose
    {
      get => hardcodeClose ??= new();
      set => hardcodeClose = value;
    }

    /// <summary>
    /// A value of HardcodeWrite.
    /// </summary>
    [JsonPropertyName("hardcodeWrite")]
    public External HardcodeWrite
    {
      get => hardcodeWrite ??= new();
      set => hardcodeWrite = value;
    }

    /// <summary>
    /// A value of HardcodeForRead.
    /// </summary>
    [JsonPropertyName("hardcodeForRead")]
    public CashReceiptDetailStatus HardcodeForRead
    {
      get => hardcodeForRead ??= new();
      set => hardcodeForRead = value;
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
    /// A value of ReportProcessing.
    /// </summary>
    [JsonPropertyName("reportProcessing")]
    public EabFileHandling ReportProcessing
    {
      get => reportProcessing ??= new();
      set => reportProcessing = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private Common errorOpen;
    private Common errorFound;
    private Common totalNumRecordsDeleted;
    private Common totalErrorRecords;
    private External hardcodeOpen;
    private External hardcodeClose;
    private External hardcodeWrite;
    private CashReceiptDetailStatus hardcodeForRead;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling reportProcessing;
    private EabReportSend reportHandling;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    /// <summary>
    /// A value of ReipdeleteReceiptRefund.
    /// </summary>
    [JsonPropertyName("reipdeleteReceiptRefund")]
    public ReceiptRefund ReipdeleteReceiptRefund
    {
      get => reipdeleteReceiptRefund ??= new();
      set => reipdeleteReceiptRefund = value;
    }

    /// <summary>
    /// A value of ReipdeleteCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("reipdeleteCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ReipdeleteCashReceiptDetailStatus
    {
      get => reipdeleteCashReceiptDetailStatus ??= new();
      set => reipdeleteCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ReipdeleteCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("reipdeleteCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ReipdeleteCashReceiptDetailStatHistory
    {
      get => reipdeleteCashReceiptDetailStatHistory ??= new();
      set => reipdeleteCashReceiptDetailStatHistory = value;
    }

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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private Collection collection;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetailFee cashReceiptDetailFee;
    private ReceiptRefund reipdeleteReceiptRefund;
    private CashReceiptDetailStatus reipdeleteCashReceiptDetailStatus;
    private CashReceiptDetailStatHistory reipdeleteCashReceiptDetailStatHistory;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
  }
#endregion
}
