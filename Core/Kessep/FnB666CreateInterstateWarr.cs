// Program: FN_B666_CREATE_INTERSTATE_WARR, ID: 372743504, model: 746.
// Short name: SWEF666B
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
/// A program: FN_B666_CREATE_INTERSTATE_WARR.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process creates payment requests for interstate disbursements.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB666CreateInterstateWarr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B666_CREATE_INTERSTATE_WARR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB666CreateInterstateWarr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB666CreateInterstateWarr.
  /// </summary>
  public FnB666CreateInterstateWarr(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	DEscription
    // 022698	govind		Initial code. Cloned from B641
    // 05dec98	lxj		'Recapture' process removed.
    // 05/04/99  Fangman         Added EFT logic & various corrections.
    // 7/8/99    Fangman         PR 84663  Changed logic to get the last EFT 
    // Number and EFT Trace Number from a control table instead of reading the
    // EFT table.
    // 1/8/00    Fangman         PR 84663  Fixed rounding error.
    // 2/7/00    Fangman         PR 87059  Added error checking for interstate 
    // disbursements without an association to an interstate request.
    // 4/6/00    Fangman         PR 86768  Correct problem with commit frequency
    // & stop display of interstate break info.
    // 4/12/00  Fangman         PR 93146  Changed code cancel warrants and deny 
    // recoveries if the amount is less than one dollar.
    // 12/8/00  Kalpesh         PR 105240 Net interstate warrants across 
    // different programs.
    // 2/12/01  Fangman  WR 284  Don't issue interstate EFTs
    // ---------------------------------------------------------------------------------------
    // ---------------------------------------------
    // This batch procedure generates the warrants / potential
    // recoveries for Interstate disbursements It 'nets' the
    // disbursement debits and creates either a warrant or a
    // potential recovery based on whether the net amount is
    // positive or negative. The netting is done by the Interstate
    // Request. The Recapture and Cost Recovery Refund
    // processes in this procedure will work as expected only if
    // the Obligee(AR) is the same for all the debts associated with
    // the Interstate Request. Other than these two processes the
    // batch procedure will work correctly even if the ARs are
    // different for different debts associated with the Interstate
    // Request. This is because Payment Request is not tied to an
    // Obligee directly. So we can have one payment request with
    // many Disb Tran records associated with different obligees.
    // All the acblks that require to read the Disb Tran must be
    // passed the original obligee person number.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.DateWorkArea.Date = Now().Date;
    UseFnB666Housekeeping();
    local.Hardcode.HardcodePassthru.SystemGeneratedIdentifier = 71;
    local.Hardcode.HardcodeCrFee.SystemGeneratedIdentifier = 73;
    local.Hardcode.HardcodeDisbursement.Type1 = "D";
    local.Hardcode.HardcodeReleased.SystemGeneratedIdentifier = 1;
    local.Hardcode.HardcodeProcessed.SystemGeneratedIdentifier = 2;
    local.Hardcode.HardcodeWarrant.SystemGeneratedIdentifier = 1;
    local.Hardcode.HardcodeDesignatedPayee.SystemGeneratedIdentifier = 1;
    local.Hardcode.HardcodeSupport.Classification = "SUP";
    local.EabFileHandling.Action = "WRITE";
    local.FirstTime.Flag = "Y";
    local.Dummy.Flag = "Y";

    // ***** Select disbursement transactions that have not been processed yet.
    // ------------------------------------------------------
    // 12/8/00  Kalpesh PR 105240
    // Remove disbursement_type.program_code from sort.
    // ------------------------------------------------------
    foreach(var item in ReadDisbursementTransactionDisbursementTypeInterstateRequest())
      
    {
      // *** If a test payee number is present then only process disbursements 
      // for this test person.
      if (!IsEmpty(local.TestPayee.Number))
      {
        if (Equal(entities.ObligeeCsePerson.Number, local.TestPayee.Number))
        {
        }
        else
        {
          continue;
        }
      }

      if (ReadDisbursementStatusHistoryDisbursementStatus())
      {
        if (entities.DisbursementStatus.SystemGeneratedIdentifier != local
          .Hardcode.HardcodeReleased.SystemGeneratedIdentifier)
        {
          continue;
        }
      }

      if (!entities.DisbursementStatusHistory.Populated)
      {
        local.EabReportSend.RptDetail =
          "Current Disbursement Status History not found for Obligee " + entities
          .ObligeeCsePerson.Number + " Disb # " + NumberToString
          (entities.Debit.SystemGeneratedIdentifier, 15);
        UseCabControlReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.CountsAmounts.NbrOfDisbTransRead.Count;
      local.CountsAmounts.AmtOfDisbTransRead.Number112 += entities.Debit.Amount;
      ++local.DisbsReadSinceCommit.Count;

      // ---------------------------------------------------------------------------------------
      // May want to replace the int_h_generated_id part of the next statement 
      // with obligee cse_person -  per business rules that say seperate
      // payments based upon person/case.
      // ---------------------------------------------------------------------------------------
      // -------------------------------------------------------
      // 12/08/00    PR 105240
      // Remove program code from IF statement below.
      // -------------------------------------------------------
      if (entities.InterstateRequest.IntHGeneratedId != local
        .PreviousPayee.IntHGeneratedId || !
        Equal(entities.InterstateRequest.OtherStateCaseId,
        local.PreviousPayee.OtherStateCaseId) || !
        Equal(entities.ObligeeCsePerson.Number, local.PreviousObligee.Number))
      {
        // ---------------------------------------------------------------------------------------
        // A new Interstate Request has been read. So process the Interstate 
        // Request break
        // ---------------------------------------------------------------------------------------
        if (AsChar(local.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "Interstate Break:";
          UseCabControlReport1();
          local.EabReportSend.RptDetail = "Previous Person " + local
            .PreviousObligee.Number + " Int ID " + NumberToString
            (local.PreviousPayee.IntHGeneratedId, 15) + " Case ID " + (
              local.PreviousPayee.OtherStateCaseId ?? "") + " Program Code " + local
            .Previous.ProgramCode;
          UseCabControlReport1();
          local.EabReportSend.RptDetail = "Current  Person " + entities
            .ObligeeCsePerson.Number + " Int ID " + NumberToString
            (entities.InterstateRequest.IntHGeneratedId, 15) + " Case ID " + entities
            .InterstateRequest.OtherStateCaseId + " Program Code " + entities
            .DisbursementType.ProgramCode;
          UseCabControlReport1();
        }

        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          // --- No obligee details pending for update.
          local.FirstTime.Flag = "N";
        }
        else
        {
          // ---------------------------------------------------------------------------------------
          // Finish the processing for the previous interstate request.
          // ---------------------------------------------------------------------------------------
          if (local.PaymentRequest.Amount > 0)
          {
            // ----------------------------------------------------------------------
            // 1  -  Check for and possibly Recapture recovery debts from the 
            // obligee.
            // ----------------------------------------------------------------------
            // ----------------------------------------------------------------------
            // Recapturing interstate payments is a future enahancement.  
            // Enabling this section and the similiar section further down in
            // the code should "turn on" recaptures.
            // ----------------------------------------------------------------------
            if (local.PaymentRequest.Amount > 0)
            {
              if (Equal(local.PaymentRequest.Type1, "WAR"))
              {
                ++local.CountsAmounts.NbrOfWarrantsCreated.Count;
                local.CountsAmounts.AmtOfWarrantsCreated.Number112 += local.
                  PaymentRequest.Amount;
              }
              else
              {
                ++local.CountsAmounts.NbrOfEftsCreated.Count;
                local.CountsAmounts.AmtOfEftsCreated.Number112 += local.
                  PaymentRequest.Amount;
              }
            }
          }
          else if (local.PaymentRequest.Amount < 0)
          {
            // ------------------------------------------------------------------------------
            // 2  -  Change the Payment Request to Potential Recovery (RCV) from
            // Warrant (WAR) or EFT.
            // -----------------------------------------------------------------------------
            local.PaymentRequest.Type1 = "RCV";
            ++local.CountsAmounts.NbrOfRecoveriesCreated.Count;
            local.CountsAmounts.AmtOfRecoveriesCreated.Number112 += -
              local.PaymentRequest.Amount;
          }

          // The payment request will be completed (amt & type set) or deleted 
          // if the amt is 0.
          local.ObligeeForPmtRequest.Number =
            entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
          UseFnCompletePaymentRequest();

          if (!IsEmpty(local.EabReportSend.RptDetail))
          {
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          // ------------------------------------------------------------------------------
          // 4  -  Create an EFT Transmission Record if needed.
          // ------------------------------------------------------------------------------
          if (AsChar(local.DisplayInd.Flag) == 'Y')
          {
            // -----------------------------------------------------------------------
            // For testing only  -  Process a disbursement
            // -----------------------------------------------------------------------
            local.EabReportSend.RptDetail = "Created pmt ID " + NumberToString
              (entities.PaymentRequest.SystemGeneratedIdentifier, 15) + " type " +
              entities.PaymentRequest.Type1 + " for CSE person " + entities
              .PaymentRequest.CsePersonNumber + " for " + NumberToString
              ((long)(entities.PaymentRequest.Amount * 100), 15);
            UseCabControlReport1();
            local.EabReportSend.RptDetail = "";
            UseCabControlReport1();
          }

          // ----------------------------------------------------------------
          // PR105240 - 12/11/00
          // We are now netting across program types.  Hence, this IF
          // statement will never be true.
          // ----------------------------------------------------------------
          if (entities.InterstateRequest.IntHGeneratedId == local
            .PreviousPayee.IntHGeneratedId && Equal
            (entities.InterstateRequest.OtherStateCaseId,
            local.PreviousPayee.OtherStateCaseId) && Equal
            (entities.ObligeeCsePerson.Number, local.PreviousObligee.Number))
          {
          }
          else
          {
            local.PreviousPositive.SystemGeneratedIdentifier = 0;
            local.PreviousNegative.SystemGeneratedIdentifier = 0;

            // ---------------------------------------------------------------------------------------
            // Since we are at a break between Interstate Requests check to see 
            // if it is time to do a commit.
            // ---------------------------------------------------------------------------------------
            if (AsChar(local.AbendForTestInd.Flag) == 'Y')
            {
              // -----------------------------------------------------------------------
              // For testing only  - skip commits to leave database unchanged
              // -----------------------------------------------------------------------
            }
            else if (local.DisbsReadSinceCommit.Count >= local
              .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
            {
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.DisbsReadSinceCommit.Count = 0;
            }
          }
        }

        // ---------------------------------------------------------------------
        // Start the processing for the payment request:
        // Determine Payment method and create a new payment request.
        // ---------------------------------------------------------------------
        local.PaymentRequest.Amount = 0;
        local.PaymentRequest.InterstateInd = "Y";
        local.TotalDisbForPayee.TotalCurrency = 0;
        local.TotalCrFeesForPayee.TotalCurrency = 0;
        local.BackoffInd.Flag = "N";
        UseFnCreateWarOrEftPmtReq();

        if (!IsEmpty(local.EabReportSend.RptDetail))
        {
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (!ReadPaymentRequest())
        {
          local.EabReportSend.RptDetail =
            "Not found reading payment request for Payee " + entities
            .ObligeeCsePerson.Number + " and payment req # " + NumberToString
            (local.ForRead.SystemGeneratedIdentifier, 15) + " disb # " + NumberToString
            (entities.Debit.SystemGeneratedIdentifier, 15);
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.PaymentRequest.Type1 = entities.PaymentRequest.Type1;

        if (Equal(entities.PaymentRequest.Type1, "EFT"))
        {
          MoveDisbursementTransaction2(entities.Debit, local.FirstDisbForPmntReq);
            
        }

        local.PreviousPayee.IntHGeneratedId =
          entities.InterstateRequest.IntHGeneratedId;
        local.PreviousPayee.OtherStateCaseId =
          entities.InterstateRequest.OtherStateCaseId;
        local.PreviousObligee.Number = entities.ObligeeCsePerson.Number;
        local.Previous.ProgramCode = entities.DisbursementType.ProgramCode;
      }

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------
        // For testing only  -  Process a disbursement
        // -----------------------------------------------------------------------
        local.BatchTimestampWorkArea.IefTimestamp = Now();
        local.BatchTimestampWorkArea.TextTimestamp = "";
        UseLeCabConvertTimestamp();
        local.EabReportSend.RptDetail =
          local.BatchTimestampWorkArea.TextTimestamp + "  About to process disb for Payee " +
          entities.ObligeeCsePerson.Number + " Disb # " + NumberToString
          (entities.Debit.SystemGeneratedIdentifier, 15) + " Amt " + NumberToString
          ((long)(entities.Debit.Amount * 100), 7, 9);
        UseCabControlReport1();
      }

      // ---------------------------------------------
      // Process this disbursement transaction
      // ---------------------------------------------
      // These totals are for calculating the collection amount on the EFT 
      // transmission record.
      local.TotalDisbForPayee.TotalCurrency += entities.Debit.Amount;

      if (entities.Debit.Amount < 0)
      {
        local.BackoffInd.Flag = "Y";
      }

      // --------------------------------------------------------------
      // The following AB associates the disbursement transaction with the 
      // payment request and sets the status to "PROCESSED".
      // --------------------------------------------------------------
      UseFnProcesDebitDisbForPmtReq();

      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.CountsAmounts.NbrOfDisbTransUpdated.Count;
      local.CountsAmounts.AmtOfDisbTransUpdated.Number112 += entities.Debit.
        Amount;
      local.PaymentRequest.Amount += entities.Debit.Amount;
    }

    // -------------------------------------------------------------------------------
    // All disb_tran are processed.
    // Finish processing of the last Obligee.  Check for 'recapture' or '
    // recovery'
    // -------------------------------------------------------------------------------
    if (AsChar(local.FirstTime.Flag) != 'Y')
    {
      if (local.PaymentRequest.Amount > 0)
      {
        // ----------------------------------------------------------------------
        // 1  -  Check for and possibly Recapture recovery debts from the 
        // obligee.
        // ----------------------------------------------------------------------
        // ----------------------------------------------------------------------
        // Recapturing interstate payments is a future enahancement.  Enabling 
        // this section and the similiar section further up in the code should "
        // turn on" recaptures.
        // ----------------------------------------------------------------------
        if (local.PaymentRequest.Amount > 0)
        {
          if (Equal(local.PaymentRequest.Type1, "WAR"))
          {
            ++local.CountsAmounts.NbrOfWarrantsCreated.Count;
            local.CountsAmounts.AmtOfWarrantsCreated.Number112 += local.
              PaymentRequest.Amount;
          }
          else
          {
            ++local.CountsAmounts.NbrOfEftsCreated.Count;
            local.CountsAmounts.AmtOfEftsCreated.Number112 += local.
              PaymentRequest.Amount;
          }
        }
      }
      else if (local.PaymentRequest.Amount < 0)
      {
        // ------------------------------------------------------------------------------
        // 2  -  Change the Payment Request to Potential Recovery (RCV) from 
        // Warrant (WAR) or EFT.
        // -----------------------------------------------------------------------------
        local.PaymentRequest.Type1 = "RCV";
        ++local.CountsAmounts.NbrOfRecoveriesCreated.Count;
        local.CountsAmounts.AmtOfRecoveriesCreated.Number112 += -
          local.PaymentRequest.Amount;
      }

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------
        // D  -  For testing only  -  Finish the payment request
        // -----------------------------------------------------------------------
        local.BatchTimestampWorkArea.IefTimestamp = Now();
        local.BatchTimestampWorkArea.TextTimestamp = "";
        UseLeCabConvertTimestamp();
        local.EabReportSend.RptDetail =
          local.BatchTimestampWorkArea.TextTimestamp + "  About to update payment request for Payee " +
          entities.PaymentRequest.CsePersonNumber + "  ID# " + NumberToString
          (entities.PaymentRequest.SystemGeneratedIdentifier, 15) + "    " + local
          .PaymentRequest.Type1 + " " + NumberToString
          ((long)(local.PaymentRequest.Amount * 100), 15);
        UseCabControlReport2();
        local.EabReportSend.RptDetail = "";
        UseCabControlReport2();
      }

      // ------------------------------------------------------------------------------
      // 3  -  Update the Payment Request as needed.
      // ------------------------------------------------------------------------------
      local.ObligeeForPmtRequest.Number =
        entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
      UseFnCompletePaymentRequest();

      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -----------------------------------------------------------------
      // 4  -  Create an EFT Transmission Record if needed.
      // -----------------------------------------------------------------
      // ---------------------------------------------------------------------------------------
      // Check for a program type change for this payee.
      // ---------------------------------------------------------------------------------------
      if (entities.InterstateRequest.IntHGeneratedId == local
        .PreviousPayee.IntHGeneratedId && Equal
        (entities.InterstateRequest.OtherStateCaseId,
        local.PreviousPayee.OtherStateCaseId) && Equal
        (entities.ObligeeCsePerson.Number, local.PreviousObligee.Number))
      {
        if (local.PaymentRequest.Amount > 0)
        {
          if (Equal(local.PaymentRequest.Type1, "WAR") || Equal
            (local.PaymentRequest.Type1, "EFT"))
          {
            if (local.PreviousPositive.SystemGeneratedIdentifier > 0)
            {
              // ---------------------------------------------------------------------------------------
              // Combine the payment request with the previous positive payment 
              // request.
              // ---------------------------------------------------------------------------------------
              UseFnNet2PmtRequestsTogether1();

              if (!IsEmpty(local.EabReportSend.RptDetail))
              {
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              if (Equal(local.PaymentRequest.Type1, "WAR"))
              {
                --local.CountsAmounts.NbrOfWarrantsCreated.Count;
              }
              else
              {
                --local.CountsAmounts.NbrOfEftsCreated.Count;
              }
            }
            else
            {
              // ---------------------------------------------------------------------------------------
              // Store the pmt request ID as the previous pmt request.
              // ---------------------------------------------------------------------------------------
              local.PreviousPositive.SystemGeneratedIdentifier =
                entities.PaymentRequest.SystemGeneratedIdentifier;
            }
          }
          else if (local.PreviousNegative.SystemGeneratedIdentifier > 0)
          {
            // ---------------------------------------------------------------------------------------
            // Combine the payment request with the previous negative payment 
            // request.
            // ---------------------------------------------------------------------------------------
            UseFnNet2PmtRequestsTogether2();

            if (!IsEmpty(local.EabReportSend.RptDetail))
            {
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            --local.CountsAmounts.NbrOfRecoveriesCreated.Count;
          }
          else
          {
            // ---------------------------------------------------------------------------------------
            // Store the pmt request ID as the previous pmt request.
            // ---------------------------------------------------------------------------------------
            local.PreviousNegative.SystemGeneratedIdentifier =
              entities.PaymentRequest.SystemGeneratedIdentifier;
          }
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail = UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Error processing CSE Person # " + entities
        .ObligeeCsePerson.Number + " Disb # " + NumberToString
        (entities.Debit.SystemGeneratedIdentifier, 15) + " " + local
        .EabReportSend.RptDetail;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------
    // Check for interstate disbursements without a association to an interstate
    // request.
    // -----------------------------------------------------------------------
    foreach(var item in ReadDisbursementTransactionCsePersonCsePersonAccount())
    {
      if (ReadInterstateRequest())
      {
        // Continue
      }
      else
      {
        local.EabReportSend.RptDetail =
          "Interstate Request not found for Obligee " + entities
          .ObligeeCsePerson.Number + " Disb # " + NumberToString
          (entities.Debit.SystemGeneratedIdentifier, 15);
        UseCabErrorReport1();
      }
    }

    UseFnPrintPmtReqCntrlTotals();

    if (AsChar(local.AbendForTestInd.Flag) == 'Y')
    {
      // -----------------------------------------------------------------------
      // For testing only  - Abend to leave database unchanged
      // -----------------------------------------------------------------------
      local.EabReportSend.RptDetail =
        "Abend for test purposes to leave database unchanged.";
      UseCabControlReport1();
      ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
    }
    else
    {
      local.EabReportSend.RptDetail = "Finished processing.";
      UseCabControlReport1();
    }

    UseSiCloseAdabas();
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCountsAmounts(Local.CountsAmountsGroup source,
    FnPrintPmtReqCntrlTotals.Import.CountsGroup target)
  {
    target.NbrOfDisbTransRead.Count = source.NbrOfDisbTransRead.Count;
    target.AmtOfDisbTransRead.Number112 = source.AmtOfDisbTransRead.Number112;
    target.NbrOfDisbTransUpdated.Count = source.NbrOfDisbTransUpdated.Count;
    target.AmtOfDisbTransUpdated.Number112 =
      source.AmtOfDisbTransUpdated.Number112;
    target.NbrOfWarrantsCreated.Count = source.NbrOfWarrantsCreated.Count;
    target.AmtOfWarrantsCreated.Number112 =
      source.AmtOfWarrantsCreated.Number112;
    target.NbrOfEftsCreated.Count = source.NbrOfEftsCreated.Count;
    target.AmtOfEftsCreated.Number112 = source.AmtOfEftsCreated.Number112;
    target.NbrOfRecapturesCreated.Count = source.NbrOfRecapturesCreated.Count;
    target.AmtOfRecapturesCreated.Number112 =
      source.AmtOfRecapturesCreated.Number112;
    target.NbrOfRecoveriesCreated.Count = source.NbrOfRecoveriesCreated.Count;
    target.AmtOfRecoveriesCreated.Number112 =
      source.AmtOfRecoveriesCreated.Number112;
  }

  private static void MoveDisbursementStatusHistory(
    DisbursementStatusHistory source, DisbursementStatusHistory target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.ReasonText = source.ReasonText;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveDisbursementTransaction1(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.CollectionDate = source.CollectionDate;
    target.InterstateInd = source.InterstateInd;
    target.DisbursementDate = source.DisbursementDate;
    target.CashNonCashInd = source.CashNonCashInd;
    target.RecapturedInd = source.RecapturedInd;
  }

  private static void MoveDisbursementTransaction2(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB666Housekeeping()
  {
    var useImport = new FnB666Housekeeping.Import();
    var useExport = new FnB666Housekeeping.Export();

    useExport.PersistentRequested.Assign(entities.Requested);
    useExport.PersistentRcvpot.Assign(entities.Rcvpot);
    useExport.PersistentProcessed.Assign(entities.Processed);
    useExport.PersistentCanceled.Assign(entities.Canceled);
    useExport.PersistentDenied.Assign(entities.Denied);

    Call(FnB666Housekeeping.Execute, useImport, useExport);

    entities.Requested.SystemGeneratedIdentifier =
      useExport.PersistentRequested.SystemGeneratedIdentifier;
    entities.Rcvpot.SystemGeneratedIdentifier =
      useExport.PersistentRcvpot.SystemGeneratedIdentifier;
    entities.Processed.SystemGeneratedIdentifier =
      useExport.PersistentProcessed.SystemGeneratedIdentifier;
    local.ProgramProcessingInfo.ProcessDate =
      useExport.ProgramProcessingInfo.ProcessDate;
    local.Maximum.Date = useExport.Maximum.Date;
    local.ProgramCheckpointRestart.ReadFrequencyCount =
      useExport.ProgramCheckpointRestart.ReadFrequencyCount;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    local.TestPayee.Number = useExport.TestPayee.Number;
    local.AbendForTestInd.Flag = useExport.AbendForTestInd.Flag;
    entities.Canceled.SystemGeneratedIdentifier =
      useExport.PersistentCanceled.SystemGeneratedIdentifier;
    entities.Denied.SystemGeneratedIdentifier =
      useExport.PersistentDenied.SystemGeneratedIdentifier;
  }

  private void UseFnCompletePaymentRequest()
  {
    var useImport = new FnCompletePaymentRequest.Import();
    var useExport = new FnCompletePaymentRequest.Export();

    useImport.Persistent.Assign(entities.PaymentRequest);
    useImport.PersistentRequested.Assign(entities.Requested);
    useImport.PersistentRcvpot.Assign(entities.Rcvpot);
    MovePaymentRequest(local.PaymentRequest, useImport.ForUpdate);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Maximum.Date = local.Maximum.Date;
    useImport.Obligee.Number = local.ObligeeForPmtRequest.Number;
    useImport.PersistentCancelled.Assign(entities.Canceled);
    useImport.PersistentDenied.Assign(entities.Denied);

    Call(FnCompletePaymentRequest.Execute, useImport, useExport);

    entities.PaymentRequest.Assign(useImport.Persistent);
    entities.Requested.SystemGeneratedIdentifier =
      useImport.PersistentRequested.SystemGeneratedIdentifier;
    entities.Rcvpot.SystemGeneratedIdentifier =
      useImport.PersistentRcvpot.SystemGeneratedIdentifier;
    entities.Canceled.SystemGeneratedIdentifier =
      useImport.PersistentCancelled.SystemGeneratedIdentifier;
    entities.Denied.SystemGeneratedIdentifier =
      useImport.PersistentDenied.SystemGeneratedIdentifier;
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnCreateWarOrEftPmtReq()
  {
    var useImport = new FnCreateWarOrEftPmtReq.Import();
    var useExport = new FnCreateWarOrEftPmtReq.Export();

    useImport.Debit.SystemGeneratedIdentifier =
      entities.Debit.SystemGeneratedIdentifier;
    useImport.Obligee.Number = entities.ObligeeCsePerson.Number;
    useImport.PaymentRequest.InterstateInd = local.PaymentRequest.InterstateInd;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(FnCreateWarOrEftPmtReq.Execute, useImport, useExport);

    local.ForRead.SystemGeneratedIdentifier =
      useExport.PaymentRequest.SystemGeneratedIdentifier;
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
    local.PersonPreferredPaymentMethod.Assign(
      useExport.PersonPreferredPaymentMethod);
    local.DesignatedPayee.Number = useExport.DesignatedPayee.Number;
  }

  private void UseFnNet2PmtRequestsTogether1()
  {
    var useImport = new FnNet2PmtRequestsTogether.Import();
    var useExport = new FnNet2PmtRequestsTogether.Export();

    useImport.Persistent.Assign(entities.PaymentRequest);
    useImport.Obligee.Number = entities.ObligeeCsePerson.Number;
    useImport.Previous.SystemGeneratedIdentifier =
      local.PreviousPositive.SystemGeneratedIdentifier;

    Call(FnNet2PmtRequestsTogether.Execute, useImport, useExport);

    entities.PaymentRequest.Assign(useImport.Persistent);
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnNet2PmtRequestsTogether2()
  {
    var useImport = new FnNet2PmtRequestsTogether.Import();
    var useExport = new FnNet2PmtRequestsTogether.Export();

    useImport.Persistent.Assign(entities.PaymentRequest);
    useImport.Obligee.Number = entities.ObligeeCsePerson.Number;
    useImport.Previous.SystemGeneratedIdentifier =
      local.PreviousNegative.SystemGeneratedIdentifier;

    Call(FnNet2PmtRequestsTogether.Execute, useImport, useExport);

    entities.PaymentRequest.Assign(useImport.Persistent);
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnPrintPmtReqCntrlTotals()
  {
    var useImport = new FnPrintPmtReqCntrlTotals.Import();
    var useExport = new FnPrintPmtReqCntrlTotals.Export();

    MoveCountsAmounts(local.CountsAmounts, useImport.Counts);

    Call(FnPrintPmtReqCntrlTotals.Execute, useImport, useExport);
  }

  private void UseFnProcesDebitDisbForPmtReq()
  {
    var useImport = new FnProcesDebitDisbForPmtReq.Import();
    var useExport = new FnProcesDebitDisbForPmtReq.Export();

    useImport.PersistentPaymentRequest.Assign(entities.PaymentRequest);
    useImport.PersistentDebit.Assign(entities.Debit);
    useImport.PersistentObligee.Assign(entities.ObligeeCsePerson);
    useImport.PersistentDisbursementStatusHistory.Assign(
      entities.DisbursementStatusHistory);
    useImport.PersistentProcessed.Assign(entities.Processed);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Maximum.Date = local.Maximum.Date;

    Call(FnProcesDebitDisbForPmtReq.Execute, useImport, useExport);

    entities.PaymentRequest.Assign(useImport.PersistentPaymentRequest);
    MoveDisbursementTransaction1(useImport.PersistentDebit, entities.Debit);
    entities.ObligeeCsePerson.Number = useImport.PersistentObligee.Number;
    MoveDisbursementStatusHistory(useImport.PersistentDisbursementStatusHistory,
      entities.DisbursementStatusHistory);
    entities.Processed.SystemGeneratedIdentifier =
      useImport.PersistentProcessed.SystemGeneratedIdentifier;
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private bool ReadDisbursementStatusHistoryDisbursementStatus()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatusHistoryDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementStatus.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private IEnumerable<bool>
    ReadDisbursementTransactionCsePersonCsePersonAccount()
  {
    entities.Debit.Populated = false;
    entities.ObligeeCsePerson.Populated = false;
    entities.ObligeeCsePersonAccount.Populated = false;

    return ReadEach("ReadDisbursementTransactionCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate",
          local.InitializedDisbursementTransaction.ProcessDate.
            GetValueOrDefault());
        db.
          SetString(command, "type", local.Hardcode.HardcodeDisbursement.Type1);
          
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.ObligeeCsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.ObligeeCsePerson.Number = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.Amount = db.GetDecimal(reader, 4);
        entities.Debit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Debit.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Debit.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.Debit.DisbursementDate = db.GetNullableDate(reader, 8);
        entities.Debit.CashNonCashInd = db.GetString(reader, 9);
        entities.Debit.RecapturedInd = db.GetString(reader, 10);
        entities.Debit.CollectionDate = db.GetNullableDate(reader, 11);
        entities.Debit.DbtGeneratedId = db.GetNullableInt32(reader, 12);
        entities.Debit.InterstateInd = db.GetNullableString(reader, 13);
        entities.Debit.IntInterId = db.GetNullableInt32(reader, 14);
        entities.Debit.Populated = true;
        entities.ObligeeCsePerson.Populated = true;
        entities.ObligeeCsePersonAccount.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligeeCsePersonAccount.Type1);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Debit.CashNonCashInd);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDisbursementTransactionDisbursementTypeInterstateRequest()
  {
    entities.Debit.Populated = false;
    entities.InterstateRequest.Populated = false;
    entities.ObligeeCsePerson.Populated = false;
    entities.ObligeeCsePersonAccount.Populated = false;
    entities.DisbursementType.Populated = false;

    return ReadEach(
      "ReadDisbursementTransactionDisbursementTypeInterstateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate",
          local.InitializedDisbursementTransaction.ProcessDate.
            GetValueOrDefault());
        db.
          SetString(command, "type", local.Hardcode.HardcodeDisbursement.Type1);
          
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.ObligeeCsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.ObligeeCsePerson.Number = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.Amount = db.GetDecimal(reader, 4);
        entities.Debit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Debit.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Debit.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.Debit.DisbursementDate = db.GetNullableDate(reader, 8);
        entities.Debit.CashNonCashInd = db.GetString(reader, 9);
        entities.Debit.RecapturedInd = db.GetString(reader, 10);
        entities.Debit.CollectionDate = db.GetNullableDate(reader, 11);
        entities.Debit.DbtGeneratedId = db.GetNullableInt32(reader, 12);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.Debit.InterstateInd = db.GetNullableString(reader, 13);
        entities.Debit.IntInterId = db.GetNullableInt32(reader, 14);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 14);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 15);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 16);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 17);
        entities.Debit.Populated = true;
        entities.InterstateRequest.Populated = true;
        entities.ObligeeCsePerson.Populated = true;
        entities.ObligeeCsePersonAccount.Populated = true;
        entities.DisbursementType.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligeeCsePersonAccount.Type1);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Debit.CashNonCashInd);

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.Debit.IntInterId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId", local.ForRead.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Type1 = db.GetString(reader, 9);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 11);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
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
    /// <summary>A CountsAmountsGroup group.</summary>
    [Serializable]
    public class CountsAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDisbTransRead.
      /// </summary>
      [JsonPropertyName("nbrOfDisbTransRead")]
      public Common NbrOfDisbTransRead
      {
        get => nbrOfDisbTransRead ??= new();
        set => nbrOfDisbTransRead = value;
      }

      /// <summary>
      /// A value of AmtOfDisbTransRead.
      /// </summary>
      [JsonPropertyName("amtOfDisbTransRead")]
      public NumericWorkSet AmtOfDisbTransRead
      {
        get => amtOfDisbTransRead ??= new();
        set => amtOfDisbTransRead = value;
      }

      /// <summary>
      /// A value of NbrOfDisbTransUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfDisbTransUpdated")]
      public Common NbrOfDisbTransUpdated
      {
        get => nbrOfDisbTransUpdated ??= new();
        set => nbrOfDisbTransUpdated = value;
      }

      /// <summary>
      /// A value of AmtOfDisbTransUpdated.
      /// </summary>
      [JsonPropertyName("amtOfDisbTransUpdated")]
      public NumericWorkSet AmtOfDisbTransUpdated
      {
        get => amtOfDisbTransUpdated ??= new();
        set => amtOfDisbTransUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfWarrantsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfWarrantsCreated")]
      public Common NbrOfWarrantsCreated
      {
        get => nbrOfWarrantsCreated ??= new();
        set => nbrOfWarrantsCreated = value;
      }

      /// <summary>
      /// A value of AmtOfWarrantsCreated.
      /// </summary>
      [JsonPropertyName("amtOfWarrantsCreated")]
      public NumericWorkSet AmtOfWarrantsCreated
      {
        get => amtOfWarrantsCreated ??= new();
        set => amtOfWarrantsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfEftsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfEftsCreated")]
      public Common NbrOfEftsCreated
      {
        get => nbrOfEftsCreated ??= new();
        set => nbrOfEftsCreated = value;
      }

      /// <summary>
      /// A value of AmtOfEftsCreated.
      /// </summary>
      [JsonPropertyName("amtOfEftsCreated")]
      public NumericWorkSet AmtOfEftsCreated
      {
        get => amtOfEftsCreated ??= new();
        set => amtOfEftsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfRecapturesCreated.
      /// </summary>
      [JsonPropertyName("nbrOfRecapturesCreated")]
      public Common NbrOfRecapturesCreated
      {
        get => nbrOfRecapturesCreated ??= new();
        set => nbrOfRecapturesCreated = value;
      }

      /// <summary>
      /// A value of AmtOfRecapturesCreated.
      /// </summary>
      [JsonPropertyName("amtOfRecapturesCreated")]
      public NumericWorkSet AmtOfRecapturesCreated
      {
        get => amtOfRecapturesCreated ??= new();
        set => amtOfRecapturesCreated = value;
      }

      /// <summary>
      /// A value of NbrOfRecoveriesCreated.
      /// </summary>
      [JsonPropertyName("nbrOfRecoveriesCreated")]
      public Common NbrOfRecoveriesCreated
      {
        get => nbrOfRecoveriesCreated ??= new();
        set => nbrOfRecoveriesCreated = value;
      }

      /// <summary>
      /// A value of AmtOfRecoveriesCreated.
      /// </summary>
      [JsonPropertyName("amtOfRecoveriesCreated")]
      public NumericWorkSet AmtOfRecoveriesCreated
      {
        get => amtOfRecoveriesCreated ??= new();
        set => amtOfRecoveriesCreated = value;
      }

      private Common nbrOfDisbTransRead;
      private NumericWorkSet amtOfDisbTransRead;
      private Common nbrOfDisbTransUpdated;
      private NumericWorkSet amtOfDisbTransUpdated;
      private Common nbrOfWarrantsCreated;
      private NumericWorkSet amtOfWarrantsCreated;
      private Common nbrOfEftsCreated;
      private NumericWorkSet amtOfEftsCreated;
      private Common nbrOfRecapturesCreated;
      private NumericWorkSet amtOfRecapturesCreated;
      private Common nbrOfRecoveriesCreated;
      private NumericWorkSet amtOfRecoveriesCreated;
    }

    /// <summary>A HardcodeGroup group.</summary>
    [Serializable]
    public class HardcodeGroup
    {
      /// <summary>
      /// A value of HardcodeCrFee.
      /// </summary>
      [JsonPropertyName("hardcodeCrFee")]
      public DisbursementType HardcodeCrFee
      {
        get => hardcodeCrFee ??= new();
        set => hardcodeCrFee = value;
      }

      /// <summary>
      /// A value of HardcodeDisbursement.
      /// </summary>
      [JsonPropertyName("hardcodeDisbursement")]
      public DisbursementTransaction HardcodeDisbursement
      {
        get => hardcodeDisbursement ??= new();
        set => hardcodeDisbursement = value;
      }

      /// <summary>
      /// A value of HardcodePassthru.
      /// </summary>
      [JsonPropertyName("hardcodePassthru")]
      public DisbursementType HardcodePassthru
      {
        get => hardcodePassthru ??= new();
        set => hardcodePassthru = value;
      }

      /// <summary>
      /// A value of HardcodeSupport.
      /// </summary>
      [JsonPropertyName("hardcodeSupport")]
      public PaymentRequest HardcodeSupport
      {
        get => hardcodeSupport ??= new();
        set => hardcodeSupport = value;
      }

      /// <summary>
      /// A value of HardcodeDesignatedPayee.
      /// </summary>
      [JsonPropertyName("hardcodeDesignatedPayee")]
      public CsePersonRlnRsn HardcodeDesignatedPayee
      {
        get => hardcodeDesignatedPayee ??= new();
        set => hardcodeDesignatedPayee = value;
      }

      /// <summary>
      /// A value of HardcodeReleased.
      /// </summary>
      [JsonPropertyName("hardcodeReleased")]
      public DisbursementStatus HardcodeReleased
      {
        get => hardcodeReleased ??= new();
        set => hardcodeReleased = value;
      }

      /// <summary>
      /// A value of HardcodeProcessed.
      /// </summary>
      [JsonPropertyName("hardcodeProcessed")]
      public DisbursementStatus HardcodeProcessed
      {
        get => hardcodeProcessed ??= new();
        set => hardcodeProcessed = value;
      }

      /// <summary>
      /// A value of HardcodeWarrant.
      /// </summary>
      [JsonPropertyName("hardcodeWarrant")]
      public PaymentMethodType HardcodeWarrant
      {
        get => hardcodeWarrant ??= new();
        set => hardcodeWarrant = value;
      }

      private DisbursementType hardcodeCrFee;
      private DisbursementTransaction hardcodeDisbursement;
      private DisbursementType hardcodePassthru;
      private PaymentRequest hardcodeSupport;
      private CsePersonRlnRsn hardcodeDesignatedPayee;
      private DisbursementStatus hardcodeReleased;
      private DisbursementStatus hardcodeProcessed;
      private PaymentMethodType hardcodeWarrant;
    }

    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of NbrOfEftsRead.
      /// </summary>
      [JsonPropertyName("nbrOfEftsRead")]
      public Common NbrOfEftsRead
      {
        get => nbrOfEftsRead ??= new();
        set => nbrOfEftsRead = value;
      }

      /// <summary>
      /// A value of NbrOfEftsReceipted.
      /// </summary>
      [JsonPropertyName("nbrOfEftsReceipted")]
      public Common NbrOfEftsReceipted
      {
        get => nbrOfEftsReceipted ??= new();
        set => nbrOfEftsReceipted = value;
      }

      /// <summary>
      /// A value of NbrOfEftsPended.
      /// </summary>
      [JsonPropertyName("nbrOfEftsPended")]
      public Common NbrOfEftsPended
      {
        get => nbrOfEftsPended ??= new();
        set => nbrOfEftsPended = value;
      }

      /// <summary>
      /// A value of AmtOfEftRecordsRead.
      /// </summary>
      [JsonPropertyName("amtOfEftRecordsRead")]
      public Common AmtOfEftRecordsRead
      {
        get => amtOfEftRecordsRead ??= new();
        set => amtOfEftRecordsRead = value;
      }

      /// <summary>
      /// A value of AmtOfEftRecReceipted.
      /// </summary>
      [JsonPropertyName("amtOfEftRecReceipted")]
      public Common AmtOfEftRecReceipted
      {
        get => amtOfEftRecReceipted ??= new();
        set => amtOfEftRecReceipted = value;
      }

      /// <summary>
      /// A value of AmtOfEftRecordsPended.
      /// </summary>
      [JsonPropertyName("amtOfEftRecordsPended")]
      public Common AmtOfEftRecordsPended
      {
        get => amtOfEftRecordsPended ??= new();
        set => amtOfEftRecordsPended = value;
      }

      /// <summary>
      /// A value of NbrOfCrReceipted.
      /// </summary>
      [JsonPropertyName("nbrOfCrReceipted")]
      public Common NbrOfCrReceipted
      {
        get => nbrOfCrReceipted ??= new();
        set => nbrOfCrReceipted = value;
      }

      /// <summary>
      /// A value of NbrOfNonCrReceipted.
      /// </summary>
      [JsonPropertyName("nbrOfNonCrReceipted")]
      public Common NbrOfNonCrReceipted
      {
        get => nbrOfNonCrReceipted ??= new();
        set => nbrOfNonCrReceipted = value;
      }

      /// <summary>
      /// A value of NbrOfIntCrMatched.
      /// </summary>
      [JsonPropertyName("nbrOfIntCrMatched")]
      public Common NbrOfIntCrMatched
      {
        get => nbrOfIntCrMatched ??= new();
        set => nbrOfIntCrMatched = value;
      }

      /// <summary>
      /// A value of NbrOfIntCrNotMatched.
      /// </summary>
      [JsonPropertyName("nbrOfIntCrNotMatched")]
      public Common NbrOfIntCrNotMatched
      {
        get => nbrOfIntCrNotMatched ??= new();
        set => nbrOfIntCrNotMatched = value;
      }

      /// <summary>
      /// A value of AmtOfCrReceipted.
      /// </summary>
      [JsonPropertyName("amtOfCrReceipted")]
      public Common AmtOfCrReceipted
      {
        get => amtOfCrReceipted ??= new();
        set => amtOfCrReceipted = value;
      }

      /// <summary>
      /// A value of AmtOfNonCrReceipted.
      /// </summary>
      [JsonPropertyName("amtOfNonCrReceipted")]
      public Common AmtOfNonCrReceipted
      {
        get => amtOfNonCrReceipted ??= new();
        set => amtOfNonCrReceipted = value;
      }

      /// <summary>
      /// A value of AmtOfIntCrMatched.
      /// </summary>
      [JsonPropertyName("amtOfIntCrMatched")]
      public Common AmtOfIntCrMatched
      {
        get => amtOfIntCrMatched ??= new();
        set => amtOfIntCrMatched = value;
      }

      /// <summary>
      /// A value of AmtOfIntCrNotMatched.
      /// </summary>
      [JsonPropertyName("amtOfIntCrNotMatched")]
      public Common AmtOfIntCrNotMatched
      {
        get => amtOfIntCrNotMatched ??= new();
        set => amtOfIntCrNotMatched = value;
      }

      /// <summary>
      /// A value of NbrOfCrdRecorded.
      /// </summary>
      [JsonPropertyName("nbrOfCrdRecorded")]
      public Common NbrOfCrdRecorded
      {
        get => nbrOfCrdRecorded ??= new();
        set => nbrOfCrdRecorded = value;
      }

      /// <summary>
      /// A value of NbrOfCrdReleased.
      /// </summary>
      [JsonPropertyName("nbrOfCrdReleased")]
      public Common NbrOfCrdReleased
      {
        get => nbrOfCrdReleased ??= new();
        set => nbrOfCrdReleased = value;
      }

      /// <summary>
      /// A value of NbrOfCrdPended.
      /// </summary>
      [JsonPropertyName("nbrOfCrdPended")]
      public Common NbrOfCrdPended
      {
        get => nbrOfCrdPended ??= new();
        set => nbrOfCrdPended = value;
      }

      /// <summary>
      /// A value of NbrOfCrdSuspended.
      /// </summary>
      [JsonPropertyName("nbrOfCrdSuspended")]
      public Common NbrOfCrdSuspended
      {
        get => nbrOfCrdSuspended ??= new();
        set => nbrOfCrdSuspended = value;
      }

      /// <summary>
      /// A value of AmtOfCrdRecorded.
      /// </summary>
      [JsonPropertyName("amtOfCrdRecorded")]
      public Common AmtOfCrdRecorded
      {
        get => amtOfCrdRecorded ??= new();
        set => amtOfCrdRecorded = value;
      }

      /// <summary>
      /// A value of AmtOfCrdReleased.
      /// </summary>
      [JsonPropertyName("amtOfCrdReleased")]
      public Common AmtOfCrdReleased
      {
        get => amtOfCrdReleased ??= new();
        set => amtOfCrdReleased = value;
      }

      /// <summary>
      /// A value of AmtOfCrdPended.
      /// </summary>
      [JsonPropertyName("amtOfCrdPended")]
      public Common AmtOfCrdPended
      {
        get => amtOfCrdPended ??= new();
        set => amtOfCrdPended = value;
      }

      /// <summary>
      /// A value of AmtOfCrdSuspended.
      /// </summary>
      [JsonPropertyName("amtOfCrdSuspended")]
      public Common AmtOfCrdSuspended
      {
        get => amtOfCrdSuspended ??= new();
        set => amtOfCrdSuspended = value;
      }

      /// <summary>
      /// A value of NbrOfCrdFees.
      /// </summary>
      [JsonPropertyName("nbrOfCrdFees")]
      public Common NbrOfCrdFees
      {
        get => nbrOfCrdFees ??= new();
        set => nbrOfCrdFees = value;
      }

      /// <summary>
      /// A value of AmtOfCrdFees.
      /// </summary>
      [JsonPropertyName("amtOfCrdFees")]
      public Common AmtOfCrdFees
      {
        get => amtOfCrdFees ??= new();
        set => amtOfCrdFees = value;
      }

      /// <summary>
      /// A value of NbrOfReads.
      /// </summary>
      [JsonPropertyName("nbrOfReads")]
      public Common NbrOfReads
      {
        get => nbrOfReads ??= new();
        set => nbrOfReads = value;
      }

      /// <summary>
      /// A value of TotNbrOfReads.
      /// </summary>
      [JsonPropertyName("totNbrOfReads")]
      public Common TotNbrOfReads
      {
        get => totNbrOfReads ??= new();
        set => totNbrOfReads = value;
      }

      /// <summary>
      /// A value of NbrOfUpdates.
      /// </summary>
      [JsonPropertyName("nbrOfUpdates")]
      public Common NbrOfUpdates
      {
        get => nbrOfUpdates ??= new();
        set => nbrOfUpdates = value;
      }

      /// <summary>
      /// A value of NbrOfPendingErrors.
      /// </summary>
      [JsonPropertyName("nbrOfPendingErrors")]
      public Common NbrOfPendingErrors
      {
        get => nbrOfPendingErrors ??= new();
        set => nbrOfPendingErrors = value;
      }

      /// <summary>
      /// A value of NbrOfNonPendingErrors.
      /// </summary>
      [JsonPropertyName("nbrOfNonPendingErrors")]
      public Common NbrOfNonPendingErrors
      {
        get => nbrOfNonPendingErrors ??= new();
        set => nbrOfNonPendingErrors = value;
      }

      /// <summary>
      /// A value of TotNbrOfUpdates.
      /// </summary>
      [JsonPropertyName("totNbrOfUpdates")]
      public Common TotNbrOfUpdates
      {
        get => totNbrOfUpdates ??= new();
        set => totNbrOfUpdates = value;
      }

      /// <summary>
      /// A value of NbrOfCheckpoints.
      /// </summary>
      [JsonPropertyName("nbrOfCheckpoints")]
      public Common NbrOfCheckpoints
      {
        get => nbrOfCheckpoints ??= new();
        set => nbrOfCheckpoints = value;
      }

      private Common nbrOfEftsRead;
      private Common nbrOfEftsReceipted;
      private Common nbrOfEftsPended;
      private Common amtOfEftRecordsRead;
      private Common amtOfEftRecReceipted;
      private Common amtOfEftRecordsPended;
      private Common nbrOfCrReceipted;
      private Common nbrOfNonCrReceipted;
      private Common nbrOfIntCrMatched;
      private Common nbrOfIntCrNotMatched;
      private Common amtOfCrReceipted;
      private Common amtOfNonCrReceipted;
      private Common amtOfIntCrMatched;
      private Common amtOfIntCrNotMatched;
      private Common nbrOfCrdRecorded;
      private Common nbrOfCrdReleased;
      private Common nbrOfCrdPended;
      private Common nbrOfCrdSuspended;
      private Common amtOfCrdRecorded;
      private Common amtOfCrdReleased;
      private Common amtOfCrdPended;
      private Common amtOfCrdSuspended;
      private Common nbrOfCrdFees;
      private Common amtOfCrdFees;
      private Common nbrOfReads;
      private Common totNbrOfReads;
      private Common nbrOfUpdates;
      private Common nbrOfPendingErrors;
      private Common nbrOfNonPendingErrors;
      private Common totNbrOfUpdates;
      private Common nbrOfCheckpoints;
    }

    /// <summary>
    /// A value of ForRead.
    /// </summary>
    [JsonPropertyName("forRead")]
    public PaymentRequest ForRead
    {
      get => forRead ??= new();
      set => forRead = value;
    }

    /// <summary>
    /// A value of PreviousPositive.
    /// </summary>
    [JsonPropertyName("previousPositive")]
    public PaymentRequest PreviousPositive
    {
      get => previousPositive ??= new();
      set => previousPositive = value;
    }

    /// <summary>
    /// A value of PreviousNegative.
    /// </summary>
    [JsonPropertyName("previousNegative")]
    public PaymentRequest PreviousNegative
    {
      get => previousNegative ??= new();
      set => previousNegative = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DisbursementType Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of PreviousObligee.
    /// </summary>
    [JsonPropertyName("previousObligee")]
    public CsePerson PreviousObligee
    {
      get => previousObligee ??= new();
      set => previousObligee = value;
    }

    /// <summary>
    /// A value of PreviousPayee.
    /// </summary>
    [JsonPropertyName("previousPayee")]
    public InterstateRequest PreviousPayee
    {
      get => previousPayee ??= new();
      set => previousPayee = value;
    }

    /// <summary>
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public InterstateRequest Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public InterstateRequest Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of OutputNoOfSuccIntReq.
    /// </summary>
    [JsonPropertyName("outputNoOfSuccIntReq")]
    public ProgramControlTotal OutputNoOfSuccIntReq
    {
      get => outputNoOfSuccIntReq ??= new();
      set => outputNoOfSuccIntReq = value;
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
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
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
    /// A value of InitializedPaymentRequest.
    /// </summary>
    [JsonPropertyName("initializedPaymentRequest")]
    public PaymentRequest InitializedPaymentRequest
    {
      get => initializedPaymentRequest ??= new();
      set => initializedPaymentRequest = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of InitializedDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("initializedDisbursementTransaction")]
    public DisbursementTransaction InitializedDisbursementTransaction
    {
      get => initializedDisbursementTransaction ??= new();
      set => initializedDisbursementTransaction = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// Gets a value of CountsAmounts.
    /// </summary>
    [JsonPropertyName("countsAmounts")]
    public CountsAmountsGroup CountsAmounts
    {
      get => countsAmounts ?? (countsAmounts = new());
      set => countsAmounts = value;
    }

    /// <summary>
    /// Gets a value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public HardcodeGroup Hardcode
    {
      get => hardcode ?? (hardcode = new());
      set => hardcode = value;
    }

    /// <summary>
    /// A value of TotalDisbForPayee.
    /// </summary>
    [JsonPropertyName("totalDisbForPayee")]
    public Common TotalDisbForPayee
    {
      get => totalDisbForPayee ??= new();
      set => totalDisbForPayee = value;
    }

    /// <summary>
    /// A value of TotalCrFeesForPayee.
    /// </summary>
    [JsonPropertyName("totalCrFeesForPayee")]
    public Common TotalCrFeesForPayee
    {
      get => totalCrFeesForPayee ??= new();
      set => totalCrFeesForPayee = value;
    }

    /// <summary>
    /// A value of BackoffInd.
    /// </summary>
    [JsonPropertyName("backoffInd")]
    public Common BackoffInd
    {
      get => backoffInd ??= new();
      set => backoffInd = value;
    }

    /// <summary>
    /// A value of Recaptured.
    /// </summary>
    [JsonPropertyName("recaptured")]
    public PaymentRequest Recaptured
    {
      get => recaptured ??= new();
      set => recaptured = value;
    }

    /// <summary>
    /// A value of NbrOfDisbCreatedInRecap.
    /// </summary>
    [JsonPropertyName("nbrOfDisbCreatedInRecap")]
    public Common NbrOfDisbCreatedInRecap
    {
      get => nbrOfDisbCreatedInRecap ??= new();
      set => nbrOfDisbCreatedInRecap = value;
    }

    /// <summary>
    /// A value of FirstDisbForPmntReq.
    /// </summary>
    [JsonPropertyName("firstDisbForPmntReq")]
    public DisbursementTransaction FirstDisbForPmntReq
    {
      get => firstDisbForPmntReq ??= new();
      set => firstDisbForPmntReq = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of NextEftId.
    /// </summary>
    [JsonPropertyName("nextEftId")]
    public ElectronicFundTransmission NextEftId
    {
      get => nextEftId ??= new();
      set => nextEftId = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of TestPayee.
    /// </summary>
    [JsonPropertyName("testPayee")]
    public CsePerson TestPayee
    {
      get => testPayee ??= new();
      set => testPayee = value;
    }

    /// <summary>
    /// A value of AbendForTestInd.
    /// </summary>
    [JsonPropertyName("abendForTestInd")]
    public Common AbendForTestInd
    {
      get => abendForTestInd ??= new();
      set => abendForTestInd = value;
    }

    /// <summary>
    /// A value of ObligeeForPmtRequest.
    /// </summary>
    [JsonPropertyName("obligeeForPmtRequest")]
    public CsePerson ObligeeForPmtRequest
    {
      get => obligeeForPmtRequest ??= new();
      set => obligeeForPmtRequest = value;
    }

    /// <summary>
    /// Gets a value of Totals.
    /// </summary>
    [JsonPropertyName("totals")]
    public TotalsGroup Totals
    {
      get => totals ?? (totals = new());
      set => totals = value;
    }

    /// <summary>
    /// A value of DisbsReadSinceCommit.
    /// </summary>
    [JsonPropertyName("disbsReadSinceCommit")]
    public Common DisbsReadSinceCommit
    {
      get => disbsReadSinceCommit ??= new();
      set => disbsReadSinceCommit = value;
    }

    private PaymentRequest forRead;
    private PaymentRequest previousPositive;
    private PaymentRequest previousNegative;
    private DisbursementType previous;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private CsePerson previousObligee;
    private InterstateRequest previousPayee;
    private InterstateRequest debug;
    private InterstateRequest restart;
    private ProgramControlTotal outputNoOfSuccIntReq;
    private ExitStateWorkArea exitStateWorkArea;
    private Common dummy;
    private Common firstTime;
    private PaymentRequest initializedPaymentRequest;
    private PaymentRequest paymentRequest;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea maximum;
    private DisbursementTransaction initializedDisbursementTransaction;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private DateWorkArea dateWorkArea;
    private CountsAmountsGroup countsAmounts;
    private HardcodeGroup hardcode;
    private Common totalDisbForPayee;
    private Common totalCrFeesForPayee;
    private Common backoffInd;
    private PaymentRequest recaptured;
    private Common nbrOfDisbCreatedInRecap;
    private DisbursementTransaction firstDisbForPmntReq;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private ElectronicFundTransmission nextEftId;
    private CsePerson designatedPayee;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common displayInd;
    private CsePerson testPayee;
    private Common abendForTestInd;
    private CsePerson obligeeForPmtRequest;
    private TotalsGroup totals;
    private Common disbsReadSinceCommit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of ObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeCsePerson")]
    public CsePerson ObligeeCsePerson
    {
      get => obligeeCsePerson ??= new();
      set => obligeeCsePerson = value;
    }

    /// <summary>
    /// A value of ObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligeeCsePersonAccount")]
    public CsePersonAccount ObligeeCsePersonAccount
    {
      get => obligeeCsePersonAccount ??= new();
      set => obligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of Requested.
    /// </summary>
    [JsonPropertyName("requested")]
    public PaymentStatus Requested
    {
      get => requested ??= new();
      set => requested = value;
    }

    /// <summary>
    /// A value of Rcvpot.
    /// </summary>
    [JsonPropertyName("rcvpot")]
    public PaymentStatus Rcvpot
    {
      get => rcvpot ??= new();
      set => rcvpot = value;
    }

    /// <summary>
    /// A value of Canceled.
    /// </summary>
    [JsonPropertyName("canceled")]
    public PaymentStatus Canceled
    {
      get => canceled ??= new();
      set => canceled = value;
    }

    /// <summary>
    /// A value of Denied.
    /// </summary>
    [JsonPropertyName("denied")]
    public PaymentStatus Denied
    {
      get => denied ??= new();
      set => denied = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of OutboundEftNumber.
    /// </summary>
    [JsonPropertyName("outboundEftNumber")]
    public ControlTable OutboundEftNumber
    {
      get => outboundEftNumber ??= new();
      set => outboundEftNumber = value;
    }

    /// <summary>
    /// A value of OutboundEftTraceNumber.
    /// </summary>
    [JsonPropertyName("outboundEftTraceNumber")]
    public ControlTable OutboundEftTraceNumber
    {
      get => outboundEftTraceNumber ??= new();
      set => outboundEftTraceNumber = value;
    }

    private PaymentRequest paymentRequest;
    private DisbursementTransaction debit;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private InterstateRequest interstateRequest;
    private CsePerson obligeeCsePerson;
    private CsePersonAccount obligeeCsePersonAccount;
    private DisbursementType disbursementType;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
    private PaymentStatus requested;
    private PaymentStatus rcvpot;
    private PaymentStatus canceled;
    private PaymentStatus denied;
    private DisbursementStatus processed;
    private ControlTable outboundEftNumber;
    private ControlTable outboundEftTraceNumber;
  }
#endregion
}
