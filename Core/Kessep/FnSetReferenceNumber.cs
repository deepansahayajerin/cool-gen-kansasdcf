// Program: FN_SET_REFERENCE_NUMBER, ID: 371742401, model: 746.
// Short name: SWE00610
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_SET_REFERENCE_NUMBER.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block sets the reference number for collections.
/// </para>
/// </summary>
[Serializable]
public partial class FnSetReferenceNumber: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SET_REFERENCE_NUMBER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSetReferenceNumber(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSetReferenceNumber.
  /// </summary>
  public FnSetReferenceNumber(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // 06/22/12 GVandy	CQ33628	Do not allow refunds to KSDLUI source type.
    // 08/12/12 GVandy	CQ42192	Do not allow refunds to CSSI source type.
    // This action block sets the reference number for collections
    if (Equal(import.CashReceiptSourceType.Code, "FDSO") || Equal
      (import.CashReceiptSourceType.Code, "SDSO") || Equal
      (import.CashReceiptSourceType.Code, "UI") || Equal
      (import.CashReceiptSourceType.Code, "KPERS") || Equal
      (import.CashReceiptSourceType.Code, "KSDLUI") || Equal
      (import.CashReceiptSourceType.Code, "CSSI"))
    {
      local.DateWorkArea.Date = Date(import.CashReceiptDetail.CreatedTmst);
      export.FnReferenceNumber.ReferenceNumber =
        NumberToString(DateToInt(local.DateWorkArea.Date), 8, 8);

      return;
    }

    if (!IsEmpty(import.CashReceiptDetail.InterfaceTransId))
    {
      export.FnReferenceNumber.ReferenceNumber =
        import.CashReceiptDetail.InterfaceTransId ?? Spaces(14);
    }
    else if (import.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault() > local
      .Blank.OffsetTaxYear.GetValueOrDefault())
    {
      export.FnReferenceNumber.ReferenceNumber =
        NumberToString(import.CashReceiptDetail.OffsetTaxYear.
          GetValueOrDefault(), 14);
    }
    else
    {
      export.FnReferenceNumber.ReferenceNumber =
        import.CashReceipt.CheckNumber ?? Spaces(14);
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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

    private CashReceiptSourceType cashReceiptSourceType;
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
    /// A value of FnReferenceNumber.
    /// </summary>
    [JsonPropertyName("fnReferenceNumber")]
    public FnReferenceNumber FnReferenceNumber
    {
      get => fnReferenceNumber ??= new();
      set => fnReferenceNumber = value;
    }

    private FnReferenceNumber fnReferenceNumber;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CashReceiptDetail Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private DateWorkArea dateWorkArea;
    private CashReceiptDetail blank;
  }
#endregion
}
