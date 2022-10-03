// Program: FN_B710_SEND_KPC_COURT_NOTC_FILE, ID: 374441244, model: 746.
// Short name: SWEF710B
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
/// A program: FN_B710_SEND_KPC_COURT_NOTC_FILE.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB710SendKpcCourtNotcFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B710_SEND_KPC_COURT_NOTC_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB710SendKpcCourtNotcFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB710SendKpcCourtNotcFile.
  /// </summary>
  public FnB710SendKpcCourtNotcFile(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // -----------     -------------   ------------
    // 
    // ----------------------------------------------
    // 06/01/2000	M Ramirez	WR 172 Seg F	Initial Dev
    // 01/19/2001	M Ramirez	111841		Decmial in wrong place
    // 10/01/2001	V Madhira	PR# 120120      HIST record of KPC notification is 
    // showing as neg.
    // 04/03/2003	GVandy		PR 174972	Performance enhancements.
    // 04/10/2003	GVandy		PR 175589	Additional performance modifications.
    // 11/17/2006	Raj S		PR 295305 	Modified the Obligation read statement to
    // 						qualify with collection entity in order to fix
    // 						the records beging skipped from processing.
    // 02/13/2018	GVandy		CQ45790		The interface will now include refunds
    // 						for payments which originated at the KPC.
    // 11/19/2020	GVandy		CQ64697		Don't send FDSO payment information until
    // 						the payment is fully disbursed.
    // ----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // --------------------------------------------------------------------
    //          PR# 120120 : Reset the Local_temp values.
    //                                                               
    // ---Vithal (10/01/2001)
    // --------------------------------------------------------------------
    local.TempEabConvertNumeric.Assign(local.TempNullEabConvertNumeric);
    local.TempBatchTimestampWorkArea.
      Assign(local.TempNullBatchTimestampWorkArea);
    UseFnB710Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // --------------------------------------------------------
      // No message will be given in Error Report because program
      // failed before the Error Report was created.
      // Message will be given via the exitstate
      // -----------------------------------------------------------
      return;
    }

    // ----------------------------------------------------------------
    // Records are processed in groups based on commit frequency
    // obtained from entity Program Checkpoint Restart.
    // Commits are performed at the end of each group.
    // ----------------------------------------------------------------
    local.CheckpointRead.Count = 0;
    local.EabFileHandling.Action = "WRITE";
    local.Collection.LastUpdatedBy = local.ProgramProcessingInfo.Name;
    local.Collection.LastUpdatedTmst = local.Current.Timestamp;
    local.TempEabConvertNumeric.SendNonSuppressPos = 3;

    // --------------------------------------------------------------------
    // The Read Each below is set to DISTINCT.
    // It will only pick up a distinct group of cash_receipt_detail,
    // legal_action and cse_person_acct that have some collection that
    // requires notice sent to the Kansas Pay Center for it.
    // --------------------------------------------------------------------
    foreach(var item in ReadCashReceiptDetailCsePersonAccountLegalAction())
    {
      // --------------------------------------------------------------------
      // Checkpoint is done against number of reads. Do not change.
      // --------------------------------------------------------------------
      ++local.CheckpointRead.Count;
      ++local.LcontrolTotalRead.Count;

      // 11/19/2020  GVandy  CQ64697  Don't send FDSO payment information until 
      // the payment is fully disbursed.
      if (ReadCollectionType())
      {
        if (Equal(entities.CollectionType.Code, "F"))
        {
          if (ReadCollection2())
          {
            // --There is a disbursable collection from the current cash receipt
            // detail that has not
            //   disbursed.  This likely won't happen in production but it did 
            // occur in acceptance
            //   testing because jobs were not run in the order they are in 
            // production.  This
            //   scenario could happen in production if for some reason 
            // disbursement was not run
            //   before the court notice job. Skip the cash receipt detail until
            // the collections
            //   disburse.
            continue;
          }
          else
          {
            // --Continue
          }

          if (ReadCollection1())
          {
            // -- The FDSO payment is not fully disbursed (i.e. there is a 
            // disbursement suppression
            //    for one or more of the collections that resulted from the FDSO
            // payment).
            continue;
          }
          else
          {
            // --Continue
          }
        }
      }

      // --------------------------------------------------------------------
      //          PR# 120120 : Reset the Local_temp values.
      //                                                               
      // ---Vithal (10/01/2001)
      // --------------------------------------------------------------------
      local.TempEabConvertNumeric.Assign(local.TempNullEabConvertNumeric);
      local.TempBatchTimestampWorkArea.Assign(
        local.TempNullBatchTimestampWorkArea);
      local.TempEabConvertNumeric.SendNonSuppressPos = 3;

      if (AsChar(local.DebugOn.Flag) == 'Y')
      {
        if (ReadCsePerson3())
        {
          local.EabReportSend.RptDetail = "DEBUG:  Obligor = " + entities
            .CsePerson.Number + ";  LA Standard No = " + entities
            .StandardNumber.StandardNumber;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }
      }

      // mjr
      // -------------------------------------------------------
      // 12/02/1999
      // Added check for collection start date
      // ---------------------------------------------------------------------
      if (Lt(local.Null1.Date, local.CollectionStart.Date))
      {
        if (Lt(entities.CashReceiptDetail.CollectionDate,
          local.CollectionStart.Date))
        {
          foreach(var item1 in ReadCollection3())
          {
            goto ReadEach1;
          }
        }
      }

      // mjr
      // -----------------------------------------------------
      // Determine if a notice needs to be sent.
      // --------------------------------------------------------
      local.SendNotice.Flag = "N";
      local.Total.Amount = 0;

      // -- 04/03/2003  GVandy  Broke apart joins causing performance problems.
      // 		Original logic is commented out following this read each.
      foreach(var item1 in ReadCollection4())
      {
        if (AsChar(entities.Collection.CourtNoticeReqInd) == 'Y' && Equal
          (entities.Collection.CourtNoticeAdjProcessDate, local.Null1.Date))
        {
          // -- Continue
        }
        else
        {
          continue;
        }

        // mlb - PR128400 - 04/23/2004 - This PR was written so that the 'Fee 
        // and Recovery Collections'
        // do not get sent to the KPC. The original read statement is commented 
        // out, and the new read
        // statement follows, and does as requested.
        // ----------------------------------------------------------------------------------------------------
        // Who: Raj S   When: 11/17/2006  Ref: PR#295305 qualified the following
        // read with collection entity
        // in order to get the right obligation record for the selected 
        // collection.
        // ----------------------------------------------------------------------------------------------------
        if (!ReadObligationObligationType())
        {
          continue;
        }

        // mlb - end
        if (ReadLegalAction2())
        {
          if (!Equal(entities.LegalAction.StandardNumber,
            entities.StandardNumber.StandardNumber))
          {
            // -- The standard number is not the same as in the outer read each.
            // Skip this collection.
            continue;
          }
        }
        else
        {
          // -- This obligation was not order by a legal action.  Skip this 
          // collection.
          continue;
        }

        if (AsChar(entities.Collection.AdjustedInd) == 'Y')
        {
          if (Lt(local.Null1.Date, entities.Collection.CourtNoticeProcessedDate))
            
          {
            // mjr
            // --------------------------------------------------------------
            // A payment has been reported to the KPC, an adjustment has been
            // made to that payment, and no notice has been given to the KPC
            // about the adjustment.
            // -----------------------------------------------------------------
            local.Collection.CourtNoticeProcessedDate =
              entities.Collection.CourtNoticeProcessedDate;
            local.Collection.CourtNoticeAdjProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
          }
          else
          {
            // mjr
            // --------------------------------------------------------------
            // A payment has not been reported to the KPC, and an adjustment
            // has been made to that payment.
            // -----------------------------------------------------------------
            // mjr
            // -------------------------------------------------------------------
            // Collections which are adjusted before they are processed by this 
            // batch
            // should not calculate into the total amount
            // ----------------------------------------------------------------------
            try
            {
              UpdateCollection2();

              continue;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_COLLECTION_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_COLLECTION_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
                .ExitStateWorkArea.Message;

              goto ReadEach2;
            }
          }
        }
        else if (Lt(local.Null1.Date,
          entities.Collection.CourtNoticeProcessedDate))
        {
          // mjr
          // --------------------------------------------------------------
          // A payment has been reported to the KPC, and no adjustment
          // has been made to that payment.
          // -----------------------------------------------------------------
          continue;
        }
        else
        {
          // mjr
          // --------------------------------------------------------------
          // A payment has not been reported to the KPC, and no adjustment
          // has been made to that payment.
          // -----------------------------------------------------------------
          local.Collection.CourtNoticeProcessedDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.Collection.CourtNoticeAdjProcessDate = local.Null1.Date;
        }

        // mjr
        // --------------------------------------------------------------
        // It has been determined to send a notice.  Update this collection
        // since it will be included in the notice totals.
        // -----------------------------------------------------------------
        try
        {
          UpdateCollection1();
          local.SendNotice.Flag = "Y";

          if (AsChar(entities.Collection.AdjustedInd) == 'Y')
          {
            local.Total.Amount -= entities.Collection.Amount;
          }
          else
          {
            local.Total.Amount += entities.Collection.Amount;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_COLLECTION_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_COLLECTION_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
            .ExitStateWorkArea.Message;

          goto ReadEach2;
        }
      }

      if (AsChar(local.SendNotice.Flag) == 'N')
      {
        continue;
      }

      if (local.Total.Amount == 0)
      {
        // mjr
        // -----------------------------------------------------------------
        // Since collections balance, nothing needs to be reported to the KPC.
        // (To them, nothing has changed.)
        // --------------------------------------------------------------------
        continue;
      }

      // mjr
      // -----------------------------------------------------------------
      // Gather identifiers for trigger record
      // --------------------------------------------------------------------
      local.Trigger.DenormNumeric1 = 0;

      if (ReadLegalAction3())
      {
        local.Trigger.DenormNumeric1 = entities.LegalAction.Identifier;
      }

      if (local.Trigger.DenormNumeric1.GetValueOrDefault() == 0)
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Legal_Action missing for Cash_Receipt_Detail.";

        break;
      }

      local.Trigger.DenormNumeric2 =
        entities.CashReceiptDetail.SequentialIdentifier;

      if (!ReadCashReceipt())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cash_Receipt missing for Cash_Receipt_Detail.";

        break;
      }

      if (ReadCashReceiptEvent())
      {
        local.Trigger.DenormNumeric3 =
          entities.CashReceiptEvent.SystemGeneratedIdentifier;
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cash_Receipt_Event missing for Cash_Receipt.";

        break;
      }

      if (ReadCsePerson2())
      {
        local.Trigger.DenormText1 = entities.CsePerson.Number;
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cse_person missing for cse_person_account.";

        break;
      }

      if (ReadCashReceiptSourceType())
      {
        local.Trigger.DenormText2 =
          NumberToString(entities.CashReceiptSourceType.
            SystemGeneratedIdentifier, 13, 3);
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cash_Receipt_Source_Type missing for Cash_Receipt_Event.";

        break;
      }

      if (ReadCashReceiptType())
      {
        local.Trigger.DenormText3 =
          NumberToString(entities.CashReceiptType.SystemGeneratedIdentifier, 13,
          3);
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cash_Receipt_Type missing for Cash_Receipt.";

        break;
      }

      local.Count.Count = 0;
      local.Trigger.Identifier = (int)(Microsecond(Now()) + (long)Now().Day * 1000000
        );

      do
      {
        local.Trigger.Identifier += local.Count.Count;

        try
        {
          CreateTrigger1();

          break;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.Count.Count;

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_TRIGGER_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      while(local.Count.Count <= 100);

      if (local.Count.Count > 100)
      {
        ExitState = "SP0000_TRIGGER_AE";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
          .ExitStateWorkArea.Message;

        break;
      }

      // mjr
      // -------------------------------------------------------------------
      // Log a record to history
      // ----------------------------------------------------------------------
      if (AsChar(local.CreateHistory.Flag) == 'Y')
      {
        local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        local.Infrastructure.DenormNumeric12 =
          local.Trigger.DenormNumeric1.GetValueOrDefault();
        local.TempBatchTimestampWorkArea.TextDate =
          NumberToString(DateToInt(entities.CashReceiptDetail.CollectionDate), 8);
          
        local.TempBatchTimestampWorkArea.TextDateYyyy =
          Substring(local.TempBatchTimestampWorkArea.TextDate, 1, 4);
        local.TempBatchTimestampWorkArea.TextDateMm =
          Substring(local.TempBatchTimestampWorkArea.TextDate, 5, 2);
        local.TempBatchTimestampWorkArea.TestDateDd =
          Substring(local.TempBatchTimestampWorkArea.TextDate, 7, 2);

        // mjr
        // -----------------------------------------
        // 01/19/2001
        // PR# 111841 - Decimal in wrong place
        // ------------------------------------------------------
        local.TempEabConvertNumeric.SendAmount =
          NumberToString((long)(local.Total.Amount * 100), 15);

        if (local.Total.Amount < 0)
        {
          local.TempEabConvertNumeric.SendSign = "-";
        }

        UseEabConvertNumeric1();
        local.TempEabConvertNumeric.ReturnCurrencySigned =
          Substring(local.TempEabConvertNumeric.ReturnCurrencySigned,
          Verify(local.TempEabConvertNumeric.ReturnCurrencySigned, " "), 22 -
          Verify(local.TempEabConvertNumeric.ReturnCurrencySigned, " "));
        local.Infrastructure.Detail = "Notice sent to KPC; Collection Dt = " + local
          .TempBatchTimestampWorkArea.TextDateMm + local
          .TempBatchTimestampWorkArea.TestDateDd + local
          .TempBatchTimestampWorkArea.TextDateYyyy + ", Amt = " + TrimEnd
          (local.TempEabConvertNumeric.ReturnCurrencySigned);
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.LcontrolTotalWarned.Count;
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = "WARNING:  Obligor = " + entities
            .CsePerson.Number + "; LA Standard No = " + entities
            .StandardNumber.StandardNumber + "; Error = " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      ++local.LcontrolTotalProcessed.Count;

      if (AsChar(local.DebugOn.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "DEBUG:       Record sent to KPC";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      // -----------------------------------------------------------------------
      // Commit processing
      // -----------------------------------------------------------------------
      if (local.CheckpointRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to commit database.";

          break;
        }

        // -------------------------------------------------------------
        // Reset checkpoint counter
        // -------------------------------------------------------------
        local.CheckpointRead.Count = 0;
      }

      // *****End of READ EACH
ReadEach1:
      ;
    }

ReadEach2:

    // 02/13/2018  GVandy  CQ45790  The interface will now include refunds for 
    // payments
    // which originated at the KPC.
    if (IsEmpty(local.EabReportSend.RptDetail))
    {
      foreach(var item in ReadReceiptRefundCashReceiptDetail())
      {
        if (ReadPaymentRequestPaymentStatusHistoryPaymentStatus())
        {
          switch(TrimEnd(entities.PaymentStatus.Code))
          {
            case "KPC":
              break;
            case "PAID":
              break;
            default:
              continue;
          }
        }
        else
        {
          continue;
        }

        ++local.CheckpointRead.Count;
        ++local.LcontrolTotalRefundRead.Count;

        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "DEBUG:  REFUND Obligor = " + entities
            .CashReceiptDetail.ObligorPersonNumber + ";  Standard No = " + entities
            .CashReceiptDetail.CourtOrderNumber;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }

        try
        {
          UpdateReceiptRefund();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "RECEIPT_REFUND_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "RECEIPT_REFUND_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
            .ExitStateWorkArea.Message;

          goto Test;
        }

        do
        {
          try
          {
            CreateTrigger2();

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++local.Count.Count;

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "SP0000_TRIGGER_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        while(local.Count.Count <= 100);

        if (local.Count.Count > 100)
        {
          ExitState = "SP0000_TRIGGER_AE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
            .ExitStateWorkArea.Message;

          goto Test;
        }

        // -- Log a record to history
        if (AsChar(local.CreateHistory.Flag) == 'Y')
        {
          local.Infrastructure.ReasonCode = "KPCREFUNDSENT";

          if (!IsEmpty(entities.CashReceiptDetail.ObligorPersonNumber))
          {
            local.Infrastructure.CsePersonNumber =
              entities.CashReceiptDetail.ObligorPersonNumber;
          }
          else if (!IsEmpty(entities.PaymentRequest.CsePersonNumber))
          {
            local.Infrastructure.CsePersonNumber =
              entities.PaymentRequest.CsePersonNumber;
          }
          else if (ReadCsePerson1())
          {
            local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          }
          else
          {
            local.Infrastructure.CsePersonNumber = "";
          }

          local.Infrastructure.DenormTimestamp =
            entities.ReceiptRefund.CreatedTimestamp;
          local.Infrastructure.DenormNumeric12 = 0;

          if (!IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
          {
            if (ReadLegalAction1())
            {
              local.Infrastructure.DenormNumeric12 =
                entities.LegalAction.Identifier;
            }
          }

          local.TempBatchTimestampWorkArea.TextDate =
            NumberToString(DateToInt(entities.ReceiptRefund.RequestDate), 8);
          local.TempBatchTimestampWorkArea.TextDateYyyy =
            Substring(local.TempBatchTimestampWorkArea.TextDate, 1, 4);
          local.TempBatchTimestampWorkArea.TextDateMm =
            Substring(local.TempBatchTimestampWorkArea.TextDate, 5, 2);
          local.TempBatchTimestampWorkArea.TestDateDd =
            Substring(local.TempBatchTimestampWorkArea.TextDate, 7, 2);
          local.TempEabConvertNumeric.SendAmount =
            NumberToString((long)entities.ReceiptRefund.Amount, 15);
          local.TempEabConvertNumeric.SendAmount =
            NumberToString((long)(entities.ReceiptRefund.Amount * 100), 15);

          if (entities.ReceiptRefund.Amount < 0)
          {
            local.TempEabConvertNumeric.SendSign = "-";
          }

          UseEabConvertNumeric1();
          local.TempEabConvertNumeric.ReturnCurrencySigned =
            Substring(local.TempEabConvertNumeric.ReturnCurrencySigned,
            Verify(local.TempEabConvertNumeric.ReturnCurrencySigned, " "), 22 -
            Verify(local.TempEabConvertNumeric.ReturnCurrencySigned, " "));
          local.Infrastructure.Detail = "Notice sent to KPC; Refund Dt = " + local
            .TempBatchTimestampWorkArea.TextDateMm + local
            .TempBatchTimestampWorkArea.TestDateDd + local
            .TempBatchTimestampWorkArea.TextDateYyyy + ", Refund Amt = " + TrimEnd
            (local.TempEabConvertNumeric.ReturnCurrencySigned);
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.LcontrolTotalWarned.Count;
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "WARNING:  REFUND Obligor = " + entities
              .CashReceiptDetail.ObligorPersonNumber + "; Standard No = " + entities
              .CashReceiptDetail.CourtOrderNumber + "; Error = " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }
        }

        ++local.LcontrolTotalRefundProcessed.Count;

        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "DEBUG:       REFUND Record sent to KPC";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }

        // -----------------------------------------------------------------------
        // Commit processing
        // -----------------------------------------------------------------------
        if (local.CheckpointRead.Count >= local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail =
              "SYSTEM ERROR:  Unable to commit database.";

            return;
          }

          // -------------------------------------------------------------
          // Reset checkpoint counter
          // -------------------------------------------------------------
          local.CheckpointRead.Count = 0;
        }

        // *****End of READ EACH
      }
    }

Test:

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      // -----------------------------------------------------------
      // Ending as an ABEND
      // -----------------------------------------------------------
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      if (local.CheckpointRead.Count < local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -----------------------------------------------------------
        // ABEND occurred at a time other than committing; commit now.
        // -----------------------------------------------------------
        UseExtToDoACommit();
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      // -----------------------------------------------------------
      // Successful end for this program
      // -----------------------------------------------------------
    }

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseFnB710WriteControlsAndClose();
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CsePersonNumber = source.CsePersonNumber;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveTrigger(Trigger source, Trigger target)
  {
    target.Type1 = source.Type1;
    target.Action = source.Action;
    target.Status = source.Status;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.UpdatedTimestamp = source.UpdatedTimestamp;
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

    useImport.EabConvertNumeric.Assign(local.TempEabConvertNumeric);
    useExport.EabConvertNumeric.ReturnCurrencySigned =
      local.TempEabConvertNumeric.ReturnCurrencySigned;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.TempEabConvertNumeric.ReturnCurrencySigned =
      useExport.EabConvertNumeric.ReturnCurrencySigned;
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

  private void UseFnB710Housekeeping()
  {
    var useImport = new FnB710Housekeeping.Import();
    var useExport = new FnB710Housekeeping.Export();

    Call(FnB710Housekeeping.Execute, useImport, useExport);

    MoveTrigger(useExport.Trigger, local.Trigger);
    MoveInfrastructure2(useExport.Infrastructure, local.Infrastructure);
    local.CreateHistory.Flag = useExport.CreateHistory.Flag;
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.CollectionStart.Date = useExport.CollectionStart.Date;
    local.Current.Timestamp = useExport.Current.Timestamp;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseFnB710WriteControlsAndClose()
  {
    var useImport = new FnB710WriteControlsAndClose.Import();
    var useExport = new FnB710WriteControlsAndClose.Export();

    useImport.Warning.Count = local.LcontrolTotalWarned.Count;
    useImport.DataError.Count = local.LcontrolTotalErred.Count;
    useImport.Processed.Count = local.LcontrolTotalProcessed.Count;
    useImport.Read.Count = local.LcontrolTotalRead.Count;
    useImport.RefundProcessed.Count = local.LcontrolTotalRefundProcessed.Count;
    useImport.RefundRead.Count = local.LcontrolTotalRefundRead.Count;

    Call(FnB710WriteControlsAndClose.Execute, useImport, useExport);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private void CreateTrigger1()
  {
    var identifier = local.Trigger.Identifier;
    var type1 = local.Trigger.Type1;
    var action1 = local.Trigger.Action ?? "";
    var status = local.Trigger.Status ?? "";
    var denormNumeric1 = local.Trigger.DenormNumeric1.GetValueOrDefault();
    var denormNumeric2 = local.Trigger.DenormNumeric2.GetValueOrDefault();
    var denormNumeric3 = local.Trigger.DenormNumeric3.GetValueOrDefault();
    var denormText1 = local.Trigger.DenormText1 ?? "";
    var denormText2 = local.Trigger.DenormText2 ?? "";
    var denormText3 = local.Trigger.DenormText3 ?? "";
    var createdBy = local.Trigger.CreatedBy ?? "";
    var createdTimestamp = local.Trigger.CreatedTimestamp;
    var lastUpdatedBy = local.Trigger.LastUpdatedBy ?? "";
    var updatedTimestamp = local.Trigger.UpdatedTimestamp;

    entities.Trigger.Populated = false;
    Update("CreateTrigger1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "action0", action1);
        db.SetNullableString(command, "status", status);
        db.SetNullableInt32(command, "denormNumeric1", denormNumeric1);
        db.SetNullableInt32(command, "denormNumeric2", denormNumeric2);
        db.SetNullableInt32(command, "denormNumeric3", denormNumeric3);
        db.SetNullableString(command, "denormText1", denormText1);
        db.SetNullableString(command, "denormText2", denormText2);
        db.SetNullableString(command, "denormText3", denormText3);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "updatedTimestamp", updatedTimestamp);
        db.SetNullableDateTime(command, "denormTimestamp", null);
      });

    entities.Trigger.Identifier = identifier;
    entities.Trigger.Type1 = type1;
    entities.Trigger.Action = action1;
    entities.Trigger.Status = status;
    entities.Trigger.DenormNumeric1 = denormNumeric1;
    entities.Trigger.DenormNumeric2 = denormNumeric2;
    entities.Trigger.DenormNumeric3 = denormNumeric3;
    entities.Trigger.DenormText1 = denormText1;
    entities.Trigger.DenormText2 = denormText2;
    entities.Trigger.DenormText3 = denormText3;
    entities.Trigger.CreatedBy = createdBy;
    entities.Trigger.CreatedTimestamp = createdTimestamp;
    entities.Trigger.LastUpdatedBy = lastUpdatedBy;
    entities.Trigger.UpdatedTimestamp = updatedTimestamp;
    entities.Trigger.DenormTimestamp = null;
    entities.Trigger.Populated = true;
  }

  private void CreateTrigger2()
  {
    var identifier = UseGenerate9DigitRandomNumber();
    var type1 = "KPC REF";
    var denormText1 = entities.CashReceiptDetail.ObligorPersonNumber;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = local.Trigger.LastUpdatedBy ?? "";
    var updatedTimestamp = local.Trigger.UpdatedTimestamp;
    var denormTimestamp = entities.ReceiptRefund.CreatedTimestamp;

    entities.Trigger.Populated = false;
    Update("CreateTrigger2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "action0", "");
        db.SetNullableString(command, "status", "");
        db.SetNullableInt32(command, "denormNumeric1", 0);
        db.SetNullableInt32(command, "denormNumeric2", 0);
        db.SetNullableInt32(command, "denormNumeric3", 0);
        db.SetNullableString(command, "denormText1", denormText1);
        db.SetNullableString(command, "denormText2", "");
        db.SetNullableString(command, "denormText3", "");
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "updatedTimestamp", updatedTimestamp);
        db.SetNullableDateTime(command, "denormTimestamp", denormTimestamp);
      });

    entities.Trigger.Identifier = identifier;
    entities.Trigger.Type1 = type1;
    entities.Trigger.Action = "";
    entities.Trigger.Status = "";
    entities.Trigger.DenormNumeric1 = 0;
    entities.Trigger.DenormNumeric2 = 0;
    entities.Trigger.DenormNumeric3 = 0;
    entities.Trigger.DenormText1 = denormText1;
    entities.Trigger.DenormText2 = "";
    entities.Trigger.DenormText3 = "";
    entities.Trigger.CreatedBy = createdBy;
    entities.Trigger.CreatedTimestamp = createdTimestamp;
    entities.Trigger.LastUpdatedBy = lastUpdatedBy;
    entities.Trigger.UpdatedTimestamp = updatedTimestamp;
    entities.Trigger.DenormTimestamp = denormTimestamp;
    entities.Trigger.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
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
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCsePersonAccountLegalAction()
  {
    entities.StandardNumber.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCsePersonAccountLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "crtNoticeProcDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 9);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 10);
        entities.StandardNumber.Identifier = db.GetInt32(reader, 11);
        entities.StandardNumber.StandardNumber =
          db.GetNullableString(reader, 12);
        entities.StandardNumber.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);

        return true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 17);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 18);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Undisbursed.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetNullableDate(
          command, "disbDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Undisbursed.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Undisbursed.DisbursementDt = db.GetNullableDate(reader, 1);
        entities.Undisbursed.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Undisbursed.DisbursementAdjProcessDate = db.GetDate(reader, 3);
        entities.Undisbursed.CrtType = db.GetInt32(reader, 4);
        entities.Undisbursed.CstId = db.GetInt32(reader, 5);
        entities.Undisbursed.CrvId = db.GetInt32(reader, 6);
        entities.Undisbursed.CrdId = db.GetInt32(reader, 7);
        entities.Undisbursed.ObgId = db.GetInt32(reader, 8);
        entities.Undisbursed.CspNumber = db.GetString(reader, 9);
        entities.Undisbursed.CpaType = db.GetString(reader, 10);
        entities.Undisbursed.OtrId = db.GetInt32(reader, 11);
        entities.Undisbursed.OtrType = db.GetString(reader, 12);
        entities.Undisbursed.OtyId = db.GetInt32(reader, 13);
        entities.Undisbursed.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 14);
        entities.Undisbursed.CourtNoticeReqInd =
          db.GetNullableString(reader, 15);
        entities.Undisbursed.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 16);
        entities.Undisbursed.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Undisbursed.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Undisbursed.CpaType);
        CheckValid<Collection>("OtrType", entities.Undisbursed.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Undisbursed.DisbursementProcessingNeedInd);
      });
  }

  private IEnumerable<bool> ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDate(
          command, "crtNtcAdjPrcDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 17);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 18);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection4()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection4",
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
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 17);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 18);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ReceiptRefund.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.StandardNumber.StandardNumber ?? "");
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cpaType1", entities.CsePersonAccount.Type1);
        db.
          SetString(command, "cspNumber1", entities.CsePersonAccount.CspNumber);
          
        db.SetInt32(command, "dtyGeneratedId", entities.Collection.OtyId);
        db.SetInt32(command, "obId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber2", entities.Collection.CspNumber);
        db.SetString(command, "cpaType2", entities.Collection.CpaType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadPaymentRequestPaymentStatusHistoryPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;
    entities.PaymentStatusHistory.Populated = false;
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequestPaymentStatusHistoryPaymentStatus",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 1);
        entities.PaymentRequest.Type1 = db.GetString(reader, 2);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 3);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 5);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 7);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.PaymentStatus.Code = db.GetString(reader, 9);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private IEnumerable<bool> ReadReceiptRefundCashReceiptDetail()
  {
    entities.ReceiptRefund.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadReceiptRefundCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "kpcNoticeProcDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 3);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 4);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 5);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 6);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 7);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 10);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 11);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 11);
        entities.ReceiptRefund.KpcNoticeReqInd =
          db.GetNullableString(reader, 12);
        entities.ReceiptRefund.KpcNoticeProcessedDate =
          db.GetNullableDate(reader, 13);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 15);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 18);
        entities.ReceiptRefund.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private void UpdateCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = local.Collection.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.Collection.LastUpdatedTmst;
    var courtNoticeProcessedDate = local.Collection.CourtNoticeProcessedDate;
    var courtNoticeAdjProcessDate = local.Collection.CourtNoticeAdjProcessDate;

    entities.Collection.Populated = false;
    Update("UpdateCollection1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetNullableDate(command, "crtNoticeProcDt", courtNoticeProcessedDate);
          
        db.SetDate(command, "crtNtcAdjPrcDt", courtNoticeAdjProcessDate);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.CourtNoticeProcessedDate = courtNoticeProcessedDate;
    entities.Collection.CourtNoticeAdjProcessDate = courtNoticeAdjProcessDate;
    entities.Collection.Populated = true;
  }

  private void UpdateCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = local.Collection.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.Collection.LastUpdatedTmst;
    var courtNoticeProcessedDate = local.ProgramProcessingInfo.ProcessDate;

    entities.Collection.Populated = false;
    Update("UpdateCollection2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetNullableDate(command, "crtNoticeProcDt", courtNoticeProcessedDate);
          
        db.SetDate(command, "crtNtcAdjPrcDt", courtNoticeProcessedDate);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.CourtNoticeProcessedDate = courtNoticeProcessedDate;
    entities.Collection.CourtNoticeAdjProcessDate = courtNoticeProcessedDate;
    entities.Collection.Populated = true;
  }

  private void UpdateReceiptRefund()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var kpcNoticeProcessedDate = local.ProgramProcessingInfo.ProcessDate;

    entities.ReceiptRefund.Populated = false;
    Update("UpdateReceiptRefund",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableDate(command, "kpcNoticeProcDt", kpcNoticeProcessedDate);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ReceiptRefund.LastUpdatedBy = lastUpdatedBy;
    entities.ReceiptRefund.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ReceiptRefund.KpcNoticeProcessedDate = kpcNoticeProcessedDate;
    entities.ReceiptRefund.Populated = true;
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
    /// A value of LcontrolTotalRefundProcessed.
    /// </summary>
    [JsonPropertyName("lcontrolTotalRefundProcessed")]
    public Common LcontrolTotalRefundProcessed
    {
      get => lcontrolTotalRefundProcessed ??= new();
      set => lcontrolTotalRefundProcessed = value;
    }

    /// <summary>
    /// A value of LcontrolTotalRefundRead.
    /// </summary>
    [JsonPropertyName("lcontrolTotalRefundRead")]
    public Common LcontrolTotalRefundRead
    {
      get => lcontrolTotalRefundRead ??= new();
      set => lcontrolTotalRefundRead = value;
    }

    /// <summary>
    /// A value of Refund.
    /// </summary>
    [JsonPropertyName("refund")]
    public Trigger Refund
    {
      get => refund ??= new();
      set => refund = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    /// <summary>
    /// A value of TempEabConvertNumeric.
    /// </summary>
    [JsonPropertyName("tempEabConvertNumeric")]
    public EabConvertNumeric2 TempEabConvertNumeric
    {
      get => tempEabConvertNumeric ??= new();
      set => tempEabConvertNumeric = value;
    }

    /// <summary>
    /// A value of TempBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("tempBatchTimestampWorkArea")]
    public BatchTimestampWorkArea TempBatchTimestampWorkArea
    {
      get => tempBatchTimestampWorkArea ??= new();
      set => tempBatchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of LcontrolTotalWarned.
    /// </summary>
    [JsonPropertyName("lcontrolTotalWarned")]
    public Common LcontrolTotalWarned
    {
      get => lcontrolTotalWarned ??= new();
      set => lcontrolTotalWarned = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of CreateHistory.
    /// </summary>
    [JsonPropertyName("createHistory")]
    public Common CreateHistory
    {
      get => createHistory ??= new();
      set => createHistory = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of FnKpcCourtNotc.
    /// </summary>
    [JsonPropertyName("fnKpcCourtNotc")]
    public FnKpcCourtNotc FnKpcCourtNotc
    {
      get => fnKpcCourtNotc ??= new();
      set => fnKpcCourtNotc = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of CollectionStart.
    /// </summary>
    [JsonPropertyName("collectionStart")]
    public DateWorkArea CollectionStart
    {
      get => collectionStart ??= new();
      set => collectionStart = value;
    }

    /// <summary>
    /// A value of SendNotice.
    /// </summary>
    [JsonPropertyName("sendNotice")]
    public Common SendNotice
    {
      get => sendNotice ??= new();
      set => sendNotice = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Collection Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of LcontrolTotalErred.
    /// </summary>
    [JsonPropertyName("lcontrolTotalErred")]
    public Common LcontrolTotalErred
    {
      get => lcontrolTotalErred ??= new();
      set => lcontrolTotalErred = value;
    }

    /// <summary>
    /// A value of LcontrolTotalProcessed.
    /// </summary>
    [JsonPropertyName("lcontrolTotalProcessed")]
    public Common LcontrolTotalProcessed
    {
      get => lcontrolTotalProcessed ??= new();
      set => lcontrolTotalProcessed = value;
    }

    /// <summary>
    /// A value of LcontrolTotalRead.
    /// </summary>
    [JsonPropertyName("lcontrolTotalRead")]
    public Common LcontrolTotalRead
    {
      get => lcontrolTotalRead ??= new();
      set => lcontrolTotalRead = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of CheckpointRead.
    /// </summary>
    [JsonPropertyName("checkpointRead")]
    public Common CheckpointRead
    {
      get => checkpointRead ??= new();
      set => checkpointRead = value;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of TempNullBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("tempNullBatchTimestampWorkArea")]
    public BatchTimestampWorkArea TempNullBatchTimestampWorkArea
    {
      get => tempNullBatchTimestampWorkArea ??= new();
      set => tempNullBatchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of TempNullEabConvertNumeric.
    /// </summary>
    [JsonPropertyName("tempNullEabConvertNumeric")]
    public EabConvertNumeric2 TempNullEabConvertNumeric
    {
      get => tempNullEabConvertNumeric ??= new();
      set => tempNullEabConvertNumeric = value;
    }

    private Common lcontrolTotalRefundProcessed;
    private Common lcontrolTotalRefundRead;
    private Trigger refund;
    private Trigger trigger;
    private EabConvertNumeric2 tempEabConvertNumeric;
    private BatchTimestampWorkArea tempBatchTimestampWorkArea;
    private Common lcontrolTotalWarned;
    private Infrastructure infrastructure;
    private Common createHistory;
    private CollectionType collectionType;
    private AbendData abendData;
    private FnKpcCourtNotc fnKpcCourtNotc;
    private Common debugOn;
    private DateWorkArea collectionStart;
    private Common sendNotice;
    private Collection collection;
    private DateWorkArea current;
    private Collection total;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea null1;
    private Common lcontrolTotalErred;
    private Common lcontrolTotalProcessed;
    private Common lcontrolTotalRead;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common checkpointRead;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common count;
    private BatchTimestampWorkArea tempNullBatchTimestampWorkArea;
    private EabConvertNumeric2 tempNullEabConvertNumeric;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Undisbursed.
    /// </summary>
    [JsonPropertyName("undisbursed")]
    public Collection Undisbursed
    {
      get => undisbursed ??= new();
      set => undisbursed = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of StandardNumber.
    /// </summary>
    [JsonPropertyName("standardNumber")]
    public LegalAction StandardNumber
    {
      get => standardNumber ??= new();
      set => standardNumber = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private Collection undisbursed;
    private DisbursementStatus disbursementStatus;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction debit;
    private DisbursementTransaction credit;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CollectionType collectionType;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private LegalAction standardNumber;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private CashReceiptDetail cashReceiptDetail;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private Collection collection;
    private Trigger trigger;
  }
#endregion
}
