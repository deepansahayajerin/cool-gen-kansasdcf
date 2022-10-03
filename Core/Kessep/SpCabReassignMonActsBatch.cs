// Program: SP_CAB_REASSIGN_MON_ACTS_BATCH, ID: 372572053, model: 746.
// Short name: SWE00023
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_REASSIGN_MON_ACTS_BATCH.
/// </summary>
[Serializable]
public partial class SpCabReassignMonActsBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_REASSIGN_MON_ACTS_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReassignMonActsBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReassignMonActsBatch.
  /// </summary>
  public SpCabReassignMonActsBatch(IContext context, Import import,
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
    // *******************************************************************
    // *                 M A I N T E N A N C E    L O G                  *
    // *******************************************************************
    // * Date   Developer        PR#     Decription
    // -------------------------------------------------------------------
    // 02/23/00 SWSRCHF       H00089087  Allow all MONA's stemming from an
    //                                   
    // event that has a business object
    //                                   
    // of LEA to be transferred
    // 08/29/00 SWSRCHF       00100345   Check for discontinue date
    //                                   
    // equal MAX date (2099-12-31)
    // 10/13/00 SWDPARM       H95478     added LRF to the Case of infrastructure
    // business
    //                                   
    // object code
    // -------------------------------------------------------------------
    if (!import.Case1.Populated)
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!import.CurrentServiceProvider.Populated)
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (!import.New1.Populated)
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    export.ChkpntNumbCreates.Count = import.Create.Count;
    export.ChkpntNumbUpdates.Count = import.Update.Count;

    // *** Problem report 00100345
    // *** 08/29/00 SWSRCHF
    // *** check for Discontinue_Date = '2099-12-31'
    foreach(var item in ReadMonitoredActivityAssignmentMonitoredActivity())
    {
      if (AsChar(entities.MonitoredActivityAssignment.OverrideInd) == 'Y')
      {
        continue;
      }

      if (!IsEmpty(entities.MonitoredActivity.ClosureReasonCode) || !
        Lt(import.CurrentDateWorkArea.Date,
        entities.MonitoredActivity.ClosureDate) || Equal
        (entities.MonitoredActivity.TypeCode, "MAN"))
      {
        continue;
      }

      switch(TrimEnd(entities.Infrastructure.BusinessObjectCd))
      {
        case "LRF":
          break;
        case "CAS":
          break;
        case "CAU":
          break;
        case "CSP":
          break;
        case "PHI":
          break;
        case "INC":
          break;
        case "CPA":
          break;
        case "ICS":
          break;
        case "BKR":
          break;
        case "PPR":
          break;
        case "CPR":
          break;
        case "CSW":
          break;
        case "GNT":
          break;
        case "PGT":
          break;
        case "CON":
          break;
        case "MIL":
          break;
        case "PAR":
          break;
        case "PIH":
          break;
        case "HIN":
          break;
        case "FPL":
          break;
        case "LEA":
          // *** Problem Report H00089087
          // *** 02/23/00 SWSRCHF
          // *** Start
          // *** End
          // *** 02/23/00 SWSRCHF
          // *** Problem Report H00089087
          break;
        default:
          continue;
      }

      try
      {
        UpdateMonitoredActivityAssignment();
        ++export.ChkpntNumbUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_MONITORED_ACT_ASSGN_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_MONITORED_ACT_ASSGN_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // The following create should be replaced with the appropriate cab.
      try
      {
        CreateMonitoredActivityAssignment();
        ++export.ChkpntNumbCreates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_MONITORED_ACT_ASSGN_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_MONITORED_ACT_ASSGN_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(import.New1.Populated);

    var reasonCode = entities.MonitoredActivityAssignment.ReasonCode;
    var responsibilityCode =
      entities.MonitoredActivityAssignment.ResponsibilityCode;
    var effectiveDate = import.CurrentDatePlus1.Date;
    var overrideInd = "N";
    var discontinueDate = import.Max.Date;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = Now();
    var spdId = import.New1.SpdGeneratedId;
    var offId = import.New1.OffGeneratedId;
    var ospCode = import.New1.RoleCode;
    var ospDate = import.New1.EffectiveDate;
    var macId = entities.MonitoredActivity.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
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
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "macId", macId);
      });

    entities.New1.SystemGeneratedIdentifier = 0;
    entities.New1.ReasonCode = reasonCode;
    entities.New1.ResponsibilityCode = responsibilityCode;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.OverrideInd = overrideInd;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTimestamp = null;
    entities.New1.SpdId = spdId;
    entities.New1.OffId = offId;
    entities.New1.OspCode = ospCode;
    entities.New1.OspDate = ospDate;
    entities.New1.MacId = macId;
    entities.New1.Populated = true;
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignmentMonitoredActivity()
  {
    entities.MonitoredActivityAssignment.Populated = false;
    entities.MonitoredActivity.Populated = false;
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignmentMonitoredActivity",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdId", import.CurrentServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.CurrentDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.MonitoredActivityAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 1);
        entities.MonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 2);
        entities.MonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 3);
        entities.MonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 4);
        entities.MonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 6);
        entities.MonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.MonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.MonitoredActivityAssignment.SpdId = db.GetInt32(reader, 10);
        entities.MonitoredActivityAssignment.OffId = db.GetInt32(reader, 11);
        entities.MonitoredActivityAssignment.OspCode = db.GetString(reader, 12);
        entities.MonitoredActivityAssignment.OspDate = db.GetDate(reader, 13);
        entities.MonitoredActivityAssignment.MacId = db.GetInt32(reader, 14);
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.MonitoredActivity.Name = db.GetString(reader, 15);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 16);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 17);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 18);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 19);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 20);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 21);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 22);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 23);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 24);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 25);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 26);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 27);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 28);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 29);
        entities.MonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 30);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 30);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 31);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 32);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 33);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 34);
        entities.MonitoredActivityAssignment.Populated = true;
        entities.MonitoredActivity.Populated = true;
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.MonitoredActivityAssignment.Populated);

    var discontinueDate = import.CurrentDateWorkArea.Date;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.MonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.
          SetInt32(command, "spdId", entities.MonitoredActivityAssignment.SpdId);
          
        db.
          SetInt32(command, "offId", entities.MonitoredActivityAssignment.OffId);
          
        db.SetString(
          command, "ospCode", entities.MonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.MonitoredActivityAssignment.OspDate.GetValueOrDefault());
        db.
          SetInt32(command, "macId", entities.MonitoredActivityAssignment.MacId);
          
      });

    entities.MonitoredActivityAssignment.DiscontinueDate = discontinueDate;
    entities.MonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.MonitoredActivityAssignment.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public Common Create
    {
      get => create ??= new();
      set => create = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CurrentServiceProvider.
    /// </summary>
    [JsonPropertyName("currentServiceProvider")]
    public ServiceProvider CurrentServiceProvider
    {
      get => currentServiceProvider ??= new();
      set => currentServiceProvider = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public OfficeServiceProvider New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentDatePlus1.
    /// </summary>
    [JsonPropertyName("currentDatePlus1")]
    public DateWorkArea CurrentDatePlus1
    {
      get => currentDatePlus1 ??= new();
      set => currentDatePlus1 = value;
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

    private Common update;
    private Common create;
    private ProgramProcessingInfo programProcessingInfo;
    private Case1 case1;
    private ServiceProvider currentServiceProvider;
    private OfficeServiceProvider new1;
    private DateWorkArea currentDateWorkArea;
    private DateWorkArea currentDatePlus1;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChkpntNumbUpdates.
    /// </summary>
    [JsonPropertyName("chkpntNumbUpdates")]
    public Common ChkpntNumbUpdates
    {
      get => chkpntNumbUpdates ??= new();
      set => chkpntNumbUpdates = value;
    }

    /// <summary>
    /// A value of ChkpntNumbCreates.
    /// </summary>
    [JsonPropertyName("chkpntNumbCreates")]
    public Common ChkpntNumbCreates
    {
      get => chkpntNumbCreates ??= new();
      set => chkpntNumbCreates = value;
    }

    private Common chkpntNumbUpdates;
    private Common chkpntNumbCreates;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MonitoredActivityAssignment New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private MonitoredActivityAssignment monitoredActivityAssignment;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private MonitoredActivityAssignment new1;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
