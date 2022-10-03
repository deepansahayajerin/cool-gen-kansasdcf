// Program: UPDATE_TRANSACTIONS, ID: 371743228, model: 746.
// Short name: SWE01509
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: UPDATE_TRANSACTIONS.
/// </summary>
[Serializable]
public partial class UpdateTransactions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_TRANSACTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateTransactions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateTransactions.
  /// </summary>
  public UpdateTransactions(IContext context, Import import, Export export):
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

    try
    {
      UpdateTransaction();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0002_SCREEN_ID_NU";

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
        entities.Transaction.Desc = db.GetNullableString(reader, 2);
        entities.Transaction.MenuInd = db.GetNullableString(reader, 3);
        entities.Transaction.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Transaction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.Transaction.NextTranAuthorization = db.GetString(reader, 6);
        entities.Transaction.Populated = true;
      });
  }

  private void UpdateTransaction()
  {
    var desc = import.Transaction.Desc ?? "";
    var menuInd = import.Transaction.MenuInd ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var nextTranAuthorization = import.Transaction.NextTranAuthorization;

    entities.Transaction.Populated = false;
    Update("UpdateTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "description", desc);
        db.SetNullableString(command, "menuInd", menuInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTstam", lastUpdatedTstamp);
        db.SetString(command, "nextTranAuth", nextTranAuthorization);
        db.SetString(command, "screenId", entities.Transaction.ScreenId);
        db.SetString(command, "trancode", entities.Transaction.Trancode);
      });

    entities.Transaction.Desc = desc;
    entities.Transaction.MenuInd = menuInd;
    entities.Transaction.LastUpdatedBy = lastUpdatedBy;
    entities.Transaction.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Transaction.NextTranAuthorization = nextTranAuthorization;
    entities.Transaction.Populated = true;
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
