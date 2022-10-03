// Program: SP_CAB_DELETE_EVENT, ID: 371778860, model: 746.
// Short name: SWE01680
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_DELETE_EVENT.
/// </para>
/// <para>
///   Deletes entity type EVENT.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDeleteEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteEvent.
  /// </summary>
  public SpCabDeleteEvent(IContext context, Import import, Export export):
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
      if (ReadEventDetail())
      {
        ExitState = "SP0000_EVENT_TIED_TO_EVNT_DET";

        return;
      }

      DeleteEvent();
      ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
    }
    else
    {
      ExitState = "SP0000_EVENT_NF";
    }
  }

  private void DeleteEvent()
  {
    bool exists;

    exists = Read("DeleteEvent#1",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Event1.ControlNumber);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_EVENT_DETAIL\".",
        "50001");
    }

    Update("DeleteEvent#2",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Event1.ControlNumber);
      });
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
        entities.Event1.Name = db.GetString(reader, 1);
        entities.Event1.Type1 = db.GetString(reader, 2);
        entities.Event1.Description = db.GetNullableString(reader, 3);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 4);
        entities.Event1.CreatedBy = db.GetString(reader, 5);
        entities.Event1.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Event1.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Event1.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.Event1.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Event1.ControlNumber);
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private Event1 event1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private Event1 event1;
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
