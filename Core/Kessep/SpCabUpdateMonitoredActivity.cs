// Program: SP_CAB_UPDATE_MONITORED_ACTIVITY, ID: 371930287, model: 746.
// Short name: SWE01800
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_MONITORED_ACTIVITY.
/// </summary>
[Serializable]
public partial class SpCabUpdateMonitoredActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_MONITORED_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateMonitoredActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateMonitoredActivity.
  /// </summary>
  public SpCabUpdateMonitoredActivity(IContext context, Import import,
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
    if (ReadMonitoredActivity())
    {
      UpdateMonitoredActivity();
    }
    else
    {
      ExitState = "SP0000_MONITORED_ACTIVITY_NF";
    }
  }

  private bool ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.MonitoredActivity.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.Name = db.GetString(reader, 1);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 2);
        entities.MonitoredActivity.TypeCode = db.GetNullableString(reader, 3);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 5);
        entities.MonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 6);
        entities.MonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 7);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 8);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 9);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 10);
        entities.MonitoredActivity.CaseUnitClosedInd = db.GetString(reader, 11);
        entities.MonitoredActivity.CreatedBy = db.GetString(reader, 12);
        entities.MonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.MonitoredActivity.Populated = true;
      });
  }

  private void UpdateMonitoredActivity()
  {
    var closureDate = import.MonitoredActivity.ClosureDate;
    var closureReasonCode = import.MonitoredActivity.ClosureReasonCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivity.Populated = false;
    Update("UpdateMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
      });

    entities.MonitoredActivity.ClosureDate = closureDate;
    entities.MonitoredActivity.ClosureReasonCode = closureReasonCode;
    entities.MonitoredActivity.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivity.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MonitoredActivity.Populated = true;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    private MonitoredActivity monitoredActivity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private MonitoredActivity monitoredActivity;
  }
#endregion
}
