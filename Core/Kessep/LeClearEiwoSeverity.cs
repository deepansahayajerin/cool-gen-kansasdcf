// Program: LE_CLEAR_EIWO_SEVERITY, ID: 1902506334, model: 746.
// Short name: SWE00843
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CLEAR_EIWO_SEVERITY.
/// </summary>
[Serializable]
public partial class LeClearEiwoSeverity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CLEAR_EIWO_SEVERITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeClearEiwoSeverity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeClearEiwoSeverity.
  /// </summary>
  public LeClearEiwoSeverity(IContext context, Import import, Export export):
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
    if (!ReadIwoTransaction())
    {
      ExitState = "IWO_TRANSACTION_NF";

      return;
    }

    if (!ReadIwoAction())
    {
      ExitState = "IWO_ACTION_NF";

      return;
    }

    try
    {
      UpdateIwoTransaction();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "IWO_TRANSACTION_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "IWO_TRANSACTION_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      UpdateIwoAction();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "IWO_ACTION_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "IWO_ACTION_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    ReadIwoActionHistory();
    ++local.Max.Identifier;

    try
    {
      CreateIwoActionHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "IWO_ACTION_HISTORY_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "IWO_ACTION_HISTORY_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateIwoActionHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var identifier = local.Max.Identifier;
    var actionTaken = "Z";
    var actionDate = Now().Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspNumber = entities.IwoAction.CspNumber;
    var lgaIdentifier = entities.IwoAction.LgaIdentifier;
    var iwtIdentifier = entities.IwoAction.IwtIdentifier;
    var iwaIdentifier = entities.IwoAction.Identifier;

    entities.IwoActionHistory.Populated = false;
    Update("CreateIwoActionHistory",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "actionTaken", actionTaken);
        db.SetNullableDate(command, "actionDate", actionDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", iwtIdentifier);
        db.SetInt32(command, "iwaIdentifier", iwaIdentifier);
      });

    entities.IwoActionHistory.Identifier = identifier;
    entities.IwoActionHistory.ActionTaken = actionTaken;
    entities.IwoActionHistory.ActionDate = actionDate;
    entities.IwoActionHistory.CreatedBy = createdBy;
    entities.IwoActionHistory.CreatedTimestamp = createdTimestamp;
    entities.IwoActionHistory.LastUpdatedBy = "";
    entities.IwoActionHistory.LastUpdatedTimestamp = null;
    entities.IwoActionHistory.CspNumber = cspNumber;
    entities.IwoActionHistory.LgaIdentifier = lgaIdentifier;
    entities.IwoActionHistory.IwtIdentifier = iwtIdentifier;
    entities.IwoActionHistory.IwaIdentifier = iwaIdentifier;
    entities.IwoActionHistory.Populated = true;
  }

  private bool ReadIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.IwoAction.Populated = false;

    return Read("ReadIwoAction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.SeverityClearedInd = db.GetNullableString(reader, 1);
        entities.IwoAction.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.IwoAction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.IwoAction.CspNumber = db.GetString(reader, 4);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 6);
        entities.IwoAction.Populated = true;
      });
  }

  private bool ReadIwoActionHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    local.Max.Populated = false;

    return Read("ReadIwoActionHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
        db.SetInt32(command, "iwaIdentifier", entities.IwoAction.Identifier);
      },
      (db, reader) =>
      {
        local.Max.Identifier = db.GetInt32(reader, 0);
        local.Max.Populated = true;
      });
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
        entities.IwoTransaction.Note = db.GetNullableString(reader, 1);
        entities.IwoTransaction.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.IwoTransaction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 4);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 5);
        entities.IwoTransaction.Populated = true;
      });
  }

  private void UpdateIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var severityClearedInd = "Y";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.IwoAction.Populated = false;
    Update("UpdateIwoAction",
      (db, command) =>
      {
        db.SetNullableString(command, "svrityClearedInd", severityClearedInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.SeverityClearedInd = severityClearedInd;
    entities.IwoAction.LastUpdatedBy = lastUpdatedBy;
    entities.IwoAction.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IwoAction.Populated = true;
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
  protected readonly Local local = new();
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
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
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

    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
    private LegalAction legalAction;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public IwoActionHistory Max
    {
      get => max ??= new();
      set => max = value;
    }

    private IwoActionHistory max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IwoActionHistory.
    /// </summary>
    [JsonPropertyName("iwoActionHistory")]
    public IwoActionHistory IwoActionHistory
    {
      get => iwoActionHistory ??= new();
      set => iwoActionHistory = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
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

    private IwoActionHistory iwoActionHistory;
    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
    private LegalAction legalAction;
    private CsePerson csePerson;
  }
#endregion
}
