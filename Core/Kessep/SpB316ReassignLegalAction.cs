// Program: SP_B316_REASSIGN_LEGAL_ACTION, ID: 374563319, model: 746.
// Short name: SWE03131
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B316_REASSIGN_LEGAL_ACTION.
/// </summary>
[Serializable]
public partial class SpB316ReassignLegalAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B316_REASSIGN_LEGAL_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB316ReassignLegalAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB316ReassignLegalAction.
  /// </summary>
  public SpB316ReassignLegalAction(IContext context, Import import,
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
    // ***********************************************************************
    // This action block reassigns service providers that are assigned to legal 
    // actions
    // that are associated to legal referrals assigned to the provided service 
    // provider.
    // It also reassigns any monitored activities associated to the legal 
    // actions.
    // ***********************************************************************
    // ------------------------------------------------------------------
    //                   M A I N T E N A N C E   L O G
    // Date		Developer	Request #	Description
    // 07/14/10	J Huss		CQ# 20646	Initial development.
    // 02/25/11        R Mathews       CQ# 23949       Add check for OSP 
    // effective date when reading
    //                                                 
    // for new service provider
    // 08/14/13	G Vandy		CQ# 38147	Add support for tribunal.
    // ------------------------------------------------------------------
    if (!Lt(local.Null1.Date, import.ProgramProcessingInfo.ProcessDate))
    {
      local.Process.Date = Now().Date;
    }
    else
    {
      local.Process.Date = import.ProgramProcessingInfo.ProcessDate;
    }

    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.LegalActionAssignment.Count = 0;
    export.MonitoredActivityChange.Count = 0;
    local.EabFileHandling.Action = "WRITE";

    // CQ#23949 Added date edit to OSP read below
    // Find the new service provider
    if (!ReadOfficeServiceProviderOfficeServiceProvider())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    // 08/14/13  GVandy  CQ# 38147  Add support for tribunal.
    // Find all of the legal actions and service providers that are associated 
    // to legal
    // referrals that are currently assigned the provided service provider.
    // -- Read each distinct case with a legal referral assigned to the "from" 
    // service provider.
    foreach(var item in ReadCase())
    {
      // 08/14/13  GVandy  CQ# 38147  Add support for tribunal.
      // 	1) If tribunal is not specified in the input file then only reassign 
      // legal
      // 	   actions if the case has one associated tribunal.
      // 	2) If tribunal is specified on the input file then only reassign legal
      // 	   actions in that tribunal.
      if (import.Tribunal.Identifier == 0)
      {
        local.Tribunal.Count = 0;

        // -- Check for multiple tribunals tied to the case.
        foreach(var item1 in ReadTribunal())
        {
          ++local.Tribunal.Count;
        }

        if (local.Tribunal.Count > 1)
        {
          // -- Log to the control report that case is skipped due to multiple 
          // tribunals.
          local.Temp.Text11 =
            NumberToString(import.FromOffice.SystemGeneratedId, 11, 5) + "/" + NumberToString
            (import.FromServiceProvider.SystemGeneratedId, 11, 5);
          local.EabReportSend.RptDetail = "Skipped - Off/SP " + local
            .Temp.Text11 + " Case # " + entities.Case1.Number + " has associated legal actions in " +
            NumberToString(local.Tribunal.Count, 12, 4) + " Tribunals.";
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          continue;
        }
      }

      foreach(var item1 in ReadLegalActionOfficeServiceProviderOfficeServiceProvider())
        
      {
        // The primary key for legal action assignment is the created timestamp,
        // so this needs to be updated every iteration.
        local.Current.Timestamp = Now();

        // Find any monitored activites associated with the current legal 
        // action.
        foreach(var item2 in ReadMonitoredActivity())
        {
          // Find the assignment associated with this monitored activity.
          if (ReadMonitoredActivityAssignment())
          {
            // End date the previous monitored activity assignment.
            try
            {
              UpdateMonitoredActivityAssignment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // Create a new monitored activity assignment.
            try
            {
              CreateMonitoredActivityAssignment();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            ++export.MonitoredActivityChange.Count;
          }
        }

        // Find the previous legal action assignment and assign it to the new 
        // service provider
        if (ReadLegalActionAssigment())
        {
          // End date the previous legal action assignment
          try
          {
            UpdateLegalActionAssigment();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "LEGAL_ACTION_ASSIGNMENT_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "LEGAL_ACTION_ASSIGNMENT_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // Create the new legal action assignment
          try
          {
            CreateLegalActionAssigment();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "LEGAL_ACTION_ASSIGNMENT_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "LEGAL_ACTION_ASSIGNMENT_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++export.LegalActionAssignment.Count;
        }
        else
        {
          ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";

          return;
        }

        // Increment the update count.  If it's greater than the update 
        // frequency count, then commit.
        ++local.UpdateCount.Count;

        if (local.UpdateCount.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.UpdateCount.Count = 0;
          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_EXT_DO_DB2_COMMIT_AB";

            return;
          }
        }

        local.Old.Text11 =
          NumberToString(entities.OldLegalActionOffice.SystemGeneratedId, 11, 5) +
          "/" + NumberToString
          (entities.OldLegalActionServiceProvider.SystemGeneratedId, 11, 5);
        local.New1.Text11 =
          NumberToString(entities.NewLegalActionOffice.SystemGeneratedId, 11, 5) +
          "/" + NumberToString
          (entities.NewLegalActionServiceProvider.SystemGeneratedId, 11, 5);
        local.LgaId.Text10 =
          NumberToString(entities.LegalAction.Identifier, 6, 10);

        // Detail:  Reassigned Off/SP #####/##### to Off/SP #####/#####.  LGA ID
        // : ##########
        local.EabReportSend.RptDetail = "Reassigned Off/SP " + local
          .Old.Text11 + " to Off/SP " + local.New1.Text11 + ". LGA ID: " + local
          .LgaId.Text10;
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }
      }
    }

    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_EXT_DO_DB2_COMMIT_AB";
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void CreateLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(
      entities.NewLegalActionOfficeServiceProvider.Populated);

    var lgaIdentifier = entities.LegalAction.Identifier;
    var ospEffectiveDate =
      entities.NewLegalActionOfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.NewLegalActionOfficeServiceProvider.RoleCode;
    var offGeneratedId =
      entities.NewLegalActionOfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId =
      entities.NewLegalActionOfficeServiceProvider.SpdGeneratedId;
    var effectiveDate = AddDays(local.Process.Date, 1);
    var discontinueDate = local.Max.Date;
    var reasonCode = entities.OldLegalActionAssigment.ReasonCode;
    var overrideInd = entities.OldLegalActionAssigment.OverrideInd;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = local.Current.Timestamp;

    entities.NewLegalActionAssigment.Populated = false;
    Update("CreateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.NewLegalActionAssigment.LgaIdentifier = lgaIdentifier;
    entities.NewLegalActionAssigment.OspEffectiveDate = ospEffectiveDate;
    entities.NewLegalActionAssigment.OspRoleCode = ospRoleCode;
    entities.NewLegalActionAssigment.OffGeneratedId = offGeneratedId;
    entities.NewLegalActionAssigment.SpdGeneratedId = spdGeneratedId;
    entities.NewLegalActionAssigment.EffectiveDate = effectiveDate;
    entities.NewLegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.NewLegalActionAssigment.ReasonCode = reasonCode;
    entities.NewLegalActionAssigment.OverrideInd = overrideInd;
    entities.NewLegalActionAssigment.CreatedBy = createdBy;
    entities.NewLegalActionAssigment.CreatedTimestamp = createdTimestamp;
    entities.NewLegalActionAssigment.Populated = true;
  }

  private void CreateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.NewLegalActionOfficeServiceProvider.Populated);

    var reasonCode = entities.OldMonitoredActivityAssignment.ReasonCode;
    var responsibilityCode =
      entities.OldMonitoredActivityAssignment.ResponsibilityCode;
    var effectiveDate = AddDays(local.Process.Date, 1);
    var overrideInd = entities.OldMonitoredActivityAssignment.OverrideInd;
    var discontinueDate = local.Max.Date;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = local.Current.Timestamp;
    var spdId = entities.NewLegalActionOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewLegalActionOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewLegalActionOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewLegalActionOfficeServiceProvider.EffectiveDate;
    var macId = entities.MonitoredActivity.SystemGeneratedIdentifier;

    entities.NewMonitoredActivityAssignment.Populated = false;
    Update("CreateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", 0);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "responsibilityCod", responsibilityCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "macId", macId);
      });

    entities.NewMonitoredActivityAssignment.SystemGeneratedIdentifier = 0;
    entities.NewMonitoredActivityAssignment.ReasonCode = reasonCode;
    entities.NewMonitoredActivityAssignment.ResponsibilityCode =
      responsibilityCode;
    entities.NewMonitoredActivityAssignment.EffectiveDate = effectiveDate;
    entities.NewMonitoredActivityAssignment.OverrideInd = overrideInd;
    entities.NewMonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.NewMonitoredActivityAssignment.CreatedBy = createdBy;
    entities.NewMonitoredActivityAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewMonitoredActivityAssignment.SpdId = spdId;
    entities.NewMonitoredActivityAssignment.OffId = offId;
    entities.NewMonitoredActivityAssignment.OspCode = ospCode;
    entities.NewMonitoredActivityAssignment.OspDate = ospDate;
    entities.NewMonitoredActivityAssignment.MacId = macId;
    entities.NewMonitoredActivityAssignment.Populated = true;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Process.Date.GetValueOrDefault());
        db.SetString(
          command, "ospCode", import.FromOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "spdId", import.FromServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.FromOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldLegalActionOfficeServiceProvider.Populated);
    entities.OldLegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Process.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(
          command, "ospRoleCode",
          entities.OldLegalActionOfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OldLegalActionOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OldLegalActionOfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OldLegalActionOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.OldLegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.OldLegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.OldLegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.OldLegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.OldLegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.OldLegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.OldLegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OldLegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.OldLegalActionAssigment.OverrideInd = db.GetString(reader, 8);
        entities.OldLegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OldLegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.OldLegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.OldLegalActionAssigment.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionOfficeServiceProviderOfficeServiceProvider()
  {
    entities.LegalAction.Populated = false;
    entities.OldLegalActionOffice.Populated = false;
    entities.OldLegalActionOfficeServiceProvider.Populated = false;
    entities.OldLegalActionServiceProvider.Populated = false;

    return ReadEach("ReadLegalActionOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Process.Date.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          import.ToServiceProvider.SystemGeneratedId);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.OldLegalActionOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 2);
        entities.OldLegalActionServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 2);
        entities.OldLegalActionOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 3);
        entities.OldLegalActionOffice.SystemGeneratedId =
          db.GetInt32(reader, 3);
        entities.OldLegalActionOfficeServiceProvider.RoleCode =
          db.GetString(reader, 4);
        entities.OldLegalActionOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 5);
        entities.OldLegalActionOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OldLegalActionOffice.OffOffice =
          db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
        entities.OldLegalActionOffice.Populated = true;
        entities.OldLegalActionOfficeServiceProvider.Populated = true;
        entities.OldLegalActionServiceProvider.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 1);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 2);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private bool ReadMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldLegalActionOfficeServiceProvider.Populated);
    entities.OldMonitoredActivityAssignment.Populated = false;

    return Read("ReadMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OldLegalActionOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospCode",
          entities.OldLegalActionOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offId",
          entities.OldLegalActionOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId",
          entities.OldLegalActionOfficeServiceProvider.SpdGeneratedId);
        db.SetInt32(
          command, "macId",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OldMonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.OldMonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 1);
        entities.OldMonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 2);
        entities.OldMonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.OldMonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.OldMonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.OldMonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.OldMonitoredActivityAssignment.SpdId = db.GetInt32(reader, 7);
        entities.OldMonitoredActivityAssignment.OffId = db.GetInt32(reader, 8);
        entities.OldMonitoredActivityAssignment.OspCode =
          db.GetString(reader, 9);
        entities.OldMonitoredActivityAssignment.OspDate =
          db.GetDate(reader, 10);
        entities.OldMonitoredActivityAssignment.MacId = db.GetInt32(reader, 11);
        entities.OldMonitoredActivityAssignment.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider()
  {
    entities.NewLegalActionOffice.Populated = false;
    entities.NewLegalActionOfficeServiceProvider.Populated = false;
    entities.NewLegalActionServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "roleCode", import.ToOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "spdGeneratedId",
          import.ToServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "officeId", import.ToOffice.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewLegalActionOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewLegalActionServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewLegalActionOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewLegalActionOffice.SystemGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewLegalActionOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.NewLegalActionOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.NewLegalActionOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.NewLegalActionOffice.OffOffice =
          db.GetNullableInt32(reader, 5);
        entities.NewLegalActionOffice.Populated = true;
        entities.NewLegalActionOfficeServiceProvider.Populated = true;
        entities.NewLegalActionServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return ReadEach("ReadTribunal",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private void UpdateLegalActionAssigment()
  {
    var discontinueDate = local.Process.Date;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.OldLegalActionAssigment.Populated = false;
    Update("UpdateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.OldLegalActionAssigment.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.OldLegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.OldLegalActionAssigment.LastUpdatedBy = lastUpdatedBy;
    entities.OldLegalActionAssigment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.OldLegalActionAssigment.Populated = true;
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.OldMonitoredActivityAssignment.Populated);

    var discontinueDate = local.Process.Date;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.OldMonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.OldMonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.OldMonitoredActivityAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.OldMonitoredActivityAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.OldMonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.OldMonitoredActivityAssignment.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "macId", entities.OldMonitoredActivityAssignment.MacId);
      });

    entities.OldMonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.OldMonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.OldMonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.OldMonitoredActivityAssignment.Populated = true;
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
    /// A value of FromOffice.
    /// </summary>
    [JsonPropertyName("fromOffice")]
    public Office FromOffice
    {
      get => fromOffice ??= new();
      set => fromOffice = value;
    }

    /// <summary>
    /// A value of ToOffice.
    /// </summary>
    [JsonPropertyName("toOffice")]
    public Office ToOffice
    {
      get => toOffice ??= new();
      set => toOffice = value;
    }

    /// <summary>
    /// A value of FromOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("fromOfficeServiceProvider")]
    public OfficeServiceProvider FromOfficeServiceProvider
    {
      get => fromOfficeServiceProvider ??= new();
      set => fromOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ToOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("toOfficeServiceProvider")]
    public OfficeServiceProvider ToOfficeServiceProvider
    {
      get => toOfficeServiceProvider ??= new();
      set => toOfficeServiceProvider = value;
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
    /// A value of FromServiceProvider.
    /// </summary>
    [JsonPropertyName("fromServiceProvider")]
    public ServiceProvider FromServiceProvider
    {
      get => fromServiceProvider ??= new();
      set => fromServiceProvider = value;
    }

    /// <summary>
    /// A value of ToServiceProvider.
    /// </summary>
    [JsonPropertyName("toServiceProvider")]
    public ServiceProvider ToServiceProvider
    {
      get => toServiceProvider ??= new();
      set => toServiceProvider = value;
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

    private Office fromOffice;
    private Office toOffice;
    private OfficeServiceProvider fromOfficeServiceProvider;
    private OfficeServiceProvider toOfficeServiceProvider;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private ServiceProvider fromServiceProvider;
    private ServiceProvider toServiceProvider;
    private Tribunal tribunal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalActionAssignment.
    /// </summary>
    [JsonPropertyName("legalActionAssignment")]
    public Common LegalActionAssignment
    {
      get => legalActionAssignment ??= new();
      set => legalActionAssignment = value;
    }

    /// <summary>
    /// A value of MonitoredActivityChange.
    /// </summary>
    [JsonPropertyName("monitoredActivityChange")]
    public Common MonitoredActivityChange
    {
      get => monitoredActivityChange ??= new();
      set => monitoredActivityChange = value;
    }

    private Common legalActionAssignment;
    private Common monitoredActivityChange;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public WorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Common Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of LgaId.
    /// </summary>
    [JsonPropertyName("lgaId")]
    public WorkArea LgaId
    {
      get => lgaId ??= new();
      set => lgaId = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public WorkArea New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public WorkArea Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of UpdateCount.
    /// </summary>
    [JsonPropertyName("updateCount")]
    public Common UpdateCount
    {
      get => updateCount ??= new();
      set => updateCount = value;
    }

    private WorkArea temp;
    private Common tribunal;
    private DateWorkArea current;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External external;
    private WorkArea lgaId;
    private DateWorkArea max;
    private WorkArea new1;
    private DateWorkArea null1;
    private WorkArea old;
    private DateWorkArea process;
    private Common updateCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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

    /// <summary>
    /// A value of LegalReferralOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("legalReferralOfficeServiceProvider")]
    public OfficeServiceProvider LegalReferralOfficeServiceProvider
    {
      get => legalReferralOfficeServiceProvider ??= new();
      set => legalReferralOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalReferralServiceProvider.
    /// </summary>
    [JsonPropertyName("legalReferralServiceProvider")]
    public ServiceProvider LegalReferralServiceProvider
    {
      get => legalReferralServiceProvider ??= new();
      set => legalReferralServiceProvider = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of NewLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("newLegalActionAssigment")]
    public LegalActionAssigment NewLegalActionAssigment
    {
      get => newLegalActionAssigment ??= new();
      set => newLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of NewLegalActionOffice.
    /// </summary>
    [JsonPropertyName("newLegalActionOffice")]
    public Office NewLegalActionOffice
    {
      get => newLegalActionOffice ??= new();
      set => newLegalActionOffice = value;
    }

    /// <summary>
    /// A value of NewLegalActionOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newLegalActionOfficeServiceProvider")]
    public OfficeServiceProvider NewLegalActionOfficeServiceProvider
    {
      get => newLegalActionOfficeServiceProvider ??= new();
      set => newLegalActionOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NewLegalActionServiceProvider.
    /// </summary>
    [JsonPropertyName("newLegalActionServiceProvider")]
    public ServiceProvider NewLegalActionServiceProvider
    {
      get => newLegalActionServiceProvider ??= new();
      set => newLegalActionServiceProvider = value;
    }

    /// <summary>
    /// A value of NewMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("newMonitoredActivityAssignment")]
    public MonitoredActivityAssignment NewMonitoredActivityAssignment
    {
      get => newMonitoredActivityAssignment ??= new();
      set => newMonitoredActivityAssignment = value;
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

    /// <summary>
    /// A value of OldLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("oldLegalActionAssigment")]
    public LegalActionAssigment OldLegalActionAssigment
    {
      get => oldLegalActionAssigment ??= new();
      set => oldLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of OldLegalActionOffice.
    /// </summary>
    [JsonPropertyName("oldLegalActionOffice")]
    public Office OldLegalActionOffice
    {
      get => oldLegalActionOffice ??= new();
      set => oldLegalActionOffice = value;
    }

    /// <summary>
    /// A value of OldLegalActionOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oldLegalActionOfficeServiceProvider")]
    public OfficeServiceProvider OldLegalActionOfficeServiceProvider
    {
      get => oldLegalActionOfficeServiceProvider ??= new();
      set => oldLegalActionOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OldLegalActionServiceProvider.
    /// </summary>
    [JsonPropertyName("oldLegalActionServiceProvider")]
    public ServiceProvider OldLegalActionServiceProvider
    {
      get => oldLegalActionServiceProvider ??= new();
      set => oldLegalActionServiceProvider = value;
    }

    /// <summary>
    /// A value of OldMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("oldMonitoredActivityAssignment")]
    public MonitoredActivityAssignment OldMonitoredActivityAssignment
    {
      get => oldMonitoredActivityAssignment ??= new();
      set => oldMonitoredActivityAssignment = value;
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

    private Tribunal tribunal;
    private Case1 case1;
    private CaseRole caseRole;
    private Infrastructure infrastructure;
    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private OfficeServiceProvider legalReferralOfficeServiceProvider;
    private ServiceProvider legalReferralServiceProvider;
    private MonitoredActivity monitoredActivity;
    private LegalActionAssigment newLegalActionAssigment;
    private Office newLegalActionOffice;
    private OfficeServiceProvider newLegalActionOfficeServiceProvider;
    private ServiceProvider newLegalActionServiceProvider;
    private MonitoredActivityAssignment newMonitoredActivityAssignment;
    private Office office;
    private LegalActionAssigment oldLegalActionAssigment;
    private Office oldLegalActionOffice;
    private OfficeServiceProvider oldLegalActionOfficeServiceProvider;
    private ServiceProvider oldLegalActionServiceProvider;
    private MonitoredActivityAssignment oldMonitoredActivityAssignment;
    private ServiceProvider serviceProvider;
  }
#endregion
}
