// Program: SP_CAB_CREATE_START_STOP, ID: 371745071, model: 746.
// Short name: SWE01740
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_START_STOP.
/// </summary>
[Serializable]
public partial class SpCabCreateStartStop: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_START_STOP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateStartStop(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateStartStop.
  /// </summary>
  public SpCabCreateStartStop(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadEventDetail())
    {
      if (ReadActivityDetail())
      {
        try
        {
          CreateActivityStartStop();
          export.ActivityStartStop.Assign(entities.New1);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_START_STOP_AE";

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
        ExitState = "SP0000_ACTIVITY_DETAIL_NF";
      }
    }
    else
    {
      ExitState = "ZD_SP0000_EVENT_DETAIL_NF1";
    }
  }

  private void CreateActivityStartStop()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);

    var actionCode = import.ActivityStartStop.ActionCode;
    var effectiveDate = import.ActivityStartStop.EffectiveDate;
    var discontinueDate = import.ActivityStartStop.DiscontinueDate;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var actNo = entities.ActivityDetail.ActNo;
    var acdId = entities.ActivityDetail.SystemGeneratedIdentifier;
    var eveNo = entities.EventDetail.EveNo;
    var evdId = entities.EventDetail.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
    Update("CreateActivityStartStop",
      (db, command) =>
      {
        db.SetString(command, "actionCode", actionCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetInt32(command, "actNo", actNo);
        db.SetInt32(command, "acdId", acdId);
        db.SetInt32(command, "eveNo", eveNo);
        db.SetInt32(command, "evdId", evdId);
      });

    entities.New1.ActionCode = actionCode;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.CreatedBy = createdBy;
    entities.New1.ActNo = actNo;
    entities.New1.AcdId = acdId;
    entities.New1.EveNo = eveNo;
    entities.New1.EvdId = evdId;
    entities.New1.Populated = true;
  }

  private bool ReadActivityDetail()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actNo", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 1);
        entities.ActivityDetail.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 1);
        entities.EventDetail.Populated = true;
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

    private ActivityStartStop activityStartStop;
    private ActivityDetail activityDetail;
    private Activity activity;
    private EventDetail eventDetail;
    private Event1 event1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private ActivityStartStop activityStartStop;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ActivityStartStop New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private ActivityStartStop new1;
    private ActivityDetail activityDetail;
    private Activity activity;
    private EventDetail eventDetail;
    private Event1 event1;
  }
#endregion
}
