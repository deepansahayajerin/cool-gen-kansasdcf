// Program: CREATE_COMMAND, ID: 371452769, model: 746.
// Short name: SWE00130
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: CREATE_COMMAND.
/// </summary>
[Serializable]
public partial class CreateCommand: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_COMMAND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateCommand(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateCommand.
  /// </summary>
  public CreateCommand(IContext context, Import import, Export export):
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
      CreateCommand1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0012_COMMAND_AE";

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

  private void CreateCommand1()
  {
    var value = import.Command.Value;
    var desc = import.Command.Desc ?? "";
    var createdBy = global.UserId;
    var createdTstamp = Now();

    entities.Command.Populated = false;
    Update("CreateCommand",
      (db, command) =>
      {
        db.SetString(command, "cmdValue", value);
        db.SetNullableString(command, "cmdDesc", desc);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTstam", null);
      });

    entities.Command.Value = value;
    entities.Command.Desc = desc;
    entities.Command.CreatedBy = createdBy;
    entities.Command.CreatedTstamp = createdTstamp;
    entities.Command.LastUpdatedBy = "";
    entities.Command.LastUpdatedTstamp = null;
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
