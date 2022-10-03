// Program: EAB_STARS_VOUCHER, ID: 372879942, model: 746.
// Short name: SWEXF780
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_STARS_VOUCHER.
/// </summary>
[Serializable]
public partial class EabStarsVoucher: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_STARS_VOUCHER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabStarsVoucher(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabStarsVoucher.
  /// </summary>
  public EabStarsVoucher(IContext context, Import import, Export export):
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
      "SWEXF780", context, import, export, EabOptions.Hpvp);
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
    /// A value of StarsVoucherNumber.
    /// </summary>
    [JsonPropertyName("starsVoucherNumber")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "TextLine8" })]
    public External StarsVoucherNumber
    {
      get => starsVoucherNumber ??= new();
      set => starsVoucherNumber = value;
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
    /// A value of BreakerEft.
    /// </summary>
    [JsonPropertyName("breakerEft")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
    public Common BreakerEft
    {
      get => breakerEft ??= new();
      set => breakerEft = value;
    }

    /// <summary>
    /// A value of BreakerWar.
    /// </summary>
    [JsonPropertyName("breakerWar")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Flag" })]
    public Common BreakerWar
    {
      get => breakerWar ??= new();
      set => breakerWar = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "TraceNumber" })]
    public ElectronicFundTransmission First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "TraceNumber" })]
    public ElectronicFundTransmission Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of Amt.
    /// </summary>
    [JsonPropertyName("amt")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "VoucherTotal" })]
      
    public StarsVoucherTotals Amt
    {
      get => amt ??= new();
      set => amt = value;
    }

    /// <summary>
    /// A value of SmartTransactionEntry.
    /// </summary>
    [JsonPropertyName("smartTransactionEntry")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "SmartClassType",
      "FinYr",
      "Suffix1",
      "BusinessUnit",
      "FundCode",
      "ProgramCode",
      "DeptId",
      "AccountNumber",
      "BudgetUnit",
      "SmartR"
    })]
    public SmartTransactionEntry SmartTransactionEntry
    {
      get => smartTransactionEntry ??= new();
      set => smartTransactionEntry = value;
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

    private External starsVoucherNumber;
    private ProgramProcessingInfo programProcessingInfo;
    private Common breakerEft;
    private Common breakerWar;
    private ElectronicFundTransmission first;
    private ElectronicFundTransmission last;
    private StarsVoucherTotals amt;
    private SmartTransactionEntry smartTransactionEntry;
    private ReportParms reportParms;
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
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
