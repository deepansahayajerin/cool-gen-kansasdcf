// Program: SP_CAB_DELETE_START_STOP, ID: 371745072, model: 746.
// Short name: SWE01741
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_START_STOP.
/// </summary>
[Serializable]
public partial class SpCabDeleteStartStop: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_START_STOP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteStartStop(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteStartStop.
  /// </summary>
  public SpCabDeleteStartStop(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadActivityStartStop())
    {
      DeleteActivityStartStop();
    }
    else
    {
      ExitState = "SP0000_START_STOP_NF";
    }
  }

  private void DeleteActivityStartStop()
  {
    Update("DeleteActivityStartStop",
      (db, command) =>
      {
        db.SetString(
          command, "actionCode", entities.ActivityStartStop.ActionCode);
        db.SetInt32(command, "actNo", entities.ActivityStartStop.ActNo);
        db.SetInt32(command, "acdId", entities.ActivityStartStop.AcdId);
        db.SetInt32(command, "eveNo", entities.ActivityStartStop.EveNo);
        db.SetInt32(command, "evdId", entities.ActivityStartStop.EvdId);
      });
  }

  private bool ReadActivityStartStop()
  {
    entities.ActivityStartStop.Populated = false;

    return Read("ReadActivityStartStop",
      (db, command) =>
      {
        db.
          SetString(command, "actionCode", import.ActivityStartStop.ActionCode);
          
        db.SetInt32(
          command, "evdId", import.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
        db.SetInt32(
          command, "acdId", import.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actNo", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityStartStop.ActionCode = db.GetString(reader, 0);
        entities.ActivityStartStop.EffectiveDate = db.GetDate(reader, 1);
        entities.ActivityStartStop.DiscontinueDate = db.GetDate(reader, 2);
        entities.ActivityStartStop.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.ActivityStartStop.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ActivityStartStop.ActNo = db.GetInt32(reader, 5);
        entities.ActivityStartStop.AcdId = db.GetInt32(reader, 6);
        entities.ActivityStartStop.EveNo = db.GetInt32(reader, 7);
        entities.ActivityStartStop.EvdId = db.GetInt32(reader, 8);
        entities.ActivityStartStop.Populated = true;
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
    /// A value of ActivityStartStop.
    /// </summary>
    [JsonPropertyName("activityStartStop")]
    public ActivityStartStop ActivityStartStop
    {
      get => activityStartStop ??= new();
      set => activityStartStop = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityStartStop activityStartStop;
    private EventDetail eventDetail;
    private Event1 event1;
    private ActivityDetail activityDetail;
    private Activity activity;
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
    /// A value of ActivityStartStop.
    /// </summary>
    [JsonPropertyName("activityStartStop")]
    public ActivityStartStop ActivityStartStop
    {
      get => activityStartStop ??= new();
      set => activityStartStop = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityStartStop activityStartStop;
    private EventDetail eventDetail;
    private Event1 event1;
    private ActivityDetail activityDetail;
    private Activity activity;
  }
#endregion
}
