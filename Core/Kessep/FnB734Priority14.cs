// Program: FN_B734_PRIORITY_1_4, ID: 945117538, model: 746.
// Short name: SWE03081
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
/// A program: FN_B734_PRIORITY_1_4.
/// </para>
/// <para>
/// Priority 1-4: Collections on Current Support
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority14: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_4 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority14(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority14.
  /// </summary>
  public FnB734Priority14(IContext context, Import import, Export export):
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
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 02/20/13  GVandy	CQ36547		Initial Development.  Priority 1-1, 1-3, and 1-
    // 4.
    // 			Segment A	
    // 08/28/13  LSS           CQ39887         OCSE157 Report modifications per 
    // the 2012 DRA
    // 					(Data Reliability Audit) requirements / findings
    //                                         
    // Numerator -
    //                                            
    // 1) Do not count negative collection where
    // adjustment
    // 					      occurred in report period to a collection
    // 					      created in a prior report period (DRA Audit
    // 					      Sample #036)
    //                                            
    // 2) Include collection created in Sept of
    // previous
    // 					      fiscal year and posted in October of the
    // 					      current fiscal year (DRA Audit Sample #006)
    //                                         
    // Denominator -
    //                                            
    // 1) Do not include furture collection amount
    // 					      when the collection created date is within
    // 					      September of the current reporting period
    // 					      but the debt detail due date is October in the
    // 					      next reporting period (DRA Audit Sample #006)
    // 04/06/17  GVandy	CQ57069		Restart csp number is not cleared out when 
    // transitioning
    // 					from in month reporting to FYTD reporting.
    // 02/04/20  GVandy	CQ66220		Correlate with OCSE157 changes beginning in FY 
    // 2022.
    // 					These changes include only amounts in OCSE157
    // 					Lines 25, 27, and 29 that are both distributed
    // 					and disbursed.  Export a cutoff FY which defaults to
    // 					2022 but can be overridden with a code table value for testing.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-4: Percent of Current Support Paid
    // -------------------------------------------------------------------------------------
    // Dollar Amount of Current Support Collected divided by Dollar Amount of 
    // Current Support Due (OCSE Line25/ OCSE Line24) Includes both FFYTD and
    // month to date collections.  Note: The YTD calculation will be a new
    // calculation each time the job runs. .
    // This will include numerator and denominator on dashboard view.
    // Report Level: State, Judicial District
    // Report Period: Month and Month/Fiscal year-to-date
    // Numerator-
    // Dollar Amount of Current Support Collected
    // 	1) Collections created (distributed) during report period.  Applied to
    // 	   current support, voluntaries, or gift.
    // 	2) In primary/secondary, credit only the primary.
    // 	3) For joint/several credit the collection only once.
    // 	4) Bypass FcrtRec and FDIR (REIP) cash receipt types.
    // 	5) Bypass adjusted collections where collection adjusted in report 
    // period.
    // 	6) Count negative collection where adjustment occurred in report period 
    // to a
    // 	   collection created in a prior report period.
    // 	7) Include CSENet incoming Interstate collection types.
    // 	8) Count for persons with both active and inactive case roles.
    // Denominator-
    // Dollar Amount Current Support Due
    // 	1) Debt details- accruing Obligation type- due date in reporting period.
    // 		a) Due date within report period and the due date >= to earliest CSE
    // 		   open date
    // 		b) Skip debts that are due before earliest case role date
    // 	2) Collections applied as voluntary obligations during report period.
    // 		a. Exclude REIP payments.
    // 		b. Include CSENet collections.
    // 	3) Collections applied as gift during report period.
    // 		a. Exclude REIP payments.
    // 		b. Include CSENet collections.
    // 	4) Count debt amounts only for primary obligation in primary/secondary
    // 	   situation.
    // 	5) For joint/several situations, credit the debt only once.
    // 	6) Look for original debt detail amount.
    // 	7) Account for adjustments done to those debt details.
    // 	8) Exclude adjusted or concurrent collections.
    // 	9) Debt adjustments must not be counted where the adjustment reason code
    // is
    // 	   for a Close Case adjustment.  Batch process must account for those 
    // accrual
    // 	   amounts that were due within the report period, even though they have
    // 	   since been adjusted off due to case closure.
    // 	10) Count the > future= collection amount, when the collection created 
    // date
    // 	    is within the report period, but the debt detail due date is in the 
    // next
    // 	    month.
    // 		a. Exclude future REIP payments.
    // 		b. Include CSENet (incoming/outgoing) collections.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // -- Determine the federal fiscal year.
    local.FiscalYear.Year = Year(AddMonths(import.FiscalYearStart.Date, 3));

    // -- Determine Previous Year report month.
    local.PreviousYear.ReportMonth = import.DashboardAuditData.ReportMonth - 100
      ;

    // --  Initialize Statewide view
    local.Statewide.AsOfDate = import.ProgramProcessingInfo.ProcessDate;
    local.Statewide.ReportLevel = "ST";
    local.Statewide.ReportLevelId = "KS";
    local.Statewide.ReportMonth = import.DashboardAuditData.ReportMonth;
    local.Contractor.Index = -1;
    local.Contractor.Count = 0;

    // --  Initialize Judicial District group view
    foreach(var item in ReadCseOrganization())
    {
      if (Verify(entities.CseOrganization.Code, "0123456789") != 0)
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Judical District code not numeric.  JD Code = " + entities
          .CseOrganization.Code;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.Local1.Index =
        (int)(StringToNumber(entities.CseOrganization.Code) - 1);
      local.Local1.CheckSize();

      local.Local1.Update.G.AsOfDate = import.ProgramProcessingInfo.ProcessDate;
      local.Local1.Update.G.ReportLevel = "JD";
      local.Local1.Update.G.ReportLevelId = entities.CseOrganization.Code;
      local.Local1.Update.G.ReportMonth = import.DashboardAuditData.ReportMonth;

      // -- Determine contractor from the judicial district to which they are 
      // assigned on the report period end date.
      local.DashboardAuditData.JudicialDistrict = entities.CseOrganization.Code;
      UseFnB734DeterContractorFromJd();
      local.Local1.Update.G.ContractorNumber = local.Contractor1.Code;

      if (local.Contractor.Count < 1)
      {
        ++local.Contractor.Index;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.Contractor1.Code;
        local.Contractor.Update.Gcontractor.ReportLevel = "XJ";
        local.Contractor.Update.Gcontractor.ReportMonth =
          import.DashboardAuditData.ReportMonth;
        local.Contractor.Update.Gcontractor.ReportLevelId =
          local.Local1.Item.G.ContractorNumber ?? Spaces(4);
        local.Contractor.Update.Gcontractor.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
      }
      else
      {
        for(local.Contractor.Index = 0; local.Contractor.Index < local
          .Contractor.Count; ++local.Contractor.Index)
        {
          if (!local.Contractor.CheckSize())
          {
            break;
          }

          if (Equal(local.Contractor1.Code,
            local.Contractor.Item.Gcontractor.ContractorNumber))
          {
            goto ReadEach1;
          }
        }

        local.Contractor.CheckIndex();

        local.Contractor.Index = local.Contractor.Count;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.Contractor1.Code;
        local.Contractor.Update.Gcontractor.ReportLevel = "XJ";
        local.Contractor.Update.Gcontractor.ReportLevelId =
          local.Local1.Item.G.ContractorNumber ?? Spaces(4);
        local.Contractor.Update.Gcontractor.ReportMonth =
          import.DashboardAuditData.ReportMonth;
        local.Contractor.Update.Gcontractor.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
      }

ReadEach1:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-04    "))
    {
      if (local.FiscalYear.Year < import
        .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault())
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-099    Supported CSE Person Number
        //  100-100    Blank
        //  101-101    Local Period Count
        local.RestartCsePerson.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);
      }
      else if (CharAt(import.ProgramCheckpointRestart.RestartInfo, 99) == 'B')
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-098    Payment Request ID
        //  099-099    "B" (indicating to restart in part 2 for the numerator)
        //  100-100    Blank
        //  101-101    Local Period Count
        local.RestartCsePerson.Number = "9999999999";
        local.RestartPaymentRequest.SystemGeneratedIdentifier =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));
      }
      else
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-099    Supported CSE Person Number
        //  100-100    Blank
        //  101-101    Local Period Count
        local.RestartCsePerson.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);
        local.RestartPaymentRequest.SystemGeneratedIdentifier = 0;
      }

      if (IsEmpty(Substring(import.ProgramCheckpointRestart.RestartInfo, 101, 1)))
        
      {
        local.PeriodStart.Count = 1;
      }
      else
      {
        local.PeriodStart.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 101, 1));
      }

      if (!IsEmpty(
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10)))
      {
        // -- Load statewide counts.
        foreach(var item in ReadDashboardStagingPriority9())
        {
          MoveDashboardStagingPriority3(entities.DashboardStagingPriority12,
            local.Statewide);
        }

        // -- Load Judicial District counts.
        foreach(var item in ReadDashboardStagingPriority10())
        {
          local.Local1.Index =
            (int)(StringToNumber(
              entities.DashboardStagingPriority12.ReportLevelId) - 1);
          local.Local1.CheckSize();

          MoveDashboardStagingPriority2(entities.DashboardStagingPriority12,
            local.Local1.Update.G);
        }
      }
      else
      {
        // -- Load statewide counts.
        foreach(var item in ReadDashboardStagingPriority18())
        {
          MoveDashboardStagingPriority3(entities.DashboardStagingPriority12,
            local.Statewide);
          local.Statewide.CurrentSupportPaidFfytdDen = 0;
          local.Statewide.CurrentSupportPaidFfytdNum = 0;
          local.Statewide.CurrentSupportPaidFfytdPer = 0;
          local.Statewide.CurrentSupportPaidFfytdRnk = 0;
          local.Statewide.CurrentSupportPaidMthDen = 0;
          local.Statewide.CurrentSupportPaidMthNum = 0;
          local.Statewide.CurrentSupportPaidMthPer = 0;
          local.Statewide.CurrentSupportPaidMthRnk = 0;
        }

        // -- Load Judicial District counts.
        foreach(var item in ReadDashboardStagingPriority19())
        {
          local.Local1.Index =
            (int)(StringToNumber(
              entities.DashboardStagingPriority12.ReportLevelId) - 1);
          local.Local1.CheckSize();

          MoveDashboardStagingPriority2(entities.DashboardStagingPriority12,
            local.Local1.Update.G);
          local.Local1.Update.G.CurrentSupportPaidFfytdDen = 0;
          local.Local1.Update.G.CurrentSupportPaidFfytdNum = 0;
          local.Local1.Update.G.CurrentSupportPaidFfytdPer = 0;
          local.Local1.Update.G.CurrentSupportPaidFfytdRnk = 0;
          local.Local1.Update.G.CurrentSupportPaidMthDen = 0;
          local.Local1.Update.G.CurrentSupportPaidMthNum = 0;
          local.Local1.Update.G.CurrentSupportPaidMthPer = 0;
          local.Local1.Update.G.CurrentSupportPaidMthRnk = 0;
        }

        foreach(var item in ReadDashboardStagingPriority6())
        {
          // this is for case workers and attorneies
          try
          {
            UpdateDashboardStagingPriority4();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
    else
    {
      local.RestartCsePerson.Number = "";
      local.RestartPaymentRequest.SystemGeneratedIdentifier = 0;
      local.PeriodStart.Count = 1;

      foreach(var item in ReadDashboardStagingPriority6())
      {
        // this is for case workers and attorneies
        try
        {
          UpdateDashboardStagingPriority4();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    for(local.Period.Count = local.PeriodStart.Count; local.Period.Count <= 2; ++
      local.Period.Count)
    {
      switch(local.Period.Count)
      {
        case 1:
          // -- Calculate in-month current support due and collected.
          MoveDateWorkArea(import.ReportStartDate, local.ReportStartDate);
          MoveDateWorkArea(import.ReportEndDate, local.ReportEndDate);

          // -- Set local audit priority value to "IM" (In-Month).  This will be
          // concatenated to
          //    the end of the audit record priority number to indicate in which
          // period the record
          //    was included.
          local.ReportingAbbreviation.Text2 = "IM";

          break;
        case 2:
          // -- Calculate Fiscal Year to Date current support due and collected.
          MoveDateWorkArea(import.FiscalYearStart, local.ReportStartDate);
          MoveDateWorkArea(import.ReportEndDate, local.ReportEndDate);

          // -- Set local audit priority value to "FY" (Fiscal Year to date).  
          // This will be
          //    concatenated to the end of the audit record priority number to 
          // indicate in which
          //    period the record was included.
          local.ReportingAbbreviation.Text2 = "FY";

          break;
        default:
          break;
      }

      local.PreviousMonthStart.Timestamp =
        AddMonths(local.ReportStartDate.Timestamp, -1);

      if (local.FiscalYear.Year < import
        .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault())
      {
        // -------------------------------------------------------------------
        // Read Each is sorted in Asc order of Supp Person #.
        // Maintain a running total for each Supp person and then
        // process a break in person #. This is so we only determine
        // Assistance type once per Supp person (as opposed to
        // once per Collection)
        // -------------------------------------------------------------------
        foreach(var item in ReadCsePersonSupported())
        {
          if (Equal(entities.Supp.Number, local.Prev.Number))
          {
            continue;
          }
          else
          {
            // ------------------------------------------------------------------------------
            // -- Checkpoint saving all the info needed for restarting.
            // ------------------------------------------------------------------------------
            if (local.RecordsReadSinceCommit.Count >= import
              .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
            {
              // -- Save the Statewide counts.
              if (ReadDashboardStagingPriority4())
              {
                try
                {
                  UpdateDashboardStagingPriority9();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
                try
                {
                  CreateDashboardStagingPriority6();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              // -- Save Judicial District counts.
              for(local.Local1.Index = 0; local.Local1.Index < local
                .Local1.Count; ++local.Local1.Index)
              {
                if (!local.Local1.CheckSize())
                {
                  break;
                }

                if (ReadDashboardStagingPriority3())
                {
                  try
                  {
                    UpdateDashboardStagingPriority8();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority5();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
              }

              local.Local1.CheckIndex();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error creating/updating Dashboard_Staging_Priority_1_2.";
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              // -- Checkpoint Info
              // Positions   Value
              // ---------   
              // ------------------------------------
              //  001-080    General Checkpoint Info for PRAD
              //  081-088    Dashboard Priority
              //  089-089    Blank
              //  090-099    Supported CSE Person Number
              //  100-100    Blank
              //  101-101    Local Period Count
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1,
                80) + "1-04    " + " " + local.Prev.Number + " " + NumberToString
                (local.PrevPeriod.Count, 15, 1);
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Error taking checkpoint.";
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.RecordsReadSinceCommit.Count = 0;
            }
          }

          local.Prev.Number = entities.Supp.Number;
          local.PrevPeriod.Count = local.Period.Count;
          ++local.RecordsReadSinceCommit.Count;
          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

          // -------------------------------------------------------------------------------------
          // -- N U M E R A T O R  (Amount of Current Support Collected) (
          // OCSE157 Line 25)
          // -------------------------------------------------------------------------------------
          // -------------------------------------------------------------------
          // -Read Gift and Curr collections
          // -Read colls 'created during' FY and un-adj at the end of FY
          // -Read colls 'adjusted during' FY but created in prev FYs
          // -Skip Concurrent colls
          // -Skip direct payments. (CRT= 2 or 7)
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Comments on READ EACH.
          // -Generates 3 table join on collection, ob_trn and crd
          // -Redundant created_tmst qualification is to aid performance
          // -------------------------------------------------------------------
          // CQ39887 - disable READ and add new READ
          //           Do not count negative adjustments from previous FY (2012 
          // DRA Audit Sample #036)
          //           Include collection created in previous FY and posted in 
          // current FY (2012 DRA Audit Sample #006)
          // CQ39887 - add new READ
          foreach(var item1 in ReadCollectionCsePerson2())
          {
            // -------------------------------------------------------------------------------------
            // -- Current Support was collected for the AP/Supported.
            // -- Include case in the Priority 1-4 numerator (Amount of Current 
            // Support Collected)
            // -- This is the same as OCSE157 Line 25.
            // -------------------------------------------------------------------------------------
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.CurrentSupportPaidMthNum =
                  local.Statewide.CurrentSupportPaidMthNum.GetValueOrDefault() +
                  local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.CurrentSupportPaidFfytdNum =
                  local.Statewide.CurrentSupportPaidFfytdNum.
                    GetValueOrDefault() + local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder4();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidMthNum =
                    local.Local1.Item.G.CurrentSupportPaidMthNum.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidFfytdNum =
                    local.Local1.Item.G.CurrentSupportPaidFfytdNum.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-4(N)" + local
              .ReportingAbbreviation.Text2;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            local.Local2NdRead.CaseNumber = "";

            if (IsEmpty(local.DashboardAuditData.CaseNumber))
            {
              local.UseApSupportedOnly.Flag = "Y";
              UseFnB734DetermineJdFromOrder1();
            }
            else
            {
              MoveDashboardAuditData4(local.DashboardAuditData,
                local.Local2NdRead);
            }

            local.CountCaseWk.Flag = "";

            if (!IsEmpty(local.Local2NdRead.CaseNumber))
            {
              if (ReadCaseAssignmentServiceProvider())
              {
                local.CountCaseWk.Flag = "Y";
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.Worker.Assign(local.DashboardAuditData);
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.CollectionAmount =
                  local.DashboardAuditData.CollectionAmount.GetValueOrDefault();
                  
                local.Worker.DashboardPriority = "1-4.1N" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month Statewide Level
                    local.DashboardStagingPriority35.CurrentSupportPaidMthNum =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthNum.
                        GetValueOrDefault() + local
                      .Worker.CollectionAmount.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date Statewide Level
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdNum =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdNum.GetValueOrDefault() + local
                      .Worker.CollectionAmount.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority2();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority2();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.DashboardStagingPriority35.Assign(
                  local.NullDashboardStagingPriority35);
                local.CountCaseAtty.Flag = "";

                foreach(var item2 in ReadLegalReferralServiceProvider())
                {
                  local.Worker.Assign(local.DashboardAuditData);
                  local.Worker.LegalReferralNumber =
                    entities.LegalReferral.Identifier;
                  local.Worker.LegalReferralDate =
                    entities.LegalReferral.ReferralDate;
                  local.CountCaseAtty.Flag = "Y";

                  if (AsChar(local.CountCaseAtty.Flag) == 'Y')
                  {
                    // -- Case does not owe arrears.  Skip this case.
                    local.DashboardStagingPriority35.AsOfDate =
                      import.ProgramProcessingInfo.ProcessDate;
                    local.DashboardStagingPriority35.ReportLevel = "AT";
                    local.DashboardStagingPriority35.ReportLevelId =
                      entities.ServiceProvider.UserId;
                    local.DashboardStagingPriority35.ReportMonth =
                      import.DashboardAuditData.ReportMonth;
                    local.Worker.CollectionAmount =
                      local.DashboardAuditData.CollectionAmount.
                        GetValueOrDefault();

                    switch(local.Period.Count)
                    {
                      case 1:
                        // -- Increment In-Month Statewide Level
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidMthNum =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidMthNum.GetValueOrDefault() + local
                          .Worker.CollectionAmount.GetValueOrDefault();

                        break;
                      case 2:
                        // -- Increment Fiscal Year to date Statewide Level
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidFfytdNum =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidFfytdNum.GetValueOrDefault() + local
                          .Worker.CollectionAmount.GetValueOrDefault();

                        break;
                      default:
                        break;
                    }

                    local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? ""
                      ;
                    local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                    local.Worker.WorkerId =
                      local.DashboardStagingPriority35.ReportLevelId;
                    local.Worker.DashboardPriority = "1-4.2N" + local
                      .ReportingAbbreviation.Text2;

                    if (AsChar(import.AuditFlag.Flag) == 'Y')
                    {
                      // -- Log to the dashboard audit table.
                      UseFnB734CreateDashboardAudit1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    if (ReadDashboardStagingPriority1())
                    {
                      try
                      {
                        UpdateDashboardStagingPriority2();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                      try
                      {
                        CreateDashboardStagingPriority2();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                            break;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error creating/updating Dashboard_Staging_Priority_1_2.";
                        
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Test1;
                  }
                }
              }
            }

Test1:
            ;
          }

          // CQ39887 - END  (disable READ and add new READ)
          // -------------------------------------------------------------------------------------
          // --  D E N O M I N A T O R  (Amount of Current Support Due) (OCSE157
          // Line 24)
          // -------------------------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Skip if supp person is not setup as CH/AR on a case.
          // -------------------------------------------------------------------
          UseFnGetEarliestCaseRole4Pers();

          if (Equal(local.Earliest.StartDate, local.NullDateWorkArea.Date))
          {
            continue;
          }

          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

          // -------------------------------------------------------------------
          // -Read Accruing debts that are 'due during' FY
          // -Skip debts due before supp person was assigned to Case
          // -Skip debts created after FY end
          // -------------------------------------------------------------------
          foreach(var item1 in ReadDebtObligationObligationTypeDebtDetailCsePerson())
            
          {
            MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
              

            // -- Split amounts on Joint/Several between the obligors.
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
            {
              local.DashboardAuditData.DebtBalanceDue = entities.Debt.Amount / 2
                ;
            }
            else
            {
              local.DashboardAuditData.DebtBalanceDue = entities.Debt.Amount;
            }

            // -------------------------------------------------------------
            // NB- There are 2 relationships between Obligation and LA.
            // One is direct, second reln is via LAD. Both relationships are
            // maintained for Accruing Obligations. For faster access we will
            // use the direct relationship.
            // ------------------------------------------------------------
            if (!ReadLegalAction())
            {
              // ------------------------------------------------------------
              // We should always find a legal action on Accruing
              // obligations. However, reationship is defined as optional.
              // Set SPACES for court order if LA is nf.
              // ------------------------------------------------------------
            }

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.CurrentSupportPaidMthDen =
                  local.Statewide.CurrentSupportPaidMthDen.GetValueOrDefault() +
                  local.DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.CurrentSupportPaidFfytdDen =
                  local.Statewide.CurrentSupportPaidFfytdDen.
                    GetValueOrDefault() + local
                  .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder5();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidMthDen =
                    local.Local1.Item.G.CurrentSupportPaidMthDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidFfytdDen =
                    local.Local1.Item.G.CurrentSupportPaidFfytdDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-4(D)" + local
              .ReportingAbbreviation.Text2;
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            local.Local2NdRead.CaseNumber = "";

            if (IsEmpty(local.DashboardAuditData.CaseNumber))
            {
              local.UseApSupportedOnly.Flag = "Y";

              // -- Determine Case Number...
              UseFnB734DetermineJdFromOrder2();
            }
            else
            {
              MoveDashboardAuditData4(local.DashboardAuditData,
                local.Local2NdRead);
            }

            local.CountCaseWk.Flag = "";

            if (!IsEmpty(local.Local2NdRead.CaseNumber))
            {
              if (ReadCaseAssignmentServiceProvider())
              {
                local.CountCaseWk.Flag = "Y";
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.Worker.Assign(local.DashboardAuditData);
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.DebtBalanceDue =
                  local.DashboardAuditData.DebtBalanceDue.GetValueOrDefault();
                local.Worker.DashboardPriority = "1-4.1D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .Worker.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .Worker.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority3();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.DashboardStagingPriority35.Assign(
                  local.NullDashboardStagingPriority35);
                local.CountCaseAtty.Flag = "";

                foreach(var item2 in ReadLegalReferralServiceProvider())
                {
                  local.Worker.Assign(local.DashboardAuditData);
                  local.Worker.LegalReferralNumber =
                    entities.LegalReferral.Identifier;
                  local.Worker.LegalReferralDate =
                    entities.LegalReferral.ReferralDate;
                  local.CountCaseAtty.Flag = "Y";

                  if (AsChar(local.CountCaseAtty.Flag) == 'Y')
                  {
                    // -- Case does not owe arrears.  Skip this case.
                    local.DashboardStagingPriority35.AsOfDate =
                      import.ProgramProcessingInfo.ProcessDate;
                    local.DashboardStagingPriority35.ReportLevel = "AT";
                    local.DashboardStagingPriority35.ReportLevelId =
                      entities.ServiceProvider.UserId;
                    local.DashboardStagingPriority35.ReportMonth =
                      import.DashboardAuditData.ReportMonth;

                    switch(local.Period.Count)
                    {
                      case 1:
                        // -- Increment In-Month
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidMthDen =
                            local.DashboardAuditData.DebtBalanceDue.
                            GetValueOrDefault() + local
                          .DashboardStagingPriority35.CurrentSupportPaidMthDen.
                            GetValueOrDefault();

                        break;
                      case 2:
                        // -- Increment Fiscal Year to date
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidFfytdDen =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                          .DashboardAuditData.DebtBalanceDue.
                            GetValueOrDefault();

                        break;
                      default:
                        break;
                    }

                    local.Worker.DebtBalanceDue =
                      local.DashboardAuditData.DebtBalanceDue.
                        GetValueOrDefault();
                    local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? ""
                      ;
                    local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                    local.Worker.WorkerId =
                      local.DashboardStagingPriority35.ReportLevelId;
                    local.Worker.DashboardPriority = "1-4.2D" + local
                      .ReportingAbbreviation.Text2;

                    if (AsChar(import.AuditFlag.Flag) == 'Y')
                    {
                      // -- Log to the dashboard audit table.
                      UseFnB734CreateDashboardAudit1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    if (ReadDashboardStagingPriority1())
                    {
                      try
                      {
                        UpdateDashboardStagingPriority1();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                      try
                      {
                        CreateDashboardStagingPriority3();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                            break;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error creating/updating Dashboard_Staging_Priority_1_2.";
                        
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    break;
                  }
                }
              }
            }

            // --------------------------------------------------------------
            // Process Debt Adj.
            // Skip Adj that were made to close a case.(Otr_Rln_Rsn.SGI=9)
            // -------------------------------------------------------------
            foreach(var item2 in ReadDebtAdjustment())
            {
              // ------------------------------------------------------------------
              // I type adj will increase original debt amt. D type decreases 
              // it.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.DashboardAuditData.DebtBalanceDue =
                  entities.DebtAdjustment.Amount;
              }
              else
              {
                local.DashboardAuditData.DebtBalanceDue =
                  -entities.DebtAdjustment.Amount;
              }

              // -- Split debt amounts on Joint/Several between the obligors.
              if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
              {
                local.DashboardAuditData.DebtBalanceDue =
                  local.DashboardAuditData.DebtBalanceDue.GetValueOrDefault() /
                  2;
              }

              // -- Increment Statewide Level
              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Statewide Level
                  local.Statewide.CurrentSupportPaidMthDen =
                    local.Statewide.CurrentSupportPaidMthDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Statewide Level
                  local.Statewide.CurrentSupportPaidFfytdDen =
                    local.Statewide.CurrentSupportPaidFfytdDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                default:
                  break;
              }

              // -- Increment Judicial District Level
              if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
              {
                local.Local1.Index =
                  (int)(
                    StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                  1);
                local.Local1.CheckSize();

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month Judicial District Level
                    local.Local1.Update.G.CurrentSupportPaidMthDen =
                      local.Local1.Item.G.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date Judicial District Level
                    local.Local1.Update.G.CurrentSupportPaidFfytdDen =
                      local.Local1.Item.G.CurrentSupportPaidFfytdDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }
              }

              if (AsChar(import.AuditFlag.Flag) == 'Y')
              {
                UseFnB734CreateDashboardAudit2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.Assign(local.DashboardAuditData);

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                local.Worker.DashboardPriority = "1-4.1D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                // -- Determine office and judicial district to which case is 
                // assigned on the report period end date.
                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority3();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }

              MoveDashboardStagingPriority1(local.Initialize,
                local.DashboardStagingPriority12);
              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseAtty.Flag) == 'Y')
              {
                // -- Case does not owe arrears.  Skip this case.
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "AT";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.ServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.Assign(local.DashboardAuditData);

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                local.Worker.DashboardPriority = "1-4.2D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority3();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }

              // ---------------------------------------------
              // End of Debt Adjustments READ EACH.
              // --------------------------------------------
            }

            // --------------------------------------------------------------
            // -- Subtract collection amounts prior to the start of the 
            // reporting period.
            // -- This will yield the debt amount "owing" at the start of the 
            // reporting period.
            // -------------------------------------------------------------
            // CQ39887 - disabled READ - do not need to to subtract collection 
            // amounts since no longer including
            // collections created during the FY with debts due in the next FY -
            // per DRA findings for 2012
            // CQ39887 - END of disabled READ - do not need to to subtract 
            // collection amounts
            // ------------------------------------------
            // End of Debt READ EACH.
            // ------------------------------------------
          }

          // --  Include collections created during the reporting period for 
          // debts due in the month following the report period.
          //     (i.e. collections created during September for debts due in 
          // October).
          // CQ39887 - disabled READ - no longer include collections for future 
          // debts - per DRA findings for 2012 (DRA Audit Sample #006)
          // CQ39887 - END  (disabled READ)
          // -------------------------------------------------------------------
          // We finished processing all Accruing debts for Supp Person.
          // Now, read Collections applied to Vol and Gifts, since these
          // debts don't show an amount due at Debt level.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // -Read Gift and VOL collections
          // -Read colls 'created during' FY and un-adj at the end of FY
          // -Skip Concurrent colls
          // -Skip direct payments. (CRT= 2 or 7)
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Comments on READ EACH.
          // -Generates 2 table join on collection and ob_trn
          // -------------------------------------------------------------------
          foreach(var item1 in ReadCollectionCsePerson3())
          {
            // -------------------------------------------------------------------
            // -Skip colls before person is assigned to case.
            // -------------------------------------------------------------------
            if (Lt(Date(entities.Collection.CreatedTmst),
              local.Earliest.StartDate))
            {
              continue;
            }

            MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
              

            // -- 03/22/2013 fyi... No need to split amounts for Joint and 
            // Several.
            // --                   J/S isn't applicable for Voluntaries and 
            // Gifts.
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.CurrentSupportPaidMthDen =
                  local.Statewide.CurrentSupportPaidMthDen.GetValueOrDefault() +
                  local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.CurrentSupportPaidFfytdDen =
                  local.Statewide.CurrentSupportPaidFfytdDen.
                    GetValueOrDefault() + local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder4();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidMthDen =
                    local.Local1.Item.G.CurrentSupportPaidMthDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidFfytdDen =
                    local.Local1.Item.G.CurrentSupportPaidFfytdDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-4(D)" + local
              .ReportingAbbreviation.Text2;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            local.Local2NdRead.CaseNumber = "";
            MoveDashboardStagingPriority1(local.Initialize,
              local.DashboardStagingPriority12);

            if (IsEmpty(local.DashboardAuditData.CaseNumber))
            {
              local.UseApSupportedOnly.Flag = "Y";
              UseFnB734DetermineJdFromOrder3();
            }
            else
            {
              MoveDashboardAuditData4(local.DashboardAuditData,
                local.Local2NdRead);
            }

            if (!IsEmpty(local.Local2NdRead.CaseNumber))
            {
              local.CountCaseWk.Flag = "";

              if (ReadCaseAssignmentServiceProvider())
              {
                local.CountCaseWk.Flag = "Y";
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.Worker.Assign(local.DashboardAuditData);
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.CollectionAmount =
                  local.DashboardAuditData.CollectionAmount.GetValueOrDefault();
                  
                local.Worker.DashboardPriority = "1-4.1D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.DashboardStagingPriority35.Assign(
                  local.NullDashboardStagingPriority35);
                local.CountCaseAtty.Flag = "";

                foreach(var item2 in ReadLegalReferralServiceProvider())
                {
                  local.Worker.Assign(local.DashboardAuditData);
                  local.Worker.LegalReferralNumber =
                    entities.LegalReferral.Identifier;
                  local.Worker.LegalReferralDate =
                    entities.LegalReferral.ReferralDate;
                  local.CountCaseAtty.Flag = "Y";
                  local.DashboardStagingPriority35.Assign(
                    local.NullDashboardStagingPriority35);

                  if (AsChar(local.CountCaseAtty.Flag) == 'Y')
                  {
                    // -- Case does not owe arrears.  Skip this case.
                    local.DashboardStagingPriority35.AsOfDate =
                      import.ProgramProcessingInfo.ProcessDate;
                    local.DashboardStagingPriority35.ReportLevel = "AT";
                    local.DashboardStagingPriority35.ReportLevelId =
                      entities.ServiceProvider.UserId;
                    local.DashboardStagingPriority35.ReportMonth =
                      import.DashboardAuditData.ReportMonth;

                    switch(local.Period.Count)
                    {
                      case 1:
                        // -- Increment In-Month
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidMthDen =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidMthDen.GetValueOrDefault() + local
                          .DashboardAuditData.CollectionAmount.
                            GetValueOrDefault();

                        break;
                      case 2:
                        // -- Increment Fiscal Year to date
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidFfytdDen =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                          .DashboardAuditData.CollectionAmount.
                            GetValueOrDefault();

                        break;
                      default:
                        break;
                    }

                    local.Worker.CollectionAmount =
                      local.DashboardAuditData.CollectionAmount.
                        GetValueOrDefault();
                    local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? ""
                      ;
                    local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                    local.Worker.WorkerId =
                      local.DashboardStagingPriority35.ReportLevelId;
                    local.Worker.DashboardPriority = "1-4.2D" + local
                      .ReportingAbbreviation.Text2;

                    if (AsChar(import.AuditFlag.Flag) == 'Y')
                    {
                      // -- Log to the dashboard audit table.
                      UseFnB734CreateDashboardAudit1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    if (ReadDashboardStagingPriority1())
                    {
                      try
                      {
                        UpdateDashboardStagingPriority1();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                      try
                      {
                        CreateDashboardStagingPriority1();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                            break;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error creating/updating Dashboard_Staging_Priority_1_2.";
                        
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Test2;
                  }
                }
              }
            }

Test2:
            ;

            // --------------------------------------------
            // End of Collection READ EACH.
            // --------------------------------------------
          }
        }
      }
      else
      {
        // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts 
        // that are both
        // distributed and disbursed.
        // -------------------------------------------------------------------
        // Read Each is sorted in Asc order of Supp Person #.
        // Maintain a running total for each Supp person and then
        // process a break in person #. This is so we only determine
        // Assistance type once per Supp person (as opposed to
        // once per Collection)
        // -------------------------------------------------------------------
        foreach(var item in ReadCsePersonSupported())
        {
          if (Equal(entities.Supp.Number, local.Prev.Number))
          {
            continue;
          }
          else
          {
            // ------------------------------------------------------------------------------
            // -- Checkpoint saving all the info needed for restarting.
            // ------------------------------------------------------------------------------
            if (local.RecordsReadSinceCommit.Count >= import
              .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
            {
              // -- Save the Statewide counts.
              if (ReadDashboardStagingPriority4())
              {
                try
                {
                  UpdateDashboardStagingPriority9();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
                try
                {
                  CreateDashboardStagingPriority6();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              // -- Save Judicial District counts.
              for(local.Local1.Index = 0; local.Local1.Index < local
                .Local1.Count; ++local.Local1.Index)
              {
                if (!local.Local1.CheckSize())
                {
                  break;
                }

                if (ReadDashboardStagingPriority3())
                {
                  try
                  {
                    UpdateDashboardStagingPriority8();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority5();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
              }

              local.Local1.CheckIndex();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error creating/updating Dashboard_Staging_Priority_1_2.";
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              // -- Checkpoint Info
              // Positions   Value
              // ---------   
              // ------------------------------------
              //  001-080    General Checkpoint Info for PRAD
              //  081-088    Dashboard Priority
              //  089-089    Blank
              //  090-099    Supported CSE Person Number
              //  100-100    Blank
              //  101-101    Local Period Count
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1,
                80) + "1-04    " + " " + local.Prev.Number + " " + NumberToString
                (local.PrevPeriod.Count, 15, 1);
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Error taking checkpoint.";
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.RecordsReadSinceCommit.Count = 0;
            }
          }

          local.Prev.Number = entities.Supp.Number;
          local.PrevPeriod.Count = local.Period.Count;
          ++local.RecordsReadSinceCommit.Count;
          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

          // -------------------------------------------------------------------------------------
          // -- N U M E R A T O R  (Amount of Current Support Collected) (
          // OCSE157 Line 25)
          // -------------------------------------------------------------------------------------
          // -------------------------------------------------------------------
          // -Read Gift and Curr collections
          // -Read colls 'created during' FY and un-adj at the end of FY
          // -Read colls 'adjusted during' FY but created in prev FYs
          // -Skip Concurrent colls
          // -Skip direct payments. (CRT= 2 or 7)
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Comments on READ EACH.
          // -Generates 3 table join on collection, ob_trn and crd
          // -Redundant created_tmst qualification is to aid performance
          // -------------------------------------------------------------------
          // CQ39887 - disable READ and add new READ
          //           Do not count negative adjustments from previous FY (2012 
          // DRA Audit Sample #036)
          //           Include collection created in previous FY and posted in 
          // current FY (2012 DRA Audit Sample #006)
          // 02/04/20  GVandy  CQ66220  Beginning in FY 2022, include only 
          // amounts that are both
          // distributed and disbursed.  First we gather the current support 
          // collections that are
          // retained by the state.  These collection will have applied as AF, 
          // FC, NF, or NC.
          // CQ39887 - add new READ
          foreach(var item1 in ReadCollectionCsePerson1())
          {
            // -------------------------------------------------------------------------------------
            // -- Current Support was collected for the AP/Supported.
            // -- Include case in the Priority 1-4 numerator (Amount of Current 
            // Support Collected)
            // -- This is the same as OCSE157 Line 25.
            // -------------------------------------------------------------------------------------
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.CurrentSupportPaidMthNum =
                  local.Statewide.CurrentSupportPaidMthNum.GetValueOrDefault() +
                  local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.CurrentSupportPaidFfytdNum =
                  local.Statewide.CurrentSupportPaidFfytdNum.
                    GetValueOrDefault() + local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder4();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidMthNum =
                    local.Local1.Item.G.CurrentSupportPaidMthNum.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidFfytdNum =
                    local.Local1.Item.G.CurrentSupportPaidFfytdNum.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-4(N)" + local
              .ReportingAbbreviation.Text2;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            local.Local2NdRead.CaseNumber = "";

            if (IsEmpty(local.DashboardAuditData.CaseNumber))
            {
              local.UseApSupportedOnly.Flag = "Y";
              UseFnB734DetermineJdFromOrder1();
            }
            else
            {
              MoveDashboardAuditData4(local.DashboardAuditData,
                local.Local2NdRead);
            }

            local.CountCaseWk.Flag = "";

            if (!IsEmpty(local.Local2NdRead.CaseNumber))
            {
              if (ReadCaseAssignmentServiceProvider())
              {
                local.CountCaseWk.Flag = "Y";
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.Worker.Assign(local.DashboardAuditData);
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.CollectionAmount =
                  local.DashboardAuditData.CollectionAmount.GetValueOrDefault();
                  
                local.Worker.DashboardPriority = "1-4.1N" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month Statewide Level
                    local.DashboardStagingPriority35.CurrentSupportPaidMthNum =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthNum.
                        GetValueOrDefault() + local
                      .Worker.CollectionAmount.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date Statewide Level
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdNum =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdNum.GetValueOrDefault() + local
                      .Worker.CollectionAmount.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority2();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority2();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.DashboardStagingPriority35.Assign(
                  local.NullDashboardStagingPriority35);
                local.CountCaseAtty.Flag = "";

                foreach(var item2 in ReadLegalReferralServiceProvider())
                {
                  local.Worker.Assign(local.DashboardAuditData);
                  local.Worker.LegalReferralNumber =
                    entities.LegalReferral.Identifier;
                  local.Worker.LegalReferralDate =
                    entities.LegalReferral.ReferralDate;
                  local.CountCaseAtty.Flag = "Y";

                  if (AsChar(local.CountCaseAtty.Flag) == 'Y')
                  {
                    // -- Case does not owe arrears.  Skip this case.
                    local.DashboardStagingPriority35.AsOfDate =
                      import.ProgramProcessingInfo.ProcessDate;
                    local.DashboardStagingPriority35.ReportLevel = "AT";
                    local.DashboardStagingPriority35.ReportLevelId =
                      entities.ServiceProvider.UserId;
                    local.DashboardStagingPriority35.ReportMonth =
                      import.DashboardAuditData.ReportMonth;
                    local.Worker.CollectionAmount =
                      local.DashboardAuditData.CollectionAmount.
                        GetValueOrDefault();

                    switch(local.Period.Count)
                    {
                      case 1:
                        // -- Increment In-Month Statewide Level
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidMthNum =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidMthNum.GetValueOrDefault() + local
                          .Worker.CollectionAmount.GetValueOrDefault();

                        break;
                      case 2:
                        // -- Increment Fiscal Year to date Statewide Level
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidFfytdNum =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidFfytdNum.GetValueOrDefault() + local
                          .Worker.CollectionAmount.GetValueOrDefault();

                        break;
                      default:
                        break;
                    }

                    local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? ""
                      ;
                    local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                    local.Worker.WorkerId =
                      local.DashboardStagingPriority35.ReportLevelId;
                    local.Worker.DashboardPriority = "1-4.2N" + local
                      .ReportingAbbreviation.Text2;

                    if (AsChar(import.AuditFlag.Flag) == 'Y')
                    {
                      // -- Log to the dashboard audit table.
                      UseFnB734CreateDashboardAudit1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    if (ReadDashboardStagingPriority1())
                    {
                      try
                      {
                        UpdateDashboardStagingPriority2();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                      try
                      {
                        CreateDashboardStagingPriority2();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                            break;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error creating/updating Dashboard_Staging_Priority_1_2.";
                        
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Test3;
                  }
                }
              }
            }

Test3:
            ;
          }

          // CQ39887 - END  (disable READ and add new READ)
          // -------------------------------------------------------------------------------------
          // --  D E N O M I N A T O R  (Amount of Current Support Due) (OCSE157
          // Line 24)
          // -------------------------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Skip if supp person is not setup as CH/AR on a case.
          // -------------------------------------------------------------------
          UseFnGetEarliestCaseRole4Pers();

          if (Equal(local.Earliest.StartDate, local.NullDateWorkArea.Date))
          {
            continue;
          }

          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

          // -------------------------------------------------------------------
          // -Read Accruing debts that are 'due during' FY
          // -Skip debts due before supp person was assigned to Case
          // -Skip debts created after FY end
          // -------------------------------------------------------------------
          foreach(var item1 in ReadDebtObligationObligationTypeDebtDetailCsePerson())
            
          {
            MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
              

            // -- Split amounts on Joint/Several between the obligors.
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
            {
              local.DashboardAuditData.DebtBalanceDue = entities.Debt.Amount / 2
                ;
            }
            else
            {
              local.DashboardAuditData.DebtBalanceDue = entities.Debt.Amount;
            }

            // -------------------------------------------------------------
            // NB- There are 2 relationships between Obligation and LA.
            // One is direct, second reln is via LAD. Both relationships are
            // maintained for Accruing Obligations. For faster access we will
            // use the direct relationship.
            // ------------------------------------------------------------
            if (!ReadLegalAction())
            {
              // ------------------------------------------------------------
              // We should always find a legal action on Accruing
              // obligations. However, reationship is defined as optional.
              // Set SPACES for court order if LA is nf.
              // ------------------------------------------------------------
            }

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.CurrentSupportPaidMthDen =
                  local.Statewide.CurrentSupportPaidMthDen.GetValueOrDefault() +
                  local.DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.CurrentSupportPaidFfytdDen =
                  local.Statewide.CurrentSupportPaidFfytdDen.
                    GetValueOrDefault() + local
                  .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder5();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidMthDen =
                    local.Local1.Item.G.CurrentSupportPaidMthDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidFfytdDen =
                    local.Local1.Item.G.CurrentSupportPaidFfytdDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-4(D)" + local
              .ReportingAbbreviation.Text2;
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            local.Local2NdRead.CaseNumber = "";

            if (IsEmpty(local.DashboardAuditData.CaseNumber))
            {
              local.UseApSupportedOnly.Flag = "Y";

              // -- Determine Case Number...
              UseFnB734DetermineJdFromOrder2();
            }
            else
            {
              MoveDashboardAuditData4(local.DashboardAuditData,
                local.Local2NdRead);
            }

            local.CountCaseWk.Flag = "";

            if (!IsEmpty(local.Local2NdRead.CaseNumber))
            {
              if (ReadCaseAssignmentServiceProvider())
              {
                local.CountCaseWk.Flag = "Y";
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.Worker.Assign(local.DashboardAuditData);
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.DebtBalanceDue =
                  local.DashboardAuditData.DebtBalanceDue.GetValueOrDefault();
                local.Worker.DashboardPriority = "1-4.1D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .Worker.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .Worker.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority3();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.DashboardStagingPriority35.Assign(
                  local.NullDashboardStagingPriority35);
                local.CountCaseAtty.Flag = "";

                foreach(var item2 in ReadLegalReferralServiceProvider())
                {
                  local.Worker.Assign(local.DashboardAuditData);
                  local.Worker.LegalReferralNumber =
                    entities.LegalReferral.Identifier;
                  local.Worker.LegalReferralDate =
                    entities.LegalReferral.ReferralDate;
                  local.CountCaseAtty.Flag = "Y";

                  if (AsChar(local.CountCaseAtty.Flag) == 'Y')
                  {
                    // -- Case does not owe arrears.  Skip this case.
                    local.DashboardStagingPriority35.AsOfDate =
                      import.ProgramProcessingInfo.ProcessDate;
                    local.DashboardStagingPriority35.ReportLevel = "AT";
                    local.DashboardStagingPriority35.ReportLevelId =
                      entities.ServiceProvider.UserId;
                    local.DashboardStagingPriority35.ReportMonth =
                      import.DashboardAuditData.ReportMonth;

                    switch(local.Period.Count)
                    {
                      case 1:
                        // -- Increment In-Month
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidMthDen =
                            local.DashboardAuditData.DebtBalanceDue.
                            GetValueOrDefault() + local
                          .DashboardStagingPriority35.CurrentSupportPaidMthDen.
                            GetValueOrDefault();

                        break;
                      case 2:
                        // -- Increment Fiscal Year to date
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidFfytdDen =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                          .DashboardAuditData.DebtBalanceDue.
                            GetValueOrDefault();

                        break;
                      default:
                        break;
                    }

                    local.Worker.DebtBalanceDue =
                      local.DashboardAuditData.DebtBalanceDue.
                        GetValueOrDefault();
                    local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? ""
                      ;
                    local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                    local.Worker.WorkerId =
                      local.DashboardStagingPriority35.ReportLevelId;
                    local.Worker.DashboardPriority = "1-4.2D" + local
                      .ReportingAbbreviation.Text2;

                    if (AsChar(import.AuditFlag.Flag) == 'Y')
                    {
                      // -- Log to the dashboard audit table.
                      UseFnB734CreateDashboardAudit1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    if (ReadDashboardStagingPriority1())
                    {
                      try
                      {
                        UpdateDashboardStagingPriority1();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                      try
                      {
                        CreateDashboardStagingPriority3();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                            break;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error creating/updating Dashboard_Staging_Priority_1_2.";
                        
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    break;
                  }
                }
              }
            }

            // --------------------------------------------------------------
            // Process Debt Adj.
            // Skip Adj that were made to close a case.(Otr_Rln_Rsn.SGI=9)
            // -------------------------------------------------------------
            foreach(var item2 in ReadDebtAdjustment())
            {
              // ------------------------------------------------------------------
              // I type adj will increase original debt amt. D type decreases 
              // it.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.DashboardAuditData.DebtBalanceDue =
                  entities.DebtAdjustment.Amount;
              }
              else
              {
                local.DashboardAuditData.DebtBalanceDue =
                  -entities.DebtAdjustment.Amount;
              }

              // -- Split debt amounts on Joint/Several between the obligors.
              if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
              {
                local.DashboardAuditData.DebtBalanceDue =
                  local.DashboardAuditData.DebtBalanceDue.GetValueOrDefault() /
                  2;
              }

              // -- Increment Statewide Level
              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Statewide Level
                  local.Statewide.CurrentSupportPaidMthDen =
                    local.Statewide.CurrentSupportPaidMthDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Statewide Level
                  local.Statewide.CurrentSupportPaidFfytdDen =
                    local.Statewide.CurrentSupportPaidFfytdDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                  break;
                default:
                  break;
              }

              // -- Increment Judicial District Level
              if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
              {
                local.Local1.Index =
                  (int)(
                    StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                  1);
                local.Local1.CheckSize();

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month Judicial District Level
                    local.Local1.Update.G.CurrentSupportPaidMthDen =
                      local.Local1.Item.G.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date Judicial District Level
                    local.Local1.Update.G.CurrentSupportPaidFfytdDen =
                      local.Local1.Item.G.CurrentSupportPaidFfytdDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }
              }

              if (AsChar(import.AuditFlag.Flag) == 'Y')
              {
                UseFnB734CreateDashboardAudit2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.Assign(local.DashboardAuditData);

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                local.Worker.DashboardPriority = "1-4.1D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                // -- Determine office and judicial district to which case is 
                // assigned on the report period end date.
                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority3();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }

              MoveDashboardStagingPriority1(local.Initialize,
                local.DashboardStagingPriority12);
              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseAtty.Flag) == 'Y')
              {
                // -- Case does not owe arrears.  Skip this case.
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "AT";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.ServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.Assign(local.DashboardAuditData);

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .DashboardAuditData.DebtBalanceDue.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                local.Worker.DashboardPriority = "1-4.2D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority3();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }

              // ---------------------------------------------
              // End of Debt Adjustments READ EACH.
              // --------------------------------------------
            }

            // --------------------------------------------------------------
            // -- Subtract collection amounts prior to the start of the 
            // reporting period.
            // -- This will yield the debt amount "owing" at the start of the 
            // reporting period.
            // -------------------------------------------------------------
            // CQ39887 - disabled READ - do not need to to subtract collection 
            // amounts since no longer including
            // collections created during the FY with debts due in the next FY -
            // per DRA findings for 2012
            // CQ39887 - END of disabled READ - do not need to to subtract 
            // collection amounts
            // ------------------------------------------
            // End of Debt READ EACH.
            // ------------------------------------------
          }

          // --  Include collections created during the reporting period for 
          // debts due in the month following the report period.
          //     (i.e. collections created during September for debts due in 
          // October).
          // CQ39887 - disabled READ - no longer include collections for future 
          // debts - per DRA findings for 2012 (DRA Audit Sample #006)
          // CQ39887 - END  (disabled READ)
          // -------------------------------------------------------------------
          // We finished processing all Accruing debts for Supp Person.
          // Now, read Collections applied to Vol and Gifts, since these
          // debts don't show an amount due at Debt level.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // -Read Gift and VOL collections
          // -Read colls 'created during' FY and un-adj at the end of FY
          // -Skip Concurrent colls
          // -Skip direct payments. (CRT= 2 or 7)
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Comments on READ EACH.
          // -Generates 2 table join on collection and ob_trn
          // -------------------------------------------------------------------
          foreach(var item1 in ReadCollectionCsePerson3())
          {
            // -------------------------------------------------------------------
            // -Skip colls before person is assigned to case.
            // -------------------------------------------------------------------
            if (Lt(Date(entities.Collection.CreatedTmst),
              local.Earliest.StartDate))
            {
              continue;
            }

            MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
              

            // -- 03/22/2013 fyi... No need to split amounts for Joint and 
            // Several.
            // --                   J/S isn't applicable for Voluntaries and 
            // Gifts.
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.CurrentSupportPaidMthDen =
                  local.Statewide.CurrentSupportPaidMthDen.GetValueOrDefault() +
                  local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.CurrentSupportPaidFfytdDen =
                  local.Statewide.CurrentSupportPaidFfytdDen.
                    GetValueOrDefault() + local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder4();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidMthDen =
                    local.Local1.Item.G.CurrentSupportPaidMthDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidFfytdDen =
                    local.Local1.Item.G.CurrentSupportPaidFfytdDen.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-4(D)" + local
              .ReportingAbbreviation.Text2;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            local.Local2NdRead.CaseNumber = "";
            MoveDashboardStagingPriority1(local.Initialize,
              local.DashboardStagingPriority12);

            if (IsEmpty(local.DashboardAuditData.CaseNumber))
            {
              local.UseApSupportedOnly.Flag = "Y";
              UseFnB734DetermineJdFromOrder3();
            }
            else
            {
              MoveDashboardAuditData4(local.DashboardAuditData,
                local.Local2NdRead);
            }

            if (!IsEmpty(local.Local2NdRead.CaseNumber))
            {
              local.CountCaseWk.Flag = "";

              if (ReadCaseAssignmentServiceProvider())
              {
                local.CountCaseWk.Flag = "Y";
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.Worker.Assign(local.DashboardAuditData);
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.CollectionAmount =
                  local.DashboardAuditData.CollectionAmount.GetValueOrDefault();
                  
                local.Worker.DashboardPriority = "1-4.1D" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month
                    local.DashboardStagingPriority35.CurrentSupportPaidMthDen =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
                        GetValueOrDefault() + local
                      .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdDen =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                      .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority1();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.DashboardStagingPriority35.Assign(
                  local.NullDashboardStagingPriority35);
                local.CountCaseAtty.Flag = "";

                foreach(var item2 in ReadLegalReferralServiceProvider())
                {
                  local.Worker.Assign(local.DashboardAuditData);
                  local.Worker.LegalReferralNumber =
                    entities.LegalReferral.Identifier;
                  local.Worker.LegalReferralDate =
                    entities.LegalReferral.ReferralDate;
                  local.CountCaseAtty.Flag = "Y";
                  local.DashboardStagingPriority35.Assign(
                    local.NullDashboardStagingPriority35);

                  if (AsChar(local.CountCaseAtty.Flag) == 'Y')
                  {
                    // -- Case does not owe arrears.  Skip this case.
                    local.DashboardStagingPriority35.AsOfDate =
                      import.ProgramProcessingInfo.ProcessDate;
                    local.DashboardStagingPriority35.ReportLevel = "AT";
                    local.DashboardStagingPriority35.ReportLevelId =
                      entities.ServiceProvider.UserId;
                    local.DashboardStagingPriority35.ReportMonth =
                      import.DashboardAuditData.ReportMonth;

                    switch(local.Period.Count)
                    {
                      case 1:
                        // -- Increment In-Month
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidMthDen =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidMthDen.GetValueOrDefault() + local
                          .DashboardAuditData.CollectionAmount.
                            GetValueOrDefault();

                        break;
                      case 2:
                        // -- Increment Fiscal Year to date
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidFfytdDen =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidFfytdDen.GetValueOrDefault() + local
                          .DashboardAuditData.CollectionAmount.
                            GetValueOrDefault();

                        break;
                      default:
                        break;
                    }

                    local.Worker.CollectionAmount =
                      local.DashboardAuditData.CollectionAmount.
                        GetValueOrDefault();
                    local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? ""
                      ;
                    local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                    local.Worker.WorkerId =
                      local.DashboardStagingPriority35.ReportLevelId;
                    local.Worker.DashboardPriority = "1-4.2D" + local
                      .ReportingAbbreviation.Text2;

                    if (AsChar(import.AuditFlag.Flag) == 'Y')
                    {
                      // -- Log to the dashboard audit table.
                      UseFnB734CreateDashboardAudit1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    if (ReadDashboardStagingPriority1())
                    {
                      try
                      {
                        UpdateDashboardStagingPriority1();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                      try
                      {
                        CreateDashboardStagingPriority1();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                            break;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error creating/updating Dashboard_Staging_Priority_1_2.";
                        
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Test4;
                  }
                }
              }
            }

Test4:
            ;

            // --------------------------------------------
            // End of Collection READ EACH.
            // --------------------------------------------
          }
        }

        // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts 
        // that are both
        // distributed and disbursed.  We are now gathering the current support 
        // collections
        // that are disbursed to a NCP or their designated payee.
        // -------------------------------------------------------------------------------------
        // -- N U M E R A T O R  Part 2  (Amount of Current Support Collected) (
        // OCSE157 Line 25)
        // -------------------------------------------------------------------------------------
        foreach(var item in ReadPaymentRequest())
        {
          ++local.RecordsReadSinceCommit.Count;
          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

          // --Count all non AF, FC, NF, and NC current support collections in 
          // this payment
          // request that are either non adjusted or adjusted after the end of 
          // the reporting
          // period.
          // --the properties on this read each are set to read DISTINCT 
          // collections.
          foreach(var item1 in ReadCollectionCsePersonCsePerson())
          {
            // -------------------------------------------------------------------------------------
            // -- Current Support was collected for the AP/Supported.
            // -- Include case in the Priority 1-4 numerator (Amount of Current 
            // Support Collected)
            // -- This is the same as OCSE157 Line 25.
            // -------------------------------------------------------------------------------------
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.CurrentSupportPaidMthNum =
                  local.Statewide.CurrentSupportPaidMthNum.GetValueOrDefault() +
                  local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.CurrentSupportPaidFfytdNum =
                  local.Statewide.CurrentSupportPaidFfytdNum.
                    GetValueOrDefault() + local
                  .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder4();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidMthNum =
                    local.Local1.Item.G.CurrentSupportPaidMthNum.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.CurrentSupportPaidFfytdNum =
                    local.Local1.Item.G.CurrentSupportPaidFfytdNum.
                      GetValueOrDefault() + local
                    .DashboardAuditData.CollectionAmount.GetValueOrDefault();

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-4(N)" + local
              .ReportingAbbreviation.Text2;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            local.Local2NdRead.CaseNumber = "";

            if (IsEmpty(local.DashboardAuditData.CaseNumber))
            {
              local.UseApSupportedOnly.Flag = "Y";
              UseFnB734DetermineJdFromOrder1();
            }
            else
            {
              MoveDashboardAuditData4(local.DashboardAuditData,
                local.Local2NdRead);
            }

            local.CountCaseWk.Flag = "";

            if (!IsEmpty(local.Local2NdRead.CaseNumber))
            {
              if (ReadCaseAssignmentServiceProvider())
              {
                local.CountCaseWk.Flag = "Y";
              }

              local.DashboardStagingPriority35.Assign(
                local.NullDashboardStagingPriority35);

              if (AsChar(local.CountCaseWk.Flag) == 'Y')
              {
                local.Worker.Assign(local.DashboardAuditData);
                local.DashboardStagingPriority35.AsOfDate =
                  import.ProgramProcessingInfo.ProcessDate;
                local.DashboardStagingPriority35.ReportLevel = "CW";
                local.DashboardStagingPriority35.ReportLevelId =
                  entities.WorkerServiceProvider.UserId;
                local.DashboardStagingPriority35.ReportMonth =
                  import.DashboardAuditData.ReportMonth;
                local.Worker.CollectionAmount =
                  local.DashboardAuditData.CollectionAmount.GetValueOrDefault();
                  
                local.Worker.DashboardPriority = "1-4.1N" + local
                  .ReportingAbbreviation.Text2;
                local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? "";
                local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                local.Worker.WorkerId =
                  local.DashboardStagingPriority35.ReportLevelId;

                switch(local.Period.Count)
                {
                  case 1:
                    // -- Increment In-Month Statewide Level
                    local.DashboardStagingPriority35.CurrentSupportPaidMthNum =
                      local.DashboardStagingPriority35.CurrentSupportPaidMthNum.
                        GetValueOrDefault() + local
                      .Worker.CollectionAmount.GetValueOrDefault();

                    break;
                  case 2:
                    // -- Increment Fiscal Year to date Statewide Level
                    local.DashboardStagingPriority35.
                      CurrentSupportPaidFfytdNum =
                        local.DashboardStagingPriority35.
                        CurrentSupportPaidFfytdNum.GetValueOrDefault() + local
                      .Worker.CollectionAmount.GetValueOrDefault();

                    break;
                  default:
                    break;
                }

                if (AsChar(import.AuditFlag.Flag) == 'Y')
                {
                  // -- Log to the dashboard audit table.
                  UseFnB734CreateDashboardAudit1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }

                if (ReadDashboardStagingPriority1())
                {
                  try
                  {
                    UpdateDashboardStagingPriority2();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                  try
                  {
                    CreateDashboardStagingPriority2();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error creating/updating Dashboard_Staging_Priority_1_2.";
                  UseCabErrorReport();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                local.DashboardStagingPriority35.Assign(
                  local.NullDashboardStagingPriority35);
                local.CountCaseAtty.Flag = "";

                foreach(var item2 in ReadLegalReferralServiceProvider())
                {
                  local.Worker.Assign(local.DashboardAuditData);
                  local.Worker.LegalReferralNumber =
                    entities.LegalReferral.Identifier;
                  local.Worker.LegalReferralDate =
                    entities.LegalReferral.ReferralDate;
                  local.CountCaseAtty.Flag = "Y";

                  if (AsChar(local.CountCaseAtty.Flag) == 'Y')
                  {
                    // -- Case does not owe arrears.  Skip this case.
                    local.DashboardStagingPriority35.AsOfDate =
                      import.ProgramProcessingInfo.ProcessDate;
                    local.DashboardStagingPriority35.ReportLevel = "AT";
                    local.DashboardStagingPriority35.ReportLevelId =
                      entities.ServiceProvider.UserId;
                    local.DashboardStagingPriority35.ReportMonth =
                      import.DashboardAuditData.ReportMonth;
                    local.Worker.CollectionAmount =
                      local.DashboardAuditData.CollectionAmount.
                        GetValueOrDefault();

                    switch(local.Period.Count)
                    {
                      case 1:
                        // -- Increment In-Month Statewide Level
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidMthNum =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidMthNum.GetValueOrDefault() + local
                          .Worker.CollectionAmount.GetValueOrDefault();

                        break;
                      case 2:
                        // -- Increment Fiscal Year to date Statewide Level
                        local.DashboardStagingPriority35.
                          CurrentSupportPaidFfytdNum =
                            local.DashboardStagingPriority35.
                            CurrentSupportPaidFfytdNum.GetValueOrDefault() + local
                          .Worker.CollectionAmount.GetValueOrDefault();

                        break;
                      default:
                        break;
                    }

                    local.Worker.CaseNumber = local.Local2NdRead.CaseNumber ?? ""
                      ;
                    local.Worker.CaseDate = local.Local2NdRead.CaseDate;
                    local.Worker.WorkerId =
                      local.DashboardStagingPriority35.ReportLevelId;
                    local.Worker.DashboardPriority = "1-4.2N" + local
                      .ReportingAbbreviation.Text2;

                    if (AsChar(import.AuditFlag.Flag) == 'Y')
                    {
                      // -- Log to the dashboard audit table.
                      UseFnB734CreateDashboardAudit1();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }

                    if (ReadDashboardStagingPriority1())
                    {
                      try
                      {
                        UpdateDashboardStagingPriority2();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

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
                      try
                      {
                        CreateDashboardStagingPriority2();
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                            break;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                            break;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "Error creating/updating Dashboard_Staging_Priority_1_2.";
                        
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Test5;
                  }
                }
              }
            }

Test5:
            ;
          }

          // ------------------------------------------------------------------------------
          // -- Checkpoint saving all the info needed for restarting.
          // ------------------------------------------------------------------------------
          if (local.RecordsReadSinceCommit.Count >= import
            .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
          {
            // -- Save the Statewide counts.
            if (ReadDashboardStagingPriority4())
            {
              try
              {
                UpdateDashboardStagingPriority9();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
              try
              {
                CreateDashboardStagingPriority6();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // -- Save Judicial District counts.
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (ReadDashboardStagingPriority3())
              {
                try
                {
                  UpdateDashboardStagingPriority8();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
                try
                {
                  CreateDashboardStagingPriority5();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            local.Local1.CheckIndex();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error creating/updating Dashboard_Staging_Priority_1_2.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Checkpoint Info
            // Positions   Value
            // ---------   
            // ------------------------------------
            //  001-080    General Checkpoint Info for PRAD
            //  081-088    Dashboard Priority
            //  089-089    Blank
            //  090-098    Payment Request ID
            //  099-099    "B" (indicating to restart in part 2 for the 
            // numerator)
            //  100-100    Blank
            //  101-101    Local Period Count
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
              "1-04    " + " " + NumberToString
              (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9) + "B " +
              NumberToString(local.Period.Count, 15, 1);
            UseUpdateCheckpointRstAndCommit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Error taking checkpoint.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.RecordsReadSinceCommit.Count = 0;
          }
        }
      }

      // 04/06/17  GVandy  CQ57069  Restart csp number is not cleared out when 
      // transitioning
      // from in month reporting to FYTD reporting.
      // -- Set restart csp number to spaces so that no records are skipped on 
      // the next iteration of the FOR loop.
      local.RestartCsePerson.Number = "";
      local.RestartPaymentRequest.SystemGeneratedIdentifier = 0;
    }

    // ------------------------------------------------------------------------------
    // -- Store final Statewide counts.
    // ------------------------------------------------------------------------------
    if (ReadDashboardStagingPriority4())
    {
      try
      {
        UpdateDashboardStagingPriority9();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
      try
      {
        CreateDashboardStagingPriority6();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Store final Judicial District counts.
    // ------------------------------------------------------------------------------
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      if (ReadDashboardStagingPriority3())
      {
        try
        {
          UpdateDashboardStagingPriority8();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
        try
        {
          CreateDashboardStagingPriority5();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    local.Local1.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Rollup current year Judicial District counts to the Contractor Level.
    // ------------------------------------------------------------------------------
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      // -- Add all the current jd amounts into the correct contractor
      for(local.Contractor.Index = 0; local.Contractor.Index < local
        .Contractor.Count; ++local.Contractor.Index)
      {
        if (!local.Contractor.CheckSize())
        {
          break;
        }

        if (Equal(local.Local1.Item.G.ContractorNumber,
          local.Contractor.Item.Gcontractor.ContractorNumber))
        {
          local.Contractor.Update.Gcontractor.CurrentSupportPaidMthNum =
            local.Contractor.Item.Gcontractor.CurrentSupportPaidMthNum.
              GetValueOrDefault() + local
            .Local1.Item.G.CurrentSupportPaidMthNum.GetValueOrDefault();
          local.Contractor.Update.Gcontractor.CurrentSupportPaidMthDen =
            local.Contractor.Item.Gcontractor.CurrentSupportPaidMthDen.
              GetValueOrDefault() + local
            .Local1.Item.G.CurrentSupportPaidMthDen.GetValueOrDefault();
          local.Contractor.Update.Gcontractor.CurrentSupportPaidFfytdNum =
            local.Contractor.Item.Gcontractor.CurrentSupportPaidFfytdNum.
              GetValueOrDefault() + local
            .Local1.Item.G.CurrentSupportPaidFfytdNum.GetValueOrDefault();
          local.Contractor.Update.Gcontractor.CurrentSupportPaidFfytdDen =
            local.Contractor.Item.Gcontractor.CurrentSupportPaidFfytdDen.
              GetValueOrDefault() + local
            .Local1.Item.G.CurrentSupportPaidFfytdDen.GetValueOrDefault();

          goto Next;
        }
      }

      local.Contractor.CheckIndex();

Next:
      ;
    }

    local.Local1.CheckIndex();

    // ------------------------------------------------------------------------------
    // --                    I M P O R T A N T     N O T E
    // --
    // -- The logic below is rolling up previous year JD data to the Contractor 
    // level
    // -- based on the Contractor hierarchy as it exists TODAY, not the 
    // hierarchy as
    // -- it existed last year.  Therefore, if the contractor hierarchy has 
    // changed
    // -- then the values derived in this logic will not match the reported 
    // Dashboard
    // -- values for the previous year for the contractor.
    // ------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // -- Rollup previous year Judicial District counts to the Contractor level.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority17())
    {
      local.DashboardAuditData.JudicialDistrict =
        entities.PreviousYear.ReportLevelId;
      UseFnB734DeterContractorFromJd();

      // -- Add previous years Current Support values to appropriate contractor.
      for(local.Contractor.Index = 0; local.Contractor.Index < local
        .Contractor.Count; ++local.Contractor.Index)
      {
        if (!local.Contractor.CheckSize())
        {
          break;
        }

        if (Equal(local.Contractor.Item.Gcontractor.ContractorNumber,
          local.Contractor1.Code))
        {
          local.Contractor.Update.Gcontractor.PrevYrCurSupprtPaidNumtr =
            local.Contractor.Item.Gcontractor.PrevYrCurSupprtPaidNumtr.
              GetValueOrDefault() + entities
            .PreviousYear.CurrentSupportPaidFfytdNum.GetValueOrDefault();
          local.Contractor.Update.Gcontractor.PrevYrCurSupprtPaidDenom =
            local.Contractor.Item.Gcontractor.PrevYrCurSupprtPaidDenom.
              GetValueOrDefault() + entities
            .PreviousYear.CurrentSupportPaidFfytdDen.GetValueOrDefault();

          goto ReadEach2;
        }
      }

      local.Contractor.CheckIndex();

ReadEach2:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Store final Contractor counts.
    // ------------------------------------------------------------------------------
    for(local.Contractor.Index = 0; local.Contractor.Index < local
      .Contractor.Count; ++local.Contractor.Index)
    {
      if (!local.Contractor.CheckSize())
      {
        break;
      }

      if (ReadDashboardStagingPriority2())
      {
        try
        {
          UpdateDashboardStagingPriority7();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

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
        try
        {
          CreateDashboardStagingPriority4();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    local.Contractor.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Calculate the Statewide, Judicial District and Contractor Percent of 
    // Current
    // -- Support Paid, Previous Year Percent, and Percent Change from the 
    // Previous Year.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority11())
    {
      MoveDashboardStagingPriority4(entities.DashboardStagingPriority12,
        local.Temp);

      // -- Calculate Current Year Current Support Paid percent.
      if (local.Temp.CurrentSupportPaidFfytdDen.GetValueOrDefault() == 0)
      {
        local.Temp.CurrentSupportPaidFfytdPer = 0;
      }
      else
      {
        local.Temp.CurrentSupportPaidFfytdPer =
          Math.Round(
            local.Temp.CurrentSupportPaidFfytdNum.GetValueOrDefault() /
          local.Temp.CurrentSupportPaidFfytdDen.GetValueOrDefault(), 3,
          MidpointRounding.AwayFromZero);
      }

      if (local.Temp.CurrentSupportPaidMthDen.GetValueOrDefault() == 0)
      {
        local.Temp.CurrentSupportPaidMthPer = 0;
      }
      else
      {
        local.Temp.CurrentSupportPaidMthPer =
          Math.Round(
            local.Temp.CurrentSupportPaidMthNum.GetValueOrDefault() /
          local.Temp.CurrentSupportPaidMthDen.GetValueOrDefault(), 3,
          MidpointRounding.AwayFromZero);
      }

      // -- Read for the previous year Current Support Paid values for all but 
      // the contractor level.
      // -- The contractor level previous year values were calculated and stored
      // earlier.
      if (!Equal(entities.DashboardStagingPriority12.ReportLevel, "XJ"))
      {
        if (ReadDashboardStagingPriority5())
        {
          local.Temp.PrevYrCurSupprtPaidNumtr =
            entities.PreviousYear.CurrentSupportPaidFfytdNum;
          local.Temp.PrevYrCurSupprtPaidDenom =
            entities.PreviousYear.CurrentSupportPaidFfytdDen;
        }
        else
        {
          local.Temp.PrevYrCurSupprtPaidNumtr = 0;
          local.Temp.PrevYrCurSupprtPaidDenom = 0;
        }
      }

      // -- Calculate Previous Year Current Support Paid percent.
      if (local.Temp.PrevYrCurSupprtPaidDenom.GetValueOrDefault() == 0)
      {
        local.Temp.CurSupprtPdPrevYrPct = 0;
      }
      else
      {
        local.Temp.CurSupprtPdPrevYrPct =
          Math.Round(
            local.Temp.PrevYrCurSupprtPaidNumtr.GetValueOrDefault() /
          local.Temp.PrevYrCurSupprtPaidDenom.GetValueOrDefault(), 3,
          MidpointRounding.AwayFromZero);
      }

      // -- Calculate percent change between Current Year Current Support 
      // percent and Previous Year Current Support percent.
      if (local.Temp.CurSupprtPdPrevYrPct.GetValueOrDefault() == 0)
      {
        local.Temp.PctChgBtwenYrsCurSuptPd = 0;
      }
      else
      {
        local.Temp.PctChgBtwenYrsCurSuptPd =
          Math.Round((
            local.Temp.CurrentSupportPaidFfytdPer.GetValueOrDefault() - local
          .Temp.CurSupprtPdPrevYrPct.GetValueOrDefault()) /
          local.Temp.CurSupprtPdPrevYrPct.GetValueOrDefault(), 3,
          MidpointRounding.AwayFromZero);
      }

      try
      {
        UpdateDashboardStagingPriority10();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    foreach(var item in ReadDashboardStagingPriority6())
    {
      // this is for case workers and attorneies
      local.DashboardStagingPriority35.Assign(
        entities.DashboardStagingPriority35);

      // -- Calculate Current Year Current Support Paid percent.
      if (local.DashboardStagingPriority35.CurrentSupportPaidFfytdDen.
        GetValueOrDefault() == 0)
      {
        local.DashboardStagingPriority35.CurrentSupportPaidFfytdPer = 0;
      }
      else
      {
        local.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
          local.DashboardStagingPriority35.CurrentSupportPaidFfytdNum.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.CurrentSupportPaidFfytdDen.
            GetValueOrDefault();
      }

      if (local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
        GetValueOrDefault() == 0)
      {
        local.DashboardStagingPriority35.CurrentSupportPaidMthPer = 0;
      }
      else
      {
        local.DashboardStagingPriority35.CurrentSupportPaidMthPer =
          local.DashboardStagingPriority35.CurrentSupportPaidMthNum.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.CurrentSupportPaidMthDen.
            GetValueOrDefault();
      }

      try
      {
        UpdateDashboardStagingPriority3();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PrevRank.CurrentSupportPaidMthPer = 0;
    local.Temp.CurrentSupportPaidMthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority15())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority12.CurrentSupportPaidMthPer,
        local.PrevRank.CurrentSupportPaidMthPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CurrentSupportPaidMthRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority13();
        MoveDashboardStagingPriority5(entities.DashboardStagingPriority12,
          local.PrevRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PrevRank.CurrentSupportPaidMthPer = 0;
    local.Temp.CurrentSupportPaidMthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority16())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority12.CurrentSupportPaidMthPer,
        local.PrevRank.CurrentSupportPaidMthPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CurrentSupportPaidMthRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority13();
        MoveDashboardStagingPriority5(entities.DashboardStagingPriority12,
          local.PrevRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PreviousRank.CurrentSupportPaidMthPer = 0;
    local.DashboardStagingPriority35.CurrentSupportPaidMthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Case Worker Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority8())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority35.CurrentSupportPaidMthPer,
        local.PreviousRank.CurrentSupportPaidMthPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority6();
        MoveDashboardStagingPriority35(entities.DashboardStagingPriority35,
          local.PreviousRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PreviousRank.CurrentSupportPaidMthPer = 0;
    local.DashboardStagingPriority35.CurrentSupportPaidMthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Attorney Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority7())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority35.CurrentSupportPaidMthPer,
        local.PreviousRank.CurrentSupportPaidMthPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority6();
        MoveDashboardStagingPriority35(entities.DashboardStagingPriority35,
          local.PreviousRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PrevRank.CurrentSupportPaidFfytdPer = 0;
    local.Temp.CurrentSupportPaidFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority13())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer,
        local.PrevRank.CurrentSupportPaidFfytdPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CurrentSupportPaidFfytdRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority11();
        MoveDashboardStagingPriority5(entities.DashboardStagingPriority12,
          local.PrevRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PrevRank.CurrentSupportPaidFfytdPer = 0;
    local.Temp.CurrentSupportPaidFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority14())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer,
        local.PrevRank.CurrentSupportPaidFfytdPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CurrentSupportPaidFfytdRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority11();
        MoveDashboardStagingPriority5(entities.DashboardStagingPriority12,
          local.PrevRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PreviousRank.CurrentSupportPaidFfytdPer = 0;
    local.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Case Worker Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority8())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer,
        local.PreviousRank.CurrentSupportPaidFfytdPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        MoveDashboardStagingPriority35(entities.DashboardStagingPriority35,
          local.PreviousRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PreviousRank.CurrentSupportPaidFfytdPer = 0;
    local.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Attorney Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority7())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer,
        local.PreviousRank.CurrentSupportPaidFfytdPer.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        MoveDashboardStagingPriority35(entities.DashboardStagingPriority35,
          local.PreviousRank);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint for restarting at the next priority.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "Y";

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInfo =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-05    ";
      
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveDashboardAuditData1(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.DashboardPriority = source.DashboardPriority;
    target.RunNumber = source.RunNumber;
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.WorkerId = source.WorkerId;
    target.CaseNumber = source.CaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.PayorCspNumber = source.PayorCspNumber;
    target.SuppCspNumber = source.SuppCspNumber;
    target.Fte = source.Fte;
    target.CollectionAmount = source.CollectionAmount;
    target.CollAppliedToCd = source.CollAppliedToCd;
    target.CollectionCreatedDate = source.CollectionCreatedDate;
    target.CollectionType = source.CollectionType;
    target.DebtBalanceDue = source.DebtBalanceDue;
    target.DebtDueDate = source.DebtDueDate;
    target.DebtType = source.DebtType;
    target.LegalActionDate = source.LegalActionDate;
    target.LegalReferralDate = source.LegalReferralDate;
    target.LegalReferralNumber = source.LegalReferralNumber;
    target.DaysReported = source.DaysReported;
    target.VerifiedDate = source.VerifiedDate;
    target.CaseDate = source.CaseDate;
    target.ReviewDate = source.ReviewDate;
  }

  private static void MoveDashboardAuditData2(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveDashboardAuditData3(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.CaseNumber = source.CaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveDashboardAuditData4(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CaseDate = source.CaseDate;
  }

  private static void MoveDashboardStagingPriority1(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.AsOfDate = source.AsOfDate;
    target.CasesPayingArrearsNumerator = source.CasesPayingArrearsNumerator;
    target.CasesPayingArrearsDenominator = source.CasesPayingArrearsDenominator;
    target.CasesPayingArrearsPercent = source.CasesPayingArrearsPercent;
    target.CasesPayingArrearsRank = source.CasesPayingArrearsRank;
    target.PrvYrCasesPaidArrearsNumtr = source.PrvYrCasesPaidArrearsNumtr;
    target.PrvYrCasesPaidArrearsDenom = source.PrvYrCasesPaidArrearsDenom;
    target.CasesPayArrearsPrvYrPct = source.CasesPayArrearsPrvYrPct;
    target.PctChgBtwenYrsCasesPayArrs = source.PctChgBtwenYrsCasesPayArrs;
  }

  private static void MoveDashboardStagingPriority2(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.AsOfDate = source.AsOfDate;
    target.CurrentSupportPaidMthNum = source.CurrentSupportPaidMthNum;
    target.CurrentSupportPaidMthDen = source.CurrentSupportPaidMthDen;
    target.CurrentSupportPaidMthPer = source.CurrentSupportPaidMthPer;
    target.CurrentSupportPaidMthRnk = source.CurrentSupportPaidMthRnk;
    target.CurrentSupportPaidFfytdNum = source.CurrentSupportPaidFfytdNum;
    target.CurrentSupportPaidFfytdDen = source.CurrentSupportPaidFfytdDen;
    target.CurrentSupportPaidFfytdPer = source.CurrentSupportPaidFfytdPer;
    target.CurrentSupportPaidFfytdRnk = source.CurrentSupportPaidFfytdRnk;
    target.ContractorNumber = source.ContractorNumber;
    target.PrevYrCurSupprtPaidNumtr = source.PrevYrCurSupprtPaidNumtr;
    target.PrevYrCurSupprtPaidDenom = source.PrevYrCurSupprtPaidDenom;
    target.CurSupprtPdPrevYrPct = source.CurSupprtPdPrevYrPct;
    target.PctChgBtwenYrsCurSuptPd = source.PctChgBtwenYrsCurSuptPd;
  }

  private static void MoveDashboardStagingPriority3(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.AsOfDate = source.AsOfDate;
    target.CurrentSupportPaidMthNum = source.CurrentSupportPaidMthNum;
    target.CurrentSupportPaidMthDen = source.CurrentSupportPaidMthDen;
    target.CurrentSupportPaidMthPer = source.CurrentSupportPaidMthPer;
    target.CurrentSupportPaidMthRnk = source.CurrentSupportPaidMthRnk;
    target.CurrentSupportPaidFfytdNum = source.CurrentSupportPaidFfytdNum;
    target.CurrentSupportPaidFfytdDen = source.CurrentSupportPaidFfytdDen;
    target.CurrentSupportPaidFfytdPer = source.CurrentSupportPaidFfytdPer;
    target.CurrentSupportPaidFfytdRnk = source.CurrentSupportPaidFfytdRnk;
    target.PrevYrCurSupprtPaidNumtr = source.PrevYrCurSupprtPaidNumtr;
    target.PrevYrCurSupprtPaidDenom = source.PrevYrCurSupprtPaidDenom;
    target.CurSupprtPdPrevYrPct = source.CurSupprtPdPrevYrPct;
    target.PctChgBtwenYrsCurSuptPd = source.PctChgBtwenYrsCurSuptPd;
  }

  private static void MoveDashboardStagingPriority4(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.CurrentSupportPaidMthNum = source.CurrentSupportPaidMthNum;
    target.CurrentSupportPaidMthDen = source.CurrentSupportPaidMthDen;
    target.CurrentSupportPaidMthPer = source.CurrentSupportPaidMthPer;
    target.CurrentSupportPaidMthRnk = source.CurrentSupportPaidMthRnk;
    target.CurrentSupportPaidFfytdNum = source.CurrentSupportPaidFfytdNum;
    target.CurrentSupportPaidFfytdDen = source.CurrentSupportPaidFfytdDen;
    target.CurrentSupportPaidFfytdPer = source.CurrentSupportPaidFfytdPer;
    target.CurrentSupportPaidFfytdRnk = source.CurrentSupportPaidFfytdRnk;
    target.PrevYrCurSupprtPaidNumtr = source.PrevYrCurSupprtPaidNumtr;
    target.PrevYrCurSupprtPaidDenom = source.PrevYrCurSupprtPaidDenom;
    target.CurSupprtPdPrevYrPct = source.CurSupprtPdPrevYrPct;
    target.PctChgBtwenYrsCurSuptPd = source.PctChgBtwenYrsCurSuptPd;
  }

  private static void MoveDashboardStagingPriority5(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.CurrentSupportPaidMthPer = source.CurrentSupportPaidMthPer;
    target.CurrentSupportPaidFfytdPer = source.CurrentSupportPaidFfytdPer;
  }

  private static void MoveDashboardStagingPriority35(
    DashboardStagingPriority35 source, DashboardStagingPriority35 target)
  {
    target.CurrentSupportPaidFfytdPer = source.CurrentSupportPaidFfytdPer;
    target.CurrentSupportPaidMthPer = source.CurrentSupportPaidMthPer;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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

  private void UseFnB734CreateDashboardAudit1()
  {
    var useImport = new FnB734CreateDashboardAudit.Import();
    var useExport = new FnB734CreateDashboardAudit.Export();

    useImport.DashboardAuditData.Assign(local.Worker);

    Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnB734CreateDashboardAudit2()
  {
    var useImport = new FnB734CreateDashboardAudit.Import();
    var useExport = new FnB734CreateDashboardAudit.Export();

    useImport.DashboardAuditData.Assign(local.DashboardAuditData);

    Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnB734DeterContractorFromJd()
  {
    var useImport = new FnB734DeterContractorFromJd.Import();
    var useExport = new FnB734DeterContractorFromJd.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.DashboardAuditData.JudicialDistrict =
      local.DashboardAuditData.JudicialDistrict;

    Call(FnB734DeterContractorFromJd.Execute, useImport, useExport);

    MoveCseOrganization(useExport.Contractor, local.Contractor1);
  }

  private void UseFnB734DetermineJdFromOrder1()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.UseApSupportedOnly.Flag = local.UseApSupportedOnly.Flag;
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.ReportEndDate.Date = local.ReportEndDate.Date;

    Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    local.Local2NdRead.CaseNumber = useExport.DashboardAuditData.CaseNumber;
  }

  private void UseFnB734DetermineJdFromOrder2()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentDebt.Assign(entities.Debt);
    useImport.UseApSupportedOnly.Flag = local.UseApSupportedOnly.Flag;
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.ReportEndDate.Date = local.ReportEndDate.Date;

    Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    local.Local2NdRead.CaseNumber = useExport.DashboardAuditData.CaseNumber;
  }

  private void UseFnB734DetermineJdFromOrder3()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.ReportEndDate.Date = local.ReportEndDate.Date;

    Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    local.Local2NdRead.CaseNumber = useExport.DashboardAuditData.CaseNumber;
  }

  private void UseFnB734DetermineJdFromOrder4()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.ReportEndDate.Date = local.ReportEndDate.Date;

    Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnB734DetermineJdFromOrder5()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentDebt.Assign(entities.Debt);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.ReportEndDate.Date = local.ReportEndDate.Date;

    Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnGetEarliestCaseRole4Pers()
  {
    var useImport = new FnGetEarliestCaseRole4Pers.Import();
    var useExport = new FnGetEarliestCaseRole4Pers.Export();

    useImport.CsePerson.Number = entities.Supp.Number;

    Call(FnGetEarliestCaseRole4Pers.Execute, useImport, useExport);

    local.Earliest.StartDate = useExport.Earliest.StartDate;
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority1()
  {
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var currentSupportPaidFfytdDen =
      local.DashboardStagingPriority35.CurrentSupportPaidFfytdDen.
        GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(command, "curSupPdYtdNum", param);
        db.SetNullableDecimal(command, "curSupPdYtdPer", param);
        db.SetNullableInt32(command, "curSupPdYtdRnk", 0);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "curSupPdMthPer", param);
        db.SetNullableInt32(command, "curSupPdMthRnk", 0);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk = 0;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthNum = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthPer = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void CreateDashboardStagingPriority2()
  {
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var currentSupportPaidFfytdNum =
      local.DashboardStagingPriority35.CurrentSupportPaidFfytdNum.
        GetValueOrDefault();
    var currentSupportPaidMthNum =
      local.DashboardStagingPriority35.CurrentSupportPaidMthNum.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableDecimal(command, "curSupPdYtdDen", param);
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(command, "curSupPdYtdPer", param);
        db.SetNullableInt32(command, "curSupPdYtdRnk", 0);
        db.SetNullableDecimal(command, "curSupPdMthDen", param);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(command, "curSupPdMthPer", param);
        db.SetNullableInt32(command, "curSupPdMthRnk", 0);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk = 0;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthDen = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthPer = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void CreateDashboardStagingPriority3()
  {
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var currentSupportPaidFfytdDen =
      local.DashboardStagingPriority35.CurrentSupportPaidFfytdDen.
        GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(command, "curSupPdYtdNum", param);
        db.SetNullableDecimal(command, "curSupPdYtdPer", param);
        db.SetNullableInt32(command, "curSupPdYtdRnk", 0);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "curSupPdMthPer", param);
        db.SetNullableInt32(command, "curSupPdMthRnk", 0);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk = 0;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthNum = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthPer = param;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void CreateDashboardStagingPriority4()
  {
    var reportMonth = local.Contractor.Item.Gcontractor.ReportMonth;
    var reportLevel = local.Contractor.Item.Gcontractor.ReportLevel;
    var reportLevelId = local.Contractor.Item.Gcontractor.ReportLevelId;
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var param = 0M;
    var currentSupportPaidMthNum =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidMthNum.
        GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidMthDen.
        GetValueOrDefault();
    var currentSupportPaidFfytdNum =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidFfytdNum.
        GetValueOrDefault();
    var currentSupportPaidFfytdDen =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidFfytdDen.
        GetValueOrDefault();
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";
    var prevYrCurSupprtPaidNumtr =
      local.Contractor.Item.Gcontractor.PrevYrCurSupprtPaidNumtr.
        GetValueOrDefault();
    var prevYrCurSupprtPaidDenom =
      local.Contractor.Item.Gcontractor.PrevYrCurSupprtPaidDenom.
        GetValueOrDefault();
    var curSupprtPdPrevYrPct =
      local.Contractor.Item.Gcontractor.CurSupprtPdPrevYrPct.
        GetValueOrDefault();
    var pctChgBtwenYrsCurSuptPd =
      local.Contractor.Item.Gcontractor.PctChgBtwenYrsCurSuptPd.
        GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(command, "casPayingArrNum", 0);
        db.SetNullableInt32(command, "casPayingArrDen", 0);
        db.SetNullableDecimal(command, "casPayingArrPer", param);
        db.SetNullableInt32(command, "casPayingArrRnk", 0);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(command, "curSupPdMthPer", param);
        db.SetNullableInt32(command, "curSupPdMthRnk", 0);
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(command, "curSupPdYtdPer", param);
        db.SetNullableInt32(command, "curSupPdYtdRnk", 0);
        db.SetNullableDecimal(command, "collYtdToPriMo", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableDecimal(
          command, "pvYrSupPdNumtr", prevYrCurSupprtPaidNumtr);
        db.SetNullableDecimal(
          command, "pvYrSupPdDenom", prevYrCurSupprtPaidDenom);
        db.SetNullableDecimal(command, "prvYrCSPdPct", curSupprtPdPrevYrPct);
        db.
          SetNullableDecimal(command, "pctChgByrCsPd", pctChgBtwenYrsCurSuptPd);
          
        db.SetNullableInt32(command, "prvYrPdArNumtr", 0);
        db.SetNullableInt32(command, "prvYrPdArDenom", 0);
        db.SetNullableDecimal(command, "payArPrvYrPct", param);
        db.SetNullableDecimal(command, "pctChgByrArsPd", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator = 0;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator = 0;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent = param;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank = 0;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthPer = param;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk = 0;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer = param;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk = 0;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
      prevYrCurSupprtPaidNumtr;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
      prevYrCurSupprtPaidDenom;
    entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
      curSupprtPdPrevYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
      pctChgBtwenYrsCurSuptPd;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr = 0;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom = 0;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority5()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var currentSupportPaidMthNum =
      local.Local1.Item.G.CurrentSupportPaidMthNum.GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.Local1.Item.G.CurrentSupportPaidMthDen.GetValueOrDefault();
    var currentSupportPaidMthPer =
      local.Local1.Item.G.CurrentSupportPaidMthPer.GetValueOrDefault();
    var currentSupportPaidMthRnk =
      local.Local1.Item.G.CurrentSupportPaidMthRnk.GetValueOrDefault();
    var currentSupportPaidFfytdNum =
      local.Local1.Item.G.CurrentSupportPaidFfytdNum.GetValueOrDefault();
    var currentSupportPaidFfytdDen =
      local.Local1.Item.G.CurrentSupportPaidFfytdDen.GetValueOrDefault();
    var currentSupportPaidFfytdPer =
      local.Local1.Item.G.CurrentSupportPaidFfytdPer.GetValueOrDefault();
    var currentSupportPaidFfytdRnk =
      local.Local1.Item.G.CurrentSupportPaidFfytdRnk.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(command, "casPayingArrNum", 0);
        db.SetNullableInt32(command, "casPayingArrDen", 0);
        db.SetNullableDecimal(command, "casPayingArrPer", param);
        db.SetNullableInt32(command, "casPayingArrRnk", 0);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
          
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetNullableDecimal(command, "collYtdToPriMo", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
        db.SetNullableDecimal(command, "pvYrSupPdNumtr", param);
        db.SetNullableDecimal(command, "pvYrSupPdDenom", param);
        db.SetNullableDecimal(command, "prvYrCSPdPct", param);
        db.SetNullableDecimal(command, "pctChgByrCsPd", param);
        db.SetNullableInt32(command, "prvYrPdArNumtr", 0);
        db.SetNullableInt32(command, "prvYrPdArDenom", 0);
        db.SetNullableDecimal(command, "payArPrvYrPct", param);
        db.SetNullableDecimal(command, "pctChgByrArsPd", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator = 0;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator = 0;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent = param;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank = 0;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr = param;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom = param;
    entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd = param;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr = 0;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom = 0;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority6()
  {
    var reportMonth = local.Statewide.ReportMonth;
    var reportLevel = local.Statewide.ReportLevel;
    var reportLevelId = local.Statewide.ReportLevelId;
    var asOfDate = local.Statewide.AsOfDate;
    var param = 0M;
    var currentSupportPaidMthNum =
      local.Statewide.CurrentSupportPaidMthNum.GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.Statewide.CurrentSupportPaidMthDen.GetValueOrDefault();
    var currentSupportPaidMthPer =
      local.Statewide.CurrentSupportPaidMthPer.GetValueOrDefault();
    var currentSupportPaidMthRnk =
      local.Statewide.CurrentSupportPaidMthRnk.GetValueOrDefault();
    var currentSupportPaidFfytdNum =
      local.Statewide.CurrentSupportPaidFfytdNum.GetValueOrDefault();
    var currentSupportPaidFfytdDen =
      local.Statewide.CurrentSupportPaidFfytdDen.GetValueOrDefault();
    var currentSupportPaidFfytdPer =
      local.Statewide.CurrentSupportPaidFfytdPer.GetValueOrDefault();
    var currentSupportPaidFfytdRnk =
      local.Statewide.CurrentSupportPaidFfytdRnk.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority6",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(command, "casPayingArrNum", 0);
        db.SetNullableInt32(command, "casPayingArrDen", 0);
        db.SetNullableDecimal(command, "casPayingArrPer", param);
        db.SetNullableInt32(command, "casPayingArrRnk", 0);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
          
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetNullableDecimal(command, "collYtdToPriMo", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
        db.SetNullableDecimal(command, "pvYrSupPdNumtr", param);
        db.SetNullableDecimal(command, "pvYrSupPdDenom", param);
        db.SetNullableDecimal(command, "prvYrCSPdPct", param);
        db.SetNullableDecimal(command, "pctChgByrCsPd", param);
        db.SetNullableInt32(command, "prvYrPdArNumtr", 0);
        db.SetNullableInt32(command, "prvYrPdArDenom", 0);
        db.SetNullableDecimal(command, "payArPrvYrPct", param);
        db.SetNullableDecimal(command, "pctChgByrArsPd", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator = 0;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator = 0;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent = param;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank = 0;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr = param;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom = param;
    entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd = param;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr = 0;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom = 0;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private bool ReadCaseAssignmentServiceProvider()
  {
    entities.WorkerCaseAssignment.Populated = false;
    entities.WorkerServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", local.Local2NdRead.CaseNumber ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.WorkerCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.WorkerCaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.WorkerCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.WorkerCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.WorkerCaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.WorkerCaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.WorkerCaseAssignment.OspCode = db.GetString(reader, 6);
        entities.WorkerCaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.WorkerCaseAssignment.CasNo = db.GetString(reader, 8);
        entities.WorkerServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 9);
        entities.WorkerServiceProvider.UserId = db.GetString(reader, 10);
        entities.WorkerCaseAssignment.Populated = true;
        entities.WorkerServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.Collection.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCollectionCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDate(
          command, "date1", local.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          local.PreviousMonthStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.ReportEndDate.Timestamp.GetValueOrDefault());
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
        entities.ApCsePerson.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;
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

  private IEnumerable<bool> ReadCollectionCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.Collection.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCollectionCsePerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDate(
          command, "date1", local.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          local.PreviousMonthStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.ReportEndDate.Timestamp.GetValueOrDefault());
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
        entities.ApCsePerson.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;
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

  private IEnumerable<bool> ReadCollectionCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.Collection.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCollectionCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDateTime(
          command, "createdTmst1",
          local.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          local.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", local.ReportEndDate.Date.GetValueOrDefault());
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
        entities.ApCsePerson.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;
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

  private IEnumerable<bool> ReadCollectionCsePersonCsePerson()
  {
    entities.Supp.Populated = false;
    entities.Collection.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCollectionCsePersonCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          local.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", local.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
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
        entities.ApCsePerson.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Supp.Number = db.GetString(reader, 20);
        entities.Supp.Populated = true;
        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;
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

  private IEnumerable<bool> ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return ReadEach("ReadCseOrganization",
      null,
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonSupported()
  {
    entities.Supp.Populated = false;
    entities.Supported.Populated = false;

    return ReadEach("ReadCsePersonSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.RestartCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supp.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

        return true;
      });
  }

  private bool ReadDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", local.DashboardStagingPriority35.ReportMonth);
          
        db.SetString(
          command, "reportLevel", local.DashboardStagingPriority35.ReportLevel);
          
        db.SetString(
          command, "reportLevelId",
          local.DashboardStagingPriority35.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority10()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority10",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority11()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority11",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority13()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority13",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority14()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority14",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority15()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority15",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority16()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority16",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority17()
  {
    entities.PreviousYear.Populated = false;

    return ReadEach("ReadDashboardStagingPriority17",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.PreviousYear.ReportMonth);
      },
      (db, reader) =>
      {
        entities.PreviousYear.ReportMonth = db.GetInt32(reader, 0);
        entities.PreviousYear.ReportLevel = db.GetString(reader, 1);
        entities.PreviousYear.ReportLevelId = db.GetString(reader, 2);
        entities.PreviousYear.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 3);
        entities.PreviousYear.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 4);
        entities.PreviousYear.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 5);
        entities.PreviousYear.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 6);
        entities.PreviousYear.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 7);
        entities.PreviousYear.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 8);
        entities.PreviousYear.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority18()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority18",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority19()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority19",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private bool ReadDashboardStagingPriority2()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          local.Contractor.Item.Gcontractor.ReportMonth);
        db.SetString(
          command, "reportLevel",
          local.Contractor.Item.Gcontractor.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          local.Contractor.Item.Gcontractor.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority3()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Local1.Item.G.ReportMonth);
        db.SetString(command, "reportLevel", local.Local1.Item.G.ReportLevel);
        db.
          SetString(command, "reportLevelId", local.Local1.Item.G.ReportLevelId);
          
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority4()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Statewide.ReportMonth);
        db.SetString(command, "reportLevel", local.Statewide.ReportLevel);
        db.SetString(command, "reportLevelId", local.Statewide.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority5()
  {
    entities.PreviousYear.Populated = false;

    return Read("ReadDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
        db.SetInt32(command, "reportMonth", local.PreviousYear.ReportMonth);
      },
      (db, reader) =>
      {
        entities.PreviousYear.ReportMonth = db.GetInt32(reader, 0);
        entities.PreviousYear.ReportLevel = db.GetString(reader, 1);
        entities.PreviousYear.ReportLevelId = db.GetString(reader, 2);
        entities.PreviousYear.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 3);
        entities.PreviousYear.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 4);
        entities.PreviousYear.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 5);
        entities.PreviousYear.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 6);
        entities.PreviousYear.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 7);
        entities.PreviousYear.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 8);
        entities.PreviousYear.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority6()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return ReadEach("ReadDashboardStagingPriority6",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority7()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return ReadEach("ReadDashboardStagingPriority7",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority8()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return ReadEach("ReadDashboardStagingPriority8",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority9()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority9",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 16);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 21);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst",
          local.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDebtObligationObligationTypeDebtDetailCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.ApCsePerson.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetailCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          local.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDate(
          command, "date1", local.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.ReportEndDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.Earliest.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 10);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.ApCsePerson.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralServiceProvider()
  {
    entities.LegalReferral.Populated = false;
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadLegalReferralServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.Local2NdRead.CaseNumber ?? "");
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "statusDate", import.ReportEndDate.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 12);
        entities.ServiceProvider.UserId = db.GetString(reader, 13);
        entities.LegalReferral.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "paymentRequestId",
          local.RestartPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PaymentRequest.Classification = db.GetString(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.PaymentRequest.InterstateInd = db.GetNullableString(reader, 7);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 8);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var currentSupportPaidFfytdDen =
      entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen.
        GetValueOrDefault() +
      local.DashboardStagingPriority35.CurrentSupportPaidFfytdDen.
        GetValueOrDefault();
    var currentSupportPaidMthDen =
      entities.DashboardStagingPriority35.CurrentSupportPaidMthDen.
        GetValueOrDefault() +
      local.DashboardStagingPriority35.CurrentSupportPaidMthDen.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority10()
  {
    var currentSupportPaidMthPer =
      local.Temp.CurrentSupportPaidMthPer.GetValueOrDefault();
    var currentSupportPaidFfytdPer =
      local.Temp.CurrentSupportPaidFfytdPer.GetValueOrDefault();
    var prevYrCurSupprtPaidNumtr =
      local.Temp.PrevYrCurSupprtPaidNumtr.GetValueOrDefault();
    var prevYrCurSupprtPaidDenom =
      local.Temp.PrevYrCurSupprtPaidDenom.GetValueOrDefault();
    var curSupprtPdPrevYrPct =
      local.Temp.CurSupprtPdPrevYrPct.GetValueOrDefault();
    var pctChgBtwenYrsCurSuptPd =
      local.Temp.PctChgBtwenYrsCurSuptPd.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority10",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableDecimal(
          command, "pvYrSupPdNumtr", prevYrCurSupprtPaidNumtr);
        db.SetNullableDecimal(
          command, "pvYrSupPdDenom", prevYrCurSupprtPaidDenom);
        db.SetNullableDecimal(command, "prvYrCSPdPct", curSupprtPdPrevYrPct);
        db.
          SetNullableDecimal(command, "pctChgByrCsPd", pctChgBtwenYrsCurSuptPd);
          
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
      prevYrCurSupprtPaidNumtr;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
      prevYrCurSupprtPaidDenom;
    entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
      curSupprtPdPrevYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
      pctChgBtwenYrsCurSuptPd;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority11()
  {
    var currentSupportPaidFfytdRnk =
      local.Temp.CurrentSupportPaidFfytdRnk.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority11",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority13()
  {
    var currentSupportPaidMthRnk =
      local.Temp.CurrentSupportPaidMthRnk.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority13",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
          
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var currentSupportPaidFfytdNum =
      entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum.
        GetValueOrDefault() +
      local.DashboardStagingPriority35.CurrentSupportPaidFfytdNum.
        GetValueOrDefault();
    var currentSupportPaidMthNum =
      entities.DashboardStagingPriority35.CurrentSupportPaidMthNum.
        GetValueOrDefault() +
      local.DashboardStagingPriority35.CurrentSupportPaidMthNum.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var currentSupportPaidFfytdPer =
      local.DashboardStagingPriority35.CurrentSupportPaidFfytdPer.
        GetValueOrDefault();
    var currentSupportPaidMthPer =
      local.DashboardStagingPriority35.CurrentSupportPaidMthPer.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var currentSupportPaidFfytdDen = 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdDen);
        db.SetNullableInt32(command, "curSupPdYtdRnk", 0);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidFfytdDen);
        db.SetNullableInt32(command, "curSupPdMthRnk", 0);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk = 0;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthPer =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var currentSupportPaidFfytdRnk =
      local.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority6()
  {
    var currentSupportPaidMthRnk =
      local.DashboardStagingPriority35.CurrentSupportPaidMthRnk.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority6",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
          
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority7()
  {
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var currentSupportPaidMthNum =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidMthNum.
        GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidMthDen.
        GetValueOrDefault();
    var currentSupportPaidFfytdNum =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidFfytdNum.
        GetValueOrDefault();
    var currentSupportPaidFfytdDen =
      local.Contractor.Item.Gcontractor.CurrentSupportPaidFfytdDen.
        GetValueOrDefault();
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";
    var prevYrCurSupprtPaidNumtr =
      local.Contractor.Item.Gcontractor.PrevYrCurSupprtPaidNumtr.
        GetValueOrDefault();
    var prevYrCurSupprtPaidDenom =
      local.Contractor.Item.Gcontractor.PrevYrCurSupprtPaidDenom.
        GetValueOrDefault();
    var curSupprtPdPrevYrPct =
      local.Contractor.Item.Gcontractor.CurSupprtPdPrevYrPct.
        GetValueOrDefault();
    var pctChgBtwenYrsCurSuptPd =
      local.Contractor.Item.Gcontractor.PctChgBtwenYrsCurSuptPd.
        GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority7",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableDecimal(
          command, "pvYrSupPdNumtr", prevYrCurSupprtPaidNumtr);
        db.SetNullableDecimal(
          command, "pvYrSupPdDenom", prevYrCurSupprtPaidDenom);
        db.SetNullableDecimal(command, "prvYrCSPdPct", curSupprtPdPrevYrPct);
        db.
          SetNullableDecimal(command, "pctChgByrCsPd", pctChgBtwenYrsCurSuptPd);
          
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
      prevYrCurSupprtPaidNumtr;
    entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
      prevYrCurSupprtPaidDenom;
    entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
      curSupprtPdPrevYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
      pctChgBtwenYrsCurSuptPd;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority8()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var currentSupportPaidMthNum =
      local.Local1.Item.G.CurrentSupportPaidMthNum.GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.Local1.Item.G.CurrentSupportPaidMthDen.GetValueOrDefault();
    var currentSupportPaidMthPer =
      local.Local1.Item.G.CurrentSupportPaidMthPer.GetValueOrDefault();
    var currentSupportPaidMthRnk =
      local.Local1.Item.G.CurrentSupportPaidMthRnk.GetValueOrDefault();
    var currentSupportPaidFfytdNum =
      local.Local1.Item.G.CurrentSupportPaidFfytdNum.GetValueOrDefault();
    var currentSupportPaidFfytdDen =
      local.Local1.Item.G.CurrentSupportPaidFfytdDen.GetValueOrDefault();
    var currentSupportPaidFfytdPer =
      local.Local1.Item.G.CurrentSupportPaidFfytdPer.GetValueOrDefault();
    var currentSupportPaidFfytdRnk =
      local.Local1.Item.G.CurrentSupportPaidFfytdRnk.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority8",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
          
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority9()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var currentSupportPaidMthNum =
      local.Statewide.CurrentSupportPaidMthNum.GetValueOrDefault();
    var currentSupportPaidMthDen =
      local.Statewide.CurrentSupportPaidMthDen.GetValueOrDefault();
    var currentSupportPaidMthPer =
      local.Statewide.CurrentSupportPaidMthPer.GetValueOrDefault();
    var currentSupportPaidMthRnk =
      local.Statewide.CurrentSupportPaidMthRnk.GetValueOrDefault();
    var currentSupportPaidFfytdNum =
      local.Statewide.CurrentSupportPaidFfytdNum.GetValueOrDefault();
    var currentSupportPaidFfytdDen =
      local.Statewide.CurrentSupportPaidFfytdDen.GetValueOrDefault();
    var currentSupportPaidFfytdPer =
      local.Statewide.CurrentSupportPaidFfytdPer.GetValueOrDefault();
    var currentSupportPaidFfytdRnk =
      local.Statewide.CurrentSupportPaidFfytdRnk.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority9",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
          
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
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
    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of FiscalYearStart.
    /// </summary>
    [JsonPropertyName("fiscalYearStart")]
    public DateWorkArea FiscalYearStart
    {
      get => fiscalYearStart ??= new();
      set => fiscalYearStart = value;
    }

    /// <summary>
    /// A value of AuditFlag.
    /// </summary>
    [JsonPropertyName("auditFlag")]
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    /// <summary>
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private DashboardAuditData dashboardAuditData;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private DateWorkArea fiscalYearStart;
    private Common auditFlag;
    private Ocse157Verification cq66220EffectiveFy;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public DashboardStagingPriority12 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority12 g;
    }

    /// <summary>A ContractorGroup group.</summary>
    [Serializable]
    public class ContractorGroup
    {
      /// <summary>
      /// A value of Gcontractor.
      /// </summary>
      [JsonPropertyName("gcontractor")]
      public DashboardStagingPriority12 Gcontractor
      {
        get => gcontractor ??= new();
        set => gcontractor = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority12 gcontractor;
    }

    /// <summary>
    /// A value of RestartPaymentRequest.
    /// </summary>
    [JsonPropertyName("restartPaymentRequest")]
    public PaymentRequest RestartPaymentRequest
    {
      get => restartPaymentRequest ??= new();
      set => restartPaymentRequest = value;
    }

    /// <summary>
    /// A value of FiscalYear.
    /// </summary>
    [JsonPropertyName("fiscalYear")]
    public DateWorkArea FiscalYear
    {
      get => fiscalYear ??= new();
      set => fiscalYear = value;
    }

    /// <summary>
    /// A value of Local2NdRead.
    /// </summary>
    [JsonPropertyName("local2NdRead")]
    public DashboardAuditData Local2NdRead
    {
      get => local2NdRead ??= new();
      set => local2NdRead = value;
    }

    /// <summary>
    /// A value of UseApSupportedOnly.
    /// </summary>
    [JsonPropertyName("useApSupportedOnly")]
    public Common UseApSupportedOnly
    {
      get => useApSupportedOnly ??= new();
      set => useApSupportedOnly = value;
    }

    /// <summary>
    /// A value of PreviousRank.
    /// </summary>
    [JsonPropertyName("previousRank")]
    public DashboardStagingPriority35 PreviousRank
    {
      get => previousRank ??= new();
      set => previousRank = value;
    }

    /// <summary>
    /// A value of NullDashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("nullDashboardStagingPriority35")]
    public DashboardStagingPriority35 NullDashboardStagingPriority35
    {
      get => nullDashboardStagingPriority35 ??= new();
      set => nullDashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority35")]
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of PreviousCheck.
    /// </summary>
    [JsonPropertyName("previousCheck")]
    public DashboardStagingPriority12 PreviousCheck
    {
      get => previousCheck ??= new();
      set => previousCheck = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DashboardStagingPriority12 Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of PreviousMonthStart.
    /// </summary>
    [JsonPropertyName("previousMonthStart")]
    public DateWorkArea PreviousMonthStart
    {
      get => previousMonthStart ??= new();
      set => previousMonthStart = value;
    }

    /// <summary>
    /// A value of PrevPeriod.
    /// </summary>
    [JsonPropertyName("prevPeriod")]
    public Common PrevPeriod
    {
      get => prevPeriod ??= new();
      set => prevPeriod = value;
    }

    /// <summary>
    /// A value of PeriodStart.
    /// </summary>
    [JsonPropertyName("periodStart")]
    public Common PeriodStart
    {
      get => periodStart ??= new();
      set => periodStart = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DashboardAuditData Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of Statewide.
    /// </summary>
    [JsonPropertyName("statewide")]
    public DashboardStagingPriority12 Statewide
    {
      get => statewide ??= new();
      set => statewide = value;
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
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of ReportingAbbreviation.
    /// </summary>
    [JsonPropertyName("reportingAbbreviation")]
    public TextWorkArea ReportingAbbreviation
    {
      get => reportingAbbreviation ??= new();
      set => reportingAbbreviation = value;
    }

    /// <summary>
    /// A value of Period.
    /// </summary>
    [JsonPropertyName("period")]
    public Common Period
    {
      get => period ??= new();
      set => period = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of RecordsReadSinceCommit.
    /// </summary>
    [JsonPropertyName("recordsReadSinceCommit")]
    public Common RecordsReadSinceCommit
    {
      get => recordsReadSinceCommit ??= new();
      set => recordsReadSinceCommit = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of Earliest.
    /// </summary>
    [JsonPropertyName("earliest")]
    public CaseRole Earliest
    {
      get => earliest ??= new();
      set => earliest = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public DashboardStagingPriority12 Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of PrevRank.
    /// </summary>
    [JsonPropertyName("prevRank")]
    public DashboardStagingPriority12 PrevRank
    {
      get => prevRank ??= new();
      set => prevRank = value;
    }

    /// <summary>
    /// A value of PreviousYear.
    /// </summary>
    [JsonPropertyName("previousYear")]
    public DashboardStagingPriority12 PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of Contractor1.
    /// </summary>
    [JsonPropertyName("contractor1")]
    public CseOrganization Contractor1
    {
      get => contractor1 ??= new();
      set => contractor1 = value;
    }

    /// <summary>
    /// Gets a value of Contractor.
    /// </summary>
    [JsonIgnore]
    public Array<ContractorGroup> Contractor => contractor ??= new(
      ContractorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Contractor for json serialization.
    /// </summary>
    [JsonPropertyName("contractor")]
    [Computed]
    public IList<ContractorGroup> Contractor_Json
    {
      get => contractor;
      set => Contractor.Assign(value);
    }

    /// <summary>
    /// A value of DetermineContractor.
    /// </summary>
    [JsonPropertyName("determineContractor")]
    public DashboardStagingPriority12 DetermineContractor
    {
      get => determineContractor ??= new();
      set => determineContractor = value;
    }

    /// <summary>
    /// A value of CountCaseWk.
    /// </summary>
    [JsonPropertyName("countCaseWk")]
    public Common CountCaseWk
    {
      get => countCaseWk ??= new();
      set => countCaseWk = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public DashboardStagingPriority12 Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority12.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority12")]
    public DashboardStagingPriority12 DashboardStagingPriority12
    {
      get => dashboardStagingPriority12 ??= new();
      set => dashboardStagingPriority12 = value;
    }

    /// <summary>
    /// A value of Worker.
    /// </summary>
    [JsonPropertyName("worker")]
    public DashboardAuditData Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    /// <summary>
    /// A value of CountCaseAtty.
    /// </summary>
    [JsonPropertyName("countCaseAtty")]
    public Common CountCaseAtty
    {
      get => countCaseAtty ??= new();
      set => countCaseAtty = value;
    }

    private PaymentRequest restartPaymentRequest;
    private DateWorkArea fiscalYear;
    private DashboardAuditData local2NdRead;
    private Common useApSupportedOnly;
    private DashboardStagingPriority35 previousRank;
    private DashboardStagingPriority35 nullDashboardStagingPriority35;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private DashboardStagingPriority12 previousCheck;
    private DashboardStagingPriority12 current;
    private DateWorkArea previousMonthStart;
    private Common prevPeriod;
    private Common periodStart;
    private DashboardAuditData initialized;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DashboardStagingPriority12 statewide;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Array<LocalGroup> local1;
    private CsePerson restartCsePerson;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private TextWorkArea reportingAbbreviation;
    private Common period;
    private CsePerson prev;
    private Common recordsReadSinceCommit;
    private DashboardAuditData dashboardAuditData;
    private CaseRole earliest;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea dateWorkArea;
    private DashboardStagingPriority12 temp;
    private Common common;
    private DashboardStagingPriority12 prevRank;
    private DashboardStagingPriority12 previousYear;
    private CseOrganization contractor1;
    private Array<ContractorGroup> contractor;
    private DashboardStagingPriority12 determineContractor;
    private Common countCaseWk;
    private DashboardStagingPriority12 initialize;
    private DashboardStagingPriority12 dashboardStagingPriority12;
    private DashboardAuditData worker;
    private Common countCaseAtty;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CaseRole Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority35")]
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of PreviousYear.
    /// </summary>
    [JsonPropertyName("previousYear")]
    public DashboardStagingPriority12 PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority12.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority12")]
    public DashboardStagingPriority12 DashboardStagingPriority12
    {
      get => dashboardStagingPriority12 ??= new();
      set => dashboardStagingPriority12 = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of WorkerCaseAssignment.
    /// </summary>
    [JsonPropertyName("workerCaseAssignment")]
    public CaseAssignment WorkerCaseAssignment
    {
      get => workerCaseAssignment ??= new();
      set => workerCaseAssignment = value;
    }

    /// <summary>
    /// A value of WorkerServiceProvider.
    /// </summary>
    [JsonPropertyName("workerServiceProvider")]
    public ServiceProvider WorkerServiceProvider
    {
      get => workerServiceProvider ??= new();
      set => workerServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbursement;
    private DisbursementTransaction disbCollection;
    private PaymentRequest paymentRequest;
    private CaseRole ch;
    private CaseRole apCaseRole;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private DashboardStagingPriority12 previousYear;
    private CseOrganization cseOrganization;
    private DashboardStagingPriority12 dashboardStagingPriority12;
    private CsePerson supp;
    private Collection collection;
    private CsePerson apCsePerson;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePersonAccount supported;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private ObligationRln obligationRln;
    private LegalAction legalAction;
    private DebtDetail debtDetail;
    private ObligationType obligationType;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private Case1 case1;
    private CaseAssignment workerCaseAssignment;
    private ServiceProvider workerServiceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private LegalReferral legalReferral;
    private ServiceProvider serviceProvider;
    private LegalReferralAssignment legalReferralAssignment;
  }
#endregion
}
