// Program: FN_READ_CASH_RCPT_BALANCE_ADJUST, ID: 372566723, model: 746.
// Short name: SWE00531
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_CASH_RCPT_BALANCE_ADJUST.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block reads an occurrence of the Cash Receipt Balance 
/// Adjustment entity.
/// IMPORTANT:  The qualification of this read does not include the Cash Receipt
/// Balance Adjustment timestamp even though it is part of the identifier in
/// the data model.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCashRcptBalanceAdjust: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CASH_RCPT_BALANCE_ADJUST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCashRcptBalanceAdjust(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCashRcptBalanceAdjust.
  /// </summary>
  public FnReadCashRcptBalanceAdjust(IContext context, Import import,
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
    if (ReadCashReceiptBalanceAdjustment())
    {
      ++export.ImportNumberOfReads.Count;
      export.CashReceiptBalanceAdjustment.Assign(
        entities.CashReceiptBalanceAdjustment);
    }
    else
    {
      ExitState = "FN0031_CASH_RCPT_BAL_ADJ_NF";
    }
  }

  private bool ReadCashReceiptBalanceAdjustment()
  {
    entities.CashReceiptBalanceAdjustment.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.
          SetInt32(command, "cashReceiptId1", import.Decrease.SequentialNumber);
          
        db.SetInt32(
          command, "crrIdentifier",
          import.CashReceiptRlnRsn.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "cashReceiptId2", import.Increase.SequentialNumber);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptBalanceAdjustment.AdjustmentAmount =
          db.GetDecimal(reader, 8);
        entities.CashReceiptBalanceAdjustment.CreatedBy =
          db.GetString(reader, 9);
        entities.CashReceiptBalanceAdjustment.Description =
          db.GetNullableString(reader, 10);
        entities.CashReceiptBalanceAdjustment.Populated = true;
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
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
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

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    private CashReceipt increase;
    private CashReceipt decrease;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
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

    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private Common importNumberOfReads;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
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
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    private CashReceipt increase;
    private CashReceipt decrease;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
  }
#endregion
}
