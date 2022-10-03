// Program: FN_B635_PROCESS_RETRO_CASH_RCPTS, ID: 372269891, model: 746.
// Short name: SWEF635B
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
/// A program: FN_B635_PROCESS_RETRO_CASH_RCPTS.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This nightly batch procedure provides a means of removing all 
/// collections for a obligor that have previously been distributed to various
/// obligations back to the date of the retroactive cash receipt.  This will
/// allow the distribution policy to redistribute all collections, thus ensuring
/// that the distribution policy rules are followed correctly.
/// Note:  A retroactive collection is a collection whose CASH_RECEIPT_DETAIL 
/// COLLECTION_DATE is less than the current month.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB635ProcessRetroCashRcpts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B635_PROCESS_RETRO_CASH_RCPTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB635ProcessRetroCashRcpts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB635ProcessRetroCashRcpts.
  /// </summary>
  public FnB635ProcessRetroCashRcpts(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    //   DATE      DEVELOPER  REQUEST #  DESCRIPTION
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/29/1998  Ed Lyman              Initial Development
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/02/1999  Ed Lyman   PR# 81189  Added program start timestamp.
    // ----------  ---------  ---------  
    // -------------------------------------
    // ..... Select only CRDs in 'Released', ' Distributed' or 'Refunded' status
    // where an undistributed amount remains.
    // Determine if it is a retroactive cash receipt detail by reading for any 
    // subsequent cash receipt details.  If found, reverse any collections
    // belonging to the subsequent cash receipt details so that distribution may
    // reapply the cash receipt detail in light of the new circumstances.
    // 1. Read cash receipt details by ascending collection date within person 
    // number.
    // 2. Are there any subsequent cash receipt details, i.e., those with a 
    // collection date greater than this cash receipt detail?
    // 3. Do not consider court order number at this point.
    // 4. Joint and Several must be handled.
    // 5.
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramStart.Timestamp = Now();
    local.TypeOfChange.SelectChar = "C";
    UseFnB635Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE") || IsExitState
        ("PROGRAM_PROCESSING_INFO_NF"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
      else
      {
        // **********************************************************
        // WRITE TO ERROR REPORT 99
        // **********************************************************
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport();
      }
    }
    else
    {
      local.Collection.CollectionAdjustmentReasonTxt =
        local.RetroCollection.Description ?? "";
      local.EabFileHandling.Action = "WRITE";

      if (!IsEmpty(local.PpiParameter.ObligorPersonNumber))
      {
        foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus1())
        {
          if (Equal(entities.CashReceiptDetail.ObligorPersonNumber,
            local.Previous.ObligorPersonNumber))
          {
            // ***  Only need to process the earliest retro collection. ***
            continue;
          }
          else
          {
            local.Previous.ObligorPersonNumber =
              entities.CashReceiptDetail.ObligorPersonNumber;

            if (AsChar(local.ReportNeeded.Flag) == 'Y')
            {
              // *** Format Receipt Amount ***
              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.CashReceiptDetail.
                  CollectionAmount * 100), 15);

              if (entities.CashReceiptDetail.CollectionAmount < 0)
              {
                local.EabConvertNumeric.SendSign = "-";
              }

              UseEabConvertNumeric1();
              local.Receipt.ReturnCurrencySigned =
                local.EabConvertNumeric.ReturnCurrencySigned;
              local.EabConvertNumeric.Assign(local.Clear);

              // *** Format Distributed Amount ***
              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.CashReceiptDetail.
                  DistributedAmount.GetValueOrDefault() * 100), 15);

              if (Lt(entities.CashReceiptDetail.DistributedAmount, 0))
              {
                local.EabConvertNumeric.SendSign = "-";
              }

              UseEabConvertNumeric1();
              local.DistributedEabConvertNumeric.ReturnCurrencySigned =
                local.EabConvertNumeric.ReturnCurrencySigned;
              local.EabConvertNumeric.Assign(local.Clear);

              // *** Format Refunded Amount ***
              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.CashReceiptDetail.RefundedAmount.
                  GetValueOrDefault() * 100), 15);

              if (Lt(entities.CashReceiptDetail.RefundedAmount, 0))
              {
                local.EabConvertNumeric.SendSign = "-";
              }

              UseEabConvertNumeric1();
              local.RefundedEabConvertNumeric.ReturnCurrencySigned =
                local.EabConvertNumeric.ReturnCurrencySigned;
              local.EabConvertNumeric.Assign(local.Clear);
              local.FormatDate.Text10 =
                NumberToString(Month(entities.CashReceiptDetail.CollectionDate),
                14, 2) + "-" + NumberToString
                (Day(entities.CashReceiptDetail.CollectionDate), 14, 2) + "-"
                + NumberToString
                (Year(entities.CashReceiptDetail.CollectionDate), 12, 4);

              // **********************************************************
              // WRITE TO BUSINESS REPORT 01
              // **********************************************************
              local.EabReportSend.RptDetail = "";
              UseCabBusinessReport01();
              local.EabReportSend.RptDetail =
                entities.CashReceiptDetailStatus.Code + "  " + entities
                .CashReceiptDetail.ObligorPersonNumber + "   " + local
                .FormatDate.Text10 + "    " + NumberToString
                (entities.CashReceiptDetail.SequentialIdentifier, 13, 3) + "  " +
                Substring
                (local.Receipt.ReturnCurrencySigned,
                EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " +
                "            " + " " + Substring
                (local.DistributedEabConvertNumeric.ReturnCurrencySigned,
                EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " +
                Substring
                (local.RefundedEabConvertNumeric.ReturnCurrencySigned,
                EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + "   " +
                entities.CashReceiptDetail.CourtOrderNumber;
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }

          if (ReadCashReceiptCashReceiptTypeCashReceiptEvent())
          {
            local.Collection.CollectionAdjustmentDt =
              local.ProgramProcessingInfo.ProcessDate;
            UseFnCabReverseAllCshRcptDtls();
          }
          else
          {
            ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (local.UpdatesSinceLastCommit.Count >= local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                entities.CashReceiptDetail.ObligorPersonNumber;
              UseUpdatePgmCheckpointRestart();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Failure writing a checkpoint record.  Obligor number = " + entities
                  .CashReceiptDetail.ObligorPersonNumber;

                // **********************************************************
                // WRITE TO ERROR REPORT 99
                // **********************************************************
                UseCabErrorReport();

                goto Test;
              }

              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Failure trying to commit a unit of work.  Obligor number = " +
                  entities.CashReceiptDetail.ObligorPersonNumber;

                // **********************************************************
                // WRITE TO ERROR REPORT 99
                // **********************************************************
                UseCabErrorReport();
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                goto Test;
              }

              local.UpdatesSinceLastCommit.Count = 0;
            }
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Failure trying to read or update database.  Obligor number = " +
              entities.CashReceiptDetail.ObligorPersonNumber;

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "Exit state message:  " + local
              .ExitStateWorkArea.Message;

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();

            if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              goto Test;
            }
          }
        }
      }
      else
      {
        foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus2())
        {
          if (Equal(entities.CashReceiptDetail.ObligorPersonNumber,
            local.Previous.ObligorPersonNumber))
          {
            // ***  Only need to process the earliest retro collection. ***
            continue;
          }
          else
          {
            local.Previous.ObligorPersonNumber =
              entities.CashReceiptDetail.ObligorPersonNumber;

            if (AsChar(local.ReportNeeded.Flag) == 'Y')
            {
              // *** Format Receipt Amount ***
              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.CashReceiptDetail.
                  CollectionAmount * 100), 15);

              if (entities.CashReceiptDetail.CollectionAmount < 0)
              {
                local.EabConvertNumeric.SendSign = "-";
              }

              UseEabConvertNumeric1();
              local.Receipt.ReturnCurrencySigned =
                local.EabConvertNumeric.ReturnCurrencySigned;
              local.EabConvertNumeric.Assign(local.Clear);

              // *** Format Distributed Amount ***
              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.CashReceiptDetail.
                  DistributedAmount.GetValueOrDefault() * 100), 15);

              if (Lt(entities.CashReceiptDetail.DistributedAmount, 0))
              {
                local.EabConvertNumeric.SendSign = "-";
              }

              UseEabConvertNumeric1();
              local.DistributedEabConvertNumeric.ReturnCurrencySigned =
                local.EabConvertNumeric.ReturnCurrencySigned;
              local.EabConvertNumeric.Assign(local.Clear);

              // *** Format Refunded Amount ***
              local.EabConvertNumeric.SendAmount =
                NumberToString((long)(entities.CashReceiptDetail.RefundedAmount.
                  GetValueOrDefault() * 100), 15);

              if (Lt(entities.CashReceiptDetail.RefundedAmount, 0))
              {
                local.EabConvertNumeric.SendSign = "-";
              }

              UseEabConvertNumeric1();
              local.RefundedEabConvertNumeric.ReturnCurrencySigned =
                local.EabConvertNumeric.ReturnCurrencySigned;
              local.EabConvertNumeric.Assign(local.Clear);
              local.FormatDate.Text10 =
                NumberToString(Month(entities.CashReceiptDetail.CollectionDate),
                14, 2) + "-" + NumberToString
                (Day(entities.CashReceiptDetail.CollectionDate), 14, 2) + "-"
                + NumberToString
                (Year(entities.CashReceiptDetail.CollectionDate), 12, 4);

              // **********************************************************
              // WRITE TO BUSINESS REPORT 01
              // **********************************************************
              local.EabReportSend.RptDetail = "";
              UseCabBusinessReport01();
              local.EabReportSend.RptDetail =
                entities.CashReceiptDetailStatus.Code + "  " + entities
                .CashReceiptDetail.ObligorPersonNumber + "   " + local
                .FormatDate.Text10 + "    " + NumberToString
                (entities.CashReceiptDetail.SequentialIdentifier, 13, 3) + "  " +
                Substring
                (local.Receipt.ReturnCurrencySigned,
                EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " +
                "            " + " " + Substring
                (local.DistributedEabConvertNumeric.ReturnCurrencySigned,
                EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " +
                Substring
                (local.RefundedEabConvertNumeric.ReturnCurrencySigned,
                EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + "   " +
                entities.CashReceiptDetail.CourtOrderNumber;
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }

          if (ReadCashReceiptCashReceiptTypeCashReceiptEvent())
          {
            local.Collection.CollectionAdjustmentDt =
              local.ProgramProcessingInfo.ProcessDate;
            UseFnCabReverseAllCshRcptDtls();
          }
          else
          {
            ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (local.UpdatesSinceLastCommit.Count >= local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                entities.CashReceiptDetail.ObligorPersonNumber;
              UseUpdatePgmCheckpointRestart();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Failure writing a checkpoint record.  Obligor number = " + entities
                  .CashReceiptDetail.ObligorPersonNumber;

                // **********************************************************
                // WRITE TO ERROR REPORT 99
                // **********************************************************
                UseCabErrorReport();

                goto Test;
              }

              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Failure trying to commit a unit of work.  Obligor number = " +
                  entities.CashReceiptDetail.ObligorPersonNumber;

                // **********************************************************
                // WRITE TO ERROR REPORT 99
                // **********************************************************
                UseCabErrorReport();
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                goto Test;
              }
              else
              {
                local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-"
                  + NumberToString(Now().Date.Month, 14, 2) + "-" + NumberToString
                  (Now().Date.Day, 14, 2);
                local.Time.Text8 =
                  NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
                local.EabReportSend.RptDetail =
                  "Checkpoint Taken after person number: " + entities
                  .CashReceiptDetail.ObligorPersonNumber + " Date: " + local
                  .Date.Text10 + " Time: " + local.Time.Text8;
                UseCabControlReport();
              }

              local.UpdatesSinceLastCommit.Count = 0;
            }
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Failure trying to read or update database.  Obligor number = " +
              entities.CashReceiptDetail.ObligorPersonNumber;

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "Exit state message:  " + local
              .ExitStateWorkArea.Message;

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();

            if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              goto Test;
            }
          }
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB635Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseFnB635Close();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnCurrencySigned = source.ReturnCurrencySigned;
    target.ReturnCurrencyNegInParens = source.ReturnCurrencyNegInParens;
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
    target.ReturnOkFlag = source.ReturnOkFlag;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.Assign(local.EabConvertNumeric);

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
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

  private void UseFnB635Close()
  {
    var useImport = new FnB635Close.Import();
    var useExport = new FnB635Close.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;

    Call(FnB635Close.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB635Housekeeping()
  {
    var useImport = new FnB635Housekeeping.Import();
    var useExport = new FnB635Housekeeping.Export();

    Call(FnB635Housekeeping.Execute, useImport, useExport);

    local.PpiParameter.ObligorPersonNumber =
      useExport.PpiParameter.ObligorPersonNumber;
    local.Restart.ObligorPersonNumber = useExport.Restart.ObligorPersonNumber;
    local.SuspendedStatus.SystemGeneratedIdentifier =
      useExport.SuspendedStatus.SystemGeneratedIdentifier;
    local.Max.Date = useExport.Max.Date;
    local.RefundedCashReceiptDetailStatus.SystemGeneratedIdentifier =
      useExport.RefundedStatus.SystemGeneratedIdentifier;
    local.DistributedCashReceiptDetailStatus.SystemGeneratedIdentifier =
      useExport.DistributedStatus.SystemGeneratedIdentifier;
    local.Released.SystemGeneratedIdentifier =
      useExport.ReleasedStatus.SystemGeneratedIdentifier;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.RetroCollection.Assign(useExport.RetroCollAdjustment);
    local.ReportNeeded.Flag = useExport.ReportNeeded.Flag;
  }

  private void UseFnCabReverseAllCshRcptDtls()
  {
    var useImport = new FnCabReverseAllCshRcptDtls.Import();
    var useExport = new FnCabReverseAllCshRcptDtls.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.InterfaceIndicator =
      entities.CashReceiptSourceType.InterfaceIndicator;
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    MoveCashReceiptDetail(entities.CashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.TypeOfChange.SelectChar = local.TypeOfChange.SelectChar;
    useImport.SuspendedStatus.SystemGeneratedIdentifier =
      local.SuspendedStatus.SystemGeneratedIdentifier;
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    MoveCollection(local.Collection, useImport.Collection);
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.RetroCollection.SystemGeneratedIdentifier;
    useImport.Max.Date = local.Max.Date;
    useImport.RefundedStatus.SystemGeneratedIdentifier =
      local.RefundedCashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.DistributedStatus.SystemGeneratedIdentifier =
      local.DistributedCashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.ReleasedStatus.SystemGeneratedIdentifier =
      local.Released.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.NoOfIncrementalUpdates.Count = local.UpdatesSinceLastCommit.Count;
    useImport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.ProgramStart.Timestamp = local.ProgramStart.Timestamp;
    useExport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useExport.NoOfIncrementalUpdates.Count = local.UpdatesSinceLastCommit.Count;
    useExport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;

    Call(FnCabReverseAllCshRcptDtls.Execute, useImport, useExport);

    local.NoCollectionsReversed.Count = useImport.NoCollectionsReversed.Count;
    local.UpdatesSinceLastCommit.Count = useImport.NoOfIncrementalUpdates.Count;
    local.NoCashRecptDtlUpdated.Count = useImport.NoCashRecptDtlUpdated.Count;
    local.NoCollectionsReversed.Count = useExport.NoCollectionsReversed.Count;
    local.UpdatesSinceLastCommit.Count = useExport.NoOfIncrementalUpdates.Count;
    local.NoCashRecptDtlUpdated.Count = useExport.NoCashRecptDtlUpdated.Count;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadCashReceiptCashReceiptTypeCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptTypeCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 5);
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus1()
  {
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", local.PpiParameter.ObligorPersonNumber ?? ""
          );
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.Released.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.DistributedCashReceiptDetailStatus.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.RefundedCashReceiptDetailStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 9);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 13);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 15);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus2()
  {
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", local.Restart.ObligorPersonNumber ?? "");
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.Released.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.DistributedCashReceiptDetailStatus.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.RefundedCashReceiptDetailStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 9);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 13);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 15);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
    }

    /// <summary>
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
    }

    /// <summary>
    /// A value of SuspendedStatus.
    /// </summary>
    [JsonPropertyName("suspendedStatus")]
    public CashReceiptDetailStatus SuspendedStatus
    {
      get => suspendedStatus ??= new();
      set => suspendedStatus = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    /// <summary>
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
    }

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
    /// A value of RetroCollection.
    /// </summary>
    [JsonPropertyName("retroCollection")]
    public CollectionAdjustmentReason RetroCollection
    {
      get => retroCollection ??= new();
      set => retroCollection = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of DistributedEabConvertNumeric.
    /// </summary>
    [JsonPropertyName("distributedEabConvertNumeric")]
    public EabConvertNumeric2 DistributedEabConvertNumeric
    {
      get => distributedEabConvertNumeric ??= new();
      set => distributedEabConvertNumeric = value;
    }

    /// <summary>
    /// A value of RefundedEabConvertNumeric.
    /// </summary>
    [JsonPropertyName("refundedEabConvertNumeric")]
    public EabConvertNumeric2 RefundedEabConvertNumeric
    {
      get => refundedEabConvertNumeric ??= new();
      set => refundedEabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Receipt.
    /// </summary>
    [JsonPropertyName("receipt")]
    public EabConvertNumeric2 Receipt
    {
      get => receipt ??= new();
      set => receipt = value;
    }

    /// <summary>
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CashReceiptDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CashReceiptDetail Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of RefundedCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("refundedCashReceiptDetailStatus")]
    public CashReceiptDetailStatus RefundedCashReceiptDetailStatus
    {
      get => refundedCashReceiptDetailStatus ??= new();
      set => refundedCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of DistributedCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("distributedCashReceiptDetailStatus")]
    public CashReceiptDetailStatus DistributedCashReceiptDetailStatus
    {
      get => distributedCashReceiptDetailStatus ??= new();
      set => distributedCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public CashReceiptDetailStatus Released
    {
      get => released ??= new();
      set => released = value;
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
    /// A value of UpdatesSinceLastCommit.
    /// </summary>
    [JsonPropertyName("updatesSinceLastCommit")]
    public Common UpdatesSinceLastCommit
    {
      get => updatesSinceLastCommit ??= new();
      set => updatesSinceLastCommit = value;
    }

    /// <summary>
    /// A value of NoCashRecptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRecptDtlUpdated")]
    public Common NoCashRecptDtlUpdated
    {
      get => noCashRecptDtlUpdated ??= new();
      set => noCashRecptDtlUpdated = value;
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
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public EabConvertNumeric2 Clear
    {
      get => clear ??= new();
      set => clear = value;
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of ProgramStart.
    /// </summary>
    [JsonPropertyName("programStart")]
    public DateWorkArea ProgramStart
    {
      get => programStart ??= new();
      set => programStart = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      ppiParameter = null;
      typeOfChange = null;
      suspendedStatus = null;
      reportNeeded = null;
      noCollectionsReversed = null;
      collection = null;
      retroCollection = null;
      max = null;
      distributedEabConvertNumeric = null;
      refundedEabConvertNumeric = null;
      receipt = null;
      formatDate = null;
      eabConvertNumeric = null;
      previous = null;
      restart = null;
      refundedCashReceiptDetailStatus = null;
      distributedCashReceiptDetailStatus = null;
      released = null;
      eabFileHandling = null;
      programProcessingInfo = null;
      programCheckpointRestart = null;
      passArea = null;
      updatesSinceLastCommit = null;
      noCashRecptDtlUpdated = null;
      clear = null;
      exitStateWorkArea = null;
      date = null;
      time = null;
      programStart = null;
    }

    private CashReceiptDetail ppiParameter;
    private Common typeOfChange;
    private CashReceiptDetailStatus suspendedStatus;
    private Common reportNeeded;
    private Common noCollectionsReversed;
    private Collection collection;
    private CollectionAdjustmentReason retroCollection;
    private DateWorkArea max;
    private EabConvertNumeric2 distributedEabConvertNumeric;
    private EabConvertNumeric2 refundedEabConvertNumeric;
    private EabConvertNumeric2 receipt;
    private TextWorkArea formatDate;
    private EabConvertNumeric2 eabConvertNumeric;
    private CashReceiptDetail previous;
    private CashReceiptDetail restart;
    private CashReceiptDetailStatus refundedCashReceiptDetailStatus;
    private CashReceiptDetailStatus distributedCashReceiptDetailStatus;
    private CashReceiptDetailStatus released;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private Common updatesSinceLastCommit;
    private Common noCashRecptDtlUpdated;
    private EabReportSend eabReportSend;
    private EabConvertNumeric2 clear;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea date;
    private TextWorkArea time;
    private DateWorkArea programStart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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

    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}
