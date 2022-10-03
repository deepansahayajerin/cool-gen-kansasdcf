// Program: DELETE_TRANSACTION_COMMANDS, ID: 371451886, model: 746.
// Short name: SWE00199
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: DELETE_TRANSACTION_COMMANDS.
/// </summary>
[Serializable]
public partial class DeleteTransactionCommands: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_TRANSACTION_COMMANDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteTransactionCommands(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteTransactionCommands.
  /// </summary>
  public DeleteTransactionCommands(IContext context, Import import,
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

    if (!ReadTransactionCommand())
    {
      ExitState = "SC0018_TRANS_COMMAND_NF";

      return;
    }

    DeleteTransactionCommand();
  }

  private void DeleteTransactionCommand()
  {
    Update("DeleteTransactionCommand",
      (db, command) =>
      {
        db.SetString(
          command, "fkTrnScreenid",
          entities.ExistingTransactionCommand.FkTrnScreenid);
        db.SetString(
          command, "fkTrnTrancode",
          entities.ExistingTransactionCommand.FkTrnTrancode);
        db.SetString(
          command, "fkCmdValue",
          entities.ExistingTransactionCommand.FkCmdValue);
      });
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

  private bool ReadTransactionCommand()
  {
    entities.ExistingTransactionCommand.Populated = false;

    return Read("ReadTransactionCommand",
      (db, command) =>
      {
        db.SetString(command, "fkCmdValue", entities.ExistingCommand.Value);
        db.SetString(
          command, "fkTrnTrancode", entities.ExistingTransaction.Trancode);
        db.SetString(
          command, "fkTrnScreenid", entities.ExistingTransaction.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransactionCommand.Id = db.GetInt32(reader, 0);
        entities.ExistingTransactionCommand.FkTrnScreenid =
          db.GetString(reader, 1);
        entities.ExistingTransactionCommand.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingTransactionCommand.FkCmdValue =
          db.GetString(reader, 3);
        entities.ExistingTransactionCommand.Populated = true;
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
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Command Command
    {
      get => command ??= new();
      set => command = value;
    }

    private Transaction transaction;
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
    /// A value of ExistingTransaction.
    /// </summary>
    [JsonPropertyName("existingTransaction")]
    public Transaction ExistingTransaction
    {
      get => existingTransaction ??= new();
      set => existingTransaction = value;
    }

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
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
    }

    private Transaction existingTransaction;
    private Command existingCommand;
    private TransactionCommand existingTransactionCommand;
  }
#endregion
}
