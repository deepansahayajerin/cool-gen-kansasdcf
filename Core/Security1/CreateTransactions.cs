// Program: CREATE_TRANSACTIONS, ID: 371455461, model: 746.
// Short name: SWE00167
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: CREATE_TRANSACTIONS.
/// </summary>
[Serializable]
public partial class CreateTransactions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_TRANSACTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateTransactions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateTransactions.
  /// </summary>
  public CreateTransactions(IContext context, Import import, Export export):
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
      CreateTransaction();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0002_SCREEN_ID_AE";

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

  private void CreateTransaction()
  {
    var screenId = import.Transaction.ScreenId;
    var trancode = import.Transaction.Trancode;
    var desc = import.Transaction.Desc ?? "";
    var menuInd = import.Transaction.MenuInd ?? "";
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var nextTranAuthorization = import.Transaction.NextTranAuthorization;

    entities.Transaction.Populated = false;
    Update("CreateTransaction",
      (db, command) =>
      {
        db.SetString(command, "screenId", screenId);
        db.SetString(command, "trancode", trancode);
        db.SetNullableString(command, "description", desc);
        db.SetNullableString(command, "menuInd", menuInd);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTstam", null);
        db.SetString(command, "nextTranAuth", nextTranAuthorization);
      });

    entities.Transaction.ScreenId = screenId;
    entities.Transaction.Trancode = trancode;
    entities.Transaction.Desc = desc;
    entities.Transaction.MenuInd = menuInd;
    entities.Transaction.CreatedBy = createdBy;
    entities.Transaction.CreatedTstamp = createdTstamp;
    entities.Transaction.LastUpdatedBy = "";
    entities.Transaction.LastUpdatedTstamp = null;
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
