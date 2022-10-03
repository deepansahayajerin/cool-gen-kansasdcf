// Program: FN_SET_DPR_FIELD_LITERALS, ID: 371962158, model: 746.
// Short name: SWE00606
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_SET_DPR_FIELD_LITERALS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block sets the screen literals for the list Distribution Policy 
/// Rule screen.
/// </para>
/// </summary>
[Serializable]
public partial class FnSetDprFieldLiterals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SET_DPR_FIELD_LITERALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSetDprFieldLiterals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSetDprFieldLiterals.
  /// </summary>
  public FnSetDprFieldLiterals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    switch(AsChar(import.DistributionPolicyRule.ApplyTo))
    {
      case 'D':
        export.Apply.TextLine10 = "Debt";

        break;
      case 'I':
        export.Apply.TextLine10 = "Interest";

        break;
      default:
        break;
    }

    switch(AsChar(import.DistributionPolicyRule.DebtState))
    {
      case 'A':
        export.DebtState.TextLine10 = "Arrears";

        break;
      case 'C':
        export.DebtState.TextLine10 = "Current";

        break;
      default:
        break;
    }

    switch(AsChar(import.DistributionPolicyRule.DebtFunctionType))
    {
      case 'A':
        export.Function.Text13 = "Accruing";

        break;
      case 'O':
        export.Function.Text13 = "No Pymt Sched";

        break;
      case 'W':
        export.Function.Text13 = "Pymt Sched";

        break;
      default:
        break;
    }
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
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    private DistributionPolicyRule distributionPolicyRule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Function.
    /// </summary>
    [JsonPropertyName("function")]
    public WorkArea Function
    {
      get => function ??= new();
      set => function = value;
    }

    /// <summary>
    /// A value of DebtState.
    /// </summary>
    [JsonPropertyName("debtState")]
    public ListScreenWorkArea DebtState
    {
      get => debtState ??= new();
      set => debtState = value;
    }

    /// <summary>
    /// A value of Apply.
    /// </summary>
    [JsonPropertyName("apply")]
    public ListScreenWorkArea Apply
    {
      get => apply ??= new();
      set => apply = value;
    }

    private WorkArea function;
    private ListScreenWorkArea debtState;
    private ListScreenWorkArea apply;
  }
#endregion
}
