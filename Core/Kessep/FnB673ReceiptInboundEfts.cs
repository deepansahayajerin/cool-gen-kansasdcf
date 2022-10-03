// Program: FN_B673_RECEIPT_INBOUND_EFTS, ID: 372404270, model: 746.
// Short name: SWEF673B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B673_RECEIPT_INBOUND_EFTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB673ReceiptInboundEfts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B673_RECEIPT_INBOUND_EFTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB673ReceiptInboundEfts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB673ReceiptInboundEfts.
  /// </summary>
  public FnB673ReceiptInboundEfts(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    // Date      Developers Name         Request #  Description
    // --------  ----------------------  ---------  ----------------------
    // 11/02/98  Mike Fangman                       Rewritten for new 
    // requirements
    // 01/26/00  Mike Fangman    PR 82289  Change logic to match FDSO EFT by 
    // year and month only.
    // 03/03/00  Mike Fangman    PR ?????  Added Not Found clause to Read 
    // statement.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.NumericString.Text10 = "0123456789";
    local.MaximumDiscontinueDate.Date = UseCabSetMaximumDiscontinueDate();
    UseFnB673Housekeeping();
    local.EabFileHandling.Action = "WRITE";

    if (!IsExitState("ACO_NN0000_ALL_OK") || !
      IsEmpty(local.EabReportSend.RptDetail))
    {
      UseCabErrorReport();
      UseEabExtractExitStateMessage1();
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (ReadFundTransaction())
    {
      local.FundTransaction.Amount = entities.FundTransaction.Amount;
      ++local.Totals.NbrOfReads.Count;
    }

    if (!entities.FundTransaction.Populated)
    {
      local.EabReportSend.RptDetail =
        "Not Found reading Fund Transaction w/ a type of " + NumberToString
        (local.HardcodedDeposited.SystemGeneratedIdentifier, 15);
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (ReadControlTable())
    {
      ++local.Totals.NbrOfReads.Count;
      local.NextCashReceiptSeqNum.LastUsedNumber =
        entities.CashReceiptSequentialNum.LastUsedNumber;

      if (AsChar(local.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "Last Cash Receipt number used was " + NumberToString
          (entities.CashReceiptSequentialNum.LastUsedNumber, 15);
        UseCabControlReport();
      }
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Critical Error:  Received a Not Found trying to read the control table to get the next Cash Receipt number.";
        
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Process each Inbound EFT row that is in a "released" status.
    foreach(var item in ReadElectronicFundTransmission())
    {
      ++local.Totals.NbrOfReads.Count;
      ++local.Totals.NbrOfEftsRead.Count;
      local.Totals.AmtOfEftRecordsRead.TotalCurrency += entities.
        ElectronicFundTransmission.TransmittalAmount;
      local.Current.Timestamp = Now();
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        entities.Released.SystemGeneratedIdentifier;
      local.ElectronicFundTransmission.TransmissionProcessDate =
        local.Null1.Date;
      local.TextEftIdentifier.Text9 =
        NumberToString(entities.ElectronicFundTransmission.
          TransmissionIdentifier, 7, 9);

      if (AsChar(local.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "";
        UseCabControlReport();
        local.EabReportSend.RptDetail = "Read inbound EFT w/ ID of " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
        UseCabControlReport();
      }

      local.EabReportSend.RptDetail = "";

      // Determine the cash receipt source type.
      UseFnDetermineCrSourceType();

      if (IsEmpty(local.EabReportSend.RptDetail))
      {
        // There were no errors (and no error msg) so fall thru and continue.
        if (local.CashReceiptSourceType.SystemGeneratedIdentifier > 0)
        {
          if (local.CashReceiptSourceType.SystemGeneratedIdentifier == entities
            .CashReceiptSourceType.SystemGeneratedIdentifier)
          {
            // The current cash receipt source type is the one needed for the 
            // current EFT transmission record.
          }
          else if (ReadCashReceiptSourceType3())
          {
            ++local.Totals.NbrOfReads.Count;
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Not Found reading Cash Receipt Source Type with a system generated ID of" +
              NumberToString
              (local.CashReceiptSourceType.SystemGeneratedIdentifier, 15);
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
        else if (!IsEmpty(local.CashReceiptSourceType.Code))
        {
          if (Equal(local.CashReceiptSourceType.Code,
            entities.CashReceiptSourceType.Code))
          {
            // The current cash receipt source type is the one needed for the 
            // current EFT transmission record.
          }
          else if (ReadCashReceiptSourceType2())
          {
            ++local.Totals.NbrOfReads.Count;
          }
          else
          {
            local.EabReportSend.RptDetail = "ID " + local
              .TextEftIdentifier.Text9 + " Pending Error: Cash Receipt Source Code not found for Code of " +
              local.CashReceiptSourceType.Code + " with FIPS code of " + NumberToString
              (local.CashReceiptSourceType.State.GetValueOrDefault(), 14, 2);
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }
          }
        }
        else if (Equal(local.CashReceiptSourceType.State.GetValueOrDefault(),
          entities.CashReceiptSourceType.State) && Equal
          (local.CashReceiptSourceType.County.GetValueOrDefault(),
          entities.CashReceiptSourceType.County) && Equal
          (local.CashReceiptSourceType.Location.GetValueOrDefault(),
          entities.CashReceiptSourceType.Location))
        {
          // The current cash receipt source type is the one needed for the 
          // current EFT transmission record.
        }
        else if (ReadCashReceiptSourceType1())
        {
          ++local.Totals.NbrOfReads.Count;
        }
        else
        {
          local.EabReportSend.RptDetail = "ID " + local
            .TextEftIdentifier.Text9 + " Pending Error: Cash Receipt Source Code not found for FIPS code of " +
            NumberToString
            (local.CashReceiptSourceType.State.GetValueOrDefault(), 14, 2) + NumberToString
            (local.CashReceiptSourceType.County.GetValueOrDefault(), 13, 3) + NumberToString
            (local.CashReceiptSourceType.Location.GetValueOrDefault(), 14, 2);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // ***** Any error dealing with file handling is  "critical" and 
            // will result in an abend. *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.Totals.NbrOfPendingErrors.Count;
          local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
          UseFnUpdateEftStatus();

          if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
          {
            return;
          }
          else
          {
            continue;
          }
        }
      }
      else
      {
        // There was an error (and an error msg) so write out the error and pend
        // the EFT record.
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // ***** Any error dealing with file handling is  "critical" and will 
          // result in an abend. *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.Totals.NbrOfPendingErrors.Count;
        local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
        UseFnUpdateEftStatus();

        if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
        {
          return;
        }
        else
        {
          continue;
        }
      }

      if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) == 'Y')
      {
        // Match the cash receipt event.  
        // ------------------------------------------------------------
        // Try to match a pre-existing cash receipt event from an interface by 
        // source and date.
        if (Equal(entities.ElectronicFundTransmission.CompanyDescriptiveDate,
          local.InitializedDateWorkArea.Date))
        {
          local.EabReportSend.RptDetail = "ID " + local
            .TextEftIdentifier.Text9 + " Pending Error: Company Descriptive Date is blank.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // ***** Any error dealing with file handling is  "critical" and 
            // will result in an abend. *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.Totals.NbrOfPendingErrors.Count;
          local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
          UseFnUpdateEftStatus();

          if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
          {
            return;
          }
          else
          {
            continue;
          }
        }

        // If this EFT is FDSO (Cash Receipt Source Type of 1) then match by 
        // year and month.
        local.CashReceiptEvent.SourceCreationDate =
          entities.ElectronicFundTransmission.CompanyDescriptiveDate;

        if (entities.CashReceiptSourceType.SystemGeneratedIdentifier == 1)
        {
          local.TxtYyyymm.Text6 =
            NumberToString(DateToInt(
              entities.ElectronicFundTransmission.CompanyDescriptiveDate), 8,
            6);
          local.StartOfMonth.SourceCreationDate =
            StringToDate(Substring(
              local.TxtYyyymm.Text6, WorkArea.Text6_MaxLength, 1, 4) + "-" + Substring
            (local.TxtYyyymm.Text6, WorkArea.Text6_MaxLength, 5, 2) + "-01");
          local.EndOfMonth.SourceCreationDate =
            AddMonths(local.StartOfMonth.SourceCreationDate, 1);
          local.EndOfMonth.SourceCreationDate =
            AddDays(local.EndOfMonth.SourceCreationDate, -1);

          if (ReadCashReceiptEventCashReceiptCashReceiptStatusHistory1())
          {
            local.CashReceiptEvent.SourceCreationDate =
              entities.CashReceiptEvent.SourceCreationDate;
          }
          else
          {
            // Continue - there is no FDSO receipt to be found in the entire 
            // month.
          }
        }

        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "   2 About to read for matching receipt for " + entities
            .CashReceiptSourceType.Code + " & Co. Descr Date " + NumberToString
            (DateToInt(
              entities.ElectronicFundTransmission.CompanyDescriptiveDate), 15);
          UseCabControlReport();
        }

        if (ReadCashReceiptEventCashReceiptCashReceiptStatusHistory2())
        {
          ++local.Totals.NbrOfReads.Count;

          if (!IsEmpty(entities.ForInterfaceMatching.CheckNumber))
          {
            // This Cash Receipt has already been matched.  This is an error 
            // situation so write out an error msg and pend the EFT.
            local.WorkArea.Text15 =
              NumberToString(DateToInt(
                entities.ElectronicFundTransmission.CompanyDescriptiveDate),
              15);
            local.WorkArea.Text10 =
              Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 12, 2) +
              "/" + Substring
              (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2) + "/"
              + Substring
              (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 8, 4);
            local.EabReportSend.RptDetail = "ID " + local
              .TextEftIdentifier.Text9 + " Pending Error: Court Interface already matched for the date of " +
              local.WorkArea.Text10 + " from the source of " + entities
              .CashReceiptSourceType.Code;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }
          }

          if (AsChar(local.TraceIndicator.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "   Found matching receipt for court " + entities
              .CashReceiptSourceType.Code + " & Source Creation Date " + NumberToString
              (DateToInt(entities.CashReceiptEvent.SourceCreationDate), 15);
            UseCabControlReport();
            local.EabReportSend.RptDetail = "   Cash Receipt Event ID " + NumberToString
              (entities.CashReceiptEvent.SystemGeneratedIdentifier, 15);
            local.EabReportSend.RptDetail =
              "   Found matching receipt for court " + entities
              .CashReceiptSourceType.Code + " & Source Creation Date " + NumberToString
              (DateToInt(entities.CashReceiptEvent.SourceCreationDate), 15);
            UseCabControlReport();
          }

          // "Match" the EFT payment to the interface.
          local.ForInterfaceMatching.CashBalanceAmt =
            entities.ForInterfaceMatching.CashDue.GetValueOrDefault() - entities
            .ElectronicFundTransmission.TransmittalAmount;

          if (local.ForInterfaceMatching.CashBalanceAmt.GetValueOrDefault() < 0)
          {
            local.ForInterfaceMatching.CashBalanceReason = "OVER";
          }
          else if (local.ForInterfaceMatching.CashBalanceAmt.
            GetValueOrDefault() == 0)
          {
            local.ForInterfaceMatching.CashBalanceReason = "";
          }
          else
          {
            local.ForInterfaceMatching.CashBalanceReason = "UNDER";
          }

          if (Equal(entities.CashReceiptSourceType.State, 20) || entities
            .CashReceiptSourceType.SystemGeneratedIdentifier == 1)
          {
            local.ForInterfaceMatching.CheckDate =
              entities.ElectronicFundTransmission.CompanyDescriptiveDate;
          }
          else
          {
            local.ForInterfaceMatching.CheckDate =
              entities.ElectronicFundTransmission.EffectiveEntryDate;
          }

          // 5/24/99 - Added deposit release date & balance timestamp per Tim's 
          // msg on 4/26/99.
          // 8/9/99 - Took out the update of create_by to user_id per Tim's 
          // request 8/9/99.
          try
          {
            UpdateCashReceipt();

            if (AsChar(local.TraceIndicator.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "   Updated cash receipt, amount " + NumberToString
                ((long)entities.ForInterfaceMatching.ReceiptAmount, 15) + ", check date " +
                NumberToString
                (DateToInt(entities.ForInterfaceMatching.CheckDate), 15) + ", Check Number " +
                entities.ForInterfaceMatching.CheckNumber + ", payor org " + entities
                .ForInterfaceMatching.PayorOrganization + ".";
              UseCabControlReport();
            }

            try
            {
              UpdateCashReceiptStatusHistory();
              ++local.Totals.NbrOfUpdates.Count;
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  local.EabReportSend.RptDetail = "ID " + NumberToString
                    (entities.ElectronicFundTransmission.TransmissionIdentifier,
                    15) + " Critical Error: Already Exists updating the Cash Receipt Status History.";
                    
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.PermittedValueViolation:
                  local.EabReportSend.RptDetail = "ID " + NumberToString
                    (entities.ElectronicFundTransmission.TransmissionIdentifier,
                    15) + " Critical Error: Permitted Value updating the Cash Receipt Status History.";
                    
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            try
            {
              CreateCashReceiptStatusHistory2();
              ++local.Totals.NbrOfUpdates.Count;
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  local.EabReportSend.RptDetail = "ID " + NumberToString
                    (entities.ElectronicFundTransmission.TransmissionIdentifier,
                    15) + " Critical Error: Already Exists creating the Cash Receipt Status History.";
                    
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.PermittedValueViolation:
                  local.EabReportSend.RptDetail = "ID " + NumberToString
                    (entities.ElectronicFundTransmission.TransmissionIdentifier,
                    15) + " Critical Error: Permitted Value creating the Cash Receipt Status History.";
                    
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                local.EabReportSend.RptDetail = "ID " + NumberToString
                  (entities.ElectronicFundTransmission.TransmissionIdentifier,
                  15) + " Critical Error: Not Unique error updating (matching) the Cash Receipt for source " +
                  entities.CashReceiptSourceType.Code + " and date " + NumberToString
                  (DateToInt(
                    entities.ElectronicFundTransmission.CompanyDescriptiveDate),
                  15);
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              case ErrorCode.PermittedValueViolation:
                local.EabReportSend.RptDetail = "ID " + NumberToString
                  (entities.ElectronicFundTransmission.TransmissionIdentifier,
                  15) + " Critical Error: Perm Value violation updating/matching Cash Receipt source of " +
                  entities.CashReceiptSourceType.Code + " date of " + NumberToString
                  (DateToInt(
                    entities.ElectronicFundTransmission.CompanyDescriptiveDate),
                  15);
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.ElectronicFundTransmission.TransmissionStatusCode = "RECEIPTED";
          local.EabReportSend.RptDetail = "";
          local.ElectronicFundTransmission.TransmissionProcessDate =
            local.Process.Date;
          local.FundTransaction.Amount += entities.ElectronicFundTransmission.
            TransmittalAmount;
          ++local.Totals.NbrOfCrReceipted.Count;
          local.Totals.AmtOfCrReceipted.TotalCurrency += entities.
            ElectronicFundTransmission.TransmittalAmount;
          ++local.Totals.NbrOfIntCrMatched.Count;
          local.Totals.AmtOfIntCrMatched.TotalCurrency += entities.
            ElectronicFundTransmission.TransmittalAmount;
        }
        else
        {
          // No matching court interface cash receipt event was found so write 
          // out a msg & escape to create a cash receipt event and a cash
          // receipt.
          local.ElectronicFundTransmission.TransmissionStatusCode = "RECEIPTED";
          local.ElectronicFundTransmission.TransmissionProcessDate =
            local.Process.Date;
          local.WorkArea.Text15 =
            NumberToString(DateToInt(
              entities.ElectronicFundTransmission.CompanyDescriptiveDate), 15);
          local.WorkArea.Text10 =
            Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 12, 2) +
            "/" + Substring
            (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2) + "/" + Substring
            (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 8, 4);
          local.EabReportSend.RptDetail = "ID " + local
            .TextEftIdentifier.Text9 + " Message: Court Interface not matched for the date of " +
            local.WorkArea.Text10 + " from the source of " + entities
            .CashReceiptSourceType.Code;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // ***** Any error dealing with file handling is  "critical" and 
            // will result in an abend. *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          goto Test;
        }

        // We are done with trying to match the EFT payment.
        // Update the status of the EFT record.
        // Get the next EFT payment to process.
        if (IsEmpty(local.EabReportSend.RptDetail))
        {
          // There were no errors (and no error msg) so continue.
        }
        else
        {
          // There was an error (and an error msg) so write out the error and 
          // pend the EFT record.
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // ***** Any error dealing with file handling is  "critical" and 
            // will result in an abend. *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.Totals.NbrOfPendingErrors.Count;
          local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
        }

        UseFnUpdateEftStatus();

        if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
        {
          return;
        }
        else
        {
          // We are finished processing this "interface" EFT so go back and read
          // the next EFT for processing.
          continue;
        }
      }
      else
      {
        // This is not an interface so fall thru and create the cash receipt 
        // event, cash receipt and cash receipt detail.
      }

Test:

      // Create the cash receipt event.  
      // ------------------------------------------------------------
      if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) == 'Y')
      {
        local.ForCreate.TotalCashFeeAmt = 0;
        local.ForCreate.TotalCashAmt = 0;
        local.ForCreate.TotalCashTransactionCount = 0;
      }
      else
      {
        local.ForCreate.TotalCashFeeAmt =
          entities.ElectronicFundTransmission.CollectionAmount.
            GetValueOrDefault() - entities
          .ElectronicFundTransmission.TransmittalAmount;
        local.ForCreate.TotalCashAmt =
          entities.ElectronicFundTransmission.TransmittalAmount;
        local.ForCreate.TotalCashTransactionCount = 1;
      }

      local.RetryCount.Count = 0;

      do
      {
        try
        {
          CreateCashReceiptEvent();
          ++local.Totals.NbrOfUpdates.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          break;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0076_CASH_RCPT_EVENT_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail = "ID " + NumberToString
                (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                " Pending Error: Permitted Value error creating the Cash Receipt Event.";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                // ***** Any error dealing with file handling is  "critical" and
                // will result in an abend. *****
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              ++local.Totals.NbrOfPendingErrors.Count;
              local.ElectronicFundTransmission.TransmissionStatusCode =
                "PENDED";
              UseFnUpdateEftStatus();

              if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
              {
                return;
              }
              else
              {
                goto ReadEach;
              }

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      while(local.RetryCount.Count < 5);

      if (IsExitState("FN0076_CASH_RCPT_EVENT_AE"))
      {
        local.EabReportSend.RptDetail = "ID " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Pending Error: Already Exists creating the Cash Receipt Event.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // ***** Any error dealing with file handling is  "critical" and will 
          // result in an abend. *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.Totals.NbrOfPendingErrors.Count;
        local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
        UseFnUpdateEftStatus();

        if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
        {
          return;
        }
        else
        {
          continue;
        }
      }

      // Create the cash receipt.  
      // -----------------------------------------------------------------------
      ++local.NextCashReceiptSeqNum.LastUsedNumber;

      if (Equal(entities.CashReceiptSourceType.Code, "Y"))
      {
        local.ForUpdate.TotalCashTransactionAmount = 0;
        local.ForUpdate.TotalCashTransactionCount = 0;
      }
      else
      {
        local.ForUpdate.TotalCashTransactionAmount =
          entities.ElectronicFundTransmission.TransmittalAmount;
        local.ForUpdate.TotalCashTransactionCount = 1;
      }

      // 5/24/99 - Added deposit release date & balance timestamp per Tim's msg 
      // on 4/26/99.
      try
      {
        CreateCashReceipt();
        ++local.Totals.NbrOfUpdates.Count;
        ++local.Totals.NbrOfCrReceipted.Count;
        local.Totals.AmtOfCrReceipted.TotalCurrency += entities.CashReceipt.
          TotalCashTransactionAmount.GetValueOrDefault();

        if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) == 'Y')
        {
          ++local.Totals.NbrOfIntCrNotMatched.Count;
          local.Totals.AmtOfIntCrNotMatched.TotalCurrency += entities.
            ElectronicFundTransmission.TransmittalAmount;
        }
        else
        {
          ++local.Totals.NbrOfNonCrReceipted.Count;
          local.Totals.AmtOfNonCrReceipted.TotalCurrency += entities.
            ElectronicFundTransmission.TransmittalAmount;
        }

        ++local.Totals.NbrOfUpdates.Count;

        // Update the deposit (fund transaction).
        local.FundTransaction.Amount += entities.ElectronicFundTransmission.
          TransmittalAmount;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabReportSend.RptDetail = "ID " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " Pending Error: Already Exists creating the Cash Receipt Number " +
              NumberToString(local.NextCashReceiptSeqNum.LastUsedNumber, 7, 9);
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            local.EabReportSend.RptDetail = "ID " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " Pending Error: Permitted Value creating the Cash Receipt.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // Set the status of the cash receipt.  
      // ---------------------------------------------------
      try
      {
        CreateCashReceiptStatusHistory1();
        ++local.Totals.NbrOfUpdates.Count;

        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "Created Cash Receipt Status History for EFT " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
          UseCabControlReport();
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabReportSend.RptDetail = "ID " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " Pending Error: Already Exists creating the Cash Receipt Status History.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            local.EabReportSend.RptDetail = "ID " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " Pending Error: Permitted Value creating the Cash Receipt Status History.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) == 'Y')
      {
        // Since this is a interface EFT we need to update the status and then 
        // we are finished with it - no detail record should be created.
        UseFnUpdateEftStatus();

        if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
        {
          return;
        }
        else
        {
          continue;
        }
      }

      // Initialize the cash receipt detail status history info.
      local.CashReceiptDetailStatHistory.ReasonCodeId = "";
      local.CashReceiptDetailStatHistory.ReasonText =
        Spaces(CashReceiptDetailStatHistory.ReasonText_MaxLength);

      // ---------------------------------------------------------------------------------------------
      // Research the Case Idenitifier to determine if it is a court order 
      // number or case number.
      UseFnResearchEftCaseIdentifier();

      // ---------------------------------------------------------------------------------------------
      // Verify that the SSN exists on our database.
      local.CsePersonsWorkSet.Ssn =
        NumberToString(entities.ElectronicFundTransmission.ApSsn, 7, 9);
      UseEabReadCsePersonUsingSsn();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabReportSend.RptDetail = "ID " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Critical Error:  Reading CSE person by SSN Problem w/ ADABAS, code = " +
          local.AbendData.AdabasResponseCd;
        UseCabErrorReport();
        local.EabReportSend.RptDetail = "ID " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Critical Error:  Reading CSE person by SSN Problem w/ CICS, code = " +
          local.AbendData.CicsResponseCd;
        UseCabErrorReport();
        UseEabExtractExitStateMessage1();
        local.EabReportSend.RptDetail = "ID " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Critical Error:  Reading CSE person by SSN, msg = " +
          local.ExitStateWorkArea.Message;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
      {
        // The case number/court order number checked out OK in the Research 
        // action block.
        if (IsEmpty(local.CsePersonsWorkSet.Number))
        {
          // The AP SSN did not match a CSE Person in our data base.
          local.CashReceiptDetail.ObligorPersonNumber = "";
          local.CsePersonsWorkSet.FormattedName =
            "AP SSN not found on database";
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedSuspended.SystemGeneratedIdentifier;
          local.CashReceiptDetailStatHistory.ReasonCodeId = "INVSSN";
        }
        else
        {
          // SSN is valid.
          local.CashReceiptDetail.ObligorPersonNumber =
            local.CsePersonsWorkSet.Number;
          local.CsePersonsWorkSet.FormattedName =
            TrimEnd(local.CsePersonsWorkSet.LastName) + ", " + TrimEnd
            (local.CsePersonsWorkSet.FirstName) + " " + local
            .CsePersonsWorkSet.MiddleInitial;

          if (AsChar(local.TraceIndicator.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = "Returned:  Person Number " + local
              .CsePersonsWorkSet.Number + " SSN " + local
              .CsePersonsWorkSet.Ssn + " Formatted Name " + local
              .CsePersonsWorkSet.FormattedName + " Last Name " + local
              .CsePersonsWorkSet.LastName;
            UseCabControlReport();
          }

          if (!Equal(local.CsePersonsWorkSet.Number,
            local.CaseOrCourtOrderNbr.Number))
          {
            if (AsChar(local.TraceIndicator.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail = "SSN CSE Person # = " + local
                .CsePersonsWorkSet.Number + "  Case/Court Order Number CSE Person # " +
                local.CaseOrCourtOrderNbr.Number;
              UseCabControlReport();
            }

            // Court order number or case number is NOT valid for the SSN.
            if (AsChar(local.CaseInd.Flag) == 'Y')
            {
              local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
                local.HardcodedSuspended.SystemGeneratedIdentifier;
              local.CashReceiptDetailStatHistory.ReasonCodeId = "NOSSNCSASS";
            }
            else
            {
              local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
                local.HardcodedSuspended.SystemGeneratedIdentifier;
              local.CashReceiptDetailStatHistory.ReasonCodeId = "CTORSSN";
            }
          }
        }
      }

      // ---------------------------------------------------------------------------------------------
      UseCabFnReadCsePersonAddress();

      if (IsExitState("CSE_PERSON_ADDRESS_NF"))
      {
        local.CsePersonAddress.Assign(local.InitializedCsePersonAddress);
        local.CsePersonAddress.Street1 = "address not found";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // Translate the application identifier into a collection type.
      switch(TrimEnd(entities.ElectronicFundTransmission.ApplicationIdentifier))
      {
        case "CS":
          local.CollectionType.SequentialIdentifier =
            local.HardcodedIwo.SequentialIdentifier;

          break;
        case "II":
          local.CollectionType.SequentialIdentifier =
            local.HardcodedIwo.SequentialIdentifier;

          break;
        case "IT":
          local.CollectionType.SequentialIdentifier =
            local.HardcodedStateOffset.SequentialIdentifier;

          break;
        case "IO":
          local.CollectionType.SequentialIdentifier =
            local.HardcodedRegular.SequentialIdentifier;

          break;
        case "RI":
          local.CollectionType.SequentialIdentifier =
            local.HardcodedIwo.SequentialIdentifier;

          break;
        case "RT":
          local.CollectionType.SequentialIdentifier =
            local.HardcodedStateOffset.SequentialIdentifier;

          break;
        case "RO":
          local.CollectionType.SequentialIdentifier =
            local.HardcodedRegular.SequentialIdentifier;

          break;
        default:
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedSuspended.SystemGeneratedIdentifier;
          local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCOLTYPE";

          if (AsChar(local.TraceIndicator.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = "ID number " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " INVCOLTYPE  Invalid Ap ID on EFT record = " + entities
              .ElectronicFundTransmission.ApplicationIdentifier;
            UseCabControlReport();
          }

          break;
      }

      if (Equal(entities.ElectronicFundTransmission.ApplicationIdentifier, "CS") ||
        Equal
        (entities.ElectronicFundTransmission.ApplicationIdentifier, "II") || Equal
        (entities.ElectronicFundTransmission.ApplicationIdentifier, "IT") || Equal
        (entities.ElectronicFundTransmission.ApplicationIdentifier, "IO"))
      {
        if (!Equal(entities.ElectronicFundTransmission.TransmittalAmount,
          entities.ElectronicFundTransmission.CollectionAmount))
        {
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedPendedCashReceiptDetailStatus.
              SystemGeneratedIdentifier;
          local.CashReceiptDetailStatHistory.ReasonCodeId = "EFTAPIDAMT";

          if (AsChar(local.TraceIndicator.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail = "EFTAPIDAMT  Application ID of " + entities
              .ElectronicFundTransmission.ApplicationIdentifier + " does not correspond to the amounts.";
              
            UseCabControlReport();
          }
        }
      }
      else if (Equal(entities.ElectronicFundTransmission.ApplicationIdentifier,
        "RI") || Equal
        (entities.ElectronicFundTransmission.ApplicationIdentifier, "RT") || Equal
        (entities.ElectronicFundTransmission.ApplicationIdentifier, "RO"))
      {
        if (!Lt(entities.ElectronicFundTransmission.TransmittalAmount,
          entities.ElectronicFundTransmission.CollectionAmount))
        {
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedPendedCashReceiptDetailStatus.
              SystemGeneratedIdentifier;
          local.CashReceiptDetailStatHistory.ReasonCodeId = "EFTAPIDAMT";
        }
      }

      if (!Equal(local.CashReceiptDetailStatHistory.ReasonCodeId, "INVCOLTYPE"))
      {
        if (entities.CollectionType.SequentialIdentifier == local
          .CollectionType.SequentialIdentifier)
        {
          // We already have currency on the collection type needed.
        }
        else
        {
          // Read the collection type.
          if (ReadCollectionType())
          {
            ++local.Totals.NbrOfReads.Count;
          }
          else
          {
            local.EabReportSend.RptDetail = "ID " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " Critical Error: Not Found reading Collection Type of" + NumberToString
              (entities.CollectionType.SequentialIdentifier, 15);
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      // Create the cash receipt detail.  
      // ----------------------------------------------------------------
      if (AsChar(local.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "About to create Cash Receipt Detail for EFT " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
        UseCabControlReport();
      }

      try
      {
        CreateCashReceiptDetail();
        ExitState = "ACO_NN0000_ALL_OK";
        ++local.Totals.NbrOfUpdates.Count;
        ++local.Totals.NbrOfCrdRecorded.Count;
        local.Totals.AmtOfCrdRecorded.TotalCurrency += entities.
          ElectronicFundTransmission.TransmittalAmount;

        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "Created Cash Receipt Detail for EFT " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
          UseCabControlReport();
          local.EabReportSend.RptDetail =
            "Created Cash Receipt Detail for EFT " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
        }

        if (!Equal(local.CashReceiptDetailStatHistory.ReasonCodeId, "INVCOLTYPE"))
          
        {
          AssociateCashReceiptDetail();
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.EabReportSend.RptDetail = "ID " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " Pending Error: Already Exists creating the Cash Receipt Detail.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            local.EabReportSend.RptDetail = "ID " + NumberToString
              (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
              " Pending Error: Permitted Value creating the Cash Receipt Detail.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // ***** Any error dealing with file handling is  "critical" and 
              // will result in an abend. *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.Totals.NbrOfPendingErrors.Count;
            local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
            UseFnUpdateEftStatus();

            if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
            {
              return;
            }
            else
            {
              continue;
            }

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // Set the status of the cash receipt detail.  
      // --------------------------------------------------------------
      // If the court order number can be matched to a case number/court order 
      // number then the status is Release else it is Pended.
      if (AsChar(local.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "About to create Cash Receipt Detail Status History for EFT " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
        UseCabControlReport();
      }

      if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == entities
        .Released.SystemGeneratedIdentifier)
      {
        // Release the Cash Receipt Detail
        try
        {
          CreateCashReceiptDetailStatHistory2();
          ExitState = "ACO_NN0000_ALL_OK";
          ++local.Totals.NbrOfCrdReleased.Count;
          local.Totals.AmtOfCrdReleased.TotalCurrency += entities.
            ElectronicFundTransmission.TransmittalAmount;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_STAT_HST_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RCPT_DTL_STAT_HST_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedSuspended.SystemGeneratedIdentifier)
      {
        // Pend the Cash Receipt Detail
        try
        {
          CreateCashReceiptDetailStatHistory3();
          ExitState = "ACO_NN0000_ALL_OK";
          ++local.Totals.NbrOfCrdSuspended.Count;
          local.Totals.AmtOfCrdSuspended.TotalCurrency += entities.
            ElectronicFundTransmission.TransmittalAmount;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_STAT_HST_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RCPT_DTL_STAT_HST_PV";

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
        // Pend the Cash Receipt Detail
        try
        {
          CreateCashReceiptDetailStatHistory1();
          ExitState = "ACO_NN0000_ALL_OK";
          ++local.Totals.NbrOfCrdPended.Count;
          local.Totals.AmtOfCrdPended.TotalCurrency += entities.
            ElectronicFundTransmission.TransmittalAmount;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CASH_RCPT_DTL_STAT_HST_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RCPT_DTL_STAT_HST_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.Totals.NbrOfUpdates.Count;

        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "Created Pended Cash Receipt Detail Status History for EFT " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
          UseCabControlReport();
          local.EabReportSend.RptDetail =
            "Cash Receipt Detail Status Reason of " + entities
            .CashReceiptDetailStatHistory.ReasonCodeId;
          UseCabControlReport();
        }
      }
      else if (IsExitState("FN0000_CASH_RCPT_DTL_STAT_HST_AE"))
      {
        local.EabReportSend.RptDetail = "ID " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Pending Error: Already Exists creating the Cash Receipt Detail Status History.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // ***** Any error dealing with file handling is  "critical" and will 
          // result in an abend. *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.Totals.NbrOfPendingErrors.Count;
        local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
        UseFnUpdateEftStatus();

        if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
        {
          return;
        }
        else
        {
          continue;
        }
      }
      else if (IsExitState("FN0000_CASH_RCPT_DTL_STAT_HST_PV"))
      {
        local.EabReportSend.RptDetail = "ID " + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Pending Error: Permitted Value creating the Cash Receipt Detail Status History.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // ***** Any error dealing with file handling is  "critical" and will 
          // result in an abend. *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.Totals.NbrOfPendingErrors.Count;
        local.ElectronicFundTransmission.TransmissionStatusCode = "PENDED";
        UseFnUpdateEftStatus();

        if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
        {
          return;
        }
        else
        {
          continue;
        }
      }
      else
      {
      }

      // If there was a fee then create a cash receipt detail fee.   
      // ---------------------------
      if (Lt(entities.ElectronicFundTransmission.TransmittalAmount,
        entities.ElectronicFundTransmission.CollectionAmount))
      {
        try
        {
          CreateCashReceiptDetailFee();
          ++local.Totals.NbrOfUpdates.Count;
          ++local.Totals.NbrOfCrdFees.Count;
          local.Totals.AmtOfCrdFees.TotalCurrency += entities.CashReceiptEvent.
            TotalCashFeeAmt.GetValueOrDefault();

          if (AsChar(local.TraceIndicator.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Created a Cash Receipt Detail Fee for " + NumberToString
              ((long)entities.CashReceiptDetailFee.Amount, 15);
            UseCabControlReport();
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail = "ID " + NumberToString
                (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                " Pending Error: Already Exists creating the Cash Receipt Detail Fee.";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                // ***** Any error dealing with file handling is  "critical" and
                // will result in an abend. *****
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              ++local.Totals.NbrOfPendingErrors.Count;
              local.ElectronicFundTransmission.TransmissionStatusCode =
                "PENDED";
              UseFnUpdateEftStatus();

              if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
              {
                return;
              }
              else
              {
                continue;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail = "ID " + NumberToString
                (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                " Pending Error: Permitted Value creating the Cash Receipt Detail Fee.";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                // ***** Any error dealing with file handling is  "critical" and
                // will result in an abend. *****
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              ++local.Totals.NbrOfPendingErrors.Count;
              local.ElectronicFundTransmission.TransmissionStatusCode =
                "PENDED";
              UseFnUpdateEftStatus();

              if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
              {
                return;
              }
              else
              {
                continue;
              }

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!IsEmpty(local.CsePersonAddress.State))
      {
        try
        {
          CreateCashReceiptDetailAddress();

          // Continue
          if (AsChar(local.TraceIndicator.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Created a Cash Receipt Detail address for CRD " + NumberToString
              (entities.CashReceiptDetail.SequentialIdentifier, 15);
            UseCabControlReport();
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail = "ID " + NumberToString
                (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                " Pending Error: Already Exists creating the Cash Receipt Detail Address.";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                // ***** Any error dealing with file handling is  "critical" and
                // will result in an abend. *****
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              ++local.Totals.NbrOfPendingErrors.Count;
              local.ElectronicFundTransmission.TransmissionStatusCode =
                "PENDED";
              UseFnUpdateEftStatus();

              if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
              {
                return;
              }
              else
              {
                continue;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail = "ID " + NumberToString
                (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                " Pending Error: Permitted Value creating the Cash Receipt Detail Address.";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                // ***** Any error dealing with file handling is  "critical" and
                // will result in an abend. *****
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              ++local.Totals.NbrOfPendingErrors.Count;
              local.ElectronicFundTransmission.TransmissionStatusCode =
                "PENDED";
              UseFnUpdateEftStatus();

              if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
              {
                return;
              }
              else
              {
                continue;
              }

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // The EFT record was receipted so update the EFT row to "RECEIPTED".
      local.ElectronicFundTransmission.TransmissionStatusCode = "RECEIPTED";
      local.ElectronicFundTransmission.TransmissionProcessDate =
        local.Process.Date;
      UseFnUpdateEftStatus();

      if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        return;
      }

      // Medical Support Alert 
      // -----------------------------------------------------
      // If the Medical Support Indicator is set to Y then call an action block 
      // to send an alert to all service providers who are assigned to an open
      // case with one or more support persons on an active accruing obligaion
      // for this AP.
      if (AsChar(entities.ElectronicFundTransmission.MedicalSupportId) == 'Y')
      {
        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "*About to do medical support alerts for EFT ID " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
          UseCabControlReport();
        }

        UseFnEftRaiseMedSupportEvent();

        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "*Finished medical support alerts for EFT ID " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
          UseCabControlReport();
          local.EabReportSend.RptDetail = UseEabExtractExitStateMessage2();
          UseCabControlReport();
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // Employment Termination Alert 
      // -----------------------------------------------------
      // If the Employment Termination Indicator is set to Y then call an action
      // block to send all necessary alerts
      if (AsChar(entities.ElectronicFundTransmission.EmploymentTerminationId) ==
        'Y')
      {
        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "About to do employment termination alerts for EFT ID " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
          UseCabControlReport();
        }

        UseFnEftRaiseEmpTermEvent();

        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "Finished employment termination alerts for EFT ID " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
          UseCabControlReport();
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      if (AsChar(local.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "End of alert area" + NumberToString
          (entities.ElectronicFundTransmission.TransmissionIdentifier, 15);
        UseCabControlReport();
      }

      // Check to see if it is time to do a checkpoint (commit a unit of work).
      if (local.Totals.NbrOfUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() || local
        .Totals.NbrOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "";
          UseCabControlReport();
          local.EabReportSend.RptDetail =
            "About to update the control table for the Cash Receipt number " + NumberToString
            (entities.CashReceiptSequentialNum.LastUsedNumber, 15);
          UseCabControlReport();
        }

        // When doing the commit update the last used cash receipt number .
        try
        {
          UpdateControlTable();

          if (AsChar(local.TraceIndicator.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Last Cash Receipt number was updated to " + NumberToString
              (entities.CashReceiptSequentialNum.LastUsedNumber, 15);
            UseCabControlReport();
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.Totals.NbrOfUpdates.Count;
              local.EabReportSend.RptDetail =
                "Critical Error: Not Unique updating Control Table for " + entities
                .CashReceiptSequentialNum.Identifier;
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail =
                "Critical Error: Permitted Value violation updating Control Table for " +
                entities.CashReceiptSequentialNum.Identifier;
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // Update the deposit (fund transaction).
        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "About to update the fund transaction";
          UseCabControlReport();
        }

        try
        {
          UpdateFundTransaction();
          ++local.Totals.NbrOfUpdates.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail = "ID " + NumberToString
                (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                " Critical Error: Not Unique updating the Fund Transaction.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail = "ID " + NumberToString
                (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) +
                " Critical Error: Permitted Value updating the Fund Transaction.";
                
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (AsChar(local.RollbackForTestInd.Flag) == 'Y')
        {
        }
        else
        {
          UseExtToDoACommit();
          local.EabReportSend.RptDetail =
            "Return code from external commit = " + NumberToString
            (local.External.NumericReturnCode, 15);
          UseCabControlReport();
        }

        local.Totals.TotNbrOfReads.Count += local.Totals.NbrOfReads.Count;
        local.Totals.NbrOfReads.Count = 0;
        local.Totals.TotNbrOfUpdates.Count += local.Totals.NbrOfUpdates.Count;
        local.Totals.NbrOfUpdates.Count = 0;
        ++local.Totals.NbrOfCheckpoints.Count;
        local.ProgramCheckpointRestart.ProgramName = global.UserId;
        UseReadPgmCheckpointRestart();

        if (AsChar(local.TraceIndicator.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "";
          UseCabControlReport();
        }
      }

ReadEach:
      ;
    }

    if (AsChar(local.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "";
      UseCabControlReport();
      local.EabReportSend.RptDetail = "Finished processing EFTs.";
      UseCabControlReport();
    }

    try
    {
      UpdateControlTable();

      if (AsChar(local.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "Last Cash Receipt number was updated to " + NumberToString
          (entities.CashReceiptSequentialNum.LastUsedNumber, 15);
        UseCabControlReport();
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ++local.Totals.NbrOfUpdates.Count;
          local.EabReportSend.RptDetail =
            "Critical Error: Not Unique updating Control Table for " + entities
            .CashReceiptSequentialNum.Identifier;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.PermittedValueViolation:
          local.EabReportSend.RptDetail =
            "Critical Error: Permitted Value violation updating Control Table for " +
            entities.CashReceiptSequentialNum.Identifier;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // Update the deposit (fund transaction).
    if (AsChar(local.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "About to update the fund transaction";
      UseCabControlReport();
    }

    try
    {
      UpdateFundTransaction();
      ++local.Totals.NbrOfUpdates.Count;

      if (AsChar(local.TraceIndicator.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "Updated the fund transaction for " + NumberToString
          ((long)(local.FundTransaction.Amount * 100), 15);
        UseCabControlReport();
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          local.EabReportSend.RptDetail = "ID " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Critical Error: Not Unique updating the Fund Transaction.";
            
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.PermittedValueViolation:
          local.EabReportSend.RptDetail = "ID " + NumberToString
            (entities.ElectronicFundTransmission.TransmissionIdentifier, 15) + " Critical Error: Permitted Value updating the Fund Transaction.";
            
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (AsChar(local.TraceIndicator.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "Finished with updating the fund transaction.";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***** Any error dealing with file handling is  "critical" and will 
        // result in an abend. *****
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ++local.Totals.NbrOfCheckpoints.Count;
    local.Totals.TotNbrOfReads.Count += local.Totals.NbrOfReads.Count;
    local.Totals.TotNbrOfUpdates.Count += local.Totals.NbrOfUpdates.Count;
    local.EabReportSend.RptDetail = "Count of EFT Records Processed:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum1();
    local.EabReportSend.RptDetail =
      "Total number of EFT records read.........." + local.WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum2();
    local.EabReportSend.RptDetail =
      "Total number of EFT records receipted....." + local.WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum3();
    local.EabReportSend.RptDetail =
      "Total number of EFT records pended........" + local.WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total Amount of EFT Records Processed:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount1();
    local.EabReportSend.RptDetail =
      "Total amount of EFT records read........" + local.WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount2();
    local.EabReportSend.RptDetail =
      "Total amount of EFT records receipted..." + local.WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount3();
    local.EabReportSend.RptDetail =
      "Total amount of EFT records pended......" + local.WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Count of EFT Records Receipted as Cash Receipts:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum4();
    local.EabReportSend.RptDetail =
      "Total number of Cash Receipts receipted..................." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum5();
    local.EabReportSend.RptDetail =
      "Total number of non-interface Cash Receipts receipted....." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum6();
    local.EabReportSend.RptDetail =
      "Total number of interface Cash Receipts matched..........." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum7();
    local.EabReportSend.RptDetail =
      "Total number of interface Cash Receipts not matched......." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Amount of EFT Records Receipted as Cash Receipts:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount4();
    local.EabReportSend.RptDetail =
      "Total amount of Cash Receipts receipted................." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount5();
    local.EabReportSend.RptDetail =
      "Total amount of non-interface Cash Receipts receipted..." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount6();
    local.EabReportSend.RptDetail =
      "Total amount of interface Cash Receipts matched........." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount7();
    local.EabReportSend.RptDetail =
      "Total amount of interface Cash Receipts not matched....." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Count of EFT Records Recorded as Cash Receipts Details:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum8();
    local.EabReportSend.RptDetail =
      "Total number of Cash Receipt Detail records recorded...." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum10();
    local.EabReportSend.RptDetail =
      "Total number of Cash Receipt Detail records released...." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum11();
    local.EabReportSend.RptDetail =
      "Total number of Cash Receipt Detail records pended......" + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum12();
    local.EabReportSend.RptDetail =
      "Total number of Cash Receipt Detail records suspended..." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Amount of EFT Records Recorded as Cash Receipt Details:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount8();
    local.EabReportSend.RptDetail =
      "Total amount of Cash Receipt Details recorded........." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount9();
    local.EabReportSend.RptDetail =
      "Total amount of Cash Receipt Details released........." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount10();
    local.EabReportSend.RptDetail =
      "Total amount of Cash Receipt Details pended..........." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount11();
    local.EabReportSend.RptDetail =
      "Total amount of Cash Receipt Details suspended........" + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum13();
    local.EabReportSend.RptDetail =
      "Total number of Cash Receipt Detail records with Fees....." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount12();
    local.EabReportSend.RptDetail =
      "Total amount of Cash Receipt Detail Fees................" + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total Checkpoint Frequency counts:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum14();
    local.EabReportSend.RptDetail = "Total number of reads................" + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum17();
    local.EabReportSend.RptDetail = "Total number of updates.............." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum18();
    local.EabReportSend.RptDetail = "Total number of checkpoints.........." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total Error counts:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum15();
    local.EabReportSend.RptDetail = "Total number of pending errors......." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum16();
    local.EabReportSend.RptDetail = "Total number of non pending errors..." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.RollbackForTestInd.Flag) == 'Y')
    {
      ExitState = "ACO_RE0000_INPUT_FILE_EMPTY_RB";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    UseSiCloseAdabas();
  }

  private static void MoveAbendData(AbendData source, AbendData target)
  {
    target.AdabasFileNumber = source.AdabasFileNumber;
    target.AdabasFileAction = source.AdabasFileAction;
    target.AdabasResponseCd = source.AdabasResponseCd;
    target.CicsResourceNm = source.CicsResourceNm;
    target.CicsFunctionCd = source.CicsFunctionCd;
    target.CicsResponseCd = source.CicsResponseCd;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveElectronicFundTransmission1(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionStatusCode = source.TransmissionStatusCode;
    target.TransmissionProcessDate = source.TransmissionProcessDate;
  }

  private static void MoveElectronicFundTransmission2(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.CompanyName = source.CompanyName;
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.FileCreationDate = source.FileCreationDate;
  }

  private static void MoveElectronicFundTransmission3(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.CompanyName = source.CompanyName;
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.FileCreationDate = source.FileCreationDate;
    target.CompanyIdentificationNumber = source.CompanyIdentificationNumber;
  }

  private static void MoveElectronicFundTransmission4(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.CompanyDescriptiveDate = source.CompanyDescriptiveDate;
    target.EffectiveEntryDate = source.EffectiveEntryDate;
    target.CompanyEntryDescription = source.CompanyEntryDescription;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
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

  private void UseCabFnReadCsePersonAddress()
  {
    var useImport = new CabFnReadCsePersonAddress.Import();
    var useExport = new CabFnReadCsePersonAddress.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabFnReadCsePersonAddress.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabTextnum1()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfEftsRead.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum2()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfEftsReceipted.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum3()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfEftsPended.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum4()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfCrReceipted.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum5()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfNonCrReceipted.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum6()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfIntCrMatched.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum7()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfIntCrNotMatched.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum8()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfCrdRecorded.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum10()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfCrdReleased.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum11()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfCrdPended.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum12()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfCrdSuspended.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum13()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfCrdFees.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum14()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.TotNbrOfReads.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum15()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfPendingErrors.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum16()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfNonPendingErrors.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum17()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.TotNbrOfUpdates.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum18()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.Totals.NbrOfCheckpoints.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnumAmount1()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfEftRecordsRead.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount2()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfEftRecReceipted.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount3()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfEftRecordsPended.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount4()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfCrReceipted.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount5()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfNonCrReceipted.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount6()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfIntCrMatched.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount7()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfIntCrNotMatched.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount8()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfCrdRecorded.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount9()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfCrdReleased.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount10()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency = local.Totals.AmtOfCrdPended.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount11()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.Totals.AmtOfCrdSuspended.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount12()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency = local.Totals.AmtOfCrdFees.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseEabExtractExitStateMessage1()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private string UseEabExtractExitStateMessage2()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    MoveAbendData(useExport.AbendData, local.AbendData);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB673Housekeeping()
  {
    var useImport = new FnB673Housekeeping.Import();
    var useExport = new FnB673Housekeeping.Export();

    useExport.PersistentEft.Assign(entities.Eft);
    useExport.PersistentDeposited.Assign(entities.Deposited);
    useExport.PersistentReleased.Assign(entities.Released);
    useExport.PersistentSuspended.Assign(entities.Suspended);
    useExport.PersistentPended.Assign(entities.Pended);
    useExport.PersistentIntCostRecov.Assign(entities.InterstateCostRecoveryFee);
    useExport.NbrOfReads.Count = local.Totals.NbrOfReads.Count;

    Call(FnB673Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.HardcodedEmployer.SystemGeneratedIdentifier =
      useExport.HardcodedEmployer.SystemGeneratedIdentifier;
    local.HardcodedFdso.SystemGeneratedIdentifier =
      useExport.HardcodedFdso.SystemGeneratedIdentifier;
    local.HardcodedMisc.SystemGeneratedIdentifier =
      useExport.HardcodedMisc.SystemGeneratedIdentifier;
    entities.Eft.SystemGeneratedIdentifier =
      useExport.PersistentEft.SystemGeneratedIdentifier;
    entities.Deposited.SystemGeneratedIdentifier =
      useExport.PersistentDeposited.SystemGeneratedIdentifier;
    local.HardcodedRegular.SequentialIdentifier =
      useExport.HardcodedRegular.SequentialIdentifier;
    local.HardcodedIwo.SequentialIdentifier =
      useExport.HardcodedIwo.SequentialIdentifier;
    local.HardcodedStateOffset.SequentialIdentifier =
      useExport.HardcodedStateOffset.SequentialIdentifier;
    local.HardcodedReleasedCashReceiptDetailStatus.SystemGeneratedIdentifier =
      useExport.HardcodedReleased.SystemGeneratedIdentifier;
    local.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.HardcodedSuspended.SystemGeneratedIdentifier;
    local.HardcodedPendedCashReceiptDetailStatus.SystemGeneratedIdentifier =
      useExport.HardcodedPended.SystemGeneratedIdentifier;
    entities.Released.SystemGeneratedIdentifier =
      useExport.PersistentReleased.SystemGeneratedIdentifier;
    entities.Suspended.SystemGeneratedIdentifier =
      useExport.PersistentSuspended.SystemGeneratedIdentifier;
    entities.Pended.SystemGeneratedIdentifier =
      useExport.PersistentPended.SystemGeneratedIdentifier;
    entities.InterstateCostRecoveryFee.SystemGeneratedIdentifier =
      useExport.PersistentIntCostRecov.SystemGeneratedIdentifier;
    local.HardcodedDeposited.SystemGeneratedIdentifier =
      useExport.HardcodedDeposited.SystemGeneratedIdentifier;
    local.Totals.NbrOfReads.Count = useExport.NbrOfReads.Count;
    local.TraceIndicator.Flag = useExport.TraceIndicator.Flag;
    local.RollbackForTestInd.Flag = useExport.RollbackForTestInd.Flag;
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnDetermineCrSourceType()
  {
    var useImport = new FnDetermineCrSourceType.Import();
    var useExport = new FnDetermineCrSourceType.Export();

    MoveElectronicFundTransmission4(entities.ElectronicFundTransmission,
      useImport.ElectronicFundTransmission);
    useImport.HardcodedEmployer.SystemGeneratedIdentifier =
      local.HardcodedEmployer.SystemGeneratedIdentifier;
    useImport.HardcodedFdso.SystemGeneratedIdentifier =
      local.HardcodedFdso.SystemGeneratedIdentifier;
    useImport.HardcodedMisc.SystemGeneratedIdentifier =
      local.HardcodedMisc.SystemGeneratedIdentifier;
    useImport.NumericString.Text10 = local.NumericString.Text10;

    Call(FnDetermineCrSourceType.Execute, useImport, useExport);

    local.CashReceiptSourceType.Assign(useExport.CashReceiptSourceType);
    local.ForUpdate.CheckDate = useExport.ForCheckDate.CheckDate;
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnEftRaiseEmpTermEvent()
  {
    var useImport = new FnEftRaiseEmpTermEvent.Import();
    var useExport = new FnEftRaiseEmpTermEvent.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    MoveElectronicFundTransmission3(entities.ElectronicFundTransmission,
      useImport.ElectronicFundTransmission);
    useImport.MaximumDate.Date = local.MaximumDiscontinueDate.Date;
    useImport.TraceIndicator.Flag = local.TraceIndicator.Flag;
    useExport.NbrOfReads.Count = local.Totals.NbrOfReads.Count;
    useExport.NbrOfUpdates.Count = local.Totals.NbrOfUpdates.Count;
    useExport.NbrOfNonPendingErrors.Count =
      local.Totals.NbrOfNonPendingErrors.Count;

    Call(FnEftRaiseEmpTermEvent.Execute, useImport, useExport);

    local.Totals.NbrOfReads.Count = useExport.NbrOfReads.Count;
    local.Totals.NbrOfUpdates.Count = useExport.NbrOfUpdates.Count;
    local.Totals.NbrOfNonPendingErrors.Count =
      useExport.NbrOfNonPendingErrors.Count;
  }

  private void UseFnEftRaiseMedSupportEvent()
  {
    var useImport = new FnEftRaiseMedSupportEvent.Import();
    var useExport = new FnEftRaiseMedSupportEvent.Export();

    MoveElectronicFundTransmission2(entities.ElectronicFundTransmission,
      useImport.ElectronicFundTransmission);
    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
    useImport.TraceIndicator.Flag = local.TraceIndicator.Flag;
    useExport.NbrOfReads.Count = local.Totals.NbrOfReads.Count;
    useExport.NbrOfUpdates.Count = local.Totals.NbrOfUpdates.Count;
    useExport.NbrOfNonPendingErrors.Count =
      local.Totals.NbrOfNonPendingErrors.Count;

    Call(FnEftRaiseMedSupportEvent.Execute, useImport, useExport);

    local.Totals.NbrOfReads.Count = useExport.NbrOfReads.Count;
    local.Totals.NbrOfUpdates.Count = useExport.NbrOfUpdates.Count;
    local.Totals.NbrOfNonPendingErrors.Count =
      useExport.NbrOfNonPendingErrors.Count;
  }

  private void UseFnResearchEftCaseIdentifier()
  {
    var useImport = new FnResearchEftCaseIdentifier.Import();
    var useExport = new FnResearchEftCaseIdentifier.Export();

    useImport.ElectronicFundTransmission.CaseId =
      entities.ElectronicFundTransmission.CaseId;
    useImport.Pended.SystemGeneratedIdentifier =
      local.HardcodedPendedCashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.Suspended.SystemGeneratedIdentifier =
      local.HardcodedSuspended.SystemGeneratedIdentifier;
    useImport.NumericString.Text10 = local.NumericString.Text10;
    useImport.TraceIndicator.Flag = local.TraceIndicator.Flag;
    useExport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useExport.NbrOfReads.Count = local.Totals.NbrOfReads.Count;

    Call(FnResearchEftCaseIdentifier.Execute, useImport, useExport);

    MoveCashReceiptDetail(useExport.CashReceiptDetail, local.CashReceiptDetail);
    local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      useExport.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    local.CashReceiptDetailStatHistory.ReasonCodeId =
      useExport.CashReceiptDetailStatHistory.ReasonCodeId;
    local.Totals.NbrOfReads.Count = useExport.NbrOfReads.Count;
    local.CaseInd.Flag = useExport.CaseInd.Flag;
    local.CaseOrCourtOrderNbr.Number = useExport.CaseOrCourtOrderNbr.Number;
  }

  private void UseFnUpdateEftStatus()
  {
    var useImport = new FnUpdateEftStatus.Import();
    var useExport = new FnUpdateEftStatus.Export();

    MoveElectronicFundTransmission1(local.ElectronicFundTransmission,
      useImport.Changes);
    useImport.TraceIndicator.Flag = local.TraceIndicator.Flag;
    useExport.Persistent.Assign(entities.ElectronicFundTransmission);
    useExport.NbrOfEftsReceipted.Count = local.Totals.NbrOfEftsReceipted.Count;
    useExport.AmtOfEftsReceipted.TotalCurrency =
      local.Totals.AmtOfEftRecReceipted.TotalCurrency;
    useExport.NbrOfEftsPended.Count = local.Totals.NbrOfEftsPended.Count;
    useExport.AmtOfEftsPended.TotalCurrency =
      local.Totals.AmtOfEftRecordsPended.TotalCurrency;
    useExport.NbrOfUpdates.Count = local.Totals.NbrOfUpdates.Count;

    Call(FnUpdateEftStatus.Execute, useImport, useExport);

    entities.ElectronicFundTransmission.Assign(useExport.Persistent);
    local.Totals.NbrOfEftsReceipted.Count = useExport.NbrOfEftsReceipted.Count;
    local.Totals.AmtOfEftRecReceipted.TotalCurrency =
      useExport.AmtOfEftsReceipted.TotalCurrency;
    local.Totals.NbrOfEftsPended.Count = useExport.NbrOfEftsPended.Count;
    local.Totals.AmtOfEftRecordsPended.TotalCurrency =
      useExport.AmtOfEftsPended.TotalCurrency;
    local.Totals.NbrOfUpdates.Count = useExport.NbrOfUpdates.Count;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
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

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void AssociateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var cltIdentifier = entities.CollectionType.SequentialIdentifier;

    entities.CashReceiptDetail.Populated = false;
    Update("AssociateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Populated = true;
  }

  private void CreateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var crvIdentifier = entities.CashReceiptEvent.SystemGeneratedIdentifier;
    var cstIdentifier = entities.CashReceiptEvent.CstIdentifier;
    var crtIdentifier = entities.Eft.SystemGeneratedIdentifier;
    var receiptAmount = entities.ElectronicFundTransmission.TransmittalAmount;
    var sequentialNumber = local.NextCashReceiptSeqNum.LastUsedNumber;
    var receiptDate = local.Process.Date;
    var checkType = "CSE";
    var checkNumber =
      NumberToString(entities.ElectronicFundTransmission.TransmissionIdentifier,
      12);
    var checkDate = local.ForUpdate.CheckDate;
    var receivedDate = entities.ElectronicFundTransmission.EffectiveEntryDate;
    var payorOrganization = entities.ElectronicFundTransmission.CompanyName;
    var balancedTimestamp = Now();
    var totalCashTransactionAmount =
      local.ForUpdate.TotalCashTransactionAmount.GetValueOrDefault();
    var totalCashTransactionCount =
      local.ForUpdate.TotalCashTransactionCount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var fttIdentifier = entities.FundTransaction.FttIdentifier;
    var pcaCode = entities.FundTransaction.PcaCode;
    var pcaEffectiveDate = entities.FundTransaction.PcaEffectiveDate;
    var funIdentifier = entities.FundTransaction.FunIdentifier;
    var ftrIdentifier = entities.FundTransaction.SystemGeneratedIdentifier;

    CheckValid<CashReceipt>("CashBalanceReason", "");
    entities.CashReceipt.Populated = false;
    Update("CreateCashReceipt",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetInt32(command, "cashReceiptId", sequentialNumber);
        db.SetDate(command, "receiptDate", receiptDate);
        db.SetNullableString(command, "checkType", checkType);
        db.SetNullableString(command, "checkNumber", checkNumber);
        db.SetNullableDate(command, "checkDate", checkDate);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "depositRlseDt", receiptDate);
        db.SetNullableString(command, "referenceNumber", "");
        db.SetNullableString(command, "payorOrganization", payorOrganization);
        db.SetNullableString(command, "payorFirstName", "");
        db.SetNullableString(command, "payorLastName", "");
        db.SetNullableString(command, "frwrdStreet1", "");
        db.SetNullableString(command, "frwrdState", "");
        db.SetNullableString(command, "frwrdZip5", "");
        db.SetNullableString(command, "frwrdZip4", "");
        db.SetNullableString(command, "frwrdZip3", "");
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetNullableDecimal(
          command, "totalCashTransac", totalCashTransactionAmount);
        db.SetNullableDecimal(command, "totNoncshTrnAmt", 0M);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableInt32(command, "totNocshTranCnt", 0);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "fttIdentifier", fttIdentifier);
        db.SetNullableString(command, "pcaCode", pcaCode);
        db.SetNullableDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetNullableInt32(command, "funIdentifier", funIdentifier);
        db.SetNullableInt32(command, "ftrIdentifier", ftrIdentifier);
        db.SetNullableString(command, "cashBalRsn", "");
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "note", "");
        db.SetNullableString(command, "payorSsn", "");
      });

    entities.CashReceipt.CrvIdentifier = crvIdentifier;
    entities.CashReceipt.CstIdentifier = cstIdentifier;
    entities.CashReceipt.CrtIdentifier = crtIdentifier;
    entities.CashReceipt.ReceiptAmount = receiptAmount;
    entities.CashReceipt.SequentialNumber = sequentialNumber;
    entities.CashReceipt.ReceiptDate = receiptDate;
    entities.CashReceipt.CheckType = checkType;
    entities.CashReceipt.CheckNumber = checkNumber;
    entities.CashReceipt.CheckDate = checkDate;
    entities.CashReceipt.ReceivedDate = receivedDate;
    entities.CashReceipt.DepositReleaseDate = receiptDate;
    entities.CashReceipt.PayorOrganization = payorOrganization;
    entities.CashReceipt.BalancedTimestamp = balancedTimestamp;
    entities.CashReceipt.TotalCashTransactionAmount =
      totalCashTransactionAmount;
    entities.CashReceipt.TotalCashTransactionCount = totalCashTransactionCount;
    entities.CashReceipt.CreatedBy = createdBy;
    entities.CashReceipt.CreatedTimestamp = createdTimestamp;
    entities.CashReceipt.FttIdentifier = fttIdentifier;
    entities.CashReceipt.PcaCode = pcaCode;
    entities.CashReceipt.PcaEffectiveDate = pcaEffectiveDate;
    entities.CashReceipt.FunIdentifier = funIdentifier;
    entities.CashReceipt.FtrIdentifier = ftrIdentifier;
    entities.CashReceipt.CashBalanceReason = "";
    entities.CashReceipt.LastUpdatedBy = createdBy;
    entities.CashReceipt.LastUpdatedTimestamp = createdTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void CreateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var sequentialIdentifier = 1;
    var courtOrderNumber = local.CashReceiptDetail.CourtOrderNumber ?? "";
    var caseNumber = local.CashReceiptDetail.CaseNumber ?? "";
    var receivedAmount = entities.ElectronicFundTransmission.TransmittalAmount;
    var collectionAmount =
      entities.ElectronicFundTransmission.CollectionAmount.GetValueOrDefault();
    var collectionDate = entities.ElectronicFundTransmission.EffectiveEntryDate;
    var obligorPersonNumber = local.CashReceiptDetail.ObligorPersonNumber ?? "";
    var obligorSocialSecurityNumber =
      NumberToString(entities.ElectronicFundTransmission.ApSsn, 9);
    var obligorFirstName = local.CsePersonsWorkSet.FirstName;
    var obligorLastName = local.CsePersonsWorkSet.LastName;
    var obligorMiddleName = local.CsePersonsWorkSet.MiddleInitial;
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;
    var cltIdentifier = entities.CollectionType.SequentialIdentifier;

    CheckValid<CashReceiptDetail>("MultiPayor", "");
    CheckValid<CashReceiptDetail>("JointReturnInd", "");
    entities.CashReceiptDetail.Populated = false;
    Update("CreateCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "crdId", sequentialIdentifier);
        db.SetNullableString(command, "interfaceTranId", "");
        db.SetNullableString(command, "adjustmentInd", "");
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableInt32(command, "offsetTaxid", 0);
        db.SetDecimal(command, "receivedAmount", receivedAmount);
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "multiPayor", "");
        db.SetNullableInt32(command, "offsetTaxYear", 0);
        db.SetNullableString(command, "jointReturnInd", "");
        db.SetNullableString(command, "jointReturnName", "");
        db.SetNullableString(
          command, "dfltdCollDatInd", GetImplicitValue<CashReceiptDetail,
          string>("DefaultedCollectionDateInd"));
        db.SetNullableString(command, "oblgorPrsnNbr", obligorPersonNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMidNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", "");
        db.SetNullableString(command, "payeeFirstName", "");
        db.SetNullableString(command, "payeeLastName", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTmst);
        db.SetNullableDecimal(command, "refundedAmt", 0M);
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetNullableString(command, "suppPersNoVol", "");
        db.SetNullableString(command, "referenc", "");
        db.SetNullableString(command, "notes", "");
        db.SetNullableDate(command, "jfaReceivedDate", default(DateTime));
      });

    entities.CashReceiptDetail.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetail.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetail.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetail.SequentialIdentifier = sequentialIdentifier;
    entities.CashReceiptDetail.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetail.CaseNumber = caseNumber;
    entities.CashReceiptDetail.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionAmount = collectionAmount;
    entities.CashReceiptDetail.CollectionDate = collectionDate;
    entities.CashReceiptDetail.MultiPayor = "";
    entities.CashReceiptDetail.JointReturnInd = "";
    entities.CashReceiptDetail.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetail.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetail.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetail.ObligorLastName = obligorLastName;
    entities.CashReceiptDetail.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetail.CreatedBy = createdBy;
    entities.CashReceiptDetail.CreatedTmst = createdTmst;
    entities.CashReceiptDetail.LastUpdatedBy = createdBy;
    entities.CashReceiptDetail.LastUpdatedTmst = createdTmst;
    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Reference = "";
    entities.CashReceiptDetail.Notes = "";
    entities.CashReceiptDetail.Populated = true;
  }

  private void CreateCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var systemGeneratedIdentifier = local.Current.Timestamp;
    var street1 = local.CsePersonAddress.Street1 ?? "";
    var street2 = local.CsePersonAddress.Street2 ?? "";
    var city = local.CsePersonAddress.City ?? "";
    var state = local.CsePersonAddress.State ?? "";
    var zipCode5 = local.CsePersonAddress.ZipCode ?? "";
    var zipCode4 = local.CsePersonAddress.Zip4 ?? "";
    var zipCode3 = local.CsePersonAddress.Zip3 ?? "";
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;

    entities.CashReceiptDetailAddress.Populated = false;
    Update("CreateCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "crdetailAddressI", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
      });

    entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptDetailAddress.Street1 = street1;
    entities.CashReceiptDetailAddress.Street2 = street2;
    entities.CashReceiptDetailAddress.City = city;
    entities.CashReceiptDetailAddress.State = state;
    entities.CashReceiptDetailAddress.ZipCode5 = zipCode5;
    entities.CashReceiptDetailAddress.ZipCode4 = zipCode4;
    entities.CashReceiptDetailAddress.ZipCode3 = zipCode3;
    entities.CashReceiptDetailAddress.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailAddress.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailAddress.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailAddress.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailAddress.Populated = true;
  }

  private void CreateCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var systemGeneratedIdentifier = Now();
    var amount =
      entities.ElectronicFundTransmission.CollectionAmount.GetValueOrDefault() -
      entities.ElectronicFundTransmission.TransmittalAmount;
    var createdBy = global.UserId;
    var cdtIdentifier =
      entities.InterstateCostRecoveryFee.SystemGeneratedIdentifier;

    entities.CashReceiptDetailFee.Populated = false;
    Update("CreateCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetDateTime(command, "crdetailFeeId", systemGeneratedIdentifier);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", systemGeneratedIdentifier);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(
          command, "lastUpdatedTmst", systemGeneratedIdentifier);
        db.SetNullableInt32(command, "cdtIdentifier", cdtIdentifier);
      });

    entities.CashReceiptDetailFee.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailFee.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailFee.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailFee.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptDetailFee.Amount = amount;
    entities.CashReceiptDetailFee.CreatedBy = createdBy;
    entities.CashReceiptDetailFee.CreatedTimestamp = systemGeneratedIdentifier;
    entities.CashReceiptDetailFee.LastUpdatedBy = createdBy;
    entities.CashReceiptDetailFee.LastUpdatedTmst = systemGeneratedIdentifier;
    entities.CashReceiptDetailFee.CdtIdentifier = cdtIdentifier;
    entities.CashReceiptDetailFee.Populated = true;
  }

  private void CreateCashReceiptDetailStatHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier = entities.Pended.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var reasonCodeId = local.CashReceiptDetailStatHistory.ReasonCodeId ?? "";
    var createdBy = global.UserId;
    var discontinueDate = local.MaximumDiscontinueDate.Date;
    var reasonText = local.CashReceiptDetailStatHistory.ReasonText ?? "";

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", reasonCodeId);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.CashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.CashReceiptDetailStatHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailStatHistory.ReasonCodeId = reasonCodeId;
    entities.CashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.ReasonText = reasonText;
    entities.CashReceiptDetailStatHistory.Populated = true;
  }

  private void CreateCashReceiptDetailStatHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier = entities.Released.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var discontinueDate = local.MaximumDiscontinueDate.Date;
    var reasonText = Spaces(240);

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory2",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.CashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.CashReceiptDetailStatHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailStatHistory.ReasonCodeId = "";
    entities.CashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.ReasonText = reasonText;
    entities.CashReceiptDetailStatHistory.Populated = true;
  }

  private void CreateCashReceiptDetailStatHistory3()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier = entities.Suspended.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var reasonCodeId = local.CashReceiptDetailStatHistory.ReasonCodeId ?? "";
    var createdBy = global.UserId;
    var discontinueDate = local.MaximumDiscontinueDate.Date;
    var reasonText = local.CashReceiptDetailStatHistory.ReasonText ?? "";

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory3",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", reasonCodeId);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.CashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.CashReceiptDetailStatHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailStatHistory.ReasonCodeId = reasonCodeId;
    entities.CashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.ReasonText = reasonText;
    entities.CashReceiptDetailStatHistory.Populated = true;
  }

  private void CreateCashReceiptEvent()
  {
    var cstIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var receivedDate = entities.ElectronicFundTransmission.EffectiveEntryDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var totalCashFeeAmt = local.ForCreate.TotalCashFeeAmt.GetValueOrDefault();
    var param = 0M;
    var totalCashAmt = local.ForCreate.TotalCashAmt.GetValueOrDefault();
    var totalCashTransactionCount =
      local.ForCreate.TotalCashTransactionCount.GetValueOrDefault();

    entities.CashReceiptEvent.Populated = false;
    Update("CreateCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "creventId", systemGeneratedIdentifier);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "sourceCreationDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableInt32(command, "totNonCshtrnCnt", 0);
        db.SetNullableDecimal(command, "totCashFeeAmt", totalCashFeeAmt);
        db.SetNullableDecimal(command, "totNoncshFeeAmt", param);
        db.SetNullableDecimal(command, "anticCheckAmt", param);
        db.SetNullableDecimal(command, "totalCashAmt", totalCashAmt);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
      });

    entities.CashReceiptEvent.CstIdentifier = cstIdentifier;
    entities.CashReceiptEvent.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptEvent.ReceivedDate = receivedDate;
    entities.CashReceiptEvent.SourceCreationDate = null;
    entities.CashReceiptEvent.CreatedBy = createdBy;
    entities.CashReceiptEvent.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptEvent.LastUpdatedBy = createdBy;
    entities.CashReceiptEvent.LastUpdatedTmst = createdTimestamp;
    entities.CashReceiptEvent.TotalCashFeeAmt = totalCashFeeAmt;
    entities.CashReceiptEvent.TotalCashAmt = totalCashAmt;
    entities.CashReceiptEvent.TotalCashTransactionCount =
      totalCashTransactionCount;
    entities.CashReceiptEvent.Populated = true;
  }

  private void CreateCashReceiptStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var crsIdentifier = entities.Deposited.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var discontinueDate = local.MaximumDiscontinueDate.Date;

    entities.CashReceiptStatusHistory.Populated = false;
    Update("CreateCashReceiptStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crsIdentifier", crsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.CashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.CashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptStatusHistory.CreatedBy = createdBy;
    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private void CreateCashReceiptStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.ForInterfaceMatching.Populated);

    var crtIdentifier = entities.ForInterfaceMatching.CrtIdentifier;
    var cstIdentifier = entities.ForInterfaceMatching.CstIdentifier;
    var crvIdentifier = entities.ForInterfaceMatching.CrvIdentifier;
    var crsIdentifier = entities.Deposited.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var discontinueDate = local.MaximumDiscontinueDate.Date;

    entities.CashReceiptStatusHistory.Populated = false;
    Update("CreateCashReceiptStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crsIdentifier", crsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.CashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.CashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptStatusHistory.CreatedBy = createdBy;
    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private bool ReadCashReceiptEventCashReceiptCashReceiptStatusHistory1()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.ForInterfaceMatching.Populated = false;
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptCashReceiptStatusHistory1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "sourceCreationDate1",
          local.StartOfMonth.SourceCreationDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "sourceCreationDate2",
          local.EndOfMonth.SourceCreationDate.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDiscontinueDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.ForInterfaceMatching.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ForInterfaceMatching.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 4);
        entities.CashReceiptEvent.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.CashReceiptEvent.TotalCashFeeAmt =
          db.GetNullableDecimal(reader, 8);
        entities.CashReceiptEvent.TotalCashAmt =
          db.GetNullableDecimal(reader, 9);
        entities.CashReceiptEvent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 10);
        entities.ForInterfaceMatching.CrtIdentifier = db.GetInt32(reader, 11);
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 11);
        entities.ForInterfaceMatching.ReceiptAmount = db.GetDecimal(reader, 12);
        entities.ForInterfaceMatching.CheckNumber =
          db.GetNullableString(reader, 13);
        entities.ForInterfaceMatching.CheckDate =
          db.GetNullableDate(reader, 14);
        entities.ForInterfaceMatching.DepositReleaseDate =
          db.GetNullableDate(reader, 15);
        entities.ForInterfaceMatching.PayorOrganization =
          db.GetNullableString(reader, 16);
        entities.ForInterfaceMatching.BalancedTimestamp =
          db.GetNullableDateTime(reader, 17);
        entities.ForInterfaceMatching.CreatedBy = db.GetString(reader, 18);
        entities.ForInterfaceMatching.FttIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.ForInterfaceMatching.PcaCode =
          db.GetNullableString(reader, 20);
        entities.ForInterfaceMatching.PcaEffectiveDate =
          db.GetNullableDate(reader, 21);
        entities.ForInterfaceMatching.FunIdentifier =
          db.GetNullableInt32(reader, 22);
        entities.ForInterfaceMatching.FtrIdentifier =
          db.GetNullableInt32(reader, 23);
        entities.ForInterfaceMatching.CashBalanceAmt =
          db.GetNullableDecimal(reader, 24);
        entities.ForInterfaceMatching.CashBalanceReason =
          db.GetNullableString(reader, 25);
        entities.ForInterfaceMatching.CashDue =
          db.GetNullableDecimal(reader, 26);
        entities.ForInterfaceMatching.LastUpdatedBy =
          db.GetNullableString(reader, 27);
        entities.ForInterfaceMatching.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 28);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 29);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 30);
        entities.CashReceiptStatusHistory.CreatedBy = db.GetString(reader, 31);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 32);
        entities.CashReceiptEvent.Populated = true;
        entities.ForInterfaceMatching.Populated = true;
        entities.CashReceiptStatusHistory.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.ForInterfaceMatching.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptEventCashReceiptCashReceiptStatusHistory2()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.ForInterfaceMatching.Populated = false;
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptCashReceiptStatusHistory2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "sourceCreationDt",
          local.CashReceiptEvent.SourceCreationDate.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDiscontinueDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.ForInterfaceMatching.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ForInterfaceMatching.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 4);
        entities.CashReceiptEvent.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.CashReceiptEvent.TotalCashFeeAmt =
          db.GetNullableDecimal(reader, 8);
        entities.CashReceiptEvent.TotalCashAmt =
          db.GetNullableDecimal(reader, 9);
        entities.CashReceiptEvent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 10);
        entities.ForInterfaceMatching.CrtIdentifier = db.GetInt32(reader, 11);
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 11);
        entities.ForInterfaceMatching.ReceiptAmount = db.GetDecimal(reader, 12);
        entities.ForInterfaceMatching.CheckNumber =
          db.GetNullableString(reader, 13);
        entities.ForInterfaceMatching.CheckDate =
          db.GetNullableDate(reader, 14);
        entities.ForInterfaceMatching.DepositReleaseDate =
          db.GetNullableDate(reader, 15);
        entities.ForInterfaceMatching.PayorOrganization =
          db.GetNullableString(reader, 16);
        entities.ForInterfaceMatching.BalancedTimestamp =
          db.GetNullableDateTime(reader, 17);
        entities.ForInterfaceMatching.CreatedBy = db.GetString(reader, 18);
        entities.ForInterfaceMatching.FttIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.ForInterfaceMatching.PcaCode =
          db.GetNullableString(reader, 20);
        entities.ForInterfaceMatching.PcaEffectiveDate =
          db.GetNullableDate(reader, 21);
        entities.ForInterfaceMatching.FunIdentifier =
          db.GetNullableInt32(reader, 22);
        entities.ForInterfaceMatching.FtrIdentifier =
          db.GetNullableInt32(reader, 23);
        entities.ForInterfaceMatching.CashBalanceAmt =
          db.GetNullableDecimal(reader, 24);
        entities.ForInterfaceMatching.CashBalanceReason =
          db.GetNullableString(reader, 25);
        entities.ForInterfaceMatching.CashDue =
          db.GetNullableDecimal(reader, 26);
        entities.ForInterfaceMatching.LastUpdatedBy =
          db.GetNullableString(reader, 27);
        entities.ForInterfaceMatching.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 28);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 29);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 30);
        entities.CashReceiptStatusHistory.CreatedBy = db.GetString(reader, 31);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 32);
        entities.CashReceiptEvent.Populated = true;
        entities.ForInterfaceMatching.Populated = true;
        entities.CashReceiptStatusHistory.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.ForInterfaceMatching.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "state",
          local.CashReceiptSourceType.State.GetValueOrDefault());
        db.SetNullableInt32(
          command, "county",
          local.CashReceiptSourceType.County.GetValueOrDefault());
        db.SetNullableInt32(
          command, "location",
          local.CashReceiptSourceType.Location.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetString(command, "code", local.CashReceiptSourceType.Code);
        db.SetNullableInt32(
          command, "state",
          local.CashReceiptSourceType.State.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptSourceType3()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          local.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          local.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadControlTable()
  {
    entities.CashReceiptSequentialNum.Populated = false;

    return Read("ReadControlTable",
      null,
      (db, reader) =>
      {
        entities.CashReceiptSequentialNum.Identifier = db.GetString(reader, 0);
        entities.CashReceiptSequentialNum.LastUsedNumber =
          db.GetInt32(reader, 1);
        entities.CashReceiptSequentialNum.Populated = true;
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return ReadEach("ReadElectronicFundTransmission",
      null,
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.CreatedBy = db.GetString(reader, 0);
        entities.ElectronicFundTransmission.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ElectronicFundTransmission.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.ElectronicFundTransmission.PayDate =
          db.GetNullableDate(reader, 4);
        entities.ElectronicFundTransmission.TransmittalAmount =
          db.GetDecimal(reader, 5);
        entities.ElectronicFundTransmission.ApSsn = db.GetInt32(reader, 6);
        entities.ElectronicFundTransmission.MedicalSupportId =
          db.GetString(reader, 7);
        entities.ElectronicFundTransmission.ApName = db.GetString(reader, 8);
        entities.ElectronicFundTransmission.FipsCode =
          db.GetNullableInt32(reader, 9);
        entities.ElectronicFundTransmission.EmploymentTerminationId =
          db.GetNullableString(reader, 10);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 11);
        entities.ElectronicFundTransmission.ReceivingDfiIdentification =
          db.GetNullableInt32(reader, 12);
        entities.ElectronicFundTransmission.DfiAccountNumber =
          db.GetNullableString(reader, 13);
        entities.ElectronicFundTransmission.TransactionCode =
          db.GetString(reader, 14);
        entities.ElectronicFundTransmission.SettlementDate =
          db.GetNullableDate(reader, 15);
        entities.ElectronicFundTransmission.CaseId = db.GetString(reader, 16);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 17);
        entities.ElectronicFundTransmission.CompanyName =
          db.GetNullableString(reader, 18);
        entities.ElectronicFundTransmission.OriginatingDfiIdentification =
          db.GetNullableInt32(reader, 19);
        entities.ElectronicFundTransmission.ReceivingEntityName =
          db.GetNullableString(reader, 20);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 21);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 22);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 23);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 24);
        entities.ElectronicFundTransmission.CompanyIdentificationIcd =
          db.GetNullableString(reader, 25);
        entities.ElectronicFundTransmission.CompanyIdentificationNumber =
          db.GetNullableString(reader, 26);
        entities.ElectronicFundTransmission.CompanyDescriptiveDate =
          db.GetNullableDate(reader, 27);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 28);
        entities.ElectronicFundTransmission.ApplicationIdentifier =
          db.GetNullableString(reader, 29);
        entities.ElectronicFundTransmission.CollectionAmount =
          db.GetNullableDecimal(reader, 30);
        entities.ElectronicFundTransmission.CompanyEntryDescription =
          db.GetNullableString(reader, 31);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private bool ReadFundTransaction()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "fttIdentifier",
          local.HardcodedDeposited.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.FundTransaction.FttIdentifier = db.GetInt32(reader, 0);
        entities.FundTransaction.PcaCode = db.GetString(reader, 1);
        entities.FundTransaction.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.FundTransaction.FunIdentifier = db.GetInt32(reader, 3);
        entities.FundTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransaction.Amount = db.GetDecimal(reader, 5);
        entities.FundTransaction.BusinessDate = db.GetDate(reader, 6);
        entities.FundTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.FundTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.FundTransaction.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.ForInterfaceMatching.Populated);
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var receiptAmount = entities.ElectronicFundTransmission.TransmittalAmount;
    var checkNumber =
      NumberToString(entities.ElectronicFundTransmission.TransmissionIdentifier,
      12);
    var checkDate = local.ForInterfaceMatching.CheckDate;
    var depositReleaseDate = local.Process.Date;
    var payorOrganization = entities.ElectronicFundTransmission.CompanyName;
    var balancedTimestamp = Now();
    var fttIdentifier = entities.FundTransaction.FttIdentifier;
    var pcaCode = entities.FundTransaction.PcaCode;
    var pcaEffectiveDate = entities.FundTransaction.PcaEffectiveDate;
    var funIdentifier = entities.FundTransaction.FunIdentifier;
    var ftrIdentifier = entities.FundTransaction.SystemGeneratedIdentifier;
    var cashBalanceAmt =
      local.ForInterfaceMatching.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = local.ForInterfaceMatching.CashBalanceReason ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.ForInterfaceMatching.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetNullableString(command, "checkNumber", checkNumber);
        db.SetNullableDate(command, "checkDate", checkDate);
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableString(command, "payorOrganization", payorOrganization);
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetNullableInt32(command, "fttIdentifier", fttIdentifier);
        db.SetNullableString(command, "pcaCode", pcaCode);
        db.SetNullableDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetNullableInt32(command, "funIdentifier", funIdentifier);
        db.SetNullableInt32(command, "ftrIdentifier", ftrIdentifier);
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "crvIdentifier",
          entities.ForInterfaceMatching.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ForInterfaceMatching.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ForInterfaceMatching.CrtIdentifier);
      });

    entities.ForInterfaceMatching.ReceiptAmount = receiptAmount;
    entities.ForInterfaceMatching.CheckNumber = checkNumber;
    entities.ForInterfaceMatching.CheckDate = checkDate;
    entities.ForInterfaceMatching.DepositReleaseDate = depositReleaseDate;
    entities.ForInterfaceMatching.PayorOrganization = payorOrganization;
    entities.ForInterfaceMatching.BalancedTimestamp = balancedTimestamp;
    entities.ForInterfaceMatching.FttIdentifier = fttIdentifier;
    entities.ForInterfaceMatching.PcaCode = pcaCode;
    entities.ForInterfaceMatching.PcaEffectiveDate = pcaEffectiveDate;
    entities.ForInterfaceMatching.FunIdentifier = funIdentifier;
    entities.ForInterfaceMatching.FtrIdentifier = ftrIdentifier;
    entities.ForInterfaceMatching.CashBalanceAmt = cashBalanceAmt;
    entities.ForInterfaceMatching.CashBalanceReason = cashBalanceReason;
    entities.ForInterfaceMatching.LastUpdatedBy = lastUpdatedBy;
    entities.ForInterfaceMatching.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ForInterfaceMatching.Populated = true;
  }

  private void UpdateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);

    var discontinueDate = Now().Date;

    entities.CashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.CashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private void UpdateControlTable()
  {
    var lastUsedNumber = local.NextCashReceiptSeqNum.LastUsedNumber;

    entities.CashReceiptSequentialNum.Populated = false;
    Update("UpdateControlTable",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(
          command, "cntlTblId", entities.CashReceiptSequentialNum.Identifier);
      });

    entities.CashReceiptSequentialNum.LastUsedNumber = lastUsedNumber;
    entities.CashReceiptSequentialNum.Populated = true;
  }

  private void UpdateFundTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var amount = local.FundTransaction.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.FundTransaction.Populated = false;
    Update("UpdateFundTransaction",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetString(command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fundTransId",
          entities.FundTransaction.SystemGeneratedIdentifier);
      });

    entities.FundTransaction.Amount = amount;
    entities.FundTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.FundTransaction.LastUpdatedTmst = lastUpdatedTmst;
    entities.FundTransaction.Populated = true;
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
    /// A value of TxtYyyymm.
    /// </summary>
    [JsonPropertyName("txtYyyymm")]
    public WorkArea TxtYyyymm
    {
      get => txtYyyymm ??= new();
      set => txtYyyymm = value;
    }

    /// <summary>
    /// A value of TxtDate.
    /// </summary>
    [JsonPropertyName("txtDate")]
    public TextWorkArea TxtDate
    {
      get => txtDate ??= new();
      set => txtDate = value;
    }

    /// <summary>
    /// A value of TxtDatevvvvvvvvvvvvvvvvvvvv.
    /// </summary>
    [JsonPropertyName("txtDatevvvvvvvvvvvvvvvvvvvv")]
    public DateWorkArea TxtDatevvvvvvvvvvvvvvvvvvvv
    {
      get => txtDatevvvvvvvvvvvvvvvvvvvv ??= new();
      set => txtDatevvvvvvvvvvvvvvvvvvvv = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of EndOfMonth.
    /// </summary>
    [JsonPropertyName("endOfMonth")]
    public CashReceiptEvent EndOfMonth
    {
      get => endOfMonth ??= new();
      set => endOfMonth = value;
    }

    /// <summary>
    /// A value of StartOfMonth.
    /// </summary>
    [JsonPropertyName("startOfMonth")]
    public CashReceiptEvent StartOfMonth
    {
      get => startOfMonth ??= new();
      set => startOfMonth = value;
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
    /// A value of CaseInd.
    /// </summary>
    [JsonPropertyName("caseInd")]
    public Common CaseInd
    {
      get => caseInd ??= new();
      set => caseInd = value;
    }

    /// <summary>
    /// A value of CaseOrCourtOrderNbr.
    /// </summary>
    [JsonPropertyName("caseOrCourtOrderNbr")]
    public CsePerson CaseOrCourtOrderNbr
    {
      get => caseOrCourtOrderNbr ??= new();
      set => caseOrCourtOrderNbr = value;
    }

    /// <summary>
    /// A value of RollbackForTestInd.
    /// </summary>
    [JsonPropertyName("rollbackForTestInd")]
    public Common RollbackForTestInd
    {
      get => rollbackForTestInd ??= new();
      set => rollbackForTestInd = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public CashReceiptEvent ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of ForInterfaceMatching.
    /// </summary>
    [JsonPropertyName("forInterfaceMatching")]
    public CashReceipt ForInterfaceMatching
    {
      get => forInterfaceMatching ??= new();
      set => forInterfaceMatching = value;
    }

    /// <summary>
    /// A value of TextEftIdentifier.
    /// </summary>
    [JsonPropertyName("textEftIdentifier")]
    public WorkArea TextEftIdentifier
    {
      get => textEftIdentifier ??= new();
      set => textEftIdentifier = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of InitializedCsePersonAddress.
    /// </summary>
    [JsonPropertyName("initializedCsePersonAddress")]
    public CsePersonAddress InitializedCsePersonAddress
    {
      get => initializedCsePersonAddress ??= new();
      set => initializedCsePersonAddress = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of TraceIndicator.
    /// </summary>
    [JsonPropertyName("traceIndicator")]
    public Common TraceIndicator
    {
      get => traceIndicator ??= new();
      set => traceIndicator = value;
    }

    /// <summary>
    /// A value of HardcodedDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedDeposited")]
    public FundTransactionType HardcodedDeposited
    {
      get => hardcodedDeposited ??= new();
      set => hardcodedDeposited = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of MaximumDiscontinueDate.
    /// </summary>
    [JsonPropertyName("maximumDiscontinueDate")]
    public DateWorkArea MaximumDiscontinueDate
    {
      get => maximumDiscontinueDate ??= new();
      set => maximumDiscontinueDate = value;
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

    /// <summary>
    /// A value of RetryCount.
    /// </summary>
    [JsonPropertyName("retryCount")]
    public Common RetryCount
    {
      get => retryCount ??= new();
      set => retryCount = value;
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
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public CashReceipt ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of NextCashReceiptSeqNum.
    /// </summary>
    [JsonPropertyName("nextCashReceiptSeqNum")]
    public ControlTable NextCashReceiptSeqNum
    {
      get => nextCashReceiptSeqNum ??= new();
      set => nextCashReceiptSeqNum = value;
    }

    /// <summary>
    /// A value of HardcodedEmployer.
    /// </summary>
    [JsonPropertyName("hardcodedEmployer")]
    public CashReceiptSourceType HardcodedEmployer
    {
      get => hardcodedEmployer ??= new();
      set => hardcodedEmployer = value;
    }

    /// <summary>
    /// A value of HardcodedFdso.
    /// </summary>
    [JsonPropertyName("hardcodedFdso")]
    public CashReceiptSourceType HardcodedFdso
    {
      get => hardcodedFdso ??= new();
      set => hardcodedFdso = value;
    }

    /// <summary>
    /// A value of HardcodedMisc.
    /// </summary>
    [JsonPropertyName("hardcodedMisc")]
    public CashReceiptSourceType HardcodedMisc
    {
      get => hardcodedMisc ??= new();
      set => hardcodedMisc = value;
    }

    /// <summary>
    /// A value of NumericString.
    /// </summary>
    [JsonPropertyName("numericString")]
    public WorkArea NumericString
    {
      get => numericString ??= new();
      set => numericString = value;
    }

    /// <summary>
    /// A value of NumericInd.
    /// </summary>
    [JsonPropertyName("numericInd")]
    public Common NumericInd
    {
      get => numericInd ??= new();
      set => numericInd = value;
    }

    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public ProgramRun Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of PaymentAmount.
    /// </summary>
    [JsonPropertyName("paymentAmount")]
    public CashReceiptDetail PaymentAmount
    {
      get => paymentAmount ??= new();
      set => paymentAmount = value;
    }

    /// <summary>
    /// A value of PassCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("passCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory PassCashReceiptDetailStatHistory
    {
      get => passCashReceiptDetailStatHistory ??= new();
      set => passCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of Supprsn.
    /// </summary>
    [JsonPropertyName("supprsn")]
    public CashReceiptDetailStatHistory Supprsn
    {
      get => supprsn ??= new();
      set => supprsn = value;
    }

    /// <summary>
    /// A value of HardcodedReleasedCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("hardcodedReleasedCashReceiptDetailStatus")]
    public CashReceiptDetailStatus HardcodedReleasedCashReceiptDetailStatus
    {
      get => hardcodedReleasedCashReceiptDetailStatus ??= new();
      set => hardcodedReleasedCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of HardcodedPendedCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("hardcodedPendedCashReceiptDetailStatus")]
    public CashReceiptDetailStatus HardcodedPendedCashReceiptDetailStatus
    {
      get => hardcodedPendedCashReceiptDetailStatus ??= new();
      set => hardcodedPendedCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of HardcodedSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedSuspended")]
    public CashReceiptDetailStatus HardcodedSuspended
    {
      get => hardcodedSuspended ??= new();
      set => hardcodedSuspended = value;
    }

    /// <summary>
    /// A value of PassCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("passCashReceiptDetail")]
    public CashReceiptDetail PassCashReceiptDetail
    {
      get => passCashReceiptDetail ??= new();
      set => passCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of HardcodedRegular.
    /// </summary>
    [JsonPropertyName("hardcodedRegular")]
    public CollectionType HardcodedRegular
    {
      get => hardcodedRegular ??= new();
      set => hardcodedRegular = value;
    }

    /// <summary>
    /// A value of HardcodedStateOffset.
    /// </summary>
    [JsonPropertyName("hardcodedStateOffset")]
    public CollectionType HardcodedStateOffset
    {
      get => hardcodedStateOffset ??= new();
      set => hardcodedStateOffset = value;
    }

    /// <summary>
    /// A value of HardcodedIwo.
    /// </summary>
    [JsonPropertyName("hardcodedIwo")]
    public CollectionType HardcodedIwo
    {
      get => hardcodedIwo ??= new();
      set => hardcodedIwo = value;
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
    /// A value of Paid.
    /// </summary>
    [JsonPropertyName("paid")]
    public ElectronicFundTransmission Paid
    {
      get => paid ??= new();
      set => paid = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of TotalEft.
    /// </summary>
    [JsonPropertyName("totalEft")]
    public Common TotalEft
    {
      get => totalEft ??= new();
      set => totalEft = value;
    }

    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HardcodedReleasedCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("hardcodedReleasedCashReceiptStatus")]
    public CashReceiptStatus HardcodedReleasedCashReceiptStatus
    {
      get => hardcodedReleasedCashReceiptStatus ??= new();
      set => hardcodedReleasedCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HardcodedPendedCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("hardcodedPendedCashReceiptStatus")]
    public CashReceiptStatus HardcodedPendedCashReceiptStatus
    {
      get => hardcodedPendedCashReceiptStatus ??= new();
      set => hardcodedPendedCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public ElectronicFundTransmission Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Inbound.
    /// </summary>
    [JsonPropertyName("inbound")]
    public ElectronicFundTransmission Inbound
    {
      get => inbound ??= new();
      set => inbound = value;
    }

    /// <summary>
    /// A value of EftCashReceiptType.
    /// </summary>
    [JsonPropertyName("eftCashReceiptType")]
    public CashReceiptType EftCashReceiptType
    {
      get => eftCashReceiptType ??= new();
      set => eftCashReceiptType = value;
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
    /// A value of EftHardcoded.
    /// </summary>
    [JsonPropertyName("eftHardcoded")]
    public CashReceiptType EftHardcoded
    {
      get => eftHardcoded ??= new();
      set => eftHardcoded = value;
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
    /// A value of EftCashReceipt.
    /// </summary>
    [JsonPropertyName("eftCashReceipt")]
    public CashReceipt EftCashReceipt
    {
      get => eftCashReceipt ??= new();
      set => eftCashReceipt = value;
    }

    /// <summary>
    /// A value of EftCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("eftCashReceiptEvent")]
    public CashReceiptEvent EftCashReceiptEvent
    {
      get => eftCashReceiptEvent ??= new();
      set => eftCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of EftCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("eftCashReceiptSourceType")]
    public CashReceiptSourceType EftCashReceiptSourceType
    {
      get => eftCashReceiptSourceType ??= new();
      set => eftCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of NextEftId.
    /// </summary>
    [JsonPropertyName("nextEftId")]
    public CashReceiptDetail NextEftId
    {
      get => nextEftId ??= new();
      set => nextEftId = value;
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
    /// A value of NextId.
    /// </summary>
    [JsonPropertyName("nextId")]
    public ProgramError NextId
    {
      get => nextId ??= new();
      set => nextId = value;
    }

    /// <summary>
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
    }

    /// <summary>
    /// A value of ControlCount.
    /// </summary>
    [JsonPropertyName("controlCount")]
    public Common ControlCount
    {
      get => controlCount ??= new();
      set => controlCount = value;
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
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
    /// Gets a value of Totals.
    /// </summary>
    [JsonPropertyName("totals")]
    public TotalsGroup Totals
    {
      get => totals ?? (totals = new());
      set => totals = value;
    }

    /// <summary>
    /// A value of ForClose.
    /// </summary>
    [JsonPropertyName("forClose")]
    public CsePersonsWorkSet ForClose
    {
      get => forClose ??= new();
      set => forClose = value;
    }

    private WorkArea txtYyyymm;
    private TextWorkArea txtDate;
    private DateWorkArea txtDatevvvvvvvvvvvvvvvvvvvv;
    private DateWorkArea initializedDateWorkArea;
    private CashReceiptEvent endOfMonth;
    private CashReceiptEvent startOfMonth;
    private CashReceiptEvent cashReceiptEvent;
    private Common caseInd;
    private CsePerson caseOrCourtOrderNbr;
    private Common rollbackForTestInd;
    private CashReceiptEvent forCreate;
    private External external;
    private CashReceipt forInterfaceMatching;
    private WorkArea textEftIdentifier;
    private WorkArea workArea;
    private CsePersonAddress initializedCsePersonAddress;
    private CsePersonAddress csePersonAddress;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private Common traceIndicator;
    private FundTransactionType hardcodedDeposited;
    private FundTransaction fundTransaction;
    private DateWorkArea maximumDiscontinueDate;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail cashReceiptDetail;
    private Common retryCount;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt forUpdate;
    private ControlTable nextCashReceiptSeqNum;
    private CashReceiptSourceType hardcodedEmployer;
    private CashReceiptSourceType hardcodedFdso;
    private CashReceiptSourceType hardcodedMisc;
    private WorkArea numericString;
    private Common numericInd;
    private ProgramRun init1;
    private CashReceiptDetail paymentAmount;
    private CashReceiptDetailStatHistory passCashReceiptDetailStatHistory;
    private CashReceiptDetailStatHistory supprsn;
    private CashReceiptDetailStatus hardcodedReleasedCashReceiptDetailStatus;
    private CashReceiptDetailStatus hardcodedPendedCashReceiptDetailStatus;
    private CashReceiptDetailStatus hardcodedSuspended;
    private CashReceiptDetail passCashReceiptDetail;
    private CollectionType hardcodedRegular;
    private CollectionType hardcodedStateOffset;
    private CollectionType hardcodedIwo;
    private CollectionType collectionType;
    private ElectronicFundTransmission paid;
    private DateWorkArea process;
    private Common totalEft;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatus hardcodedReleasedCashReceiptStatus;
    private CashReceiptStatus hardcodedPendedCashReceiptStatus;
    private ElectronicFundTransmission error;
    private ElectronicFundTransmission inbound;
    private CashReceiptType eftCashReceiptType;
    private DateWorkArea null1;
    private CashReceiptType eftHardcoded;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CashReceipt eftCashReceipt;
    private CashReceiptEvent eftCashReceiptEvent;
    private CashReceiptSourceType eftCashReceiptSourceType;
    private CashReceiptDetail nextEftId;
    private ExitStateWorkArea exitStateWorkArea;
    private ProgramError nextId;
    private Common firstTimeThru;
    private Common controlCount;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ElectronicFundTransmission electronicFundTransmission;
    private DateWorkArea current;
    private TotalsGroup totals;
    private CsePersonsWorkSet forClose;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
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
    /// A value of CashReceiptSequentialNum.
    /// </summary>
    [JsonPropertyName("cashReceiptSequentialNum")]
    public ControlTable CashReceiptSequentialNum
    {
      get => cashReceiptSequentialNum ??= new();
      set => cashReceiptSequentialNum = value;
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
    /// A value of ForInterfaceMatching.
    /// </summary>
    [JsonPropertyName("forInterfaceMatching")]
    public CashReceipt ForInterfaceMatching
    {
      get => forInterfaceMatching ??= new();
      set => forInterfaceMatching = value;
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
    /// A value of Deposited.
    /// </summary>
    [JsonPropertyName("deposited")]
    public CashReceiptStatus Deposited
    {
      get => deposited ??= new();
      set => deposited = value;
    }

    /// <summary>
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
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
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public CashReceiptType Eft
    {
      get => eft ??= new();
      set => eft = value;
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
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of InterstateCostRecoveryFee.
    /// </summary>
    [JsonPropertyName("interstateCostRecoveryFee")]
    public CashReceiptDetailFeeType InterstateCostRecoveryFee
    {
      get => interstateCostRecoveryFee ??= new();
      set => interstateCostRecoveryFee = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    private FundTransactionType fundTransactionType;
    private CashReceiptDetailFee cashReceiptDetailFee;
    private ElectronicFundTransmission electronicFundTransmission;
    private CollectionType collectionType;
    private ControlTable cashReceiptSequentialNum;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt forInterfaceMatching;
    private CashReceipt cashReceipt;
    private CashReceiptStatus deposited;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType eft;
    private CashReceiptDetailStatus released;
    private CashReceiptDetailStatus suspended;
    private CashReceiptDetailStatus pended;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetailFeeType interstateCostRecoveryFee;
    private FundTransaction fundTransaction;
  }
#endregion
}
