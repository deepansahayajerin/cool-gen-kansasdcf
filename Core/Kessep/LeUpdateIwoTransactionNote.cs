// Program: LE_UPDATE_IWO_TRANSACTION_NOTE, ID: 1902506332, model: 746.
// Short name: SWE00846
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_UPDATE_IWO_TRANSACTION_NOTE.
/// </summary>
[Serializable]
public partial class LeUpdateIwoTransactionNote: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_IWO_TRANSACTION_NOTE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateIwoTransactionNote(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateIwoTransactionNote.
  /// </summary>
  public LeUpdateIwoTransactionNote(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 08/20/15  GVandy	CQ22212		Initial Code.
    // -------------------------------------------------------------------------------------
    MoveIwoTransaction(import.IwoTransaction, export.IwoTransaction);

    if (!ReadIwoTransaction())
    {
      ExitState = "IWO_TRANSACTION_NF";

      return;
    }

    try
    {
      UpdateIwoTransaction();
      export.IwoTransaction.Assign(entities.IwoTransaction);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "IWO_TRANSACTION_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "IWO_TRANSACTION_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveIwoTransaction(IwoTransaction source,
    IwoTransaction target)
  {
    target.Identifier = source.Identifier;
    target.Note = source.Note;
  }

  private bool ReadIwoTransaction()
  {
    entities.IwoTransaction.Populated = false;

    return Read("ReadIwoTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.IwoTransaction.Identifier);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.Note = db.GetNullableString(reader, 4);
        entities.IwoTransaction.CreatedBy = db.GetString(reader, 5);
        entities.IwoTransaction.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.IwoTransaction.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.IwoTransaction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 9);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 10);
        entities.IwoTransaction.Populated = true;
      });
  }

  private void UpdateIwoTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);

    var note = import.IwoTransaction.Note ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.IwoTransaction.Populated = false;
    Update("UpdateIwoTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", entities.IwoTransaction.Identifier);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
      });

    entities.IwoTransaction.Note = note;
    entities.IwoTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.IwoTransaction.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IwoTransaction.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    private LegalAction legalAction;
    private CsePerson csePerson;
    private IwoTransaction iwoTransaction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    private IwoTransaction iwoTransaction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    private LegalAction legalAction;
    private CsePerson csePerson;
    private IwoTransaction iwoTransaction;
  }
#endregion
}
