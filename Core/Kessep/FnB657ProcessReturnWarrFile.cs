// Program: FN_B657_PROCESS_RETURN_WARR_FILE, ID: 372723762, model: 746.
// Short name: SWEF657B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B657_PROCESS_RETURN_WARR_FILE.
/// </para>
/// <para>
/// This skeleton is an example that uses:
/// A sequential driver file
/// An external to do a DB2 commit
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB657ProcessReturnWarrFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B657_PROCESS_RETURN_WARR_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB657ProcessReturnWarrFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB657ProcessReturnWarrFile.
  /// </summary>
  public FnB657ProcessReturnWarrFile(IContext context, Import import,
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
    // *********************************************************************************************************
    // *                     MAINTENANCE LOG
    // 
    // *
    // *
    // 
    // *
    // *   DATE    REQUEST #    
    // PROGRAMMER   DESCRIPTION
    // 
    // *
    // * --------  ---------    
    // ----------   -----------
    // 
    // *
    // * 10/16/00  WR # 226     E Lyman     Automate the ReIssue of Warrants
    // *
    // * 02/01/01  WR # 263     K Doshi     Replace DOA status with KPC.
    // *
    // * 02/01/01  WR # 10347   M Kumar     Enhancement to control report .
    // *
    // * 02/01/01  WR # 20139   M Kumar     Should accept and process type 7 
    // records . 			*
    // * 12/12/01  WR # 20147   K 
    // Doshi     KPC Recoupment.
    // 
    // *
    // * 12/19/02  PR # 162884  M Fangman   Added code to reset a flag and to 
    // print out			*
    // *
    // 
    // details on bypassed records.
    // *
    // * 03/20/03  PR # 173850  M Fangman   Added code to obtain a unique 
    // Payment Request ID.  Added code to 	*
    // *				     print out more info for the AE error.  Changed abend to use the
    // *
    // *				     standard batch abort exit state.					*
    // * 09/24/07  WR # 280422  GVandy      Support disbursements via Debit 
    // Cards. 				*
    // *********************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeRead.FileInstruction = "READ";
    local.PaidPaymentStatus.SystemGeneratedIdentifier = 3;
    local.ReissuedPaymentStatus.SystemGeneratedIdentifier = 8;
    local.NewLastCheckNo.Number = "000000000";
    local.NewLastEftNo.Number = "E00000000";
    local.NewLastDcNo.Number = "C00000000";
    local.CurrentDate.Date = Now().Date;
    local.ReissuedPaymentStatusHistory.ReasonText =
      "CHECK REISSUED BY KANSAS PAY CENTER";
    local.PaidPaymentStatusHistory.ReasonText =
      "CHECK ISSUED BY KANSAS PAY CENTER";
    local.CancelAndReissue.ReasonText =
      "OLD CHECK STOPPED AND NEW CHECK REISSUED BY KANSAS PAY CENTER";
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // ***** Get the run parameters for this program.
    UseFnB657Housekeeping();

    // ----- Start of changes for PR 162884 - missing pmts from KPC -----
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "Rec    Pmt      Pmt   Check        Prt    Recoup";
    UseCabErrorReport1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Typ   Req Id    Typ   Number       Dt     Ind KPC";
    UseCabErrorReport1();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ----- End of changes for PR 162884 - missing pmts from KPC -----
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (ReadControlTable1())
      {
        local.OldLastCheckNo.Number =
          NumberToString(entities.Check.LastUsedNumber, 7, 9);
      }
      else
      {
        ExitState = "FN0000_CONTROL_TABLE_RECORD_NF";

        return;
      }

      if (ReadControlTable3())
      {
        local.OldLastEftNo.Number =
          NumberToString(entities.Eft.LastUsedNumber, 7, 9);
        local.OldLastEftNo.Number = "E" + Substring
          (local.OldLastEftNo.Number, 9, 2, 8);
      }
      else
      {
        ExitState = "FN0000_CONTROL_TABLE_RECORD_NF";

        return;
      }

      if (ReadControlTable2())
      {
        local.OldLastDcNo.Number =
          NumberToString(entities.DebitCard.LastUsedNumber, 7, 9);
        local.OldLastDcNo.Number = "C" + Substring
          (local.OldLastDcNo.Number, 9, 2, 8);
      }
      else
      {
        ExitState = "FN0000_CONTROL_TABLE_RECORD_NF";

        return;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (local.FileInError.ReportNumber == 99)
      {
        // : There was an error opening the error file, so we can't use
        //   the error report .
      }
      else
      {
        UseEabExtractExitStateMessage();

        if (local.FileInError.ReportNumber == 98)
        {
          // : Problem opening the control file.
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " Control File";
        }
        else
        {
          local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport1();
      }

      return;
    }

    // *** Read the PAID and REIS payment_status to have currency on them ***
    if (ReadPaymentStatus1())
    {
      if (!ReadPaymentStatus2())
      {
        ExitState = "FN0000_PYMNT_STAT_NF";
      }
    }
    else
    {
      ExitState = "FN0000_PYMNT_STAT_NF";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***** Process Input file from KPC.
    do
    {
      // ***** Call external to read the driver file.
      local.PassArea.FileInstruction = local.HardcodeRead.FileInstruction;
      UseFnAccessDoaTape();

      switch(TrimEnd(local.PassArea.TextReturnCode))
      {
        case "02":
          if (ReadPaymentRequest1())
          {
            if (!Equal(entities.Existing.Type1, "WAR"))
            {
              ExitState = "FN0000_PMNT_REQ_NOT_WARRANT";
            }
            else if (IsEmpty(entities.Existing.Number))
            {
              // : Update the payment request if the number is spaces.  If the
              //  number is not spaces, we are probably running with the same 
              // file
              //  as the night before, so do not want to process the record 
              // again.
              if (Equal(local.FromFile.PrintDate, new DateTime(1, 1, 1)))
              {
                local.FromFile.PrintDate =
                  AddDays(entities.Existing.ProcessDate, 1);
              }

              try
              {
                UpdatePaymentRequest();

                // *** Continue Processing
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PAYMENT_REQUEST_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PAYMENT_REQUEST_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              if (CharAt(local.FromFile.Number, 1) == 'E')
              {
                if (Lt(entities.Existing.Number, local.OldLastEftNo.Number))
                {
                  local.EftBelowLastUsedNumber.Flag = "Y";
                }

                if (Lt(local.NewLastEftNo.Number, entities.Existing.Number))
                {
                  local.NewLastEftNo.Number = entities.Existing.Number;
                }

                ++local.NumberOfEftsPaid.Count;
              }
              else if (CharAt(local.FromFile.Number, 1) == 'C')
              {
                if (Lt(entities.Existing.Number, local.OldLastDcNo.Number))
                {
                  local.DcBelowLastUsedNumber.Flag = "Y";
                }

                if (Lt(local.NewLastDcNo.Number, entities.Existing.Number))
                {
                  local.NewLastDcNo.Number = entities.Existing.Number;
                }

                ++local.NumberOfDcsPaid.Count;
              }
              else
              {
                if (Lt(entities.Existing.Number, local.OldLastCheckNo.Number))
                {
                  local.ChkBelowLastUsedNumber.Flag = "Y";
                }

                if (Lt(local.NewLastCheckNo.Number, entities.Existing.Number))
                {
                  local.NewLastCheckNo.Number = entities.Existing.Number;
                }

                ++local.NumberOfChecksPaid.Count;
              }
            }
            else
            {
              if (CharAt(local.FromFile.Number, 1) == 'E')
              {
                ++local.NumberOfEftsBypassed.Count;
              }
              else if (CharAt(local.FromFile.Number, 1) == 'C')
              {
                ++local.NumberOfDcsBypassed.Count;
              }
              else
              {
                ++local.NumberOfChecksBypassed.Count;
              }

              // ----- Start of changes for PR 162884 - missing pmts from KPC 
              // -----
              // ***** Bypassed records should be written to the error report. 
              // *****
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "The following KPC record will be bypassed because the Original payment request has already been processed.";
                
              UseCabErrorReport1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = local.PassArea.TextReturnCode + "  " +
                NumberToString
                (local.FromFile.SystemGeneratedIdentifier, 7, 9) + "  " + local
                .FromFile.Type1 + "  " + (local.FromFile.Number ?? "") + "  " +
                NumberToString(DateToInt(local.FromFile.PrintDate), 8, 8) + "  " +
                (local.FromFile.RecoupmentIndKpc ?? "");
              UseCabErrorReport1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              // ----- End of changes for PR 162884 - missing pmts from KPC 
              // -----
              continue;
            }
          }
          else
          {
            ExitState = "FN0000_PAYMENT_REQUEST_NF";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "  Payment Request ID is:  " +
              NumberToString(local.FromFile.SystemGeneratedIdentifier, 15);
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (ReadPaymentStatusHistory())
          {
            // : Discontinue the payment status history for status of KPC.
            try
            {
              UpdatePaymentStatusHistory();

              // : Create a new Payment Status History with status of 'PAID'.
              for(local.Counter.Count = 1; local.Counter.Count <= 10; ++
                local.Counter.Count)
              {
                try
                {
                  CreatePaymentStatusHistory1();

                  // All Ok - continue
                  goto Read1;
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      // : Try again
                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                      goto Read1;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              // : If the logic comes here, we could not find a unique 
              // identifier
              //   for the new payment status history.
              ExitState = "FN0000_PYMNT_STATUS_AE";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_PMNT_STAT_HIST_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

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
            ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";
          }

Read1:

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "  Payment Request ID is:  " +
              NumberToString(local.FromFile.SystemGeneratedIdentifier, 15);
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          break;
        case "06":
          // ***   Read the original warrant that is being reissued.
          // ***   Set status to reissued.
          if (ReadPaymentRequest1())
          {
            if (ReadPaymentStatusHistoryPaymentStatus1())
            {
              if (entities.PaymentStatus.SystemGeneratedIdentifier == local
                .ReissuedPaymentStatus.SystemGeneratedIdentifier)
              {
                if (CharAt(local.FromFile.Number, 1) == 'E')
                {
                  ++local.NumberOfEftsBypassed.Count;
                }
                else if (CharAt(local.FromFile.Number, 1) == 'C')
                {
                  ++local.NumberOfDcsBypassed.Count;
                }
                else
                {
                  ++local.NumberOfChecksBypassed.Count;
                }

                // ----- Start of changes for PR 162884 - missing pmts from KPC 
                // -----
                // ***** Bypassed records should be written to the error report.
                // *****
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "The following KPC record will be bypassed because the payment request is already reissued.";
                  
                UseCabErrorReport1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.EabReportSend.RptDetail =
                  local.PassArea.TextReturnCode + "  " + NumberToString
                  (local.FromFile.SystemGeneratedIdentifier, 7, 9) + "  " + local
                  .FromFile.Type1 + "  " + (local.FromFile.Number ?? "") + "  " +
                  NumberToString(DateToInt(local.FromFile.PrintDate), 8, 8) + "  " +
                  (local.FromFile.RecoupmentIndKpc ?? "");
                UseCabErrorReport1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                // ----- End of changes for PR 162884 - missing pmts from KPC 
                // -----
                continue;
              }

              if (CharAt(local.FromFile.Number, 1) == 'E')
              {
                if (Lt(local.FromFile.Number, local.OldLastEftNo.Number))
                {
                  local.EftBelowLastUsedNumber.Flag = "Y";
                }

                ++local.NumberOfEftsReissued.Count;
              }
              else if (CharAt(local.FromFile.Number, 1) == 'C')
              {
                if (Lt(local.FromFile.Number, local.OldLastDcNo.Number))
                {
                  local.DcBelowLastUsedNumber.Flag = "Y";
                }

                ++local.NumberOfDcsReissued.Count;
              }
              else
              {
                if (Lt(local.FromFile.Number, local.OldLastCheckNo.Number))
                {
                  local.ChkBelowLastUsedNumber.Flag = "Y";
                }

                ++local.NumberOfChecksReissued.Count;
              }

              // : Discontinue the existing payment status history.
              try
              {
                UpdatePaymentStatusHistory();

                // : Create a new Payment Status History with status of 'REIS'.
                for(local.Counter.Count = 1; local.Counter.Count <= 10; ++
                  local.Counter.Count)
                {
                  try
                  {
                    CreatePaymentStatusHistory3();

                    // All Ok - continue
                    goto Read2;
                  }
                  catch(Exception e1)
                  {
                    switch(GetErrorCode(e1))
                    {
                      case ErrorCode.AlreadyExists:
                        // : Try again
                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                        goto Read2;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                // : If the logic comes here, we could not find a unique 
                // identifier
                //   for the new payment status history.
                ExitState = "FN0000_PYMNT_STATUS_AE";
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PMNT_STAT_HIST_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

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
              ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";
            }
          }
          else
          {
            ExitState = "FN0000_PAYMENT_REQUEST_NF";
          }

Read2:

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "  Payment Request ID is:  " +
              NumberToString(local.FromFile.SystemGeneratedIdentifier, 15);
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (AsChar(entities.Existing.RecoupmentIndKpc) == 'Y')
          {
            local.PaymentRequest.RecoupmentIndKpc = "Y";
          }
          else
          {
            local.PaymentRequest.RecoupmentIndKpc =
              local.FromFile.RecoupmentIndKpc ?? "";
          }

          local.ForCreate.SystemGeneratedIdentifier = UseFnGetPmtReqId();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "  New Reissued Payment Request ID is:  " +
              NumberToString(local.ForCreate.SystemGeneratedIdentifier, 15);
            UseCabErrorReport1();

            goto AfterCycle;
          }

          try
          {
            CreatePaymentRequest1();

            // *** Create a Payment_Status_Hist of PAID for the new warrant
            for(local.Counter.Count = 1; local.Counter.Count <= 10; ++
              local.Counter.Count)
            {
              try
              {
                CreatePaymentStatusHistory2();

                // All Ok - continue
                goto Create1;
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    // : Try again
                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                    goto AfterCycle;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // : If the logic comes here, we could not find a unique identifier
            //   for the new payment status history.
            ExitState = "FN0000_PYMNT_STATUS_AE";

            goto AfterCycle;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PAYMENT_REQUEST_AE_RB";

                goto AfterCycle;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

                goto AfterCycle;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

Create1:

          if (CharAt(local.FromFile.Number, 1) == 'E')
          {
            local.EabReportSend.RptDetail = "REISSUE: old EFT # " + entities
              .Existing.Number + " new EFT # " + (
                local.FromFile.Number ?? "") + " amount " + NumberToString
              ((long)(entities.Existing.Amount * 100), 15);

            if (Lt(local.NewLastEftNo.Number,
              entities.ReissuedPaymentRequest.Number))
            {
              local.NewLastEftNo.Number =
                entities.ReissuedPaymentRequest.Number;
            }
          }
          else if (CharAt(local.FromFile.Number, 1) == 'C')
          {
            local.EabReportSend.RptDetail = "REISSUE: old Debit Card # " + entities
              .Existing.Number + " new Debit Card # " + (
                local.FromFile.Number ?? "") + " amount " + NumberToString
              ((long)(entities.Existing.Amount * 100), 15);

            if (Lt(local.NewLastDcNo.Number,
              entities.ReissuedPaymentRequest.Number))
            {
              local.NewLastDcNo.Number = entities.ReissuedPaymentRequest.Number;
            }
          }
          else
          {
            local.EabReportSend.RptDetail = "REISSUE: old check # " + entities
              .Existing.Number + " new check # " + (
                local.FromFile.Number ?? "") + " amount " + NumberToString
              ((long)(entities.Existing.Amount * 100), 15);

            if (Lt(local.NewLastCheckNo.Number,
              entities.ReissuedPaymentRequest.Number))
            {
              local.NewLastCheckNo.Number =
                entities.ReissuedPaymentRequest.Number;
            }
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabControlReport1();

          break;
        case "07":
          // ***   Read the reissued warrant that is being reissued .
          // ***   Set status to reissued.
          local.ReisReis.SystemGeneratedIdentifier =
            local.FromFile.SystemGeneratedIdentifier;

          if (!ReadPaymentRequest2())
          {
            ExitState = "FN0000_PAYMENT_REQUEST_NF";
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "  Payment Request ID is:  " +
              NumberToString(local.FromFile.SystemGeneratedIdentifier, 15);
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            continue;
          }

          local.NoOfTimesChkEftReissd.Count = 0;

          // ----- Start of changes for PR 162884 - missing pmts from KPC -----
          local.LastPmtRqstWithReisSt.Flag = "N";

          // ----- End of changes for PR 162884 - missing pmts from KPC -----
          do
          {
            if (ReadPaymentRequest3())
            {
              if (ReadPaymentStatusHistoryPaymentStatus2())
              {
                ++local.NoOfTimesChkEftReissd.Count;
                local.ReisReis.SystemGeneratedIdentifier =
                  entities.ReissuedPaymentRequest.SystemGeneratedIdentifier;
              }
              else
              {
                ExitState = "FN0000_PAYMENT_STATUS_HISTORY_NF";

                break;
              }
            }
            else
            {
              local.LastPmtRqstWithReisSt.Flag = "Y";
            }
          }
          while(AsChar(local.LastPmtRqstWithReisSt.Flag) != 'Y');

          if (local.NoOfTimesChkEftReissd.Count > 0 && IsExitState
            ("ACO_NN0000_ALL_OK"))
          {
            if (ReadPaymentRequest4())
            {
              if (Equal(entities.ReissuedPaymentRequest.Number,
                local.FromFile.Number))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "The check/EFT/debit card with number " + (
                    local.FromFile.Number ?? "") + " is reissued more than once .";
                  
                UseCabErrorReport1();

                break;
              }
            }
            else
            {
              break;
            }
          }
          else
          {
            // *****************************************************
            //    Write to the error report that the chk is
            //  not a reissued reissued check .
            // *****************************************************
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = " The check/EFT/debit card " + (
              local.FromFile.Number ?? "") + " is not a reissued reissued check .";
              
            UseCabErrorReport1();

            continue;
          }

          // ********************************************************
          //      The Check/EFT  which was reissued more
          //  than once is found .
          // ********************************************************
          if (ReadPaymentStatusHistoryPaymentStatus2())
          {
            if (entities.PaymentStatus.SystemGeneratedIdentifier == local
              .ReissuedPaymentStatus.SystemGeneratedIdentifier)
            {
              if (CharAt(local.FromFile.Number, 1) == 'E')
              {
                ++local.NoEftsBypassMoreThan1.Count;
              }
              else if (CharAt(local.FromFile.Number, 1) == 'C')
              {
                ++local.NoDcsBypassMoreThan1.Count;
              }
              else
              {
                ++local.NoChksBypassMoreThan1.Count;
              }

              // ----- Start of changes for PR 162884 - missing pmts from KPC 
              // -----
              // ***** Bypassed records should be written to the error report. 
              // *****
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "The following KPC record will be bypassed because the last reissued payment request is already reissued.";
                
              UseCabErrorReport1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.EabReportSend.RptDetail = local.PassArea.TextReturnCode + "  " +
                NumberToString
                (local.FromFile.SystemGeneratedIdentifier, 7, 9) + "  " + local
                .FromFile.Type1 + "  " + (local.FromFile.Number ?? "") + "  " +
                NumberToString(DateToInt(local.FromFile.PrintDate), 8, 8) + "  " +
                (local.FromFile.RecoupmentIndKpc ?? "");
              UseCabErrorReport1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              // ----- End of changes for PR 162884 - missing pmts from KPC 
              // -----
              continue;
            }

            if (CharAt(local.FromFile.Number, 1) == 'E')
            {
              if (Lt(local.FromFile.Number, local.OldLastEftNo.Number))
              {
                local.EftBelowLastUsedNumber.Flag = "Y";
              }

              ++local.NoEftsReisMoreThan1.Count;
            }
            else if (CharAt(local.FromFile.Number, 1) == 'C')
            {
              if (Lt(local.FromFile.Number, local.OldLastDcNo.Number))
              {
                local.DcBelowLastUsedNumber.Flag = "Y";
              }

              ++local.NoDcsReisMoreThan1.Count;
            }
            else
            {
              if (Lt(local.FromFile.Number, local.OldLastCheckNo.Number))
              {
                local.ChkBelowLastUsedNumber.Flag = "Y";
              }

              ++local.NoChecksReisMoreThan1.Count;
            }

            // : Discontinue the existing payment status history.
            try
            {
              UpdatePaymentStatusHistory();

              // : Create a new Payment Status History with status of 'REIS'.
              for(local.Counter.Count = 1; local.Counter.Count <= 10; ++
                local.Counter.Count)
              {
                try
                {
                  CreatePaymentStatusHistory4();

                  // All Ok - continue
                  goto Read3;
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      // : Try again
                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                      goto Read3;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              // : If the logic comes here, we could not find a unique 
              // identifier
              //   for the new payment status history.
              ExitState = "FN0000_PYMNT_STATUS_AE";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_PMNT_STAT_HIST_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

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
            ExitState = "FN0000_PYMNT_STAT_HISTORY_NF";
          }

Read3:

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "  Payment Request ID is:  " +
              NumberToString(local.FromFile.SystemGeneratedIdentifier, 15);
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          if (AsChar(entities.ReissuedPaymentRequest.RecoupmentIndKpc) == 'Y')
          {
            local.PaymentRequest.RecoupmentIndKpc = "Y";
          }
          else
          {
            local.PaymentRequest.RecoupmentIndKpc =
              local.FromFile.RecoupmentIndKpc ?? "";
          }

          local.ForCreate.SystemGeneratedIdentifier = UseFnGetPmtReqId();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              TrimEnd(local.ExitStateWorkArea.Message) + "  New Reissued Reissued Payment Request ID is:  " +
              NumberToString(local.ForCreate.SystemGeneratedIdentifier, 15);
            UseCabErrorReport1();

            goto AfterCycle;
          }

          try
          {
            CreatePaymentRequest2();

            for(local.Counter.Count = 1; local.Counter.Count <= 10; ++
              local.Counter.Count)
            {
              try
              {
                CreatePaymentStatusHistory5();

                // All Ok.
                goto Create2;
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    // Try again.
                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_STAT_HISTORY_PV";

                    goto AfterCycle;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // ************************************************
            //   If it comes here we could not find a
            //  unique identifier.
            // ************************************************
            ExitState = "FN0000_PYMNT_STATUS_AE";

            goto AfterCycle;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PAYMENT_REQUEST_AE_RB";

                goto AfterCycle;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

                goto AfterCycle;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

Create2:

          if (CharAt(local.FromFile.Number, 1) == 'E')
          {
            local.EabReportSend.RptDetail = "REISSUE: old EFT # " + entities
              .ReissuedPaymentRequest.Number + " new EFT # " + (
                local.FromFile.Number ?? "") + " amount " + NumberToString
              ((long)(entities.ReissuedReissued.Amount * 100), 15);

            if (Lt(local.NewLastEftNo.Number, entities.ReissuedReissued.Number))
            {
              local.NewLastEftNo.Number = entities.ReissuedReissued.Number;
            }
          }
          else if (CharAt(local.FromFile.Number, 1) == 'C')
          {
            local.EabReportSend.RptDetail = "REISSUE: old Debit Card # " + entities
              .ReissuedPaymentRequest.Number + " new Debit Card # " + (
                local.FromFile.Number ?? "") + " amount " + NumberToString
              ((long)(entities.ReissuedReissued.Amount * 100), 15);

            if (Lt(local.NewLastDcNo.Number, entities.ReissuedReissued.Number))
            {
              local.NewLastDcNo.Number = entities.ReissuedReissued.Number;
            }
          }
          else
          {
            local.EabReportSend.RptDetail = "REISSUE: old check # " + entities
              .ReissuedPaymentRequest.Number + " new check # " + (
                local.FromFile.Number ?? "") + " amount " + NumberToString
              ((long)(entities.ReissuedReissued.Amount * 100), 15);

            if (Lt(local.NewLastCheckNo.Number, entities.ReissuedReissued.Number))
              
            {
              local.NewLastCheckNo.Number = entities.ReissuedReissued.Number;
            }
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabControlReport1();

          break;
        case "EF":
          // END OF FILE, CLOSE EVERYTHING UP.
          local.TerminateLoopInd.Flag = "Y";

          goto AfterCycle;
        default:
          local.EabReportSend.RptDetail = "Return File Read error: " + local
            .PassArea.TextReturnCode;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }
    }
    while(AsChar(local.TerminateLoopInd.Flag) != 'Y');

AfterCycle:

    // *** NORMAL TERMINATION OF THE PROCEDURE EXECUTION ***
    // *** Create Program_Control_Totals for KPC records Read and Updated ***
    UseFnB657PrintControlTotals();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (ReadControlTable1())
      {
        if (Equal(local.NewLastCheckNo.Number, "000000000"))
        {
          goto Read4;
        }

        try
        {
          UpdateControlTable1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN_UPDATE_ON_CONTROL_TABLE_NU";

              goto Test;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN_UPDATE_ON_CONTROL_TABLE_PV";

              goto Test;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "FN0000_CONTROL_TABLE_RECORD_NF";

        goto Test;
      }

Read4:

      if (ReadControlTable3())
      {
        if (Equal(local.NewLastEftNo.Number, "E00000000"))
        {
          goto Read5;
        }

        try
        {
          UpdateControlTable3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN_UPDATE_ON_CONTROL_TABLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN_UPDATE_ON_CONTROL_TABLE_PV";

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
        ExitState = "FN0000_CONTROL_TABLE_RECORD_NF";
      }

Read5:

      if (ReadControlTable2())
      {
        if (Equal(local.NewLastDcNo.Number, "C00000000"))
        {
          goto Test;
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
              ExitState = "FN_UPDATE_ON_CONTROL_TABLE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN_UPDATE_ON_CONTROL_TABLE_PV";

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
        ExitState = "FN0000_CONTROL_TABLE_RECORD_NF";
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        TrimEnd(local.ExitStateWorkArea.Message) + "  KPC file Payment Request ID is:  " +
        NumberToString(local.ForCreate.SystemGeneratedIdentifier, 15);
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // ----- Start of changes for PR 162884 - missing pmts from KPC -----
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();
    UseCabErrorReport2();

    // ----- End of changes for PR 162884 - missing pmts from KPC -----
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.RecoupmentIndKpc = source.RecoupmentIndKpc;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnAccessDoaTape()
  {
    var useImport = new FnAccessDoaTape.Import();
    var useExport = new FnAccessDoaTape.Export();

    useImport.External.Assign(local.PassArea);
    useExport.External.Assign(local.PassArea);
    MovePaymentRequest(local.FromFile, useExport.PaymentRequest);

    Call(FnAccessDoaTape.Execute, useImport, useExport);

    local.PassArea.Assign(useExport.External);
    local.FromFile.Assign(useExport.PaymentRequest);
  }

  private void UseFnB657Housekeeping()
  {
    var useImport = new FnB657Housekeeping.Import();
    var useExport = new FnB657Housekeeping.Export();

    Call(FnB657Housekeeping.Execute, useImport, useExport);

    local.FileInError.ReportNumber = useExport.FileInError.ReportNumber;
  }

  private void UseFnB657PrintControlTotals()
  {
    var useImport = new FnB657PrintControlTotals.Import();
    var useExport = new FnB657PrintControlTotals.Export();

    useImport.NumberOfChecksReissued.Count = local.NumberOfChecksReissued.Count;
    useImport.DcBelowLastNo.Flag = local.DcBelowLastUsedNumber.Flag;
    useImport.NoDcsBypassedMore1.Count = local.NoDcsBypassMoreThan1.Count;
    useImport.NumberOfDcsPaid.Count = local.NumberOfDcsPaid.Count;
    useImport.NumberOfDcsReissued.Count = local.NumberOfDcsReissued.Count;
    useImport.NumberOfDcsBypassed.Count = local.NumberOfDcsBypassed.Count;
    useImport.DcsReisMoreThan1.Count = local.NoDcsReisMoreThan1.Count;
    useImport.NumberOfChecksPaid.Count = local.NumberOfChecksPaid.Count;
    useImport.NumberOfChecksBypassed.Count = local.NumberOfChecksBypassed.Count;
    useImport.CheckBelowLastNo.Flag = local.ChkBelowLastUsedNumber.Flag;
    useImport.EftBelowLastNo.Flag = local.EftBelowLastUsedNumber.Flag;
    useImport.NoOfChksBypassedMore1.Count = local.NoChksBypassMoreThan1.Count;
    useImport.NoEftsBypassedMore1.Count = local.NoEftsBypassMoreThan1.Count;
    useImport.NumberOfEftsPaid.Count = local.NumberOfEftsPaid.Count;
    useImport.NumberOfEftsReissued.Count = local.NumberOfEftsReissued.Count;
    useImport.NumberOfEftsBypassed.Count = local.NumberOfEftsBypassed.Count;
    useImport.ChecksReisMoreThan1.Count = local.NoChecksReisMoreThan1.Count;
    useImport.EftsReisMoreThan1.Count = local.NoEftsReisMoreThan1.Count;

    Call(FnB657PrintControlTotals.Execute, useImport, useExport);
  }

  private int UseFnGetPmtReqId()
  {
    var useImport = new FnGetPmtReqId.Import();
    var useExport = new FnGetPmtReqId.Export();

    Call(FnGetPmtReqId.Execute, useImport, useExport);

    return useExport.PaymentRequest.SystemGeneratedIdentifier;
  }

  private void CreatePaymentRequest1()
  {
    var systemGeneratedIdentifier = local.ForCreate.SystemGeneratedIdentifier;
    var processDate = local.CurrentDate.Date;
    var amount = entities.Existing.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo =
      entities.Existing.DesignatedPayeeCsePersonNo;
    var csePersonNumber = entities.Existing.CsePersonNumber;
    var imprestFundCode = entities.Existing.ImprestFundCode;
    var classification = entities.Existing.Classification;
    var number = local.FromFile.Number ?? "";
    var printDate = local.FromFile.PrintDate;
    var type1 = entities.Existing.Type1;
    var prqRGeneratedId = entities.Existing.SystemGeneratedIdentifier;
    var recoupmentIndKpc = local.PaymentRequest.RecoupmentIndKpc ?? "";

    CheckValid<PaymentRequest>("Type1", type1);
    entities.ReissuedPaymentRequest.Populated = false;
    Update("CreatePaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.
          SetNullableString(command, "dpCsePerNum", designatedPayeeCsePersonNo);
          
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", imprestFundCode);
        db.SetString(command, "classification", classification);
        db.SetString(command, "recoveryFiller", "");
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", number);
        db.SetNullableDate(command, "printDate", printDate);
        db.SetString(command, "type", type1);
        db.SetNullableInt32(command, "prqRGeneratedId", prqRGeneratedId);
        db.SetNullableString(command, "recoupmentIndKpc", recoupmentIndKpc);
      });

    entities.ReissuedPaymentRequest.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ReissuedPaymentRequest.ProcessDate = processDate;
    entities.ReissuedPaymentRequest.Amount = amount;
    entities.ReissuedPaymentRequest.CreatedBy = createdBy;
    entities.ReissuedPaymentRequest.CreatedTimestamp = createdTimestamp;
    entities.ReissuedPaymentRequest.DesignatedPayeeCsePersonNo =
      designatedPayeeCsePersonNo;
    entities.ReissuedPaymentRequest.CsePersonNumber = csePersonNumber;
    entities.ReissuedPaymentRequest.ImprestFundCode = imprestFundCode;
    entities.ReissuedPaymentRequest.Classification = classification;
    entities.ReissuedPaymentRequest.Number = number;
    entities.ReissuedPaymentRequest.PrintDate = printDate;
    entities.ReissuedPaymentRequest.Type1 = type1;
    entities.ReissuedPaymentRequest.PrqRGeneratedId = prqRGeneratedId;
    entities.ReissuedPaymentRequest.RecoupmentIndKpc = recoupmentIndKpc;
    entities.ReissuedPaymentRequest.Populated = true;
  }

  private void CreatePaymentRequest2()
  {
    var systemGeneratedIdentifier = local.ForCreate.SystemGeneratedIdentifier;
    var processDate = local.CurrentDate.Date;
    var amount = entities.ReissuedPaymentRequest.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo =
      entities.ReissuedPaymentRequest.DesignatedPayeeCsePersonNo;
    var csePersonNumber = entities.ReissuedPaymentRequest.CsePersonNumber;
    var imprestFundCode = entities.ReissuedPaymentRequest.ImprestFundCode;
    var classification = entities.ReissuedPaymentRequest.Classification;
    var number = local.FromFile.Number ?? "";
    var printDate = local.FromFile.PrintDate;
    var type1 = entities.ReissuedPaymentRequest.Type1;
    var prqRGeneratedId =
      entities.ReissuedPaymentRequest.SystemGeneratedIdentifier;
    var recoupmentIndKpc = local.PaymentRequest.RecoupmentIndKpc ?? "";

    CheckValid<PaymentRequest>("Type1", type1);
    entities.ReissuedReissued.Populated = false;
    Update("CreatePaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.
          SetNullableString(command, "dpCsePerNum", designatedPayeeCsePersonNo);
          
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", imprestFundCode);
        db.SetString(command, "classification", classification);
        db.SetString(command, "recoveryFiller", "");
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", number);
        db.SetNullableDate(command, "printDate", printDate);
        db.SetString(command, "type", type1);
        db.SetNullableInt32(command, "prqRGeneratedId", prqRGeneratedId);
        db.SetNullableString(command, "recoupmentIndKpc", recoupmentIndKpc);
      });

    entities.ReissuedReissued.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ReissuedReissued.ProcessDate = processDate;
    entities.ReissuedReissued.Amount = amount;
    entities.ReissuedReissued.CreatedBy = createdBy;
    entities.ReissuedReissued.CreatedTimestamp = createdTimestamp;
    entities.ReissuedReissued.DesignatedPayeeCsePersonNo =
      designatedPayeeCsePersonNo;
    entities.ReissuedReissued.CsePersonNumber = csePersonNumber;
    entities.ReissuedReissued.ImprestFundCode = imprestFundCode;
    entities.ReissuedReissued.Classification = classification;
    entities.ReissuedReissued.Number = number;
    entities.ReissuedReissued.PrintDate = printDate;
    entities.ReissuedReissued.Type1 = type1;
    entities.ReissuedReissued.PrqRGeneratedId = prqRGeneratedId;
    entities.ReissuedReissued.RecoupmentIndKpc = recoupmentIndKpc;
    entities.ReissuedReissued.Populated = true;
  }

  private void CreatePaymentStatusHistory1()
  {
    var pstGeneratedId = entities.Paid.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.Existing.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = Now().Date;
    var discontinueDate = local.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = local.PaidPaymentStatusHistory.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory2()
  {
    var pstGeneratedId = entities.Paid.SystemGeneratedIdentifier;
    var prqGeneratedId =
      entities.ReissuedPaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = local.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = local.ReissuedPaymentStatusHistory.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory3()
  {
    var pstGeneratedId =
      entities.ReissuedPaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.Existing.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = local.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = local.CancelAndReissue.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory3",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory4()
  {
    var pstGeneratedId =
      entities.ReissuedPaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId =
      entities.ReissuedPaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = local.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = local.CancelAndReissue.ReasonText ?? "";

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory4",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = reasonText;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory5()
  {
    var pstGeneratedId = entities.Paid.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.ReissuedReissued.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = local.CurrentDate.Date;
    var discontinueDate = local.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = local.ReissuedPaymentStatusHistory.ReasonText ?? "";

    entities.ReisReis.Populated = false;
    Update("CreatePaymentStatusHistory5",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.ReisReis.PstGeneratedId = pstGeneratedId;
    entities.ReisReis.PrqGeneratedId = prqGeneratedId;
    entities.ReisReis.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.ReisReis.EffectiveDate = effectiveDate;
    entities.ReisReis.DiscontinueDate = discontinueDate;
    entities.ReisReis.CreatedBy = createdBy;
    entities.ReisReis.CreatedTimestamp = createdTimestamp;
    entities.ReisReis.ReasonText = reasonText;
    entities.ReisReis.Populated = true;
  }

  private bool ReadControlTable1()
  {
    entities.Check.Populated = false;

    return Read("ReadControlTable1",
      null,
      (db, reader) =>
      {
        entities.Check.Identifier = db.GetString(reader, 0);
        entities.Check.LastUsedNumber = db.GetInt32(reader, 1);
        entities.Check.Dummy1 = db.GetNullableString(reader, 2);
        entities.Check.Dummy2 = db.GetNullableString(reader, 3);
        entities.Check.Populated = true;
      });
  }

  private bool ReadControlTable2()
  {
    entities.DebitCard.Populated = false;

    return Read("ReadControlTable2",
      null,
      (db, reader) =>
      {
        entities.DebitCard.Identifier = db.GetString(reader, 0);
        entities.DebitCard.LastUsedNumber = db.GetInt32(reader, 1);
        entities.DebitCard.Dummy1 = db.GetNullableString(reader, 2);
        entities.DebitCard.Dummy2 = db.GetNullableString(reader, 3);
        entities.DebitCard.Populated = true;
      });
  }

  private bool ReadControlTable3()
  {
    entities.Eft.Populated = false;

    return Read("ReadControlTable3",
      null,
      (db, reader) =>
      {
        entities.Eft.Identifier = db.GetString(reader, 0);
        entities.Eft.LastUsedNumber = db.GetInt32(reader, 1);
        entities.Eft.Dummy1 = db.GetNullableString(reader, 2);
        entities.Eft.Dummy2 = db.GetNullableString(reader, 3);
        entities.Eft.Populated = true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    entities.Existing.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          local.FromFile.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.ProcessDate = db.GetDate(reader, 1);
        entities.Existing.Amount = db.GetDecimal(reader, 2);
        entities.Existing.CreatedBy = db.GetString(reader, 3);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Existing.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.Existing.CsePersonNumber = db.GetNullableString(reader, 6);
        entities.Existing.ImprestFundCode = db.GetNullableString(reader, 7);
        entities.Existing.Classification = db.GetString(reader, 8);
        entities.Existing.Number = db.GetNullableString(reader, 9);
        entities.Existing.PrintDate = db.GetNullableDate(reader, 10);
        entities.Existing.Type1 = db.GetString(reader, 11);
        entities.Existing.PrqRGeneratedId = db.GetNullableInt32(reader, 12);
        entities.Existing.RecoupmentIndKpc = db.GetNullableString(reader, 13);
        entities.Existing.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.Existing.Type1);
      });
  }

  private bool ReadPaymentRequest2()
  {
    entities.Existing.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          local.ReisReis.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.ProcessDate = db.GetDate(reader, 1);
        entities.Existing.Amount = db.GetDecimal(reader, 2);
        entities.Existing.CreatedBy = db.GetString(reader, 3);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Existing.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.Existing.CsePersonNumber = db.GetNullableString(reader, 6);
        entities.Existing.ImprestFundCode = db.GetNullableString(reader, 7);
        entities.Existing.Classification = db.GetString(reader, 8);
        entities.Existing.Number = db.GetNullableString(reader, 9);
        entities.Existing.PrintDate = db.GetNullableDate(reader, 10);
        entities.Existing.Type1 = db.GetString(reader, 11);
        entities.Existing.PrqRGeneratedId = db.GetNullableInt32(reader, 12);
        entities.Existing.RecoupmentIndKpc = db.GetNullableString(reader, 13);
        entities.Existing.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.Existing.Type1);
      });
  }

  private bool ReadPaymentRequest3()
  {
    entities.ReissuedPaymentRequest.Populated = false;

    return Read("ReadPaymentRequest3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqRGeneratedId", local.ReisReis.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ReissuedPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReissuedPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ReissuedPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ReissuedPaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.ReissuedPaymentRequest.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ReissuedPaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.ReissuedPaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.ReissuedPaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.ReissuedPaymentRequest.Classification =
          db.GetString(reader, 8);
        entities.ReissuedPaymentRequest.Number =
          db.GetNullableString(reader, 9);
        entities.ReissuedPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 10);
        entities.ReissuedPaymentRequest.Type1 = db.GetString(reader, 11);
        entities.ReissuedPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.ReissuedPaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 13);
        entities.ReissuedPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ReissuedPaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest4()
  {
    entities.ReissuedPaymentRequest.Populated = false;

    return Read("ReadPaymentRequest4",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          local.ReisReis.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReissuedPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReissuedPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ReissuedPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ReissuedPaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.ReissuedPaymentRequest.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ReissuedPaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.ReissuedPaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.ReissuedPaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.ReissuedPaymentRequest.Classification =
          db.GetString(reader, 8);
        entities.ReissuedPaymentRequest.Number =
          db.GetNullableString(reader, 9);
        entities.ReissuedPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 10);
        entities.ReissuedPaymentRequest.Type1 = db.GetString(reader, 11);
        entities.ReissuedPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.ReissuedPaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 13);
        entities.ReissuedPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ReissuedPaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatus1()
  {
    entities.Paid.Populated = false;

    return Read("ReadPaymentStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          local.PaidPaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Paid.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Paid.Code = db.GetString(reader, 1);
        entities.Paid.Populated = true;
      });
  }

  private bool ReadPaymentStatus2()
  {
    entities.ReissuedPaymentStatus.Populated = false;

    return Read("ReadPaymentStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          local.ReissuedPaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReissuedPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReissuedPaymentStatus.Code = db.GetString(reader, 1);
        entities.ReissuedPaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.Existing.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistoryPaymentStatus1()
  {
    entities.PaymentStatus.Populated = false;
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistoryPaymentStatus1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.Existing.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatus.Code = db.GetString(reader, 8);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistoryPaymentStatus2()
  {
    entities.PaymentStatus.Populated = false;
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistoryPaymentStatus2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.ReissuedPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatus.Code = db.GetString(reader, 8);
        entities.PaymentStatus.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private void UpdateControlTable1()
  {
    var lastUsedNumber = (int)StringToNumber(local.NewLastCheckNo.Number);

    entities.Check.Populated = false;
    Update("UpdateControlTable1",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(command, "cntlTblId", entities.Check.Identifier);
      });

    entities.Check.LastUsedNumber = lastUsedNumber;
    entities.Check.Populated = true;
  }

  private void UpdateControlTable2()
  {
    var lastUsedNumber =
      (int)StringToNumber(Substring(local.NewLastDcNo.Number, 9, 2, 8));

    entities.DebitCard.Populated = false;
    Update("UpdateControlTable2",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(command, "cntlTblId", entities.DebitCard.Identifier);
      });

    entities.DebitCard.LastUsedNumber = lastUsedNumber;
    entities.DebitCard.Populated = true;
  }

  private void UpdateControlTable3()
  {
    var lastUsedNumber =
      (int)StringToNumber(Substring(local.NewLastEftNo.Number, 9, 2, 8));

    entities.Eft.Populated = false;
    Update("UpdateControlTable3",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(command, "cntlTblId", entities.Eft.Identifier);
      });

    entities.Eft.LastUsedNumber = lastUsedNumber;
    entities.Eft.Populated = true;
  }

  private void UpdatePaymentRequest()
  {
    var number = local.FromFile.Number ?? "";
    var printDate = local.FromFile.PrintDate;
    var recoupmentIndKpc = local.FromFile.RecoupmentIndKpc ?? "";

    entities.Existing.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "number", number);
        db.SetNullableDate(command, "printDate", printDate);
        db.SetNullableString(command, "recoupmentIndKpc", recoupmentIndKpc);
        db.SetInt32(
          command, "paymentRequestId",
          entities.Existing.SystemGeneratedIdentifier);
      });

    entities.Existing.Number = number;
    entities.Existing.PrintDate = printDate;
    entities.Existing.RecoupmentIndKpc = recoupmentIndKpc;
    entities.Existing.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = local.CurrentDate.Date;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
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
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public PaymentRequest ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
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
    /// A value of NoOfTimesChkEftReissd.
    /// </summary>
    [JsonPropertyName("noOfTimesChkEftReissd")]
    public Common NoOfTimesChkEftReissd
    {
      get => noOfTimesChkEftReissd ??= new();
      set => noOfTimesChkEftReissd = value;
    }

    /// <summary>
    /// A value of ChkBelowLastUsedNumber.
    /// </summary>
    [JsonPropertyName("chkBelowLastUsedNumber")]
    public Common ChkBelowLastUsedNumber
    {
      get => chkBelowLastUsedNumber ??= new();
      set => chkBelowLastUsedNumber = value;
    }

    /// <summary>
    /// A value of EftBelowLastUsedNumber.
    /// </summary>
    [JsonPropertyName("eftBelowLastUsedNumber")]
    public Common EftBelowLastUsedNumber
    {
      get => eftBelowLastUsedNumber ??= new();
      set => eftBelowLastUsedNumber = value;
    }

    /// <summary>
    /// A value of DcBelowLastUsedNumber.
    /// </summary>
    [JsonPropertyName("dcBelowLastUsedNumber")]
    public Common DcBelowLastUsedNumber
    {
      get => dcBelowLastUsedNumber ??= new();
      set => dcBelowLastUsedNumber = value;
    }

    /// <summary>
    /// A value of NewLastCheckNo.
    /// </summary>
    [JsonPropertyName("newLastCheckNo")]
    public PaymentRequest NewLastCheckNo
    {
      get => newLastCheckNo ??= new();
      set => newLastCheckNo = value;
    }

    /// <summary>
    /// A value of NewLastEftNo.
    /// </summary>
    [JsonPropertyName("newLastEftNo")]
    public PaymentRequest NewLastEftNo
    {
      get => newLastEftNo ??= new();
      set => newLastEftNo = value;
    }

    /// <summary>
    /// A value of NewLastDcNo.
    /// </summary>
    [JsonPropertyName("newLastDcNo")]
    public PaymentRequest NewLastDcNo
    {
      get => newLastDcNo ??= new();
      set => newLastDcNo = value;
    }

    /// <summary>
    /// A value of OldLastCheckNo.
    /// </summary>
    [JsonPropertyName("oldLastCheckNo")]
    public PaymentRequest OldLastCheckNo
    {
      get => oldLastCheckNo ??= new();
      set => oldLastCheckNo = value;
    }

    /// <summary>
    /// A value of OldLastEftNo.
    /// </summary>
    [JsonPropertyName("oldLastEftNo")]
    public PaymentRequest OldLastEftNo
    {
      get => oldLastEftNo ??= new();
      set => oldLastEftNo = value;
    }

    /// <summary>
    /// A value of OldLastDcNo.
    /// </summary>
    [JsonPropertyName("oldLastDcNo")]
    public PaymentRequest OldLastDcNo
    {
      get => oldLastDcNo ??= new();
      set => oldLastDcNo = value;
    }

    /// <summary>
    /// A value of NoChksBypassMoreThan1.
    /// </summary>
    [JsonPropertyName("noChksBypassMoreThan1")]
    public Common NoChksBypassMoreThan1
    {
      get => noChksBypassMoreThan1 ??= new();
      set => noChksBypassMoreThan1 = value;
    }

    /// <summary>
    /// A value of NoEftsBypassMoreThan1.
    /// </summary>
    [JsonPropertyName("noEftsBypassMoreThan1")]
    public Common NoEftsBypassMoreThan1
    {
      get => noEftsBypassMoreThan1 ??= new();
      set => noEftsBypassMoreThan1 = value;
    }

    /// <summary>
    /// A value of NoDcsBypassMoreThan1.
    /// </summary>
    [JsonPropertyName("noDcsBypassMoreThan1")]
    public Common NoDcsBypassMoreThan1
    {
      get => noDcsBypassMoreThan1 ??= new();
      set => noDcsBypassMoreThan1 = value;
    }

    /// <summary>
    /// A value of NoOfTimesPmtReissued.
    /// </summary>
    [JsonPropertyName("noOfTimesPmtReissued")]
    public Common NoOfTimesPmtReissued
    {
      get => noOfTimesPmtReissued ??= new();
      set => noOfTimesPmtReissued = value;
    }

    /// <summary>
    /// A value of ReisReis.
    /// </summary>
    [JsonPropertyName("reisReis")]
    public PaymentRequest ReisReis
    {
      get => reisReis ??= new();
      set => reisReis = value;
    }

    /// <summary>
    /// A value of LastPmtRqstWithReisSt.
    /// </summary>
    [JsonPropertyName("lastPmtRqstWithReisSt")]
    public Common LastPmtRqstWithReisSt
    {
      get => lastPmtRqstWithReisSt ??= new();
      set => lastPmtRqstWithReisSt = value;
    }

    /// <summary>
    /// A value of FromFile.
    /// </summary>
    [JsonPropertyName("fromFile")]
    public PaymentRequest FromFile
    {
      get => fromFile ??= new();
      set => fromFile = value;
    }

    /// <summary>
    /// A value of NumberOfChecksPaid.
    /// </summary>
    [JsonPropertyName("numberOfChecksPaid")]
    public Common NumberOfChecksPaid
    {
      get => numberOfChecksPaid ??= new();
      set => numberOfChecksPaid = value;
    }

    /// <summary>
    /// A value of NumberOfEftsPaid.
    /// </summary>
    [JsonPropertyName("numberOfEftsPaid")]
    public Common NumberOfEftsPaid
    {
      get => numberOfEftsPaid ??= new();
      set => numberOfEftsPaid = value;
    }

    /// <summary>
    /// A value of NumberOfDcsPaid.
    /// </summary>
    [JsonPropertyName("numberOfDcsPaid")]
    public Common NumberOfDcsPaid
    {
      get => numberOfDcsPaid ??= new();
      set => numberOfDcsPaid = value;
    }

    /// <summary>
    /// A value of NumberOfChecksReissued.
    /// </summary>
    [JsonPropertyName("numberOfChecksReissued")]
    public Common NumberOfChecksReissued
    {
      get => numberOfChecksReissued ??= new();
      set => numberOfChecksReissued = value;
    }

    /// <summary>
    /// A value of NumberOfEftsReissued.
    /// </summary>
    [JsonPropertyName("numberOfEftsReissued")]
    public Common NumberOfEftsReissued
    {
      get => numberOfEftsReissued ??= new();
      set => numberOfEftsReissued = value;
    }

    /// <summary>
    /// A value of NumberOfDcsReissued.
    /// </summary>
    [JsonPropertyName("numberOfDcsReissued")]
    public Common NumberOfDcsReissued
    {
      get => numberOfDcsReissued ??= new();
      set => numberOfDcsReissued = value;
    }

    /// <summary>
    /// A value of NumberOfChecksBypassed.
    /// </summary>
    [JsonPropertyName("numberOfChecksBypassed")]
    public Common NumberOfChecksBypassed
    {
      get => numberOfChecksBypassed ??= new();
      set => numberOfChecksBypassed = value;
    }

    /// <summary>
    /// A value of NumberOfEftsBypassed.
    /// </summary>
    [JsonPropertyName("numberOfEftsBypassed")]
    public Common NumberOfEftsBypassed
    {
      get => numberOfEftsBypassed ??= new();
      set => numberOfEftsBypassed = value;
    }

    /// <summary>
    /// A value of NumberOfDcsBypassed.
    /// </summary>
    [JsonPropertyName("numberOfDcsBypassed")]
    public Common NumberOfDcsBypassed
    {
      get => numberOfDcsBypassed ??= new();
      set => numberOfDcsBypassed = value;
    }

    /// <summary>
    /// A value of NoChecksReisMoreThan1.
    /// </summary>
    [JsonPropertyName("noChecksReisMoreThan1")]
    public Common NoChecksReisMoreThan1
    {
      get => noChecksReisMoreThan1 ??= new();
      set => noChecksReisMoreThan1 = value;
    }

    /// <summary>
    /// A value of NoEftsReisMoreThan1.
    /// </summary>
    [JsonPropertyName("noEftsReisMoreThan1")]
    public Common NoEftsReisMoreThan1
    {
      get => noEftsReisMoreThan1 ??= new();
      set => noEftsReisMoreThan1 = value;
    }

    /// <summary>
    /// A value of NoDcsReisMoreThan1.
    /// </summary>
    [JsonPropertyName("noDcsReisMoreThan1")]
    public Common NoDcsReisMoreThan1
    {
      get => noDcsReisMoreThan1 ??= new();
      set => noDcsReisMoreThan1 = value;
    }

    /// <summary>
    /// A value of FileInError.
    /// </summary>
    [JsonPropertyName("fileInError")]
    public EabReportSend FileInError
    {
      get => fileInError ??= new();
      set => fileInError = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of PaidPaymentStatus.
    /// </summary>
    [JsonPropertyName("paidPaymentStatus")]
    public PaymentStatus PaidPaymentStatus
    {
      get => paidPaymentStatus ??= new();
      set => paidPaymentStatus = value;
    }

    /// <summary>
    /// A value of Doa.
    /// </summary>
    [JsonPropertyName("doa")]
    public PaymentStatus Doa
    {
      get => doa ??= new();
      set => doa = value;
    }

    /// <summary>
    /// A value of ReissuedPaymentStatus.
    /// </summary>
    [JsonPropertyName("reissuedPaymentStatus")]
    public PaymentStatus ReissuedPaymentStatus
    {
      get => reissuedPaymentStatus ??= new();
      set => reissuedPaymentStatus = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of TerminateLoopInd.
    /// </summary>
    [JsonPropertyName("terminateLoopInd")]
    public Common TerminateLoopInd
    {
      get => terminateLoopInd ??= new();
      set => terminateLoopInd = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of PaidPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paidPaymentStatusHistory")]
    public PaymentStatusHistory PaidPaymentStatusHistory
    {
      get => paidPaymentStatusHistory ??= new();
      set => paidPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of ReissuedPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("reissuedPaymentStatusHistory")]
    public PaymentStatusHistory ReissuedPaymentStatusHistory
    {
      get => reissuedPaymentStatusHistory ??= new();
      set => reissuedPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of CancelAndReissue.
    /// </summary>
    [JsonPropertyName("cancelAndReissue")]
    public PaymentStatusHistory CancelAndReissue
    {
      get => cancelAndReissue ??= new();
      set => cancelAndReissue = value;
    }

    private PaymentRequest forCreate;
    private PaymentRequest paymentRequest;
    private Common noOfTimesChkEftReissd;
    private Common chkBelowLastUsedNumber;
    private Common eftBelowLastUsedNumber;
    private Common dcBelowLastUsedNumber;
    private PaymentRequest newLastCheckNo;
    private PaymentRequest newLastEftNo;
    private PaymentRequest newLastDcNo;
    private PaymentRequest oldLastCheckNo;
    private PaymentRequest oldLastEftNo;
    private PaymentRequest oldLastDcNo;
    private Common noChksBypassMoreThan1;
    private Common noEftsBypassMoreThan1;
    private Common noDcsBypassMoreThan1;
    private Common noOfTimesPmtReissued;
    private PaymentRequest reisReis;
    private Common lastPmtRqstWithReisSt;
    private PaymentRequest fromFile;
    private Common numberOfChecksPaid;
    private Common numberOfEftsPaid;
    private Common numberOfDcsPaid;
    private Common numberOfChecksReissued;
    private Common numberOfEftsReissued;
    private Common numberOfDcsReissued;
    private Common numberOfChecksBypassed;
    private Common numberOfEftsBypassed;
    private Common numberOfDcsBypassed;
    private Common noChecksReisMoreThan1;
    private Common noEftsReisMoreThan1;
    private Common noDcsReisMoreThan1;
    private EabReportSend fileInError;
    private Common counter;
    private PaymentStatus paidPaymentStatus;
    private PaymentStatus doa;
    private PaymentStatus reissuedPaymentStatus;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private Common terminateLoopInd;
    private DateWorkArea maximum;
    private External hardcodeOpen;
    private External hardcodeRead;
    private External passArea;
    private DateWorkArea currentDate;
    private PaymentStatusHistory paidPaymentStatusHistory;
    private PaymentStatusHistory reissuedPaymentStatusHistory;
    private PaymentStatusHistory cancelAndReissue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DebitCard.
    /// </summary>
    [JsonPropertyName("debitCard")]
    public ControlTable DebitCard
    {
      get => debitCard ??= new();
      set => debitCard = value;
    }

    /// <summary>
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public ControlTable Eft
    {
      get => eft ??= new();
      set => eft = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public ControlTable Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of ReissuedReissued.
    /// </summary>
    [JsonPropertyName("reissuedReissued")]
    public PaymentRequest ReissuedReissued
    {
      get => reissuedReissued ??= new();
      set => reissuedReissued = value;
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
    /// A value of ReissuedPaymentStatus.
    /// </summary>
    [JsonPropertyName("reissuedPaymentStatus")]
    public PaymentStatus ReissuedPaymentStatus
    {
      get => reissuedPaymentStatus ??= new();
      set => reissuedPaymentStatus = value;
    }

    /// <summary>
    /// A value of Paid.
    /// </summary>
    [JsonPropertyName("paid")]
    public PaymentStatus Paid
    {
      get => paid ??= new();
      set => paid = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public PaymentRequest Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ReissuedPaymentRequest.
    /// </summary>
    [JsonPropertyName("reissuedPaymentRequest")]
    public PaymentRequest ReissuedPaymentRequest
    {
      get => reissuedPaymentRequest ??= new();
      set => reissuedPaymentRequest = value;
    }

    /// <summary>
    /// A value of ReisReis.
    /// </summary>
    [JsonPropertyName("reisReis")]
    public PaymentStatusHistory ReisReis
    {
      get => reisReis ??= new();
      set => reisReis = value;
    }

    private ControlTable debitCard;
    private ControlTable eft;
    private ControlTable check;
    private PaymentRequest reissuedReissued;
    private PaymentStatus paymentStatus;
    private PaymentStatus reissuedPaymentStatus;
    private PaymentStatus paid;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest existing;
    private PaymentRequest reissuedPaymentRequest;
    private PaymentStatusHistory reisReis;
  }
#endregion
}
