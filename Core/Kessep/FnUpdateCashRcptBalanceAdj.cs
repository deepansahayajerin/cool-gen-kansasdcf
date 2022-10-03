// Program: FN_UPDATE_CASH_RCPT_BALANCE_ADJ, ID: 371726037, model: 746.
// Short name: SWE00627
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_CASH_RCPT_BALANCE_ADJ.
/// </para>
/// <para>
/// RESP:  FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateCashRcptBalanceAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_CASH_RCPT_BALANCE_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateCashRcptBalanceAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateCashRcptBalanceAdj.
  /// </summary>
  public FnUpdateCashRcptBalanceAdj(IContext context, Import import,
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
    var condition = !import.Persistent.Populated;

    if (!condition)
    {
      // IS LOCKED expression is used.
      // Entity is considered to be locked during the call.
      condition = !true;
    }

    if (condition)
    {
      if (ReadCashReceiptBalanceAdjustment())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0031_CASH_RCPT_BAL_ADJ_NF";

        return;
      }
    }

    try
    {
      UpdateCashReceiptBalanceAdjustment();
      ++export.ImportNumberOfUpdates.Count;
      export.CashReceiptBalanceAdjustment.Assign(import.Persistent);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0032_CASH_RCPT_BAL_ADJ_NU";

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

  private bool ReadCashReceiptBalanceAdjustment()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.CashReceiptBalanceAdjustment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "cashReceiptId1", import.Decreasing.SequentialNumber);
        db.SetInt32(
          command, "crrIdentifier",
          import.CashReceiptRlnRsn.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cashReceiptId2", import.Increasing.SequentialNumber);
      },
      (db, reader) =>
      {
        import.Persistent.CrtIdentifier = db.GetInt32(reader, 0);
        import.Persistent.CstIdentifier = db.GetInt32(reader, 1);
        import.Persistent.CrvIdentifier = db.GetInt32(reader, 2);
        import.Persistent.CrtIIdentifier = db.GetInt32(reader, 3);
        import.Persistent.CstIIdentifier = db.GetInt32(reader, 4);
        import.Persistent.CrvIIdentifier = db.GetInt32(reader, 5);
        import.Persistent.CrrIdentifier = db.GetInt32(reader, 6);
        import.Persistent.CreatedTimestamp = db.GetDateTime(reader, 7);
        import.Persistent.AdjustmentAmount = db.GetDecimal(reader, 8);
        import.Persistent.CreatedBy = db.GetString(reader, 9);
        import.Persistent.Description = db.GetNullableString(reader, 10);
        import.Persistent.Populated = true;
      });
  }

  private void UpdateCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var adjustmentAmount = import.CashReceiptBalanceAdjustment.AdjustmentAmount;
    var description = import.CashReceiptBalanceAdjustment.Description ?? "";

    import.Persistent.Populated = false;
    Update("UpdateCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetDecimal(command, "adjustmentAmount", adjustmentAmount);
        db.SetNullableString(command, "description", description);
        db.SetInt32(command, "crtIdentifier", import.Persistent.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.Persistent.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.Persistent.CrvIdentifier);
        db.
          SetInt32(command, "crtIIdentifier", import.Persistent.CrtIIdentifier);
          
        db.
          SetInt32(command, "cstIIdentifier", import.Persistent.CstIIdentifier);
          
        db.
          SetInt32(command, "crvIIdentifier", import.Persistent.CrvIIdentifier);
          
        db.SetInt32(command, "crrIdentifier", import.Persistent.CrrIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          import.Persistent.CreatedTimestamp.GetValueOrDefault());
      });

    import.Persistent.AdjustmentAmount = adjustmentAmount;
    import.Persistent.Description = description;
    import.Persistent.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptBalanceAdjustment Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

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
    /// A value of Increasing.
    /// </summary>
    [JsonPropertyName("increasing")]
    public CashReceipt Increasing
    {
      get => increasing ??= new();
      set => increasing = value;
    }

    /// <summary>
    /// A value of Decreasing.
    /// </summary>
    [JsonPropertyName("decreasing")]
    public CashReceipt Decreasing
    {
      get => decreasing ??= new();
      set => decreasing = value;
    }

    private CashReceiptBalanceAdjustment persistent;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceipt increasing;
    private CashReceipt decreasing;
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
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of Increasing.
    /// </summary>
    [JsonPropertyName("increasing")]
    public CashReceipt Increasing
    {
      get => increasing ??= new();
      set => increasing = value;
    }

    /// <summary>
    /// A value of Decreasing.
    /// </summary>
    [JsonPropertyName("decreasing")]
    public CashReceipt Decreasing
    {
      get => decreasing ??= new();
      set => decreasing = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceipt increasing;
    private CashReceipt decreasing;
  }
#endregion
}
