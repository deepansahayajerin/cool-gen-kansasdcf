// Program: SP_CAB_CREATE_FIELD, ID: 372109322, model: 746.
// Short name: SWE02223
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_CREATE_FIELD.
/// </para>
/// <para>
/// Elementary process used to create FIELD
/// </para>
/// </summary>
[Serializable]
public partial class SpCabCreateField: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_FIELD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateField(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateField.
  /// </summary>
  public SpCabCreateField(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Field.CreatedBy = global.UserId;
    local.Field.CreatedTimestamp = Now();

    try
    {
      CreateField();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FIELD_AE";

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

  private void CreateField()
  {
    var name = import.Field.Name;
    var dependancy = import.Field.Dependancy;
    var subroutineName = import.Field.SubroutineName;
    var screenName = import.Field.ScreenName;
    var description = import.Field.Description;
    var createdBy = local.Field.CreatedBy;
    var createdTimestamp = local.Field.CreatedTimestamp;

    entities.Field.Populated = false;
    Update("CreateField",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetString(command, "dependancy", dependancy);
        db.SetString(command, "subroutineName", subroutineName);
        db.SetString(command, "screenName", screenName);
        db.SetString(command, "description", description);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
      });

    entities.Field.Name = name;
    entities.Field.Dependancy = dependancy;
    entities.Field.SubroutineName = subroutineName;
    entities.Field.ScreenName = screenName;
    entities.Field.Description = description;
    entities.Field.CreatedBy = createdBy;
    entities.Field.CreatedTimestamp = createdTimestamp;
    entities.Field.LastUpdatedBy = "";
    entities.Field.LastUpdatedTimestamp = null;
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
#endregion
}
