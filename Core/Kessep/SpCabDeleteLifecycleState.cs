// Program: SP_CAB_DELETE_LIFECYCLE_STATE, ID: 371779158, model: 746.
// Short name: SWE01725
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_LIFECYCLE_STATE.
/// </summary>
[Serializable]
public partial class SpCabDeleteLifecycleState: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_LIFECYCLE_STATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteLifecycleState(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteLifecycleState.
  /// </summary>
  public SpCabDeleteLifecycleState(IContext context, Import import,
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
      DeleteLifecycleState();
    }
    else
    {
      ExitState = "SP0000_LIFECYCLE_STATE_NF";
    }
  }

  private void DeleteLifecycleState()
  {
    bool exists;

    exists = Read("DeleteLifecycleState#1",
      (db, command) =>
      {
        db.SetString(command, "lcsIdPri", entities.LifecycleState.Identifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_LIFECYCLE_TRAN\".",
        "50001");
    }

    exists = Read("DeleteLifecycleState#2",
      (db, command) =>
      {
        db.SetString(command, "lcsIdPri", entities.LifecycleState.Identifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_LIFECYCLE_TRAN\".",
        "50001");
    }

    Update("DeleteLifecycleState#3",
      (db, command) =>
      {
        db.SetString(command, "lcsIdPri", entities.LifecycleState.Identifier);
      });
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
        entities.LifecycleState.Populated = true;
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
