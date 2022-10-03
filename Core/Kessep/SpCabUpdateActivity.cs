// Program: SP_CAB_UPDATE_ACTIVITY, ID: 371745522, model: 746.
// Short name: SWE01739
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_ACTIVITY.
/// </summary>
[Serializable]
public partial class SpCabUpdateActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateActivity.
  /// </summary>
  public SpCabUpdateActivity(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadActivity())
    {
      try
      {
        UpdateActivity();
        export.Activity.Assign(entities.Activity);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_ACTIVITY_NU";

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
    else
    {
      ExitState = "SP0000_ACTIVITY_NF";
    }
  }

  private bool ReadActivity()
  {
    entities.Activity.Populated = false;

    return Read("ReadActivity",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Name = db.GetString(reader, 1);
        entities.Activity.TypeCode = db.GetString(reader, 2);
        entities.Activity.Description = db.GetNullableString(reader, 3);
        entities.Activity.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Activity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.Activity.Populated = true;
      });
  }

  private void UpdateActivity()
  {
    var name = import.Activity.Name;
    var description = import.Activity.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.Activity.Populated = false;
    Update("UpdateActivity",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "controlNumber", entities.Activity.ControlNumber);
      });

    entities.Activity.Name = name;
    entities.Activity.Description = description;
    entities.Activity.LastUpdatedBy = lastUpdatedBy;
    entities.Activity.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Activity.Populated = true;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
