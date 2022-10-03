﻿// Program: FN_B734_PRIORITY_3_1, ID: 945148924, model: 746.
// Short name: SWE03677
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
/// A program: FN_B734_PRIORITY_3_1.
/// </para>
/// <para>
/// Priority 3-1: Caseload Counts
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority31: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority31(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority31.
  /// </summary>
  public FnB734Priority31(IContext context, Import import, Export export):
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
    // Priority 3-1.1 Caseload Counts by Establishment Referral
    // Priority 3-1.2 Caseload Counts by Enforcement Referral
    // -------------------------------------------------------------------------------------
    // Priority 3-1.1 Caseload Counts by Establishemnt Referral
    // Report Level: Judicial District
    // Report Period: Month
    // 1)	Count of all cases that have a referral reason of EST or PAT when the 
    // referral status date is within the reporting period  (Open cases)
    // Priority 3-1.2 Caseload Counts by Enforcement Referral
    // Report Level: Judicial District
    // Report Period: Month
    // 1)	Count of all cases that have a referral reason of ENF or CV when the 
    // referral status date is within the reporting period.
    // -------------------------------------------------------------------------------------
    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-099    CSE Case Number
    //  100-125    legal referral create timestamp
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    local.Begin.Date = import.ReportStartDate.Date;
    local.Begin.Time = StringToTime("00.00.00.000000").GetValueOrDefault();
    UseFnBuildTimestampFrmDateTime();
    MoveDateWorkArea2(import.ReportEndDate, local.End);

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

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-01"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 100, 26)))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 100, 26);
          UseLeCabConvertTimestamp();
          local.Restart.CreatedTimestamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }

        // -- Load Judicial District counts.
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 10)))
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
            local.Local1.Update.G.CasesWithEnfReferral = 0;
            local.Local1.Update.G.CasesWithEstReferral = 0;
          }
        }

        local.Case1.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);
        local.Previous.Number = local.Case1.Number;
      }
      else
      {
        local.Case1.Number = "";
      }
    }
    else
    {
      local.Case1.Number = "";
    }

    foreach(var item in ReadLegalReferralCase())
    {
      if (!IsEmpty(entities.Case1.NoJurisdictionCd))
      {
        continue;
      }

      // -- Re-initialize Judicial District and Office
      local.Initialized.JudicialDistrict = "";
      local.Initialized.Office = 0;
      local.DashboardAuditData.Assign(local.Initialized);
      local.CountCase.Flag = "N";
      local.Case1.Number = entities.Case1.Number;
      local.DashboardStagingPriority35.CasesWithEstReferral = 0;
      local.DashboardStagingPriority35.CasesWithEnfReferral = 0;

      if (Equal(entities.Case1.Number, local.Previous.Number))
      {
        if (Lt(entities.LegalReferral.CreatedTimestamp,
          local.Restart.CreatedTimestamp))
        {
        }
        else
        {
          continue;
        }
      }
      else
      {
        local.Previous.Number = entities.Case1.Number;
      }

      local.Restart.CreatedTimestamp = entities.LegalReferral.CreatedTimestamp;

      if (Equal(entities.LegalReferral.ReferralReason1, "ENF") || Equal
        (entities.LegalReferral.ReferralReason2, "ENF") || Equal
        (entities.LegalReferral.ReferralReason3, "ENF") || Equal
        (entities.LegalReferral.ReferralReason4, "ENF") || Equal
        (entities.LegalReferral.ReferralReason1, "CV") || Equal
        (entities.LegalReferral.ReferralReason2, "CV") || Equal
        (entities.LegalReferral.ReferralReason3, "CV") || Equal
        (entities.LegalReferral.ReferralReason4, "CV"))
      {
        local.DashboardStagingPriority35.CasesWithEnfReferral = 1;
        local.DashboardAuditData.DashboardPriority = "3-1.2";
        local.CountCase.Flag = "Y";
      }
      else if (Equal(entities.LegalReferral.ReferralReason1, "EST") || Equal
        (entities.LegalReferral.ReferralReason2, "EST") || Equal
        (entities.LegalReferral.ReferralReason3, "EST") || Equal
        (entities.LegalReferral.ReferralReason4, "EST") || Equal
        (entities.LegalReferral.ReferralReason1, "PAT") || Equal
        (entities.LegalReferral.ReferralReason2, "PAT") || Equal
        (entities.LegalReferral.ReferralReason3, "PAT") || Equal
        (entities.LegalReferral.ReferralReason4, "PAT"))
      {
        if (!Equal(entities.LegalReferral.ReferralReason1, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason1, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason1))
        {
          continue;
        }

        if (!Equal(entities.LegalReferral.ReferralReason2, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason2, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason2))
        {
          continue;
        }

        if (!Equal(entities.LegalReferral.ReferralReason3, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason3, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason3))
        {
          continue;
        }

        if (!Equal(entities.LegalReferral.ReferralReason4, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason4, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason4))
        {
          continue;
        }

        local.DashboardStagingPriority35.CasesWithEstReferral = 1;
        local.DashboardAuditData.DashboardPriority = "3-1.1";
        local.CountCase.Flag = "Y";
      }
      else
      {
        continue;
      }

      if (AsChar(local.CountCase.Flag) == 'N')
      {
        // -- Case does not owe arrears.  Skip this case.
        continue;
      }

      if (AsChar(entities.Case1.Status) == 'C')
      {
        ReadCaseAssignment();
        local.End.Date = entities.CaseAssignment.DiscontinueDate;
      }
      else
      {
        local.End.Date = import.ReportEndDate.Date;
      }

      local.DashboardAuditData.CollectionCreatedDate =
        Date(entities.LegalReferral.CreatedTimestamp);
      local.DashboardAuditData.CaseNumber = entities.Case1.Number;
      local.DashboardAuditData.LegalReferralNumber =
        entities.LegalReferral.Identifier;
      local.DashboardAuditData.LegalReferralDate =
        entities.LegalReferral.ReferralDate;

      // -- Determine office and judicial district to which case is assigned on 
      // the report period end date.
      UseFnB734DetermineJdFromCase();

      if (IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        // did not find a JD so skip this case, no one to credit it to
        continue;
      }

      if (AsChar(import.AuditFlag.Flag) == 'Y')
      {
        // -- Log to the dashboard audit table.
        UseFnB734CreateDashboardAudit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -- Increment Judicial District Level
      if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        local.Local1.Index =
          (int)(StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1);
        local.Local1.CheckSize();

        local.Local1.Update.G.CasesWithEnfReferral =
          local.Local1.Item.G.CasesWithEnfReferral.GetValueOrDefault() + local
          .DashboardStagingPriority35.CasesWithEnfReferral.GetValueOrDefault();
        local.Local1.Update.G.CasesWithEstReferral =
          local.Local1.Item.G.CasesWithEstReferral.GetValueOrDefault() + local
          .DashboardStagingPriority35.CasesWithEstReferral.GetValueOrDefault();
      }

      ++local.RecordProcessed.Count;

      if (local.RecordProcessed.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
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
            "Error creating/updating Dashboard_Staging_Priority_3_5.";
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
        //  090-099    CSE Case Number
        //  100-125    legal refferral createtimestamp
        local.BatchTimestampWorkArea.IefTimestamp =
          local.Restart.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-01    " +
          " " + local.Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 99) + local
          .BatchTimestampWorkArea.TextTimestamp;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.CheckpointCount = 0;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.RecordProcessed.Count = 0;
      }
    }

    if (local.RecordProcessed.Count > 0)
    {
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
          "Error creating/updating Dashboard_Staging_Priority_3_5.";
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-03   ";
      
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
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

  private static void MoveDateWorkArea1(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDateWorkArea2(DateWorkArea source, DateWorkArea target)
    
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
    useImport.ReportEndDate.Date = local.End.Date;

    Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea1(local.Begin, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.Begin.Assign(useExport.DateWorkArea);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
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
    var casesWithEstReferral =
      local.Local1.Item.G.CasesWithEstReferral.GetValueOrDefault();
    var casesWithEnfReferral =
      local.Local1.Item.G.CasesWithEnfReferral.GetValueOrDefault();
    var param = 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", casesWithEstReferral);
        db.SetNullableInt32(command, "casWEnfRef", casesWithEnfReferral);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableInt32(command, "newOrdEst", 0);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CasesWithEstReferral =
      casesWithEstReferral;
    entities.DashboardStagingPriority35.CasesWithEnfReferral =
      casesWithEnfReferral;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
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
        entities.DashboardStagingPriority35.CasesWithEstReferral =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesWithEnfReferral =
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
        entities.DashboardStagingPriority35.CasesWithEstReferral =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesWithEnfReferral =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralCase()
  {
    entities.LegalReferral.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadLegalReferralCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
        db.SetDateTime(
          command, "createdTimestamp", local.End.Timestamp.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "statusDate", import.ReportEndDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.Case1.Status = db.GetNullableString(reader, 10);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 11);
        entities.LegalReferral.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var casesWithEstReferral =
      local.Local1.Item.G.CasesWithEstReferral.GetValueOrDefault();
    var casesWithEnfReferral =
      local.Local1.Item.G.CasesWithEnfReferral.GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", casesWithEstReferral);
        db.SetNullableInt32(command, "casWEnfRef", casesWithEnfReferral);
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
    entities.DashboardStagingPriority35.CasesWithEstReferral =
      casesWithEstReferral;
    entities.DashboardStagingPriority35.CasesWithEnfReferral =
      casesWithEnfReferral;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of AuditFlag.
    /// </summary>
    [JsonPropertyName("auditFlag")]
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private ProgramProcessingInfo programProcessingInfo;
    private DashboardAuditData dashboardAuditData;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public LegalReferral Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Begin.
    /// </summary>
    [JsonPropertyName("begin")]
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
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
    /// A value of CountCase.
    /// </summary>
    [JsonPropertyName("countCase")]
    public Common CountCase
    {
      get => countCase ??= new();
      set => countCase = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Checkpoint.
    /// </summary>
    [JsonPropertyName("checkpoint")]
    public Infrastructure Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
    }

    private LegalReferral restart;
    private Case1 previous;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private Case1 case1;
    private DateWorkArea begin;
    private DateWorkArea end;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Array<LocalGroup> local1;
    private DashboardAuditData dashboardAuditData;
    private Common recordProcessed;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common countCase;
    private DashboardAuditData initialized;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Infrastructure checkpoint;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private DashboardStagingPriority35 dashboardStagingPriority35;
    private LegalReferral legalReferral;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CseOrganization cseOrganization;
    private Office office;
  }
#endregion
}
