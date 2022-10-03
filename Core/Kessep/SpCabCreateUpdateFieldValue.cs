// Program: SP_CAB_CREATE_UPDATE_FIELD_VALUE, ID: 371914323, model: 746.
// Short name: SWE02237
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_CREATE_UPDATE_FIELD_VALUE.
/// </para>
/// <para>
/// This CAB either creates a new field_value or updates the existing 
/// field_value for a given outgoing_document, and field combination.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabCreateUpdateFieldValue: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_UPDATE_FIELD_VALUE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateUpdateFieldValue(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateUpdateFieldValue.
  /// </summary>
  public SpCabCreateUpdateFieldValue(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadOutgoingDocument())
    {
      ExitState = "OUTGOING_DOCUMENT_NF";

      return;
    }

    if (!ReadDocumentField())
    {
      ExitState = "DOCUMENT_FIELD_NF_RB";

      return;
    }

    if (ReadFieldValue())
    {
      try
      {
        UpdateFieldValue();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FIELD_VALUE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FIELD_VALUE_PV";

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
      try
      {
        CreateFieldValue();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FIELD_VALUE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FIELD_VALUE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);

    var createdBy = import.FieldValue.CreatedBy;
    var createdTimestamp = import.FieldValue.CreatedTimestamp;
    var value = import.FieldValue.Value ?? "";
    var fldName = entities.DocumentField.FldName;
    var docName = entities.DocumentField.DocName;
    var docEffectiveDte = entities.DocumentField.DocEffectiveDte;
    var infIdentifier = entities.OutgoingDocument.InfId;

    entities.FieldValue.Populated = false;
    Update("CreateFieldValue",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatdTstamp", null);
        db.SetNullableString(command, "valu", value);
        db.SetString(command, "fldName", fldName);
        db.SetString(command, "docName", docName);
        db.SetDate(command, "docEffectiveDte", docEffectiveDte);
        db.SetInt32(command, "infIdentifier", infIdentifier);
      });

    entities.FieldValue.CreatedBy = createdBy;
    entities.FieldValue.CreatedTimestamp = createdTimestamp;
    entities.FieldValue.LastUpdatedBy = "";
    entities.FieldValue.LastUpdatdTstamp = null;
    entities.FieldValue.Value = value;
    entities.FieldValue.FldName = fldName;
    entities.FieldValue.DocName = docName;
    entities.FieldValue.DocEffectiveDte = docEffectiveDte;
    entities.FieldValue.InfIdentifier = infIdentifier;
    entities.FieldValue.Populated = true;
  }

  private bool ReadDocumentField()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.DocumentField.Populated = false;

    return Read("ReadDocumentField",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.
          SetString(command, "docName", entities.OutgoingDocument.DocName ?? "");
          
        db.SetString(command, "fldName", import.Field.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 1);
        entities.DocumentField.DocName = db.GetString(reader, 2);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 3);
        entities.DocumentField.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
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
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.CreatedBy = db.GetString(reader, 0);
        entities.FieldValue.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.FieldValue.LastUpdatedBy = db.GetString(reader, 2);
        entities.FieldValue.LastUpdatdTstamp = db.GetDateTime(reader, 3);
        entities.FieldValue.Value = db.GetNullableString(reader, 4);
        entities.FieldValue.FldName = db.GetString(reader, 5);
        entities.FieldValue.DocName = db.GetString(reader, 6);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 7);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 8);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 3);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private void UpdateFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.FieldValue.Populated);

    var lastUpdatedBy = import.FieldValue.LastUpdatedBy;
    var lastUpdatdTstamp = import.FieldValue.LastUpdatdTstamp;
    var value = import.FieldValue.Value ?? "";

    entities.FieldValue.Populated = false;
    Update("UpdateFieldValue",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "valu", value);
        db.SetString(command, "fldName", entities.FieldValue.FldName);
        db.SetString(command, "docName", entities.FieldValue.DocName);
        db.SetDate(
          command, "docEffectiveDte",
          entities.FieldValue.DocEffectiveDte.GetValueOrDefault());
        db.
          SetInt32(command, "infIdentifier", entities.FieldValue.InfIdentifier);
          
      });

    entities.FieldValue.LastUpdatedBy = lastUpdatedBy;
    entities.FieldValue.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.FieldValue.Value = value;
    entities.FieldValue.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private Infrastructure infrastructure;
    private Field field;
    private FieldValue fieldValue;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of ZdelRecordedDocument.
    /// </summary>
    [JsonPropertyName("zdelRecordedDocument")]
    public ZdelRecordedDocument ZdelRecordedDocument
    {
      get => zdelRecordedDocument ??= new();
      set => zdelRecordedDocument = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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

    private OutgoingDocument outgoingDocument;
    private ZdelRecordedDocument zdelRecordedDocument;
    private Infrastructure infrastructure;
    private DocumentField documentField;
    private Document document;
    private Field field;
    private FieldValue fieldValue;
  }
#endregion
}
