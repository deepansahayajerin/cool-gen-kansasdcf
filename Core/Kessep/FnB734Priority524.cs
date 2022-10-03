﻿// Program: FN_B734_PRIORITY_5_24, ID: 945148982, model: 746.
// Short name: SWE03721
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
/// A program: FN_B734_PRIORITY_5_24.
/// </para>
/// <para>
/// Priority 3-9: Modifications
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority524: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_5_24 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority524(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority524.
  /// </summary>
  public FnB734Priority524(IContext context, Import import, Export export):
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
    // Priority 5-24: Case Reviews
    // -------------------------------------------------------------------------------------
    // This is a count of all court order modifications entered within the 
    // report period.  Attorney stats roll up into an Office, Into a Judicial
    // District.
    // All out of state orders will be excluded.
    // Report Level: Caseworker
    // Report Period: Month
    // Modifications by Caseworker:
    // 1)	Case open at any time during report period.
    // 2)	Use J class legal action with history record file date entered in 
    // current report period.
    // 3)	Count only following Action Taken: OSPRVWS
    // 4)	Count only Legal Action Established by CS or CT
    // 5)	Does not have to be obligated to be considered
    // 6)	Credit attorney/office assigned to legal action on file date (ASIN).  
    // Each unique qualifying court order will only be credited to one attorney
    // as only one attorney will be active on the file date.
    // -------------------------------------------------------------------------------------
    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-110    intrafursture createtimestamp
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveDashboardAuditData2(import.DashboardAuditData,
      local.InitializedDashboardAuditData);

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "5-24"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.Checkpoint.CreatedTimestamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }

        if (Lt(local.Null1.Timestamp, local.Checkpoint.CreatedTimestamp))
        {
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          local.Checkpoint.CreatedTimestamp = import.ReportEndDate.Timestamp;

          foreach(var item in ReadDashboardStagingPriority2())
          {
            // need to clear the previcously determined totals before the 
            // program begins or the numbers will not be correct, they will
            // reflect previous run numbers also
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
      }
      else
      {
        local.Checkpoint.CreatedTimestamp = import.ReportEndDate.Timestamp;

        foreach(var item in ReadDashboardStagingPriority2())
        {
          // need to clear the previcously determined totals before the program 
          // begins or the numbers will not be correct, they will reflect
          // previous run numbers also
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
    }
    else
    {
      local.Checkpoint.CreatedTimestamp = import.ReportEndDate.Timestamp;

      foreach(var item in ReadDashboardStagingPriority2())
      {
        // need to clear the previcously determined totals before the program 
        // begins or the numbers will not be correct, they will reflect previous
        // run numbers also
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

    foreach(var item in ReadInfrastructure())
    {
      local.Checkpoint.CreatedTimestamp =
        entities.Infrastructure.CreatedTimestamp;

      if (ReadCase())
      {
        if (!IsEmpty(entities.Case1.NoJurisdictionCd))
        {
          continue;
        }
      }

      local.DashboardStagingPriority35.ReportLevelId = "";

      if (ReadServiceProviderOfficeServiceProvider())
      {
        // the worker must currently have a CO role
        local.DashboardStagingPriority35.ReportLevelId =
          entities.ServiceProvider.UserId;
      }

      if (IsEmpty(local.DashboardStagingPriority35.ReportLevelId))
      {
        continue;
      }

      // the worker not only has to have a active CO role but also
      //  must be assigned to a case during the time period in question.
      if (ReadCaseAssignment())
      {
        // ok can be counted
      }
      else
      {
        // no cases currently assigned so the worker can not be counted
        continue;
      }

      local.DashboardAuditData.Assign(local.InitializedDashboardAuditData);
      local.DashboardStagingPriority35.Assign(
        local.InitializedDashboardStagingPriority35);
      local.ReferanceDate.Date = entities.Infrastructure.ReferenceDate;
      local.DashboardStagingPriority35.AsOfDate =
        import.ProgramProcessingInfo.ProcessDate;
      local.DashboardStagingPriority35.ReportLevel = "CW";
      local.DashboardStagingPriority35.ReportLevelId =
        entities.ServiceProvider.UserId;
      local.DashboardStagingPriority35.ReportMonth =
        import.DashboardAuditData.ReportMonth;
      local.DashboardStagingPriority35.CaseReviews =
        local.DashboardStagingPriority35.CaseReviews.GetValueOrDefault() + 1;
      local.DashboardAuditData.WorkerId =
        local.DashboardStagingPriority35.ReportLevelId;
      local.DashboardAuditData.DashboardPriority = "5-24";
      local.DashboardAuditData.CaseNumber = entities.Infrastructure.CaseNumber;
      local.DashboardAuditData.ReviewDate =
        entities.Infrastructure.ReferenceDate;

      if (AsChar(import.AuditFlag.Flag) == 'Y')
      {
        // -- Log to the dashboard audit table.
        UseFnB734CreateDashboardAudit();

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
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.RecordProcessed.Count;

      if (local.RecordProcessed.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-110    infrasturcture createtimestamp
        local.BatchTimestampWorkArea.IefTimestamp =
          local.Checkpoint.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-24    " +
          " " + local.BatchTimestampWorkArea.TextTimestamp;
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-25    ";
      
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
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var caseReviews =
      local.DashboardStagingPriority35.CaseReviews.GetValueOrDefault();

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
        db.SetNullableInt32(command, "caseReviews", caseReviews);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CaseReviews = caseReviews;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 4);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 5);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
        db.
          SetInt32(command, "spdId", entities.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
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
        entities.DashboardStagingPriority35.CaseReviews =
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
        entities.DashboardStagingPriority35.CaseReviews =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Checkpoint.CreatedTimestamp.GetValueOrDefault());
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 2);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 3);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 4);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 6);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", entities.Infrastructure.CreatedBy);
        db.SetDate(
          command, "effectiveDate",
          entities.Infrastructure.ReferenceDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var caseReviews =
      entities.DashboardStagingPriority35.CaseReviews.GetValueOrDefault() + 1;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "caseReviews", caseReviews);
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
    entities.DashboardStagingPriority35.CaseReviews = caseReviews;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "caseReviews", 0);
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

    entities.DashboardStagingPriority35.CaseReviews = 0;
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
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of Checkpoint.
    /// </summary>
    [JsonPropertyName("checkpoint")]
    public Infrastructure Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
    }

    /// <summary>
    /// A value of ReferanceDate.
    /// </summary>
    [JsonPropertyName("referanceDate")]
    public DateWorkArea ReferanceDate
    {
      get => referanceDate ??= new();
      set => referanceDate = value;
    }

    /// <summary>
    /// A value of InitializedDashboardAuditData.
    /// </summary>
    [JsonPropertyName("initializedDashboardAuditData")]
    public DashboardAuditData InitializedDashboardAuditData
    {
      get => initializedDashboardAuditData ??= new();
      set => initializedDashboardAuditData = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of InitializedDashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("initializedDashboardStagingPriority35")]
    public DashboardStagingPriority35 InitializedDashboardStagingPriority35
    {
      get => initializedDashboardStagingPriority35 ??= new();
      set => initializedDashboardStagingPriority35 = value;
    }

    private DateWorkArea null1;
    private Infrastructure checkpoint;
    private DateWorkArea referanceDate;
    private DashboardAuditData initializedDashboardAuditData;
    private DateWorkArea end;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Case1 case1;
    private DashboardAuditData dashboardAuditData;
    private Common recordProcessed;
    private ProgramCheckpointRestart programCheckpointRestart;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private DashboardStagingPriority35 initializedDashboardStagingPriority35;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private Infrastructure infrastructure;
    private Case1 case1;
  }
#endregion
}
