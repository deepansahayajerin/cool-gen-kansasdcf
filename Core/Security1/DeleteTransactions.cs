// Program: DELETE_TRANSACTIONS, ID: 371455541, model: 746.
// Short name: SWE00200
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: DELETE_TRANSACTIONS.
/// </summary>
[Serializable]
public partial class DeleteTransactions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_TRANSACTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteTransactions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteTransactions.
  /// </summary>
  public DeleteTransactions(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadTransaction())
    {
      DeleteTransaction();
    }
    else
    {
      ExitState = "SC0002_SCREEN_ID_NF";
    }
  }

  private void DeleteTransaction()
  {
    Update("DeleteTransaction",
      (db, command) =>
      {
        db.SetString(command, "screenId", entities.Transaction.ScreenId);
        db.SetString(command, "trancode", entities.Transaction.Trancode);
      });
  }

  private bool ReadTransaction()
  {
    entities.Transaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "screenId", import.Transaction.ScreenId);
        db.SetString(command, "trancode", import.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.Transaction.ScreenId = db.GetString(reader, 0);
        entities.Transaction.Trancode = db.GetString(reader, 1);
        entities.Transaction.Populated = true;
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
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    private Transaction transaction;
  }
#endregion
}
