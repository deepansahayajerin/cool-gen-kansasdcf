// Program: EAB_FORMAT_NON_DEBT_DETAIL_LINE, ID: 372117067, model: 746.
// Short name: SWEXFW12
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_FORMAT_NON_DEBT_DETAIL_LINE.
/// </summary>
[Serializable]
public partial class EabFormatNonDebtDetailLine: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FORMAT_NON_DEBT_DETAIL_LINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFormatNonDebtDetailLine(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFormatNonDebtDetailLine.
  /// </summary>
  public EabFormatNonDebtDetailLine(IContext context, Import import,
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
      "SWEXFW12", context, import, export, EabOptions.NoIefParams);
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    [Member(Index = 1, Members = new[] { "Code" })]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ObligationTrnType.
    /// </summary>
    [JsonPropertyName("obligationTrnType")]
    [Member(Index = 2, Members = new[] { "ActionEntry" })]
    public Common ObligationTrnType
    {
      get => obligationTrnType ??= new();
      set => obligationTrnType = value;
    }

    /// <summary>
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    [Member(Index = 3, Members = new[] { "Date" })]
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

    /// <summary>
    /// A value of EffectiveDate.
    /// </summary>
    [JsonPropertyName("effectiveDate")]
    [Member(Index = 4, Members = new[] { "Date" })]
    public DateWorkArea EffectiveDate
    {
      get => effectiveDate ??= new();
      set => effectiveDate = value;
    }

    /// <summary>
    /// A value of TrnAmount.
    /// </summary>
    [JsonPropertyName("trnAmount")]
    [Member(Index = 5, Members = new[] { "TotalCurrency" })]
    public Common TrnAmount
    {
      get => trnAmount ??= new();
      set => trnAmount = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    [Member(Index = 6, Members = new[] { "Flag" })]
    public Common Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    [Member(Index = 7, Members = new[]
    {
      "AppliedToOrderTypeCode",
      "DistributionMethod",
      "ProgramAppliedTo",
      "DistPgmStateAppldTo"
    })]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of AdjCode.
    /// </summary>
    [JsonPropertyName("adjCode")]
    [Member(Index = 8, Members = new[] { "TextLine10" })]
    public ListScreenWorkArea AdjCode
    {
      get => adjCode ??= new();
      set => adjCode = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private Common obligationTrnType;
    private DateWorkArea collectionDate;
    private DateWorkArea effectiveDate;
    private Common trnAmount;
    private Common status;
    private Collection collection;
    private ListScreenWorkArea adjCode;
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
