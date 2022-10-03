// Program: CAB_COMPUTE_AVG_MONTHLY_INCOME, ID: 371762892, model: 746.
// Short name: SWE00028
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_COMPUTE_AVG_MONTHLY_INCOME.
/// </para>
/// <para>
/// Computes the average monthly income for a given income amount and frequency.
/// </para>
/// </summary>
[Serializable]
public partial class CabComputeAvgMonthlyIncome: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_COMPUTE_AVG_MONTHLY_INCOME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabComputeAvgMonthlyIncome(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabComputeAvgMonthlyIncome.
  /// </summary>
  public CabComputeAvgMonthlyIncome(IContext context, Import import,
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
    switch(AsChar(import.New1.Freq))
    {
      case 'M':
        // -------
        // Monthly
        // -------
        export.Common.TotalCurrency = import.New1.IncomeAmt.GetValueOrDefault();

        break;
      case 'W':
        // -------
        // Weekly
        // -------
        export.Common.TotalCurrency =
          import.New1.IncomeAmt.GetValueOrDefault() * 52 / 12;

        break;
      case 'B':
        // ---------
        // Bi-Weekly
        // ---------
        export.Common.TotalCurrency =
          import.New1.IncomeAmt.GetValueOrDefault() * 26 / 12;

        break;
      case 'S':
        // ------------
        // Semi-Monthly
        // ------------
        export.Common.TotalCurrency =
          import.New1.IncomeAmt.GetValueOrDefault() * 2;

        break;
      case 'D':
        // ------------
        // Daily
        // ------------
        export.Common.TotalCurrency =
          import.New1.IncomeAmt.GetValueOrDefault() * 365 / 12;

        // ------------
        // Deduct the weekends wages, assuming that the workker works only for 5
        // days a week
        // ------------
        export.Common.TotalCurrency -= import.New1.IncomeAmt.
          GetValueOrDefault() * 104 / 12;

        break;
      default:
        break;
    }

    export.Common.TotalCurrency += import.New1.MilitaryBaqAllotment.
      GetValueOrDefault();
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PersonIncomeHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private PersonIncomeHistory new1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }
#endregion
}
