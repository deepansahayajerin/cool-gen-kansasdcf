// Program: FN_CAB_ACCUMULATE_NET_TOTAL, ID: 372430658, model: 746.
// Short name: SWE01626
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_ACCUMULATE_NET_TOTAL.
/// </summary>
[Serializable]
public partial class FnCabAccumulateNetTotal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_ACCUMULATE_NET_TOTAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabAccumulateNetTotal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabAccumulateNetTotal.
  /// </summary>
  public FnCabAccumulateNetTotal(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (import.Adjustment.TotalCurrency != 0)
    {
      // *****
      // Add totals for total record.
      // *****
      export.ImportNet.TotalCurrency -= import.Adjustment.TotalCurrency;
      export.ImportCashReceipt.TotalDetailAdjustmentCount =
        export.ImportCashReceipt.TotalDetailAdjustmentCount.
          GetValueOrDefault() + 1;
      export.ImportAdjustment.TotalCurrency += import.Adjustment.TotalCurrency;
    }

    if (import.CashReceiptDetail.CollectionAmount != 0)
    {
      // *****
      // Add totals for total record.
      // *****
      export.ImportNet.TotalCurrency += import.CashReceiptDetail.
        CollectionAmount;
      export.ImportCashReceiptDetail.CollectionAmount += import.
        CashReceiptDetail.CollectionAmount;
      export.ImportCashReceipt.TotalNoncashTransactionCount =
        export.ImportCashReceipt.TotalNoncashTransactionCount.
          GetValueOrDefault() + 1;
      export.ImportCashReceipt.TotalNoncashTransactionAmount =
        export.ImportCashReceipt.TotalNoncashTransactionAmount.
          GetValueOrDefault() + import.CashReceiptDetail.CollectionAmount;
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public Common Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private Common adjustment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImportAdjustment.
    /// </summary>
    [JsonPropertyName("importAdjustment")]
    public Common ImportAdjustment
    {
      get => importAdjustment ??= new();
      set => importAdjustment = value;
    }

    /// <summary>
    /// A value of ImportCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("importCashReceiptDetail")]
    public CashReceiptDetail ImportCashReceiptDetail
    {
      get => importCashReceiptDetail ??= new();
      set => importCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ImportNet.
    /// </summary>
    [JsonPropertyName("importNet")]
    public Common ImportNet
    {
      get => importNet ??= new();
      set => importNet = value;
    }

    /// <summary>
    /// A value of ImportCashReceipt.
    /// </summary>
    [JsonPropertyName("importCashReceipt")]
    public CashReceipt ImportCashReceipt
    {
      get => importCashReceipt ??= new();
      set => importCashReceipt = value;
    }

    private Common importAdjustment;
    private CashReceiptDetail importCashReceiptDetail;
    private Common importNet;
    private CashReceipt importCashReceipt;
  }
#endregion
}
