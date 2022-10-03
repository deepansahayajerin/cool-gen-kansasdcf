// Program: LE_UPDATE_IWO_ACTION_STATUS, ID: 1902468731, model: 746.
// Short name: SWE00839
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_UPDATE_IWO_ACTION_STATUS.
/// </summary>
[Serializable]
public partial class LeUpdateIwoActionStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_IWO_ACTION_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateIwoActionStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateIwoActionStatus.
  /// </summary>
  public LeUpdateIwoActionStatus(IContext context, Import import, Export export):
    
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
    // 06/09/15  GVandy	CQ22212		Initial Code
    // -------------------------------------------------------------------------------------
    MoveIwoAction1(import.IwoAction, export.IwoAction);
    export.IwoTransaction.Assign(entities.IwoTransaction);

    // -- Establish currency on the IWO Transaction
    if (!ReadIwoTransaction())
    {
      ExitState = "IWO_TRANSACTION_NF";

      return;
    }

    // -- Establish currency on the IWO Action
    if (!ReadIwoAction1())
    {
      ExitState = "IWO_ACTION_NF";

      return;
    }

    if (!IsEmpty(import.IwoAction.DocumentTrackingIdentifier))
    {
      local.IwoAction.Assign(import.IwoAction);
    }
    else
    {
      MoveIwoAction2(entities.IwoAction, local.IwoAction);
    }

    // -- Update the IWO Action
    try
    {
      UpdateIwoAction();
      export.IwoAction.Assign(entities.IwoAction);
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

    // -- Determine the largest IWO Action History identifier for this IWO 
    // Action.
    ReadIwoActionHistory();
    ++local.Max.Identifier;

    // -- Create a new IWO Action History.
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

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "IWO_ACTION_HISTORY_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // -- If this is the most recent IWO Action for the IWO Transaction then 
    // update the status of the IWO Transaction.
    if (!ReadIwoAction2())
    {
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
  }

  private static void MoveIwoAction1(IwoAction source, IwoAction target)
  {
    target.Identifier = source.Identifier;
    target.StatusCd = source.StatusCd;
    target.StatusReasonCode = source.StatusReasonCode;
    target.DocumentTrackingIdentifier = source.DocumentTrackingIdentifier;
    target.FileControlId = source.FileControlId;
    target.BatchControlId = source.BatchControlId;
  }

  private static void MoveIwoAction2(IwoAction source, IwoAction target)
  {
    target.DocumentTrackingIdentifier = source.DocumentTrackingIdentifier;
    target.FileControlId = source.FileControlId;
    target.BatchControlId = source.BatchControlId;
  }

  private void CreateIwoActionHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var identifier = local.Max.Identifier;
    var actionTaken = import.IwoAction.StatusCd ?? "";
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
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
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
    entities.IwoActionHistory.CspNumber = cspNumber;
    entities.IwoActionHistory.LgaIdentifier = lgaIdentifier;
    entities.IwoActionHistory.IwtIdentifier = iwtIdentifier;
    entities.IwoActionHistory.IwaIdentifier = iwaIdentifier;
    entities.IwoActionHistory.Populated = true;
  }

  private bool ReadIwoAction1()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.IwoAction.Populated = false;

    return Read("ReadIwoAction1",
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
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 2);
        entities.IwoAction.StatusReasonCode = db.GetNullableString(reader, 3);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 4);
        entities.IwoAction.FileControlId = db.GetNullableString(reader, 5);
        entities.IwoAction.BatchControlId = db.GetNullableString(reader, 6);
        entities.IwoAction.SeverityClearedInd = db.GetNullableString(reader, 7);
        entities.IwoAction.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.IwoAction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.IwoAction.CspNumber = db.GetString(reader, 10);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 11);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 12);
        entities.IwoAction.Populated = true;
      });
  }

  private bool ReadIwoAction2()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.Other.Populated = false;

    return Read("ReadIwoAction2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
      },
      (db, reader) =>
      {
        entities.Other.Identifier = db.GetInt32(reader, 0);
        entities.Other.CspNumber = db.GetString(reader, 1);
        entities.Other.LgaIdentifier = db.GetInt32(reader, 2);
        entities.Other.IwtIdentifier = db.GetInt32(reader, 3);
        entities.Other.Populated = true;
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
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 1);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 2);
        entities.IwoTransaction.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.IwoTransaction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 6);
        entities.IwoTransaction.Populated = true;
      });
  }

  private void UpdateIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var statusCd = import.IwoAction.StatusCd ?? "";
    var statusDate = Now().Date;
    var statusReasonCode = import.IwoAction.StatusReasonCode ?? "";
    var documentTrackingIdentifier =
      local.IwoAction.DocumentTrackingIdentifier ?? "";
    var fileControlId = local.IwoAction.FileControlId ?? "";
    var batchControlId = local.IwoAction.BatchControlId ?? "";
    var severityClearedInd = "N";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.IwoAction.Populated = false;
    Update("UpdateIwoAction",
      (db, command) =>
      {
        db.SetNullableString(command, "statusCd", statusCd);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "statusReasonCd", statusReasonCode);
        db.SetNullableString(
          command, "docTrackingId", documentTrackingIdentifier);
        db.SetNullableString(command, "fileControlId", fileControlId);
        db.SetNullableString(command, "batchControlId", batchControlId);
        db.SetNullableString(command, "svrityClearedInd", severityClearedInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.StatusCd = statusCd;
    entities.IwoAction.StatusDate = statusDate;
    entities.IwoAction.StatusReasonCode = statusReasonCode;
    entities.IwoAction.DocumentTrackingIdentifier = documentTrackingIdentifier;
    entities.IwoAction.FileControlId = fileControlId;
    entities.IwoAction.BatchControlId = batchControlId;
    entities.IwoAction.SeverityClearedInd = severityClearedInd;
    entities.IwoAction.LastUpdatedBy = lastUpdatedBy;
    entities.IwoAction.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IwoAction.Populated = true;
  }

  private void UpdateIwoTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);

    var currentStatus = import.IwoAction.StatusCd ?? "";
    var statusDate = Now().Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.IwoTransaction.Populated = false;
    Update("UpdateIwoTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "currentStatus", currentStatus);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", entities.IwoTransaction.Identifier);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
      });

    entities.IwoTransaction.CurrentStatus = currentStatus;
    entities.IwoTransaction.StatusDate = statusDate;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private LegalAction legalAction;
    private IwoAction iwoAction;
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

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    private IwoTransaction iwoTransaction;
    private IwoAction iwoAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public IwoActionHistory Max
    {
      get => max ??= new();
      set => max = value;
    }

    private IwoAction iwoAction;
    private IwoActionHistory max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public IwoAction Other
    {
      get => other ??= new();
      set => other = value;
    }

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

    private IwoAction other;
    private IwoActionHistory iwoActionHistory;
    private IwoAction iwoAction;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private IwoTransaction iwoTransaction;
  }
#endregion
}
