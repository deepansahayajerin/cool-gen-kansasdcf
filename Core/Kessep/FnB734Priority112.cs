﻿// Program: FN_B734_PRIORITY_1_12, ID: 945132080, model: 746.
// Short name: SWE03093
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
/// A program: FN_B734_PRIORITY_1_12.
/// </para>
/// <para>
/// Priority 1-12: Collections per FTE (full-time equivalent)
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority112: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_12 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority112(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority112.
  /// </summary>
  public FnB734Priority112(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 03/22/13  GVandy	CQ36547		Initial Development.
    // 			Segment B	
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-12: Collections Per FTE (full-time equivalent)
    // -------------------------------------------------------------------------------------
    // Total Collections divided by Total FTE
    // FTE will need to be manually entered.  Business will need to decide how 
    // to calculate/capture this.
    // Total Collections as defined above (for whatever reporting period being 
    // captured; report month, FFY To Date)
    // Report Level: State, Judicial District
    // Report Period: Month
    // Numerator
    // 	1) See Priority 1.6
    // Denominator
    // 	1) Manual entry.
    // -------------------------------------------------------------------------------------
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Contractor.Index = -1;
    local.Contractor.Count = 0;

    // ------------------------------------------------------------------------------
    // -- Calculate the full time equivalent (FTE) count for each judicial 
    // district.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadCseOrganization())
    {
      if (Verify(entities.JudicialDistrict.Code, "0123456789") != 0)
      {
        continue;
      }

      local.Local1.Index =
        (int)(StringToNumber(entities.JudicialDistrict.Code) - 1);
      local.Local1.CheckSize();

      foreach(var item1 in ReadOffice())
      {
        if (ReadOfficeStaffing())
        {
          local.Local1.Update.GofficeStaffing.FullTimeEquivalent =
            local.Local1.Item.GofficeStaffing.FullTimeEquivalent.
              GetValueOrDefault() + entities
            .OfficeStaffing.FullTimeEquivalent.GetValueOrDefault();
          local.Statewide.FullTimeEquivalent =
            local.Statewide.FullTimeEquivalent.GetValueOrDefault() + entities
            .OfficeStaffing.FullTimeEquivalent.GetValueOrDefault();
        }
      }

      // -- Determine contractor from the judicial district to which they are 
      // assigned on the report period end date.
      local.DashboardAuditData.JudicialDistrict =
        entities.JudicialDistrict.Code;
      UseFnB734DeterContractorFromJd();
      local.Local1.Update.GdashboardStagingPriority12.ContractorNumber =
        local.Contractor1.Code;

      if (local.Contractor.Count < 1)
      {
        ++local.Contractor.Index;
        local.Contractor.CheckSize();

        local.Contractor.Update.GlocalContractorDashboardStagingPriority12.
          ContractorNumber = local.Contractor1.Code;
        local.Contractor.Update.GlocalContractorOfficeStaffing.
          FullTimeEquivalent =
            local.Contractor.Item.GlocalContractorOfficeStaffing.
            FullTimeEquivalent.GetValueOrDefault() + local
          .Local1.Item.GofficeStaffing.FullTimeEquivalent.GetValueOrDefault();
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
            local.Contractor.Item.GlocalContractorDashboardStagingPriority12.
              ContractorNumber))
          {
            local.Contractor.Update.GlocalContractorOfficeStaffing.
              FullTimeEquivalent =
                local.Local1.Item.GofficeStaffing.FullTimeEquivalent.
                GetValueOrDefault() + local
              .Contractor.Item.GlocalContractorOfficeStaffing.
                FullTimeEquivalent.GetValueOrDefault();

            goto ReadEach;
          }
        }

        local.Contractor.CheckIndex();

        local.Contractor.Index = local.Contractor.Count;
        local.Contractor.CheckSize();

        local.Contractor.Update.GlocalContractorDashboardStagingPriority12.
          ContractorNumber = local.Contractor1.Code;
        local.Contractor.Update.GlocalContractorOfficeStaffing.
          FullTimeEquivalent =
            local.Contractor.Item.GlocalContractorOfficeStaffing.
            FullTimeEquivalent.GetValueOrDefault() + local
          .Local1.Item.GofficeStaffing.FullTimeEquivalent.GetValueOrDefault();
      }

ReadEach:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Calculate the Numerator, Denominator, and Average using values 
    // previously
    // -- stored during processing for collections in month and cases under 
    // order
    // -- numerator.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority1())
    {
      local.Temp.CollectionsPerFteDenominator = 0;
      local.Temp.CollectionsPerFteNumerator =
        entities.DashboardStagingPriority12.CollectionsInMonthActual;

      switch(TrimEnd(entities.DashboardStagingPriority12.ReportLevel))
      {
        case "ST":
          local.Temp.CollectionsPerFteDenominator =
            local.Statewide.FullTimeEquivalent.GetValueOrDefault();

          break;
        case "JD":
          local.Local1.Index =
            (int)(StringToNumber(
              entities.DashboardStagingPriority12.ReportLevelId) - 1);
          local.Local1.CheckSize();

          local.Temp.CollectionsPerFteDenominator =
            local.Local1.Item.GofficeStaffing.FullTimeEquivalent.
              GetValueOrDefault();

          break;
        case "XJ":
          for(local.Contractor.Index = 0; local.Contractor.Index < local
            .Contractor.Count; ++local.Contractor.Index)
          {
            if (!local.Contractor.CheckSize())
            {
              break;
            }

            if (Equal(entities.DashboardStagingPriority12.ReportLevelId,
              local.Contractor.Item.GlocalContractorDashboardStagingPriority12.
                ContractorNumber))
            {
              local.Temp.CollectionsPerFteDenominator =
                local.Contractor.Item.GlocalContractorOfficeStaffing.
                  FullTimeEquivalent.GetValueOrDefault();

              goto Test;
            }
          }

          local.Contractor.CheckIndex();

          break;
        default:
          break;
      }

Test:

      if (local.Temp.CollectionsPerFteDenominator.GetValueOrDefault() == 0)
      {
        local.Temp.CollectionsPerFteAverage = 0;
      }
      else
      {
        local.Temp.CollectionsPerFteAverage =
          Math.Round(
            local.Temp.CollectionsPerFteNumerator.GetValueOrDefault() /
          local.Temp.CollectionsPerFteDenominator.GetValueOrDefault(), 2,
          MidpointRounding.AwayFromZero);
      }

      try
      {
        UpdateDashboardStagingPriority1();
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
    local.PrevRank.CollectionsPerFteAverage = 0;
    local.Temp.CollectionsPerFteRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority2())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority12.CollectionsPerFteAverage,
        local.PrevRank.CollectionsPerFteAverage.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CollectionsPerFteRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority2();
        local.PrevRank.CollectionsPerFteAverage =
          entities.DashboardStagingPriority12.CollectionsPerFteAverage;
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
    local.PrevRank.CollectionsPerFteAverage = 0;
    local.Temp.CollectionsPerFteRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDashboardStagingPriority3())
    {
      ++local.Common.Count;

      if (Equal(entities.DashboardStagingPriority12.CollectionsPerFteAverage,
        local.PrevRank.CollectionsPerFteAverage.GetValueOrDefault()))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CollectionsPerFteRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority2();
        local.PrevRank.CollectionsPerFteAverage =
          entities.DashboardStagingPriority12.CollectionsPerFteAverage;
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-10    ";
      
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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCseOrganization()
  {
    entities.JudicialDistrict.Populated = false;

    return ReadEach("ReadCseOrganization",
      null,
      (db, reader) =>
      {
        entities.JudicialDistrict.Code = db.GetString(reader, 0);
        entities.JudicialDistrict.Type1 = db.GetString(reader, 1);
        entities.JudicialDistrict.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority1",
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
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CasesPerFteNumerator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.CasesPerFteDenominator =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CasesPerFteAverage =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CasesPerFteRank =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.CollectionsPerFteNumerator =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsPerFteDenominator =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsPerFteAverage =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CollectionsPerFteRank =
          db.GetNullableInt32(reader, 13);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 14);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority2()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority2",
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
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CasesPerFteNumerator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.CasesPerFteDenominator =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CasesPerFteAverage =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CasesPerFteRank =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.CollectionsPerFteNumerator =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsPerFteDenominator =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsPerFteAverage =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CollectionsPerFteRank =
          db.GetNullableInt32(reader, 13);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 14);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority3()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return ReadEach("ReadDashboardStagingPriority3",
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
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CasesPerFteNumerator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.CasesPerFteDenominator =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CasesPerFteAverage =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CasesPerFteRank =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.CollectionsPerFteNumerator =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsPerFteDenominator =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsPerFteAverage =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.CollectionsPerFteRank =
          db.GetNullableInt32(reader, 13);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 14);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.Office2.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cogChildType", entities.JudicialDistrict.Type1);
        db.SetString(command, "cogChildCode", entities.JudicialDistrict.Code);
      },
      (db, reader) =>
      {
        entities.Office2.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office2.CogTypeCode = db.GetNullableString(reader, 1);
        entities.Office2.CogCode = db.GetNullableString(reader, 2);
        entities.Office2.EffectiveDate = db.GetDate(reader, 3);
        entities.Office2.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Office2.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office2.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeStaffing()
  {
    entities.OfficeStaffing.Populated = false;

    return Read("ReadOfficeStaffing",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.Office2.SystemGeneratedId);
        db.
          SetInt32(command, "yearMonth", import.DashboardAuditData.ReportMonth);
          
      },
      (db, reader) =>
      {
        entities.OfficeStaffing.YearMonth = db.GetInt32(reader, 0);
        entities.OfficeStaffing.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 1);
        entities.OfficeStaffing.OffGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeStaffing.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var collectionsPerFteNumerator =
      local.Temp.CollectionsPerFteNumerator.GetValueOrDefault();
    var collectionsPerFteDenominator =
      local.Temp.CollectionsPerFteDenominator.GetValueOrDefault();
    var collectionsPerFteAverage =
      local.Temp.CollectionsPerFteAverage.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "collPerFteNum", collectionsPerFteNumerator);
        db.SetNullableDecimal(
          command, "collPerFteDen", collectionsPerFteDenominator);
        db.
          SetNullableDecimal(command, "collPerFteAvg", collectionsPerFteAverage);
          
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

    entities.DashboardStagingPriority12.CollectionsPerFteNumerator =
      collectionsPerFteNumerator;
    entities.DashboardStagingPriority12.CollectionsPerFteDenominator =
      collectionsPerFteDenominator;
    entities.DashboardStagingPriority12.CollectionsPerFteAverage =
      collectionsPerFteAverage;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var collectionsPerFteRank =
      local.Temp.CollectionsPerFteRank.GetValueOrDefault();

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "collPerFteRnk", collectionsPerFteRank);
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

    entities.DashboardStagingPriority12.CollectionsPerFteRank =
      collectionsPerFteRank;
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

    private DashboardAuditData dashboardAuditData;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea reportStartDate;
    private DateWorkArea reportEndDate;
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
      /// A value of GdashboardStagingPriority12.
      /// </summary>
      [JsonPropertyName("gdashboardStagingPriority12")]
      public DashboardStagingPriority12 GdashboardStagingPriority12
      {
        get => gdashboardStagingPriority12 ??= new();
        set => gdashboardStagingPriority12 = value;
      }

      /// <summary>
      /// A value of GofficeStaffing.
      /// </summary>
      [JsonPropertyName("gofficeStaffing")]
      public OfficeStaffing GofficeStaffing
      {
        get => gofficeStaffing ??= new();
        set => gofficeStaffing = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private DashboardStagingPriority12 gdashboardStagingPriority12;
      private OfficeStaffing gofficeStaffing;
    }

    /// <summary>A ContractorGroup group.</summary>
    [Serializable]
    public class ContractorGroup
    {
      /// <summary>
      /// A value of GlocalContractorDashboardStagingPriority12.
      /// </summary>
      [JsonPropertyName("glocalContractorDashboardStagingPriority12")]
      public DashboardStagingPriority12 GlocalContractorDashboardStagingPriority12
        
      {
        get => glocalContractorDashboardStagingPriority12 ??= new();
        set => glocalContractorDashboardStagingPriority12 = value;
      }

      /// <summary>
      /// A value of GlocalContractorOfficeStaffing.
      /// </summary>
      [JsonPropertyName("glocalContractorOfficeStaffing")]
      public OfficeStaffing GlocalContractorOfficeStaffing
      {
        get => glocalContractorOfficeStaffing ??= new();
        set => glocalContractorOfficeStaffing = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private DashboardStagingPriority12 glocalContractorDashboardStagingPriority12;
        
      private OfficeStaffing glocalContractorOfficeStaffing;
    }

    /// <summary>
    /// A value of Statewide.
    /// </summary>
    [JsonPropertyName("statewide")]
    public OfficeStaffing Statewide
    {
      get => statewide ??= new();
      set => statewide = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public DashboardStagingPriority12 Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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

    private OfficeStaffing statewide;
    private Array<LocalGroup> local1;
    private Common common;
    private DashboardStagingPriority12 prevRank;
    private DashboardStagingPriority12 temp;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DashboardAuditData dashboardAuditData;
    private CseOrganization contractor1;
    private Array<ContractorGroup> contractor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of Office1.
    /// </summary>
    [JsonPropertyName("office1")]
    public CseOrganization Office1
    {
      get => office1 ??= new();
      set => office1 = value;
    }

    /// <summary>
    /// A value of Office2.
    /// </summary>
    [JsonPropertyName("office2")]
    public Office Office2
    {
      get => office2 ??= new();
      set => office2 = value;
    }

    /// <summary>
    /// A value of OfficeStaffing.
    /// </summary>
    [JsonPropertyName("officeStaffing")]
    public OfficeStaffing OfficeStaffing
    {
      get => officeStaffing ??= new();
      set => officeStaffing = value;
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
    /// A value of JudicialDistrict.
    /// </summary>
    [JsonPropertyName("judicialDistrict")]
    public CseOrganization JudicialDistrict
    {
      get => judicialDistrict ??= new();
      set => judicialDistrict = value;
    }

    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization office1;
    private Office office2;
    private OfficeStaffing officeStaffing;
    private DashboardStagingPriority12 dashboardStagingPriority12;
    private CseOrganization judicialDistrict;
  }
#endregion
}
