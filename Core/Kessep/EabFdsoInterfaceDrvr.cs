// Program: EAB_FDSO_INTERFACE_DRVR, ID: 372529960, model: 746.
// Short name: SWEXFE99
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_FDSO_INTERFACE_DRVR.
/// </summary>
[Serializable]
public partial class EabFdsoInterfaceDrvr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FDSO_INTERFACE_DRVR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFdsoInterfaceDrvr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFdsoInterfaceDrvr.
  /// </summary>
  public EabFdsoInterfaceDrvr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXFE99", context, import, export, 0);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CollectionRecordGroup group.</summary>
    [Serializable]
    public class CollectionRecordGroup
    {
      /// <summary>
      /// A value of DetailLocalCode.
      /// </summary>
      [JsonPropertyName("detailLocalCode")]
      [Member(Index = 1, Members = new[] { "Text4" })]
      public TextWorkArea DetailLocalCode
      {
        get => detailLocalCode ??= new();
        set => detailLocalCode = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      [Member(Index = 2, Members = new[]
      {
        "ObligorPersonNumber",
        "ObligorLastName",
        "ObligorFirstName",
        "CollectionAmount",
        "ObligorSocialSecurityNumber",
        "OffsetTaxYear",
        "JointReturnInd",
        "JointReturnName",
        "InjuredSpouseInd"
      })]
      public CashReceiptDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of LocalGrpDetailAdjAmount.
      /// </summary>
      [JsonPropertyName("localGrpDetailAdjAmount")]
      [Member(Index = 3, Members = new[] { "TotalCurrency" })]
      public Common LocalGrpDetailAdjAmount
      {
        get => localGrpDetailAdjAmount ??= new();
        set => localGrpDetailAdjAmount = value;
      }

      private TextWorkArea detailLocalCode;
      private CashReceiptDetail detail;
      private Common localGrpDetailAdjAmount;
    }

    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of DetailTotAdjAmt.
      /// </summary>
      [JsonPropertyName("detailTotAdjAmt")]
      [Member(Index = 1, Members = new[] { "TotalCurrency" })]
      public Common DetailTotAdjAmt
      {
        get => detailTotAdjAmt ??= new();
        set => detailTotAdjAmt = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      [Member(Index = 2, Members = new[] { "TotalAdjustmentCount" })]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      [Member(Index = 3, Members = new[]
      {
        "TotalNoncashTransactionCount",
        "ReceiptAmount",
        "TotalNoncashTransactionAmount"
      })]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailNetAmount.
      /// </summary>
      [JsonPropertyName("detailNetAmount")]
      [Member(Index = 4, Members = new[] { "TotalCurrency" })]
      public Common DetailNetAmount
      {
        get => detailNetAmount ??= new();
        set => detailNetAmount = value;
      }

      private Common detailTotAdjAmt;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceipt detailCashReceipt;
      private Common detailNetAmount;
    }

    /// <summary>A AdditionalInfoGroup group.</summary>
    [Serializable]
    public class AdditionalInfoGroup
    {
      /// <summary>
      /// A value of GrDetailAdjAmount.
      /// </summary>
      [JsonPropertyName("grDetailAdjAmount")]
      [Member(Index = 1, AccessFields = false, Members
        = new[] { "OffsetTaxYear", "InterfaceTransId" })]
      public CashReceiptDetail GrDetailAdjAmount
      {
        get => grDetailAdjAmount ??= new();
        set => grDetailAdjAmount = value;
      }

      /// <summary>
      /// A value of DetailAddress.
      /// </summary>
      [JsonPropertyName("detailAddress")]
      [Member(Index = 2, AccessFields = false, Members = new[]
      {
        "Street1",
        "City",
        "State",
        "ZipCode5",
        "ZipCode4"
      })]
      public CashReceiptDetailAddress DetailAddress
      {
        get => detailAddress ??= new();
        set => detailAddress = value;
      }

      private CashReceiptDetail grDetailAdjAmount;
      private CashReceiptDetailAddress detailAddress;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "TextLine80", "TextReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RecordTypeReturned.
    /// </summary>
    [JsonPropertyName("recordTypeReturned")]
    [Member(Index = 2, Members = new[] { "Count" })]
    public Common RecordTypeReturned
    {
      get => recordTypeReturned ??= new();
      set => recordTypeReturned = value;
    }

    /// <summary>
    /// Gets a value of CollectionRecord.
    /// </summary>
    [JsonPropertyName("collectionRecord")]
    [Member(Index = 3)]
    public CollectionRecordGroup CollectionRecord
    {
      get => collectionRecord ?? (collectionRecord = new());
      set => collectionRecord = value;
    }

    /// <summary>
    /// Gets a value of TotalRecord.
    /// </summary>
    [JsonPropertyName("totalRecord")]
    [Member(Index = 4)]
    public TotalRecordGroup TotalRecord
    {
      get => totalRecord ?? (totalRecord = new());
      set => totalRecord = value;
    }

    /// <summary>
    /// Gets a value of AdditionalInfo.
    /// </summary>
    [JsonPropertyName("additionalInfo")]
    [Member(Index = 5)]
    public AdditionalInfoGroup AdditionalInfo
    {
      get => additionalInfo ?? (additionalInfo = new());
      set => additionalInfo = value;
    }

    private External external;
    private Common recordTypeReturned;
    private CollectionRecordGroup collectionRecord;
    private TotalRecordGroup totalRecord;
    private AdditionalInfoGroup additionalInfo;
  }
#endregion
}
