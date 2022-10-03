﻿// Program: FN_B734_PRIORITY_3_7_AND_3_8, ID: 945148929, model: 746.
// Short name: SWE03684
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
/// A program: FN_B734_PRIORITY_3_7_AND_3_8.
/// </para>
/// <para>
/// Priority 3-7: Cases Closed With Orders
/// Priority 3-8: Cases closed Without Orders
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority37And38: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_7_AND_3_8 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority37And38(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority37And38.
  /// </summary>
  public FnB734Priority37And38(IContext context, Import import, Export export):
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
    // Priority 3-7: Cases Closed With Orders
    // Priority 3-8: Cases closed Without Orders
    // -------------------------------------------------------------------------------------
    // Report Level: State, Judicial District, Region, Office, Supervisor, 
    // Caseworker
    // Report Period: Month
    // 1)	Cases open at any time during the reporting period with a J or O class
    // legal action and a legal detail with no end date and an obligation type
    // of CRCH, CS, MS, AJ, MJ-NA, ZCS, HIC or UM.
    // 2)	Must read for AP/CH combination (defined on LROL screen for non-
    // financial legal details and LOPS screen for financial legal details)
    // 3)	Case roles can be open or closed.
    // 4)	Case roles do not have to have been open during the reporting period.
    // 5)	Read for any J or O class legal action.
    // 6)	Do not read for open legal detail on financial obligations (financial 
    // obligations must be obligated)
    // 7)	Do count if non-financial legal detail is ended, but the end date is 
    // in the future (after report period end).
    // 8)	Do count if obligation was created any time during or prior to the 
    // reporting period.
    // 9)	Read Legal Action Case Role for HIC & UM legal details
    // 10)	 Count case if there was a cash or medical support order at one time,
    // but there is no money owed now, or the medical support is no longer in
    // effect
    // 11)	EP should not be considered for this line.
    // 12)	The legal action must have a filed date.
    // 13)	Count all cases with a case closure date within the reporting period.
    // 14)	Count each case only once.
    // 15)	Count caseworker assigned to the case as of refresh date.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

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
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "3-07    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    CSE Case Number
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);

      // -- Load Judicial District counts.
      if (!IsEmpty(local.Restart.Number))
      {
        foreach(var item in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)(StringToNumber(
              entities.DashboardStagingPriority35.ReportLevelId) - 1);
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority35);
        }
      }
      else
      {
        // this is when there is a month in change in the middle of a week. we 
        // do not want to double count the results
        foreach(var item in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)(StringToNumber(
              entities.DashboardStagingPriority35.ReportLevelId) - 1);
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority35);
          local.Local1.Update.G.CasesClosedWithOrders = 0;
          local.Local1.Update.G.CasesClosedWithoutOrders = 0;
        }
      }
    }
    else
    {
      local.Restart.Number = "";
    }

    // ------------------------------------------------------------------------------
    // -- Read each open case.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadCaseCaseAssignment())
    {
      if (Equal(entities.Case1.Number, local.Prev.Number))
      {
        continue;
      }
      else
      {
        local.Prev.Number = entities.Case1.Number;
      }

      if (AsChar(entities.Case1.Status) == 'O')
      {
        if (ReadInfrastructure())
        {
          // THIS IS OK TO PROCESS THIS ONE
        }
        else
        {
          // WAS NOT CLOSED IN THE TIME REPORT PERIOD
          continue;
        }
      }

      local.DashboardAuditData.Assign(local.Initialized);
      local.CaseAssignment.Date = entities.CaseAssignment.EffectiveDate;

      // -- Determine office and judicial district to which case is assigned on 
      // the report period end date.
      UseFnB734DetermineJdFromCase();

      if (IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        // did not find a JD so skip this case, no one to credit it to
        continue;
      }

      local.CaseUnderOrder.Flag = "";
      local.ReportEndDate.Date = entities.CaseAssignment.DiscontinueDate;
      local.ReportEndDate.Timestamp = import.ReportEndDate.Timestamp;

      // ----------------------------------------------------------------------
      // We will now check to see if the case is under order are not
      // -----------------------------------------------------------------------
      UseFnB734CaseUnderOrder();

      if (AsChar(local.CaseUnderOrder.Flag) == 'N')
      {
        // -- Case is not under order.
        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)(StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1);
            
          local.Local1.CheckSize();

          local.Local1.Update.G.CasesClosedWithoutOrders =
            local.Local1.Item.G.CasesClosedWithoutOrders.GetValueOrDefault() + 1
            ;
        }

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          local.DashboardAuditData.DashboardPriority = "3-8";
          UseFnB734CreateDashboardAudit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
      else
      {
        // -- Case is under order.  Count in the Priority 3-7 and log to the 
        // Dashboard Audit Table.
        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)(StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1);
            
          local.Local1.CheckSize();

          local.Local1.Update.G.CasesClosedWithOrders =
            local.Local1.Item.G.CasesClosedWithOrders.GetValueOrDefault() + 1;
        }

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          local.DashboardAuditData.DashboardPriority = "3-7";
          UseFnB734CreateDashboardAudit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }

      ++local.RecordsReadSinceCommit.Count;

      // ------------------------------------------------------------------------------
      // -- Checkpoint saving all the info needed for restarting.
      // ------------------------------------------------------------------------------
      if (local.RecordsReadSinceCommit.Count >= import
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Save Judicial District counts.
        local.Local1.Index = 0;

        for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          if (ReadDashboardStagingPriority1())
          {
            try
            {
              UpdateDashboardStagingPriority35();
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
              CreateDashboardStagingPriority35();
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
              "Error creating/updating Dashboard_Staging_Priority_3_5.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }

        local.Local1.CheckIndex();

        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-099    CSE Case Number
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-07    " +
          " " + entities.Case1.Number;
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

    if (local.RecordsReadSinceCommit.Count > 0)
    {
      // ------------------------------------------------------------------------------
      // -- Store final Judicial District counts.
      // ------------------------------------------------------------------------------
      // -- Save Judicial District counts.
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (ReadDashboardStagingPriority1())
        {
          try
          {
            UpdateDashboardStagingPriority35();
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
            CreateDashboardStagingPriority35();
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
      }

      local.Local1.CheckIndex();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error creating/updating Dashboard_Staging_Priority_3-5.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint for restarting at the next priority.
    // ------------------------------------------------------------------------------
    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInfo =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-09    ";
      
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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

  private void UseFnB734CaseUnderOrder()
  {
    var useImport = new FnB734CaseUnderOrder.Import();
    var useExport = new FnB734CaseUnderOrder.Export();

    useImport.Case1.Number = entities.Case1.Number;
    MoveDateWorkArea(local.ReportEndDate, useImport.ReportEndDate);

    Call(FnB734CaseUnderOrder.Execute, useImport, useExport);

    local.CaseUnderOrder.Flag = useExport.CaseUnderOrder.Flag;
  }

  private void UseFnB734CreateDashboardAudit()
  {
    var useImport = new FnB734CreateDashboardAudit.Import();
    var useExport = new FnB734CreateDashboardAudit.Export();

    MoveDashboardAuditData1(local.DashboardAuditData,
      useImport.DashboardAuditData);

    Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = local.CaseAssignment.Date;

    Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority35()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var casesClosedWithOrders =
      local.Local1.Item.G.CasesClosedWithOrders.GetValueOrDefault();
    var casesClosedWithoutOrders =
      local.Local1.Item.G.CasesClosedWithoutOrders.GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableInt32(command, "casesClsWOrder", casesClosedWithOrders);
        db.
          SetNullableInt32(command, "casesClsWoOrder", casesClosedWithoutOrders);
          
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CasesClosedWithOrders =
      casesClosedWithOrders;
    entities.DashboardStagingPriority35.CasesClosedWithoutOrders =
      casesClosedWithoutOrders;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    entities.Case1.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 4);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 5);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.CaseAssignment.OspCode = db.GetString(reader, 10);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;

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

  private bool ReadDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Local1.Item.G.ReportMonth);
        db.SetString(command, "reportLevel", local.Local1.Item.G.ReportLevel);
        db.
          SetString(command, "reportLevelId", local.Local1.Item.G.ReportLevelId);
          
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
        entities.DashboardStagingPriority35.CasesClosedWithOrders =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesClosedWithoutOrders =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority2()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return ReadEach("ReadDashboardStagingPriority2",
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
        entities.DashboardStagingPriority35.CasesClosedWithOrders =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesClosedWithoutOrders =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Infrastructure.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var casesClosedWithOrders =
      local.Local1.Item.G.CasesClosedWithOrders.GetValueOrDefault();
    var casesClosedWithoutOrders =
      local.Local1.Item.G.CasesClosedWithoutOrders.GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casesClsWOrder", casesClosedWithOrders);
        db.
          SetNullableInt32(command, "casesClsWoOrder", casesClosedWithoutOrders);
          
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
    entities.DashboardStagingPriority35.CasesClosedWithOrders =
      casesClosedWithOrders;
    entities.DashboardStagingPriority35.CasesClosedWithoutOrders =
      casesClosedWithoutOrders;
    entities.DashboardStagingPriority35.Populated = true;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
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
    /// A value of AuditFlag.
    /// </summary>
    [JsonPropertyName("auditFlag")]
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private DashboardAuditData dashboardAuditData;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Common auditFlag;
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
      public DashboardStagingPriority35 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority35 g;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public DateWorkArea CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
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
    /// A value of NonFinLdet.
    /// </summary>
    [JsonPropertyName("nonFinLdet")]
    public Common NonFinLdet
    {
      get => nonFinLdet ??= new();
      set => nonFinLdet = value;
    }

    /// <summary>
    /// A value of FinLdet.
    /// </summary>
    [JsonPropertyName("finLdet")]
    public Common FinLdet
    {
      get => finLdet ??= new();
      set => finLdet = value;
    }

    /// <summary>
    /// A value of AccrualInstrFound.
    /// </summary>
    [JsonPropertyName("accrualInstrFound")]
    public Common AccrualInstrFound
    {
      get => accrualInstrFound ??= new();
      set => accrualInstrFound = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of CaseUnderOrder.
    /// </summary>
    [JsonPropertyName("caseUnderOrder")]
    public Common CaseUnderOrder
    {
      get => caseUnderOrder ??= new();
      set => caseUnderOrder = value;
    }

    private DateWorkArea reportEndDate;
    private DateWorkArea caseAssignment;
    private DashboardAuditData initialized;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Array<LocalGroup> local1;
    private Case1 restart;
    private Case1 prev;
    private Common recordsReadSinceCommit;
    private DashboardAuditData dashboardAuditData;
    private Common nonFinLdet;
    private Common finLdet;
    private Common accrualInstrFound;
    private DateWorkArea null1;
    private Program program;
    private Common caseUnderOrder;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private DashboardStagingPriority35 dashboardStagingPriority35;
    private Infrastructure infrastructure;
    private CseOrganization cseOrganization;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }
#endregion
}
