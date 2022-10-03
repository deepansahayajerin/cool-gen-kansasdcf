// Program: FN_B670_PRCSS_NTFY_FED_OF_REFND, ID: 372537174, model: 746.
// Short name: SWEF670B
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
/// A program: FN_B670_PRCSS_NTFY_FED_OF_REFND.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB670PrcssNtfyFedOfRefnd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B670_PRCSS_NTFY_FED_OF_REFND program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB670PrcssNtfyFedOfRefnd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB670PrcssNtfyFedOfRefnd.
  /// </summary>
  public FnB670PrcssNtfyFedOfRefnd(IContext context, Import import,
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
    // ********************************************************************************
    // **                     M A I N T E N A N C E    L O G
    // ********************************************************************************
    // **  Date	PR/WR #		UserID		Description
    // ********************************************************************************
    // ** 07/05/2001	PR#122720	E.Shirk		Discontinued the process of 
    // concatenating five leading zeroes on the person number that his sent to
    // the Feds.
    // ********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodeOpen.FileInstruction = "OPEN";
    local.HardcodeClose.FileInstruction = "CLOSE";
    local.HardcodeWrite.FileInstruction = "WRITE";
    local.ErrorFound.Flag = "N";
    local.ErrorFileOpen.Flag = "N";
    local.ProgramControlTotal.SystemGeneratedIdentifier = 0;

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
      // ***** Call external to open the output file.
      local.PassArea.FileInstruction = local.HardcodeOpen.FileInstruction;
      UseEabWriteFedRefundRecord3();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        ExitState = "FILE_OPEN_ERROR";

        // * * * * * * * * * *
        // SAVE the Exit_State message
        // * * * * * * * * * *
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
        local.ErrorFound.Flag = "Y";
      }
    }
    else
    {
      // * * * * * * * * * *
      // SAVE the Exit_State message
      // * * * * * * * * * *
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ErrorFound.Flag = "Y";
    }

    // ******************************************************************
    // If NO Error found -- Process the Records
    // ******************************************************************
    if (AsChar(local.ErrorFound.Flag) != 'Y')
    {
      // ***** Process the selected records in groups based upon the commit
      // frequencies.  Do a DB2 commit at the end of each group.
      local.ZeroDate.DateTransmitted = null;
      local.TotalNumRecordsWritten.Count = 0;

      // **************************************************
      // According to IDCR # 288, the read statement was
      // changed to pick up all records without a
      // transmitted date. For a failed job run, the check
      // for CURRENT DATE - 3 would pick up all records
      // updated during the failed run
      // **************************************************
      // Changed Current_Date to PPI_Processing Date in ALL occurances.
      // **************************************************
      foreach(var item in ReadCashReceiptCashReceiptDetailReceiptRefund())
      {
        // 05/05/1999 Per Tim Hood and Sunya Sharp
        // 
        // - if the REFUND does NOT have a PAYMENT Request
        // - BYPASS IT!!!!
        local.PaymentRequestFound.Flag = "N";

        if (ReadPaymentRequest())
        {
          local.PaymentRequestFound.Flag = "Y";
        }

        if (AsChar(local.PaymentRequestFound.Flag) == 'N')
        {
          continue;
        }

        if (Lt(local.ZeroDate.DateTransmitted,
          entities.ExistingReceiptRefund.DateTransmitted) && Lt
          (entities.ExistingReceiptRefund.DateTransmitted,
          AddDays(local.ProgramProcessingInfo.ProcessDate, -3)))
        {
          continue;
        }

        if (ReadCashReceiptDetailAddress1())
        {
          local.FdsoNtfyFedRefTapeRec1999.AddressLine1 =
            entities.ExistingCashReceiptDetailAddress.Street1;
          local.FdsoNtfyFedRefTapeRec1999.AddressLine2 =
            entities.ExistingCashReceiptDetailAddress.Street2 ?? Spaces(30);
          local.FdsoNtfyFedRefTapeRec1999.City =
            entities.ExistingCashReceiptDetailAddress.City;
          local.FdsoNtfyFedRefTapeRec1999.StateCode =
            entities.ExistingCashReceiptDetailAddress.State;
          local.FdsoNtfyFedRefTapeRec1999.ZipCode9 =
            entities.ExistingCashReceiptDetailAddress.ZipCode5 + entities
            .ExistingCashReceiptDetailAddress.ZipCode4;
        }
        else if (ReadCashReceiptDetailAddress2())
        {
          local.FdsoNtfyFedRefTapeRec1999.AddressLine1 =
            entities.ExistingCashReceiptDetailAddress.Street1;
          local.FdsoNtfyFedRefTapeRec1999.AddressLine2 =
            entities.ExistingCashReceiptDetailAddress.Street2 ?? Spaces(30);
          local.FdsoNtfyFedRefTapeRec1999.City =
            entities.ExistingCashReceiptDetailAddress.City;
          local.FdsoNtfyFedRefTapeRec1999.StateCode =
            entities.ExistingCashReceiptDetailAddress.State;
          local.FdsoNtfyFedRefTapeRec1999.ZipCode9 =
            entities.ExistingCashReceiptDetailAddress.ZipCode5 + entities
            .ExistingCashReceiptDetailAddress.ZipCode4;
        }
        else
        {
          if (AsChar(local.ErrorFileOpen.Flag) != 'Y')
          {
            // * * * * * * * * * *
            // OPEN the ERROR REPORT
            // * * * * * * * * * *
            local.ReportHandling.ProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
            local.ReportHandling.ProgramName = "SWEFB670";
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

            local.ErrorFileOpen.Flag = "Y";
          }

          // * * * * * * * * * *
          // WRITE the ERROR REPORT
          // * * * * * * * * * *
          local.ReportHandling.RptDetail =
            "* * NO Address Record exist for Case Number " + entities
            .ExistingCashReceiptDetail.CaseNumber + " SSN " + entities
            .ExistingCashReceiptDetail.ObligorSocialSecurityNumber;
          local.ReportHandling.ProgramName = "SWEFB670";
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
          local.ReportHandling.ProgramName = "SWEFB670";
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

          continue;
        }

        local.FdsoNtfyFedRefTapeRec1999.SubmittingState = "KS";
        local.FdsoNtfyFedRefTapeRec1999.LocalCode = "177";
        local.FdsoNtfyFedRefTapeRec1999.Ssn =
          entities.ExistingCashReceiptDetail.OffsetTaxid.GetValueOrDefault();

        // Set Up CASE Number Field
        // This field actually contains the person number.
        // This is the way the entire KESSEP system is currently
        // populating this field.
        // It comes back from the Feds with whatever we send
        // - the Feds don't use this field.
        // Cash_Receipt_Detail will normally have blanks
        // for the Case Number.
        if (!IsEmpty(entities.ExistingCashReceiptDetail.ObligorPersonNumber))
        {
          local.FdsoNtfyFedRefTapeRec1999.CaseNumber =
            entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? Spaces
            (15);
        }

        local.FdsoNtfyFedRefTapeRec1999.FirstName =
          entities.ExistingCashReceiptDetail.ObligorFirstName ?? Spaces(15);
        local.FdsoNtfyFedRefTapeRec1999.LastName =
          entities.ExistingCashReceiptDetail.ObligorLastName ?? Spaces(20);
        local.FdsoNtfyFedRefTapeRec1999.RefundAmount =
          (int)(entities.ExistingReceiptRefund.Amount + 0.5M);
        local.FdsoNtfyFedRefTapeRec1999.TransactionType = "S";

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        // 02/17/1999   SWSRPDP   case_type is NOW being stored in the 
        // Joint_return_name which is NOT being used or sent back from FDSO.
        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        local.FdsoNtfyFedRefTapeRec1999.CaseType =
          Substring(entities.ExistingCashReceiptDetail.JointReturnName, 13, 1);

        if (CharAt(entities.ExistingCashReceiptDetail.JointReturnName, 13) == 'A'
          )
        {
          ++local.NumTanfRecordsRead.Count;
          local.NumTanfRecordsRead.TotalInteger =
            (long)(local.NumTanfRecordsRead.TotalInteger + entities
            .ExistingReceiptRefund.Amount + 0.5M);
          local.NumTanfRecordsRead.TotalCurrency += entities.
            ExistingReceiptRefund.Amount;
        }
        else
        {
          ++local.NumNontanfRcordsRead.Count;
          local.NumNontanfRcordsRead.TotalInteger =
            (long)(local.NumNontanfRcordsRead.TotalInteger + entities
            .ExistingReceiptRefund.Amount + 0.5M);
          local.NumNontanfRcordsRead.TotalCurrency += entities.
            ExistingReceiptRefund.Amount;
        }

        // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
        local.FdsoNtfyFedRefTapeRec1999.TransferState = "";
        local.FdsoNtfyFedRefTapeRec1999.TransferLocalCode = "000";
        local.FdsoNtfyFedRefTapeRec1999.ProcessYear =
          NumberToString(entities.ExistingCashReceiptDetail.OffsetTaxYear.
            GetValueOrDefault(), 4);
        local.PassArea.FileInstruction = local.HardcodeWrite.FileInstruction;
        UseEabWriteFedRefundRecord1();

        if (!IsEmpty(local.PassArea.TextReturnCode))
        {
          // ******************************************************************
          // 01/02/99  SWSRPDP Changes to meet DIR Batch Report Standards.
          // * * * * * * * * * *
          // ERROR Condition -- Write Report and ABEND
          // * * * * * * * * * *
          ExitState = "FILE_WRITE_ERROR_RB";

          // * * * * * * * * * *
          // SAVE the Exit_State message
          // * * * * * * * * * *
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
          local.ErrorFound.Flag = "Y";

          goto Test;
        }

        // Advancement Refunds are created in SWEFB612
        // Collection Refunds are created elsewhere
        if (Equal(entities.ExistingReceiptRefund.ReasonCode, "ADVANCE"))
        {
          ++local.TotalAdvancements.Count;
          local.TotalAdvancements.TotalInteger =
            (long)(local.TotalAdvancements.TotalInteger + entities
            .ExistingReceiptRefund.Amount + 0.5M);
          local.TotalAdvancements.TotalCurrency += entities.
            ExistingReceiptRefund.Amount;
        }
        else
        {
          ++local.TotalCollections.Count;
          local.TotalCollections.TotalInteger =
            (long)(local.TotalCollections.TotalInteger + entities
            .ExistingReceiptRefund.Amount + 0.5M);
          local.TotalCollections.TotalCurrency += entities.
            ExistingReceiptRefund.Amount;
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
              ExitState = "FN0000_RCPT_REFUND_NU";
              local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
              local.ErrorFound.Flag = "Y";

              goto Test;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_RCPT_REFUND_PV";
              local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
              local.ErrorFound.Flag = "Y";

              goto Test;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.TotalNumRecordsWritten.Count;
        local.TotalNumRecordsWritten.TotalInteger =
          (long)(local.TotalNumRecordsWritten.TotalInteger + entities
          .ExistingReceiptRefund.Amount + 0.5M);
        local.TotalNumRecordsWritten.TotalCurrency += entities.
          ExistingReceiptRefund.Amount;
      }
    }

Test:

    // ---------------------------------------------
    // After all processing has completed
    // Print the control total Report which
    // will reflect the total creates.
    // ---------------------------------------------
    // * * * * * * * * * *
    // 01/02/99  SWSRPDP Changes to meet DIR Batch Report Standards.
    // * * * * * * * * * *
    // OPEN the CONTROL REPORT
    // * * * * * * * * * *
    local.ReportHandling.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.ReportHandling.ProgramName = "SWEFB670";
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
      "Total Number of Records Processed................." + NumberToString
      (local.TotalNumRecordsWritten.Count, 15);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Count of TANF Records Processed
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Number of TANF Records Processed............" + NumberToString
      (local.NumTanfRecordsRead.Count, 15);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Count of NON-TANF Records Processed
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Number of NON-TANF Records Processed........" + NumberToString
      (local.NumNontanfRcordsRead.Count, 15);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Count of Advancements
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Number of Advancement Refunds Processed....." + NumberToString
      (local.TotalAdvancements.Count, 15);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Count of Collection
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Number of Collection Refunds Processed......" + NumberToString
      (local.TotalCollections.Count, 15);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL AMOUNT REPORT -- Total Amount of Records Processed
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Amount of Records Processed................." + NumberToString
      (local.TotalNumRecordsWritten.TotalInteger, 15);
    local.ReportHandling.RptDetail = TrimEnd(local.ReportHandling.RptDetail) + "."
      + NumberToString(local.TotalNumRecordsWritten.TotalInteger * 100, 14, 2);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Amount of TANF Records Processed
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Amount of TANF Records Processed............" + NumberToString
      (local.NumTanfRecordsRead.TotalInteger, 15);
    local.ReportHandling.RptDetail = TrimEnd(local.ReportHandling.RptDetail) + "."
      + NumberToString(local.NumTanfRecordsRead.TotalInteger * 100, 14, 2);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Amount of NON-TANF Records Processed
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Amount of NON-TANF Records Processed........" + NumberToString
      (local.NumNontanfRcordsRead.TotalInteger, 15);
    local.ReportHandling.RptDetail = TrimEnd(local.ReportHandling.RptDetail) + "."
      + NumberToString(local.NumNontanfRcordsRead.TotalInteger * 100, 14, 2);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Amount of Advancement Records
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Amount of Advancement Refunds Processed....." + NumberToString
      (local.TotalAdvancements.TotalInteger, 15);
    local.ReportHandling.RptDetail = TrimEnd(local.ReportHandling.RptDetail) + "."
      + NumberToString(local.TotalAdvancements.TotalInteger * 100, 14, 2);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // WRITE the CONTROL REPORT -- Total Amount of Collection Records
    // * * * * * * * * * *
    local.ReportHandling.RptDetail =
      "Total Amount of Collection Refunds Processed......" + NumberToString
      (local.TotalCollections.TotalInteger, 15);
    local.ReportHandling.RptDetail = TrimEnd(local.ReportHandling.RptDetail) + "."
      + NumberToString(local.TotalCollections.TotalInteger * 100, 14, 2);
    local.ReportHandling.ProgramName = "SWEFB670";
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
    local.ReportHandling.ProgramName = "SWEFB670";
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
    // Check that TOTALS Balance
    // * * * * * * * * * *
    if (local.TotalNumRecordsWritten.TotalInteger != local
      .NumNontanfRcordsRead.TotalInteger + local
      .NumTanfRecordsRead.TotalInteger)
    {
      local.ExitStateWorkArea.Message =
        "Total Amount of Records Written to File";
      local.ErrorFound.Flag = "Y";
    }

    if (local.TotalNumRecordsWritten.Count != local
      .NumNontanfRcordsRead.Count + local.NumTanfRecordsRead.Count)
    {
      local.ExitStateWorkArea.Message =
        "Total Count of Records Written to File";
      local.ErrorFound.Flag = "Y";
    }

    // * * * * * * * * * *
    // Error found - write message on CONTROL Report
    // * * * * * * * * * *
    if (AsChar(local.ErrorFound.Flag) == 'Y' || AsChar
      (local.ErrorFileOpen.Flag) == 'Y')
    {
      // * * * * * * * * * *
      // WRITE the CONTROL REPORT -- ERROR Message
      // * * * * * * * * * *
      local.ReportHandling.RptDetail =
        "* * *  PROGRAM ABORT - SEE ERROR REPORT * * *";
      local.ReportHandling.ProgramName = "SWEFB670";
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
      local.ReportHandling.ProgramName = "SWEFB670";
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
    else if (local.TotalNumRecordsWritten.Count == 0)
    {
      // * * * * * * * * * *
      // WRITE the CONTROL REPORT -- NO RECORDS PROCESSED
      // * * * * * * * * * *
      local.ReportHandling.RptDetail =
        "* * *  NO Records were Written this Run * * *";
      local.ReportHandling.ProgramName = "SWEFB670";
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
      local.ReportHandling.ProgramName = "SWEFB670";
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
    local.ReportHandling.ProgramName = "SWEFB670";
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

    // *********************************************
    // * Call external to close the output file    *
    // *********************************************
    local.PassArea.FileInstruction = local.HardcodeClose.FileInstruction;
    UseEabWriteFedRefundRecord2();

    if (!IsEmpty(local.PassArea.TextReturnCode))
    {
      // 01/02/99  SWSRPDP Changes to meet DIR Batch Report Standards.
      // * * * * * * * * * *
      // ERROR Condition -- Write Report and ABEND
      // * * * * * * * * * *
      ExitState = "FILE_CLOSE_ERROR";

      // * * * * * * * * * *
      // SAVE the Exit_State message
      // * * * * * * * * * *
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.ErrorFound.Flag = "Y";
    }

    // Process ERROR Report
    if (AsChar(local.ErrorFound.Flag) == 'Y')
    {
      if (AsChar(local.ErrorFileOpen.Flag) == 'N')
      {
        // * * * * * * * * * *
        // OPEN the ERROR REPORT
        // * * * * * * * * * *
        local.ReportHandling.ProcessDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.ReportHandling.ProgramName = "SWEFB670";
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

        local.ErrorFileOpen.Flag = "Y";
      }

      // * * * * * * * * * *
      // WRITE the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.RptDetail = " " + local.ExitStateWorkArea.Message;
      local.ReportHandling.ProgramName = "SWEFB670";
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
      local.ReportHandling.ProgramName = "SWEFB670";
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
    }

    if (AsChar(local.ErrorFileOpen.Flag) == 'Y')
    {
      // * * * * * * * * * *
      // CLOSE the ERROR REPORT
      // * * * * * * * * * *
      local.ReportHandling.ProgramName = "SWEFB670";
      local.ReportProcessing.Action = local.HardcodeClose.FileInstruction;
      UseCabErrorReport2();

      if (Equal(local.ReportProcessing.Status, "OK"))
      {
        // A Program ERROR Occured to get HERE
        // -- ALL Errors have been written to Reports and files closed
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
      else
      {
        ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

        return;
      }
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextReturnCode = source.TextReturnCode;
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

  private void UseEabWriteFedRefundRecord1()
  {
    var useImport = new EabWriteFedRefundRecord.Import();
    var useExport = new EabWriteFedRefundRecord.Export();

    useImport.FdsoNtfyFedRefTapeRec1999.Assign(local.FdsoNtfyFedRefTapeRec1999);
    MoveExternal(local.PassArea, useImport.ExternalParms);
    MoveExternal(local.PassArea, useExport.ExternalParms);

    Call(EabWriteFedRefundRecord.Execute, useImport, useExport);

    MoveExternal(useExport.ExternalParms, local.PassArea);
  }

  private void UseEabWriteFedRefundRecord2()
  {
    var useImport = new EabWriteFedRefundRecord.Import();
    var useExport = new EabWriteFedRefundRecord.Export();

    MoveExternal(local.PassArea, useImport.ExternalParms);

    Call(EabWriteFedRefundRecord.Execute, useImport, useExport);
  }

  private void UseEabWriteFedRefundRecord3()
  {
    var useImport = new EabWriteFedRefundRecord.Import();
    var useExport = new EabWriteFedRefundRecord.Export();

    MoveExternal(local.PassArea, useImport.ExternalParms);
    MoveExternal(local.PassArea, useExport.ExternalParms);

    Call(EabWriteFedRefundRecord.Execute, useImport, useExport);

    MoveExternal(useExport.ExternalParms, local.PassArea);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailReceiptRefund()
  {
    entities.ExistingCashReceiptEvent.Populated = false;
    entities.ExistingCashReceipt.Populated = false;
    entities.ExistingCashReceiptDetail.Populated = false;
    entities.ExistingReceiptRefund.Populated = false;

    return ReadEach("ReadCashReceiptCashReceiptDetailReceiptRefund",
      null,
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingReceiptRefund.CrvIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingReceiptRefund.CstIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingReceiptRefund.CrtIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceipt.ReceivedDate = db.GetDate(reader, 4);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingReceiptRefund.CrdIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 7);
        entities.ExistingCashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 8);
        entities.ExistingCashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 13);
        entities.ExistingReceiptRefund.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingReceiptRefund.ReasonCode = db.GetString(reader, 15);
        entities.ExistingReceiptRefund.Taxid = db.GetNullableString(reader, 16);
        entities.ExistingReceiptRefund.PayeeName =
          db.GetNullableString(reader, 17);
        entities.ExistingReceiptRefund.Amount = db.GetDecimal(reader, 18);
        entities.ExistingReceiptRefund.OffsetTaxYear =
          db.GetNullableInt32(reader, 19);
        entities.ExistingReceiptRefund.RequestDate = db.GetDate(reader, 20);
        entities.ExistingReceiptRefund.CreatedBy = db.GetString(reader, 21);
        entities.ExistingReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 22);
        entities.ExistingReceiptRefund.OffsetClosed = db.GetString(reader, 23);
        entities.ExistingReceiptRefund.DateTransmitted =
          db.GetNullableDate(reader, 24);
        entities.ExistingReceiptRefund.ReasonText =
          db.GetNullableString(reader, 25);
        entities.ExistingReceiptRefund.LastUpdatedBy =
          db.GetNullableString(reader, 26);
        entities.ExistingReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 27);
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        entities.ExistingReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ExistingReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private bool ReadCashReceiptDetailAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingReceiptRefund.Populated);
    entities.ExistingCashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.ExistingReceiptRefund.CdaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.ExistingCashReceiptDetailAddress.Street1 =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.ExistingCashReceiptDetailAddress.City =
          db.GetString(reader, 3);
        entities.ExistingCashReceiptDetailAddress.State =
          db.GetString(reader, 4);
        entities.ExistingCashReceiptDetailAddress.ZipCode5 =
          db.GetString(reader, 5);
        entities.ExistingCashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingCashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ExistingCashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingCashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingCashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailAddress2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.ExistingCashReceiptDetailAddress.Street1 =
          db.GetString(reader, 1);
        entities.ExistingCashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.ExistingCashReceiptDetailAddress.City =
          db.GetString(reader, 3);
        entities.ExistingCashReceiptDetailAddress.State =
          db.GetString(reader, 4);
        entities.ExistingCashReceiptDetailAddress.ZipCode5 =
          db.GetString(reader, 5);
        entities.ExistingCashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingCashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ExistingCashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingCashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingCashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ExistingReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 1);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.Populated = true;
      });
  }

  private void UpdateReceiptRefund()
  {
    var dateTransmitted = local.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingReceiptRefund.Populated = false;
    Update("UpdateReceiptRefund",
      (db, command) =>
      {
        db.SetNullableDate(command, "dateTransmitted", dateTransmitted);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ExistingReceiptRefund.DateTransmitted = dateTransmitted;
    entities.ExistingReceiptRefund.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingReceiptRefund.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingReceiptRefund.Populated = true;
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
    /// A value of PaymentRequestFound.
    /// </summary>
    [JsonPropertyName("paymentRequestFound")]
    public Common PaymentRequestFound
    {
      get => paymentRequestFound ??= new();
      set => paymentRequestFound = value;
    }

    /// <summary>
    /// A value of TotalCollections.
    /// </summary>
    [JsonPropertyName("totalCollections")]
    public Common TotalCollections
    {
      get => totalCollections ??= new();
      set => totalCollections = value;
    }

    /// <summary>
    /// A value of TotalAdvancements.
    /// </summary>
    [JsonPropertyName("totalAdvancements")]
    public Common TotalAdvancements
    {
      get => totalAdvancements ??= new();
      set => totalAdvancements = value;
    }

    /// <summary>
    /// A value of ErrorFileOpen.
    /// </summary>
    [JsonPropertyName("errorFileOpen")]
    public Common ErrorFileOpen
    {
      get => errorFileOpen ??= new();
      set => errorFileOpen = value;
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
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public ReceiptRefund ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of FdsoNtfyFedRefTapeRec1999.
    /// </summary>
    [JsonPropertyName("fdsoNtfyFedRefTapeRec1999")]
    public FdsoNtfyFedRefTapeRec1999 FdsoNtfyFedRefTapeRec1999
    {
      get => fdsoNtfyFedRefTapeRec1999 ??= new();
      set => fdsoNtfyFedRefTapeRec1999 = value;
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
    /// A value of TotalNumRecordsWritten.
    /// </summary>
    [JsonPropertyName("totalNumRecordsWritten")]
    public Common TotalNumRecordsWritten
    {
      get => totalNumRecordsWritten ??= new();
      set => totalNumRecordsWritten = value;
    }

    /// <summary>
    /// A value of NumNontanfRcordsRead.
    /// </summary>
    [JsonPropertyName("numNontanfRcordsRead")]
    public Common NumNontanfRcordsRead
    {
      get => numNontanfRcordsRead ??= new();
      set => numNontanfRcordsRead = value;
    }

    /// <summary>
    /// A value of NumTanfRecordsRead.
    /// </summary>
    [JsonPropertyName("numTanfRecordsRead")]
    public Common NumTanfRecordsRead
    {
      get => numTanfRecordsRead ??= new();
      set => numTanfRecordsRead = value;
    }

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

    private Common paymentRequestFound;
    private Common totalCollections;
    private Common totalAdvancements;
    private Common errorFileOpen;
    private Common errorFound;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling reportProcessing;
    private EabReportSend reportHandling;
    private ReceiptRefund zeroDate;
    private FdsoNtfyFedRefTapeRec1999 fdsoNtfyFedRefTapeRec1999;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common totalNumRecordsWritten;
    private Common numNontanfRcordsRead;
    private Common numTanfRecordsRead;
    private ProgramControlTotal programControlTotal;
    private External hardcodeOpen;
    private External hardcodeClose;
    private External hardcodeWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
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
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailAddress")]
    public CashReceiptDetailAddress ExistingCashReceiptDetailAddress
    {
      get => existingCashReceiptDetailAddress ??= new();
      set => existingCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of ExistingReceiptRefund.
    /// </summary>
    [JsonPropertyName("existingReceiptRefund")]
    public ReceiptRefund ExistingReceiptRefund
    {
      get => existingReceiptRefund ??= new();
      set => existingReceiptRefund = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    private CashReceiptEvent existingCashReceiptEvent;
    private PaymentRequest paymentRequest;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptDetailAddress existingCashReceiptDetailAddress;
    private ReceiptRefund existingReceiptRefund;
    private CashReceiptSourceType existingCashReceiptSourceType;
  }
#endregion
}
