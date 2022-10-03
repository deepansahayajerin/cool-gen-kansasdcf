// Program: SP_CAB_UPDATE_EVENT_DETAIL, ID: 371778242, model: 746.
// Short name: SWE01751
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_EVENT_DETAIL.
/// </summary>
[Serializable]
public partial class SpCabUpdateEventDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_EVENT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateEventDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateEventDetail.
  /// </summary>
  public SpCabUpdateEventDetail(IContext context, Import import, Export export):
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
      try
      {
        UpdateEventDetail();
        export.EventDetail.Assign(entities.EventDetail);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_EVENT_DETAIL_AE";

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
      ExitState = "SP0000_EVENT_DETAIL_NF";
    }
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
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.ProcedureName = db.GetNullableString(reader, 6);
        entities.EventDetail.LifecycleImpactCode = db.GetString(reader, 7);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 8);
        entities.EventDetail.DateMonitorDays = db.GetNullableInt32(reader, 9);
        entities.EventDetail.NextEventId = db.GetNullableInt32(reader, 10);
        entities.EventDetail.NextEventDetailId =
          db.GetNullableString(reader, 11);
        entities.EventDetail.EffectiveDate = db.GetDate(reader, 12);
        entities.EventDetail.DiscontinueDate = db.GetDate(reader, 13);
        entities.EventDetail.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.EventDetail.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 15);
        entities.EventDetail.EveNo = db.GetInt32(reader, 16);
        entities.EventDetail.Function = db.GetNullableString(reader, 17);
        entities.EventDetail.ExceptionRoutine =
          db.GetNullableString(reader, 18);
        entities.EventDetail.Populated = true;
      });
  }

  private void UpdateEventDetail()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);

    var detailName = import.EventDetail.DetailName;
    var description = import.EventDetail.Description ?? "";
    var procedureName = import.EventDetail.ProcedureName ?? "";
    var lifecycleImpactCode = import.EventDetail.LifecycleImpactCode;
    var logToDiaryInd = import.EventDetail.LogToDiaryInd;
    var dateMonitorDays =
      import.EventDetail.DateMonitorDays.GetValueOrDefault();
    var nextEventId = import.EventDetail.NextEventId.GetValueOrDefault();
    var nextEventDetailId = import.EventDetail.NextEventDetailId ?? "";
    var effectiveDate = import.EventDetail.EffectiveDate;
    var discontinueDate = import.EventDetail.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedDtstamp = Now();
    var function = import.EventDetail.Function ?? "";
    var exceptionRoutine = import.EventDetail.ExceptionRoutine ?? "";

    entities.EventDetail.Populated = false;
    Update("UpdateEventDetail",
      (db, command) =>
      {
        db.SetString(command, "detailName", detailName);
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "procedureName", procedureName);
        db.SetString(command, "lifecyclImpactCd", lifecycleImpactCode);
        db.SetString(command, "logToDiaryInd", logToDiaryInd);
        db.SetNullableInt32(command, "dateMonitorDays", dateMonitorDays);
        db.SetNullableInt32(command, "nextEventId", nextEventId);
        db.SetNullableString(command, "nextEventDetail", nextEventDetailId);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdtdDtstamp", lastUpdatedDtstamp);
        db.SetNullableString(command, "function", function);
        db.SetNullableString(command, "exceptionRoutine", exceptionRoutine);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
      });

    entities.EventDetail.DetailName = detailName;
    entities.EventDetail.Description = description;
    entities.EventDetail.ProcedureName = procedureName;
    entities.EventDetail.LifecycleImpactCode = lifecycleImpactCode;
    entities.EventDetail.LogToDiaryInd = logToDiaryInd;
    entities.EventDetail.DateMonitorDays = dateMonitorDays;
    entities.EventDetail.NextEventId = nextEventId;
    entities.EventDetail.NextEventDetailId = nextEventDetailId;
    entities.EventDetail.EffectiveDate = effectiveDate;
    entities.EventDetail.DiscontinueDate = discontinueDate;
    entities.EventDetail.LastUpdatedBy = lastUpdatedBy;
    entities.EventDetail.LastUpdatedDtstamp = lastUpdatedDtstamp;
    entities.EventDetail.Function = function;
    entities.EventDetail.ExceptionRoutine = exceptionRoutine;
    entities.EventDetail.Populated = true;
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
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    private EventDetail eventDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private EventDetail eventDetail;
    private Event1 event1;
  }
#endregion
}
