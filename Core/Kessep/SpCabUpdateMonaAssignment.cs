// Program: SP_CAB_UPDATE_MONA_ASSIGNMENT, ID: 372318288, model: 746.
// Short name: SWE01903
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_MONA_ASSIGNMENT.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This acblk updates the Monitored Activity Assignment entity
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateMonaAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_MONA_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateMonaAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateMonaAssignment.
  /// </summary>
  public SpCabUpdateMonaAssignment(IContext context, Import import,
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
    if (ReadMonitoredActivityAssignment())
    {
      try
      {
        UpdateMonitoredActivityAssignment();
        MoveMonitoredActivityAssignment(entities.MonitoredActivityAssignment,
          export.MonitoredActivityAssignment);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_NU";

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
      ExitState = "MONITORED_ACTIVITY_NF";
    }
  }

  private static void MoveMonitoredActivityAssignment(
    MonitoredActivityAssignment source, MonitoredActivityAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.EffectiveDate = source.EffectiveDate;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private bool ReadMonitoredActivityAssignment()
  {
    entities.MonitoredActivityAssignment.Populated = false;

    return Read("ReadMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId", import.MonitoredActivity.SystemGeneratedIdentifier);
          
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
        db.SetDateTime(
          command, "createdTimestamp",
          import.MonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
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
        entities.MonitoredActivityAssignment.Populated = true;
      });
  }

  private void UpdateMonitoredActivityAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.MonitoredActivityAssignment.Populated);

    var reasonCode = import.MonitoredActivityAssignment.ReasonCode;
    var overrideInd = import.MonitoredActivityAssignment.OverrideInd;
    var discontinueDate = import.MonitoredActivityAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
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

    entities.MonitoredActivityAssignment.ReasonCode = reasonCode;
    entities.MonitoredActivityAssignment.OverrideInd = overrideInd;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private MonitoredActivity monitoredActivity;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private Case1 case1;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
