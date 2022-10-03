// Program: FN_B641_CREATE_WARR_AND_POT_REC, ID: 372673890, model: 746.
// Short name: SWEF641B
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
/// A program: FN_B641_CREATE_WARR_AND_POT_REC.
/// </para>
/// <para>
/// RESP: FINANCE
/// This proc step nets the disbursement debits and creates either a warrant or 
/// a potential recovery based on whether the net amount is positive or
/// negative.
/// If there is any recapture it also creates the cash receipt detail for the 
/// recaptured amount and immediately distributes it.
/// During the netting process it takes into account any refund of the Non ADC 
/// recovery fee that needs to be made.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB641CreateWarrAndPotRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B641_CREATE_WARR_AND_POT_REC program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB641CreateWarrAndPotRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB641CreateWarrAndPotRec.
  /// </summary>
  public FnB641CreateWarrAndPotRec(IContext context, Import import,
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
    // ---------------------------------------------
    // This batch procedure 'nets' the disbursement debits and
    // creates either a warrant or a potential recovery based on
    // whether the net amount is positive or negative. The netting
    // is done by the Payee. The Payee is the Designated Payee
    // or the Obligee itself (if desig payee is not specified). Note
    // that the Disb Tran is associated only with the original
    // Obligee (and not the designated payee). All the acblks that
    // require to read the Disb Tran must be passed the original
    // obligee person number. If any recapture is involved, it also
    // creates the cash receipt detail and distributes immediately.
    // During the netting process it takes into consideration any
    // refund of non adc cost recovery fee that needs to be made.
    // This batch procedure step combines the logic in all the
    // following batch procedures and thus makes them not
    // required any more.
    // 	SWEFB654 - Create Warrants
    // 	SWEFB659 - Create Potential Recoveries
    // 	SWEFB658 - Create Recaptures
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	Description
    // ----	--	-----------
    // 092997	govind	-Initial code. Cloned from FN BATCH
    // 		CREATE WARRANT. Created to combine
    // 		the disbursement jobs into one
    // 		procedure.
    // 111897	govind	-Fixed to skip 'last obligee processing'
    // 		at the end if READ EACH did not retrieve
    // 		any record.
    // 121197	govind	-Modified to use the new recapture
    // 		process FN RCAP PROCESS
    // 		RECAPTURE.
    // 		-Modified to group by the designated
    // 		payee instead of obligee.
    // 022498	govind	-Modified to generate warrant based on
    // 		Collection Disburse To AR Ind and
    // 		Program Applied To code.
    // 		- Modified to skip interstate disbursmnt.
    // 022698	govind	-Close adabas.
    // 030698	govind	-Skip Disbursement Transactions with
    // 		State as the Obligee.
    // 25nov98	lxj	-Bugs in Cost Recovery Fee are fixed.
    // 01dec98	lxj	-Create EFT payment_requests logic
    // 		included in this batch.
    // 		-Hardcode Action Blocks removed from
    // 		procedure.
    // 16dec98	lxj	-New reporting ABs added to this
    // 		procedure. Old ABs removed.
    // 04/05/99  Fangman  Added code for EFT transmission record and various 
    // other corrections.
    // 7/8/99     Fangman  Changed logic to use the control table to store the 
    // last EFT Number and EFT Trace Number instead of using the last number off
    // of the EFT table.
    // 8/5/99      Fangman  Restructured to give more info on abends and to 
    // accumulate totals at commit points only.
    // 1/8/00      Fangman  PR 84663  Fixed rounding error.
    // 4/12/00    Fangman  PR 93146  Changed code to "automatically" cancel 
    // warrants and deny recoveries if the amount is less than one dollar.
    // 12/28/01  Fangman  WR 000235  PSUM redevelopment.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnPmtRequestHousekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.Hardcoded.HardcodeDisbursement.Type1 = "D";
    local.Hardcoded.HardcodePassthru.SystemGeneratedIdentifier = 71;
    local.Hardcoded.HardcodeCrFee.SystemGeneratedIdentifier = 73;
    local.Hardcoded.HardcodeReleased.SystemGeneratedIdentifier = 1;
    local.Dummy.Flag = "Y";
    local.FirstTime.Flag = "Y";
    local.EabFileHandling.Action = "WRITE";
    local.PaymentRequest.InterstateInd = "N";

    if (ReadControlTable1())
    {
      local.NextEftId.TransmissionIdentifier =
        entities.OutboundEftNumber.LastUsedNumber + 1;

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------
        // For testing only  -  Process a disbursement
        // -----------------------------------------------------------------------
        local.EabReportSend.RptDetail =
          "Local_next_eft_id eft transmission_identifier = " + NumberToString
          (local.NextEftId.TransmissionIdentifier, 15);
        UseCabControlReport();
      }
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Critical Error:  Received a Not Found trying to read the control table to get the next Outbound EFT number.";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (ReadControlTable2())
    {
      local.NextEftId.TraceNumber =
        (long)entities.OutboundEftTraceNumber.LastUsedNumber + 1;

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------
        // For testing only  -  Process a disbursement
        // -----------------------------------------------------------------------
        local.EabReportSend.RptDetail =
          "Local_next_eft_id eft trace_number = " + NumberToString
          (local.NextEftId.TraceNumber.GetValueOrDefault(), 15);
        UseCabControlReport();
      }
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Critical Error:  Received a Not Found trying to read the control table to get the next Inbound EFT number.";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *** Read the unprocessed transactions ***
    foreach(var item in ReadCsePersonCsePersonAccountDisbursementTransaction())
    {
      if (entities.DisbursementType.SystemGeneratedIdentifier == local
        .Hardcoded.HardcodePassthru.SystemGeneratedIdentifier)
      {
        // *** All passthrus are handled in 652 or 653.
        continue;
      }

      // *** If a test payee number is present then only process disbursements 
      // for this test person.
      if (!IsEmpty(local.TestPayee.Number))
      {
        if (Equal(entities.ObligeeCsePerson.Number, local.TestPayee.Number))
        {
          // Process this test payee's disbursement.
        }
        else
        {
          continue;
        }
      }

      // The read for the status history was broken out of the main read each 
      // because not all disbursements have a current status history with an
      // ending date of 12-31-2099.
      if (ReadDisbursementStatusHistoryDisbursementStatus())
      {
        if (entities.DisbursementStatus.SystemGeneratedIdentifier != local
          .Hardcoded.HardcodeReleased.SystemGeneratedIdentifier)
        {
          // *** The disbursement may be in a suppressed status.
          continue;
        }
      }

      if (!entities.DisbursementStatusHistory.Populated)
      {
        local.EabReportSend.RptDetail =
          "No current disbursement status history found for Payee " + entities
          .ObligeeCsePerson.Number + " Disb # " + NumberToString
          (entities.Debit.SystemGeneratedIdentifier, 15);
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      ++local.CountsAmounts.NbrOfDisbTransRead.Count;
      local.CountsAmounts.AmtOfDisbTransRead.Number112 += entities.Debit.Amount;

      if (!Equal(entities.ObligeeCsePerson.Number, local.PreviousPayee.Number))
      {
        // ---------------------------------------------
        // A new Obligee has been read. So process the obligee break
        // ---------------------------------------------
        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          // --- No obligee details pending for update.
          local.FirstTime.Flag = "N";
        }
        else
        {
          if (AsChar(local.DisplayInd.Flag) == 'Y')
          {
            // -----------------------------------------------------------------------
            // For testing only  -  Process a disbursement
            // -----------------------------------------------------------------------
            local.EabReportSend.RptDetail = "";
            UseCabControlReport();
          }

          if (local.PaymentRequest.Amount > 0)
          {
            // ----------------------------------------------------------------------
            // 1  -  Check for and possibly Recapture recovery debts from the 
            // obligee.
            // ----------------------------------------------------------------------
            local.EabReportSend.RptDetail = "";
            local.ObligeeForPmtRequest.Number =
              entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
            UseFnRcapProcessRecapture();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            if (local.Recaptured.Amount > 0)
            {
              ++local.CountsAmounts.NbrOfRecapturesCreated.Count;
              local.CountsAmounts.AmtOfRecapturesCreated.Number112 += local.
                Recaptured.Amount;
              local.PaymentRequest.Amount -= local.Recaptured.Amount;
            }

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
            local.ObligeeForPmtRequest.Number =
              entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
            ++local.CountsAmounts.NbrOfRecoveriesCreated.Count;
            local.CountsAmounts.AmtOfRecoveriesCreated.Number112 += -
              local.PaymentRequest.Amount;
          }

          // ------------------------------------------------------------------------------
          // 3  -  Update the Payment Request as needed.
          // ------------------------------------------------------------------------------
          UseFnCompletePaymentRequest();

          if (!IsEmpty(local.EabReportSend.RptDetail))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }

          // ------------------------------------------------------------------------------
          // 4  -  Create an EFT Transmission Record if needed.
          // ------------------------------------------------------------------------------
          if (Equal(local.PaymentRequest.Type1, "EFT") && local
            .PaymentRequest.Amount > 0)
          {
            UseFnCreateEftTransmissionRec();

            if (!IsEmpty(local.EabReportSend.RptDetail))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              break;
            }
          }

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
            UseCabControlReport();
            local.EabReportSend.RptDetail = "";
            UseCabControlReport();
          }

          // ***** Since we are at a break between CSE_persons we can check to 
          // see if it is time to do a commit.
          if (local.CountsAmounts.NbrOfDisbTransUpdated.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            if (AsChar(local.AbendForTestInd.Flag) == 'Y')
            {
              // -----------------------------------------------------------------------
              // For testing only  - skip commits to leave database unchanged
              // -----------------------------------------------------------------------
            }
            else
            {
              try
              {
                UpdateControlTable1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ++local.NbrOfUpdates.Count;
                    local.EabReportSend.RptDetail =
                      "Critical Error: Not Unique updating Control Table for " +
                      entities.OutboundEftNumber.Identifier;

                    break;
                  case ErrorCode.PermittedValueViolation:
                    local.EabReportSend.RptDetail =
                      "Critical Error: Permitted Value violation updating Control Table for " +
                      entities.OutboundEftNumber.Identifier;

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              try
              {
                UpdateControlTable2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ++local.NbrOfUpdates.Count;
                    local.EabReportSend.RptDetail =
                      "Critical Error: Not Unique updating Control Table for " +
                      entities.OutboundEftTraceNumber.Identifier;

                    break;
                  case ErrorCode.PermittedValueViolation:
                    local.EabReportSend.RptDetail =
                      "Critical Error: Permitted Value violation updating Control Table for " +
                      entities.OutboundEftTraceNumber.Identifier;

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                // ***** The commit did not work so write out an error and 
                // abort.
                local.EabReportSend.RptDetail =
                  "Error:  External to do a commit returned a code of " + NumberToString
                  (local.PassArea.NumericReturnCode, 14, 2);
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                break;
              }

              // Update totals with trans just committed to database.
              local.TotCountsAmounts.TotNbrOfDisbTransRead.Count += local.
                CountsAmounts.NbrOfDisbTransRead.Count;
              local.TotCountsAmounts.TotAmtOfDisbTransRead.Number112 += local.
                CountsAmounts.AmtOfDisbTransRead.Number112;
              local.TotCountsAmounts.TotNbrOfDisbTransUpdate.Count += local.
                CountsAmounts.NbrOfDisbTransUpdated.Count;
              local.TotCountsAmounts.TotAmtOfDisbTransUpdate.Number112 += local.
                CountsAmounts.AmtOfDisbTransUpdated.Number112;
              local.TotCountsAmounts.TotNbrOfWarrantsCreated.Count += local.
                CountsAmounts.NbrOfWarrantsCreated.Count;
              local.TotCountsAmounts.TotAmtOfWarrantsCreated.Number112 += local.
                CountsAmounts.AmtOfWarrantsCreated.Number112;
              local.TotCountsAmounts.TotNbrOfEftsCreated.Count += local.
                CountsAmounts.NbrOfEftsCreated.Count;
              local.TotCountsAmounts.TotAmtOfEftsCreated.Number112 += local.
                CountsAmounts.AmtOfEftsCreated.Number112;
              local.TotCountsAmounts.TotNbrOfRecapturesCreate.Count += local.
                CountsAmounts.NbrOfRecapturesCreated.Count;
              local.TotCountsAmounts.TotAmtOfRecpaturesCreate.
                Number112 += local.CountsAmounts.AmtOfRecapturesCreated.
                  Number112;
              local.TotCountsAmounts.TotNbrOfRecoveriesCreate.Count += local.
                CountsAmounts.NbrOfRecoveriesCreated.Count;
              local.TotCountsAmounts.TotAmtOfRecoveriesCreate.
                Number112 += local.CountsAmounts.AmtOfRecoveriesCreated.
                  Number112;
              local.CountsAmounts.NbrOfDisbTransRead.Count = 0;
              local.CountsAmounts.NbrOfDisbTransUpdated.Count = 0;
              local.CountsAmounts.NbrOfWarrantsCreated.Count = 0;
              local.CountsAmounts.NbrOfEftsCreated.Count = 0;
              local.CountsAmounts.NbrOfRecapturesCreated.Count = 0;
              local.CountsAmounts.NbrOfRecoveriesCreated.Count = 0;
              local.CountsAmounts.AmtOfDisbTransRead.Number112 = 0;
              local.CountsAmounts.AmtOfDisbTransUpdated.Number112 = 0;
              local.CountsAmounts.AmtOfWarrantsCreated.Number112 = 0;
              local.CountsAmounts.AmtOfEftsCreated.Number112 = 0;
              local.CountsAmounts.AmtOfRecapturesCreated.Number112 = 0;
              local.CountsAmounts.AmtOfRecoveriesCreated.Number112 = 0;
            }
          }
        }

        // ---------------------------------------------------------------------
        // Start the processing for the new obligee:
        // Determine Payment method and create a new payment request.
        // ---------------------------------------------------------------------
        local.PreviousPayee.Number = entities.ObligeeCsePerson.Number;
        local.PaymentRequest.Amount = 0;
        UseFnCreateWarOrEftPmtReq();

        if (!IsEmpty(local.EabReportSend.RptDetail))
        {
          break;
        }

        if (!ReadPaymentRequest())
        {
          local.EabReportSend.RptDetail =
            "Not found reading payment request for Payee " + entities
            .ObligeeCsePerson.Number + " and payment req # " + NumberToString
            (local.ForRead.SystemGeneratedIdentifier, 15) + " disb # " + NumberToString
            (entities.Debit.SystemGeneratedIdentifier, 15);
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.PaymentRequest.Type1 = entities.PaymentRequest.Type1;

        if (Equal(entities.PaymentRequest.Type1, "EFT"))
        {
          MoveDisbursementTransaction2(entities.Debit, local.FirstDisbForPmntReq);
            
        }
      }

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------
        // For testing only  -  Process a disbursement
        // -----------------------------------------------------------------------
        local.BatchTimestampWorkArea.IefTimestamp = Now();
        local.BatchTimestampWorkArea.TextTimestamp = "";
        UseLeCabConvertTimestamp();
        ReadDisbursementType();
        local.EabReportSend.RptDetail =
          local.BatchTimestampWorkArea.TextTimestamp + " Payee " + entities
          .ObligeeCsePerson.Number + " Disb # " + NumberToString
          (entities.Debit.SystemGeneratedIdentifier, 15) + " Type " + entities
          .TempForDisplays.Code + " Amt " + NumberToString
          ((long)(entities.Debit.Amount * 100), 7, 9);
        UseCabControlReport();
      }

      // --------------------------------------------------------------
      // The following AB associates the disbursement transaction with the 
      // payment request and sets the status to "PROCESSED".
      // --------------------------------------------------------------
      UseFnProcesDebitDisbForPmtReq();

      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
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
    if (AsChar(local.FirstTime.Flag) != 'Y' && IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (local.PaymentRequest.Amount >= 0)
      {
        // ----------------------------------------------------------------------
        // 1  -  Check for and possibly Recapture recovery debts from the 
        // obligee.
        // ----------------------------------------------------------------------
        local.ObligeeForPmtRequest.Number =
          entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
        UseFnRcapProcessRecapture();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test1;
        }

        if (local.Recaptured.Amount > 0)
        {
          ++local.CountsAmounts.NbrOfRecapturesCreated.Count;
          local.CountsAmounts.AmtOfRecapturesCreated.Number112 += local.
            Recaptured.Amount;
          local.PaymentRequest.Amount -= local.Recaptured.Amount;
        }

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
      else
      {
        // ------------------------------------------------------------------------------
        // 2  -  Change the Payment Request to Potential Recovery (RCV) from 
        // Warrant (WAR) or EFT.
        // -----------------------------------------------------------------------------
        local.PaymentRequest.Type1 = "RCV";
        local.ObligeeForPmtRequest.Number =
          entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
        ++local.CountsAmounts.NbrOfRecoveriesCreated.Count;
        local.CountsAmounts.AmtOfRecoveriesCreated.Number112 += -
          local.PaymentRequest.Amount;
      }

      // ------------------------------------------------------------------------------
      // 3  -  Update the Payment Request as needed.
      // ------------------------------------------------------------------------------
      UseFnCompletePaymentRequest();

      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        goto Test1;
      }

      // ------------------------------------------------------------------------------
      // 4  -  Create an EFT Transmission Record if needed.
      // ------------------------------------------------------------------------------
      if (Equal(local.PaymentRequest.Type1, "EFT") && local
        .PaymentRequest.Amount > 0)
      {
        if (AsChar(local.DisplayInd.Flag) == 'Y')
        {
          // -----------------------------------------------------------------------
          // For testing only  -  Process a disbursement
          // -----------------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Local_next_eft_id eft transmission_identifier = " + NumberToString
            (local.NextEftId.TransmissionIdentifier, 15);
          UseCabControlReport();
          local.EabReportSend.RptDetail =
            "Local_next_eft_id eft trace_number = " + NumberToString
            (local.NextEftId.TraceNumber.GetValueOrDefault(), 15);
          UseCabControlReport();
        }

        UseFnCreateEftTransmissionRec();

        if (!IsEmpty(local.EabReportSend.RptDetail))
        {
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }
    }

Test1:

    // --- This IF statement is only an envelope to trap and process the errors.
    if (AsChar(local.Dummy.Flag) == 'Y' && IsExitState("ACO_NN0000_ALL_OK"))
    {
      try
      {
        UpdateControlTable1();
        ++local.NbrOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabReportSend.RptDetail =
              "Critical Error: Not Unique updating Control Table for " + entities
              .OutboundEftNumber.Identifier;
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto Test2;
          case ErrorCode.PermittedValueViolation:
            local.EabReportSend.RptDetail =
              "Critical Error: Permitted Value violation updating Control Table for " +
              entities.OutboundEftNumber.Identifier;
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto Test2;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        UpdateControlTable2();
        ++local.NbrOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabReportSend.RptDetail =
              "Critical Error: Not Unique updating Control Table for " + entities
              .OutboundEftTraceNumber.Identifier;
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          case ErrorCode.PermittedValueViolation:
            local.EabReportSend.RptDetail =
              "Critical Error: Permitted Value violation updating Control Table for " +
              entities.OutboundEftTraceNumber.Identifier;
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

Test2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.AbendForTestInd.Flag) == 'Y')
      {
        // -----------------------------------------------------------------------
        // For testing only  - Abend to leave database unchanged
        // -----------------------------------------------------------------------
        local.EabReportSend.RptDetail =
          "Abend for test purposes to leave database unchanged.";
        ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
      }
      else
      {
        local.EabReportSend.RptDetail = "Finished processing.";
      }

      UseCabControlReport();

      // Update totals with trans about to be committed to database.
      local.TotCountsAmounts.TotNbrOfDisbTransRead.Count += local.CountsAmounts.
        NbrOfDisbTransRead.Count;
      local.TotCountsAmounts.TotAmtOfDisbTransRead.Number112 += local.
        CountsAmounts.AmtOfDisbTransRead.Number112;
      local.TotCountsAmounts.TotNbrOfDisbTransUpdate.Count += local.
        CountsAmounts.NbrOfDisbTransUpdated.Count;
      local.TotCountsAmounts.TotAmtOfDisbTransUpdate.Number112 += local.
        CountsAmounts.AmtOfDisbTransUpdated.Number112;
      local.TotCountsAmounts.TotNbrOfWarrantsCreated.Count += local.
        CountsAmounts.NbrOfWarrantsCreated.Count;
      local.TotCountsAmounts.TotAmtOfWarrantsCreated.Number112 += local.
        CountsAmounts.AmtOfWarrantsCreated.Number112;
      local.TotCountsAmounts.TotNbrOfEftsCreated.Count += local.CountsAmounts.
        NbrOfEftsCreated.Count;
      local.TotCountsAmounts.TotAmtOfEftsCreated.Number112 += local.
        CountsAmounts.AmtOfEftsCreated.Number112;
      local.TotCountsAmounts.TotNbrOfRecapturesCreate.Count += local.
        CountsAmounts.NbrOfRecapturesCreated.Count;
      local.TotCountsAmounts.TotAmtOfRecpaturesCreate.Number112 += local.
        CountsAmounts.AmtOfRecapturesCreated.Number112;
      local.TotCountsAmounts.TotNbrOfRecoveriesCreate.Count += local.
        CountsAmounts.NbrOfRecoveriesCreated.Count;
      local.TotCountsAmounts.TotAmtOfRecoveriesCreate.Number112 += local.
        CountsAmounts.AmtOfRecoveriesCreated.Number112;
    }
    else
    {
      UseCabErrorReport1();
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport1();
      local.EabReportSend.RptDetail = "CSE Person -";
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + entities
        .ObligeeCsePerson.Number;
      UseCabErrorReport1();
      local.EabReportSend.RptDetail = "Disbursement Transaction -";
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (entities.Debit.SystemGeneratedIdentifier, 15);
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    UseFnPrintPmtReqCntrlTotals();
    UseSiCloseAdabas();
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
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

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.TraceNumber = source.TraceNumber;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private static void MoveTotCountsAmounts(Local.TotCountsAmountsGroup source,
    FnPrintPmtReqCntrlTotals.Import.CountsGroup target)
  {
    target.NbrOfDisbTransRead.Count = source.TotNbrOfDisbTransRead.Count;
    target.AmtOfDisbTransRead.Number112 =
      source.TotAmtOfDisbTransRead.Number112;
    target.NbrOfDisbTransUpdated.Count = source.TotNbrOfDisbTransUpdate.Count;
    target.AmtOfDisbTransUpdated.Number112 =
      source.TotAmtOfDisbTransUpdate.Number112;
    target.NbrOfWarrantsCreated.Count = source.TotNbrOfWarrantsCreated.Count;
    target.AmtOfWarrantsCreated.Number112 =
      source.TotAmtOfWarrantsCreated.Number112;
    target.NbrOfEftsCreated.Count = source.TotNbrOfEftsCreated.Count;
    target.AmtOfEftsCreated.Number112 = source.TotAmtOfEftsCreated.Number112;
    target.NbrOfRecapturesCreated.Count = source.TotNbrOfRecapturesCreate.Count;
    target.AmtOfRecapturesCreated.Number112 =
      source.TotAmtOfRecpaturesCreate.Number112;
    target.NbrOfRecoveriesCreated.Count = source.TotNbrOfRecoveriesCreate.Count;
    target.AmtOfRecoveriesCreated.Number112 =
      source.TotAmtOfRecoveriesCreate.Number112;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseFnCompletePaymentRequest()
  {
    var useImport = new FnCompletePaymentRequest.Import();
    var useExport = new FnCompletePaymentRequest.Export();

    useImport.Obligee.Number = local.ObligeeForPmtRequest.Number;
    useImport.Persistent.Assign(entities.PaymentRequest);
    MovePaymentRequest(local.PaymentRequest, useImport.ForUpdate);
    useImport.PersistentRequested.Assign(entities.Requested);
    useImport.PersistentRcvpot.Assign(entities.Rcvpot);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Maximum.Date = local.Maximum.Date;
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

  private void UseFnCreateEftTransmissionRec()
  {
    var useImport = new FnCreateEftTransmissionRec.Import();
    var useExport = new FnCreateEftTransmissionRec.Export();

    useImport.Obligee.Number = local.ObligeeForPmtRequest.Number;
    useImport.Persistent.Assign(entities.PaymentRequest);
    useImport.DesignatedPayee.Number = local.DesignatedPayee.Number;
    MoveDisbursementTransaction2(local.FirstDisbForPmntReq,
      useImport.FirstDisbForPmtReq);
    useImport.PersonPreferredPaymentMethod.Assign(
      local.PersonPreferredPaymentMethod);
    MoveElectronicFundTransmission(local.NextEftId, useImport.ExportNext);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(FnCreateEftTransmissionRec.Execute, useImport, useExport);

    entities.PaymentRequest.Assign(useImport.Persistent);
    MoveElectronicFundTransmission(useImport.ExportNext, local.NextEftId);
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnCreateWarOrEftPmtReq()
  {
    var useImport = new FnCreateWarOrEftPmtReq.Import();
    var useExport = new FnCreateWarOrEftPmtReq.Export();

    useImport.Obligee.Number = entities.ObligeeCsePerson.Number;
    useImport.PaymentRequest.InterstateInd = local.PaymentRequest.InterstateInd;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Debit.SystemGeneratedIdentifier =
      entities.Debit.SystemGeneratedIdentifier;

    Call(FnCreateWarOrEftPmtReq.Execute, useImport, useExport);

    local.DesignatedPayee.Number = useExport.DesignatedPayee.Number;
    local.PersonPreferredPaymentMethod.Assign(
      useExport.PersonPreferredPaymentMethod);
    local.ForRead.SystemGeneratedIdentifier =
      useExport.PaymentRequest.SystemGeneratedIdentifier;
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnPmtRequestHousekeeping()
  {
    var useImport = new FnPmtRequestHousekeeping.Import();
    var useExport = new FnPmtRequestHousekeeping.Export();

    useExport.PersistentProcessed.Assign(entities.Processed);
    useExport.PersistentRequested.Assign(entities.Requested);
    useExport.PersistentRcvpot.Assign(entities.Rcvpot);
    useExport.PersistentCanceled.Assign(entities.Canceled);
    useExport.PersistentDenied.Assign(entities.Denied);

    Call(FnPmtRequestHousekeeping.Execute, useImport, useExport);

    entities.Processed.SystemGeneratedIdentifier =
      useExport.PersistentProcessed.SystemGeneratedIdentifier;
    entities.Requested.SystemGeneratedIdentifier =
      useExport.PersistentRequested.SystemGeneratedIdentifier;
    entities.Rcvpot.SystemGeneratedIdentifier =
      useExport.PersistentRcvpot.SystemGeneratedIdentifier;
    local.ProgramProcessingInfo.ProcessDate =
      useExport.ProgramProcessingInfo.ProcessDate;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    local.TestPayee.Number = useExport.TestPayee.Number;
    local.Maximum.Date = useExport.Maximum.Date;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    local.AbendForTestInd.Flag = useExport.AbendForTestInd.Flag;
    entities.Canceled.SystemGeneratedIdentifier =
      useExport.PersistentCanceled.SystemGeneratedIdentifier;
    entities.Denied.SystemGeneratedIdentifier =
      useExport.PersistentDenied.SystemGeneratedIdentifier;
  }

  private void UseFnPrintPmtReqCntrlTotals()
  {
    var useImport = new FnPrintPmtReqCntrlTotals.Import();
    var useExport = new FnPrintPmtReqCntrlTotals.Export();

    MoveTotCountsAmounts(local.TotCountsAmounts, useImport.Counts);

    Call(FnPrintPmtReqCntrlTotals.Execute, useImport, useExport);
  }

  private void UseFnProcesDebitDisbForPmtReq()
  {
    var useImport = new FnProcesDebitDisbForPmtReq.Import();
    var useExport = new FnProcesDebitDisbForPmtReq.Export();

    useImport.PersistentObligee.Assign(entities.ObligeeCsePerson);
    useImport.PersistentPaymentRequest.Assign(entities.PaymentRequest);
    useImport.PersistentDebit.Assign(entities.Debit);
    useImport.PersistentDisbursementStatusHistory.Assign(
      entities.DisbursementStatusHistory);
    useImport.PersistentProcessed.Assign(entities.Processed);
    useImport.Maximum.Date = local.Maximum.Date;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(FnProcesDebitDisbForPmtReq.Execute, useImport, useExport);

    entities.ObligeeCsePerson.Number = useImport.PersistentObligee.Number;
    entities.PaymentRequest.Assign(useImport.PersistentPaymentRequest);
    MoveDisbursementTransaction1(useImport.PersistentDebit, entities.Debit);
    MoveDisbursementStatusHistory(useImport.PersistentDisbursementStatusHistory,
      entities.DisbursementStatusHistory);
    entities.Processed.SystemGeneratedIdentifier =
      useImport.PersistentProcessed.SystemGeneratedIdentifier;
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnRcapProcessRecapture()
  {
    var useImport = new FnRcapProcessRecapture.Import();
    var useExport = new FnRcapProcessRecapture.Export();

    useImport.Obligee.Number = local.ObligeeForPmtRequest.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.PaymentRequest.SystemGeneratedIdentifier =
      entities.PaymentRequest.SystemGeneratedIdentifier;

    Call(FnRcapProcessRecapture.Execute, useImport, useExport);

    local.Recaptured.Amount = useExport.Recap.Amount;
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

  private bool ReadControlTable1()
  {
    entities.OutboundEftNumber.Populated = false;

    return Read("ReadControlTable1",
      null,
      (db, reader) =>
      {
        entities.OutboundEftNumber.Identifier = db.GetString(reader, 0);
        entities.OutboundEftNumber.LastUsedNumber = db.GetInt32(reader, 1);
        entities.OutboundEftNumber.Populated = true;
      });
  }

  private bool ReadControlTable2()
  {
    entities.OutboundEftTraceNumber.Populated = false;

    return Read("ReadControlTable2",
      null,
      (db, reader) =>
      {
        entities.OutboundEftTraceNumber.Identifier = db.GetString(reader, 0);
        entities.OutboundEftTraceNumber.LastUsedNumber = db.GetInt32(reader, 1);
        entities.OutboundEftTraceNumber.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCsePersonCsePersonAccountDisbursementTransaction()
  {
    entities.ObligeeCsePerson.Populated = false;
    entities.ObligeeCsePersonAccount.Populated = false;
    entities.Debit.Populated = false;
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadCsePersonCsePersonAccountDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate",
          local.Initialized.ProcessDate.GetValueOrDefault());
        db.
          SetString(command, "type", local.Hardcoded.HardcodeDisbursement.Type1);
          
      },
      (db, reader) =>
      {
        entities.ObligeeCsePerson.Number = db.GetString(reader, 0);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 0);
        entities.ObligeeCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.Debit.CpaType = db.GetString(reader, 1);
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
        entities.Debit.ExcessUraInd = db.GetNullableString(reader, 14);
        entities.ObligeeCsePerson.Populated = true;
        entities.ObligeeCsePersonAccount.Populated = true;
        entities.Debit.Populated = true;
        entities.DisbursementType.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligeeCsePersonAccount.Type1);
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Debit.CashNonCashInd);

        return true;
      });
  }

  private bool ReadDisbursementStatusHistoryDisbursementStatus()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.DisbursementStatus.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;

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
        entities.DisbursementStatus.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private bool ReadDisbursementType()
  {
    entities.TempForDisplays.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          entities.DisbursementType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.TempForDisplays.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.TempForDisplays.Code = db.GetString(reader, 1);
        entities.TempForDisplays.Populated = true;
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

  private void UpdateControlTable1()
  {
    var lastUsedNumber = local.NextEftId.TransmissionIdentifier - 1;

    entities.OutboundEftNumber.Populated = false;
    Update("UpdateControlTable1",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.
          SetString(command, "cntlTblId", entities.OutboundEftNumber.Identifier);
          
      });

    entities.OutboundEftNumber.LastUsedNumber = lastUsedNumber;
    entities.OutboundEftNumber.Populated = true;
  }

  private void UpdateControlTable2()
  {
    var lastUsedNumber =
      (int)local.NextEftId.TraceNumber.GetValueOrDefault() - 1;

    entities.OutboundEftTraceNumber.Populated = false;
    Update("UpdateControlTable2",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(
          command, "cntlTblId", entities.OutboundEftTraceNumber.Identifier);
      });

    entities.OutboundEftTraceNumber.LastUsedNumber = lastUsedNumber;
    entities.OutboundEftTraceNumber.Populated = true;
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
    /// <summary>A HardcodedGroup group.</summary>
    [Serializable]
    public class HardcodedGroup
    {
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
      /// A value of HardcodeCrFee.
      /// </summary>
      [JsonPropertyName("hardcodeCrFee")]
      public DisbursementType HardcodeCrFee
      {
        get => hardcodeCrFee ??= new();
        set => hardcodeCrFee = value;
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

      private DisbursementTransaction hardcodeDisbursement;
      private DisbursementType hardcodePassthru;
      private DisbursementType hardcodeCrFee;
      private DisbursementStatus hardcodeReleased;
    }

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

    /// <summary>A TotCountsAmountsGroup group.</summary>
    [Serializable]
    public class TotCountsAmountsGroup
    {
      /// <summary>
      /// A value of TotNbrOfDisbTransRead.
      /// </summary>
      [JsonPropertyName("totNbrOfDisbTransRead")]
      public Common TotNbrOfDisbTransRead
      {
        get => totNbrOfDisbTransRead ??= new();
        set => totNbrOfDisbTransRead = value;
      }

      /// <summary>
      /// A value of TotAmtOfDisbTransRead.
      /// </summary>
      [JsonPropertyName("totAmtOfDisbTransRead")]
      public NumericWorkSet TotAmtOfDisbTransRead
      {
        get => totAmtOfDisbTransRead ??= new();
        set => totAmtOfDisbTransRead = value;
      }

      /// <summary>
      /// A value of TotNbrOfDisbTransUpdate.
      /// </summary>
      [JsonPropertyName("totNbrOfDisbTransUpdate")]
      public Common TotNbrOfDisbTransUpdate
      {
        get => totNbrOfDisbTransUpdate ??= new();
        set => totNbrOfDisbTransUpdate = value;
      }

      /// <summary>
      /// A value of TotAmtOfDisbTransUpdate.
      /// </summary>
      [JsonPropertyName("totAmtOfDisbTransUpdate")]
      public NumericWorkSet TotAmtOfDisbTransUpdate
      {
        get => totAmtOfDisbTransUpdate ??= new();
        set => totAmtOfDisbTransUpdate = value;
      }

      /// <summary>
      /// A value of TotNbrOfWarrantsCreated.
      /// </summary>
      [JsonPropertyName("totNbrOfWarrantsCreated")]
      public Common TotNbrOfWarrantsCreated
      {
        get => totNbrOfWarrantsCreated ??= new();
        set => totNbrOfWarrantsCreated = value;
      }

      /// <summary>
      /// A value of TotAmtOfWarrantsCreated.
      /// </summary>
      [JsonPropertyName("totAmtOfWarrantsCreated")]
      public NumericWorkSet TotAmtOfWarrantsCreated
      {
        get => totAmtOfWarrantsCreated ??= new();
        set => totAmtOfWarrantsCreated = value;
      }

      /// <summary>
      /// A value of TotNbrOfEftsCreated.
      /// </summary>
      [JsonPropertyName("totNbrOfEftsCreated")]
      public Common TotNbrOfEftsCreated
      {
        get => totNbrOfEftsCreated ??= new();
        set => totNbrOfEftsCreated = value;
      }

      /// <summary>
      /// A value of TotAmtOfEftsCreated.
      /// </summary>
      [JsonPropertyName("totAmtOfEftsCreated")]
      public NumericWorkSet TotAmtOfEftsCreated
      {
        get => totAmtOfEftsCreated ??= new();
        set => totAmtOfEftsCreated = value;
      }

      /// <summary>
      /// A value of TotNbrOfRecapturesCreate.
      /// </summary>
      [JsonPropertyName("totNbrOfRecapturesCreate")]
      public Common TotNbrOfRecapturesCreate
      {
        get => totNbrOfRecapturesCreate ??= new();
        set => totNbrOfRecapturesCreate = value;
      }

      /// <summary>
      /// A value of TotAmtOfRecpaturesCreate.
      /// </summary>
      [JsonPropertyName("totAmtOfRecpaturesCreate")]
      public NumericWorkSet TotAmtOfRecpaturesCreate
      {
        get => totAmtOfRecpaturesCreate ??= new();
        set => totAmtOfRecpaturesCreate = value;
      }

      /// <summary>
      /// A value of TotNbrOfRecoveriesCreate.
      /// </summary>
      [JsonPropertyName("totNbrOfRecoveriesCreate")]
      public Common TotNbrOfRecoveriesCreate
      {
        get => totNbrOfRecoveriesCreate ??= new();
        set => totNbrOfRecoveriesCreate = value;
      }

      /// <summary>
      /// A value of TotAmtOfRecoveriesCreate.
      /// </summary>
      [JsonPropertyName("totAmtOfRecoveriesCreate")]
      public NumericWorkSet TotAmtOfRecoveriesCreate
      {
        get => totAmtOfRecoveriesCreate ??= new();
        set => totAmtOfRecoveriesCreate = value;
      }

      private Common totNbrOfDisbTransRead;
      private NumericWorkSet totAmtOfDisbTransRead;
      private Common totNbrOfDisbTransUpdate;
      private NumericWorkSet totAmtOfDisbTransUpdate;
      private Common totNbrOfWarrantsCreated;
      private NumericWorkSet totAmtOfWarrantsCreated;
      private Common totNbrOfEftsCreated;
      private NumericWorkSet totAmtOfEftsCreated;
      private Common totNbrOfRecapturesCreate;
      private NumericWorkSet totAmtOfRecpaturesCreate;
      private Common totNbrOfRecoveriesCreate;
      private NumericWorkSet totAmtOfRecoveriesCreate;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public MonthlyObligeeSummary ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
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
    /// A value of ObligeeForPmtRequest.
    /// </summary>
    [JsonPropertyName("obligeeForPmtRequest")]
    public CsePerson ObligeeForPmtRequest
    {
      get => obligeeForPmtRequest ??= new();
      set => obligeeForPmtRequest = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of PreviousPayee.
    /// </summary>
    [JsonPropertyName("previousPayee")]
    public CsePerson PreviousPayee
    {
      get => previousPayee ??= new();
      set => previousPayee = value;
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
    /// A value of NextEftId.
    /// </summary>
    [JsonPropertyName("nextEftId")]
    public ElectronicFundTransmission NextEftId
    {
      get => nextEftId ??= new();
      set => nextEftId = value;
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
    /// A value of Recaptured.
    /// </summary>
    [JsonPropertyName("recaptured")]
    public PaymentRequest Recaptured
    {
      get => recaptured ??= new();
      set => recaptured = value;
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
    /// A value of TestPayee.
    /// </summary>
    [JsonPropertyName("testPayee")]
    public CsePerson TestPayee
    {
      get => testPayee ??= new();
      set => testPayee = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of LeftPadZero.
    /// </summary>
    [JsonPropertyName("leftPadZero")]
    public TextWorkArea LeftPadZero
    {
      get => leftPadZero ??= new();
      set => leftPadZero = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DisbursementTransaction Initialized
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
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
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
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
    /// A value of AbendForTestInd.
    /// </summary>
    [JsonPropertyName("abendForTestInd")]
    public Common AbendForTestInd
    {
      get => abendForTestInd ??= new();
      set => abendForTestInd = value;
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
    /// Gets a value of Hardcoded.
    /// </summary>
    [JsonPropertyName("hardcoded")]
    public HardcodedGroup Hardcoded
    {
      get => hardcoded ?? (hardcoded = new());
      set => hardcoded = value;
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
    /// Gets a value of TotCountsAmounts.
    /// </summary>
    [JsonPropertyName("totCountsAmounts")]
    public TotCountsAmountsGroup TotCountsAmounts
    {
      get => totCountsAmounts ?? (totCountsAmounts = new());
      set => totCountsAmounts = value;
    }

    private MonthlyObligeeSummary forUpdate;
    private Common nbrOfUpdates;
    private CsePerson obligeeForPmtRequest;
    private PaymentRequest forRead;
    private TextWorkArea textWorkArea;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private CsePerson designatedPayee;
    private CsePerson previousPayee;
    private DisbursementTransaction firstDisbForPmntReq;
    private ElectronicFundTransmission nextEftId;
    private PaymentRequest paymentRequest;
    private PaymentRequest recaptured;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private CsePerson testPayee;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private TextWorkArea leftPadZero;
    private ExitStateWorkArea exitStateWorkArea;
    private DisbursementTransaction initialized;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private DateWorkArea maximum;
    private Common createAttempts;
    private Common firstTime;
    private Common dummy;
    private Common displayInd;
    private Common abendForTestInd;
    private Common nbrOfDisbCreatedInRecap;
    private HardcodedGroup hardcoded;
    private CountsAmountsGroup countsAmounts;
    private TotCountsAmountsGroup totCountsAmounts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligeeForPsumUpdate.
    /// </summary>
    [JsonPropertyName("obligeeForPsumUpdate")]
    public CsePersonAccount ObligeeForPsumUpdate
    {
      get => obligeeForPsumUpdate ??= new();
      set => obligeeForPsumUpdate = value;
    }

    /// <summary>
    /// A value of TempForDisplays.
    /// </summary>
    [JsonPropertyName("tempForDisplays")]
    public DisbursementType TempForDisplays
    {
      get => tempForDisplays ??= new();
      set => tempForDisplays = value;
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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of Requested.
    /// </summary>
    [JsonPropertyName("requested")]
    public PaymentStatus Requested
    {
      get => requested ??= new();
      set => requested = value;
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
    /// A value of OutboundEftTraceNumber.
    /// </summary>
    [JsonPropertyName("outboundEftTraceNumber")]
    public ControlTable OutboundEftTraceNumber
    {
      get => outboundEftTraceNumber ??= new();
      set => outboundEftTraceNumber = value;
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

    private CsePersonAccount obligeeForPsumUpdate;
    private DisbursementType tempForDisplays;
    private CsePerson obligeeCsePerson;
    private CsePersonAccount obligeeCsePersonAccount;
    private DisbursementTransaction debit;
    private DisbursementType disbursementType;
    private DisbursementStatus disbursementStatus;
    private DisbursementStatus processed;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransactionRln disbursementTransactionRln;
    private PaymentRequest paymentRequest;
    private PaymentStatus rcvpot;
    private PaymentStatus requested;
    private PaymentStatus canceled;
    private PaymentStatus denied;
    private ControlTable outboundEftTraceNumber;
    private ControlTable outboundEftNumber;
  }
#endregion
}
