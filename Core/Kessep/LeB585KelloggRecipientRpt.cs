// Program: LE_B585_KELLOGG_RECIPIENT_RPT, ID: 1902440233, model: 746.
// Short name: SWEB585P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B585_KELLOGG_RECIPIENT_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB585KelloggRecipientRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B585_KELLOGG_RECIPIENT_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB585KelloggRecipientRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB585KelloggRecipientRpt.
  /// </summary>
  public LeB585KelloggRecipientRpt(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 07/09/14  GVandy	CQ42192		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Identify all 529 College Savings Participants who have met the criteria 
    // for matching Kellogg contributions.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Cssi.Code = "CSSI";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.TranCode;
    UseReadProgramProcessingInfo();

    // -------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Read the Checkpoint/Restart Record.
    // -------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabReportSend.BlankLineAfterHeading = "Y";

    // -------------------------------------------------------------------------------------
    // --  Open Business Report 1.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Business Report 1.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open Business Report 2.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabBusinessReport5();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Business Report 2.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -- Determine starting and ending reporting timeframe.
    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // -- Default to the Month of the MPPI date.
      local.ReportingPeriod.StartDate =
        AddDays(local.ProgramProcessingInfo.ProcessDate,
        -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));
      local.ReportingPeriod.EndDate =
        AddDays(AddMonths(local.ReportingPeriod.StartDate, 1), -1);
    }
    else
    {
      // -------------------------------------------------------------------------------------
      //  MPPI Parameter Info...
      // 	Position  Description
      // 	--------  
      // -----------------------------------------
      // 	001-010   Starting Report Perdiod Date (yyyy-mm-dd)
      // 	011-011   Blank
      // 	012-021   Ending Report Period Date (yyyy-mm-dd)
      // -------------------------------------------------------------------------------------
      // -- Set reporting period start and end dates to MPPI parameter values.
      local.ReportingPeriod.StartDate =
        StringToDate(Substring(local.ProgramProcessingInfo.ParameterList, 1, 10));
        
      local.ReportingPeriod.EndDate =
        StringToDate(
          Substring(local.ProgramProcessingInfo.ParameterList, 12, 10));
    }

    local.Starting.Date = local.ReportingPeriod.StartDate;
    UseCabDate2TextWithHyphens3();
    local.Ending.Date = local.ReportingPeriod.EndDate;
    UseCabDate2TextWithHyphens2();

    // -------------------------------------------------------------------------------------
    // --  Write Business Report 1 header.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 7; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 2:
          local.EabReportSend.RptDetail =
            "NCPs participating in the 529 College Savings Initiative who met qualifications for matching funds during";
            

          break;
        case 3:
          local.EabReportSend.RptDetail = "report period " + local
            .StartingDate.Text10 + " through " + local.EndingDate.Text10;

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "                                                -----  Balance -----   ------ Payment ------    Applied   ---- Amount To ----";
            

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "NCP #      NCP name              Court Order      Current    Arrears   Date           Amount       To        Family     State";
            

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "---------- --------------------  ------------   ---------  ---------   ----------  ---------    -------   --------- ---------";
            

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing Business Report 1 header.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Write Business Report 2 header.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 7; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 2:
          local.EabReportSend.RptDetail =
            "NCPs participating in the 529 College Savings Initiative who did not meet qualifications for matching funds during";
            

          break;
        case 3:
          local.EabReportSend.RptDetail = "report period " + local
            .StartingDate.Text10 + " through " + local.EndingDate.Text10;

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "                                                -----  Balance -----   ------ Payment ------    Applied  ---- Amount To -----";
            

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "NCP #      NCP name              Court Order      Current    Arrears   Date           Amount       To        Family     State";
            

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "---------- --------------------  ------------   ---------  ---------   ----------  ---------    -------  ---------- ---------";
            

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
          "Error writing Business Report 2 header.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -- Initialize counters of the number of NCP/Court Order combinations that
    // did and did not meet qualifications for matching funds.
    local.QualiedNcpCtOrd.Count = 0;
    local.NonQualifiedNcpCtOrd.Count = 0;

    // -- Find each 529 participant.
    foreach(var item in Read529AccountParticipantCsePerson())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error reading ADABAS.  CSP Number " + entities
          .CsePerson.Number + local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // -- initialize group view
      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        local.Group.Update.G.CollectionAmount = 0;
        local.Group.Update.G.CollectionDate = local.Null1.Date;
        local.Group.Update.GlocalArrearsFamily.TotalCurrency = 0;
        local.Group.Update.GlocalArrearsState.TotalCurrency = 0;
        local.Group.Update.GlocalCurrentFamily.TotalCurrency = 0;
        local.Group.Update.GlocalCurrentState.TotalCurrency = 0;
        local.Group.Update.GlocalGiftFamily.TotalCurrency = 0;
        local.Group.Update.GlocalGiftState.TotalCurrency = 0;
      }

      local.Group.CheckIndex();
      local.Group.Count = 0;
      local.Group.Index = -1;

      // -- Calculate current and arrears balance.
      local.ScreenOwedAmounts.ArrearsAmountOwed = 0;
      local.ScreenOwedAmounts.CurrentAmountOwed = 0;

      foreach(var item1 in ReadDebtDetailObligationType())
      {
        if (AsChar(entities.ObligationType.Classification) == 'A')
        {
          if (!Lt(entities.DebtDetail.DueDt, local.ReportingPeriod.StartDate))
          {
            local.ScreenOwedAmounts.CurrentAmountOwed += entities.DebtDetail.
              BalanceDueAmt;
          }
          else
          {
            local.ScreenOwedAmounts.ArrearsAmountOwed += entities.DebtDetail.
              BalanceDueAmt;
          }
        }
        else
        {
          local.ScreenOwedAmounts.ArrearsAmountOwed += entities.DebtDetail.
            BalanceDueAmt;
        }
      }

      local.ArrearsCollection.TotalCurrency = 0;

      // -- Read all non REIP cash receipt details received during the reporting
      // period.
      foreach(var item1 in ReadCashReceiptDetail())
      {
        if (!ReadCashReceiptSourceType())
        {
          continue;
        }

        if (Equal(entities.CashReceiptSourceType.Code, local.Cssi.Code))
        {
          // -- Exclude Kellogg payments (CSSI source type).
          continue;
        }

        if (Equal(entities.CashReceiptDetail.CourtOrderNumber,
          entities.Data529AccountParticipant.StandardNumber))
        {
          // -- Continue.
        }
        else if (IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
        {
          // -- If any money from this cash receipt detail posted to the court 
          // order or
          //    if there is money remaining to distribute then include this cash
          // receipt detail
          //    amount on the report.
          if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) !=
            'Y')
          {
            goto Test;
          }

          if (ReadCollection1())
          {
            goto Test;
          }

          continue;
        }
        else
        {
          // -- Skip this cash receipt detail.
          continue;
        }

Test:

        ++local.Group.Index;
        local.Group.CheckSize();

        MoveCashReceiptDetail(entities.CashReceiptDetail, local.Group.Update.G);

        foreach(var item2 in ReadCollection2())
        {
          if (Equal(entities.Collection.ProgramAppliedTo, "NA") || Equal
            (entities.Collection.ProgramAppliedTo, "NAI"))
          {
            switch(AsChar(entities.Collection.AppliedToCode))
            {
              case 'C':
                local.Group.Update.GlocalCurrentFamily.TotalCurrency =
                  local.Group.Item.GlocalCurrentFamily.TotalCurrency + entities
                  .Collection.Amount;

                break;
              case 'A':
                local.Group.Update.GlocalArrearsFamily.TotalCurrency =
                  local.Group.Item.GlocalArrearsFamily.TotalCurrency + entities
                  .Collection.Amount;
                local.ArrearsCollection.TotalCurrency += entities.Collection.
                  Amount;

                break;
              case 'G':
                local.Group.Update.GlocalGiftFamily.TotalCurrency =
                  local.Group.Item.GlocalGiftFamily.TotalCurrency + entities
                  .Collection.Amount;

                break;
              default:
                break;
            }
          }
          else
          {
            switch(AsChar(entities.Collection.AppliedToCode))
            {
              case 'C':
                local.Group.Update.GlocalCurrentState.TotalCurrency =
                  local.Group.Item.GlocalCurrentState.TotalCurrency + entities
                  .Collection.Amount;

                break;
              case 'A':
                local.Group.Update.GlocalArrearsState.TotalCurrency =
                  local.Group.Item.GlocalArrearsState.TotalCurrency + entities
                  .Collection.Amount;
                local.ArrearsCollection.TotalCurrency += entities.Collection.
                  Amount;

                break;
              case 'G':
                local.Group.Update.GlocalGiftState.TotalCurrency =
                  local.Group.Item.GlocalGiftState.TotalCurrency + entities
                  .Collection.Amount;

                break;
              default:
                break;
            }
          }
        }
      }

      // -- If current balance = $0.00 and at least $1 applied to arrears then 
      // write to
      //    "Met Qualifications" report.  Else write to "Did Not Meet 
      // Qualifications" Report...
      if (local.ScreenOwedAmounts.CurrentAmountOwed == 0 && (
        local.ScreenOwedAmounts.ArrearsAmountOwed == 0 || local
        .ArrearsCollection.TotalCurrency >= 1M))
      {
        local.Met529Qualifications.Flag = "Y";
        ++local.QualiedNcpCtOrd.Count;
      }
      else
      {
        local.Met529Qualifications.Flag = "N";
        ++local.NonQualifiedNcpCtOrd.Count;
      }

      local.Group.Index = -1;

      do
      {
        if (local.Group.Index == -1)
        {
          local.EabReportSend.RptDetail = "";

          // -- Convert current balance amount to text
          local.CurrentBalanceCommon.TotalCurrency =
            local.ScreenOwedAmounts.CurrentAmountOwed;
          UseFnCabCurrencyToText5();

          // -- Convert arrears balance amount to text
          local.ArrearsBalanceCommon.TotalCurrency =
            local.ScreenOwedAmounts.ArrearsAmountOwed;
          UseFnCabCurrencyToText4();

          // -- First line for the NCP should include the NCP #, Name, Court 
          // Order #, and balance amounts.
          local.EabReportSend.RptDetail = entities.CsePerson.Number + " " + Substring
            (local.CsePersonsWorkSet.FormattedName,
            CsePersonsWorkSet.FormattedName_MaxLength, 1, 20) + "  " + entities
            .Data529AccountParticipant.StandardNumber;
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 49) + Substring
            (local.CurrentBalanceTextWorkArea.Text10,
            TextWorkArea.Text10_MaxLength, 2, 9) + "  " + Substring
            (local.ArrearsBalanceTextWorkArea.Text10,
            TextWorkArea.Text10_MaxLength, 2, 9);
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        if (local.Group.Count == 0)
        {
          // -- There were no payments made during the reporting period.
          for(local.Common.Count = 1; local.Common.Count <= 2; ++
            local.Common.Count)
          {
            ++local.Group.Index;
            local.Group.CheckSize();

            switch(local.Common.Count)
            {
              case 1:
                // -- Just write out the NCP #, Name, and Balance Info that was 
                // set above.
                // -- No additional processing required here.
                break;
              case 2:
                // -- Write a blank line
                local.EabReportSend.RptDetail = "";

                break;
              default:
                break;
            }

            local.EabFileHandling.Action = "WRITE";

            if (AsChar(local.Met529Qualifications.Flag) == 'Y')
            {
              UseCabBusinessReport3();
            }
            else
            {
              UseCabBusinessReport6();
            }

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // -- Write to the error report.
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(02) Error Writing Business Report...  Returned Status = " + local
                .EabFileHandling.Status;
              UseCabErrorReport2();

              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          break;
        }
        else
        {
          // -- One or more payments were made during the reporting period.
          ++local.Group.Index;
          local.Group.CheckSize();

          for(local.Common.Count = 1; local.Common.Count <= 4; ++
            local.Common.Count)
          {
            switch(local.Common.Count)
            {
              case 1:
                // -- Convert collection date to text
                local.PaymentDateDateWorkArea.Date =
                  local.Group.Item.G.CollectionDate;
                UseCabDate2TextWithHyphens1();

                // -- Convert payment amount to text
                local.PaymentAmountCommon.TotalCurrency =
                  local.Group.Item.G.CollectionAmount;
                UseFnCabCurrencyToText3();
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 71) + local
                  .PaymentDateTextWorkArea.Text10 + "   " + Substring
                  (local.PaymentAmountTextWorkArea.Text10,
                  TextWorkArea.Text10_MaxLength, 2, 9);

                // -- Convert current family amount to text
                local.FamilyAmountCommon.TotalCurrency =
                  local.Group.Item.GlocalCurrentFamily.TotalCurrency;
                UseFnCabCurrencyToText2();

                // -- Convert current state amount to text
                local.StateAmountCommon.TotalCurrency =
                  local.Group.Item.GlocalCurrentState.TotalCurrency;
                UseFnCabCurrencyToText1();
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 96) + "Current    " + Substring
                  (local.FamilyAmountTextWorkArea.Text10,
                  TextWorkArea.Text10_MaxLength, 2, 9) + " " + Substring
                  (local.StateAmountTextWorkArea.Text10,
                  TextWorkArea.Text10_MaxLength, 2, 9);

                break;
              case 2:
                // -- Convert arrears family amount to text
                local.FamilyAmountCommon.TotalCurrency =
                  local.Group.Item.GlocalArrearsFamily.TotalCurrency;
                UseFnCabCurrencyToText2();

                // -- Convert arrears state amount to text
                local.StateAmountCommon.TotalCurrency =
                  local.Group.Item.GlocalArrearsState.TotalCurrency;
                UseFnCabCurrencyToText1();
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 96) + "Arrears    " + Substring
                  (local.FamilyAmountTextWorkArea.Text10,
                  TextWorkArea.Text10_MaxLength, 2, 9) + " " + Substring
                  (local.StateAmountTextWorkArea.Text10,
                  TextWorkArea.Text10_MaxLength, 2, 9);

                break;
              case 3:
                if (local.Group.Item.GlocalGiftFamily.TotalCurrency == 0 && local
                  .Group.Item.GlocalGiftState.TotalCurrency == 0)
                {
                  // -- Don't write zero gift amounts to the reports
                  continue;
                }

                // -- Convert gift family amount to text
                local.FamilyAmountCommon.TotalCurrency =
                  local.Group.Item.GlocalGiftFamily.TotalCurrency;
                UseFnCabCurrencyToText2();

                // -- Convert gift state amount to text
                local.StateAmountCommon.TotalCurrency =
                  local.Group.Item.GlocalGiftState.TotalCurrency;
                UseFnCabCurrencyToText1();
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 96) + "Gift       " + Substring
                  (local.FamilyAmountTextWorkArea.Text10,
                  TextWorkArea.Text10_MaxLength, 2, 9) + " " + Substring
                  (local.StateAmountTextWorkArea.Text10,
                  TextWorkArea.Text10_MaxLength, 2, 9);

                break;
              case 4:
                // -- Write a blank line
                local.EabReportSend.RptDetail = "";

                break;
              default:
                break;
            }

            local.EabFileHandling.Action = "WRITE";

            if (AsChar(local.Met529Qualifications.Flag) == 'Y')
            {
              UseCabBusinessReport3();
            }
            else
            {
              UseCabBusinessReport6();
            }

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // -- Write to the error report.
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error Writing Business Report...  Returned Status = " + local
                .EabFileHandling.Status;
              UseCabErrorReport2();

              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Set rpt_detail to spaces so that the NCP #, NCP Name, Court 
            // Order #, and Balance amounts won't be repeated on the next
            // payment.
            local.EabReportSend.RptDetail = "";
          }
        }
      }
      while(local.Group.Index + 1 < local.Group.Count);

      ++local.RecordsRead.Count;

      if (local.RecordsRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        local.RecordsRead.Count = 0;

        // -- Set restart indicator to "N".  This report will always restart 
        // from the beginning.
        local.ProgramCheckpointRestart.RestartInd = "N";
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Number of NCP/Court Order Combinations that qualified for Matching Funds..........." +
            NumberToString(local.QualiedNcpCtOrd.Count, 9, 7);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Number of NCP/Court Order Combinations that did not qualify for Matching Funds....." +
            NumberToString(local.NonQualifiedNcpCtOrd.Count, 9, 7);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(01) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Close Business Report 1.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing Business Report 1.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close Business Report 2.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport4();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing Business Report 2.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport4()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport5()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport6()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.PaymentDateDateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.PaymentDateTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Ending.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.EndingDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens3()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Starting.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.StartingDate.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnCabCurrencyToText1()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    MoveCommon(local.StateAmountCommon, useImport.Common);

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.StateAmountTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText2()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    MoveCommon(local.FamilyAmountCommon, useImport.Common);

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.FamilyAmountTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText3()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    MoveCommon(local.PaymentAmountCommon, useImport.Common);

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.PaymentAmountTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText4()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    MoveCommon(local.ArrearsBalanceCommon, useImport.Common);

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.ArrearsBalanceTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText5()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    MoveCommon(local.CurrentBalanceCommon, useImport.Common);

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.CurrentBalanceTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> Read529AccountParticipantCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Data529AccountParticipant.Populated = false;

    return ReadEach("Read529AccountParticipantCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Data529AccountParticipant.Identifier = db.GetInt32(reader, 0);
        entities.Data529AccountParticipant.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.Data529AccountParticipant.StartDate =
          db.GetNullableDate(reader, 2);
        entities.Data529AccountParticipant.EndDate =
          db.GetNullableDate(reader, 3);
        entities.Data529AccountParticipant.CreatedBy = db.GetString(reader, 4);
        entities.Data529AccountParticipant.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Data529AccountParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Data529AccountParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.Data529AccountParticipant.CspNumber = db.GetString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.CsePerson.Populated = true;
        entities.Data529AccountParticipant.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.
          SetNullableString(command, "oblgorPrsnNbr", entities.CsePerson.Number);
          
        db.SetNullableDate(
          command, "startDate",
          local.ReportingPeriod.StartDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          local.ReportingPeriod.EndDate.GetValueOrDefault());
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
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptDetail.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
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
          
        db.SetNullableString(
          command, "ctOrdAppliedTo",
          entities.Data529AccountParticipant.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetNullableString(
          command, "ctOrdAppliedTo",
          entities.Data529AccountParticipant.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationType",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt", local.ReportingPeriod.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "standardNo",
          entities.Data529AccountParticipant.StandardNumber ?? "");
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ObligationType.Classification = db.GetString(reader, 10);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
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

      /// <summary>
      /// A value of GlocalArrearsState.
      /// </summary>
      [JsonPropertyName("glocalArrearsState")]
      public Common GlocalArrearsState
      {
        get => glocalArrearsState ??= new();
        set => glocalArrearsState = value;
      }

      /// <summary>
      /// A value of GlocalArrearsFamily.
      /// </summary>
      [JsonPropertyName("glocalArrearsFamily")]
      public Common GlocalArrearsFamily
      {
        get => glocalArrearsFamily ??= new();
        set => glocalArrearsFamily = value;
      }

      /// <summary>
      /// A value of GlocalCurrentState.
      /// </summary>
      [JsonPropertyName("glocalCurrentState")]
      public Common GlocalCurrentState
      {
        get => glocalCurrentState ??= new();
        set => glocalCurrentState = value;
      }

      /// <summary>
      /// A value of GlocalCurrentFamily.
      /// </summary>
      [JsonPropertyName("glocalCurrentFamily")]
      public Common GlocalCurrentFamily
      {
        get => glocalCurrentFamily ??= new();
        set => glocalCurrentFamily = value;
      }

      /// <summary>
      /// A value of GlocalGiftState.
      /// </summary>
      [JsonPropertyName("glocalGiftState")]
      public Common GlocalGiftState
      {
        get => glocalGiftState ??= new();
        set => glocalGiftState = value;
      }

      /// <summary>
      /// A value of GlocalGiftFamily.
      /// </summary>
      [JsonPropertyName("glocalGiftFamily")]
      public Common GlocalGiftFamily
      {
        get => glocalGiftFamily ??= new();
        set => glocalGiftFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CashReceiptDetail g;
      private Common glocalArrearsState;
      private Common glocalArrearsFamily;
      private Common glocalCurrentState;
      private Common glocalCurrentFamily;
      private Common glocalGiftState;
      private Common glocalGiftFamily;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
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
    /// A value of StateAmountTextWorkArea.
    /// </summary>
    [JsonPropertyName("stateAmountTextWorkArea")]
    public TextWorkArea StateAmountTextWorkArea
    {
      get => stateAmountTextWorkArea ??= new();
      set => stateAmountTextWorkArea = value;
    }

    /// <summary>
    /// A value of StateAmountCommon.
    /// </summary>
    [JsonPropertyName("stateAmountCommon")]
    public Common StateAmountCommon
    {
      get => stateAmountCommon ??= new();
      set => stateAmountCommon = value;
    }

    /// <summary>
    /// A value of FamilyAmountTextWorkArea.
    /// </summary>
    [JsonPropertyName("familyAmountTextWorkArea")]
    public TextWorkArea FamilyAmountTextWorkArea
    {
      get => familyAmountTextWorkArea ??= new();
      set => familyAmountTextWorkArea = value;
    }

    /// <summary>
    /// A value of FamilyAmountCommon.
    /// </summary>
    [JsonPropertyName("familyAmountCommon")]
    public Common FamilyAmountCommon
    {
      get => familyAmountCommon ??= new();
      set => familyAmountCommon = value;
    }

    /// <summary>
    /// A value of PaymentAmountTextWorkArea.
    /// </summary>
    [JsonPropertyName("paymentAmountTextWorkArea")]
    public TextWorkArea PaymentAmountTextWorkArea
    {
      get => paymentAmountTextWorkArea ??= new();
      set => paymentAmountTextWorkArea = value;
    }

    /// <summary>
    /// A value of PaymentAmountCommon.
    /// </summary>
    [JsonPropertyName("paymentAmountCommon")]
    public Common PaymentAmountCommon
    {
      get => paymentAmountCommon ??= new();
      set => paymentAmountCommon = value;
    }

    /// <summary>
    /// A value of ArrearsBalanceTextWorkArea.
    /// </summary>
    [JsonPropertyName("arrearsBalanceTextWorkArea")]
    public TextWorkArea ArrearsBalanceTextWorkArea
    {
      get => arrearsBalanceTextWorkArea ??= new();
      set => arrearsBalanceTextWorkArea = value;
    }

    /// <summary>
    /// A value of ArrearsBalanceCommon.
    /// </summary>
    [JsonPropertyName("arrearsBalanceCommon")]
    public Common ArrearsBalanceCommon
    {
      get => arrearsBalanceCommon ??= new();
      set => arrearsBalanceCommon = value;
    }

    /// <summary>
    /// A value of CurrentBalanceTextWorkArea.
    /// </summary>
    [JsonPropertyName("currentBalanceTextWorkArea")]
    public TextWorkArea CurrentBalanceTextWorkArea
    {
      get => currentBalanceTextWorkArea ??= new();
      set => currentBalanceTextWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentBalanceCommon.
    /// </summary>
    [JsonPropertyName("currentBalanceCommon")]
    public Common CurrentBalanceCommon
    {
      get => currentBalanceCommon ??= new();
      set => currentBalanceCommon = value;
    }

    /// <summary>
    /// A value of PaymentDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("paymentDateTextWorkArea")]
    public TextWorkArea PaymentDateTextWorkArea
    {
      get => paymentDateTextWorkArea ??= new();
      set => paymentDateTextWorkArea = value;
    }

    /// <summary>
    /// A value of PaymentDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("paymentDateDateWorkArea")]
    public DateWorkArea PaymentDateDateWorkArea
    {
      get => paymentDateDateWorkArea ??= new();
      set => paymentDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of Met529Qualifications.
    /// </summary>
    [JsonPropertyName("met529Qualifications")]
    public Common Met529Qualifications
    {
      get => met529Qualifications ??= new();
      set => met529Qualifications = value;
    }

    /// <summary>
    /// A value of ArrearsCollection.
    /// </summary>
    [JsonPropertyName("arrearsCollection")]
    public Common ArrearsCollection
    {
      get => arrearsCollection ??= new();
      set => arrearsCollection = value;
    }

    /// <summary>
    /// A value of EndingDate.
    /// </summary>
    [JsonPropertyName("endingDate")]
    public TextWorkArea EndingDate
    {
      get => endingDate ??= new();
      set => endingDate = value;
    }

    /// <summary>
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public DateWorkArea Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    /// <summary>
    /// A value of StartingDate.
    /// </summary>
    [JsonPropertyName("startingDate")]
    public TextWorkArea StartingDate
    {
      get => startingDate ??= new();
      set => startingDate = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Cssi.
    /// </summary>
    [JsonPropertyName("cssi")]
    public CashReceiptSourceType Cssi
    {
      get => cssi ??= new();
      set => cssi = value;
    }

    /// <summary>
    /// A value of ReportingPeriod.
    /// </summary>
    [JsonPropertyName("reportingPeriod")]
    public Data529AccountParticipant ReportingPeriod
    {
      get => reportingPeriod ??= new();
      set => reportingPeriod = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of QualiedNcpCtOrd.
    /// </summary>
    [JsonPropertyName("qualiedNcpCtOrd")]
    public Common QualiedNcpCtOrd
    {
      get => qualiedNcpCtOrd ??= new();
      set => qualiedNcpCtOrd = value;
    }

    /// <summary>
    /// A value of NonQualifiedNcpCtOrd.
    /// </summary>
    [JsonPropertyName("nonQualifiedNcpCtOrd")]
    public Common NonQualifiedNcpCtOrd
    {
      get => nonQualifiedNcpCtOrd ??= new();
      set => nonQualifiedNcpCtOrd = value;
    }

    private Common recordsRead;
    private ProgramCheckpointRestart programCheckpointRestart;
    private TextWorkArea stateAmountTextWorkArea;
    private Common stateAmountCommon;
    private TextWorkArea familyAmountTextWorkArea;
    private Common familyAmountCommon;
    private TextWorkArea paymentAmountTextWorkArea;
    private Common paymentAmountCommon;
    private TextWorkArea arrearsBalanceTextWorkArea;
    private Common arrearsBalanceCommon;
    private TextWorkArea currentBalanceTextWorkArea;
    private Common currentBalanceCommon;
    private TextWorkArea paymentDateTextWorkArea;
    private DateWorkArea paymentDateDateWorkArea;
    private Common met529Qualifications;
    private Common arrearsCollection;
    private TextWorkArea endingDate;
    private DateWorkArea ending;
    private TextWorkArea startingDate;
    private DateWorkArea starting;
    private ScreenOwedAmounts screenOwedAmounts;
    private LegalAction legalAction;
    private DateWorkArea null1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<GroupGroup> group;
    private CashReceiptSourceType cssi;
    private Data529AccountParticipant reportingPeriod;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Common qualiedNcpCtOrd;
    private Common nonQualifiedNcpCtOrd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Data529AccountParticipant.
    /// </summary>
    [JsonPropertyName("data529AccountParticipant")]
    public Data529AccountParticipant Data529AccountParticipant
    {
      get => data529AccountParticipant ??= new();
      set => data529AccountParticipant = value;
    }

    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private LegalAction legalAction;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private ObligationTransaction debt;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private CsePerson csePerson;
    private Collection collection;
    private Data529AccountParticipant data529AccountParticipant;
  }
#endregion
}
