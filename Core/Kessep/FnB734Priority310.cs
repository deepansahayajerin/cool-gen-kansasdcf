// Program: FN_B734_PRIORITY_3_10, ID: 945148930, model: 746.
// Short name: SWE03678
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
/// A program: FN_B734_PRIORITY_3_10.
/// </para>
/// <para>
/// Priority 3-10: Income Withholdings Issued
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority310: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_10 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority310(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority310.
  /// </summary>
  public FnB734Priority310(IContext context, Import import, Export export):
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
    // Priority 3-10: Income Withholdings Issued
    // -------------------------------------------------------------------------------------
    // This is a count of all ORD/NOTICE WITHHOLD INCOME CS issued during the 
    // report period.  Attorney statistics roll up into an Office, then into a
    // Judicial District.
    // Report Level: State, Judicial District, Region, Office, Attorney/
    // Caseworker
    // Report Period: Month
    // All out of state orders will be excluded.
    // 3-10.1 Income Withholdings Issued by Attorney
    // 1)	Count I class legal action with CREATED DATE entered in current 
    // report period.  (usually not a file date entered on these legal actions.)
    // 2)	Count only following action taken: ORDIWO2 (ORD/NOTICE WITHHOLD INCOME
    // CS)
    // 3)	An associated entry must exist on the IWGL screen.
    // 4)	Credit attorney/office assigned to legal action on created date (ASIN
    // ).  Each unique qualifying court order will only be credited to one
    // attorney as only one attorney will be active on the created date.
    // 3-10.2	Income Withholdings Issued by Caseworker
    // 1)	Count I class legal action with CREATED DATE entered in current 
    // report period.  (usually not a file date entered on these legal actions.)
    // 2)	Count only following action taken: ORDIWO2 (ORD/NOTICE WITHHOLD INCOME
    // CS)
    // 3)	An associated entry must exist on the IWGL screen.
    // 4)	Look for AP/CH combination on LOPS for all legal details.  Credit all 
    // cases (caseworkers) where that same AP/CH combination overlapped at any
    // time.  The court order MUST also be associated to the case through LROL (
    // CSE CASE #s field).
    // 5)	Case must have been open on ORDIWO2 Created Date.
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
    local.ReportingPeriod.Update.EndDate.Timestamp =
      import.ReportEndDate.Timestamp;

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-110    legal action create timestamp
    local.SubscrpitCount.Count = 0;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-10"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.SubscrpitCount.Count =
            (int)StringToNumber(Substring(
              import.ProgramCheckpointRestart.RestartInfo, 250, 117, 1));
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.Checkpoint.CreatedTstamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }
      }
    }
    else
    {
      local.Checkpoint.CreatedTstamp = import.ReportEndDate.Timestamp;
    }

    local.ReportingPeriod.Index = 0;

    for(var limit = local.ReportingPeriod.Count; local.ReportingPeriod.Index < limit
      ; ++local.ReportingPeriod.Index)
    {
      if (!local.ReportingPeriod.CheckSize())
      {
        break;
      }

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
          if (Lt(local.Null1.Timestamp, local.Checkpoint.CreatedTstamp) || local
            .SubscrpitCount.Count > 0)
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
            // this is when there is a month in change in the middle of a week. 
            // we do not want to double count the results
            foreach(var item in ReadDashboardStagingPriority2())
            {
              local.Local1.Index =
                (int)(StringToNumber(
                  entities.DashboardStagingPriority35.ReportLevelId) - 1);
              local.Local1.CheckSize();

              local.Local1.Update.G.Assign(entities.DashboardStagingPriority35);
              local.Local1.Update.G.IncomeWithholdingsIssued = 0;
            }

            local.Checkpoint.CreatedTstamp = import.ReportEndDate.Timestamp;
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

          local.Local1.Update.G.IncomeWithholdingsIssued = 0;
        }

        local.Local1.CheckIndex();
        local.Checkpoint.CreatedTstamp = local.End.Timestamp;
      }

      foreach(var item in ReadLegalAction())
      {
        local.Checkpoint.CreatedTstamp = entities.LegalAction.CreatedTstamp;
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

        ReadLegalActionIncomeSourceCsePerson();

        if (entities.LegalActionIncomeSource.Populated)
        {
          // we can count this record since it has a iwgl record tied to the 
          // current legal action for the obligor
        }
        else
        {
          continue;
        }

        // -- Determine office and judicial district to which case is assigned 
        // on the report period end date.
        UseFnB734DetermineJdFromOrder();

        if (IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          continue;
        }

        local.DashboardAuditData.DashboardPriority = "3-10";
        local.DashboardAuditData.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.DashboardAuditData.LegalActionDate =
          Date(entities.LegalAction.CreatedTstamp);
        local.DashboardAuditData.PayorCspNumber = entities.CsePerson.Number;

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

          local.Local1.Update.G.IncomeWithholdingsIssued =
            local.Local1.Item.G.IncomeWithholdingsIssued.GetValueOrDefault() + 1
            ;
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
          //  090-116    legal action create timestamp
          local.BatchTimestampWorkArea.TextTimestamp = "";
          local.BatchTimestampWorkArea.IefTimestamp =
            local.Checkpoint.CreatedTstamp;
          UseLeCabConvertTimestamp();
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "3-10    " + " " + local.BatchTimestampWorkArea.TextTimestamp;
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 116) +
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-11     ";
      
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
    useImport.ReportStartDate.Date = local.Begin.Date;
    useImport.ReportEndDate.Date = local.End.Date;

    Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
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
    var param = 0M;
    var incomeWithholdingsIssued =
      local.Local1.Item.G.IncomeWithholdingsIssued.GetValueOrDefault();

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
        db.SetNullableInt32(command, "iwIssued", incomeWithholdingsIssued);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.IncomeWithholdingsIssued =
      incomeWithholdingsIssued;
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
        entities.DashboardStagingPriority35.IncomeWithholdingsIssued =
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
        entities.DashboardStagingPriority35.IncomeWithholdingsIssued =
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

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", local.Begin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", local.End.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          local.Checkpoint.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionIncomeSourceCsePerson()
  {
    entities.LegalActionIncomeSource.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadLegalActionIncomeSourceCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 3);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionIncomeSource.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var incomeWithholdingsIssued =
      local.Local1.Item.G.IncomeWithholdingsIssued.GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "iwIssued", incomeWithholdingsIssued);
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
    entities.DashboardStagingPriority35.IncomeWithholdingsIssued =
      incomeWithholdingsIssued;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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

    private DateWorkArea null1;
    private BatchTimestampWorkArea batchTimestampWorkArea;
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
    private Array<ReportingPeriodGroup> reportingPeriod;
    private Common subscrpitCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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

    private Employer employer;
    private IncomeSource employment;
    private LegalActionIncomeSource legalActionIncomeSource;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private CseOrganization cseOrganization;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
