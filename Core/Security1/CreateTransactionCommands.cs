// Program: CREATE_TRANSACTION_COMMANDS, ID: 371451887, model: 746.
// Short name: SWE00166
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: CREATE_TRANSACTION_COMMANDS.
/// </summary>
[Serializable]
public partial class CreateTransactionCommands: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_TRANSACTION_COMMANDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateTransactionCommands(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateTransactionCommands.
  /// </summary>
  public CreateTransactionCommands(IContext context, Import import,
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
    if (!ReadTransaction())
    {
      ExitState = "SC0002_SCREEN_ID_NF";

      return;
    }

    if (!ReadCommand())
    {
      ExitState = "SC0012_COMMAND_NF";

      return;
    }

    try
    {
      CreateTransactionCommand();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0018_TRANS_COMMAND_AE";

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

  private void CreateTransactionCommand()
  {
    var fkTrnScreenid = entities.ExistingTransaction.ScreenId;
    var fkTrnTrancode = entities.ExistingTransaction.Trancode;
    var fkCmdValue = entities.ExistingCommand.Value;

    entities.ExistingTransactionCommand.Populated = false;
    Update("CreateTransactionCommand",
      (db, command) =>
      {
        db.SetInt32(command, "trcId", 0);
        db.SetString(command, "fkTrnScreenid", fkTrnScreenid);
        db.SetString(command, "fkTrnTrancode", fkTrnTrancode);
        db.SetString(command, "fkCmdValue", fkCmdValue);
      });

    entities.ExistingTransactionCommand.Id = 0;
    entities.ExistingTransactionCommand.FkTrnScreenid = fkTrnScreenid;
    entities.ExistingTransactionCommand.FkTrnTrancode = fkTrnTrancode;
    entities.ExistingTransactionCommand.FkCmdValue = fkCmdValue;
    entities.ExistingTransactionCommand.Populated = true;
  }

  private bool ReadCommand()
  {
    entities.ExistingCommand.Populated = false;

    return Read("ReadCommand",
      (db, command) =>
      {
        db.SetString(command, "cmdValue", import.Command.Value);
      },
      (db, reader) =>
      {
        entities.ExistingCommand.Value = db.GetString(reader, 0);
        entities.ExistingCommand.Populated = true;
      });
  }

  private bool ReadTransaction()
  {
    entities.ExistingTransaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "screenId", import.Transaction.ScreenId);
        db.SetString(command, "trancode", import.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Populated = true;
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

    /// <summary>
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    private Command command;
    private Transaction transaction;
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
    /// A value of ExistingCommand.
    /// </summary>
    [JsonPropertyName("existingCommand")]
    public Command ExistingCommand
    {
      get => existingCommand ??= new();
      set => existingCommand = value;
    }

    /// <summary>
    /// A value of ExistingTransaction.
    /// </summary>
    [JsonPropertyName("existingTransaction")]
    public Transaction ExistingTransaction
    {
      get => existingTransaction ??= new();
      set => existingTransaction = value;
    }

    /// <summary>
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
    }

    private Command existingCommand;
    private Transaction existingTransaction;
    private TransactionCommand existingTransactionCommand;
  }
#endregion
}
