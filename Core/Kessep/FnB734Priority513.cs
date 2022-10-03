// Program: FN_B734_PRIORITY_5_13, ID: 945148983, model: 746.
// Short name: SWE03722
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
/// A program: FN_B734_PRIORITY_5_13.
/// </para>
/// <para>
/// Priority 3-13: Collections by Type
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority513: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_5_13 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority513(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority513.
  /// </summary>
  public FnB734Priority513(IContext context, Import import, Export export):
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
    // Priority 3-13: Collections by Type
    // -------------------------------------------------------------------------------------
    // Report Level: State, Judicial District, Region, Office, Supervisor, 
    // Caseworker
    // Report Period: Month
    // Use Definition of Collections from Priorities 1-5 and 1-6 and also use 
    // the following rules:
    // 1)	Credit collections to caseworker assigned to the case as of the 
    // refresh date.
    // 2)	Sort by collection types: S, F, I, U, C.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData,
      local.InitializedDashboardAuditData);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "5-13"))
      {
        local.Restart.SystemGeneratedIdentifier =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));

        if (local.Restart.SystemGeneratedIdentifier > 0)
        {
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          local.Restart.SystemGeneratedIdentifier = 0;

          foreach(var item in ReadDashboardStagingPriority3())
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
        local.Restart.SystemGeneratedIdentifier = 0;

        foreach(var item in ReadDashboardStagingPriority3())
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
      local.Restart.SystemGeneratedIdentifier = 0;

      foreach(var item in ReadDashboardStagingPriority3())
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

    // -------------------------------------------------------------------
    // Read Each is sorted in Asc order of Supp Person #.
    // -------------------------------------------------------------------
    foreach(var item in ReadCollectionObligationTypeCsePerson())
    {
      if (ReadCollectionType())
      {
        // -- Skip CSENet collections.
        if (entities.CollectionType.SequentialIdentifier == 27 || entities
          .CollectionType.SequentialIdentifier == 28 || entities
          .CollectionType.SequentialIdentifier == 29)
        {
          continue;
        }
      }

      local.Prev.SystemGeneratedIdentifier =
        entities.Collection.SystemGeneratedIdentifier;
      ++local.RecordsReadSinceCommit.Count;
      local.DashboardAuditData.Assign(local.InitializedDashboardAuditData);
      local.DashboardStagingPriority35.Assign(
        local.InitializedDashboardStagingPriority35);

      // -------------------------------------------------------------------------------------
      // -- Include collection in the in month amount.
      // -------------------------------------------------------------------------------------
      if (Lt(entities.Collection.CreatedTmst, import.ReportStartDate.Timestamp))
      {
        // -----------------------------------------------------------------
        // This must be an adjustment to a collection created in prev report 
        // period.
        // -----------------------------------------------------------------
        local.DashboardAuditData.CollectionAmount = -entities.Collection.Amount;
      }
      else
      {
        local.DashboardAuditData.CollectionAmount = entities.Collection.Amount;
      }

      // -- Determine Judicial District...
      if (AsChar(entities.ObligationType.Classification) == 'F')
      {
        if (!ReadLegalActionDetail())
        {
          goto Test;
        }

        // -- For Fees on non Kansas orders, use the case entered on LOPS for 
        // the Obligor to determine the Judicial District.
        if (ReadCase())
        {
          local.DashboardAuditData.CaseNumber = entities.Case1.Number;
        }
      }
      else
      {
        // -- For non Fees, use the order to determine Judicial District.
        local.UseApSupportedOnly.Flag = "Y";
        UseFnB734DetermineJdFromOrder();
      }

Test:

      if (IsEmpty(local.DashboardAuditData.CaseNumber))
      {
        continue;

        // no case found so we can not determine a service provider, can not 
        // count this
        // collection type go to next collection
      }

      local.DashboardAuditData.WorkerId = "";

      if (!IsEmpty(local.DashboardAuditData.CaseNumber))
      {
        if (ReadCaseAssignmentServiceProvider())
        {
          local.DashboardAuditData.WorkerId = entities.ServiceProvider.UserId;
        }
      }

      // -- Log to the audit table.
      if (AsChar(entities.ObligationType.Classification) != 'F')
      {
        if (ReadCsePerson())
        {
          local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
        }
      }

      local.DashboardAuditData.PayorCspNumber = entities.Ap.Number;
      local.DashboardAuditData.CollectionCreatedDate =
        Date(entities.Collection.CreatedTmst);
      local.DashboardAuditData.DashboardPriority = "5-13" + local
        .ReportingAbbreviation.Text2;
      local.DashboardAuditData.CollAppliedToCd =
        entities.Collection.AppliedToCode;
      local.DashboardAuditData.CollectionType = entities.CollectionType.Code;

      // @@@@@  This is where you need to keep track of the collection amounts 
      // by type...
      switch(TrimEnd(entities.CollectionType.Code))
      {
        case "S":
          local.DashboardStagingPriority35.StypeCollectionAmount =
            local.DashboardStagingPriority35.StypeCollectionAmount.
              GetValueOrDefault() + local
            .DashboardAuditData.CollectionAmount.GetValueOrDefault();

          // set g_local dashboard_staging_priority_35 s_type_collection_amount 
          // to g_local dashboard_staging_priority_35 s_type_collection_amount +
          // local dashboard_audit_data collection_amount
          break;
        case "F":
          local.DashboardStagingPriority35.FtypeCollectionAmount =
            local.DashboardStagingPriority35.FtypeCollectionAmount.
              GetValueOrDefault() + local
            .DashboardAuditData.CollectionAmount.GetValueOrDefault();

          // set g_local dashboard_staging_priority_35 f_type_collection_amount 
          // to g_local dashboard_staging_priority_35 f_type_collection_amount +
          // local dashboard_audit_data collection_amount
          break;
        case "I":
          local.DashboardStagingPriority35.ItypeCollectionAmount =
            local.DashboardStagingPriority35.ItypeCollectionAmount.
              GetValueOrDefault() + local
            .DashboardAuditData.CollectionAmount.GetValueOrDefault();

          // set g_local dashboard_staging_priority_35 i_type_collection_amount 
          // to g_local dashboard_staging_priority_35 i_type_collection_amount +
          // local dashboard_audit_data collection_amount
          break;
        case "U":
          local.DashboardStagingPriority35.UtypeCollectionAmount =
            local.DashboardStagingPriority35.UtypeCollectionAmount.
              GetValueOrDefault() + local
            .DashboardAuditData.CollectionAmount.GetValueOrDefault();

          // set g_local dashboard_staging_priority_35 u_type_collection_amount 
          // to g_local dashboard_staging_priority_35 u_type_collection_amount +
          // local dashboard_audit_data collection_amount
          break;
        case "C":
          local.DashboardStagingPriority35.CtypeCollectionAmount =
            local.DashboardStagingPriority35.CtypeCollectionAmount.
              GetValueOrDefault() + local
            .DashboardAuditData.CollectionAmount.GetValueOrDefault();

          // set g_local dashboard_staging_priority_35 c_type_collection_amount 
          // to g_local dashboard_staging_priority_35 c_type_collection_amount +
          // local dashboard_audit_data collection_amount
          break;
        default:
          break;
      }

      local.DashboardStagingPriority35.TotalCollectionAmount =
        local.DashboardStagingPriority35.TotalCollectionAmount.
          GetValueOrDefault() + local
        .DashboardAuditData.CollectionAmount.GetValueOrDefault();

      // set g_local dashboard_staging_priority_35 total_collection_amount to 
      // g_local dashboard_staging_priority_35 total_collection_amount + local
      // dashboard_audit_data collection_amount
      if (!IsEmpty(local.DashboardAuditData.WorkerId))
      {
        local.DashboardStagingPriority35.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.DashboardStagingPriority35.ReportLevel = "CW";
        local.DashboardStagingPriority35.ReportMonth =
          import.DashboardAuditData.ReportMonth;
        local.DashboardAuditData.DashboardPriority = "5-13.1";
        local.DashboardStagingPriority35.ReportLevelId =
          local.DashboardAuditData.WorkerId ?? Spaces(8);

        // -- Determine office and judicial district to which case is assigned 
        // on the report period end date.
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
      }

      local.DashboardAuditData.WorkerId = "";
      local.LegalAction.StandardNumber = "";

      if (ReadLegalAction())
      {
        local.LegalAction.StandardNumber = entities.LegalAction.StandardNumber;
      }

      if (!IsEmpty(local.LegalAction.StandardNumber))
      {
        if (ReadLegalActionLegalActionAssigmentServiceProvider())
        {
          local.DashboardAuditData.WorkerId = entities.ServiceProvider.UserId;
        }

        if (!IsEmpty(local.DashboardAuditData.WorkerId))
        {
          local.DashboardStagingPriority35.AsOfDate =
            import.ProgramProcessingInfo.ProcessDate;

          if (Equal(entities.ServiceProvider.RoleCode, "AT") || Equal
            (entities.ServiceProvider.RoleCode, "CT"))
          {
            local.DashboardStagingPriority35.ReportLevel = "AT";
          }
          else
          {
            local.DashboardStagingPriority35.ReportLevel = "CA";
          }

          local.DashboardStagingPriority35.ReportLevelId =
            local.DashboardAuditData.WorkerId ?? Spaces(8);
          local.DashboardStagingPriority35.ReportMonth =
            import.DashboardAuditData.ReportMonth;
          local.DashboardAuditData.DashboardPriority = "5-13.2";

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
        }
      }

      // ------------------------------------------------------------------------------
      // -- Checkpoint saving all the info needed for restarting.
      // ------------------------------------------------------------------------------
      if (local.RecordsReadSinceCommit.Count >= import
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-098    Collection System Generated Identifier
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-13    " +
          " " + NumberToString(local.Prev.SystemGeneratedIdentifier, 7, 9);
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

    // @@@@@@  Here you need to calculate the X_type_percent_of_total for 
    // judicial districts.
    // ------------------------------------------------------------------------------
    // -- Calculate the Attorney/Case Worker Percent Change.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority2())
    {
      local.DashboardStagingPriority35.Assign(
        entities.DashboardStagingPriority35);

      if (local.DashboardStagingPriority35.CtypeCollectionAmount.
        GetValueOrDefault() == 0)
      {
        local.DashboardStagingPriority35.CtypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.CtypePercentOfTotal =
          local.DashboardStagingPriority35.CtypeCollectionAmount.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.TotalCollectionAmount.GetValueOrDefault();
          
      }

      if (local.DashboardStagingPriority35.FtypeCollectionAmount.
        GetValueOrDefault() == 0)
      {
        local.DashboardStagingPriority35.FtypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.FtypePercentOfTotal =
          local.DashboardStagingPriority35.FtypeCollectionAmount.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.TotalCollectionAmount.GetValueOrDefault();
          
      }

      if (local.DashboardStagingPriority35.ItypeCollectionAmount.
        GetValueOrDefault() == 0)
      {
        local.DashboardStagingPriority35.ItypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.ItypePercentOfTotal =
          local.DashboardStagingPriority35.ItypeCollectionAmount.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.TotalCollectionAmount.GetValueOrDefault();
          
      }

      if (local.DashboardStagingPriority35.StypeCollectionAmount.
        GetValueOrDefault() == 0)
      {
        local.DashboardStagingPriority35.StypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.StypePercentOfTotal =
          local.DashboardStagingPriority35.StypeCollectionAmount.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.TotalCollectionAmount.GetValueOrDefault();
          
      }

      if (local.DashboardStagingPriority35.UtypeCollectionAmount.
        GetValueOrDefault() == 0)
      {
        local.DashboardStagingPriority35.UtypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.UtypePercentOfTotal =
          local.DashboardStagingPriority35.UtypeCollectionAmount.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.TotalCollectionAmount.GetValueOrDefault();
          
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

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_3_5.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-15    ";
      
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

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;
    useImport.UseApSupportedOnly.Flag = local.UseApSupportedOnly.Flag;

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
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var stypeCollectionAmount =
      local.DashboardStagingPriority35.StypeCollectionAmount.
        GetValueOrDefault();
    var ftypeCollectionAmount =
      local.DashboardStagingPriority35.FtypeCollectionAmount.
        GetValueOrDefault();
    var itypeCollectionAmount =
      local.DashboardStagingPriority35.ItypeCollectionAmount.
        GetValueOrDefault();
    var utypeCollectionAmount =
      local.DashboardStagingPriority35.UtypeCollectionAmount.
        GetValueOrDefault();
    var ctypeCollectionAmount =
      local.DashboardStagingPriority35.CtypeCollectionAmount.
        GetValueOrDefault();
    var totalCollectionAmount =
      local.DashboardStagingPriority35.TotalCollectionAmount.
        GetValueOrDefault();

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
        db.SetNullableDecimal(command, "STypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableDecimal(command, "FTypeCollAmt", ftypeCollectionAmount);
        db.SetNullableDecimal(command, "FTypeCollPer", param);
        db.SetNullableDecimal(command, "ITypeCollAmt", itypeCollectionAmount);
        db.SetNullableDecimal(command, "ITypeCollPer", param);
        db.SetNullableDecimal(command, "UTypeCollAmt", utypeCollectionAmount);
        db.SetNullableDecimal(command, "UTypeCollPer", param);
        db.SetNullableDecimal(command, "CTypeCollAmt", ctypeCollectionAmount);
        db.SetNullableDecimal(command, "CTypeCollPer", param);
        db.SetNullableDecimal(command, "totalCollAmt", totalCollectionAmount);
        db.SetNullableDecimal(command, "ordEstDaysAvg", param);
        db.SetNullableInt32(command, "caseloadCount", 0);
        db.SetNullableDecimal(command, "curSupPdYtdDen", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.StypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.StypePercentOfTotal = param;
    entities.DashboardStagingPriority35.FtypeCollectionAmount =
      ftypeCollectionAmount;
    entities.DashboardStagingPriority35.FtypePercentOfTotal = param;
    entities.DashboardStagingPriority35.ItypeCollectionAmount =
      itypeCollectionAmount;
    entities.DashboardStagingPriority35.ItypePercentOfTotal = param;
    entities.DashboardStagingPriority35.UtypeCollectionAmount =
      utypeCollectionAmount;
    entities.DashboardStagingPriority35.UtypePercentOfTotal = param;
    entities.DashboardStagingPriority35.CtypeCollectionAmount =
      ctypeCollectionAmount;
    entities.DashboardStagingPriority35.CtypePercentOfTotal = param;
    entities.DashboardStagingPriority35.TotalCollectionAmount =
      totalCollectionAmount;
    entities.DashboardStagingPriority35.CaseloadCount = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 5);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignmentServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "casNo", local.DashboardAuditData.CaseNumber ?? "");
          
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
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
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.ServiceProvider.UserId = db.GetString(reader, 10);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 11);
        entities.ServiceProvider.Populated = true;
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTypeCsePerson()
  {
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadCollectionObligationTypeCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetInt32(command, "collId", local.Restart.SystemGeneratedIdentifier);
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
        entities.Ap.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.ObligationType.Code = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supp.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supp.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.
          SetInt32(command, "reportMonth", local.DashboardAuditData.ReportMonth);
          
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
        entities.DashboardStagingPriority35.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority35.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority35.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority35.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority35.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority35.CaseloadCount =
          db.GetNullableInt32(reader, 15);
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
        entities.DashboardStagingPriority35.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority35.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority35.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority35.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority35.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority35.CaseloadCount =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority3()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return ReadEach("ReadDashboardStagingPriority3",
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
        entities.DashboardStagingPriority35.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority35.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority35.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority35.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority35.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority35.CaseloadCount =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Collection.OtyId);
        db.SetInt32(command, "obId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Collection.OtyId);
        db.SetInt32(command, "obId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 2);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private bool ReadLegalActionLegalActionAssigmentServiceProvider()
  {
    entities.LegalActionAssigment.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionLegalActionAssigmentServiceProvider",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
        db.SetDate(
          command, "effectiveDt",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDt", import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 4);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 7);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 12);
        entities.LegalActionAssigment.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.LegalAction.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var stypeCollectionAmount =
      local.DashboardStagingPriority35.StypeCollectionAmount.
        GetValueOrDefault() +
      entities.DashboardStagingPriority35.StypeCollectionAmount.
        GetValueOrDefault();
    var ftypeCollectionAmount =
      local.DashboardStagingPriority35.FtypeCollectionAmount.
        GetValueOrDefault() +
      entities.DashboardStagingPriority35.FtypeCollectionAmount.
        GetValueOrDefault();
    var itypeCollectionAmount =
      local.DashboardStagingPriority35.ItypeCollectionAmount.
        GetValueOrDefault() +
      entities.DashboardStagingPriority35.ItypeCollectionAmount.
        GetValueOrDefault();
    var utypeCollectionAmount =
      local.DashboardStagingPriority35.UtypeCollectionAmount.
        GetValueOrDefault() +
      entities.DashboardStagingPriority35.UtypeCollectionAmount.
        GetValueOrDefault();
    var ctypeCollectionAmount =
      local.DashboardStagingPriority35.CtypeCollectionAmount.
        GetValueOrDefault() +
      entities.DashboardStagingPriority35.CtypeCollectionAmount.
        GetValueOrDefault();
    var totalCollectionAmount =
      local.DashboardStagingPriority35.TotalCollectionAmount.
        GetValueOrDefault() +
      entities.DashboardStagingPriority35.TotalCollectionAmount.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableDecimal(command, "STypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "FTypeCollAmt", ftypeCollectionAmount);
        db.SetNullableDecimal(command, "ITypeCollAmt", itypeCollectionAmount);
        db.SetNullableDecimal(command, "UTypeCollAmt", utypeCollectionAmount);
        db.SetNullableDecimal(command, "CTypeCollAmt", ctypeCollectionAmount);
        db.SetNullableDecimal(command, "totalCollAmt", totalCollectionAmount);
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
    entities.DashboardStagingPriority35.StypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.FtypeCollectionAmount =
      ftypeCollectionAmount;
    entities.DashboardStagingPriority35.ItypeCollectionAmount =
      itypeCollectionAmount;
    entities.DashboardStagingPriority35.UtypeCollectionAmount =
      utypeCollectionAmount;
    entities.DashboardStagingPriority35.CtypeCollectionAmount =
      ctypeCollectionAmount;
    entities.DashboardStagingPriority35.TotalCollectionAmount =
      totalCollectionAmount;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var stypeCollectionAmount = 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "STypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "STypeCollPer", stypeCollectionAmount);
        db.SetNullableDecimal(command, "FTypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "FTypeCollPer", stypeCollectionAmount);
        db.SetNullableDecimal(command, "ITypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "ITypeCollPer", stypeCollectionAmount);
        db.SetNullableDecimal(command, "UTypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "UTypeCollPer", stypeCollectionAmount);
        db.SetNullableDecimal(command, "CTypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "CTypeCollPer", stypeCollectionAmount);
        db.SetNullableDecimal(command, "totalCollAmt", stypeCollectionAmount);
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

    entities.DashboardStagingPriority35.StypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.StypePercentOfTotal =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.FtypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.FtypePercentOfTotal =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.ItypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.ItypePercentOfTotal =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.UtypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.UtypePercentOfTotal =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.CtypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.CtypePercentOfTotal =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.TotalCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var stypePercentOfTotal =
      local.DashboardStagingPriority35.StypePercentOfTotal.GetValueOrDefault();
    var ftypePercentOfTotal =
      local.DashboardStagingPriority35.FtypePercentOfTotal.GetValueOrDefault();
    var itypePercentOfTotal =
      local.DashboardStagingPriority35.ItypePercentOfTotal.GetValueOrDefault();
    var utypePercentOfTotal =
      local.DashboardStagingPriority35.UtypePercentOfTotal.GetValueOrDefault();
    var ctypePercentOfTotal =
      local.DashboardStagingPriority35.CtypePercentOfTotal.GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "STypeCollPer", stypePercentOfTotal);
        db.SetNullableDecimal(command, "FTypeCollPer", ftypePercentOfTotal);
        db.SetNullableDecimal(command, "ITypeCollPer", itypePercentOfTotal);
        db.SetNullableDecimal(command, "UTypeCollPer", utypePercentOfTotal);
        db.SetNullableDecimal(command, "CTypeCollPer", ctypePercentOfTotal);
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

    entities.DashboardStagingPriority35.StypePercentOfTotal =
      stypePercentOfTotal;
    entities.DashboardStagingPriority35.FtypePercentOfTotal =
      ftypePercentOfTotal;
    entities.DashboardStagingPriority35.ItypePercentOfTotal =
      itypePercentOfTotal;
    entities.DashboardStagingPriority35.UtypePercentOfTotal =
      utypePercentOfTotal;
    entities.DashboardStagingPriority35.CtypePercentOfTotal =
      ctypePercentOfTotal;
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
    /// <summary>
    /// A value of InitializedDashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("initializedDashboardStagingPriority35")]
    public DashboardStagingPriority35 InitializedDashboardStagingPriority35
    {
      get => initializedDashboardStagingPriority35 ??= new();
      set => initializedDashboardStagingPriority35 = value;
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
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public DashboardAuditData Hold
    {
      get => hold ??= new();
      set => hold = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority35")]
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
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
    /// A value of Checkpoint.
    /// </summary>
    [JsonPropertyName("checkpoint")]
    public LegalAction Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Collection Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Collection Prev
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public DateWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private DashboardStagingPriority35 initializedDashboardStagingPriority35;
    private Common useApSupportedOnly;
    private DashboardAuditData hold;
    private LegalAction legalAction;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private DashboardAuditData initializedDashboardAuditData;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscrpitCount;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private LegalAction checkpoint;
    private Collection restart;
    private Collection prev;
    private Common recordsReadSinceCommit;
    private DashboardAuditData dashboardAuditData;
    private DateWorkArea temp;
    private TextWorkArea reportingAbbreviation;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority35")]
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private LegalActionAssigment legalActionAssigment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CseOrganization cseOrganization;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private LegalActionDetail legalActionDetail;
    private Obligation obligation;
    private ObligationTransaction debt;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private Case1 case1;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private CaseAssignment caseAssignment;
    private ObligationType obligationType;
    private CsePerson supp;
    private CsePersonAccount supported;
    private CsePerson ap;
    private CsePersonAccount obligor;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
