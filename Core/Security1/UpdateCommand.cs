// Program: UPDATE_COMMAND, ID: 371452768, model: 746.
// Short name: SWE01469
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: UPDATE_COMMAND.
/// </summary>
[Serializable]
public partial class UpdateCommand: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_COMMAND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCommand(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCommand.
  /// </summary>
  public UpdateCommand(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadCommand())
    {
      ExitState = "SC0012_COMMAND_NF";

      return;
    }

    try
    {
      UpdateCommand1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0012_COMMAND_NU";

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

  private bool ReadCommand()
  {
    entities.Command.Populated = false;

    return Read("ReadCommand",
      (db, command) =>
      {
        db.SetString(command, "cmdValue", import.Command.Value);
      },
      (db, reader) =>
      {
        entities.Command.Value = db.GetString(reader, 0);
        entities.Command.Desc = db.GetNullableString(reader, 1);
        entities.Command.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Command.LastUpdatedTstamp = db.GetNullableDateTime(reader, 3);
        entities.Command.Populated = true;
      });
  }

  private void UpdateCommand1()
  {
    var desc = import.Command.Desc ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.Command.Populated = false;
    Update("UpdateCommand",
      (db, command) =>
      {
        db.SetNullableString(command, "cmdDesc", desc);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTstam", lastUpdatedTstamp);
        db.SetString(command, "cmdValue", entities.Command.Value);
      });

    entities.Command.Desc = desc;
    entities.Command.LastUpdatedBy = lastUpdatedBy;
    entities.Command.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Command.Populated = true;
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
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Command Command
    {
      get => command ??= new();
      set => command = value;
    }

    private Command command;
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
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Command Command
    {
      get => command ??= new();
      set => command = value;
    }

    private Command command;
  }
#endregion
}
