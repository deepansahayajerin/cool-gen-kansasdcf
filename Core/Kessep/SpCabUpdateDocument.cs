// Program: SP_CAB_UPDATE_DOCUMENT, ID: 372107397, model: 746.
// Short name: SWE01688
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_DOCUMENT.
/// </para>
/// <para>
///   Performs update of table DOCUMENT.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateDocument.
  /// </summary>
  public SpCabUpdateDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Userid.CreatedBy = global.UserId;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    if (!ReadDocument())
    {
      ExitState = "SP0000_DOCUMENT_NF";

      return;
    }

    if (import.Event1.ControlNumber == import.CheckEvent.ControlNumber && import
      .EventDetail.SystemGeneratedIdentifier == import
      .CheckEventDetail.SystemGeneratedIdentifier)
    {
      local.Transfer.Flag = "N";
    }
    else
    {
      local.Transfer.Flag = "Y";
    }

    try
    {
      UpdateDocument();
      export.HiddenCheck.Assign(entities.Document);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_EVENT_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_PERMITTED_VAL_VIOLATN_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (AsChar(local.Transfer.Flag) == 'Y')
    {
      // mjr
      // ----------------------------------------------
      // 09/16/1998
      // If we delete this document all the associated document_fields
      // will also be deleted, so we need to preserve those records.
      // If any outgoing_docs exist this delete will fail, so we
      // need to preserve those records, as well.
      // This is a lot of work that can be avoided by making the
      // relationship between event_detail and document transferrable.
      // In the meantime, this option is not available.
      // ------------------------------------------------------------
      ExitState = "FN0000_CANT_UPDATE_KEYS_CHANGED";

      if (!ReadEventEventDetail1())
      {
        // mjr---> This is not possible, as each document must have an 
        // event_detail
        ExitState = "SP0000_EVENT_DETAIL_NF";

        return;
      }

      if (!ReadEventEventDetail2())
      {
        ExitState = "SP0000_EVENT_DETAIL_NF";
      }

      // mjr----> Add TRANSFER statement, when available
    }
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
        db.SetDate(
          command, "effectiveDate",
          import.Document.EffectiveDate.GetValueOrDefault());
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

  private bool ReadEventEventDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Document.Populated);
    entities.ExistingEvent.Populated = false;
    entities.ExistingEventDetail.Populated = false;

    return Read("ReadEventEventDetail1",
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
        entities.ExistingEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.ExistingEventDetail.EveNo = db.GetInt32(reader, 0);
        entities.ExistingEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingEventDetail.DetailName = db.GetString(reader, 2);
        entities.ExistingEvent.Populated = true;
        entities.ExistingEventDetail.Populated = true;
      });
  }

  private bool ReadEventEventDetail2()
  {
    entities.NewEvent.Populated = false;
    entities.NewEventDetail.Populated = false;

    return Read("ReadEventEventDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          import.EventDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.NewEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.NewEventDetail.EveNo = db.GetInt32(reader, 0);
        entities.NewEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.NewEventDetail.DetailName = db.GetString(reader, 2);
        entities.NewEvent.Populated = true;
        entities.NewEventDetail.Populated = true;
      });
  }

  private void UpdateDocument()
  {
    var type1 = import.Document.Type1;
    var lastUpdatedBy = local.Userid.CreatedBy;
    var lastUpdatdTstamp = local.Current.Timestamp;
    var description = import.Document.Description ?? "";
    var businessObject = import.Document.BusinessObject;
    var requiredResponseDays = import.Document.RequiredResponseDays;
    var detailedDescription = import.Document.DetailedDescription;
    var expirationDate = import.Document.ExpirationDate;
    var printPreviewSwitch = import.Document.PrintPreviewSwitch;
    var versionNumber = import.Document.VersionNumber;

    entities.Document.Populated = false;
    Update("UpdateDocument",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "businessObject", businessObject);
        db.SetInt32(command, "rquredRspnseDays", requiredResponseDays);
        db.SetString(command, "detailedDesc", detailedDescription);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "printPreviewSw", printPreviewSwitch);
        db.SetString(command, "versionNbr", versionNumber);
        db.SetString(command, "name", entities.Document.Name);
        db.SetDate(
          command, "effectiveDate",
          entities.Document.EffectiveDate.GetValueOrDefault());
      });

    entities.Document.Type1 = type1;
    entities.Document.LastUpdatedBy = lastUpdatedBy;
    entities.Document.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.Document.Description = description;
    entities.Document.BusinessObject = businessObject;
    entities.Document.RequiredResponseDays = requiredResponseDays;
    entities.Document.DetailedDescription = detailedDescription;
    entities.Document.ExpirationDate = expirationDate;
    entities.Document.PrintPreviewSwitch = printPreviewSwitch;
    entities.Document.VersionNumber = versionNumber;
    entities.Document.Populated = true;
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
    /// A value of CheckEventDetail.
    /// </summary>
    [JsonPropertyName("checkEventDetail")]
    public EventDetail CheckEventDetail
    {
      get => checkEventDetail ??= new();
      set => checkEventDetail = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    private EventDetail checkEventDetail;
    private Event1 checkEvent;
    private EventDetail eventDetail;
    private Event1 event1;
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
    /// A value of CheckEvent.
    /// </summary>
    [JsonPropertyName("checkEvent")]
    public Event1 CheckEvent
    {
      get => checkEvent ??= new();
      set => checkEvent = value;
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
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Document HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
    }

    private EventDetail checkEventDetail;
    private Event1 checkEvent;
    private EventDetail eventDetail;
    private Event1 event1;
    private Document hiddenCheck;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Transfer.
    /// </summary>
    [JsonPropertyName("transfer")]
    public Common Transfer
    {
      get => transfer ??= new();
      set => transfer = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Userid.
    /// </summary>
    [JsonPropertyName("userid")]
    public Document Userid
    {
      get => userid ??= new();
      set => userid = value;
    }

    private Common transfer;
    private DateWorkArea current;
    private Document userid;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
    }

    /// <summary>
    /// A value of ExistingEventDetail.
    /// </summary>
    [JsonPropertyName("existingEventDetail")]
    public EventDetail ExistingEventDetail
    {
      get => existingEventDetail ??= new();
      set => existingEventDetail = value;
    }

    /// <summary>
    /// A value of NewEvent.
    /// </summary>
    [JsonPropertyName("newEvent")]
    public Event1 NewEvent
    {
      get => newEvent ??= new();
      set => newEvent = value;
    }

    /// <summary>
    /// A value of NewEventDetail.
    /// </summary>
    [JsonPropertyName("newEventDetail")]
    public EventDetail NewEventDetail
    {
      get => newEventDetail ??= new();
      set => newEventDetail = value;
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

    private Event1 existingEvent;
    private EventDetail existingEventDetail;
    private Event1 newEvent;
    private EventDetail newEventDetail;
    private Document document;
  }
#endregion
}
