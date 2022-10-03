// Program: EAB_COURT_INTERFACE_DRVR, ID: 372565637, model: 746.
// Short name: SWEXFE03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_COURT_INTERFACE_DRVR.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block reads the edited court interface file and returns a single
/// record to the batch program for processing.
/// This action block will be used in batch processes to access a driver file.  
/// It will recieve an access instruction (OPEN, READ, CLOSE, POSITION) and will
/// send a return code and driver record.
/// </para>
/// </summary>
[Serializable]
public partial class EabCourtInterfaceDrvr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_COURT_INTERFACE_DRVR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCourtInterfaceDrvr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCourtInterfaceDrvr.
  /// </summary>
  public EabCourtInterfaceDrvr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXFE03", context, import, export, 0);
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

    /// <summary>
    /// A value of RestartCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("restartCashReceiptSourceType")]
    [Member(Index = 2, Members = new[] { "Code" })]
    public CashReceiptSourceType RestartCashReceiptSourceType
    {
      get => restartCashReceiptSourceType ??= new();
      set => restartCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("restartCashReceiptEvent")]
    [Member(Index = 3, Members = new[] { "SourceCreationDate" })]
    public CashReceiptEvent RestartCashReceiptEvent
    {
      get => restartCashReceiptEvent ??= new();
      set => restartCashReceiptEvent = value;
    }

    private External external;
    private CashReceiptSourceType restartCashReceiptSourceType;
    private CashReceiptEvent restartCashReceiptEvent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HeaderRecordGroup group.</summary>
    [Serializable]
    public class HeaderRecordGroup
    {
      /// <summary>
      /// A value of HeaderInputCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("headerInputCashReceiptSourceType")]
      [Member(Index = 1, Members = new[] { "Code" })]
      public CashReceiptSourceType HeaderInputCashReceiptSourceType
      {
        get => headerInputCashReceiptSourceType ??= new();
        set => headerInputCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of HeaderInputCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("headerInputCashReceiptEvent")]
      [Member(Index = 2, Members = new[] { "SourceCreationDate" })]
      public CashReceiptEvent HeaderInputCashReceiptEvent
      {
        get => headerInputCashReceiptEvent ??= new();
        set => headerInputCashReceiptEvent = value;
      }

      private CashReceiptSourceType headerInputCashReceiptSourceType;
      private CashReceiptEvent headerInputCashReceiptEvent;
    }

    /// <summary>A CollectionRecordGroup group.</summary>
    [Serializable]
    public class CollectionRecordGroup
    {
      /// <summary>
      /// A value of CollectionInputCollectionType.
      /// </summary>
      [JsonPropertyName("collectionInputCollectionType")]
      [Member(Index = 1, Members = new[] { "Code" })]
      public CollectionType CollectionInputCollectionType
      {
        get => collectionInputCollectionType ??= new();
        set => collectionInputCollectionType = value;
      }

      /// <summary>
      /// A value of CollectionInputCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("collectionInputCashReceiptDetail")]
      [Member(Index = 2, Members = new[]
      {
        "InterfaceTransId",
        "CourtOrderNumber",
        "ReceivedAmount",
        "CollectionDate",
        "MultiPayor",
        "ObligorSocialSecurityNumber",
        "ObligorFirstName",
        "ObligorLastName",
        "ObligorMiddleName"
      })]
      public CashReceiptDetail CollectionInputCashReceiptDetail
      {
        get => collectionInputCashReceiptDetail ??= new();
        set => collectionInputCashReceiptDetail = value;
      }

      private CollectionType collectionInputCollectionType;
      private CashReceiptDetail collectionInputCashReceiptDetail;
    }

    /// <summary>A FeesRecordGroup group.</summary>
    [Serializable]
    public class FeesRecordGroup
    {
      /// <summary>
      /// A value of FeesInputSrs.
      /// </summary>
      [JsonPropertyName("feesInputSrs")]
      [Member(Index = 1, Members = new[] { "Amount" })]
      public CashReceiptDetailFee FeesInputSrs
      {
        get => feesInputSrs ??= new();
        set => feesInputSrs = value;
      }

      /// <summary>
      /// A value of FeesInputSendingState.
      /// </summary>
      [JsonPropertyName("feesInputSendingState")]
      [Member(Index = 2, Members = new[] { "Amount" })]
      public CashReceiptDetailFee FeesInputSendingState
      {
        get => feesInputSendingState ??= new();
        set => feesInputSendingState = value;
      }

      /// <summary>
      /// A value of FeesInputCourt.
      /// </summary>
      [JsonPropertyName("feesInputCourt")]
      [Member(Index = 3, Members = new[] { "Amount" })]
      public CashReceiptDetailFee FeesInputCourt
      {
        get => feesInputCourt ??= new();
        set => feesInputCourt = value;
      }

      /// <summary>
      /// A value of FeesInputMiscellaneous.
      /// </summary>
      [JsonPropertyName("feesInputMiscellaneous")]
      [Member(Index = 4, Members = new[] { "Amount" })]
      public CashReceiptDetailFee FeesInputMiscellaneous
      {
        get => feesInputMiscellaneous ??= new();
        set => feesInputMiscellaneous = value;
      }

      private CashReceiptDetailFee feesInputSrs;
      private CashReceiptDetailFee feesInputSendingState;
      private CashReceiptDetailFee feesInputCourt;
      private CashReceiptDetailFee feesInputMiscellaneous;
    }

    /// <summary>A AdjustmentRecordGroup group.</summary>
    [Serializable]
    public class AdjustmentRecordGroup
    {
      /// <summary>
      /// A value of AdjustmentInput.
      /// </summary>
      [JsonPropertyName("adjustmentInput")]
      [Member(Index = 1, Members = new[] { "Code" })]
      public CashReceiptDetailRlnRsn AdjustmentInput
      {
        get => adjustmentInput ??= new();
        set => adjustmentInput = value;
      }

      /// <summary>
      /// A value of AdjustmentInputOriginal.
      /// </summary>
      [JsonPropertyName("adjustmentInputOriginal")]
      [Member(Index = 2, Members
        = new[] { "InterfaceTransId", "CourtOrderNumber" })]
      public CashReceiptDetail AdjustmentInputOriginal
      {
        get => adjustmentInputOriginal ??= new();
        set => adjustmentInputOriginal = value;
      }

      /// <summary>
      /// A value of AdjustmentInputNew.
      /// </summary>
      [JsonPropertyName("adjustmentInputNew")]
      [Member(Index = 3, Members = new[] { "InterfaceTransId" })]
      public CashReceiptDetail AdjustmentInputNew
      {
        get => adjustmentInputNew ??= new();
        set => adjustmentInputNew = value;
      }

      private CashReceiptDetailRlnRsn adjustmentInput;
      private CashReceiptDetail adjustmentInputOriginal;
      private CashReceiptDetail adjustmentInputNew;
    }

    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of TotalInput.
      /// </summary>
      [JsonPropertyName("totalInput")]
      [Member(Index = 1, Members = new[]
      {
        "TotalCashAmt",
        "TotalCashTransactionCount",
        "TotalCashFeeAmt",
        "TotalNonCashAmt",
        "TotalNonCashTransactionCount",
        "TotalNonCashFeeAmt",
        "AnticipatedCheckAmt",
        "TotalAdjustmentCount"
      })]
      public CashReceiptEvent TotalInput
      {
        get => totalInput ??= new();
        set => totalInput = value;
      }

      private CashReceiptEvent totalInput;
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
    /// Gets a value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    [Member(Index = 3)]
    public HeaderRecordGroup HeaderRecord
    {
      get => headerRecord ?? (headerRecord = new());
      set => headerRecord = value;
    }

    /// <summary>
    /// Gets a value of CollectionRecord.
    /// </summary>
    [JsonPropertyName("collectionRecord")]
    [Member(Index = 4)]
    public CollectionRecordGroup CollectionRecord
    {
      get => collectionRecord ?? (collectionRecord = new());
      set => collectionRecord = value;
    }

    /// <summary>
    /// Gets a value of FeesRecord.
    /// </summary>
    [JsonPropertyName("feesRecord")]
    [Member(Index = 5)]
    public FeesRecordGroup FeesRecord
    {
      get => feesRecord ?? (feesRecord = new());
      set => feesRecord = value;
    }

    /// <summary>
    /// Gets a value of AdjustmentRecord.
    /// </summary>
    [JsonPropertyName("adjustmentRecord")]
    [Member(Index = 6)]
    public AdjustmentRecordGroup AdjustmentRecord
    {
      get => adjustmentRecord ?? (adjustmentRecord = new());
      set => adjustmentRecord = value;
    }

    /// <summary>
    /// Gets a value of TotalRecord.
    /// </summary>
    [JsonPropertyName("totalRecord")]
    [Member(Index = 7)]
    public TotalRecordGroup TotalRecord
    {
      get => totalRecord ?? (totalRecord = new());
      set => totalRecord = value;
    }

    private External external;
    private Common recordTypeReturned;
    private HeaderRecordGroup headerRecord;
    private CollectionRecordGroup collectionRecord;
    private FeesRecordGroup feesRecord;
    private AdjustmentRecordGroup adjustmentRecord;
    private TotalRecordGroup totalRecord;
  }
#endregion
}
