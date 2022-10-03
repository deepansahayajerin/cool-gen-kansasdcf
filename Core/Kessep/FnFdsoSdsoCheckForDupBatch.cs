// Program: FN_FDSO_SDSO_CHECK_FOR_DUP_BATCH, ID: 372914402, model: 746.
// Short name: SWE01528
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_FDSO_SDSO_CHECK_FOR_DUP_BATCH.
/// </para>
/// <para>
/// Verify that NO Existing Cash Receipt matches the just created Cash Receipt 
/// on cash amount, cash count, adjustment count and net.
/// </para>
/// </summary>
[Serializable]
public partial class FnFdsoSdsoCheckForDupBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_FDSO_SDSO_CHECK_FOR_DUP_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnFdsoSdsoCheckForDupBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnFdsoSdsoCheckForDupBatch.
  /// </summary>
  public FnFdsoSdsoCheckForDupBatch(IContext context, Import import,
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
    export.DuplicateCount.Count = 0;
    local.DaysToSearch.Date =
      AddDays(import.CashReceipt.ReceivedDate, -
      import.FindDuplicateDaysCount.Count);

    foreach(var item in ReadCashReceipt())
    {
      ++export.DuplicateCount.Count;
      export.Found.Assign(entities.CashReceipt);
    }
  }

  private IEnumerable<bool> ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt",
      (db, command) =>
      {
        db.SetString(command, "createdBy", import.CashReceipt.CreatedBy);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
        db.SetDate(
          command, "receivedDate1",
          import.CashReceipt.ReceivedDate.GetValueOrDefault());
        db.SetDate(
          command, "receivedDate2",
          local.DaysToSearch.Date.GetValueOrDefault());
        db.SetNullableDecimal(
          command, "totalCashTransac",
          import.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault());
        db.SetNullableDecimal(
          command, "cashDue", import.CashReceipt.CashDue.GetValueOrDefault());
        db.SetNullableInt32(
          command, "totCashTranCnt",
          import.CashReceipt.TotalCashTransactionCount.GetValueOrDefault());
        db.SetNullableInt32(
          command, "totDetailAdjCnt",
          import.CashReceipt.TotalDetailAdjustmentCount.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 5);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 7);
        entities.CashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 8);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 9);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 10);
        entities.CashReceipt.Populated = true;

        return true;
      });
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
    /// A value of FindDuplicateDaysCount.
    /// </summary>
    [JsonPropertyName("findDuplicateDaysCount")]
    public Common FindDuplicateDaysCount
    {
      get => findDuplicateDaysCount ??= new();
      set => findDuplicateDaysCount = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private Common findDuplicateDaysCount;
    private CashReceipt cashReceipt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DuplicateCount.
    /// </summary>
    [JsonPropertyName("duplicateCount")]
    public Common DuplicateCount
    {
      get => duplicateCount ??= new();
      set => duplicateCount = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public CashReceipt Found
    {
      get => found ??= new();
      set => found = value;
    }

    private Common duplicateCount;
    private CashReceipt found;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DaysToSearch.
    /// </summary>
    [JsonPropertyName("daysToSearch")]
    public DateWorkArea DaysToSearch
    {
      get => daysToSearch ??= new();
      set => daysToSearch = value;
    }

    private DateWorkArea daysToSearch;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceipt cashReceipt;
  }
#endregion
}
