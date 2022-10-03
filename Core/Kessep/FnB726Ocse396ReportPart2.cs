// Program: FN_B726_OCSE_396_REPORT_PART_2, ID: 371347392, model: 746.
// Short name: SWEF726B
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
/// A program: FN_B726_OCSE_396_REPORT_PART_2.
/// </para>
/// <para>
/// Supplement to the OCSE396a report which identifies the number of non-TAF 
/// cases that qualified for the $25 Deficit Reduction Act fee during the
/// quarter.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB726Ocse396ReportPart2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B726_OCSE_396_REPORT_PART_2 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB726Ocse396ReportPart2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB726Ocse396ReportPart2.
  /// </summary>
  public FnB726Ocse396ReportPart2(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // -------------------------------------------------------------------------
    // 03/17/2008  GVandy	CQ296		Initial Coding
    // -----------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -----------------------------------------------------------------------------------------------
    // Retrieve the PPI info.
    // -----------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = global.TranCode;
    local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Control Report
    // -----------------------------------------------------------------------------------------------
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open OCSE396 Supplement Report
    // -----------------------------------------------------------------------------------------------
    local.NeededToOpen.BlankLineAfterHeading = "Y";
    local.NeededToOpen.RptHeading3 =
      "OCSE-396A SUPPLEMENT - CASES QUALIFYING FOR THE DRA FEE";
    UseCabBusinessReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening OCSE396a Supplement report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Get the DB2 commit frequency counts and determine if we are restarting.
    // -----------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmChkpntRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -----------------------------------------------------------------------------------------------
      // Extract checkpoint/restart info...
      //   Fiscal Year			columns  1 -  4
      //   Threshold Date      	 	columns  5 - 14
      //   Obligee CSP Number		columns 15 - 24
      //   Prev FY Qtr 1 Count		columns 25 - 30
      //   Prev FY Qtr 2 Count		columns 31 - 36
      //   Prev FY Qtr 3 Count		columns 37 - 42
      //   Prev FY Qtr 4 Count		columns 43 - 48
      //   Curr FY Qtr 1 Count		columns 49 - 54
      //   Curr FY Qtr 2 Count		columns 55 - 61
      //   Curr FY Qtr 3 Count		columns 62 - 67
      //   Curr FY Qtr 4 Count		columns 68 - 73
      // -----------------------------------------------------------------------------------------------
      local.Restart.FiscalYear =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 4));
      local.Restart.ThresholdDate =
        StringToDate(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 5, 10));
      local.RestartObligee.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 15, 10);

      // -- Load the saved counts for each quarter.
      for(local.Common.Count = 1; local.Common.Count <= 8; ++local.Common.Count)
      {
        local.Local1.Index = local.Common.Count - 1;
        local.Local1.CheckSize();

        local.Local1.Update.GlocalQuarterCount.TotalInteger =
          StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250,
          (int)((local.Common.Count - 1) * (long)6 + 25), 6));
      }

      // -- Write the restart info to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.NeededToWrite.RptDetail =
              "Process restarting from: Fiscal Year " + NumberToString
              (local.Restart.FiscalYear, 12, 4);

            break;
          case 2:
            local.NeededToWrite.RptDetail =
              "                         Threshold Date " + NumberToString
              (Year(local.Restart.ThresholdDate), 12, 4) + "-" + NumberToString
              (Month(local.Restart.ThresholdDate), 14, 2) + "-" + NumberToString
              (Day(local.Restart.ThresholdDate), 14, 2);

            break;
          case 3:
            local.NeededToWrite.RptDetail =
              "                         CSP Number " + local
              .RestartObligee.Number;

            break;
          case 4:
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing Restart Info to control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // -- Determine the start and end dates of the current and previous fiscal 
    // years.
    // -----------------------------------------------------------------------------------------------
    local.CurrentFyEnd.Date =
      StringToDate(NumberToString(
        Year(local.ProgramProcessingInfo.ProcessDate), 12, 4) + "-09-30");
    local.CurrentFyStart.Date =
      AddYears(AddDays(local.CurrentFyEnd.Date, 1), -1);
    local.CurrentFy.Year = Year(local.CurrentFyEnd.Date);
    local.PreviousFyEnd.Date = AddYears(local.CurrentFyEnd.Date, -1);
    local.PreviousFyStart.Date = AddYears(local.CurrentFyStart.Date, -1);
    local.PreviousFy.Year = local.CurrentFy.Year - 1;

    // -----------------------------------------------------------------------------------------------
    // -- Determine the start and end dates of each quarter during the current 
    // and previous fiscal years.
    // -----------------------------------------------------------------------------------------------
    for(local.Local1.Index = 0; local.Local1.Index < 8; ++local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      local.Local1.Update.GlocalQuarterStart.Date =
        AddMonths(local.PreviousFyStart.Date, local.Local1.Index * 3);
      local.Local1.Update.GlocalQuarterEnd.Date =
        AddDays(AddMonths(local.Local1.Item.GlocalQuarterStart.Date, 3), -1);

      if (local.Local1.Index < 4)
      {
        local.Local1.Update.GlocalQuarterTextString.Text8 = "FY" + NumberToString
          (Year(local.PreviousFyEnd.Date), 12, 4) + "Q" + NumberToString
          (local.Local1.Index + 1, 15, 1);
      }
      else
      {
        local.Local1.Update.GlocalQuarterTextString.Text8 = "FY" + NumberToString
          (Year(local.CurrentFyEnd.Date), 12, 4) + "Q" + NumberToString
          (local.Local1.Index - 3, 15, 1);
      }
    }

    local.Local1.CheckIndex();

    // -----------------------------------------------------------------------------------------------
    // Extract parameters from the PPI record.
    // -----------------------------------------------------------------------------------------------
    if (Find(local.ProgramProcessingInfo.ParameterList, "DISPLAY") == 0)
    {
    }
    else
    {
      local.Display.Flag = "Y";
      local.Local1.Index = -1;

      // -- Write parameter info to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 15; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.NeededToWrite.RptDetail =
              "DISPLAY (Display Disbursement Summary Detail) was specified on the PPI record.";
              

            break;
          case 2:
            local.NeededToWrite.RptDetail = "";

            break;
          case 3:
            local.NeededToWrite.RptDetail = "     Previous Fiscal Year " + NumberToString
              (local.PreviousFy.Year, 12, 4);
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + "  Start Date " + NumberToString
              (Year(local.PreviousFyStart.Date), 12, 4) + "-" + NumberToString
              (Month(local.PreviousFyStart.Date), 14, 2) + "-" + NumberToString
              (Day(local.PreviousFyStart.Date), 14, 2);
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + "  End Date " + NumberToString
              (Year(local.PreviousFyEnd.Date), 12, 4) + "-" + NumberToString
              (Month(local.PreviousFyEnd.Date), 14, 2) + "-" + NumberToString
              (Day(local.PreviousFyEnd.Date), 14, 2);

            break;
          case 8:
            local.NeededToWrite.RptDetail = "";

            break;
          case 9:
            local.NeededToWrite.RptDetail = "      Current Fiscal Year " + NumberToString
              (local.CurrentFy.Year, 12, 4);
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + "  Start Date " + NumberToString
              (Year(local.CurrentFyStart.Date), 12, 4) + "-" + NumberToString
              (Month(local.CurrentFyStart.Date), 14, 2) + "-" + NumberToString
              (Day(local.CurrentFyStart.Date), 14, 2);
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + "  End Date " + NumberToString
              (Year(local.CurrentFyEnd.Date), 12, 4) + "-" + NumberToString
              (Month(local.CurrentFyEnd.Date), 14, 2) + "-" + NumberToString
              (Day(local.CurrentFyEnd.Date), 14, 2);

            break;
          case 14:
            local.NeededToWrite.RptDetail = "";

            break;
          case 15:
            local.NeededToWrite.RptDetail = "";

            break;
          default:
            ++local.Local1.Index;
            local.Local1.CheckSize();

            local.NeededToWrite.RptDetail = "                      " + local
              .Local1.Item.GlocalQuarterTextString.Text8;
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + "  Start Date " + NumberToString
              (Year(local.Local1.Item.GlocalQuarterStart.Date), 12, 4) + "-" + NumberToString
              (Month(local.Local1.Item.GlocalQuarterStart.Date), 14, 2) + "-"
              + NumberToString
              (Day(local.Local1.Item.GlocalQuarterStart.Date), 14, 2);
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + "  End Date " + NumberToString
              (Year(local.Local1.Item.GlocalQuarterEnd.Date), 12, 4) + "-" + NumberToString
              (Month(local.Local1.Item.GlocalQuarterEnd.Date), 14, 2) + "-" + NumberToString
              (Day(local.Local1.Item.GlocalQuarterEnd.Date), 14, 2);

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing parameter info to control report.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Changed hard coded fee amount to parmeter drive. CQ61451  02/07/2018
    // -----------------------------------------------------------------------------------------------
    local.FeeText.Text3 =
      Substring(local.ProgramProcessingInfo.ParameterList, 9, 3);

    if (Verify(local.FeeText.Text3, " 0123456789") == 0)
    {
      local.FeeAmount.Count = (int)StringToNumber(local.FeeText.Text3);
    }
    else
    {
      local.FeeAmount.Count = 35;
    }

    // -----------------------------------------------------------------------------------------------
    // Read each disbursement summary for the current and previous fiscal year 
    // with a threshold date set.
    // -----------------------------------------------------------------------------------------------
    foreach(var item in ReadDisbursementSummaryCsePersonCsePerson())
    {
      ++local.NumberOfReads.Count;
      ++local.TotalNumberRecordsRead.Count;

      // -- Determine quarter during which the threshold date falls and 
      // increment the appropriate counter.
      local.Local1.Index = 0;

      for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (!Lt(entities.DisbursementSummary.ThresholdDate,
          local.Local1.Item.GlocalQuarterStart.Date) && !
          Lt(local.Local1.Item.GlocalQuarterEnd.Date,
          entities.DisbursementSummary.ThresholdDate))
        {
          local.Local1.Update.GlocalQuarterCount.TotalInteger =
            local.Local1.Item.GlocalQuarterCount.TotalInteger + 1;

          if (AsChar(local.Display.Flag) == 'Y')
          {
            // -- Log the obligee number, obligor number, the quarter in which 
            // they were counted, and the threshold date to the control report.
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "Obligee " + entities
              .Obligee2.Number + "    Obligor " + entities.Obligor2.Number + "   " +
              local.Local1.Item.GlocalQuarterTextString.Text8 + " " + NumberToString
              (Year(entities.DisbursementSummary.ThresholdDate), 12, 4) + "-"
              + NumberToString
              (Month(entities.DisbursementSummary.ThresholdDate), 14, 2) + "-"
              + NumberToString
              (Day(entities.DisbursementSummary.ThresholdDate), 14, 2);
            UseCabControlReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "Error encountered writing Disbursement Summary info to control report.";
                
              UseCabErrorReport2();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          break;
        }
      }

      local.Local1.CheckIndex();

      // --  Check for commit point.
      if (local.NumberOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // --  Update program checkpoint restart with current commit position.
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -----------------------------------------------------------------------------------------------
        // Save checkpoint/restart info...
        //   Fiscal Year			columns  1 -  4
        //   Threshold Date      	 	columns  5 - 14
        //   Obligee CSP Number		columns 15 - 24
        //   Prev FY Qtr 1 Count		columns 25 - 30
        //   Prev FY Qtr 2 Count		columns 31 - 36
        //   Prev FY Qtr 3 Count		columns 37 - 42
        //   Prev FY Qtr 4 Count		columns 43 - 48
        //   Curr FY Qtr 1 Count		columns 49 - 54
        //   Curr FY Qtr 2 Count		columns 55 - 61
        //   Curr FY Qtr 3 Count		columns 62 - 67
        //   Curr FY Qtr 4 Count		columns 68 - 73
        // -----------------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(entities.DisbursementSummary.FiscalYear, 12, 4);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (Year(entities.DisbursementSummary.ThresholdDate), 12, 4) + "-" + NumberToString
          (Month(entities.DisbursementSummary.ThresholdDate), 14, 2) + "-" + NumberToString
          (Day(entities.DisbursementSummary.ThresholdDate), 14, 2);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + entities
          .Obligee2.Number;

        for(local.Local1.Index = 0; local.Local1.Index < 8; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (local.Local1.Item.GlocalQuarterCount.TotalInteger, 10, 6);
        }

        local.Local1.CheckIndex();
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Update of checkpoint restart failed at person number -   " + entities
            .Obligee2.Number;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Commit Failed at person number -   " + entities.Obligee2.Number;
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NumberOfReads.Count = 0;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // -- Create the OCSE396a supplement report.
    // -----------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 13; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.NeededToWrite.RptDetail = "";

          break;
        case 2:
          local.NeededToWrite.RptDetail =
            "            ---------------------   CURRENT FISCAL YEAR  (";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
            (local.CurrentFy.Year, 12, 4);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + ")   --------------------";
            

          break;
        case 3:
          local.NeededToWrite.RptDetail =
            "                1ST QTR        2ND QTR        3RD QTR        4TH QTR          TOTAL";
            

          break;
        case 4:
          local.NeededToWrite.RptDetail = " # CASES";
          local.TotalForFiscalYear.TotalInteger = 0;

          for(local.Local2.Count = 1; local.Local2.Count <= 5; ++
            local.Local2.Count)
          {
            if (local.Local2.Count == 5)
            {
              local.AmountForReport.TotalInteger =
                local.TotalForFiscalYear.TotalInteger;
            }
            else
            {
              local.Local1.Index = local.Local2.Count + 3;
              local.Local1.CheckSize();

              local.AmountForReport.TotalInteger =
                local.Local1.Item.GlocalQuarterCount.TotalInteger;
              local.TotalForFiscalYear.TotalInteger += local.AmountForReport.
                TotalInteger;
            }

            if (local.AmountForReport.TotalInteger == 0)
            {
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "              0";
            }
            else
            {
              local.TextForReport.Text8 =
                NumberToString(local.AmountForReport.TotalInteger, 8, 8);

              // -- Find first non zero character in the string.
              local.Local3.Count = Verify(local.TextForReport.Text8, "0");

              // -- Remove leading zeros.
              local.TextForReport.Text8 =
                Substring(local.Blank.Text8, TextWorkArea.Text8_MaxLength, 1,
                local.Local3.Count - 1) + Substring
                (local.TextForReport.Text8, TextWorkArea.Text8_MaxLength,
                local.Local3.Count, 8 - local.Local3.Count + 1);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "       " + local
                .TextForReport.Text8;
            }
          }

          break;
        case 5:
          local.NeededToWrite.RptDetail = " $ AMOUNT";
          local.TotalForFiscalYear.TotalInteger = 0;

          for(local.Local2.Count = 1; local.Local2.Count <= 5; ++
            local.Local2.Count)
          {
            if (local.Local2.Count == 5)
            {
              local.AmountForReport.TotalInteger =
                local.TotalForFiscalYear.TotalInteger;
            }
            else
            {
              local.Local1.Index = local.Local2.Count + 3;
              local.Local1.CheckSize();

              local.AmountForReport.TotalInteger =
                (long)((decimal)local.Local1.Item.GlocalQuarterCount.
                  TotalInteger * local.FeeAmount.Count);
              local.TotalForFiscalYear.TotalInteger += local.AmountForReport.
                TotalInteger;
            }

            if (local.AmountForReport.TotalInteger == 0)
            {
              if (local.Local2.Count == 1)
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "            $0";
              }
              else
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "             $0";
              }
            }
            else
            {
              local.TextForReport.Text8 =
                NumberToString(local.AmountForReport.TotalInteger, 8, 8);

              // -- Find first non zero character in the string.
              local.Local3.Count = Verify(local.TextForReport.Text8, "0");

              // -- Remove leading zeros and add "$".
              local.TextForReport.Text8 =
                Substring(local.Blank.Text8, TextWorkArea.Text8_MaxLength, 1,
                local.Local3.Count - 2) + "$" + Substring
                (local.TextForReport.Text8, TextWorkArea.Text8_MaxLength,
                local.Local3.Count, 8 - local.Local3.Count + 1);

              if (local.Local2.Count == 1)
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "   " + local
                  .TextForReport.Text8 + ".00";
              }
              else
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "    " + local
                  .TextForReport.Text8 + ".00";
              }
            }
          }

          break;
        case 6:
          local.NeededToWrite.RptDetail = "";

          break;
        case 7:
          local.NeededToWrite.RptDetail = "";

          break;
        case 8:
          local.NeededToWrite.RptDetail = "";

          break;
        case 9:
          local.NeededToWrite.RptDetail =
            "            --------------------   PREVIOUS FISCAL YEAR  (";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + NumberToString
            (local.PreviousFy.Year, 12, 4);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + ")   --------------------";
            

          break;
        case 10:
          local.NeededToWrite.RptDetail =
            "                1ST QTR        2ND QTR        3RD QTR        4TH QTR          TOTAL";
            

          break;
        case 11:
          local.NeededToWrite.RptDetail = " # CASES";
          local.TotalForFiscalYear.TotalInteger = 0;

          for(local.Local2.Count = 1; local.Local2.Count <= 5; ++
            local.Local2.Count)
          {
            if (local.Local2.Count == 5)
            {
              local.AmountForReport.TotalInteger =
                local.TotalForFiscalYear.TotalInteger;
            }
            else
            {
              local.Local1.Index = local.Local2.Count - 1;
              local.Local1.CheckSize();

              local.AmountForReport.TotalInteger =
                local.Local1.Item.GlocalQuarterCount.TotalInteger;
              local.TotalForFiscalYear.TotalInteger += local.AmountForReport.
                TotalInteger;
            }

            if (local.AmountForReport.TotalInteger == 0)
            {
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "              0";
            }
            else
            {
              local.TextForReport.Text8 =
                NumberToString(local.AmountForReport.TotalInteger, 8, 8);

              // -- Find first non zero character in the string.
              local.Local3.Count = Verify(local.TextForReport.Text8, "0");

              // -- Remove leading zeros.
              local.TextForReport.Text8 =
                Substring(local.Blank.Text8, TextWorkArea.Text8_MaxLength, 1,
                local.Local3.Count - 1) + Substring
                (local.TextForReport.Text8, TextWorkArea.Text8_MaxLength,
                local.Local3.Count, 8 - local.Local3.Count + 1);
              local.NeededToWrite.RptDetail =
                TrimEnd(local.NeededToWrite.RptDetail) + "       " + local
                .TextForReport.Text8;
            }
          }

          break;
        case 12:
          local.NeededToWrite.RptDetail = " $ AMOUNT";
          local.TotalForFiscalYear.TotalInteger = 0;

          for(local.Local2.Count = 1; local.Local2.Count <= 5; ++
            local.Local2.Count)
          {
            if (local.Local2.Count == 5)
            {
              local.AmountForReport.TotalInteger =
                local.TotalForFiscalYear.TotalInteger;
            }
            else
            {
              local.Local1.Index = local.Local2.Count - 1;
              local.Local1.CheckSize();

              local.AmountForReport.TotalInteger =
                (long)((decimal)local.Local1.Item.GlocalQuarterCount.
                  TotalInteger * local.FeeAmount.Count);
              local.TotalForFiscalYear.TotalInteger += local.AmountForReport.
                TotalInteger;
            }

            if (local.AmountForReport.TotalInteger == 0)
            {
              if (local.Local2.Count == 1)
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "            $0";
              }
              else
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "             $0";
              }
            }
            else
            {
              local.TextForReport.Text8 =
                NumberToString(local.AmountForReport.TotalInteger, 8, 8);

              // -- Find first non zero character in the string.
              local.Local3.Count = Verify(local.TextForReport.Text8, "0");

              // -- Remove leading zeros and add "$".
              local.TextForReport.Text8 =
                Substring(local.Blank.Text8, TextWorkArea.Text8_MaxLength, 1,
                local.Local3.Count - 2) + "$" + Substring
                (local.TextForReport.Text8, TextWorkArea.Text8_MaxLength,
                local.Local3.Count, 8 - local.Local3.Count + 1);

              if (local.Local2.Count == 1)
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "   " + local
                  .TextForReport.Text8 + ".00";
              }
              else
              {
                local.NeededToWrite.RptDetail =
                  TrimEnd(local.NeededToWrite.RptDetail) + "    " + local
                  .TextForReport.Text8 + ".00";
              }
            }
          }

          break;
        default:
          local.NeededToWrite.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing OCSE396 Supplement Report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Log the total number of Disbursement Summary records read to the control 
    // report.
    // -----------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.NeededToWrite.RptDetail = "";

          break;
        case 2:
          local.NeededToWrite.RptDetail =
            "Total Number of Disbursement Summary records read - " + NumberToString
            (local.TotalNumberRecordsRead.Count, 6, 10);

          break;
        case 3:
          local.NeededToWrite.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing Totals to control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Take a final checkpoint.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.CheckpointCount = -1;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Final update of checkpoint restart table failed.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close OCSE396a Supplement Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered while closing OCSE396a Supplement report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Control Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Close Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
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
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

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
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.NeededToOpen, useImport.NeededToOpen);

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
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadDisbursementSummaryCsePersonCsePerson()
  {
    entities.Obligor2.Populated = false;
    entities.DisbursementSummary.Populated = false;
    entities.Obligee2.Populated = false;

    return ReadEach("ReadDisbursementSummaryCsePersonCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "fiscalYear1", local.CurrentFy.Year);
        db.SetDate(
          command, "date1", local.CurrentFyStart.Date.GetValueOrDefault());
        db.
          SetDate(command, "date2", local.CurrentFyEnd.Date.GetValueOrDefault());
          
        db.SetInt32(command, "fiscalYear2", local.PreviousFy.Year);
        db.SetDate(
          command, "date3", local.PreviousFyStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date4", local.PreviousFyEnd.Date.GetValueOrDefault());
        db.SetInt32(command, "fiscalYear3", local.Restart.FiscalYear);
        db.SetNullableDate(
          command, "thresholdDate",
          local.Restart.ThresholdDate.GetValueOrDefault());
        db.SetString(command, "cspNumberOblgee", local.RestartObligee.Number);
      },
      (db, reader) =>
      {
        entities.DisbursementSummary.FiscalYear = db.GetInt32(reader, 0);
        entities.DisbursementSummary.NonTafAmount =
          db.GetNullableDecimal(reader, 1);
        entities.DisbursementSummary.ThresholdDate =
          db.GetNullableDate(reader, 2);
        entities.DisbursementSummary.CspNumberOblgee = db.GetString(reader, 3);
        entities.Obligee2.Number = db.GetString(reader, 3);
        entities.DisbursementSummary.CpaTypeOblgee = db.GetString(reader, 4);
        entities.DisbursementSummary.CspNumberOblgr = db.GetString(reader, 5);
        entities.Obligor2.Number = db.GetString(reader, 5);
        entities.DisbursementSummary.CpaTypeOblgr = db.GetString(reader, 6);
        entities.Obligor2.Populated = true;
        entities.DisbursementSummary.Populated = true;
        entities.Obligee2.Populated = true;
        CheckValid<DisbursementSummary>("CpaTypeOblgee",
          entities.DisbursementSummary.CpaTypeOblgee);
        CheckValid<DisbursementSummary>("CpaTypeOblgr",
          entities.DisbursementSummary.CpaTypeOblgr);

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
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of GlocalQuarterCount.
      /// </summary>
      [JsonPropertyName("glocalQuarterCount")]
      public Common GlocalQuarterCount
      {
        get => glocalQuarterCount ??= new();
        set => glocalQuarterCount = value;
      }

      /// <summary>
      /// A value of GlocalQuarterEnd.
      /// </summary>
      [JsonPropertyName("glocalQuarterEnd")]
      public DateWorkArea GlocalQuarterEnd
      {
        get => glocalQuarterEnd ??= new();
        set => glocalQuarterEnd = value;
      }

      /// <summary>
      /// A value of GlocalQuarterStart.
      /// </summary>
      [JsonPropertyName("glocalQuarterStart")]
      public DateWorkArea GlocalQuarterStart
      {
        get => glocalQuarterStart ??= new();
        set => glocalQuarterStart = value;
      }

      /// <summary>
      /// A value of GlocalQuarterTextString.
      /// </summary>
      [JsonPropertyName("glocalQuarterTextString")]
      public TextWorkArea GlocalQuarterTextString
      {
        get => glocalQuarterTextString ??= new();
        set => glocalQuarterTextString = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private Common glocalQuarterCount;
      private DateWorkArea glocalQuarterEnd;
      private DateWorkArea glocalQuarterStart;
      private TextWorkArea glocalQuarterTextString;
    }

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public Common Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of FeeText.
    /// </summary>
    [JsonPropertyName("feeText")]
    public WorkArea FeeText
    {
      get => feeText ??= new();
      set => feeText = value;
    }

    /// <summary>
    /// A value of FeeAmount.
    /// </summary>
    [JsonPropertyName("feeAmount")]
    public Common FeeAmount
    {
      get => feeAmount ??= new();
      set => feeAmount = value;
    }

    /// <summary>
    /// A value of Local3.
    /// </summary>
    [JsonPropertyName("local3")]
    public Common Local3
    {
      get => local3 ??= new();
      set => local3 = value;
    }

    /// <summary>
    /// A value of TotalForFiscalYear.
    /// </summary>
    [JsonPropertyName("totalForFiscalYear")]
    public Common TotalForFiscalYear
    {
      get => totalForFiscalYear ??= new();
      set => totalForFiscalYear = value;
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
    /// A value of Local2.
    /// </summary>
    [JsonPropertyName("local2")]
    public Common Local2
    {
      get => local2 ??= new();
      set => local2 = value;
    }

    /// <summary>
    /// A value of AmountForReport.
    /// </summary>
    [JsonPropertyName("amountForReport")]
    public Common AmountForReport
    {
      get => amountForReport ??= new();
      set => amountForReport = value;
    }

    /// <summary>
    /// A value of TextForReport.
    /// </summary>
    [JsonPropertyName("textForReport")]
    public TextWorkArea TextForReport
    {
      get => textForReport ??= new();
      set => textForReport = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public TextWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of RestartObligee.
    /// </summary>
    [JsonPropertyName("restartObligee")]
    public CsePerson RestartObligee
    {
      get => restartObligee ??= new();
      set => restartObligee = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public DisbursementSummary Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of PreviousFy.
    /// </summary>
    [JsonPropertyName("previousFy")]
    public DateWorkArea PreviousFy
    {
      get => previousFy ??= new();
      set => previousFy = value;
    }

    /// <summary>
    /// A value of CurrentFy.
    /// </summary>
    [JsonPropertyName("currentFy")]
    public DateWorkArea CurrentFy
    {
      get => currentFy ??= new();
      set => currentFy = value;
    }

    /// <summary>
    /// A value of CurrentFyStart.
    /// </summary>
    [JsonPropertyName("currentFyStart")]
    public DateWorkArea CurrentFyStart
    {
      get => currentFyStart ??= new();
      set => currentFyStart = value;
    }

    /// <summary>
    /// A value of CurrentFyEnd.
    /// </summary>
    [JsonPropertyName("currentFyEnd")]
    public DateWorkArea CurrentFyEnd
    {
      get => currentFyEnd ??= new();
      set => currentFyEnd = value;
    }

    /// <summary>
    /// A value of PreviousFyStart.
    /// </summary>
    [JsonPropertyName("previousFyStart")]
    public DateWorkArea PreviousFyStart
    {
      get => previousFyStart ??= new();
      set => previousFyStart = value;
    }

    /// <summary>
    /// A value of PreviousFyEnd.
    /// </summary>
    [JsonPropertyName("previousFyEnd")]
    public DateWorkArea PreviousFyEnd
    {
      get => previousFyEnd ??= new();
      set => previousFyEnd = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of TotalNumberRecordsRead.
    /// </summary>
    [JsonPropertyName("totalNumberRecordsRead")]
    public Common TotalNumberRecordsRead
    {
      get => totalNumberRecordsRead ??= new();
      set => totalNumberRecordsRead = value;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
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

    private Common verify;
    private WorkArea feeText;
    private Common feeAmount;
    private Common local3;
    private Common totalForFiscalYear;
    private Common common;
    private Common local2;
    private Common amountForReport;
    private TextWorkArea textForReport;
    private TextWorkArea blank;
    private CsePerson restartObligee;
    private DisbursementSummary restart;
    private DateWorkArea previousFy;
    private DateWorkArea currentFy;
    private DateWorkArea currentFyStart;
    private DateWorkArea currentFyEnd;
    private DateWorkArea previousFyStart;
    private DateWorkArea previousFyEnd;
    private Array<LocalGroup> local1;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common numberOfReads;
    private Common totalNumberRecordsRead;
    private Common display;
    private External passArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of DisbursementSummary.
    /// </summary>
    [JsonPropertyName("disbursementSummary")]
    public DisbursementSummary DisbursementSummary
    {
      get => disbursementSummary ??= new();
      set => disbursementSummary = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    private CsePersonAccount obligee1;
    private CsePersonAccount obligor1;
    private CsePerson obligor2;
    private DisbursementSummary disbursementSummary;
    private CsePerson obligee2;
  }
#endregion
}
