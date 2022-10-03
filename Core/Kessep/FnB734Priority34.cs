// Program: FN_B734_PRIORITY_3_4, ID: 945148927, model: 746.
// Short name: SWE03682
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
/// A program: FN_B734_PRIORITY_3_4.
/// </para>
/// <para>
/// Priority 3-4: Paternities Established
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority34: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_4 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority34(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority34.
  /// </summary>
  public FnB734Priority34(IContext context, Import import, Export export):
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
    // Priority 3-4: Paternities Established
    // -------------------------------------------------------------------------------------
    // This is a count of all children with paternity established by court order
    // in the review period (by attorney and caseworker).  Caseworker/Attorney
    // stats roll up into an Office, Into a Judicial District.
    // County of order needs to be applied to the county pull.
    // All out of state orders will be excluded.
    // 3-4.1 Report Level: State, Judicial District, Region, Office, Supervisor,
    // and Caseworker
    // Report Period: Month
    // Paternities Established by Caseworker
    // 1)	Case open at any time during report period.
    // 2)	J class legal action with file date entered in current report period.
    // 3)	Count only Legal Action Established by Child Support (CS) or Court 
    // Trustee (CT)
    // 4)	Look for Establish Paternity (EP) legal detail.
    // 5)	Count each child (S Role on LOPS on EP legal detail).  Multiple 
    // children will be counted if multiple children exist on LOPS.
    // 6)	Use AP/CH combination (LOPS), find case where AP/CH combination is 
    // active on legal action file date.  Credit caseworker/office assigned to
    // case (ASIN) on legal action file.
    // 3-4.2 Report Level: State, Judicial District, Region, Office, and 
    // Attorney
    // Report Period: Month
    // Paternities Established by Attorney
    // 1)	Case open at any time during report period.
    // 2)	J class legal action with file date entered in current report period.
    // 3)	Count only Legal Action Established by CS or CT
    // 4)	Look for EP legal detail.
    // 5)	Count each child (S Role on LOPS on EP legal detail).  Multiple 
    // children will be counted if multiple children exist on LOPS.
    // 6)	Credit attorney/office assigned to legal action on file date (ASIN).  
    // Each unique qualifying court order will only be credited to one attorney
    // as only one attorney will be active on the file date.
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
    }

    local.ReportingPeriod.Index = -1;

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

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

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

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

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

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

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-109    Standard Number
    //  110-117    Filed date
    //  118-118    report period
    local.SubscrpitCount.Count = 0;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-04"))
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

                local.Local1.Update.G.
                  Assign(entities.DashboardStagingPriority35);
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

                local.Local1.Update.G.
                  Assign(entities.DashboardStagingPriority35);
                local.Local1.Update.G.PaternitiesEstablished = 0;
              }

              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
              local.Checkpoint.StandardNumber = "";
            }
          }
        }
        else
        {
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
          local.Checkpoint.StandardNumber = "";

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            local.Local1.Update.G.PaternitiesEstablished = 0;
          }

          local.Local1.CheckIndex();
        }
      }
      else if (local.ReportingPeriod.Index == 1)
      {
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

                local.Local1.Update.G.
                  Assign(entities.DashboardStagingPriority35);
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

                local.Local1.Update.G.
                  Assign(entities.DashboardStagingPriority35);
                local.Local1.Update.G.PaternitiesEstablished = 0;
              }

              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
              local.Checkpoint.StandardNumber = "";
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

            local.Local1.Update.G.PaternitiesEstablished = 0;
          }

          local.Local1.CheckIndex();
        }
      }
      else if (local.ReportingPeriod.Index == 2)
      {
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

                local.Local1.Update.G.
                  Assign(entities.DashboardStagingPriority35);
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

                local.Local1.Update.G.
                  Assign(entities.DashboardStagingPriority35);
                local.Local1.Update.G.PaternitiesEstablished = 0;
              }

              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
              local.Checkpoint.StandardNumber = "";
            }
          }
        }
        else
        {
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            local.Local1.Update.G.PaternitiesEstablished = 0;
          }

          local.Local1.CheckIndex();
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
          local.Checkpoint.StandardNumber = "";
        }
      }

      foreach(var item in ReadLegalActionLaDetNonfinancial())
      {
        if (Equal(local.PreviousRecord.StandardNumber,
          entities.LegalAction.StandardNumber))
        {
          continue;
        }

        local.Initialized.JudicialDistrict = "";
        local.Initialized.Office = 0;
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

        local.NumberOfChildren.Count = 0;

        // -- Determine office and judicial district to which case is assigned 
        // on the report period end date.
        UseFnB734DetermineJdFromOrder();
        local.DashboardAuditData.DashboardPriority = "3-4";
        local.DashboardAuditData.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.DashboardAuditData.LegalActionDate =
          entities.LegalAction.FiledDate;
        local.DashboardAuditData.DebtType =
          entities.LaDetNonfinancial.NonFinOblgType;

        foreach(var item1 in ReadCsePerson())
        {
          local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
          ++local.NumberOfChildren.Count;

          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        if (local.NumberOfChildren.Count <= 0)
        {
          continue;
        }

        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)(StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1);
            
          local.Local1.CheckSize();

          local.Local1.Update.G.PaternitiesEstablished =
            local.Local1.Item.G.PaternitiesEstablished.GetValueOrDefault() + local
            .NumberOfChildren.Count;
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
          //  090-109    Standard number
          //  110-117    Filed date
          //  118-118    Period number
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "3-04     " + entities.LegalAction.StandardNumber + NumberToString
            (DateToInt(entities.LegalAction.FiledDate), 8, 8);
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 117) +
            NumberToString(local.ReportingPeriod.Index + 1, 15, 1);
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

        local.RecordProcessed.Count = 0;
      }
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-05    ";
      
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
    useImport.ReportEndDate.Date = local.ReportingPeriod.Item.EndDate.Date;
    useImport.ReportStartDate.Date = local.ReportingPeriod.Item.BeingDate.Date;

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
    var paternitiesEstablished =
      local.Local1.Item.G.PaternitiesEstablished.GetValueOrDefault();

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
        db.SetNullableInt32(command, "paternitiesEst", paternitiesEstablished);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.PaternitiesEstablished =
      paternitiesEstablished;
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

  private IEnumerable<bool> ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LaDetNonfinancial.Populated);
    entities.Supp.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LaDetNonfinancial.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LaDetNonfinancial.LgaIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Begin.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supp.Populated = true;

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
        entities.DashboardStagingPriority35.PaternitiesEstablished =
          db.GetNullableInt32(reader, 4);
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
        entities.DashboardStagingPriority35.PaternitiesEstablished =
          db.GetNullableInt32(reader, 4);
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

  private IEnumerable<bool> ReadLegalActionLaDetNonfinancial()
  {
    entities.LegalAction.Populated = false;
    entities.LaDetNonfinancial.Populated = false;

    return ReadEach("ReadLegalActionLaDetNonfinancial",
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
        entities.LaDetNonfinancial.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.LaDetNonfinancial.Number = db.GetInt32(reader, 6);
        entities.LaDetNonfinancial.NonFinOblgType =
          db.GetNullableString(reader, 7);
        entities.LegalAction.Populated = true;
        entities.LaDetNonfinancial.Populated = true;

        return true;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var paternitiesEstablished =
      local.Local1.Item.G.PaternitiesEstablished.GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "paternitiesEst", paternitiesEstablished);
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
    entities.DashboardStagingPriority35.PaternitiesEstablished =
      paternitiesEstablished;
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

    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private ProgramCheckpointRestart programCheckpointRestart;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of NumberOfChildren.
    /// </summary>
    [JsonPropertyName("numberOfChildren")]
    public Common NumberOfChildren
    {
      get => numberOfChildren ??= new();
      set => numberOfChildren = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private DateWorkArea null1;
    private Common subscrpitCount;
    private Array<ReportingPeriodGroup> reportingPeriod;
    private Common numberOfChildren;
    private LegalAction checkpoint;
    private DateWorkArea begin;
    private DateWorkAttributes dateWorkAttributes;
    private DateWorkArea end;
    private LegalAction previousRecord;
    private Common recordProcessed;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Array<LocalGroup> local1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DashboardAuditData dashboardAuditData;
    private DashboardAuditData initialized;
    private BatchTimestampWorkArea batchTimestampWorkArea;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LaDetNonfinancial.
    /// </summary>
    [JsonPropertyName("laDetNonfinancial")]
    public LegalActionDetail LaDetNonfinancial
    {
      get => laDetNonfinancial ??= new();
      set => laDetNonfinancial = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of ChOrAr.
    /// </summary>
    [JsonPropertyName("chOrAr")]
    public CaseRole ChOrAr
    {
      get => chOrAr ??= new();
      set => chOrAr = value;
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

    private DashboardStagingPriority35 dashboardStagingPriority35;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail laDetNonfinancial;
    private CsePerson supp;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole chOrAr;
    private CseOrganization cseOrganization;
  }
#endregion
}
