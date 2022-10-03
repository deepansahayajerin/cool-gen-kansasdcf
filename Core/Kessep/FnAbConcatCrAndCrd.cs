// Program: FN_AB_CONCAT_CR_AND_CRD, ID: 371870676, model: 746.
// Short name: SWE00240
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_CONCAT_CR_AND_CRD.
/// </summary>
[Serializable]
public partial class FnAbConcatCrAndCrd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_CONCAT_CR_AND_CRD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbConcatCrAndCrd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbConcatCrAndCrd.
  /// </summary>
  public FnAbConcatCrAndCrd(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CrdCrComboNo.CrdCrCombo =
      NumberToString(import.CashReceipt.SequentialNumber, 9, 9) + "-" + NumberToString
      (import.CashReceiptDetail.SequentialIdentifier, 12, 4);
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    private CrdCrComboNo crdCrComboNo;
  }
#endregion
}
