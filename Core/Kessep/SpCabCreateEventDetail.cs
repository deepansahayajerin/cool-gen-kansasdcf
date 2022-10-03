// Program: SP_CAB_CREATE_EVENT_DETAIL, ID: 371778240, model: 746.
// Short name: SWE01749
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_EVENT_DETAIL.
/// </summary>
[Serializable]
public partial class SpCabCreateEventDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_EVENT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateEventDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateEventDetail.
  /// </summary>
  public SpCabCreateEventDetail(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadEvent())
    {
      try
      {
        CreateEventDetail();
        export.EventDetail.Assign(entities.New1);
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
      ExitState = "SP0000_EVENT_NF";
    }
  }

  private void CreateEventDetail()
  {
    var systemGeneratedIdentifier =
      import.EventDetail.SystemGeneratedIdentifier;
    var detailName = import.EventDetail.DetailName;
    var description = import.EventDetail.Description ?? "";
    var initiatingStateCode = import.EventDetail.InitiatingStateCode;
    var csenetInOutCode = import.EventDetail.CsenetInOutCode;
    var reasonCode = import.EventDetail.ReasonCode;
    var procedureName = import.EventDetail.ProcedureName ?? "";
    var lifecycleImpactCode = import.EventDetail.LifecycleImpactCode;
    var logToDiaryInd = import.EventDetail.LogToDiaryInd;
    var dateMonitorDays =
      import.EventDetail.DateMonitorDays.GetValueOrDefault();
    var nextEventId = import.EventDetail.NextEventId.GetValueOrDefault();
    var nextEventDetailId = import.EventDetail.NextEventDetailId ?? "";
    var effectiveDate = import.EventDetail.EffectiveDate;
    var discontinueDate = import.EventDetail.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var eveNo = entities.Event1.ControlNumber;
    var function = import.EventDetail.Function ?? "";
    var exceptionRoutine = import.EventDetail.ExceptionRoutine ?? "";

    entities.New1.Populated = false;
    Update("CreateEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "detailName", detailName);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "initiatingStCd", initiatingStateCode);
        db.SetString(command, "csenetInOutCode", csenetInOutCode);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetNullableString(command, "procedureName", procedureName);
        db.SetString(command, "lifecyclImpactCd", lifecycleImpactCode);
        db.SetString(command, "logToDiaryInd", logToDiaryInd);
        db.SetNullableInt32(command, "dateMonitorDays", dateMonitorDays);
        db.SetNullableInt32(command, "nextEventId", nextEventId);
        db.SetNullableString(command, "nextEventDetail", nextEventDetailId);
        db.SetString(command, "nextInitSt", "");
        db.SetNullableString(command, "nextCsenetIo", "");
        db.SetNullableString(command, "nextReason", "");
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdtdDtstamp", default(DateTime));
        db.SetInt32(command, "eveNo", eveNo);
        db.SetNullableString(command, "function", function);
        db.SetNullableString(command, "exceptionRoutine", exceptionRoutine);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.DetailName = detailName;
    entities.New1.Description = description;
    entities.New1.InitiatingStateCode = initiatingStateCode;
    entities.New1.CsenetInOutCode = csenetInOutCode;
    entities.New1.ReasonCode = reasonCode;
    entities.New1.ProcedureName = procedureName;
    entities.New1.LifecycleImpactCode = lifecycleImpactCode;
    entities.New1.LogToDiaryInd = logToDiaryInd;
    entities.New1.DateMonitorDays = dateMonitorDays;
    entities.New1.NextEventId = nextEventId;
    entities.New1.NextEventDetailId = nextEventDetailId;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.EveNo = eveNo;
    entities.New1.Function = function;
    entities.New1.ExceptionRoutine = exceptionRoutine;
    entities.New1.Populated = true;
  }

  private bool ReadEvent()
  {
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", import.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.Populated = true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public EventDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private EventDetail new1;
    private Event1 event1;
  }
#endregion
}
