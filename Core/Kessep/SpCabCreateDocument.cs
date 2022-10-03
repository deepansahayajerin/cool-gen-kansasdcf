// Program: SP_CAB_CREATE_DOCUMENT, ID: 372107398, model: 746.
// Short name: SWE01683
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_CREATE_DOCUMENT.
/// </para>
/// <para>
///   Creates table DOCUMENT
/// </para>
/// </summary>
[Serializable]
public partial class SpCabCreateDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateDocument.
  /// </summary>
  public SpCabCreateDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Document.CreatedBy = global.UserId;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // mjr
    // ----------------------------------------
    // 09/16/1998
    // Only one document can be effective for any given period
    // -----------------------------------------------------
    ReadDocument();

    if (Lt(local.Current.Date, entities.Previous.ExpirationDate))
    {
      ExitState = "SP0000_EXPIRE_DOCUMENT_BEFOR_ADD";

      return;
    }
    else if (Equal(entities.Previous.ExpirationDate, local.Current.Date))
    {
      local.Current.Date = AddDays(local.Current.Date, 1);
    }

    if (!ReadEventEventDetail())
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";

      return;
    }

    try
    {
      CreateDocument();
      export.HiddenCheck.Assign(entities.Document);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_DOCUMENT_AE";

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

  private void CreateDocument()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);

    var name = import.Document.Name;
    var type1 = import.Document.Type1;
    var createdBy = local.Document.CreatedBy;
    var createdTimestamp = local.Current.Timestamp;
    var description = import.Document.Description ?? "";
    var businessObject = import.Document.BusinessObject;
    var requiredResponseDays = import.Document.RequiredResponseDays;
    var eveNo = entities.EventDetail.EveNo;
    var evdId = entities.EventDetail.SystemGeneratedIdentifier;
    var detailedDescription = import.Document.DetailedDescription;
    var effectiveDate = local.Current.Date;
    var expirationDate = import.Document.ExpirationDate;
    var printPreviewSwitch = import.Document.PrintPreviewSwitch;
    var versionNumber = import.Document.VersionNumber;

    entities.Document.Populated = false;
    Update("CreateDocument",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "businessObject", businessObject);
        db.SetInt32(command, "rquredRspnseDays", requiredResponseDays);
        db.SetNullableInt32(command, "eveNo", eveNo);
        db.SetNullableInt32(command, "evdId", evdId);
        db.SetString(command, "detailedDesc", detailedDescription);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetString(command, "printPreviewSw", printPreviewSwitch);
        db.SetString(command, "versionNbr", versionNumber);
      });

    entities.Document.Name = name;
    entities.Document.Type1 = type1;
    entities.Document.CreatedBy = createdBy;
    entities.Document.CreatedTimestamp = createdTimestamp;
    entities.Document.LastUpdatedBy = "";
    entities.Document.LastUpdatdTstamp = null;
    entities.Document.Description = description;
    entities.Document.BusinessObject = businessObject;
    entities.Document.RequiredResponseDays = requiredResponseDays;
    entities.Document.EveNo = eveNo;
    entities.Document.EvdId = evdId;
    entities.Document.DetailedDescription = detailedDescription;
    entities.Document.EffectiveDate = effectiveDate;
    entities.Document.ExpirationDate = expirationDate;
    entities.Document.PrintPreviewSwitch = printPreviewSwitch;
    entities.Document.VersionNumber = versionNumber;
    entities.Document.Populated = true;
  }

  private bool ReadDocument()
  {
    entities.Previous.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
      },
      (db, reader) =>
      {
        entities.Previous.Name = db.GetString(reader, 0);
        entities.Previous.EffectiveDate = db.GetDate(reader, 1);
        entities.Previous.ExpirationDate = db.GetDate(reader, 2);
        entities.Previous.Populated = true;
      });
  }

  private bool ReadEventEventDetail()
  {
    entities.EventDetail.Populated = false;
    entities.Event1.Populated = false;

    return Read("ReadEventEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          import.EventDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.DetailName = db.GetString(reader, 2);
        entities.EventDetail.Populated = true;
        entities.Event1.Populated = true;
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
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Document HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
    }

    private Document hiddenCheck;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Document document;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Document Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private Document previous;
    private EventDetail eventDetail;
    private Event1 event1;
    private Document document;
  }
#endregion
}
