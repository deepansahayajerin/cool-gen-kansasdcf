// Program: SP_CAB_READ_DOCUMENT, ID: 372107395, model: 746.
// Short name: SWE01687
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_READ_DOCUMENT.
/// </summary>
[Serializable]
public partial class SpCabReadDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_READ_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReadDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReadDocument.
  /// </summary>
  public SpCabReadDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Max.Date = new DateTime(2099, 12, 31);

    if (ReadDocument())
    {
      export.Document.Assign(entities.Document);
      export.HiddenCheck.Assign(entities.Document);
    }

    if (!entities.Document.Populated)
    {
      export.Document.Name = import.Document.Name;
      ExitState = "DOCUMENT_NF";

      return;
    }

    if (ReadEventEventDetail())
    {
      export.Event1.ControlNumber = entities.Event1.ControlNumber;
      export.CheckEvent.ControlNumber = entities.Event1.ControlNumber;
      MoveEventDetail(entities.EventDetail, export.EventDetail);
      MoveEventDetail(entities.EventDetail, export.CheckEventDetail);
    }
    else
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";
    }
  }

  private static void MoveEventDetail(EventDetail source, EventDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DetailName = source.DetailName;
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
        db.
          SetDate(command, "expirationDate", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.CreatedBy = db.GetString(reader, 2);
        entities.Document.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Document.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Document.LastUpdatdTstamp = db.GetNullableDateTime(reader, 5);
        entities.Document.Description = db.GetNullableString(reader, 6);
        entities.Document.BusinessObject = db.GetString(reader, 7);
        entities.Document.RequiredResponseDays = db.GetInt32(reader, 8);
        entities.Document.EveNo = db.GetNullableInt32(reader, 9);
        entities.Document.EvdId = db.GetNullableInt32(reader, 10);
        entities.Document.DetailedDescription = db.GetString(reader, 11);
        entities.Document.EffectiveDate = db.GetDate(reader, 12);
        entities.Document.ExpirationDate = db.GetDate(reader, 13);
        entities.Document.PrintPreviewSwitch = db.GetString(reader, 14);
        entities.Document.VersionNumber = db.GetString(reader, 15);
        entities.Document.Populated = true;
      });
  }

  private bool ReadEventEventDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Document.Populated);
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Document.EvdId.GetValueOrDefault());
        db.SetInt32(
          command, "eveNo", entities.Document.EveNo.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.Event1.Populated = true;
        entities.EventDetail.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    private Document document;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CheckEventDetail.
    /// </summary>
    [JsonPropertyName("checkEventDetail")]
    public EventDetail CheckEventDetail
    {
      get => checkEventDetail ??= new();
      set => checkEventDetail = value;
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
    /// A value of CheckEvent.
    /// </summary>
    [JsonPropertyName("checkEvent")]
    public Event1 CheckEvent
    {
      get => checkEvent ??= new();
      set => checkEvent = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Document HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
    }

    private EventDetail checkEventDetail;
    private EventDetail eventDetail;
    private Event1 checkEvent;
    private Event1 event1;
    private Document document;
    private Document hiddenCheck;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    private Event1 event1;
    private EventDetail eventDetail;
    private Document document;
  }
#endregion
}
