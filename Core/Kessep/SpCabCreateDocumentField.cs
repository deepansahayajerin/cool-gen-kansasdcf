// Program: SP_CAB_CREATE_DOCUMENT_FIELD, ID: 372103793, model: 746.
// Short name: SWE02227
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_DOCUMENT_FIELD.
/// </summary>
[Serializable]
public partial class SpCabCreateDocumentField: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_DOCUMENT_FIELD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateDocumentField(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateDocumentField.
  /// </summary>
  public SpCabCreateDocumentField(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Current.Timestamp = Now();
    local.Max.Date = new DateTime(2099, 12, 31);

    if (!ReadDocument())
    {
      ExitState = "DOCUMENT_NF";

      return;
    }

    if (!ReadField())
    {
      ExitState = "FIELD_NF";

      return;
    }

    try
    {
      CreateDocumentField();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "DOCUMENT_FIELD_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "DOCUMENT_FIELD_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateDocumentField()
  {
    var position = import.DocumentField.Position;
    var requiredSwitch = import.DocumentField.RequiredSwitch;
    var screenPrompt = import.DocumentField.ScreenPrompt;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var fldName = entities.Field.Name;
    var docName = entities.Document.Name;
    var docEffectiveDte = entities.Document.EffectiveDate;

    entities.DocumentField.Populated = false;
    Update("CreateDocumentField",
      (db, command) =>
      {
        db.SetInt32(command, "orderPosition", position);
        db.SetString(command, "requiredSwitch", requiredSwitch);
        db.SetString(command, "screenPrompt", screenPrompt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "fldName", fldName);
        db.SetString(command, "docName", docName);
        db.SetDate(command, "docEffectiveDte", docEffectiveDte);
      });

    entities.DocumentField.Position = position;
    entities.DocumentField.RequiredSwitch = requiredSwitch;
    entities.DocumentField.ScreenPrompt = screenPrompt;
    entities.DocumentField.CreatedBy = createdBy;
    entities.DocumentField.CreatedTimestamp = createdTimestamp;
    entities.DocumentField.LastUpdatedBy = "";
    entities.DocumentField.LastUpdatedTimestamp = null;
    entities.DocumentField.FldName = fldName;
    entities.DocumentField.DocName = docName;
    entities.DocumentField.DocEffectiveDte = docEffectiveDte;
    entities.DocumentField.Populated = true;
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
        entities.Document.EffectiveDate = db.GetDate(reader, 1);
        entities.Document.ExpirationDate = db.GetDate(reader, 2);
        entities.Document.Populated = true;
      });
  }

  private bool ReadField()
  {
    entities.Field.Populated = false;

    return Read("ReadField",
      (db, command) =>
      {
        db.SetString(command, "name", import.Field.Name);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Populated = true;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    private Field field;
    private Document document;
    private DocumentField documentField;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea max;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    private Field field;
    private Document document;
    private DocumentField documentField;
  }
#endregion
}
