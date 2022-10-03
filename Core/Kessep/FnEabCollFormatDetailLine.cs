// Program: FN_EAB_COLL_FORMAT_DETAIL_LINE, ID: 371741375, model: 746.
// Short name: SWEXFW00
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_EAB_COLL_FORMAT_DETAIL_LINE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This external action block formats a detail line for List Collection 
/// Activity by AP/Payor.  The import is a code indicating the line type (
/// collection or activity), as well as the applicable data for that line type.
/// The Cobol program then formats the screen detail line.
/// </para>
/// </summary>
[Serializable]
public partial class FnEabCollFormatDetailLine: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EAB_COLL_FORMAT_DETAIL_LINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEabCollFormatDetailLine(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEabCollFormatDetailLine.
  /// </summary>
  public FnEabCollFormatDetailLine(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEXFW00", context, import, export, EabOptions.NoIefParams);
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
    /// <summary>A ActivityLineGroup group.</summary>
    [Serializable]
    public class ActivityLineGroup
    {
      /// <summary>
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      [Member(Index = 1, AccessFields = false, Members
        = new[] { "PrimarySecondaryCode" })]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
      }

      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      [Member(Index = 2, Members = new[] { "Code" })]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      [Member(Index = 3, Members = new[] { "FormattedName" })]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      [Member(Index = 4, Members = new[] { "Code" })]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      [Member(Index = 5, Members = new[] { "UserId" })]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      [Member(Index = 6, Members = new[]
      {
        "Amount",
        "AppliedToCode",
        "DistPgmStateAppldTo"
      })]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>
      /// A value of DateWorkArea.
      /// </summary>
      [JsonPropertyName("dateWorkArea")]
      [Member(Index = 7, Members = new[] { "Date" })]
      public DateWorkArea DateWorkArea
      {
        get => dateWorkArea ??= new();
        set => dateWorkArea = value;
      }

      /// <summary>
      /// A value of AutoManualDist.
      /// </summary>
      [JsonPropertyName("autoManualDist")]
      [Member(Index = 8, Members = new[] { "Flag" })]
      public Common AutoManualDist
      {
        get => autoManualDist ??= new();
        set => autoManualDist = value;
      }

      /// <summary>
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      [Member(Index = 9, Members = new[] { "DueDt" })]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
      }

      private Obligation obligation;
      private ObligationType obligationType;
      private CsePersonsWorkSet csePersonsWorkSet;
      private Program program;
      private ServiceProvider serviceProvider;
      private Collection collection;
      private DateWorkArea dateWorkArea;
      private Common autoManualDist;
      private DebtDetail debtDetail;
    }

    /// <summary>A CollectionLineGroup group.</summary>
    [Serializable]
    public class CollectionLineGroup
    {
      /// <summary>
      /// A value of CcashReceiptEvent.
      /// </summary>
      [JsonPropertyName("ccashReceiptEvent")]
      [Member(Index = 1, AccessFields = false, Members
        = new[] { "ReceivedDate" })]
      public CashReceiptEvent CcashReceiptEvent
      {
        get => ccashReceiptEvent ??= new();
        set => ccashReceiptEvent = value;
      }

      /// <summary>
      /// A value of CcashReceiptDetailStatHistory.
      /// </summary>
      [JsonPropertyName("ccashReceiptDetailStatHistory")]
      [Member(Index = 2, Members = new[] { "ReasonText", "ReasonCodeId" })]
      public CashReceiptDetailStatHistory CcashReceiptDetailStatHistory
      {
        get => ccashReceiptDetailStatHistory ??= new();
        set => ccashReceiptDetailStatHistory = value;
      }

      /// <summary>
      /// A value of CcashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("ccashReceiptSourceType")]
      [Member(Index = 3, Members = new[] { "Code" })]
      public CashReceiptSourceType CcashReceiptSourceType
      {
        get => ccashReceiptSourceType ??= new();
        set => ccashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of Ccommon.
      /// </summary>
      [JsonPropertyName("ccommon")]
      [Member(Index = 4, Members = new[] { "TotalCurrency" })]
      public Common Ccommon
      {
        get => ccommon ??= new();
        set => ccommon = value;
      }

      /// <summary>
      /// A value of CcashReceipt.
      /// </summary>
      [JsonPropertyName("ccashReceipt")]
      [Member(Index = 5, Members = new[] { "ReceiptDate", "ReceivedDate" })]
      public CashReceipt CcashReceipt
      {
        get => ccashReceipt ??= new();
        set => ccashReceipt = value;
      }

      /// <summary>
      /// A value of CcashReceiptDetail.
      /// </summary>
      [JsonPropertyName("ccashReceiptDetail")]
      [Member(Index = 6, Members = new[]
      {
        "CollectionDate",
        "CollectionAmount",
        "RefundedAmount"
      })]
      public CashReceiptDetail CcashReceiptDetail
      {
        get => ccashReceiptDetail ??= new();
        set => ccashReceiptDetail = value;
      }

      private CashReceiptEvent ccashReceiptEvent;
      private CashReceiptDetailStatHistory ccashReceiptDetailStatHistory;
      private CashReceiptSourceType ccashReceiptSourceType;
      private Common ccommon;
      private CashReceipt ccashReceipt;
      private CashReceiptDetail ccashReceiptDetail;
    }

    /// <summary>
    /// A value of LineType.
    /// </summary>
    [JsonPropertyName("lineType")]
    [Member(Index = 1, Members = new[] { "Flag" })]
    public Common LineType
    {
      get => lineType ??= new();
      set => lineType = value;
    }

    /// <summary>
    /// Gets a value of ActivityLine.
    /// </summary>
    [JsonPropertyName("activityLine")]
    [Member(Index = 2)]
    public ActivityLineGroup ActivityLine
    {
      get => activityLine ?? (activityLine = new());
      set => activityLine = value;
    }

    /// <summary>
    /// Gets a value of CollectionLine.
    /// </summary>
    [JsonPropertyName("collectionLine")]
    [Member(Index = 3)]
    public CollectionLineGroup CollectionLine
    {
      get => collectionLine ?? (collectionLine = new());
      set => collectionLine = value;
    }

    /// <summary>
    /// A value of AdjLineAdjRsn.
    /// </summary>
    [JsonPropertyName("adjLineAdjRsn")]
    [Member(Index = 4, Members = new[] { "Code" })]
    public CollectionAdjustmentReason AdjLineAdjRsn
    {
      get => adjLineAdjRsn ??= new();
      set => adjLineAdjRsn = value;
    }

    /// <summary>
    /// A value of AdjLineCollAdj.
    /// </summary>
    [JsonPropertyName("adjLineCollAdj")]
    [Member(Index = 5, Members = new[]
    {
      "Amount",
      "CollectionAdjustmentDt",
      "LastUpdatedBy"
    })]
    public Collection AdjLineCollAdj
    {
      get => adjLineCollAdj ??= new();
      set => adjLineCollAdj = value;
    }

    private Common lineType;
    private ActivityLineGroup activityLine;
    private CollectionLineGroup collectionLine;
    private CollectionAdjustmentReason adjLineAdjRsn;
    private Collection adjLineCollAdj;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ListScreenWorkArea.
    /// </summary>
    [JsonPropertyName("listScreenWorkArea")]
    [Member(Index = 1, Members = new[] { "TextLine76" })]
    public ListScreenWorkArea ListScreenWorkArea
    {
      get => listScreenWorkArea ??= new();
      set => listScreenWorkArea = value;
    }

    private ListScreenWorkArea listScreenWorkArea;
  }
#endregion
}
