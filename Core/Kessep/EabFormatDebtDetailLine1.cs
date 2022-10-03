// Program: EAB_FORMAT_DEBT_DETAIL_LINE_1, ID: 372117071, model: 746.
// Short name: SWEXFW10
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_FORMAT_DEBT_DETAIL_LINE_1.
/// </para>
/// <para>
/// This action block formats data for display debt detail information
/// </para>
/// </summary>
[Serializable]
public partial class EabFormatDebtDetailLine1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FORMAT_DEBT_DETAIL_LINE_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFormatDebtDetailLine1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFormatDebtDetailLine1.
  /// </summary>
  public EabFormatDebtDetailLine1(IContext context, Import import, Export export)
    :
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
      "SWEXFW10", context, import, export, EabOptions.NoIefParams);
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    [Member(Index = 1, Members = new[]
    {
      "CurrentAmountOwed",
      "ArrearsAmountOwed",
      "InterestAmountOwed",
      "TotalAmountOwed"
    })]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of ProgramScreenAttributes.
    /// </summary>
    [JsonPropertyName("programScreenAttributes")]
    [Member(Index = 2, Members = new[] { "ProgramTypeInd" })]
    public ProgramScreenAttributes ProgramScreenAttributes
    {
      get => programScreenAttributes ??= new();
      set => programScreenAttributes = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    [Member(Index = 3, Members = new[]
    {
      "DueDt",
      "BalanceDueAmt",
      "InterestBalanceDueAmt"
    })]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    [Member(Index = 4, Members = new[] { "Type1", "Amount" })]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    [Member(Index = 5, Members = new[] { "Code" })]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "ProgramState" })]
      
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    private ScreenOwedAmounts screenOwedAmounts;
    private ProgramScreenAttributes programScreenAttributes;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private DprProgram dprProgram;
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
