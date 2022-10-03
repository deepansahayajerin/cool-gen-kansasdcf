// Program: DELETE_COMMAND, ID: 371452770, model: 746.
// Short name: SWE00174
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: DELETE_COMMAND.
/// </summary>
[Serializable]
public partial class DeleteCommand: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_COMMAND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteCommand(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteCommand.
  /// </summary>
  public DeleteCommand(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadCommand())
    {
      DeleteCommand1();
    }
    else
    {
      ExitState = "SC0012_COMMAND_NF";
    }
  }

  private void DeleteCommand1()
  {
    Update("DeleteCommand",
      (db, command) =>
      {
        db.SetString(command, "cmdValue", entities.Command.Value);
      });
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
        entities.Command.Populated = true;
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
