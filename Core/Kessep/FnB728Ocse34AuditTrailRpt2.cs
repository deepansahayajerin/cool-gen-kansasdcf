// Program: FN_B728_OCSE34_AUDIT_TRAIL_RPT_2, ID: 945039019, model: 746.
// Short name: SWEF728B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B728_OCSE34_AUDIT_TRAIL_RPT_2.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB728Ocse34AuditTrailRpt2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B728_OCSE34_AUDIT_TRAIL_RPT_2 program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB728Ocse34AuditTrailRpt2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB728Ocse34AuditTrailRpt2.
  /// </summary>
  public FnB728Ocse34AuditTrailRpt2(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 11/05/10  GVandy	CQ23186		Initial Development
    // -----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------------
    // -- Read the PPI record.
    // -------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEFB727";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------------------------------
    // -- Open Error Report.
    // --------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProgramName = "SWEFB727";
    local.Open.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // --------------------------------------------------------------------------------------------
    // -- Open Control Report.
    // --------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // --------------------------------------------------------------------------------------------
    // -- Open Business Report 1.
    // --------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabBusinessReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ZD_FILE_OPEN_ERROR_WITH_AB";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // --------------------------------------------------------------------------------------------
    // -- PPI Info...
    // --	Positions	Description
    // --	---------	
    // ------------------------------------------
    // --	 1 to 6   	OCSE34 Reporting Period (YYYYQQ)
    // --	 8 to 19	Starting Cash Receipt Detail ID
    // --	21 to 32	Ending Cash Receipt Detail ID
    // --	34       	Write all audit data flag
    // --	36       	Intelligent balance flag
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- Determine the OCSE34 reporting period.
    // --------------------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 6)))
    {
      // -- Reporting period was not specified on the PPI record.
      // -- Default to the most recent quarter which ended prior to the PPI 
      // date.
      if (Month(local.ProgramProcessingInfo.ProcessDate) >= 10)
      {
        local.Ocse34.Period = Year(local.ProgramProcessingInfo.ProcessDate) * 100
          + 3;
      }
      else if (Month(local.ProgramProcessingInfo.ProcessDate) >= 7)
      {
        local.Ocse34.Period = Year(local.ProgramProcessingInfo.ProcessDate) * 100
          + 2;
      }
      else if (Month(local.ProgramProcessingInfo.ProcessDate) >= 4)
      {
        local.Ocse34.Period = Year(local.ProgramProcessingInfo.ProcessDate) * 100
          + 1;
      }
      else
      {
        local.Ocse34.Period =
          (Year(local.ProgramProcessingInfo.ProcessDate) - 1) * 100 + 4;
      }
    }
    else
    {
      if (Verify(Substring(local.ProgramProcessingInfo.ParameterList, 1, 6),
        "0123456789") != 0)
      {
        local.EabReportSend.RptDetail =
          "Invalid OCSE34 report period.  PPI char 1-6 = " + Substring
          (local.ProgramProcessingInfo.ParameterList, 1, 6);
        UseCabErrorReport2();
        ExitState = "ACO_AE0000_BATCH_ABEND";

        return;
      }

      local.Ocse34.Period =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 6));
    }

    local.Ocse157Verification.FiscalYear =
      (int?)StringToNumber(NumberToString(local.Ocse34.Period, 10, 4));
    local.Ocse157Verification.RunNumber =
      (int?)StringToNumber(NumberToString(local.Ocse34.Period, 14, 2));

    // --------------------------------------------------------------------------------------------
    // -- Determine the starting and ending cash receipt detail ids.
    // --------------------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 8, 12)))
    {
      local.Starting.CaseWorkerName = "0000000-0000";
    }
    else
    {
      local.Starting.CaseWorkerName =
        Substring(local.ProgramProcessingInfo.ParameterList, 8, 12);
    }

    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 21, 12)))
    {
      local.Ending.CaseWorkerName = "9999999-9999";
    }
    else
    {
      local.Ending.CaseWorkerName =
        Substring(local.ProgramProcessingInfo.ParameterList, 21, 12);
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract flag signifying whether audit detail is to be written to the 
    // report for all cash
    // -- receipt details, whether they balance or not.
    // --------------------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 34, 1)))
    {
      local.WriteAllAuditInfo.Flag = "N";
    }
    else
    {
      local.WriteAllAuditInfo.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 34, 1);
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract flag signifying whether intelligent balancing will be done.
    // --------------------------------------------------------------------------------------------
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 36, 1)))
    {
      local.IntelligentBalance.Flag = "N";
    }
    else
    {
      local.IntelligentBalance.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 36, 1);
    }

    // --------------------------------------------------------------------------------------------
    // -- Log the parameters to the Control Report.
    // --------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 7; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "OCSE34 Reporting Period         = " + Substring
            (local.ProgramProcessingInfo.ParameterList, 1, 6);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Starting Cash Receipt Detail ID = " + (
              local.Starting.CaseWorkerName ?? "");

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "Ending Cash Receipt Detail ID   = " + (
              local.Ending.CaseWorkerName ?? "");

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "Write Audit Data for all CRDs   = " + local
            .WriteAllAuditInfo.Flag;

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Intelligent Balance             = " + local
            .IntelligentBalance.Flag;

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // --Determine the previous quarter
    if (local.Ocse157Verification.RunNumber.GetValueOrDefault() == 1)
    {
      local.PreviousQuarter.FiscalYear =
        local.Ocse157Verification.FiscalYear.GetValueOrDefault() - 1;
      local.PreviousQuarter.RunNumber = 4;
    }
    else
    {
      local.PreviousQuarter.FiscalYear =
        local.Ocse157Verification.FiscalYear.GetValueOrDefault();
      local.PreviousQuarter.RunNumber =
        local.Ocse157Verification.RunNumber.GetValueOrDefault() - 1;
    }

    // --------------------------------------------------------------------------------------------
    // -- Log message to the Business Report 1.
    // --------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      if (local.Common.Count == 3)
      {
        local.EabReportSend.RptDetail = "CRDs undistributed in quarter " + NumberToString
          (local.PreviousQuarter.FiscalYear.GetValueOrDefault(), 12, 4) + NumberToString
          (local.PreviousQuarter.RunNumber.GetValueOrDefault(), 14, 2) + " not balanced in quarter " +
          NumberToString
          (local.Ocse157Verification.FiscalYear.GetValueOrDefault(), 12, 4) + NumberToString
          (local.Ocse157Verification.RunNumber.GetValueOrDefault(), 14, 2) + "."
          ;
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Find the OCSE34 record for the reporting period.
    // --------------------------------------------------------------------------------------------
    if (!ReadOcse34())
    {
      local.EabReportSend.RptDetail =
        "OCSE34 record not found for report period = " + Substring
        (local.ProgramProcessingInfo.ParameterList, 1, 6);
      UseCabErrorReport2();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    local.Group.Index = -1;

    // --------------------------------------------------------------------------------------------
    // -- Read the audit records sorted by cash receipt detail number.  For each
    // cash receipt
    // -- detail received during the quarter the audit trail should contain a 
    // credit amount equal
    // -- to the sum of the corresponding debit amounts.
    // -- Note:  The cash receipt detail number is stored in the case worker 
    // name attribute.
    // --------------------------------------------------------------------------------------------
    foreach(var item in ReadOcse157Verification1())
    {
      if (Equal(entities.DistinctCrdId.CaseWorkerName,
        local.Previous.CaseWorkerName))
      {
        // -- For some unknown reason the generated code for the distinct read 
        // each above is including the ocse157_verification
        //    created_timestamp in the read each causing multiple occurrences of
        // the same CRD_ID (i.e. case_worker_name) to
        //    be returned.
        continue;
      }
      else
      {
        local.Previous.CaseWorkerName = entities.DistinctCrdId.CaseWorkerName;
      }

      local.BalanceCheck.TotalCurrency = 0;
      local.Group.Index = -1;
      local.Group.Count = 0;
      local.Line2EntryFound.Flag = "N";

      foreach(var item1 in ReadOcse157Verification3())
      {
        if (AsChar(local.IntelligentBalance.Flag) == 'Y' && AsChar
          (local.Line2EntryFound.Flag) == 'N')
        {
          // -- If intelligent balance flag is Y we will only attempt to 
          // balanace CRDs where we have a collection
          //    record (i.e. a line 2x entry) for the CRD in the audit table.  
          // Since we only retain about 8 quarters
          //    worth of audit records we might be dealing with a CRD that was 
          // received prior to our earliest
          //    audit record.  If we don't have the initial collection entry 
          // then the CRD will not balance.
          if (CharAt(entities.Ocse157Verification.LineNumber, 1) == '2' && Lt
            (0, entities.Ocse157Verification.CollectionAmount))
          {
            local.Line2EntryFound.Flag = "Y";
          }
          else if (!Lt(entities.Ocse157Verification.LineNumber, "3"))
          {
            // We don't have the initial collection record in the audit table 
            // therefore this CRD won't balance.  Skip this CRD.
            local.EabReportSend.RptDetail =
              "CRD skipped due to intelligent balance.  CRD # " + entities
              .Ocse157Verification.CaseWorkerName;
            UseCabErrorReport2();

            goto ReadEach;
          }
          else
          {
            // -- Continue.
            // -- (This would be a B13 entry or a negative line 2 amount.)
          }
        }

        switch(TrimEnd(Substring(entities.Ocse157Verification.LineNumber, 1, 1)))
          
        {
          case "2":
            // -- Amounts in line 2 are amounts received during the quarter.  
            // Add this amount to the running cash receipt detail total.
            local.BalanceCheck.TotalCurrency += entities.Ocse157Verification.
              CollectionAmount.GetValueOrDefault();

            break;
          case "9":
            // -- Skip any audit info for line 9.  This is old info and not used
            // anymore.  It should not be used to determine if the CRD
            // balances.
            continue;
          default:
            // -- Amounts in any line other than line 2 are amounts that we have
            // accounted for (i.e. sent to other states or
            //    countries, distributed as assistance reimbursement, 
            // distributed to family, currently in suspense, disbursement
            //    suppressed, etc.).  Subtract this amount from the running cash
            // receipt detail total.
            if (!Equal(entities.Ocse157Verification.LineNumber, "B13") || Equal
              (entities.Ocse157Verification.LineNumber, "B13") && Equal
              (entities.Ocse157Verification.FiscalYear,
              local.Ocse157Verification.FiscalYear.GetValueOrDefault()) && Equal
              (entities.Ocse157Verification.RunNumber,
              local.Ocse157Verification.RunNumber.GetValueOrDefault()))
            {
              // -- For line B13, only include the amount if the record is for 
              // the current quarter.
              local.BalanceCheck.TotalCurrency -= entities.Ocse157Verification.
                CollectionAmount.GetValueOrDefault();
            }

            break;
        }

        ++local.Group.Index;
        local.Group.CheckSize();

        if (local.Group.Index >= Local.GroupGroup.Capacity)
        {
          // -- group is full
          break;
        }

        local.Group.Update.G.Assign(entities.Ocse157Verification);
      }

      // -- Determine if the cash receipt detail balanced.
      if (local.BalanceCheck.TotalCurrency == 0)
      {
        // -- Cash receipt detail amount balanced.
        ++local.BalancedCrd.Count;
      }
      else
      {
        // -- Cash receipt detail amount did not balanced.  Write the cash 
        // receipt detail info to the report.
        ++local.NonBalancedCrd.Count;
      }

      local.GrandTotalUnbalanced.TotalCurrency += local.BalanceCheck.
        TotalCurrency;

      if (local.BalanceCheck.TotalCurrency != 0 || AsChar
        (local.WriteAllAuditInfo.Flag) == 'Y')
      {
        local.Group.Index = -1;
        local.Common.Count = 1;

        for(var limit = local.Group.Count + 6; local.Common.Count <= limit; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "";

              break;
            case 2:
              local.EabReportSend.RptDetail = "";

              break;
            case 3:
              local.EabReportSend.RptDetail =
                "CRD ID        QUARTER  LINE #  CSP NUMBER  DATE              AMOUNT   COMMENT";
                

              break;
            case 4:
              local.EabReportSend.RptDetail =
                "------------  -------  ------  ----------  -------- ---------------   ----------------------------------------";
                

              break;
            default:
              if (local.Group.Index + 1 < local.Group.Count)
              {
                ++local.Group.Index;
                local.Group.CheckSize();

                if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
                {
                  // -- group is full
                  local.EabReportSend.RptDetail =
                    "More than 4,000 audit records found for this CRD... balance difference will not be accurate!";
                    

                  break;
                }

                local.EabReportSend.RptDetail =
                  local.Group.Item.G.CaseWorkerName ?? Spaces(132);
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 14) + NumberToString
                  (local.Group.Item.G.FiscalYear.GetValueOrDefault(), 12, 4) + NumberToString
                  (local.Group.Item.G.RunNumber.GetValueOrDefault(), 14, 2);
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 23) + (
                    local.Group.Item.G.LineNumber ?? "");
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 31) + (
                    local.Group.Item.G.ObligorPersonNbr ?? "");
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 43) + NumberToString
                  (DateToInt(local.Group.Item.G.CollectionDte), 8, 8);
                local.TempNumericConversionCommon.Count =
                  (int)StringToNumber(NumberToString((long)local.Group.Item.G.
                    CollectionAmount.GetValueOrDefault(), 15));

                if (local.TempNumericConversionCommon.Count == 0)
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    "              0";
                }
                else
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    Substring("               ", 1,
                    Verify(NumberToString((long)local.Group.Item.G.
                      CollectionAmount.GetValueOrDefault(), 15), "0") - 1) + NumberToString
                    ((long)local.Group.Item.G.CollectionAmount.
                      GetValueOrDefault(),
                    Verify(NumberToString((long)local.Group.Item.G.
                      CollectionAmount.GetValueOrDefault(), 15), "0"), 15);
                }

                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 52) + Substring
                  (local.TempNumericConversionEabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 4, 12);
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + ".";
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                  ((long)((local.Group.Item.G.CollectionAmount.
                    GetValueOrDefault() - local
                  .TempNumericConversionCommon.Count) * 100), 14, 2);

                if (local.Group.Item.G.CollectionAmount.GetValueOrDefault() < 0)
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "-";
                }

                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 70) + (
                    local.Group.Item.G.Comment ?? "");
              }
              else if (local.Common.Count == local.Group.Count + 5)
              {
                local.EabReportSend.RptDetail =
                  "------------  -------  ------  ----------  -------- ---------------   ----------------------------------------";
                  
              }
              else
              {
                local.EabReportSend.RptDetail = "";
                local.TempNumericConversionCommon.Count =
                  (int)StringToNumber(NumberToString((long)local.BalanceCheck.
                    TotalCurrency, 15));

                if (local.TempNumericConversionCommon.Count == 0)
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    "              0";
                }
                else
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    Substring("               ", 1,
                    Verify(NumberToString((long)local.BalanceCheck.
                      TotalCurrency, 15), "0") - 1) + NumberToString
                    ((long)local.BalanceCheck.TotalCurrency,
                    Verify(NumberToString((long)local.BalanceCheck.
                      TotalCurrency, 15), "0"), 15);
                }

                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 52) + Substring
                  (local.TempNumericConversionEabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 4, 12);
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + ".";
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                  ((long)((local.BalanceCheck.TotalCurrency - local
                  .TempNumericConversionCommon.Count) * 100), 14, 2);

                if (local.BalanceCheck.TotalCurrency < 0)
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "-";
                }
              }

              break;
          }

          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }
        }
      }

ReadEach:
      ;
    }

    // --------------------------------------------------------------------------------------------
    // -- Log the number of balancing and non balancing cash receipt details to 
    // the Control Report.
    // --------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 2:
          local.EabReportSend.RptDetail = "Number of balanced CRDs       = " + NumberToString
            (local.BalancedCrd.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Number of non balanced CRDs   = " + NumberToString
            (local.NonBalancedCrd.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "Total non balanced CRD amount = " + NumberToString
            (local.NonBalancedCrd.Count, 15);
          local.TempNumericConversionCommon.Count =
            (int)StringToNumber(NumberToString((long)local.GrandTotalUnbalanced.
              TotalCurrency, 15));
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 33) + NumberToString
            ((long)local.GrandTotalUnbalanced.TotalCurrency, 4, 12);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ".";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            ((long)((local.GrandTotalUnbalanced.TotalCurrency - local
            .TempNumericConversionCommon.Count) * 100), 14, 2);

          if (local.GrandTotalUnbalanced.TotalCurrency < 0)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // @@@@@@@@@@@@@@@@@@@@@@@@@
    // New Below...
    // @@@@@@@@@@@@@@@@@@@@@@@@@
    // --------------------------------------------------------------------------------------------
    // -- NEW ENHANCEMENT BELOW  05/12/2011
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // -- Read for all cash receipt details that had an undistributed amount on 
    // the previous
    // -- quarter report.  We want to determine if the previous undistributed 
    // amount is accounted
    // -- for somewhere on the current quarter report.
    // --------------------------------------------------------------------------------------------
    local.BalancedCrd.Count = 0;
    local.NonBalancedCrd.Count = 0;
    local.GrandTotalUnbalanced.TotalCurrency = 0;
    local.Previous.CaseWorkerName = "";

    foreach(var item in ReadOcse157Verification2())
    {
      if (Equal(entities.PrevQuarterDistinctCrdId.CaseWorkerName,
        local.Previous.CaseWorkerName))
      {
        // -- For some unknown reason the generated code for the distinct read 
        // each above is including the ocse157_verification
        //    created_timestamp in the read each causing multiple occurrences of
        // the same CRD_ID (i.e. case_worker_name) to
        //    be returned.
        continue;
      }
      else
      {
        local.Previous.CaseWorkerName =
          entities.PrevQuarterDistinctCrdId.CaseWorkerName;
      }

      local.BalanceCheck.TotalCurrency = 0;
      local.Group.Index = -1;
      local.Group.Count = 0;
      local.Line2EntryFound.Flag = "N";

      foreach(var item1 in ReadOcse157Verification4())
      {
        if (CharAt(entities.Ocse157Verification.LineNumber, 1) == '9')
        {
          // -- Skip any Line 9 entries found.  This is old data and should not 
          // be used in balancing.
          continue;
        }

        if (Equal(entities.Ocse157Verification.FiscalYear,
          local.PreviousQuarter.FiscalYear.GetValueOrDefault()) && Equal
          (entities.Ocse157Verification.RunNumber,
          local.PreviousQuarter.RunNumber.GetValueOrDefault()) && Equal
          (entities.Ocse157Verification.LineNumber, "B13"))
        {
          local.BalanceCheck.TotalCurrency += entities.Ocse157Verification.
            CollectionAmount.GetValueOrDefault();
        }
        else if (Equal(entities.Ocse157Verification.FiscalYear,
          local.Ocse157Verification.FiscalYear.GetValueOrDefault()) && Equal
          (entities.Ocse157Verification.RunNumber,
          local.Ocse157Verification.RunNumber.GetValueOrDefault()))
        {
          if (CharAt(entities.Ocse157Verification.LineNumber, 1) == '2')
          {
            local.BalanceCheck.TotalCurrency += entities.Ocse157Verification.
              CollectionAmount.GetValueOrDefault();
          }
          else
          {
            local.BalanceCheck.TotalCurrency -= entities.Ocse157Verification.
              CollectionAmount.GetValueOrDefault();
          }
        }

        ++local.Group.Index;
        local.Group.CheckSize();

        if (local.Group.Index >= Local.GroupGroup.Capacity)
        {
          // -- group is full
          break;
        }

        local.Group.Update.G.Assign(entities.Ocse157Verification);
      }

      // -- Determine if the cash receipt detail balanced.
      if (local.BalanceCheck.TotalCurrency == 0)
      {
        // -- Cash receipt detail amount balanced.
        ++local.BalancedCrd.Count;
      }
      else
      {
        // -- Cash receipt detail amount did not balanced.  Write the cash 
        // receipt detail info to the report.
        ++local.NonBalancedCrd.Count;
      }

      local.GrandTotalUnbalanced.TotalCurrency += local.BalanceCheck.
        TotalCurrency;

      if (local.BalanceCheck.TotalCurrency != 0 || AsChar
        (local.WriteAllAuditInfo.Flag) == 'Y')
      {
        local.Group.Index = -1;
        local.Common.Count = 1;

        for(var limit = local.Group.Count + 6; local.Common.Count <= limit; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "";

              break;
            case 2:
              local.EabReportSend.RptDetail = "";

              break;
            case 3:
              local.EabReportSend.RptDetail =
                "CRD ID        QUARTER  LINE #  CSP NUMBER  DATE              AMOUNT   COMMENT";
                

              break;
            case 4:
              local.EabReportSend.RptDetail =
                "------------  -------  ------  ----------  -------- ---------------   ----------------------------------------";
                

              break;
            default:
              if (local.Group.Index + 1 < local.Group.Count)
              {
                ++local.Group.Index;
                local.Group.CheckSize();

                if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
                {
                  // -- group is full
                  local.EabReportSend.RptDetail =
                    "More than 4,000 audit records found for this CRD... balance difference will not be accurate!";
                    

                  break;
                }

                local.EabReportSend.RptDetail =
                  local.Group.Item.G.CaseWorkerName ?? Spaces(132);
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 14) + NumberToString
                  (local.Group.Item.G.FiscalYear.GetValueOrDefault(), 12, 4) + NumberToString
                  (local.Group.Item.G.RunNumber.GetValueOrDefault(), 14, 2);
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 23) + (
                    local.Group.Item.G.LineNumber ?? "");
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 31) + (
                    local.Group.Item.G.ObligorPersonNbr ?? "");
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 43) + NumberToString
                  (DateToInt(local.Group.Item.G.CollectionDte), 8, 8);
                local.TempNumericConversionCommon.Count =
                  (int)StringToNumber(NumberToString((long)local.Group.Item.G.
                    CollectionAmount.GetValueOrDefault(), 15));

                if (local.TempNumericConversionCommon.Count == 0)
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    "              0";
                }
                else
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    Substring("               ", 1,
                    Verify(NumberToString((long)local.Group.Item.G.
                      CollectionAmount.GetValueOrDefault(), 15), "0") - 1) + NumberToString
                    ((long)local.Group.Item.G.CollectionAmount.
                      GetValueOrDefault(),
                    Verify(NumberToString((long)local.Group.Item.G.
                      CollectionAmount.GetValueOrDefault(), 15), "0"), 15);
                }

                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 52) + Substring
                  (local.TempNumericConversionEabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 4, 12);
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + ".";
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                  ((long)((local.Group.Item.G.CollectionAmount.
                    GetValueOrDefault() - local
                  .TempNumericConversionCommon.Count) * 100), 14, 2);

                if (local.Group.Item.G.CollectionAmount.GetValueOrDefault() < 0)
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "-";
                }

                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 70) + (
                    local.Group.Item.G.Comment ?? "");
              }
              else if (local.Common.Count == local.Group.Count + 5)
              {
                local.EabReportSend.RptDetail =
                  "------------  -------  ------  ----------  -------- ---------------   ----------------------------------------";
                  
              }
              else
              {
                local.EabReportSend.RptDetail = "";
                local.TempNumericConversionCommon.Count =
                  (int)StringToNumber(NumberToString((long)local.BalanceCheck.
                    TotalCurrency, 15));

                if (local.TempNumericConversionCommon.Count == 0)
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    "              0";
                }
                else
                {
                  local.TempNumericConversionEabReportSend.RptDetail =
                    Substring("               ", 1,
                    Verify(NumberToString((long)local.BalanceCheck.
                      TotalCurrency, 15), "0") - 1) + NumberToString
                    ((long)local.BalanceCheck.TotalCurrency,
                    Verify(NumberToString((long)local.BalanceCheck.
                      TotalCurrency, 15), "0"), 15);
                }

                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1, 52) + Substring
                  (local.TempNumericConversionEabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 4, 12);
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + ".";
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                  ((long)((local.BalanceCheck.TotalCurrency - local
                  .TempNumericConversionCommon.Count) * 100), 14, 2);

                if (local.BalanceCheck.TotalCurrency < 0)
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "-";
                }
              }

              break;
          }

          UseCabBusinessReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Log the number of balancing and non balancing cash receipt details to 
    // the Control Report.
    // --------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 2:
          local.EabReportSend.RptDetail = "Number of balanced CRDs       = " + NumberToString
            (local.BalancedCrd.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Number of non balanced CRDs   = " + NumberToString
            (local.NonBalancedCrd.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "Total non balanced CRD amount = " + NumberToString
            (local.NonBalancedCrd.Count, 15);
          local.TempNumericConversionCommon.Count =
            (int)StringToNumber(NumberToString((long)local.GrandTotalUnbalanced.
              TotalCurrency, 15));
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 33) + NumberToString
            ((long)local.GrandTotalUnbalanced.TotalCurrency, 4, 12);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ".";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            ((long)((local.GrandTotalUnbalanced.TotalCurrency - local
            .TempNumericConversionCommon.Count) * 100), 14, 2);

          if (local.GrandTotalUnbalanced.TotalCurrency < 0)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Business report 1.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // @@@@@@@@@@@@@@@@@@@@@@@@@
    // New Above...
    // @@@@@@@@@@@@@@@@@@@@@@@@@
    // -------------------------------------------------------------------------------------------
    // -- Close the Control report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Error report.
    // -------------------------------------------------------------------------------------------
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

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
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

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

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private IEnumerable<bool> ReadOcse157Verification1()
  {
    entities.DistinctCrdId.Populated = false;

    return ReadEach("ReadOcse157Verification1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseWorkerName1", local.Starting.CaseWorkerName ?? "");
        db.SetNullableString(
          command, "caseWorkerName2", local.Ending.CaseWorkerName ?? "");
        db.SetNullableInt32(
          command, "fiscalYear",
          local.Ocse157Verification.FiscalYear.GetValueOrDefault());
        db.SetNullableInt32(
          command, "runNumber",
          local.Ocse157Verification.RunNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DistinctCrdId.FiscalYear = db.GetNullableInt32(reader, 0);
        entities.DistinctCrdId.RunNumber = db.GetNullableInt32(reader, 1);
        entities.DistinctCrdId.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.DistinctCrdId.CaseWorkerName = db.GetNullableString(reader, 3);
        entities.DistinctCrdId.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOcse157Verification2()
  {
    entities.PrevQuarterDistinctCrdId.Populated = false;

    return ReadEach("ReadOcse157Verification2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          local.PreviousQuarter.FiscalYear.GetValueOrDefault());
        db.SetNullableInt32(
          command, "runNumber",
          local.PreviousQuarter.RunNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PrevQuarterDistinctCrdId.FiscalYear =
          db.GetNullableInt32(reader, 0);
        entities.PrevQuarterDistinctCrdId.RunNumber =
          db.GetNullableInt32(reader, 1);
        entities.PrevQuarterDistinctCrdId.LineNumber =
          db.GetNullableString(reader, 2);
        entities.PrevQuarterDistinctCrdId.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.PrevQuarterDistinctCrdId.CaseWorkerName =
          db.GetNullableString(reader, 4);
        entities.PrevQuarterDistinctCrdId.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOcse157Verification3()
  {
    entities.Ocse157Verification.Populated = false;

    return ReadEach("ReadOcse157Verification3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseWorkerName", entities.DistinctCrdId.CaseWorkerName ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Ocse157Verification.FiscalYear =
          db.GetNullableInt32(reader, 0);
        entities.Ocse157Verification.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Verification.LineNumber =
          db.GetNullableString(reader, 2);
        entities.Ocse157Verification.Column = db.GetNullableString(reader, 3);
        entities.Ocse157Verification.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.Ocse157Verification.ObligorPersonNbr =
          db.GetNullableString(reader, 5);
        entities.Ocse157Verification.CollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.Ocse157Verification.CollectionDte =
          db.GetNullableDate(reader, 7);
        entities.Ocse157Verification.CaseWorkerName =
          db.GetNullableString(reader, 8);
        entities.Ocse157Verification.Comment = db.GetNullableString(reader, 9);
        entities.Ocse157Verification.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOcse157Verification4()
  {
    entities.Ocse157Verification.Populated = false;

    return ReadEach("ReadOcse157Verification4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "caseWorkerName",
          entities.PrevQuarterDistinctCrdId.CaseWorkerName ?? "");
      },
      (db, reader) =>
      {
        entities.Ocse157Verification.FiscalYear =
          db.GetNullableInt32(reader, 0);
        entities.Ocse157Verification.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Verification.LineNumber =
          db.GetNullableString(reader, 2);
        entities.Ocse157Verification.Column = db.GetNullableString(reader, 3);
        entities.Ocse157Verification.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.Ocse157Verification.ObligorPersonNbr =
          db.GetNullableString(reader, 5);
        entities.Ocse157Verification.CollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.Ocse157Verification.CollectionDte =
          db.GetNullableDate(reader, 7);
        entities.Ocse157Verification.CaseWorkerName =
          db.GetNullableString(reader, 8);
        entities.Ocse157Verification.Comment = db.GetNullableString(reader, 9);
        entities.Ocse157Verification.Populated = true;

        return true;
      });
  }

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetInt32(command, "period", local.Ocse34.Period);
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Ocse34.Populated = true;
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
      public Ocse157Verification G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4000;

      private Ocse157Verification g;
    }

    /// <summary>
    /// A value of Line2EntryFound.
    /// </summary>
    [JsonPropertyName("line2EntryFound")]
    public Common Line2EntryFound
    {
      get => line2EntryFound ??= new();
      set => line2EntryFound = value;
    }

    /// <summary>
    /// A value of PreviousQuarter.
    /// </summary>
    [JsonPropertyName("previousQuarter")]
    public Ocse157Verification PreviousQuarter
    {
      get => previousQuarter ??= new();
      set => previousQuarter = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Ocse157Verification Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of TbdLocalFirstAuditRecord.
    /// </summary>
    [JsonPropertyName("tbdLocalFirstAuditRecord")]
    public Common TbdLocalFirstAuditRecord
    {
      get => tbdLocalFirstAuditRecord ??= new();
      set => tbdLocalFirstAuditRecord = value;
    }

    /// <summary>
    /// A value of IntelligentBalance.
    /// </summary>
    [JsonPropertyName("intelligentBalance")]
    public Common IntelligentBalance
    {
      get => intelligentBalance ??= new();
      set => intelligentBalance = value;
    }

    /// <summary>
    /// A value of TempNumericConversionEabReportSend.
    /// </summary>
    [JsonPropertyName("tempNumericConversionEabReportSend")]
    public EabReportSend TempNumericConversionEabReportSend
    {
      get => tempNumericConversionEabReportSend ??= new();
      set => tempNumericConversionEabReportSend = value;
    }

    /// <summary>
    /// A value of GrandTotalUnbalanced.
    /// </summary>
    [JsonPropertyName("grandTotalUnbalanced")]
    public Common GrandTotalUnbalanced
    {
      get => grandTotalUnbalanced ??= new();
      set => grandTotalUnbalanced = value;
    }

    /// <summary>
    /// A value of TempNumericConversionCommon.
    /// </summary>
    [JsonPropertyName("tempNumericConversionCommon")]
    public Common TempNumericConversionCommon
    {
      get => tempNumericConversionCommon ??= new();
      set => tempNumericConversionCommon = value;
    }

    /// <summary>
    /// A value of WriteAllAuditInfo.
    /// </summary>
    [JsonPropertyName("writeAllAuditInfo")]
    public Common WriteAllAuditInfo
    {
      get => writeAllAuditInfo ??= new();
      set => writeAllAuditInfo = value;
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
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public Ocse157Verification Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Ocse157Verification Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of NonBalancedCrd.
    /// </summary>
    [JsonPropertyName("nonBalancedCrd")]
    public Common NonBalancedCrd
    {
      get => nonBalancedCrd ??= new();
      set => nonBalancedCrd = value;
    }

    /// <summary>
    /// A value of BalancedCrd.
    /// </summary>
    [JsonPropertyName("balancedCrd")]
    public Common BalancedCrd
    {
      get => balancedCrd ??= new();
      set => balancedCrd = value;
    }

    /// <summary>
    /// A value of BalanceCheck.
    /// </summary>
    [JsonPropertyName("balanceCheck")]
    public Common BalanceCheck
    {
      get => balanceCheck ??= new();
      set => balanceCheck = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    private Common line2EntryFound;
    private Ocse157Verification previousQuarter;
    private Ocse157Verification previous;
    private Common tbdLocalFirstAuditRecord;
    private Common intelligentBalance;
    private EabReportSend tempNumericConversionEabReportSend;
    private Common grandTotalUnbalanced;
    private Common tempNumericConversionCommon;
    private Common writeAllAuditInfo;
    private Array<GroupGroup> group;
    private Ocse157Verification ending;
    private Ocse157Verification starting;
    private Common common;
    private Common nonBalancedCrd;
    private Common balancedCrd;
    private Common balanceCheck;
    private EabFileHandling eabFileHandling;
    private Ocse157Verification ocse157Verification;
    private Ocse34 ocse34;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend open;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrevQuarterDistinctCrdId.
    /// </summary>
    [JsonPropertyName("prevQuarterDistinctCrdId")]
    public Ocse157Verification PrevQuarterDistinctCrdId
    {
      get => prevQuarterDistinctCrdId ??= new();
      set => prevQuarterDistinctCrdId = value;
    }

    /// <summary>
    /// A value of DistinctCrdId.
    /// </summary>
    [JsonPropertyName("distinctCrdId")]
    public Ocse157Verification DistinctCrdId
    {
      get => distinctCrdId ??= new();
      set => distinctCrdId = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private Ocse157Verification prevQuarterDistinctCrdId;
    private Ocse157Verification distinctCrdId;
    private Ocse34 ocse34;
    private Ocse157Verification ocse157Verification;
  }
#endregion
}
