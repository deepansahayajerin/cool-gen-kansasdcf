// Program: LE_BFX8_UI_DISTRIBUTION_REPORT, ID: 1902428799, model: 746.
// Short name: SWELFX8B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX8_UI_DISTRIBUTION_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeBfx8UiDistributionReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX8_UI_DISTRIBUTION_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx8UiDistributionReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx8UiDistributionReport.
  /// </summary>
  public LeBfx8UiDistributionReport(IContext context, Import import,
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
    // 03/22/14  GVandy                       Initial Development
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // This program takes a return file from Kansas Department of Labor (KDOL) 
    // containing
    // withholding from Unemployment Compensation (UI) and determines the debts 
    // and programs
    // to which the money would apply.
    // A cash receipt detail is created only for those court orders which would 
    // apply strictly to NA or NAI debts.
    // A report is created detailing the receipting process.
    // This program was developed following an error with the DOL certification 
    // program
    // which caused UI to not be offset for the week of 3/15/2014.  Once our 
    // certification
    // was corrected then DOL created the file to show what would have been
    // offset if the certification had occurred correctly.  DCFs intent is to 
    // only receipt
    // the UI offset which will apply strictly NA or NAI.
    // Those payments that would be retained by the state due to posting as an 
    // AF,FC,etc.
    // collection will not be receipted.
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
    // -- Open the business reports.
    // ------------------------------------------------------------------------------
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Busines Report. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabBusinessReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Busines Report. Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabBusinessReport5();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Busines Report. Status = " + local
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
    UseLeEabReadKdolUiOffsetInfo1();

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
    // -- Write business report headers.
    // ------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      if (local.Common.Count == 2)
      {
        local.EabReportSend.RptDetail =
          "---------  UI PAYMENTS APPLYING ONLY AS NA OR NAI  ---------";
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing header to the business report.  Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      if (local.Common.Count == 2)
      {
        local.EabReportSend.RptDetail =
          "---------  UI PAYMENTS SUSPENDING OR APPLYING ANYTHING OTHER THAN NA OR NAI  ---------";
          
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport4();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing header to the business report.  Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    for(local.Common.Count = 1; local.Common.Count <= 7; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 2:
          local.EabReportSend.RptDetail =
            "---------  UI PAYMENTS RECIEPTED BY COURT ORDER  ---------";

          break;
        case 6:
          local.EabReportSend.RptDetail = "                KDOL      COURT";

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "NCP #          AMOUNT   ORDER AMT   COURT ORDER";

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport6();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing header to the business report.  Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Create the Cash Receipt.
    // ------------------------------------------------------------------------------
    local.CashReceiptEvent.SourceCreationDate = local.KdolFileCreationDate.Date;
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

    // -- Commit the cash receipt record...
    UseExtToDoACommit();

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
      UseLeEabReadKdolUiOffsetInfo2();

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
        // -- Create the cash receipt detail for the person, court order, and 
        // amount.
        // ------------------------------------------------------------------------------
        UseLeB578ProcessUiCrDetail1();

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

        local.ReceiptMoney.Flag = "N";

        if (ReadCashReceiptDetail())
        {
          local.ApplRunMode.Text8 = "BATCH";
          local.PrworaDateOfConversion.Date = new DateTime(2000, 10, 1);
          local.AllowITypeProcInd.Flag = "N";
          local.ItypeWindow.Count = 0;

          if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
          {
            local.ProcessDate.Date = Now().Date;
          }
          else
          {
            local.ProcessDate.Date = local.ProgramProcessingInfo.ProcessDate;
          }

          // -- Set update frequency count as high as possible (i.e. 9999999) 
          // because we don't
          //    want any checkpoints to occur.  We will rollback all processing 
          // after the payment
          //    is distributed.
          local.ProgramCheckpointRestart.UpdateFrequencyCount = 9999999;

          // --  Distribute CRDs....
          UseFnAutoDistributeCrdsToDebts();

          if (local.TotalAmtSuspended.TotalCurrency > 0)
          {
            local.BusinessReport.Count = 2;
          }
          else if (ReadCollection1())
          {
            local.BusinessReport.Count = 2;
          }
          else if (ReadCollection2())
          {
            local.BusinessReport.Count = 1;
            local.ReceiptMoney.Flag = "Y";
          }
          else
          {
            local.BusinessReport.Count = 2;
          }

          // ------------------------------------------------------------------------------
          // -- Log info to the business report.  Example:
          //                   KDOL      COURT                    AMOUNT    
          // AMOUNT
          // NCP #           AMOUNT   ORDER AMT   COURT ORDER    APPLIED  
          // SUSPENDED
          // 0002440505      202.00      102.00   SG03DM006135     77.00      
          // 25.00
          // ------------------------------------------------------------------------------
          for(local.Common.Count = -2; local.Common.Count <= 6; ++
            local.Common.Count)
          {
            switch(local.Common.Count)
            {
              case -1:
                local.EabReportSend.RptDetail =
                  "----------------------------------------------------------------------------------";
                  

                break;
              case 1:
                local.EabReportSend.RptDetail =
                  "                KDOL      COURT                    AMOUNT    AMOUNT";
                  

                break;
              case 2:
                local.EabReportSend.RptDetail =
                  "NCP #          AMOUNT   ORDER AMT   COURT ORDER    APPLIED  SUSPENDED";
                  

                break;
              case 3:
                local.ForConversion.TotalCurrency =
                  local.CashReceiptDetail.CollectionAmount;
                UseFnCabCurrencyToText2();
                local.EabReportSend.RptDetail =
                  entities.CashReceiptDetail.ObligorPersonNumber + "  " + local
                  .TextWorkArea.Text10;
                local.ForConversion.TotalCurrency =
                  local.TotalAmtAttempted.TotalCurrency;
                UseFnCabCurrencyToText2();
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "   " + local
                  .TextWorkArea.Text10;
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "  " + Substring
                  (entities.CashReceiptDetail.CourtOrderNumber,
                  CashReceiptDetail.CourtOrderNumber_MaxLength, 1, 12);
                local.ForConversion.TotalCurrency =
                  local.TotalAmtProcessed.TotalCurrency;
                UseFnCabCurrencyToText2();

                if (IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "                " +
                    local.TextWorkArea.Text10;
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "  " + local
                    .TextWorkArea.Text10;
                }

                local.ForConversion.TotalCurrency =
                  local.TotalAmtSuspended.TotalCurrency;
                UseFnCabCurrencyToText2();
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "  " + local
                  .TextWorkArea.Text10;

                break;
              case 4:
                local.EabReportSend.RptDetail = "";

                break;
              case 5:
                if (local.TotalAmtProcessed.TotalCurrency == 0)
                {
                  goto AfterCycle;
                }

                local.EabReportSend.RptDetail =
                  "        COLLECTION APPLD           SUPPORTED  DEBT       DEBT";
                  

                break;
              case 6:
                local.EabReportSend.RptDetail =
                  "            AMOUNT  TO    PGM/ST      PERSON  TYPE      DUE DT";
                  

                break;
              default:
                local.EabReportSend.RptDetail = "";

                break;
            }

            local.EabFileHandling.Action = "WRITE";

            switch(local.BusinessReport.Count)
            {
              case 1:
                UseCabBusinessReport2();

                break;
              case 2:
                UseCabBusinessReport4();

                break;
              default:
                break;
            }

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error encountered while file totals to the business report.  Status = " +
                local.EabFileHandling.Status;
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

AfterCycle:

          foreach(var item in ReadCollection3())
          {
            // @@@  Log results to business report....
            ReadDebtDetail();

            if (ReadCsePerson())
            {
              local.Supported.Number = entities.Supported.Number;
            }
            else
            {
              local.Supported.Number = "";
            }

            if (ReadObligationType())
            {
              local.ObligationType.Code = entities.ObligationType.Code;
            }
            else
            {
              local.ObligationType.Code = "";
            }

            // ------------------------------------------------------------------------------
            // -- Log info to the business report.  Example:
            //         COLLECTION APPLD           SUPPORTED     DEBT    DEBT
            //             AMOUNT  TO    PGM/ST      PERSON     TYPE   DUE DT
            //              40.00   C     NA-NA   1234567890 xxxxxCS  03012013
            // ------------------------------------------------------------------------------
            local.ForConversion.TotalCurrency = entities.Collection.Amount;
            UseFnCabCurrencyToText2();
            local.EabReportSend.RptDetail = "" + "        " + local
              .TextWorkArea.Text10;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + entities
              .Collection.AppliedToCode;

            if (CharAt(entities.Collection.ProgramAppliedTo, 3) == 'I')
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "    " + entities
                .Collection.ProgramAppliedTo + "     " + local
                .Supported.Number;
            }
            else if (IsEmpty(entities.Collection.DistPgmStateAppldTo))
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "    " + entities
                .Collection.ProgramAppliedTo + " " + entities
                .Collection.DistPgmStateAppldTo + "  " + local
                .Supported.Number;
            }
            else
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "    " + entities
                .Collection.ProgramAppliedTo + "-" + entities
                .Collection.DistPgmStateAppldTo + "  " + local
                .Supported.Number;
            }

            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "  " + local
              .ObligationType.Code + "  " + NumberToString
              (DateToInt(entities.DebtDetail.DueDt), 8, 8);
            local.EabFileHandling.Action = "WRITE";

            switch(local.BusinessReport.Count)
            {
              case 1:
                UseCabBusinessReport2();

                break;
              case 2:
                UseCabBusinessReport4();

                break;
              default:
                break;
            }

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error writing business report.  Status = " + local
                .EabFileHandling.Status;
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          // -- Only process the most recently added cash receipt detail.
        }

        // ------------------------------------------------------------------------------
        // -- ROLLBACK!!!!   We don't want to keep any of the distribution 
        // processing.
        // ------------------------------------------------------------------------------
        UseEabRollbackSql();

        // ------------------------------------------------------------------------------
        // -- If the cash receipt detail applied fully NA or NAI then receipt 
        // the cash receipt detail.
        // ------------------------------------------------------------------------------
        if (AsChar(local.ReceiptMoney.Flag) == 'Y')
        {
          // -- Receipt the money for real this time.
          // ------------------------------------------------------------------------------
          // -- Create the cash receipt detail for the person, court order, and 
          // amount.
          // ------------------------------------------------------------------------------
          UseLeB578ProcessUiCrDetail2();

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

          UseExtToDoACommit();
          ++local.TotalReceiptedForReal.Count;
          local.TotalReceiptedForReal.TotalCurrency += local.Group.Item.G.
            CollectionAmount;

          // --  Log to the receipt report.
          local.ForConversion.TotalCurrency =
            local.CashReceiptDetail.CollectionAmount;
          UseFnCabCurrencyToText2();
          local.EabReportSend.RptDetail =
            entities.CashReceiptDetail.ObligorPersonNumber + "  " + local
            .TextWorkArea.Text10;
          local.ForConversion.TotalCurrency =
            local.TotalAmtAttempted.TotalCurrency;
          UseFnCabCurrencyToText2();
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "   " + local
            .TextWorkArea.Text10;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  " + Substring
            (entities.CashReceiptDetail.CourtOrderNumber,
            CashReceiptDetail.CourtOrderNumber_MaxLength, 1, 12);
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport6();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error encountered writing header to the business report.  Status = " +
              local.EabFileHandling.Status;
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      local.Group.CheckIndex();
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
        UseFnCabCurrencyToText1();

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
    // -- Log counts to the receipt report.
    // ------------------------------------------------------------------------------
    for(local.Common.Count = -2; local.Common.Count <= 15; ++local.Common.Count)
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
          local.ForConversion.TotalCurrency = local.RecCollectionsForReal.Count;
          local.EabReportSend.RptDetail = "   UI Records Recorded   :";

          break;
        case 9:
          local.ForConversion.Flag = "N";
          local.ForConversion.TotalCurrency =
            local.RecCollectionsForReal.TotalCurrency;

          break;
        case 10:
          local.ForConversion.Flag = "Y";
          local.ForConversion.TotalCurrency = local.RelCollectionsForReal.Count;
          local.EabReportSend.RptDetail = "   UI Records Released   :";

          break;
        case 11:
          local.ForConversion.Flag = "N";
          local.ForConversion.TotalCurrency =
            local.RelCollectionsForReal.TotalCurrency;

          break;
        case 12:
          local.ForConversion.Flag = "Y";
          local.ForConversion.TotalCurrency = local.SusCollectionsForReal.Count;
          local.EabReportSend.RptDetail = "   UI Records Suspended  :";

          break;
        case 13:
          local.ForConversion.Flag = "N";
          local.ForConversion.TotalCurrency =
            local.SusCollectionsForReal.TotalCurrency;

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      if (local.Common.Count >= 6 && local.Common.Count <= 13)
      {
        UseFnCabCurrencyToText1();

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
      UseCabBusinessReport6();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing header to the business report.  Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Do a final rollback.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseEabRollbackSql();

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
    UseLeEabReadKdolUiOffsetInfo1();

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
    // -- Close the business reports.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing business report.  Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabBusinessReport4();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing business report.  Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabBusinessReport6();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing business report.  Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();
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

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
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

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveEabReportSend2(EabReportSend source,
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

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport4()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport5()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport03.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport6()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport03.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnAddFdsoSdsoHeaderInfo()
  {
    var useImport = new FnAddFdsoSdsoHeaderInfo.Import();
    var useExport = new FnAddFdsoSdsoHeaderInfo.Export();

    useImport.CashReceiptEvent.SourceCreationDate =
      local.CashReceiptEvent.SourceCreationDate;
    useImport.CashReceiptSourceType.Code = local.CashReceiptSourceType.Code;

    Call(FnAddFdsoSdsoHeaderInfo.Execute, useImport, useExport);

    local.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      local.CashReceiptSourceType);
    local.CashReceipt.Assign(useExport.NonCash);
  }

  private void UseFnAutoDistributeCrdsToDebts()
  {
    var useImport = new FnAutoDistributeCrdsToDebts.Import();
    var useExport = new FnAutoDistributeCrdsToDebts.Export();

    useImport.StartCashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.EndCashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.StartCashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.EndCashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.ApplRunMode.Text8 = local.ApplRunMode.Text8;
    useImport.PrworaDateOfConversion.Date = local.PrworaDateOfConversion.Date;
    useImport.AllowITypeProcInd.Flag = local.AllowITypeProcInd.Flag;
    useImport.ItypeWindow.Count = local.ItypeWindow.Count;
    useImport.ProcessDate.Date = local.ProcessDate.Date;
    useImport.ProgramCheckpointRestart.UpdateFrequencyCount =
      local.ProgramCheckpointRestart.UpdateFrequencyCount;

    Call(FnAutoDistributeCrdsToDebts.Execute, useImport, useExport);

    MoveCashReceiptDetailStatHistory(useExport.SuspendedReason,
      local.SuspendedReason);
    local.TotalAmtSuspended.TotalCurrency =
      useExport.TotalAmtSuspended.TotalCurrency;
    local.TotalAmtProcessed.TotalCurrency =
      useExport.TotalAmtProcessed.TotalCurrency;
    local.TotalAmtAttempted.TotalCurrency =
      useExport.TotalAmtAttempted.TotalCurrency;
  }

  private void UseFnCabCurrencyToText1()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.ForConversion.TotalCurrency;
    useImport.ReturnAnInteger.Flag = local.ForConversion.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText2()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.ForConversion.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseLeB578PreEditDolFile()
  {
    var useImport = new LeB578PreEditDolFile.Import();
    var useExport = new LeB578PreEditDolFile.Export();

    Call(LeB578PreEditDolFile.Execute, useImport, useExport);

    local.Kdol.TotalCurrency = useExport.Kdol.TotalCurrency;
    MoveDateWorkArea(useExport.KdolFileCreationDate, local.KdolFileCreationDate);
      
  }

  private void UseLeB578ProcessUiCrDetail1()
  {
    var useImport = new LeB578ProcessUiCrDetail.Import();
    var useExport = new LeB578ProcessUiCrDetail.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    MoveCommon(local.RelCollections, useImport.ExportRelCollections);
    MoveCommon(local.RecCollections, useImport.ExportRecCollections);
    MoveCommon(local.SusCollections, useImport.ExportSusCollections);
    useImport.CashReceiptDetail.Assign(local.Group.Item.G);

    Call(LeB578ProcessUiCrDetail.Execute, useImport, useExport);

    MoveCommon(useImport.ExportRelCollections, local.RelCollections);
    MoveCommon(useImport.ExportRecCollections, local.RecCollections);
    MoveCommon(useImport.ExportSusCollections, local.SusCollections);
  }

  private void UseLeB578ProcessUiCrDetail2()
  {
    var useImport = new LeB578ProcessUiCrDetail.Import();
    var useExport = new LeB578ProcessUiCrDetail.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    MoveCommon(local.RecCollectionsForReal, useImport.ExportRecCollections);
    MoveCommon(local.RelCollectionsForReal, useImport.ExportRelCollections);
    MoveCommon(local.SusCollectionsForReal, useImport.ExportSusCollections);
    useImport.CashReceiptDetail.Assign(local.Group.Item.G);

    Call(LeB578ProcessUiCrDetail.Execute, useImport, useExport);

    MoveCommon(useImport.ExportRecCollections, local.RecCollectionsForReal);
    MoveCommon(useImport.ExportRelCollections, local.RelCollectionsForReal);
    MoveCommon(useImport.ExportSusCollections, local.SusCollectionsForReal);
  }

  private void UseLeB578ProrateUi2CourtOrder()
  {
    var useImport = new LeB578ProrateUi2CourtOrder.Import();
    var useExport = new LeB578ProrateUi2CourtOrder.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.CashReceiptDetail.Assign(local.CashReceiptDetail);
    useImport.DolUiWithholding.WithholdingCertificationDate =
      local.DolUiWithholding.WithholdingCertificationDate;
    useImport.InterstateCaseloadOffice.SystemGeneratedId =
      local.InterstateCaseloadOffice.SystemGeneratedId;
    useImport.InterstateCaseloadServiceProvider.SystemGeneratedId =
      local.InterstateCaseloadServiceProvider.SystemGeneratedId;
    useImport.InterstateCaseloadOfficeServiceProvider.RoleCode =
      local.InterstateCaseloadOfficeServiceProvider.RoleCode;

    Call(LeB578ProrateUi2CourtOrder.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Group, MoveExport1ToGroup);
  }

  private void UseLeEabReadKdolUiOffsetInfo1()
  {
    var useImport = new LeEabReadKdolUiOffsetInfo.Import();
    var useExport = new LeEabReadKdolUiOffsetInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadKdolUiOffsetInfo.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeEabReadKdolUiOffsetInfo2()
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

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCashReceiptCashReceiptTypeCashReceiptSourceType()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;

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
        entities.CashReceipt.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
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
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.Populated = true;
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
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Amount = db.GetDecimal(reader, 12);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 13);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 14);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 15);
        entities.Collection.AppliedToFuture = db.GetString(reader, 16);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 17);
        entities.Collection.Populated = true;
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection2",
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
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Amount = db.GetDecimal(reader, 12);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 13);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 14);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 15);
        entities.Collection.AppliedToFuture = db.GetString(reader, 16);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 17);
        entities.Collection.Populated = true;
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
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Amount = db.GetDecimal(reader, 12);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 13);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 14);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 15);
        entities.Collection.AppliedToFuture = db.GetString(reader, 16);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 17);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supported.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supported.Number = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otrGeneratedId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Collection.OtyId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of Kdol.
    /// </summary>
    [JsonPropertyName("kdol")]
    public Common Kdol
    {
      get => kdol ??= new();
      set => kdol = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of KdolUiInboundFile.
    /// </summary>
    [JsonPropertyName("kdolUiInboundFile")]
    public KdolUiInboundFile KdolUiInboundFile
    {
      get => kdolUiInboundFile ??= new();
      set => kdolUiInboundFile = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of SusCollections.
    /// </summary>
    [JsonPropertyName("susCollections")]
    public Common SusCollections
    {
      get => susCollections ??= new();
      set => susCollections = value;
    }

    /// <summary>
    /// A value of ReceiptMoney.
    /// </summary>
    [JsonPropertyName("receiptMoney")]
    public Common ReceiptMoney
    {
      get => receiptMoney ??= new();
      set => receiptMoney = value;
    }

    /// <summary>
    /// A value of ApplRunMode.
    /// </summary>
    [JsonPropertyName("applRunMode")]
    public TextWorkArea ApplRunMode
    {
      get => applRunMode ??= new();
      set => applRunMode = value;
    }

    /// <summary>
    /// A value of PrworaDateOfConversion.
    /// </summary>
    [JsonPropertyName("prworaDateOfConversion")]
    public DateWorkArea PrworaDateOfConversion
    {
      get => prworaDateOfConversion ??= new();
      set => prworaDateOfConversion = value;
    }

    /// <summary>
    /// A value of AllowITypeProcInd.
    /// </summary>
    [JsonPropertyName("allowITypeProcInd")]
    public Common AllowITypeProcInd
    {
      get => allowITypeProcInd ??= new();
      set => allowITypeProcInd = value;
    }

    /// <summary>
    /// A value of ItypeWindow.
    /// </summary>
    [JsonPropertyName("itypeWindow")]
    public Common ItypeWindow
    {
      get => itypeWindow ??= new();
      set => itypeWindow = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of SuspendedReason.
    /// </summary>
    [JsonPropertyName("suspendedReason")]
    public CashReceiptDetailStatHistory SuspendedReason
    {
      get => suspendedReason ??= new();
      set => suspendedReason = value;
    }

    /// <summary>
    /// A value of TotalAmtSuspended.
    /// </summary>
    [JsonPropertyName("totalAmtSuspended")]
    public Common TotalAmtSuspended
    {
      get => totalAmtSuspended ??= new();
      set => totalAmtSuspended = value;
    }

    /// <summary>
    /// A value of TotalAmtProcessed.
    /// </summary>
    [JsonPropertyName("totalAmtProcessed")]
    public Common TotalAmtProcessed
    {
      get => totalAmtProcessed ??= new();
      set => totalAmtProcessed = value;
    }

    /// <summary>
    /// A value of TotalAmtAttempted.
    /// </summary>
    [JsonPropertyName("totalAmtAttempted")]
    public Common TotalAmtAttempted
    {
      get => totalAmtAttempted ??= new();
      set => totalAmtAttempted = value;
    }

    /// <summary>
    /// A value of BusinessReport.
    /// </summary>
    [JsonPropertyName("businessReport")]
    public Common BusinessReport
    {
      get => businessReport ??= new();
      set => businessReport = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of RecCollectionsForReal.
    /// </summary>
    [JsonPropertyName("recCollectionsForReal")]
    public Common RecCollectionsForReal
    {
      get => recCollectionsForReal ??= new();
      set => recCollectionsForReal = value;
    }

    /// <summary>
    /// A value of RelCollectionsForReal.
    /// </summary>
    [JsonPropertyName("relCollectionsForReal")]
    public Common RelCollectionsForReal
    {
      get => relCollectionsForReal ??= new();
      set => relCollectionsForReal = value;
    }

    /// <summary>
    /// A value of SusCollectionsForReal.
    /// </summary>
    [JsonPropertyName("susCollectionsForReal")]
    public Common SusCollectionsForReal
    {
      get => susCollectionsForReal ??= new();
      set => susCollectionsForReal = value;
    }

    /// <summary>
    /// A value of TotalReceiptedForReal.
    /// </summary>
    [JsonPropertyName("totalReceiptedForReal")]
    public Common TotalReceiptedForReal
    {
      get => totalReceiptedForReal ??= new();
      set => totalReceiptedForReal = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common kdol;
    private DateWorkArea kdolFileCreationDate;
    private Common common;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private ExitStateWorkArea exitStateWorkArea;
    private External external;
    private KdolUiInboundFile kdolUiInboundFile;
    private Common recordsSinceCommit;
    private CashReceiptDetail cashReceiptDetail;
    private DolUiWithholding dolUiWithholding;
    private Array<GroupGroup> group;
    private Office interstateCaseloadOffice;
    private ServiceProvider interstateCaseloadServiceProvider;
    private OfficeServiceProvider interstateCaseloadOfficeServiceProvider;
    private Common relCollections;
    private Common recCollections;
    private Common susCollections;
    private Common receiptMoney;
    private TextWorkArea applRunMode;
    private DateWorkArea prworaDateOfConversion;
    private Common allowITypeProcInd;
    private Common itypeWindow;
    private DateWorkArea processDate;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CashReceiptDetailStatHistory suspendedReason;
    private Common totalAmtSuspended;
    private Common totalAmtProcessed;
    private Common totalAmtAttempted;
    private Common businessReport;
    private Common forConversion;
    private TextWorkArea textWorkArea;
    private CsePerson supported;
    private ObligationType obligationType;
    private Common recCollectionsForReal;
    private Common relCollectionsForReal;
    private Common susCollectionsForReal;
    private Common totalReceiptedForReal;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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

    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private CsePerson supported;
    private CsePersonAccount csePersonAccount;
    private ObligationType obligationType;
    private Obligation obligation;
  }
#endregion
}
