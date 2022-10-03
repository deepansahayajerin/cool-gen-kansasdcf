// Program: SP_CAB_DELETE_DOCUMENT_FIELD, ID: 372103791, model: 746.
// Short name: SWE02228
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_DOCUMENT_FIELD.
/// </summary>
[Serializable]
public partial class SpCabDeleteDocumentField: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_DOCUMENT_FIELD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteDocumentField(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteDocumentField.
  /// </summary>
  public SpCabDeleteDocumentField(IContext context, Import import, Export export)
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

    if (!ReadDocumentField())
    {
      ExitState = "DOCUMENT_FIELD_NF_RB";

      return;
    }

    if (ReadFieldValue())
    {
      ExitState = "SP0000_CANNOT_DELETE_FIELD";

      return;
    }

    DeleteDocumentField();
  }

  private void DeleteDocumentField()
  {
    Update("DeleteDocumentField",
      (db, command) =>
      {
        db.SetString(command, "fldName", entities.DocumentField.FldName);
        db.SetString(command, "docName", entities.DocumentField.DocName);
        db.SetDate(
          command, "docEffectiveDte",
          entities.DocumentField.DocEffectiveDte.GetValueOrDefault());
      });
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

  private bool ReadDocumentField()
  {
    entities.DocumentField.Populated = false;

    return Read("ReadDocumentField",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
        db.SetString(command, "fldName", entities.Field.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.ScreenPrompt = db.GetString(reader, 2);
        entities.DocumentField.CreatedBy = db.GetString(reader, 3);
        entities.DocumentField.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.DocumentField.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.DocumentField.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.DocumentField.FldName = db.GetString(reader, 7);
        entities.DocumentField.DocName = db.GetString(reader, 8);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 9);
        entities.DocumentField.Populated = true;
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

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.DocumentField.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "docName", entities.DocumentField.DocName);
        db.SetString(command, "fldName", entities.DocumentField.FldName);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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

    private Field field;
    private DocumentField documentField;
    private Document document;
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

    private DateWorkArea max;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private Field field;
    private DocumentField documentField;
    private Document document;
    private FieldValue fieldValue;
  }
#endregion
}
