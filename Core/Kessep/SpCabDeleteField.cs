// Program: SP_CAB_DELETE_FIELD, ID: 372109323, model: 746.
// Short name: SWE02224
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_DELETE_FIELD.
/// </para>
/// <para>
/// Elementary process for delete of FIELD.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDeleteField: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_FIELD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteField(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteField.
  /// </summary>
  public SpCabDeleteField(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadField())
    {
      if (ReadDocumentField())
      {
        ExitState = "SP0000_CANNOT_DELETE_FIELD";

        return;
      }

      DeleteField();
    }
    else
    {
      ExitState = "FIELD_NF";
    }
  }

  private void DeleteField()
  {
    Update("DeleteField",
      (db, command) =>
      {
        db.SetString(command, "name", entities.Field.Name);
      });
  }

  private bool ReadDocumentField()
  {
    entities.DocumentField.Populated = false;

    return Read("ReadDocumentField",
      (db, command) =>
      {
        db.SetString(command, "fldName", entities.Field.Name);
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
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.ScreenName = db.GetString(reader, 3);
        entities.Field.Description = db.GetString(reader, 4);
        entities.Field.CreatedBy = db.GetString(reader, 5);
        entities.Field.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Field.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Field.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 8);
        entities.Field.Populated = true;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    private Field field;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    private FieldValue fieldValue;
    private DocumentField documentField;
    private Field field;
  }
#endregion
}
