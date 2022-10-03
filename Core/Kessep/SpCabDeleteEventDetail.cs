// Program: SP_CAB_DELETE_EVENT_DETAIL, ID: 371778241, model: 746.
// Short name: SWE01750
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_EVENT_DETAIL.
/// </summary>
[Serializable]
public partial class SpCabDeleteEventDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_EVENT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteEventDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteEventDetail.
  /// </summary>
  public SpCabDeleteEventDetail(IContext context, Import import, Export export):
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
      DeleteEventDetail();
    }
    else
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";
    }
  }

  private void DeleteEventDetail()
  {
    bool exists;

    exists = Read("DeleteEventDetail#1",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ACT_START_STOP\".",
        "50001");
    }

    exists = Read("DeleteEventDetail#2",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_DOCUMENT\".", "50001");
    }

    exists = Read("DeleteEventDetail#3",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_LIFECYCLE_TRAN\".",
        "50001");
    }

    exists = Read("DeleteEventDetail#4",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_LIFECYCLE_TRAN\".",
        "50001");
    }

    Update("DeleteEventDetail#5",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
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
