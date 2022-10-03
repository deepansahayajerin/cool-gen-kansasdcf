﻿// Program: FN_B734_PRIORITY_3_15, ID: 945148934, model: 746.
// Short name: SWE03688
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
/// A program: FN_B734_PRIORITY_3_15.
/// </para>
/// <para>
/// Priority 3-15: Federal Timeframes- Days from referral to order establishment
/// by referral Attorney
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority315: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_15 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority315(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority315.
  /// </summary>
  public FnB734Priority315(IContext context, Import import, Export export):
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
    // Priority 3-15: Federal Timeframes- Days from referral to order 
    // establishment by referral Attorney
    // -------------------------------------------------------------------------------------
    // 45CFR 303.4(d)- Within 90 calendar days of locating the alleged father or
    // noncustodial parent, regardless of whether paternity has been
    // established, establish an order for support or complete service of
    // process necessary to commence proceedings to establish a support order
    // and, if necessary, paternity (or document unsuccessful attempt to serve
    // process).
    // Report Level: State, Judicial District, Region, Office, Attorney
    // Report Period: Month
    // 1)	Find all J class legal actions with file date entered in current 
    // report period.
    // 2)	Count only following Action Taken: DEFJPATJ, DFLTSUPJ, JEF, MEDEXPJ, 
    // PATERNJ, PATMEDJ, PATONLYJ, SUPPORTJ, VOLPATTJ, VOLSUPTJ, VOL718BJ,
    // 718BDEFJ, 718BJERJ,
    // 3)	Count only Legal Action Established by CS or CT
    // 4)	Does not have to be obligated to be considered
    // 5)	Take case number from child role on LROL, look for PAT or EST referral
    // on that case (LGRQ).
    // 6)	Match child on LROL to child on LGRQ.
    // 7)	Count number of days from legal action file date to referral sent 
    // date.
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

    local.ReportingPeriod.Index = -1;

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

    local.ReportingPeriod.Update.BeingDate.Timestamp =
      import.ReportStartDate.Timestamp;
    local.Begin.Year = Year(import.ReportStartDate.Date);
    local.Begin.Month = Month(import.ReportStartDate.Date);
    local.Begin.Day = 1;
    local.DateWorkAttributes.TextYear = NumberToString(local.Begin.Year, 12, 4);
    local.DateWorkAttributes.TextMonth =
      NumberToString(local.Begin.Month, 14, 2);
    local.DateWorkAttributes.TextDay = NumberToString(local.Begin.Day, 14, 2);
    local.DateWorkAttributes.TextDate10Char =
      local.DateWorkAttributes.TextYear + "-" + local
      .DateWorkAttributes.TextMonth + "-" + local.DateWorkAttributes.TextDay;
    local.ReportingPeriod.Update.BeingDate.Date =
      StringToDate(local.DateWorkAttributes.TextDate10Char);
    local.ReportingPeriod.Update.DashboardAuditData.ReportMonth =
      (int)StringToNumber(local.DateWorkAttributes.TextYear +
      local.DateWorkAttributes.TextMonth);
    local.ReportingPeriod.Update.EndDate.Date = import.ReportEndDate.Date;
    local.ReportingPeriod.Update.EndDate.Timestamp = Now();

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

    local.ReportingPeriod.Update.BeingDate.Timestamp =
      AddMonths(import.ReportStartDate.Timestamp, -1);
    local.Begin.Year = Year(AddMonths(import.ReportStartDate.Date, -1));
    local.Begin.Month = Month(AddMonths(import.ReportStartDate.Date, -1));
    local.Begin.Day = 1;
    local.DateWorkAttributes.TextYear = NumberToString(local.Begin.Year, 12, 4);
    local.DateWorkAttributes.TextMonth =
      NumberToString(local.Begin.Month, 14, 2);
    local.DateWorkAttributes.TextDay = NumberToString(local.Begin.Day, 14, 2);
    local.DateWorkAttributes.TextDate10Char =
      local.DateWorkAttributes.TextYear + "-" + local
      .DateWorkAttributes.TextMonth + "-" + local.DateWorkAttributes.TextDay;
    local.ReportingPeriod.Update.BeingDate.Date =
      StringToDate(local.DateWorkAttributes.TextDate10Char);
    local.ReportingPeriod.Update.DashboardAuditData.ReportMonth =
      (int)StringToNumber(local.DateWorkAttributes.TextYear +
      local.DateWorkAttributes.TextMonth);
    local.ReportingPeriod.Update.EndDate.Date =
      AddDays(AddMonths(local.ReportingPeriod.Item.BeingDate.Date, 1), -1);
    local.ReportingPeriod.Update.EndDate.Timestamp = Now();

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

    local.ReportingPeriod.Update.BeingDate.Timestamp =
      AddMonths(import.ReportStartDate.Timestamp, -2);
    local.Begin.Year = Year(AddMonths(import.ReportStartDate.Date, -2));
    local.Begin.Month = Month(AddMonths(import.ReportStartDate.Date, -2));
    local.Begin.Day = 1;
    local.DateWorkAttributes.TextYear = NumberToString(local.Begin.Year, 12, 4);
    local.DateWorkAttributes.TextMonth =
      NumberToString(local.Begin.Month, 14, 2);
    local.DateWorkAttributes.TextDay = NumberToString(local.Begin.Day, 14, 2);
    local.DateWorkAttributes.TextDate10Char =
      local.DateWorkAttributes.TextYear + "-" + local
      .DateWorkAttributes.TextMonth + "-" + local.DateWorkAttributes.TextDay;
    local.ReportingPeriod.Update.BeingDate.Date =
      StringToDate(local.DateWorkAttributes.TextDate10Char);
    local.ReportingPeriod.Update.DashboardAuditData.ReportMonth =
      (int)StringToNumber(local.DateWorkAttributes.TextYear +
      local.DateWorkAttributes.TextMonth);
    local.ReportingPeriod.Update.EndDate.Date =
      AddDays(AddMonths(local.ReportingPeriod.Item.BeingDate.Date, 1), -1);
    local.ReportingPeriod.Update.EndDate.Timestamp = Now();
    local.Null1.Date = new DateTime(1, 1, 1);

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-109    Standard Number
    //  110-117    filed date
    //  119-119    process group
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-15"))
      {
        local.Checkpoint.StandardNumber =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 20);

        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 110, 8)))
        {
          local.Checkpoint.FiledDate =
            IntToDate((int)StringToNumber(Substring(
              import.ProgramCheckpointRestart.RestartInfo, 250, 110, 8)));
        }

        local.SubscrpitCount.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 118, 1));
      }
      else
      {
        local.Checkpoint.StandardNumber = "";
        local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
      }
    }
    else
    {
      local.Checkpoint.StandardNumber = "";
      local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
    }

    local.ReportingPeriod.Index = 0;

    for(var limit = local.ReportingPeriod.Count; local.ReportingPeriod.Index < limit
      ; ++local.ReportingPeriod.Index)
    {
      if (!local.ReportingPeriod.CheckSize())
      {
        break;
      }

      if (local.SubscrpitCount.Count > 0 && local.SubscrpitCount.Count > local
        .ReportingPeriod.Index + 1)
      {
        continue;
      }

      if (local.ReportingPeriod.Index == 0)
      {
        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.Begin.Timestamp = local.ReportingPeriod.Item.BeingDate.Timestamp;
        local.End.Timestamp = local.ReportingPeriod.Item.EndDate.Timestamp;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.Initialized.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.Local1.Update.G.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
        }

        local.Local1.CheckIndex();

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
          local.Checkpoint.CreatedTstamp =
            local.BatchTimestampWorkArea.IefTimestamp;

          if (!IsEmpty(import.ProgramCheckpointRestart.RestartInfo))
          {
            // -- Load Judicial District counts.
            if (!IsEmpty(local.Checkpoint.StandardNumber) || Lt
              (local.Null1.Date, local.Checkpoint.FiledDate) || local
              .SubscrpitCount.Count > 0)
            {
              foreach(var item in ReadDashboardStagingPriority2())
              {
                local.Local1.Index =
                  (int)(StringToNumber(
                    entities.DashboardStagingPriority35.ReportLevelId) - 1);
                local.Local1.CheckSize();

                MoveDashboardStagingPriority35(entities.
                  DashboardStagingPriority35, local.Local1.Update.G);
              }
            }
            else
            {
              // this is when there is a month in change in the middle of a 
              // week. we do not want to double count the results
              foreach(var item in ReadDashboardStagingPriority2())
              {
                local.Local1.Index =
                  (int)(StringToNumber(
                    entities.DashboardStagingPriority35.ReportLevelId) - 1);
                local.Local1.CheckSize();

                MoveDashboardStagingPriority35(entities.
                  DashboardStagingPriority35, local.Local1.Update.G);
                local.Local1.Update.G.DaysToOrderEstblshmntAvg = 0;
                local.Local1.Update.G.DaysToOrderEstblshmntDenom = 0;
                local.Local1.Update.G.DaysToOrderEstblshmntNumer = 0;
              }

              local.Checkpoint.StandardNumber = "";
              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
            }
          }
        }
        else
        {
          local.Checkpoint.StandardNumber = "";
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            local.Local1.Update.G.DaysToOrderEstblshmntAvg = 0;
            local.Local1.Update.G.DaysToOrderEstblshmntDenom = 0;
            local.Local1.Update.G.DaysToOrderEstblshmntNumer = 0;
          }

          local.Local1.CheckIndex();
        }
      }
      else if (local.ReportingPeriod.Index == 1)
      {
        local.Begin.Timestamp = local.ReportingPeriod.Item.BeingDate.Timestamp;
        local.End.Timestamp = local.ReportingPeriod.Item.EndDate.Timestamp;
        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.Initialized.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.Local1.Update.G.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
        }

        local.Local1.CheckIndex();

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
          if (!IsEmpty(import.ProgramCheckpointRestart.RestartInfo))
          {
            // -- Load Judicial District counts.
            if (!IsEmpty(local.Checkpoint.StandardNumber) || Lt
              (local.Null1.Date, local.Checkpoint.FiledDate) || local
              .SubscrpitCount.Count > 0)
            {
              foreach(var item in ReadDashboardStagingPriority2())
              {
                local.Local1.Index =
                  (int)(StringToNumber(
                    entities.DashboardStagingPriority35.ReportLevelId) - 1);
                local.Local1.CheckSize();

                MoveDashboardStagingPriority35(entities.
                  DashboardStagingPriority35, local.Local1.Update.G);
              }
            }
            else
            {
              // this is when there is a month in change in the middle of a 
              // week. we do not want to double count the results
              foreach(var item in ReadDashboardStagingPriority2())
              {
                local.Local1.Index =
                  (int)(StringToNumber(
                    entities.DashboardStagingPriority35.ReportLevelId) - 1);
                local.Local1.CheckSize();

                MoveDashboardStagingPriority35(entities.
                  DashboardStagingPriority35, local.Local1.Update.G);
                local.Local1.Update.G.DaysToOrderEstblshmntAvg = 0;
                local.Local1.Update.G.DaysToOrderEstblshmntDenom = 0;
                local.Local1.Update.G.DaysToOrderEstblshmntNumer = 0;
              }

              local.Checkpoint.StandardNumber = "";
              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
            }
          }
        }
        else
        {
          local.Checkpoint.StandardNumber = "";
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            local.Local1.Update.G.DaysToOrderEstblshmntAvg = 0;
            local.Local1.Update.G.DaysToOrderEstblshmntDenom = 0;
            local.Local1.Update.G.DaysToOrderEstblshmntNumer = 0;
          }

          local.Local1.CheckIndex();
        }
      }
      else if (local.ReportingPeriod.Index == 2)
      {
        local.Begin.Timestamp = local.ReportingPeriod.Item.BeingDate.Timestamp;
        local.End.Timestamp = local.ReportingPeriod.Item.EndDate.Timestamp;
        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.Initialized.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.Local1.Update.G.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
        }

        local.Local1.CheckIndex();

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
          if (!IsEmpty(import.ProgramCheckpointRestart.RestartInfo))
          {
            // -- Load Judicial District counts.
            if (!IsEmpty(local.Checkpoint.StandardNumber) || Lt
              (local.Null1.Date, local.Checkpoint.FiledDate) || local
              .SubscrpitCount.Count > 0)
            {
              foreach(var item in ReadDashboardStagingPriority2())
              {
                local.Local1.Index =
                  (int)(StringToNumber(
                    entities.DashboardStagingPriority35.ReportLevelId) - 1);
                local.Local1.CheckSize();

                MoveDashboardStagingPriority35(entities.
                  DashboardStagingPriority35, local.Local1.Update.G);
              }
            }
            else
            {
              // this is when there is a month in change in the middle of a 
              // week. we do not want to double count the results
              foreach(var item in ReadDashboardStagingPriority2())
              {
                local.Local1.Index =
                  (int)(StringToNumber(
                    entities.DashboardStagingPriority35.ReportLevelId) - 1);
                local.Local1.CheckSize();

                MoveDashboardStagingPriority35(entities.
                  DashboardStagingPriority35, local.Local1.Update.G);
                local.Local1.Update.G.DaysToOrderEstblshmntAvg = 0;
                local.Local1.Update.G.DaysToOrderEstblshmntDenom = 0;
                local.Local1.Update.G.DaysToOrderEstblshmntNumer = 0;
              }

              local.Checkpoint.StandardNumber = "";
              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
            }
          }
        }
        else
        {
          local.Checkpoint.StandardNumber = "";
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            local.Local1.Update.G.DaysToOrderEstblshmntAvg = 0;
            local.Local1.Update.G.DaysToOrderEstblshmntDenom = 0;
            local.Local1.Update.G.DaysToOrderEstblshmntNumer = 0;
          }

          local.Local1.CheckIndex();
        }
      }

      foreach(var item in ReadLegalAction2())
      {
        if (ReadLegalAction1())
        {
          // already had one so we can not count it
          continue;
        }
        else
        {
          // no previous legal action for this court order number so we can 
          // count it
        }

        local.Initialized.JudicialDistrict = "";
        local.Initialized.Office = 0;

        if (Equal(entities.LegalAction.StandardNumber,
          local.Checkpoint.StandardNumber))
        {
          if (Lt(entities.LegalAction.FiledDate, local.Checkpoint.FiledDate))
          {
            goto Next;
          }
          else
          {
          }
        }

        local.DashboardAuditData.Assign(local.Initialized);
        local.PreviousRecord.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.Checkpoint.StandardNumber = entities.LegalAction.StandardNumber;
        local.Checkpoint.FiledDate = entities.LegalAction.FiledDate;

        if (ReadFipsTribunal())
        {
          if (entities.Fips.State != 20)
          {
            continue;
          }
        }
        else
        {
          continue;
        }

        foreach(var item1 in ReadLegalReferralCase())
        {
          if (!Lt(local.Null1.Date, entities.LegalReferral.ReferralDate))
          {
            continue;
          }

          if (Equal(entities.LegalReferral.ReferralReason1, "PAT") || Equal
            (entities.LegalReferral.ReferralReason2, "PAT") || Equal
            (entities.LegalReferral.ReferralReason3, "PAT") || Equal
            (entities.LegalReferral.ReferralReason4, "PAT") || Equal
            (entities.LegalReferral.ReferralReason1, "EST") || Equal
            (entities.LegalReferral.ReferralReason2, "EST") || Equal
            (entities.LegalReferral.ReferralReason3, "EST") || Equal
            (entities.LegalReferral.ReferralReason4, "EST"))
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

            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.Case1.Number = entities.Case1.Number;
            local.DashboardAuditData.DaysReported =
              DaysFromAD(entities.LegalAction.FiledDate) - DaysFromAD
              (entities.LegalReferral.ReferralDate);
            local.DashboardAuditData.LegalReferralDate =
              entities.LegalReferral.ReferralDate;
            local.DashboardAuditData.LegalActionDate =
              entities.LegalAction.FiledDate;
            local.DashboardAuditData.DashboardPriority = "3-15";
            local.DashboardAuditData.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.CountCase.Flag = "Y";

            // -- Determine office and judicial district to which case is 
            // assigned on the report period end date.
            UseFnB734DetermineJdFromOrder();

            if (IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              goto ReadEach;
            }

            local.DashboardAuditData.DashboardPriority = "3-15";
            local.DashboardAuditData.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.DashboardAuditData.DebtType =
              entities.LegalActionDetail.NonFinOblgType;
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;

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
                (int)(
                  StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
                );
              local.Local1.CheckSize();

              local.Local1.Update.G.DaysToOrderEstblshmntNumer =
                local.Local1.Item.G.DaysToOrderEstblshmntNumer.
                  GetValueOrDefault() + local
                .DashboardAuditData.DaysReported.GetValueOrDefault();
              local.Local1.Update.G.DaysToOrderEstblshmntDenom =
                local.Local1.Item.G.DaysToOrderEstblshmntDenom.
                  GetValueOrDefault() + 1;

              if (local.Local1.Item.G.DaysToOrderEstblshmntNumer.
                GetValueOrDefault() > 0 && local
                .Local1.Item.G.DaysToOrderEstblshmntDenom.
                  GetValueOrDefault() > 0)
              {
                local.Local1.Update.G.DaysToOrderEstblshmntAvg =
                  (decimal)local.Local1.Item.G.DaysToOrderEstblshmntNumer.
                    GetValueOrDefault() / local
                  .Local1.Item.G.DaysToOrderEstblshmntDenom.GetValueOrDefault();
                  
              }
              else
              {
                local.Local1.Update.G.DaysToOrderEstblshmntAvg = 0;
              }
            }
          }
          else
          {
            continue;
          }
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
          //  090-109    Standard Number
          //  110-117    filed date
          //  118-118    process group
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "3-15     " + entities.LegalAction.StandardNumber + NumberToString
            (DateToInt(entities.LegalAction.FiledDate), 8, 8);
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 117) +
            NumberToString(local.ReportingPeriod.Index + 1, 15, 1);
          local.ProgramCheckpointRestart.CheckpointCount = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
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

ReadEach:
        ;
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

Next:
      ;
    }

    local.ReportingPeriod.CheckIndex();

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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-16    ";
      
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
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveDashboardStagingPriority35(
    DashboardStagingPriority35 source, DashboardStagingPriority35 target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.AsOfDate = source.AsOfDate;
    target.DaysToOrderEstblshmntNumer = source.DaysToOrderEstblshmntNumer;
    target.DaysToOrderEstblshmntDenom = source.DaysToOrderEstblshmntDenom;
    target.DaysToOrderEstblshmntAvg = source.DaysToOrderEstblshmntAvg;
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

  private void UseFnB734DetermineJdFromOrder()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentLegalAction.Assign(entities.LegalAction);
    useImport.ReportStartDate.Date = local.Begin.Date;
    useImport.ReportEndDate.Date = local.End.Date;

    Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

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
    var daysToOrderEstblshmntNumer =
      local.Local1.Item.G.DaysToOrderEstblshmntNumer.GetValueOrDefault();
    var daysToOrderEstblshmntDenom =
      local.Local1.Item.G.DaysToOrderEstblshmntDenom.GetValueOrDefault();
    var daysToOrderEstblshmntAvg =
      local.Local1.Item.G.DaysToOrderEstblshmntAvg.GetValueOrDefault();

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
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.
          SetNullableInt32(command, "ordEstDaysNmr", daysToOrderEstblshmntNumer);
          
        db.SetNullableInt32(
          command, "ordEstDaysDnom", daysToOrderEstblshmntDenom);
        db.
          SetNullableDecimal(command, "ordEstDaysAvg", daysToOrderEstblshmntAvg);
          
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.DaysToOrderEstblshmntNumer =
      daysToOrderEstblshmntNumer;
    entities.DashboardStagingPriority35.DaysToOrderEstblshmntDenom =
      daysToOrderEstblshmntDenom;
    entities.DashboardStagingPriority35.DaysToOrderEstblshmntAvg =
      daysToOrderEstblshmntAvg;
    entities.DashboardStagingPriority35.Populated = true;
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
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntNumer =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntDenom =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 6);
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
          command, "reportMonth",
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth);
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
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntNumer =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntDenom =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private bool ReadFipsTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadFipsTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.Name = db.GetString(reader, 3);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 5);
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetNullableDate(
          command, "filedDt", local.Begin.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N2dRead.Identifier = db.GetInt32(reader, 0);
        entities.N2dRead.Classification = db.GetString(reader, 1);
        entities.N2dRead.ActionTaken = db.GetString(reader, 2);
        entities.N2dRead.FiledDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.StandardNumber = db.GetNullableString(reader, 4);
        entities.N2dRead.EstablishmentCode = db.GetNullableString(reader, 5);
        entities.N2dRead.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.Begin.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.End.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", local.Checkpoint.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalAction.Populated = true;

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
        db.SetNullableDate(
          command, "statusDate",
          entities.LegalAction.FiledDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 8);
        entities.LegalReferral.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var daysToOrderEstblshmntNumer =
      local.Local1.Item.G.DaysToOrderEstblshmntNumer.GetValueOrDefault();
    var daysToOrderEstblshmntDenom =
      local.Local1.Item.G.DaysToOrderEstblshmntDenom.GetValueOrDefault();
    var daysToOrderEstblshmntAvg =
      local.Local1.Item.G.DaysToOrderEstblshmntAvg.GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.
          SetNullableInt32(command, "ordEstDaysNmr", daysToOrderEstblshmntNumer);
          
        db.SetNullableInt32(
          command, "ordEstDaysDnom", daysToOrderEstblshmntDenom);
        db.
          SetNullableDecimal(command, "ordEstDaysAvg", daysToOrderEstblshmntAvg);
          
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
    entities.DashboardStagingPriority35.DaysToOrderEstblshmntNumer =
      daysToOrderEstblshmntNumer;
    entities.DashboardStagingPriority35.DaysToOrderEstblshmntDenom =
      daysToOrderEstblshmntDenom;
    entities.DashboardStagingPriority35.DaysToOrderEstblshmntAvg =
      daysToOrderEstblshmntAvg;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private ProgramCheckpointRestart programCheckpointRestart;
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

    /// <summary>A ReportingPeriodGroup group.</summary>
    [Serializable]
    public class ReportingPeriodGroup
    {
      /// <summary>
      /// A value of EndDate.
      /// </summary>
      [JsonPropertyName("endDate")]
      public DateWorkArea EndDate
      {
        get => endDate ??= new();
        set => endDate = value;
      }

      /// <summary>
      /// A value of BeingDate.
      /// </summary>
      [JsonPropertyName("beingDate")]
      public DateWorkArea BeingDate
      {
        get => beingDate ??= new();
        set => beingDate = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private DateWorkArea endDate;
      private DateWorkArea beingDate;
      private DashboardAuditData dashboardAuditData;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DashboardAuditData Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of Checkpoint.
    /// </summary>
    [JsonPropertyName("checkpoint")]
    public LegalAction Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
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
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
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
    /// A value of PreviousRecord.
    /// </summary>
    [JsonPropertyName("previousRecord")]
    public LegalAction PreviousRecord
    {
      get => previousRecord ??= new();
      set => previousRecord = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority35")]
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// Gets a value of ReportingPeriod.
    /// </summary>
    [JsonIgnore]
    public Array<ReportingPeriodGroup> ReportingPeriod =>
      reportingPeriod ??= new(ReportingPeriodGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ReportingPeriod for json serialization.
    /// </summary>
    [JsonPropertyName("reportingPeriod")]
    [Computed]
    public IList<ReportingPeriodGroup> ReportingPeriod_Json
    {
      get => reportingPeriod;
      set => ReportingPeriod.Assign(value);
    }

    /// <summary>
    /// A value of SubscrpitCount.
    /// </summary>
    [JsonPropertyName("subscrpitCount")]
    public Common SubscrpitCount
    {
      get => subscrpitCount ??= new();
      set => subscrpitCount = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private DateWorkArea null1;
    private DashboardAuditData initialized;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Array<LocalGroup> local1;
    private LegalAction checkpoint;
    private DateWorkArea begin;
    private DateWorkAttributes dateWorkAttributes;
    private DateWorkArea end;
    private LegalAction previousRecord;
    private DashboardAuditData dashboardAuditData;
    private Common recordProcessed;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common countCase;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private Array<ReportingPeriodGroup> reportingPeriod;
    private Common subscrpitCount;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ch2Nd.
    /// </summary>
    [JsonPropertyName("ch2Nd")]
    public CaseRole Ch2Nd
    {
      get => ch2Nd ??= new();
      set => ch2Nd = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public LegalAction N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private CaseRole ch2Nd;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalReferralCaseRole legalReferralCaseRole;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson;
    private LegalReferral legalReferral;
    private Case1 case1;
    private CseOrganization cseOrganization;
    private LegalAction legalAction;
    private LegalAction n2dRead;
    private Fips fips;
    private Tribunal tribunal;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
