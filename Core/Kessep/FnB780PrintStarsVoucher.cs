// Program: FN_B780_PRINT_STARS_VOUCHER, ID: 372879563, model: 746.
// Short name: SWEF780B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B780_PRINT_STARS_VOUCHER.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB780PrintStarsVoucher: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B780_PRINT_STARS_VOUCHER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB780PrintStarsVoucher(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB780PrintStarsVoucher.
  /// </summary>
  public FnB780PrintStarsVoucher(IContext context, Import import, Export export):
    
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
    // *
    // 
    // *
    // * C H A N G E   L O G
    // 
    // *
    // * ===================
    // 
    // *
    // *
    // 
    // *
    // *******************************************************************
    // *
    // 
    // *
    // *   Date   Name      PR#  Reason
    // 
    // *
    // *   ----   ----      ---  ------
    // 
    // *
    // * 08/27/99                
    // Production
    // 
    // *
    // * 09/02/99 C Fairley 349  Trace number range on report incorrect  *
    // *                         need to retrieve voucher number from JCL*
    // * 05/03/00 C Fairley 94251 Do not report warrants under $1        *
    // * 08/03/00 E Lyman   99513 Expand voucher number for KPC          *
    // * 07/16/01 Fangman 123714  Change read statement for Payment      *
    // *			   Requests and for Disbursements to      *
    // *			   ensure that SWEFB780 processes the     *
    // *			   same records as SWEFB656.              *
    // * 06/05/12 GVandy CQ33868  Include Support Warrants < $1.00.      *
    // *
    // 
    // *
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    local.DetermineFy.NumericalMonth =
      Month(local.ProgramProcessingInfo.ProcessDate);
    local.DetermineFy.NumericalYear =
      Year(local.ProgramProcessingInfo.ProcessDate);

    if (local.DetermineFy.NumericalMonth > 6)
    {
      ++local.DetermineFy.NumericalYear;
    }

    local.DateMmddyyAlpha.AlphaYy =
      NumberToString(local.DetermineFy.NumericalYear, 14, 2);

    if (ReadSmartTransactionEntry1())
    {
      if (local.DetermineFy.NumericalMonth > 6)
      {
        if (!Equal(entities.SmartTransactionEntry.FinYr,
          local.DateMmddyyAlpha.AlphaYy))
        {
          foreach(var item in ReadSmartTransactionEntry3())
          {
            try
            {
              UpdateSmartTransactionEntry();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
    }

    // * Following code added by E Lyman on 8/3/2000
    local.JulianDate.Count =
      DateToJulianNumber(local.ProgramProcessingInfo.ProcessDate);
    local.StarsVoucherNumber.TextLine8 = "T" + "629" + NumberToString
      (local.JulianDate.Count, 13, 3) + "3";

    // * Preceding code added by E Lyman on 8/3/2000
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    local.PaymentRequest2.Type1 =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 3);

    if (IsEmpty(local.PaymentRequest2.Type1))
    {
      ExitState = "FN0000_SYSIN_PARM_FORMAT_ERR_A";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // ***
    // *** OPEN the STARS Voucher report
    // ***
    export.ReportParms.Parm1 = "OF";
    export.ReportParms.Parm2 = "";

    // need to add stars transaction entry table back into the following table
    UseEabStarsVoucher2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** ERROR opening report
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error Opening The STARS Voucher Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";
      }

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    switch(TrimEnd(local.PaymentRequest2.Type1))
    {
      case "WAR":
        local.NeededToWrite.RptDetail = "Processing Type : WARRANT";

        break;
      case "EFT":
        local.NeededToWrite.RptDetail = "Processing Type : EFT";

        break;
      default:
        ExitState = "FN0000_SYSIN_PARM_FORMAT_ERR_A";

        return;
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // need to add read each on starts transaction entry table targeting local 
    // group
    // read each stars transaction entry
    //     targeting local group from the beginning until full
    // move stars transaction entry to local grp stars transaction entry
    // end read each
    local.Group.Index = 0;
    local.Group.Clear();

    foreach(var item in ReadSmartTransactionEntry2())
    {
      local.Group.Update.SmartTransactionEntry.Assign(
        entities.SmartTransactionEntry);
      local.Group.Next();
    }

    // **********************************************************
    // MAIN PROCESS LOOP
    // **********************************************************
    local.FirstTimeThru.Flag = "Y";

    foreach(var item in ReadPaymentRequest2())
    {
      // -- 06/05/12 GVandy CQ33868  Include Support Warrants < $1.00.
      if (ReadPaymentRequest1())
      {
        continue;
      }
      else
      {
        // : No Reissue Found - Continue Processing.
      }

      // ***Skip payments that are in a requested status ***
      if (ReadPaymentStatus())
      {
        if (entities.PaymentStatus.SystemGeneratedIdentifier == 1)
        {
          local.NeededToWrite.RptDetail =
            "Warning:  Process_Date set and REQuested status found for payment request w/ id " +
            NumberToString
            (entities.PaymentRequest.SystemGeneratedIdentifier, 15);
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";
          }

          continue;
        }
      }
      else
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        local.NeededToWrite.RptDetail =
          "Error finding current status for payment request w/ id " + NumberToString
          (entities.PaymentRequest.SystemGeneratedIdentifier, 15);
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";
        }

        return;
      }

      ++local.PaymentRequest1.Count;
      local.WarrantOrEftTotal.TotalCurrency += entities.PaymentRequest.Amount;

      if (Equal(local.PaymentRequest2.Type1, "EFT"))
      {
        // *** processing EFT's
        // ***
        // *** get Electronic Fund Transmission for current Payment Request
        // ***
        if (ReadElectronicFundTransmission())
        {
          if (AsChar(local.FirstTimeThru.Flag) == 'Y')
          {
            local.First.TraceNumber =
              entities.ElectronicFundTransmission.TraceNumber;
            local.Last.TraceNumber =
              entities.ElectronicFundTransmission.TraceNumber;
            local.FirstTimeThru.Flag = "N";
          }
          else
          {
            // *** 09/02/99 PR #349
            // *** old code
            // *** 09/02/99 PR #349
            // *** start modification
            if (Lt(entities.ElectronicFundTransmission.TraceNumber,
              local.First.TraceNumber.GetValueOrDefault()))
            {
              local.First.TraceNumber =
                entities.ElectronicFundTransmission.TraceNumber;
            }
            else if (Lt(local.Last.TraceNumber.GetValueOrDefault(),
              entities.ElectronicFundTransmission.TraceNumber))
            {
              local.Last.TraceNumber =
                entities.ElectronicFundTransmission.TraceNumber;
            }

            // *** 09/02/99 PR #349
            // *** end modification
          }
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Electronic Fund Transmission not found for Payment Request with Identifier " +
            NumberToString
            (entities.PaymentRequest.SystemGeneratedIdentifier, 15);
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }
        }
      }

      if (Equal(entities.PaymentRequest.Classification, "REF") || Equal
        (entities.PaymentRequest.Classification, "ADV"))
      {
        local.StarsVoucherTotals.Refunds += entities.PaymentRequest.Amount;
      }
      else if (AsChar(entities.PaymentRequest.InterstateInd) == 'Y')
      {
        local.StarsVoucherTotals.Interstate += entities.PaymentRequest.Amount;
      }
      else
      {
        local.DisbursementTranFound.Flag = "N";

        foreach(var item1 in ReadDisbursementTransaction())
        {
          local.DisbursementTranFound.Flag = "Y";

          if (ReadDisbursementType())
          {
            if (Equal(entities.DisbursementType.Code, 1, 2, "NA"))
            {
              // *** NON-AFDC and Excess URA combined per Jennifer Elliot 08/24/
              // 99
              local.StarsVoucherTotals.NonAfdc += entities.
                DisbursementTransaction.Amount;
            }
            else if (Equal(entities.DisbursementType.Code, "PT"))
            {
              local.StarsVoucherTotals.Passthru += entities.
                DisbursementTransaction.Amount;
            }
            else if (CharAt(entities.DisbursementType.Code, 1) == 'X')
            {
              // *** NON-AFDC and Excess URA combined per Jennifer Elliot 08/24/
              // 99
              local.StarsVoucherTotals.NonAfdc += entities.
                DisbursementTransaction.Amount;
            }
            else
            {
              local.StarsVoucherTotals.Error += entities.
                DisbursementTransaction.Amount;
              local.NeededToWrite.RptDetail =
                "Error Processing Payment Request # : " + NumberToString
                (entities.PaymentRequest.SystemGeneratedIdentifier, 15) + " - Disbursement Type not within the processing criteria - $ " +
                NumberToString
                ((long)(entities.DisbursementTransaction.Amount * 100), 15);
              UseCabErrorReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

                return;
              }
            }
          }
          else
          {
            local.StarsVoucherTotals.Error += entities.DisbursementTransaction.
              Amount;
            local.NeededToWrite.RptDetail =
              "Error Processing Payment Request # : " + NumberToString
              (entities.PaymentRequest.SystemGeneratedIdentifier, 15) + " - Disbursement Type not found - $ " +
              NumberToString((long)(entities.DisbursementTransaction.Amount * 100
              ), 15);
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

              return;
            }
          }
        }

        if (AsChar(local.DisbursementTranFound.Flag) == 'N')
        {
          local.StarsVoucherTotals.Error += entities.PaymentRequest.Amount;
          local.NeededToWrite.RptDetail =
            "Error Processing Payment Request # : " + NumberToString
            (entities.PaymentRequest.SystemGeneratedIdentifier, 15) + " - Disbursement Transaction not found - $ " +
            NumberToString((long)(entities.PaymentRequest.Amount * 100), 15);
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

            return;
          }
        }
      }
    }

    local.StarsVoucherTotals.VoucherTotal =
      local.StarsVoucherTotals.ExcessUra + local
      .StarsVoucherTotals.Interstate + local.StarsVoucherTotals.NonAfdc + local
      .StarsVoucherTotals.Passthru + local.StarsVoucherTotals.Refunds + local
      .StarsVoucherTotals.Error;

    // ***
    // *** initialize reporting FLAG's and COUNT's
    // ***
    export.BreakerWar.Flag = "N";
    export.BreakerEft.Flag = "N";
    local.Line.Count = 0;

    // ***
    // *** Report processing LOOP
    // ***
    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      export.SmartTransactionEntry.
        Assign(local.Group.Item.SmartTransactionEntry);

      // move local grp stars transaction entry to export stars transaction 
      // entry
      // case of localgrp stars transaction entry stars class type
      switch(TrimEnd(local.Group.Item.SmartTransactionEntry.SmartClassType))
      {
        case "1":
          // *** PASSTHRU first line
          if (local.StarsVoucherTotals.Passthru == 0)
          {
            continue;
          }

          ++local.Line.Count;

          // set export stars transaction entry suffix 1 to textnum(local line 
          // ief supplied count)
          export.SmartTransactionEntry.Suffix1 =
            NumberToString(local.Line.Count, 2);
          export.Amt.VoucherTotal = local.StarsVoucherTotals.Passthru;

          break;
        case "2":
          // *** PASSTHRU second line
          if (local.StarsVoucherTotals.Passthru == 0)
          {
            continue;
          }

          ++local.Line.Count;

          // set export stars transaction entry suffix 1 to textnum(local line 
          // ief supplied count)
          export.SmartTransactionEntry.Suffix1 =
            NumberToString(local.Line.Count, 2);
          export.Amt.VoucherTotal = local.StarsVoucherTotals.Passthru;

          break;
        case "3":
          // *** PASSTHRU third line
          if (local.StarsVoucherTotals.Passthru == 0)
          {
            continue;
          }

          ++local.Line.Count;

          // set export stars transaction entry suffix 1 to textnum(local line 
          // ief supplied count)
          export.SmartTransactionEntry.Suffix1 =
            NumberToString(local.Line.Count, 2);
          export.Amt.VoucherTotal = local.StarsVoucherTotals.Passthru;

          break;
        case "4":
          // *** CSE Refunds line
          if (local.StarsVoucherTotals.Refunds == 0)
          {
            continue;
          }

          ++local.Line.Count;

          // set export stars transaction entry suffix 1 to textnum(local line 
          // ief supplied count)
          export.SmartTransactionEntry.Suffix1 =
            NumberToString(local.Line.Count, 2);
          export.Amt.VoucherTotal = local.StarsVoucherTotals.Refunds;

          break;
        case "5":
          // *** NON-AFDC and Excess URA line
          if (local.StarsVoucherTotals.NonAfdc == 0)
          {
            continue;
          }

          // *** NON-AFDC and Excess URA combined per Jennifer Elliot 08/24/99
          ++local.Line.Count;

          // set export stars transaction entry suffix 1 to textnum(local line 
          // ief supplied count)
          export.SmartTransactionEntry.Suffix1 =
            NumberToString(local.Line.Count, 2);
          export.Amt.VoucherTotal = local.StarsVoucherTotals.NonAfdc;

          break;
        case "6":
          // *** Interstate line
          if (local.StarsVoucherTotals.Interstate == 0)
          {
            continue;
          }

          ++local.Line.Count;

          // set export stars transaction entry suffix 1 to textnum(local line 
          // ief supplied count)
          export.SmartTransactionEntry.Suffix1 =
            NumberToString(local.Line.Count, 2);
          export.Amt.VoucherTotal = local.StarsVoucherTotals.Interstate;

          break;
        default:
          break;
      }

      // ***
      // *** WRITE to the STARS Voucher report
      // ***
      export.ReportParms.Parm1 = "GR";
      export.ReportParms.Parm2 = "";

      switch(TrimEnd(local.PaymentRequest2.Type1))
      {
        case "WAR":
          // *** set sub-report code for Warrant's
          export.ReportParms.SubreportCode = "MAIN";

          break;
        case "EFT":
          // *** set sub-report code for EFT's
          export.ReportParms.SubreportCode = "EFT";
          export.First.TraceNumber =
            local.First.TraceNumber.GetValueOrDefault();
          export.Last.TraceNumber = local.Last.TraceNumber.GetValueOrDefault();

          break;
        default:
          break;
      }

      UseEabStarsVoucher1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        // *** ERROR writing to report
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error Writing To The STARS Voucher Report";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";
        }

        return;
      }
    }

    // **********************************************************
    // PRINT CONTROL TOTALS
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Excess Child Support  . . . . . . . : " + NumberToString
      ((long)(local.StarsVoucherTotals.ExcessUra * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Interstate. . . . . . . . . . . . . : " + NumberToString
      ((long)(local.StarsVoucherTotals.Interstate * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "NA Child Support. . . . . . . . . . : " + NumberToString
      ((long)(local.StarsVoucherTotals.NonAfdc * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Passthru. . . . . . . . . . . . . . : " + NumberToString
      ((long)(local.StarsVoucherTotals.Passthru * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Refunds . . . . . . . . . . . . . . : " + NumberToString
      ((long)(local.StarsVoucherTotals.Refunds * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Error . . . . . . . . . . . . . . . : " + NumberToString
      ((long)(local.StarsVoucherTotals.Error * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Voucher Total . . . . . . . . . . . : " + NumberToString
      ((long)(local.StarsVoucherTotals.VoucherTotal * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Total Payment Requests Processed. . : " + NumberToString
      (local.PaymentRequest1.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "Total Amount Processed. . . . . . . : " + NumberToString
      ((long)(local.WarrantOrEftTotal.TotalCurrency * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // ***
    // *** CLOSE the STARS Voucher report
    // ***
    export.ReportParms.Parm1 = "CF";
    export.ReportParms.Parm2 = "";
    UseEabStarsVoucher2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      // *** ERROR closing report
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error Closing The STARS Voucher Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";
      }

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
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

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabStarsVoucher1()
  {
    var useImport = new EabStarsVoucher.Import();
    var useExport = new EabStarsVoucher.Export();

    useImport.StarsVoucherNumber.TextLine8 = local.StarsVoucherNumber.TextLine8;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.BreakerEft.Flag = export.BreakerEft.Flag;
    useImport.BreakerWar.Flag = export.BreakerWar.Flag;
    useImport.First.TraceNumber = export.First.TraceNumber;
    useImport.Last.TraceNumber = export.Last.TraceNumber;
    useImport.Amt.VoucherTotal = export.Amt.VoucherTotal;
    useImport.ReportParms.Assign(export.ReportParms);
    useImport.SmartTransactionEntry.Assign(export.SmartTransactionEntry);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabStarsVoucher.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabStarsVoucher2()
  {
    var useImport = new EabStarsVoucher.Import();
    var useExport = new EabStarsVoucher.Export();

    useImport.ReportParms.Assign(export.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabStarsVoucher.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;
    useExport.ProgramProcessingInfo.ParameterList =
      local.ProgramProcessingInfo.ParameterList;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
    local.ProgramProcessingInfo.ParameterList =
      useExport.ProgramProcessingInfo.ParameterList;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.PaymentRequest.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 3);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);

        return true;
      });
  }

  private bool ReadDisbursementType()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          entities.DisbursementTransaction.DbtGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 0);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 1);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.ElectronicFundTransmission.TraceNumber =
          db.GetNullableInt64(reader, 3);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentRequest.Populated);
    entities.CheckForReissue.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.PrqRGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CheckForReissue.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CheckForReissue.PrqRGeneratedId =
          db.GetNullableInt32(reader, 1);
        entities.CheckForReissue.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest2()
  {
    entities.PaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetString(command, "type", local.PaymentRequest2.Type1);
        db.SetDate(
          command, "processDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.Classification = db.GetString(reader, 6);
        entities.PaymentRequest.Type1 = db.GetString(reader, 7);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.PaymentRequest.InterstateInd = db.GetNullableString(reader, 9);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadSmartTransactionEntry1()
  {
    entities.SmartTransactionEntry.Populated = false;

    return Read("ReadSmartTransactionEntry1",
      null,
      (db, reader) =>
      {
        entities.SmartTransactionEntry.SmartClassType = db.GetString(reader, 0);
        entities.SmartTransactionEntry.FinYr = db.GetNullableString(reader, 1);
        entities.SmartTransactionEntry.Suffix1 =
          db.GetNullableString(reader, 2);
        entities.SmartTransactionEntry.BusinessUnit =
          db.GetNullableString(reader, 3);
        entities.SmartTransactionEntry.FundCode =
          db.GetNullableString(reader, 4);
        entities.SmartTransactionEntry.ProgramCode =
          db.GetNullableString(reader, 5);
        entities.SmartTransactionEntry.DeptId = db.GetNullableString(reader, 6);
        entities.SmartTransactionEntry.AccountNumber =
          db.GetNullableString(reader, 7);
        entities.SmartTransactionEntry.BudgetUnit =
          db.GetNullableString(reader, 8);
        entities.SmartTransactionEntry.SmartR = db.GetNullableString(reader, 9);
        entities.SmartTransactionEntry.Populated = true;
      });
  }

  private IEnumerable<bool> ReadSmartTransactionEntry2()
  {
    return ReadEach("ReadSmartTransactionEntry2",
      null,
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.SmartTransactionEntry.SmartClassType = db.GetString(reader, 0);
        entities.SmartTransactionEntry.FinYr = db.GetNullableString(reader, 1);
        entities.SmartTransactionEntry.Suffix1 =
          db.GetNullableString(reader, 2);
        entities.SmartTransactionEntry.BusinessUnit =
          db.GetNullableString(reader, 3);
        entities.SmartTransactionEntry.FundCode =
          db.GetNullableString(reader, 4);
        entities.SmartTransactionEntry.ProgramCode =
          db.GetNullableString(reader, 5);
        entities.SmartTransactionEntry.DeptId = db.GetNullableString(reader, 6);
        entities.SmartTransactionEntry.AccountNumber =
          db.GetNullableString(reader, 7);
        entities.SmartTransactionEntry.BudgetUnit =
          db.GetNullableString(reader, 8);
        entities.SmartTransactionEntry.SmartR = db.GetNullableString(reader, 9);
        entities.SmartTransactionEntry.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadSmartTransactionEntry3()
  {
    entities.Update.Populated = false;

    return ReadEach("ReadSmartTransactionEntry3",
      null,
      (db, reader) =>
      {
        entities.Update.SmartClassType = db.GetString(reader, 0);
        entities.Update.FinYr = db.GetNullableString(reader, 1);
        entities.Update.Suffix1 = db.GetNullableString(reader, 2);
        entities.Update.BusinessUnit = db.GetNullableString(reader, 3);
        entities.Update.FundCode = db.GetNullableString(reader, 4);
        entities.Update.ProgramCode = db.GetNullableString(reader, 5);
        entities.Update.DeptId = db.GetNullableString(reader, 6);
        entities.Update.AccountNumber = db.GetNullableString(reader, 7);
        entities.Update.BudgetUnit = db.GetNullableString(reader, 8);
        entities.Update.SmartR = db.GetNullableString(reader, 9);
        entities.Update.Populated = true;

        return true;
      });
  }

  private void UpdateSmartTransactionEntry()
  {
    var finYr = local.DateMmddyyAlpha.AlphaYy;

    entities.Update.Populated = false;
    Update("UpdateSmartTransactionEntry",
      (db, command) =>
      {
        db.SetNullableString(command, "finYr", finYr);
        db.SetString(command, "smartClassType", entities.Update.SmartClassType);
      });

    entities.Update.FinYr = finYr;
    entities.Update.Populated = true;
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
    /// <summary>
    /// A value of StarsVoucherSysin.
    /// </summary>
    [JsonPropertyName("starsVoucherSysin")]
    public StarsVoucherSysin StarsVoucherSysin
    {
      get => starsVoucherSysin ??= new();
      set => starsVoucherSysin = value;
    }

    /// <summary>
    /// A value of BreakerEft.
    /// </summary>
    [JsonPropertyName("breakerEft")]
    public Common BreakerEft
    {
      get => breakerEft ??= new();
      set => breakerEft = value;
    }

    /// <summary>
    /// A value of BreakerWar.
    /// </summary>
    [JsonPropertyName("breakerWar")]
    public Common BreakerWar
    {
      get => breakerWar ??= new();
      set => breakerWar = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public ElectronicFundTransmission First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public ElectronicFundTransmission Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of Amt.
    /// </summary>
    [JsonPropertyName("amt")]
    public StarsVoucherTotals Amt
    {
      get => amt ??= new();
      set => amt = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of SmartTransactionEntry.
    /// </summary>
    [JsonPropertyName("smartTransactionEntry")]
    public SmartTransactionEntry SmartTransactionEntry
    {
      get => smartTransactionEntry ??= new();
      set => smartTransactionEntry = value;
    }

    private StarsVoucherSysin starsVoucherSysin;
    private Common breakerEft;
    private Common breakerWar;
    private ElectronicFundTransmission first;
    private ElectronicFundTransmission last;
    private StarsVoucherTotals amt;
    private ReportParms reportParms;
    private SmartTransactionEntry smartTransactionEntry;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of SmartTransactionEntry.
      /// </summary>
      [JsonPropertyName("smartTransactionEntry")]
      public SmartTransactionEntry SmartTransactionEntry
      {
        get => smartTransactionEntry ??= new();
        set => smartTransactionEntry = value;
      }

      /// <summary>
      /// A value of TempStarsTransaction.
      /// </summary>
      [JsonPropertyName("tempStarsTransaction")]
      public TextWorkArea TempStarsTransaction
      {
        get => tempStarsTransaction ??= new();
        set => tempStarsTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private SmartTransactionEntry smartTransactionEntry;
      private TextWorkArea tempStarsTransaction;
    }

    /// <summary>
    /// A value of DateMmddyyAlpha.
    /// </summary>
    [JsonPropertyName("dateMmddyyAlpha")]
    public DateMmddyyAlpha DateMmddyyAlpha
    {
      get => dateMmddyyAlpha ??= new();
      set => dateMmddyyAlpha = value;
    }

    /// <summary>
    /// A value of DetermineFy.
    /// </summary>
    [JsonPropertyName("determineFy")]
    public DateWorkAttributes DetermineFy
    {
      get => determineFy ??= new();
      set => determineFy = value;
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
    /// A value of JulianDate.
    /// </summary>
    [JsonPropertyName("julianDate")]
    public Common JulianDate
    {
      get => julianDate ??= new();
      set => julianDate = value;
    }

    /// <summary>
    /// A value of StarsVoucherNumber.
    /// </summary>
    [JsonPropertyName("starsVoucherNumber")]
    public External StarsVoucherNumber
    {
      get => starsVoucherNumber ??= new();
      set => starsVoucherNumber = value;
    }

    /// <summary>
    /// A value of Line.
    /// </summary>
    [JsonPropertyName("line")]
    public Common Line
    {
      get => line ??= new();
      set => line = value;
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
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public ElectronicFundTransmission First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public ElectronicFundTransmission Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of DisbursementTranFound.
    /// </summary>
    [JsonPropertyName("disbursementTranFound")]
    public Common DisbursementTranFound
    {
      get => disbursementTranFound ??= new();
      set => disbursementTranFound = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of WarrantOrEftTotal.
    /// </summary>
    [JsonPropertyName("warrantOrEftTotal")]
    public Common WarrantOrEftTotal
    {
      get => warrantOrEftTotal ??= new();
      set => warrantOrEftTotal = value;
    }

    /// <summary>
    /// A value of PaymentRequest1.
    /// </summary>
    [JsonPropertyName("paymentRequest1")]
    public Common PaymentRequest1
    {
      get => paymentRequest1 ??= new();
      set => paymentRequest1 = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of PaymentRequest2.
    /// </summary>
    [JsonPropertyName("paymentRequest2")]
    public PaymentRequest PaymentRequest2
    {
      get => paymentRequest2 ??= new();
      set => paymentRequest2 = value;
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
    /// A value of StarsVoucherTotals.
    /// </summary>
    [JsonPropertyName("starsVoucherTotals")]
    public StarsVoucherTotals StarsVoucherTotals
    {
      get => starsVoucherTotals ??= new();
      set => starsVoucherTotals = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    private DateMmddyyAlpha dateMmddyyAlpha;
    private DateWorkAttributes determineFy;
    private DateWorkArea max;
    private Common julianDate;
    private External starsVoucherNumber;
    private Common line;
    private Common firstTimeThru;
    private ElectronicFundTransmission first;
    private ElectronicFundTransmission last;
    private ReportParms reportParms;
    private Common disbursementTranFound;
    private Array<GroupGroup> group;
    private Common warrantOrEftTotal;
    private Common paymentRequest1;
    private External external;
    private DateWorkArea null1;
    private PaymentRequest paymentRequest2;
    private ProgramProcessingInfo programProcessingInfo;
    private StarsVoucherTotals starsVoucherTotals;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public SmartTransactionEntry Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of SmartTransactionEntry.
    /// </summary>
    [JsonPropertyName("smartTransactionEntry")]
    public SmartTransactionEntry SmartTransactionEntry
    {
      get => smartTransactionEntry ??= new();
      set => smartTransactionEntry = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of CheckForReissue.
    /// </summary>
    [JsonPropertyName("checkForReissue")]
    public PaymentRequest CheckForReissue
    {
      get => checkForReissue ??= new();
      set => checkForReissue = value;
    }

    private SmartTransactionEntry update;
    private SmartTransactionEntry smartTransactionEntry;
    private CsePerson obligee1;
    private CsePersonAccount obligee2;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private ElectronicFundTransmission electronicFundTransmission;
    private PaymentRequest paymentRequest;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementType disbursementType;
    private PaymentRequest checkForReissue;
  }
#endregion
}
