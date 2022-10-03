// Program: EAB_UNPROCESSED_CASH_RECEIPT_DTL, ID: 372736457, model: 746.
// Short name: SWEXF730
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_UNPROCESSED_CASH_RECEIPT_DTL.
/// </summary>
[Serializable]
public partial class EabUnprocessedCashReceiptDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_UNPROCESSED_CASH_RECEIPT_DTL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabUnprocessedCashReceiptDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabUnprocessedCashReceiptDtl.
  /// </summary>
  public EabUnprocessedCashReceiptDtl(IContext context, Import import,
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
      "SWEXF730", context, import, export, EabOptions.Hpvp);
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
    /// A value of ConcatenateName.
    /// </summary>
    [JsonPropertyName("concatenateName")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public ConcatenateName ConcatenateName
    {
      get => concatenateName ??= new();
      set => concatenateName = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "ProcessDate" })]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "UserId" })]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Code" })]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Code" })]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of Undist.
    /// </summary>
    [JsonPropertyName("undist")]
    [Member(Index = 6, AccessFields = false, Members
      = new[] { "CollectionAmount" })]
    public CashReceiptDetail Undist
    {
      get => undist ??= new();
      set => undist = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    [Member(Index = 7, AccessFields = false, Members = new[]
    {
      "SequentialIdentifier",
      "CourtOrderNumber",
      "CollectionAmount",
      "ObligorPersonNumber",
      "RefundedAmount",
      "DistributedAmount"
    })]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    [Member(Index = 8, AccessFields = false, Members
      = new[] { "SequentialNumber", "ReceivedDate" })]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 9, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private ConcatenateName concatenateName;
    private ProgramProcessingInfo programProcessingInfo;
    private ServiceProvider serviceProvider;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail undist;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private ReportParms reportParms;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
