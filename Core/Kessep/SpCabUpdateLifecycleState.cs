// Program: SP_CAB_UPDATE_LIFECYCLE_STATE, ID: 371779159, model: 746.
// Short name: SWE01726
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_LIFECYCLE_STATE.
/// </summary>
[Serializable]
public partial class SpCabUpdateLifecycleState: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_LIFECYCLE_STATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateLifecycleState(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateLifecycleState.
  /// </summary>
  public SpCabUpdateLifecycleState(IContext context, Import import,
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
    if (ReadLifecycleState())
    {
      try
      {
        UpdateLifecycleState();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_LIFECYCLE_STATE_AE";

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
      ExitState = "SP0000_LIFECYCLE_STATE_NF";
    }
  }

  private bool ReadLifecycleState()
  {
    entities.LifecycleState.Populated = false;

    return Read("ReadLifecycleState",
      (db, command) =>
      {
        db.SetString(command, "identifier", import.LifecycleState.Identifier);
      },
      (db, reader) =>
      {
        entities.LifecycleState.Identifier = db.GetString(reader, 0);
        entities.LifecycleState.Description = db.GetNullableString(reader, 1);
        entities.LifecycleState.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.LifecycleState.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.LifecycleState.Populated = true;
      });
  }

  private void UpdateLifecycleState()
  {
    var description = import.LifecycleState.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LifecycleState.Populated = false;
    Update("UpdateLifecycleState",
      (db, command) =>
      {
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "identifier", entities.LifecycleState.Identifier);
      });

    entities.LifecycleState.Description = description;
    entities.LifecycleState.LastUpdatedBy = lastUpdatedBy;
    entities.LifecycleState.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LifecycleState.Populated = true;
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
    /// A value of LifecycleState.
    /// </summary>
    [JsonPropertyName("lifecycleState")]
    public LifecycleState LifecycleState
    {
      get => lifecycleState ??= new();
      set => lifecycleState = value;
    }

    private LifecycleState lifecycleState;
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
    /// A value of LifecycleState.
    /// </summary>
    [JsonPropertyName("lifecycleState")]
    public LifecycleState LifecycleState
    {
      get => lifecycleState ??= new();
      set => lifecycleState = value;
    }

    private LifecycleState lifecycleState;
  }
#endregion
}
