// Program: LE_B578_RECEIPT_DOL_UI_PAYMENTS, ID: 945095737, model: 746.
// Short name: SWEL578B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B578_RECEIPT_DOL_UI_PAYMENTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB578ReceiptDolUiPayments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B578_RECEIPT_DOL_UI_PAYMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB578ReceiptDolUiPayments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB578ReceiptDolUiPayments.
  /// </summary>
  public LeB578ReceiptDolUiPayments(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 05/14/12  GVandy            CQ33628    Initial Development
    // 06/16/12  GVandy	    CQ33628    Changed to receipt the withholding 
    // directly
    // 				       into KAECSES instead of forwarding to KPC.
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // This program takes a return file from Kansas Department of Labor (KDOL) 
    // containing
    // withholding from Unemployment Compensation (UI) and allocates the 
    // withholding amount
    // to appropriate court orders for the Non Custodial Parent (NCP).  Cash 
    // receipting is
    // then performed for each court order amount.
    // Additionally, we will create an ORDIWO2B document and associated legal 
    // information
    // which will be mailed to the NCP detailing the amounts that are being 
    // withheld from
    // the NCP's UI if this is the first UI withholding under a court order.
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ------------------------------------------------------------------------------
    // -- Read the PPI record.
    // ------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------------------
    // -- Read for restart info.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------------------
    // -- Open the error report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Open the control report.
    // ------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Control Report. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Pre-edit the KDOL file for record types, footer totals, etc.
    // ------------------------------------------------------------------------------
    UseLeB578PreEditDolFile();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------------------
    // -- Open the UI offset file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadKdolUiOffsetInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening KDOL UI offset input file. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ------------------------------------------------------------------------------
      // -- Extract the restart information.
      // ------------------------------------------------------------------------------
      // -- Checkpoint Info
      // Positions   Value
      //  01 - 15    Running KDOL record count
      //  16 - 16    Blank
      //  17 - 31    Running KDOL record dollar amount
      //  32 - 32    Blank
      //  33 - 41    Last SSN processed
      //  42 - 48    Blank(s)
      //  49 - 63    Running recorded CRD count
      //  64 - 64    Blank
      //  65 - 79    Running recorded CRD amount
      //  80 - 80    Blank
      //  81 - 95    Running released CRD count
      //  96 - 96    Blank
      //  97 - 104   KDOL File Creation Date from the header record
      // 105 - 105   Blank
      // 106 - 120   Running release CRD amount
      // 121 - 121   Blank
      // 122 - 136   Running suspended CRD count
      // 137 - 137   Blank
      // 138 - 152   Running suspended CRD amount
      // 153 - 153   Blank
      // 154 - 168   Cash Receipt ID
      local.Kdol.TotalInteger =
        StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 15));
      local.Kdol.TotalCurrency =
        StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 17, 15)) / (
          decimal)100;
      local.Restart.ObligorSocialSecurityNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 33, 9);
      local.RecCollections.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 49, 15));
      local.RecCollections.TotalCurrency =
        StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 65, 15)) / (
          decimal)100;
      local.RelCollections.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 81, 15));
      local.RestartKdolFileDate.TextDate =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 97, 8);
      local.RelCollections.TotalCurrency =
        StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 106, 15)) / (
          decimal)100;
      local.SusCollections.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 122, 15));
      local.SusCollections.TotalCurrency =
        StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 138, 15)) / (
          decimal)100;
      local.CashReceipt.SequentialNumber =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 154, 15));

      // ------------------------------------------------------------------------------
      // -- Write restart info to control report.
      // ------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 15; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Program restarting.  Previous checkpoint info below.";

            break;
          case 2:
            local.ForConversion.Flag = "Y";
            local.ForConversion.TotalCurrency = local.Kdol.TotalInteger;
            local.EabReportSend.RptDetail = "   KDOL File Record Number :";

            break;
          case 3:
            local.ForConversion.Flag = "N";
            local.ForConversion.TotalCurrency = local.Kdol.TotalCurrency;
            local.EabReportSend.RptDetail = "   KDOL File Dollar Amount :";

            break;
          case 4:
            local.ForConversion.Flag = "Y";
            local.ForConversion.TotalCurrency =
              StringToNumber(local.Restart.ObligorSocialSecurityNumber);
            local.EabReportSend.RptDetail = "   KDOL File SSN           :";

            break;
          case 5:
            local.ForConversion.Flag = "Y";
            local.ForConversion.TotalCurrency =
              StringToNumber(local.RestartKdolFileDate.TextDate);
            local.EabReportSend.RptDetail = "   KDOL File Creation Date :";

            break;
          case 6:
            local.ForConversion.Flag = "Y";
            local.ForConversion.TotalCurrency =
              local.CashReceipt.SequentialNumber;
            local.EabReportSend.RptDetail = "   Cash Receipt ID         :";

            break;
          case 7:
            local.ForConversion.Flag = "Y";
            local.ForConversion.TotalCurrency = local.RecCollections.Count;
            local.EabReportSend.RptDetail = "   CRD Recorded Count      :";

            break;
          case 8:
            local.ForConversion.Flag = "N";
            local.ForConversion.TotalCurrency =
              local.RecCollections.TotalCurrency;
            local.EabReportSend.RptDetail = "   CRD Recorded Amount     :";

            break;
          case 9:
            local.ForConversion.Flag = "Y";
            local.ForConversion.TotalCurrency = local.RelCollections.Count;
            local.EabReportSend.RptDetail = "   CRD Released Count      :";

            break;
          case 10:
            local.ForConversion.Flag = "N";
            local.ForConversion.TotalCurrency =
              local.RelCollections.TotalCurrency;
            local.EabReportSend.RptDetail = "   CRD Released Amount     :";

            break;
          case 11:
            local.ForConversion.Flag = "Y";
            local.ForConversion.TotalCurrency = local.SusCollections.Count;
            local.EabReportSend.RptDetail = "   CRD Suspended Count     :";

            break;
          case 12:
            local.ForConversion.Flag = "N";
            local.ForConversion.TotalCurrency =
              local.SusCollections.TotalCurrency;
            local.EabReportSend.RptDetail = "   CRD Suspended Amount    :";

            break;
          case 13:
            local.EabReportSend.RptDetail = "-- End of restart information  --";

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        if (local.Common.Count >= 2 && local.Common.Count <= 12)
        {
          UseFnCabCurrencyToText();

          if (AsChar(local.ForConversion.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " " + local
              .TextWorkArea.Text10;
          }
          else
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "  " + local
              .TextWorkArea.Text10;
          }
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing restart info to the control report.  Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // ------------------------------------------------------------------------------
      // -- Re-position the KDOL file for restart.  (The restart record count is
      // the
      // -- number of Detail records read plus 1 to account for the header 
      // header record).
      // ------------------------------------------------------------------------------
      local.Common.Count = 1;

      for(var limit = (int)local.Kdol.TotalInteger; local.Common.Count <= limit
        ; ++local.Common.Count)
      {
        local.EabFileHandling.Action = "READ";
        UseLeEabReadKdolUiOffsetInfo1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          if (Equal(local.EabFileHandling.Status, "EF"))
          {
            local.EabReportSend.RptDetail =
              "End of file reached while repositioning for restart.  Restart info = " +
              local.EabFileHandling.Status;
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Error reading KDOL UI withholding file for restart. Status = " +
              local.EabFileHandling.Status;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // -- Insure that the file creation date is the same as the checkpointed 
      // file creation date.
      if (!Equal(local.KdolFileCreationDate.TextDate,
        local.RestartKdolFileDate.TextDate))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "KDOL file date " + local
          .KdolFileCreationDate.TextDate + " does not match checkpointed file date " +
          local.RestartKdolFileDate.TextDate + ".  Checkpointed file did not complete processing!!!";
          
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Insure that the SSN at the restart position matches the checkpointed
      // SSN.
      if (!Equal(local.KdolUiInboundFile.UiWithholdingRecord, 2, 9,
        local.Restart.ObligorSocialSecurityNumber))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "SSN at restart position " + Substring
          (local.KdolUiInboundFile.UiWithholdingRecord,
          KdolUiInboundFile.UiWithholdingRecord_MaxLength, 2, 9) + " does not match checkpointed SSN " +
          (local.Restart.ObligorSocialSecurityNumber ?? "") + ".  File record number = " +
          NumberToString(local.Kdol.TotalInteger, 15);
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      // ------------------------------------------------------------------------------
      // -- Insure the KDOL file creation date is greater than the creation date
      // of the
      // -- last file processed.
      // ------------------------------------------------------------------------------
      if (!Lt(Substring(local.ProgramProcessingInfo.ParameterList, 15, 8),
        local.KdolFileCreationDate.TextDate))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "KDOL file date " + local
          .KdolFileCreationDate.TextDate + " is less than or equal to the most recently processed file date " +
          Substring(local.ProgramProcessingInfo.ParameterList, 15, 8) + ".  File should have a more recent date.";
          
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ------------------------------------------------------------------------------
      // -- Create the Cash Receipt.
      // ------------------------------------------------------------------------------
      local.CashReceiptEvent.SourceCreationDate =
        local.KdolFileCreationDate.Date;
      local.CashReceiptSourceType.Code = "KSDLUI";
      UseFnAddFdsoSdsoHeaderInfo();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage1();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error creating cash receipt.  " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Extract parms and log to the control report.
    // --
    // --   PPI parms are in the format
    // --
    // --   9999 99999 XX
    // --
    // --   where positions  1 -  4 are the office number
    // --         positions  6 - 10 are the service provider number
    // --         positions 12 - 13 are the office service provider role code
    // --         positions 15 - 22 are the KDOL creation date of the last file 
    // processed
    // --         positions 24 - 27 are the central office default atty office 
    // number
    // --         positions 29 - 33 are the central office default atty service 
    // provider num
    // --         positions 35 - 36 are the central office default atty service 
    // provider role code
    // ------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 13)))
    {
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "No special caseload for incoming interstate cases specified on the PPI record.";
            
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing PPI Parms to the control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.InterstateCaseloadOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 4));
      local.InterstateCaseloadServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 6, 5));
      local.InterstateCaseloadOfficeServiceProvider.RoleCode =
        Substring(local.ProgramProcessingInfo.ParameterList, 12, 2);

      // cq47223 Added the following parms being so the that the central office 
      // default attorney can bee passed in and assigned to legal action when it
      // is not a KS tribunal and there is not one already assigned.
      local.CentralOffDefaultAttyOffice.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 24, 4));
      local.CentralOffDefaultAttyServiceProvider.SystemGeneratedId =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 29, 5));
      local.CentralOffDefaultAttyOfficeServiceProvider.RoleCode =
        Substring(local.ProgramProcessingInfo.ParameterList, 35, 2);

      for(local.Common.Count = 1; local.Common.Count <= 6; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "PPI special caseload for incoming interstate cases:";

            break;
          case 2:
            local.EabReportSend.RptDetail = "     Office: " + Substring
              (local.ProgramProcessingInfo.ParameterList, 1, 4);

            break;
          case 3:
            local.EabReportSend.RptDetail = "     Service Provider: " + Substring
              (local.ProgramProcessingInfo.ParameterList, 6, 5);

            break;
          case 4:
            local.EabReportSend.RptDetail = "     Role Code: " + Substring
              (local.ProgramProcessingInfo.ParameterList, 12, 2);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing PPI Parms to the control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Read cash receipt identifying info.
    // ------------------------------------------------------------------------------
    if (ReadCashReceiptCashReceiptTypeCashReceiptSourceType())
    {
      // -- Continue
    }
    else
    {
      ExitState = "FN0084_CASH_RCPT_NF";
      UseEabExtractExitStateMessage1();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error reading cash receipt.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Process each withholding record returned from DOL creating the 
    // appropriate
    // -- U type cash receipt details by court order number.
    // ------------------------------------------------------------------------------
    do
    {
      // ------------------------------------------------------------------------------
      // -- Read a withholding record returned from DOL.
      // ------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseLeEabReadKdolUiOffsetInfo1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.Kdol.TotalInteger;
          ++local.RecordsSinceCommit.Count;

          break;
        case "EF":
          continue;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading KDOL UI withholding file. Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }

      if (CharAt(local.KdolUiInboundFile.UiWithholdingRecord, 1) == '2')
      {
        // -- We already pre-edited the file and validated the record types and 
        // totals.  Just
        // -- process the type '2' (Detail) records here.  Continue.
      }
      else
      {
        continue;
      }

      // ------------------------------------------------------------------------------
      // -- Separate file record into local views.
      // ------------------------------------------------------------------------------
      local.CashReceiptDetail.ObligorSocialSecurityNumber =
        Substring(local.KdolUiInboundFile.UiWithholdingRecord, 2, 9);
      local.CashReceiptDetail.ObligorFirstName =
        Substring(local.KdolUiInboundFile.UiWithholdingRecord, 11, 12);
      local.CashReceiptDetail.ObligorMiddleName =
        Substring(local.KdolUiInboundFile.UiWithholdingRecord, 24, 1);
      local.CashReceiptDetail.ObligorLastName =
        Substring(local.KdolUiInboundFile.UiWithholdingRecord, 26, 25);
      local.CashReceiptDetail.ObligorPersonNumber =
        Substring(local.KdolUiInboundFile.UiWithholdingRecord, 56, 10);
      local.CashReceiptDetail.CollectionDate =
        StringToDate(Substring(
          local.KdolUiInboundFile.UiWithholdingRecord,
        KdolUiInboundFile.UiWithholdingRecord_MaxLength, 71, 4) + "-" + Substring
        (local.KdolUiInboundFile.UiWithholdingRecord,
        KdolUiInboundFile.UiWithholdingRecord_MaxLength, 75, 2) + "-" + Substring
        (local.KdolUiInboundFile.UiWithholdingRecord,
        KdolUiInboundFile.UiWithholdingRecord_MaxLength, 77, 2));
      local.CashReceiptDetail.CollectionAmount =
        StringToNumber(Substring(
          local.KdolUiInboundFile.UiWithholdingRecord,
        KdolUiInboundFile.UiWithholdingRecord_MaxLength, 81, 8)) / (
          decimal)100;
      local.DolUiWithholding.WithholdingCertificationDate =
        StringToDate(Substring(
          local.KdolUiInboundFile.UiWithholdingRecord,
        KdolUiInboundFile.UiWithholdingRecord_MaxLength, 91, 4) + "-" + Substring
        (local.KdolUiInboundFile.UiWithholdingRecord,
        KdolUiInboundFile.UiWithholdingRecord_MaxLength, 95, 2) + "-" + Substring
        (local.KdolUiInboundFile.UiWithholdingRecord,
        KdolUiInboundFile.UiWithholdingRecord_MaxLength, 97, 2));
      local.Kdol.TotalCurrency += local.CashReceiptDetail.CollectionAmount;

      // ------------------------------------------------------------------------------
      // -- Prorate the Unemployment withholding among the court orders 
      // previously certified.
      // ------------------------------------------------------------------------------
      UseLeB578ProrateUi2CourtOrder();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.Group.Index = 0;

      for(var limit = local.Group.Count; local.Group.Index < limit; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        // ------------------------------------------------------------------------------
        // -- Create the cash receipt detail for the person, court order, and 
        // amount.
        // ------------------------------------------------------------------------------
        UseLeB578ProcessUiCrDetail();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          for(local.Common.Count = 1; local.Common.Count <= 2; ++
            local.Common.Count)
          {
            switch(local.Common.Count)
            {
              case 1:
                local.ExitStateWorkArea.Message =
                  UseEabExtractExitStateMessage2();
                local.EabReportSend.RptDetail =
                  "Error processing CRD for KDOL record below.  " + local
                  .ExitStateWorkArea.Message;

                break;
              case 2:
                local.EabReportSend.RptDetail =
                  local.KdolUiInboundFile.UiWithholdingRecord;

                break;
              default:
                break;
            }

            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport2();
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      local.Group.CheckIndex();

      // ------------------------------------------------------------------------------
      // -- Checkpoint saving all the file info needed for restarting.
      // ------------------------------------------------------------------------------
      if (local.RecordsSinceCommit.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -- Checkpoint Info
        // Positions   Value
        //  01 - 15    Running KDOL record count
        //  16 - 16    Blank
        //  17 - 31    Running KDOL record dollar amount
        //  32 - 32    Blank
        //  33 - 41    Last SSN processed
        //  42 - 48    Blank(s)
        //  49 - 63    Running recorded CRD count
        //  64 - 64    Blank
        //  65 - 79    Running recorded CRD amount
        //  80 - 80    Blank
        //  81 - 95    Running released CRD count
        //  96 - 96    Blank
        //  97 - 104   KDOL File Creation Date from the header record
        // 105 - 105   Blank
        // 106 - 120   Running release CRD amount
        // 121 - 121   Blank
        // 122 - 136   Running suspended CRD count
        // 137 - 137   Blank
        // 138 - 152   Running suspended CRD amount
        // 153 - 153   Blank
        // 154 - 168   Cash Receipt ID
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(local.Kdol.TotalInteger, 15) + " " + NumberToString
          ((long)(local.Kdol.TotalCurrency * 100), 15) + " " + (
            local.CashReceiptDetail.ObligorSocialSecurityNumber ?? "") + "       " +
          NumberToString(local.RecCollections.Count, 15) + " " + NumberToString
          ((long)(local.RecCollections.TotalCurrency * 100), 15) + " " + NumberToString
          (local.RelCollections.Count, 15);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + local
          .KdolFileCreationDate.TextDate;
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 105) + NumberToString
          ((long)(local.RelCollections.TotalCurrency * 100), 15) + " " + NumberToString
          (local.SusCollections.Count, 15) + " " + NumberToString
          ((long)(local.SusCollections.TotalCurrency * 100), 15);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (local.CashReceipt.SequentialNumber, 15);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.RecordsSinceCommit.Count = 0;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // ------------------------------------------------------------------------------
    // -- Insure the KAECSES cash receipts total the amount from the KDOL file.
    // ------------------------------------------------------------------------------
    if (local.Kdol.TotalCurrency != local.RecCollections.TotalCurrency)
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Internal Error.  Dollar amount from KDOL " + NumberToString
        ((long)(local.Kdol.TotalCurrency * 100), 15) + " does not equal dollar amount receipted " +
        NumberToString((long)(local.RecCollections.TotalCurrency * 100), 15);
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Log counts to the control report.
    // ------------------------------------------------------------------------------
    for(local.Common.Count = -1; local.Common.Count <= 15; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "   Cash Receipt ID       : " + NumberToString
            (local.CashReceipt.SequentialNumber, 7, 9);

          break;
        case 2:
          local.EabReportSend.RptDetail = "   KDOL File Creation Dt : " + local
            .KdolFileCreationDate.TextDate;

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "                                 Count      Amount";

          break;
        case 6:
          local.ForConversion.Flag = "Y";
          local.ForConversion.TotalCurrency = local.Kdol.TotalInteger;
          local.EabReportSend.RptDetail = "   KDOL File Record      :";

          break;
        case 7:
          local.ForConversion.Flag = "N";
          local.ForConversion.TotalCurrency = local.Kdol.TotalCurrency;

          break;
        case 8:
          local.ForConversion.Flag = "Y";
          local.ForConversion.TotalCurrency = local.RecCollections.Count;
          local.EabReportSend.RptDetail = "   UI Records Recorded   :";

          break;
        case 9:
          local.ForConversion.Flag = "N";
          local.ForConversion.TotalCurrency =
            local.RecCollections.TotalCurrency;

          break;
        case 10:
          local.ForConversion.Flag = "Y";
          local.ForConversion.TotalCurrency = local.RelCollections.Count;
          local.EabReportSend.RptDetail = "   UI Records Released   :";

          break;
        case 11:
          local.ForConversion.Flag = "N";
          local.ForConversion.TotalCurrency =
            local.RelCollections.TotalCurrency;

          break;
        case 12:
          local.ForConversion.Flag = "Y";
          local.ForConversion.TotalCurrency = local.SusCollections.Count;
          local.EabReportSend.RptDetail = "   UI Records Suspended  :";

          break;
        case 13:
          local.ForConversion.Flag = "N";
          local.ForConversion.TotalCurrency =
            local.SusCollections.TotalCurrency;

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      if (local.Common.Count >= 6 && local.Common.Count <= 13)
      {
        UseFnCabCurrencyToText();

        if (AsChar(local.ForConversion.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  " + local
            .TextWorkArea.Text10;
        }
        else
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "   " + local
            .TextWorkArea.Text10;
        }
      }

      if (local.Common.Count == 6 || local.Common.Count == 8 || local
        .Common.Count == 10 || local.Common.Count == 12)
      {
        continue;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while file totals to the control report.  Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Store the KDOL file creation date in Positions 15 - 22 of the PPI 
    // record.
    // -- On the next run we'll use this date to insure that file being 
    // processed was
    // -- created after this file.  This will insure we don't re-process a file 
    // or that
    // -- we get a wrong file on the FTP somehow.
    // ------------------------------------------------------------------------------
    local.ProgramProcessingInfo.ParameterList =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 14) + local
      .KdolFileCreationDate.TextDate + Substring
      (local.ProgramProcessingInfo.ParameterList, 23, 15);
    UseUpdateProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error adding KDOL file creation date to PPI record.  " + " ";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking final checkpoint.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the UI offset file from KDOL.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadKdolUiOffsetInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing KDOL UI offset input file. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the control report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing control report.  Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the error report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.TextDate = source.TextDate;
    target.Date = source.Date;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExport1ToGroup(LeB578ProrateUi2CourtOrder.Export.
    ExportGroup source, Local.GroupGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseFnAddFdsoSdsoHeaderInfo()
  {
    var useImport = new FnAddFdsoSdsoHeaderInfo.Import();
    var useExport = new FnAddFdsoSdsoHeaderInfo.Export();

    useImport.CashReceiptSourceType.Code = local.CashReceiptSourceType.Code;
    useImport.CashReceiptEvent.SourceCreationDate =
      local.CashReceiptEvent.SourceCreationDate;

    Call(FnAddFdsoSdsoHeaderInfo.Execute, useImport, useExport);

    local.CashReceipt.Assign(useExport.NonCash);
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      local.CashReceiptSourceType);
    local.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
  }

  private void UseFnCabCurrencyToText()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.ReturnAnInteger.Flag = local.ForConversion.Flag;
    useImport.Common.TotalCurrency = local.ForConversion.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseLeB578PreEditDolFile()
  {
    var useImport = new LeB578PreEditDolFile.Import();
    var useExport = new LeB578PreEditDolFile.Export();

    Call(LeB578PreEditDolFile.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.KdolFileCreationDate, local.KdolFileCreationDate);
      
    local.Kdol.TotalCurrency = useExport.Kdol.TotalCurrency;
  }

  private void UseLeB578ProcessUiCrDetail()
  {
    var useImport = new LeB578ProcessUiCrDetail.Import();
    var useExport = new LeB578ProcessUiCrDetail.Export();

    useImport.CashReceiptDetail.Assign(local.Group.Item.G);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.CashReceiptType.SystemGeneratedIdentifier;
    MoveCommon(local.RecCollections, useImport.ExportRecCollections);
    MoveCommon(local.RelCollections, useImport.ExportRelCollections);
    MoveCommon(local.SusCollections, useImport.ExportSusCollections);

    Call(LeB578ProcessUiCrDetail.Execute, useImport, useExport);

    MoveCommon(useImport.ExportRecCollections, local.RecCollections);
    MoveCommon(useImport.ExportRelCollections, local.RelCollections);
    MoveCommon(useImport.ExportSusCollections, local.SusCollections);
  }

  private void UseLeB578ProrateUi2CourtOrder()
  {
    var useImport = new LeB578ProrateUi2CourtOrder.Import();
    var useExport = new LeB578ProrateUi2CourtOrder.Export();

    useImport.InterstateCaseloadOffice.SystemGeneratedId =
      local.InterstateCaseloadOffice.SystemGeneratedId;
    useImport.InterstateCaseloadServiceProvider.SystemGeneratedId =
      local.InterstateCaseloadServiceProvider.SystemGeneratedId;
    useImport.InterstateCaseloadOfficeServiceProvider.RoleCode =
      local.InterstateCaseloadOfficeServiceProvider.RoleCode;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.DolUiWithholding.WithholdingCertificationDate =
      local.DolUiWithholding.WithholdingCertificationDate;
    useImport.CashReceiptDetail.Assign(local.CashReceiptDetail);
    useImport.CentralOffDefaultAttyOfficeServiceProvider.RoleCode =
      local.CentralOffDefaultAttyOfficeServiceProvider.RoleCode;
    useImport.CentralOffDefaultAttyOffice.SystemGeneratedId =
      local.CentralOffDefaultAttyOffice.SystemGeneratedId;
    useImport.CentralOffDefaultAttyServiceProvider.SystemGeneratedId =
      local.CentralOffDefaultAttyServiceProvider.SystemGeneratedId;

    Call(LeB578ProrateUi2CourtOrder.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Group, MoveExport1ToGroup);
  }

  private void UseLeEabReadKdolUiOffsetInfo1()
  {
    var useImport = new LeEabReadKdolUiOffsetInfo.Import();
    var useExport = new LeEabReadKdolUiOffsetInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.KdolUiInboundFile.UiWithholdingRecord =
      local.KdolUiInboundFile.UiWithholdingRecord;

    Call(LeEabReadKdolUiOffsetInfo.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.KdolUiInboundFile.UiWithholdingRecord =
      useExport.KdolUiInboundFile.UiWithholdingRecord;
  }

  private void UseLeEabReadKdolUiOffsetInfo2()
  {
    var useImport = new LeEabReadKdolUiOffsetInfo.Import();
    var useExport = new LeEabReadKdolUiOffsetInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadKdolUiOffsetInfo.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCashReceiptCashReceiptTypeCashReceiptSourceType()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptTypeCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetInt32(command, "cashReceiptId", local.CashReceipt.SequentialNumber);
          
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 5);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 7);
        entities.CashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 8);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 9);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 10);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceipt.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CashReceiptDetail G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CashReceiptDetail g;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOfficeServiceProvider")]
    public OfficeServiceProvider CentralOffDefaultAttyOfficeServiceProvider
    {
      get => centralOffDefaultAttyOfficeServiceProvider ??= new();
      set => centralOffDefaultAttyOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOffice.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOffice")]
    public Office CentralOffDefaultAttyOffice
    {
      get => centralOffDefaultAttyOffice ??= new();
      set => centralOffDefaultAttyOffice = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyServiceProvider")]
    public ServiceProvider CentralOffDefaultAttyServiceProvider
    {
      get => centralOffDefaultAttyServiceProvider ??= new();
      set => centralOffDefaultAttyServiceProvider = value;
    }

    /// <summary>
    /// A value of SusCollections.
    /// </summary>
    [JsonPropertyName("susCollections")]
    public Common SusCollections
    {
      get => susCollections ??= new();
      set => susCollections = value;
    }

    /// <summary>
    /// A value of RelCollections.
    /// </summary>
    [JsonPropertyName("relCollections")]
    public Common RelCollections
    {
      get => relCollections ??= new();
      set => relCollections = value;
    }

    /// <summary>
    /// A value of RecCollections.
    /// </summary>
    [JsonPropertyName("recCollections")]
    public Common RecCollections
    {
      get => recCollections ??= new();
      set => recCollections = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of ForConversion.
    /// </summary>
    [JsonPropertyName("forConversion")]
    public Common ForConversion
    {
      get => forConversion ??= new();
      set => forConversion = value;
    }

    /// <summary>
    /// A value of RestartKdolFileDate.
    /// </summary>
    [JsonPropertyName("restartKdolFileDate")]
    public DateWorkArea RestartKdolFileDate
    {
      get => restartKdolFileDate ??= new();
      set => restartKdolFileDate = value;
    }

    /// <summary>
    /// A value of KdolFileCreationDate.
    /// </summary>
    [JsonPropertyName("kdolFileCreationDate")]
    public DateWorkArea KdolFileCreationDate
    {
      get => kdolFileCreationDate ??= new();
      set => kdolFileCreationDate = value;
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
    /// A value of RecordsSinceCommit.
    /// </summary>
    [JsonPropertyName("recordsSinceCommit")]
    public Common RecordsSinceCommit
    {
      get => recordsSinceCommit ??= new();
      set => recordsSinceCommit = value;
    }

    /// <summary>
    /// A value of Kdol.
    /// </summary>
    [JsonPropertyName("kdol")]
    public Common Kdol
    {
      get => kdol ??= new();
      set => kdol = value;
    }

    /// <summary>
    /// A value of KdolUiInboundFile.
    /// </summary>
    [JsonPropertyName("kdolUiInboundFile")]
    public KdolUiInboundFile KdolUiInboundFile
    {
      get => kdolUiInboundFile ??= new();
      set => kdolUiInboundFile = value;
    }

    /// <summary>
    /// A value of DolUiWithholding.
    /// </summary>
    [JsonPropertyName("dolUiWithholding")]
    public DolUiWithholding DolUiWithholding
    {
      get => dolUiWithholding ??= new();
      set => dolUiWithholding = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
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
    /// A value of InterstateCaseloadOffice.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOffice")]
    public Office InterstateCaseloadOffice
    {
      get => interstateCaseloadOffice ??= new();
      set => interstateCaseloadOffice = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadServiceProvider")]
    public ServiceProvider InterstateCaseloadServiceProvider
    {
      get => interstateCaseloadServiceProvider ??= new();
      set => interstateCaseloadServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOfficeServiceProvider")]
    public OfficeServiceProvider InterstateCaseloadOfficeServiceProvider
    {
      get => interstateCaseloadOfficeServiceProvider ??= new();
      set => interstateCaseloadOfficeServiceProvider = value;
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

    private OfficeServiceProvider centralOffDefaultAttyOfficeServiceProvider;
    private Office centralOffDefaultAttyOffice;
    private ServiceProvider centralOffDefaultAttyServiceProvider;
    private Common susCollections;
    private Common relCollections;
    private Common recCollections;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private TextWorkArea textWorkArea;
    private Common forConversion;
    private DateWorkArea restartKdolFileDate;
    private DateWorkArea kdolFileCreationDate;
    private CashReceiptDetail restart;
    private Common recordsSinceCommit;
    private Common kdol;
    private KdolUiInboundFile kdolUiInboundFile;
    private DolUiWithholding dolUiWithholding;
    private Array<GroupGroup> group;
    private CashReceiptDetail cashReceiptDetail;
    private Common common;
    private Common recordType;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common fileOpened;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Office interstateCaseloadOffice;
    private ServiceProvider interstateCaseloadServiceProvider;
    private OfficeServiceProvider interstateCaseloadOfficeServiceProvider;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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

    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
  }
#endregion
}
