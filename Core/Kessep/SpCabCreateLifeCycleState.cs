// Program: SP_CAB_CREATE_LIFE_CYCLE_STATE, ID: 371779160, model: 746.
// Short name: SWE01724
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_LIFE_CYCLE_STATE.
/// </summary>
[Serializable]
public partial class SpCabCreateLifeCycleState: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_LIFE_CYCLE_STATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateLifeCycleState(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateLifeCycleState.
  /// </summary>
  public SpCabCreateLifeCycleState(IContext context, Import import,
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
    try
    {
      CreateLifecycleState();
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

  private void CreateLifecycleState()
  {
    var identifier = import.LifecycleState.Identifier;
    var description = import.LifecycleState.Description ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.New1.Populated = false;
    Update("CreateLifecycleState",
      (db, command) =>
      {
        db.SetString(command, "identifier", identifier);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.New1.Identifier = identifier;
    entities.New1.Description = description;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.Populated = true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public LifecycleState New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private LifecycleState new1;
  }
#endregion
}
