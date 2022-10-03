// Program: FN_AB_DETERMINE_COLL_AMT_APPLIED, ID: 371770029, model: 746.
// Short name: SWE01685
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_DETERMINE_COLL_AMT_APPLIED.
/// </summary>
[Serializable]
public partial class FnAbDetermineCollAmtApplied: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_DETERMINE_COLL_AMT_APPLIED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbDetermineCollAmtApplied(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbDetermineCollAmtApplied.
  /// </summary>
  public FnAbDetermineCollAmtApplied(IContext context, Import import,
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
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadCashReceipt())
    {
      // *** The collection amount applied should total the cash receipt detail 
      // received amounts for the cash receipt.  The adjusted and refunded
      // amounts should not be subtracted from this total.  Logic to support
      // this is being removed.  Sunya Sharp 11/4/98 ***
      // *** Added logic to include cash receipt details in adjusted status in 
      // the adjusted amount.  Sunya Sharp 11/4/98 ***
      local.Refunded.TotalCurrency = 0;

      foreach(var item in ReadCashReceiptDetail1())
      {
        export.CollAmtApplied.TotalCurrency += entities.CashReceiptDetail.
          ReceivedAmount;
        local.Adj.TotalCurrency = 0;
        local.Refunded.TotalCurrency += entities.CashReceiptDetail.
          RefundedAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptDetail2())
        {
          local.Adj.TotalCurrency += entities.ExistingDetailAdj.
            CollectionAmount;
        }

        if (local.Adj.TotalCurrency == 0)
        {
          if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
          {
            if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
            {
              local.Adj.TotalCurrency += entities.CashReceiptDetail.
                CollectionAmount;
            }
          }
        }

        if (import.CashReceiptDetail.SequentialIdentifier > 0)
        {
          if (import.CashReceiptDetail.SequentialIdentifier == entities
            .CashReceiptDetail.SequentialIdentifier)
          {
            MoveCashReceiptDetail(entities.CashReceiptDetail, export.Current);
            export.CurrentAdjusted.TotalCurrency = local.Adj.TotalCurrency;
          }
        }

        export.TotalAdjusted.TotalCurrency += local.Adj.TotalCurrency;
      }

      export.TotalRefunded.TotalCurrency = local.Refunded.TotalCurrency;
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 9);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ExistingDetailAdj.Populated = false;

    return ReadEach("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDetailAdj.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDetailAdj.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingDetailAdj.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingDetailAdj.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDetailAdj.CollectionAmount = db.GetDecimal(reader, 4);
        entities.ExistingDetailAdj.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 10);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotalAdjusted.
    /// </summary>
    [JsonPropertyName("totalAdjusted")]
    public Common TotalAdjusted
    {
      get => totalAdjusted ??= new();
      set => totalAdjusted = value;
    }

    /// <summary>
    /// A value of TotalRefunded.
    /// </summary>
    [JsonPropertyName("totalRefunded")]
    public Common TotalRefunded
    {
      get => totalRefunded ??= new();
      set => totalRefunded = value;
    }

    /// <summary>
    /// A value of CurrentAdjusted.
    /// </summary>
    [JsonPropertyName("currentAdjusted")]
    public Common CurrentAdjusted
    {
      get => currentAdjusted ??= new();
      set => currentAdjusted = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CashReceiptDetail Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CollAmtApplied.
    /// </summary>
    [JsonPropertyName("collAmtApplied")]
    public Common CollAmtApplied
    {
      get => collAmtApplied ??= new();
      set => collAmtApplied = value;
    }

    private Common totalAdjusted;
    private Common totalRefunded;
    private Common currentAdjusted;
    private CashReceiptDetail current;
    private Common collAmtApplied;
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
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Refunded.
    /// </summary>
    [JsonPropertyName("refunded")]
    public Common Refunded
    {
      get => refunded ??= new();
      set => refunded = value;
    }

    /// <summary>
    /// A value of Adj.
    /// </summary>
    [JsonPropertyName("adj")]
    public Common Adj
    {
      get => adj ??= new();
      set => adj = value;
    }

    /// <summary>
    /// A value of Net.
    /// </summary>
    [JsonPropertyName("net")]
    public CashReceiptDetail Net
    {
      get => net ??= new();
      set => net = value;
    }

    private DateWorkArea max;
    private Common refunded;
    private Common adj;
    private CashReceiptDetail net;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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

    /// <summary>
    /// A value of ExistingDetailAdj.
    /// </summary>
    [JsonPropertyName("existingDetailAdj")]
    public CashReceiptDetail ExistingDetailAdj
    {
      get => existingDetailAdj ??= new();
      set => existingDetailAdj = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
    }

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptDetail existingDetailAdj;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
  }
#endregion
}
