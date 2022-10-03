// Program: SP_CAB_CREATE_ACTIVITY, ID: 371745523, model: 746.
// Short name: SWE01720
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_ACTIVITY.
/// </summary>
[Serializable]
public partial class SpCabCreateActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateActivity.
  /// </summary>
  public SpCabCreateActivity(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ReadActivity();
    local.Activity.ControlNumber = entities.Activity.ControlNumber + 1;

    try
    {
      CreateActivity();
      export.Activity.Assign(entities.New1);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_ACTIVITY_AE";

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

  private void CreateActivity()
  {
    var controlNumber = local.Activity.ControlNumber;
    var name = import.Activity.Name;
    var typeCode = import.Activity.TypeCode;
    var description = import.Activity.Description ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.New1.Populated = false;
    Update("CreateActivity",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", controlNumber);
        db.SetString(command, "name", name);
        db.SetString(command, "typeCode", typeCode);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.New1.ControlNumber = controlNumber;
    entities.New1.Name = name;
    entities.New1.TypeCode = typeCode;
    entities.New1.Description = description;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.Populated = true;
  }

  private bool ReadActivity()
  {
    entities.Activity.Populated = false;

    return Read("ReadActivity",
      null,
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Populated = true;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private Activity activity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private Activity activity;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private Activity activity;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Activity New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private Activity new1;
    private Activity activity;
  }
#endregion
}
