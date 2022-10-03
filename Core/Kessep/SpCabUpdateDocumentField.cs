// Program: SP_CAB_UPDATE_DOCUMENT_FIELD, ID: 372103792, model: 746.
// Short name: SWE02229
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_DOCUMENT_FIELD.
/// </summary>
[Serializable]
public partial class SpCabUpdateDocumentField: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_DOCUMENT_FIELD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateDocumentField(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateDocumentField.
  /// </summary>
  public SpCabUpdateDocumentField(IContext context, Import import, Export export)
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

    if (!ReadField2())
    {
      ExitState = "FIELD_NF";

      return;
    }

    if (!ReadDocumentField())
    {
      ExitState = "DOCUMENT_FIELD_NF_RB";

      return;
    }

    if (Equal(import.New1.Name, import.Old.Name))
    {
      try
      {
        UpdateDocumentField();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DOCUMENT_FIELD_NU_RB";

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
    else
    {
      if (!ReadField1())
      {
        ExitState = "FIELD_NF";

        return;
      }

      if (ReadFieldValue())
      {
        ExitState = "SP0000_CANNOT_UPDATE_FIELD";

        return;
      }

      try
      {
        CreateDocumentField();
        DeleteDocumentField();
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
  }

  private void CreateDocumentField()
  {
    var position = import.DocumentField.Position;
    var requiredSwitch = import.DocumentField.RequiredSwitch;
    var screenPrompt = import.DocumentField.ScreenPrompt;
    var createdBy = entities.DocumentField.CreatedBy;
    var createdTimestamp = entities.DocumentField.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var fldName = entities.New1.Name;
    var docName = entities.Document.Name;
    var docEffectiveDte = entities.Document.EffectiveDate;

    entities.NewField.Populated = false;
    Update("CreateDocumentField",
      (db, command) =>
      {
        db.SetInt32(command, "orderPosition", position);
        db.SetString(command, "requiredSwitch", requiredSwitch);
        db.SetString(command, "screenPrompt", screenPrompt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "fldName", fldName);
        db.SetString(command, "docName", docName);
        db.SetDate(command, "docEffectiveDte", docEffectiveDte);
      });

    entities.NewField.Position = position;
    entities.NewField.RequiredSwitch = requiredSwitch;
    entities.NewField.ScreenPrompt = screenPrompt;
    entities.NewField.CreatedBy = createdBy;
    entities.NewField.CreatedTimestamp = createdTimestamp;
    entities.NewField.LastUpdatedBy = lastUpdatedBy;
    entities.NewField.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NewField.FldName = fldName;
    entities.NewField.DocName = docName;
    entities.NewField.DocEffectiveDte = docEffectiveDte;
    entities.NewField.Populated = true;
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
        db.SetString(command, "fldName", entities.Old.Name);
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

  private bool ReadField1()
  {
    entities.New1.Populated = false;

    return Read("ReadField1",
      (db, command) =>
      {
        db.SetString(command, "name", import.New1.Name);
      },
      (db, reader) =>
      {
        entities.New1.Name = db.GetString(reader, 0);
        entities.New1.Populated = true;
      });
  }

  private bool ReadField2()
  {
    entities.Old.Populated = false;

    return Read("ReadField2",
      (db, command) =>
      {
        db.SetString(command, "name", import.Old.Name);
      },
      (db, reader) =>
      {
        entities.Old.Name = db.GetString(reader, 0);
        entities.Old.Populated = true;
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

  private void UpdateDocumentField()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);

    var position = import.DocumentField.Position;
    var requiredSwitch = import.DocumentField.RequiredSwitch;
    var screenPrompt = import.DocumentField.ScreenPrompt;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.DocumentField.Populated = false;
    Update("UpdateDocumentField",
      (db, command) =>
      {
        db.SetInt32(command, "orderPosition", position);
        db.SetString(command, "requiredSwitch", requiredSwitch);
        db.SetString(command, "screenPrompt", screenPrompt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "fldName", entities.DocumentField.FldName);
        db.SetString(command, "docName", entities.DocumentField.DocName);
        db.SetDate(
          command, "docEffectiveDte",
          entities.DocumentField.DocEffectiveDte.GetValueOrDefault());
      });

    entities.DocumentField.Position = position;
    entities.DocumentField.RequiredSwitch = requiredSwitch;
    entities.DocumentField.ScreenPrompt = screenPrompt;
    entities.DocumentField.LastUpdatedBy = lastUpdatedBy;
    entities.DocumentField.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.DocumentField.Populated = true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Field New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Field Old
    {
      get => old ??= new();
      set => old = value;
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

    private Field new1;
    private Field old;
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
    /// A value of NewField.
    /// </summary>
    [JsonPropertyName("newField")]
    public DocumentField NewField
    {
      get => newField ??= new();
      set => newField = value;
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

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Field New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Field Old
    {
      get => old ??= new();
      set => old = value;
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

    private DocumentField newField;
    private FieldValue fieldValue;
    private Field new1;
    private DocumentField documentField;
    private Field old;
    private Document document;
  }
#endregion
}
