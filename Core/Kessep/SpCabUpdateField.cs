// Program: SP_CAB_UPDATE_FIELD, ID: 372109324, model: 746.
// Short name: SWE02225
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_FIELD.
/// </para>
/// <para>
/// Elementary process to update FIELD
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateField: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_FIELD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateField(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateField.
  /// </summary>
  public SpCabUpdateField(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Field.LastUpdatedBy = global.UserId;
    local.Field.LastUpdatedTimestamp = Now();

    if (ReadField())
    {
      if (ReadDocumentField())
      {
        ExitState = "SP0000_CANNOT_UPDATE_FIELD";

        return;
      }

      try
      {
        UpdateField();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FIELD_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FIELD_PV";

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
      ExitState = "FIELD_NF";
    }
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

  private void UpdateField()
  {
    var dependancy = import.Field.Dependancy;
    var subroutineName = import.Field.SubroutineName;
    var screenName = import.Field.ScreenName;
    var description = import.Field.Description;
    var createdBy = entities.Field.CreatedBy;
    var createdTimestamp = entities.Field.CreatedTimestamp;
    var lastUpdatedBy = local.Field.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.Field.LastUpdatedTimestamp;

    entities.Field.Populated = false;
    Update("UpdateField",
      (db, command) =>
      {
        db.SetString(command, "dependancy", dependancy);
        db.SetString(command, "subroutineName", subroutineName);
        db.SetString(command, "screenName", screenName);
        db.SetString(command, "description", description);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "name", entities.Field.Name);
      });

    entities.Field.Dependancy = dependancy;
    entities.Field.SubroutineName = subroutineName;
    entities.Field.ScreenName = screenName;
    entities.Field.Description = description;
    entities.Field.LastUpdatedBy = lastUpdatedBy;
    entities.Field.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Field.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
