// Program: FN_CREATE_CASH_RCPT_BALANCE_ADJ, ID: 371725265, model: 746.
// Short name: SWE00338
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RCPT_BALANCE_ADJ.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block creates an occurrence of the Cash Receipt Balance 
/// Adjustment entity.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashRcptBalanceAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RCPT_BALANCE_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashRcptBalanceAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashRcptBalanceAdj.
  /// </summary>
  public FnCreateCashRcptBalanceAdj(IContext context, Import import,
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
    // *****  Establish currency on the Cash Receipts and the Cash Receipt 
    // Relationship Reason.
    if (!import.Persistent.Populated)
    {
      if (ReadCashReceiptRlnRsn())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0093_CASH_RCPT_RLN_RSN_NF";

        return;
      }
    }

    if (!import.PersistentIncrease.Populated)
    {
      if (ReadCashReceipt2())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }
    }

    if (!import.PersistentDecrease.Populated)
    {
      if (ReadCashReceipt1())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }
    }

    // *****  Create the Cash Receipt Balance Adjustment.
    try
    {
      CreateCashReceiptBalanceAdjustment();
      ++export.ImportNumberOfUpdates.Count;
      export.CashReceiptBalanceAdjustment.Assign(
        entities.CashReceiptBalanceAdjustment);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0030_CASH_RCPT_BAL_ADJ_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0033_CASH_RCPT_BAL_ADJ_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(import.PersistentIncrease.Populated);
    System.Diagnostics.Debug.Assert(import.PersistentDecrease.Populated);

    var crtIdentifier = import.PersistentDecrease.CrtIdentifier;
    var cstIdentifier = import.PersistentDecrease.CstIdentifier;
    var crvIdentifier = import.PersistentDecrease.CrvIdentifier;
    var crtIIdentifier = import.PersistentIncrease.CrtIdentifier;
    var cstIIdentifier = import.PersistentIncrease.CstIdentifier;
    var crvIIdentifier = import.PersistentIncrease.CrvIdentifier;
    var crrIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var adjustmentAmount = import.CashReceiptBalanceAdjustment.AdjustmentAmount;
    var createdBy = global.UserId;
    var description = import.CashReceiptBalanceAdjustment.Description ?? "";

    entities.CashReceiptBalanceAdjustment.Populated = false;
    Update("CreateCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crtIIdentifier", crtIIdentifier);
        db.SetInt32(command, "cstIIdentifier", cstIIdentifier);
        db.SetInt32(command, "crvIIdentifier", crvIIdentifier);
        db.SetInt32(command, "crrIdentifier", crrIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDecimal(command, "adjustmentAmount", adjustmentAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "description", description);
      });

    entities.CashReceiptBalanceAdjustment.CrtIdentifier = crtIdentifier;
    entities.CashReceiptBalanceAdjustment.CstIdentifier = cstIdentifier;
    entities.CashReceiptBalanceAdjustment.CrvIdentifier = crvIdentifier;
    entities.CashReceiptBalanceAdjustment.CrtIIdentifier = crtIIdentifier;
    entities.CashReceiptBalanceAdjustment.CstIIdentifier = cstIIdentifier;
    entities.CashReceiptBalanceAdjustment.CrvIIdentifier = crvIIdentifier;
    entities.CashReceiptBalanceAdjustment.CrrIdentifier = crrIdentifier;
    entities.CashReceiptBalanceAdjustment.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptBalanceAdjustment.AdjustmentAmount = adjustmentAmount;
    entities.CashReceiptBalanceAdjustment.CreatedBy = createdBy;
    entities.CashReceiptBalanceAdjustment.Description = description;
    entities.CashReceiptBalanceAdjustment.Populated = true;
  }

  private bool ReadCashReceipt1()
  {
    import.PersistentDecrease.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", import.Decrease.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentDecrease.CrvIdentifier = db.GetInt32(reader, 0);
        import.PersistentDecrease.CstIdentifier = db.GetInt32(reader, 1);
        import.PersistentDecrease.CrtIdentifier = db.GetInt32(reader, 2);
        import.PersistentDecrease.SequentialNumber = db.GetInt32(reader, 3);
        import.PersistentDecrease.Populated = true;
      });
  }

  private bool ReadCashReceipt2()
  {
    import.PersistentIncrease.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", import.Increase.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentIncrease.CrvIdentifier = db.GetInt32(reader, 0);
        import.PersistentIncrease.CstIdentifier = db.GetInt32(reader, 1);
        import.PersistentIncrease.CrtIdentifier = db.GetInt32(reader, 2);
        import.PersistentIncrease.SequentialNumber = db.GetInt32(reader, 3);
        import.PersistentIncrease.Populated = true;
      });
  }

  private bool ReadCashReceiptRlnRsn()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crRlnRsnId",
          import.CashReceiptRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.Persistent.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        import.Persistent.Populated = true;
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
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptRlnRsn Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of PersistentIncrease.
    /// </summary>
    [JsonPropertyName("persistentIncrease")]
    public CashReceipt PersistentIncrease
    {
      get => persistentIncrease ??= new();
      set => persistentIncrease = value;
    }

    /// <summary>
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
    }

    /// <summary>
    /// A value of PersistentDecrease.
    /// </summary>
    [JsonPropertyName("persistentDecrease")]
    public CashReceipt PersistentDecrease
    {
      get => persistentDecrease ??= new();
      set => persistentDecrease = value;
    }

    /// <summary>
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
    }

    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn persistent;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceipt persistentIncrease;
    private CashReceipt increase;
    private CashReceipt persistentDecrease;
    private CashReceipt decrease;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
  }
#endregion
}
