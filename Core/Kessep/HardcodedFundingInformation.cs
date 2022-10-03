// Program: HARDCODED_FUNDING_INFORMATION, ID: 371725747, model: 746.
// Short name: SWE00707
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: HARDCODED_FUNDING_INFORMATION.
/// </para>
/// <para>
/// RESP: CASHMGMT	
/// This action block sets some hardcoded values for the funding processes.  
/// This approach was taken because the funding area is external to CSE and at
/// the time was being redesigned by another project.  Maybe this will all be
/// replaced someday.
/// </para>
/// </summary>
[Serializable]
public partial class HardcodedFundingInformation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the HARDCODED_FUNDING_INFORMATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new HardcodedFundingInformation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of HardcodedFundingInformation.
  /// </summary>
  public HardcodedFundingInformation(IContext context, Import import,
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
    export.ClearingFundRevenue.Code = "22140";
    export.ClearingFund.SystemGeneratedIdentifier = 9069;
    export.Deposit.SystemGeneratedIdentifier = 1;
    export.Refund.SystemGeneratedIdentifier = 5;
    export.Active.SystemGeneratedIdentifier = 1;
    export.Inactive.SystemGeneratedIdentifier = 2;
    export.Open.SystemGeneratedIdentifier = 3;
    export.Closed.SystemGeneratedIdentifier = 4;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public FundTransactionStatus Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public FundTransactionStatus Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    /// <summary>
    /// A value of Inactive.
    /// </summary>
    [JsonPropertyName("inactive")]
    public FundTransactionStatus Inactive
    {
      get => inactive ??= new();
      set => inactive = value;
    }

    /// <summary>
    /// A value of Refund.
    /// </summary>
    [JsonPropertyName("refund")]
    public FundTransactionType Refund
    {
      get => refund ??= new();
      set => refund = value;
    }

    /// <summary>
    /// A value of ClearingFundRevenue.
    /// </summary>
    [JsonPropertyName("clearingFundRevenue")]
    public ProgramCostAccount ClearingFundRevenue
    {
      get => clearingFundRevenue ??= new();
      set => clearingFundRevenue = value;
    }

    /// <summary>
    /// A value of Deposit.
    /// </summary>
    [JsonPropertyName("deposit")]
    public FundTransactionType Deposit
    {
      get => deposit ??= new();
      set => deposit = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public FundTransactionStatus Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of ClearingFund.
    /// </summary>
    [JsonPropertyName("clearingFund")]
    public Fund ClearingFund
    {
      get => clearingFund ??= new();
      set => clearingFund = value;
    }

    private FundTransactionStatus open;
    private FundTransactionStatus closed;
    private FundTransactionStatus inactive;
    private FundTransactionType refund;
    private ProgramCostAccount clearingFundRevenue;
    private FundTransactionType deposit;
    private FundTransactionStatus active;
    private Fund clearingFund;
  }
#endregion
}
