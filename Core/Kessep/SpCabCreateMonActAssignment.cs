// Program: SP_CAB_CREATE_MON_ACT_ASSIGNMENT, ID: 371785883, model: 746.
// Short name: SWE01796
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_MON_ACT_ASSIGNMENT.
/// </summary>
[Serializable]
public partial class SpCabCreateMonActAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_MON_ACT_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateMonActAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateMonActAssignment.
  /// </summary>
  public SpCabCreateMonActAssignment(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/24/96 Rick Delgado               Initial Development
    // 10/27/97  Siraj Konkader            Performance tuning. Replaced 
    // persistent views, moved edits to PRAD.
    // 11/12/97  Jack Rookard              Remove functionality which sets 
    // Monitored Activity Assignment System Generated ID using Control Table.
    // This attribute is not used in identifying an occurrence of Monitored
    // Activity Assignment, and should eventually be removed from the entity, as
    // it is useless.
    // ------------------------------------------------------------
    if (ReadMonitoredActivity())
    {
      if (ReadOfficeServiceProvider())
      {
        try
        {
          CreateMonitoredActivityAssignment();
          export.MonitoredActivityAssignment.Assign(entities.New1);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_MONITORED_ACTVTY_ASGN_AE";

              break;
            case ErrorCode.PermittedValueViolation:
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
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";
      }
    }
    else
    {
      ExitState = "SP0000_MONITORED_ACTIVITY_NF";
    }
  }

  private void CreateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingOfficeServiceProvider.Populated);

    var reasonCode = import.MonitoredActivityAssignment.ReasonCode;
    var responsibilityCode =
      import.MonitoredActivityAssignment.ResponsibilityCode;
    var effectiveDate = import.MonitoredActivityAssignment.EffectiveDate;
    var overrideInd = import.MonitoredActivityAssignment.OverrideInd;
    var discontinueDate = import.MonitoredActivityAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.MonitoredActivityAssignment.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp =
      import.MonitoredActivityAssignment.LastUpdatedTimestamp;
    var spdId = entities.ExistingOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.ExistingOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.ExistingOfficeServiceProvider.RoleCode;
    var ospDate = entities.ExistingOfficeServiceProvider.EffectiveDate;
    var macId = entities.ExistingMonitoredActivity.SystemGeneratedIdentifier;

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
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
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
    entities.New1.LastUpdatedBy = lastUpdatedBy;
    entities.New1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.New1.SpdId = spdId;
    entities.New1.OffId = offId;
    entities.New1.OspCode = ospCode;
    entities.New1.OspDate = ospDate;
    entities.New1.MacId = macId;
    entities.New1.Populated = true;
  }

  private bool ReadMonitoredActivity()
  {
    entities.ExistingMonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.MonitoredActivity.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingMonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingMonitoredActivity.Name = db.GetString(reader, 1);
        entities.ExistingMonitoredActivity.TypeCode =
          db.GetNullableString(reader, 2);
        entities.ExistingMonitoredActivity.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private MonitoredActivity monitoredActivity;
    private OfficeServiceProvider officeServiceProvider;
    private MonitoredActivityAssignment monitoredActivityAssignment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private MonitoredActivityAssignment monitoredActivityAssignment;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ExistingMonitoredActivity.
    /// </summary>
    [JsonPropertyName("existingMonitoredActivity")]
    public MonitoredActivity ExistingMonitoredActivity
    {
      get => existingMonitoredActivity ??= new();
      set => existingMonitoredActivity = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
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

    private ServiceProvider serviceProvider;
    private Office office;
    private MonitoredActivity existingMonitoredActivity;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private MonitoredActivityAssignment new1;
  }
#endregion
}
