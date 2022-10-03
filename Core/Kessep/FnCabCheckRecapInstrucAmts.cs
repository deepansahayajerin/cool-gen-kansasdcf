// Program: FN_CAB_CHECK_RECAP_INSTRUC_AMTS, ID: 372128803, model: 746.
// Short name: SWE02315
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_CHECK_RECAP_INSTRUC_AMTS.
/// </para>
/// <para>
/// This CAB is used by RHST.  It looks at the amounts entered for ADC current, 
/// ADC arrears, and passthru. It sets flags if amount is entered with max or
/// percent, or max is entered without percent, or percent is greater than 100.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabCheckRecapInstrucAmts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CHECK_RECAP_INSTRUC_AMTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCheckRecapInstrucAmts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCheckRecapInstrucAmts.
  /// </summary>
  public FnCabCheckRecapInstrucAmts(IContext context, Import import,
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
    // *** Note: exitstate might not be all ok coming in here.
    export.ErrorCount.Count = import.ErrorCount.Count;

    // *** At least one amount must be entered
    if (import.ObligorRule.NonAdcCurrentPercentage.GetValueOrDefault() == 0 && import
      .ObligorRule.NonAdcCurrentAmount.GetValueOrDefault() == 0 && import
      .ObligorRule.NonAdcCurrentMaxAmount.GetValueOrDefault() == 0 && import
      .ObligorRule.NonAdcArrearsAmount.GetValueOrDefault() == 0 && import
      .ObligorRule.NonAdcArrearsMaxAmount.GetValueOrDefault() == 0 && import
      .ObligorRule.NonAdcArrearsPercentage.GetValueOrDefault() == 0 && import
      .ObligorRule.PassthruAmount.GetValueOrDefault() == 0 && import
      .ObligorRule.PassthruMaxAmount.GetValueOrDefault() == 0 && import
      .ObligorRule.PassthruPercentage.GetValueOrDefault() == 0)
    {
      ++export.ErrorCount.Count;
      export.NadcCurrPerErr.Flag = "Y";
      export.NadcCurrAmtErr.Flag = "Y";
      export.NadcArrearsAmtErr.Flag = "Y";
      export.NadcArrearsPerErr.Flag = "Y";
      export.PassthruAmtErr.Flag = "Y";
      export.PassthruPerErr.Flag = "Y";
      ExitState = "FN0000_AT_LEAST_1_AMOUNT_REQD";

      return;
    }

    // *** Non ADC current field edits.
    if (import.ObligorRule.NonAdcCurrentPercentage.GetValueOrDefault() > 0 && import
      .ObligorRule.NonAdcCurrentAmount.GetValueOrDefault() > 0)
    {
      ++export.ErrorCount.Count;
      export.NadcCurrPerErr.Flag = "Y";
      export.NadcCurrAmtErr.Flag = "Y";
      ExitState = "FN0000_EITHER_PERCENTAGE_OR_AMT";
    }

    if (import.ObligorRule.NonAdcCurrentMaxAmount.GetValueOrDefault() > 0)
    {
      if (import.ObligorRule.NonAdcCurrentAmount.GetValueOrDefault() > 0)
      {
        ++export.ErrorCount.Count;
        export.NadcCurrAmtErr.Flag = "Y";
        export.NadcCurrMaxErr.Flag = "Y";
        ExitState = "FN0000_EITHER_MAX_OR_AMOUNT";

        goto Test1;
      }

      if (import.ObligorRule.NonAdcCurrentPercentage.GetValueOrDefault() == 0)
      {
        ++export.ErrorCount.Count;
        export.NadcCurrMaxErr.Flag = "Y";
        ExitState = "FN0000_MAX_WO_PERCENT";
      }
    }

Test1:

    // *** Non ADC arrears field edits.
    if (import.ObligorRule.NonAdcArrearsAmount.GetValueOrDefault() > 0 && import
      .ObligorRule.NonAdcArrearsPercentage.GetValueOrDefault() > 0)
    {
      ++export.ErrorCount.Count;
      export.NadcArrearsAmtErr.Flag = "Y";
      export.NadcArrearsPerErr.Flag = "Y";
      ExitState = "FN0000_EITHER_PERCENTAGE_OR_AMT";
    }

    if (import.ObligorRule.NonAdcArrearsMaxAmount.GetValueOrDefault() > 0)
    {
      if (import.ObligorRule.NonAdcArrearsAmount.GetValueOrDefault() > 0)
      {
        ++export.ErrorCount.Count;
        export.NadcArrearsMaxErr.Flag = "Y";
        export.NadcArrearsAmtErr.Flag = "Y";
        ExitState = "FN0000_EITHER_MAX_OR_AMOUNT";

        goto Test2;
      }

      if (import.ObligorRule.NonAdcArrearsPercentage.GetValueOrDefault() == 0)
      {
        ++export.ErrorCount.Count;
        export.NadcArrearsMaxErr.Flag = "Y";
        ExitState = "FN0000_MAX_WO_PERCENT";
      }
    }

Test2:

    // *** Passthru field edits.
    if (import.ObligorRule.PassthruPercentage.GetValueOrDefault() > 0 && import
      .ObligorRule.PassthruAmount.GetValueOrDefault() > 0)
    {
      ++export.ErrorCount.Count;
      export.PassthruAmtErr.Flag = "Y";
      export.PassthruPerErr.Flag = "Y";
      ExitState = "FN0000_EITHER_PERCENTAGE_OR_AMT";
    }

    if (import.ObligorRule.PassthruMaxAmount.GetValueOrDefault() > 0)
    {
      if (import.ObligorRule.PassthruAmount.GetValueOrDefault() > 0)
      {
        ++export.ErrorCount.Count;
        export.PassthruMaxErr.Flag = "Y";
        export.PassthruAmtErr.Flag = "Y";
        ExitState = "FN0000_EITHER_MAX_OR_AMOUNT";

        goto Test3;
      }

      if (import.ObligorRule.PassthruPercentage.GetValueOrDefault() == 0)
      {
        ++export.ErrorCount.Count;
        export.PassthruMaxErr.Flag = "Y";
        ExitState = "FN0000_MAX_WO_PERCENT";
      }
    }

Test3:

    // *** Percentages cannot be greater than 100.
    if (import.ObligorRule.NonAdcArrearsPercentage.GetValueOrDefault() > 100)
    {
      ++export.ErrorCount.Count;
      export.NadcArrearsPerErr.Flag = "Y";
      ExitState = "FN0000_PCT_GREATER_THAN_100";
    }

    if (import.ObligorRule.NonAdcCurrentPercentage.GetValueOrDefault() > 100)
    {
      ++export.ErrorCount.Count;
      export.NadcCurrPerErr.Flag = "Y";
      ExitState = "FN0000_PCT_GREATER_THAN_100";
    }

    if (import.ObligorRule.PassthruPercentage.GetValueOrDefault() > 100)
    {
      ++export.ErrorCount.Count;
      export.PassthruPerErr.Flag = "Y";
      ExitState = "FN0000_PCT_GREATER_THAN_100";
    }

    if (export.ErrorCount.Count > 1)
    {
      ExitState = "FN0000_MULTIPLE_ERRORS_FOUND";
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
    /// A value of ErrorCount.
    /// </summary>
    [JsonPropertyName("errorCount")]
    public Common ErrorCount
    {
      get => errorCount ??= new();
      set => errorCount = value;
    }

    /// <summary>
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
    }

    private Common errorCount;
    private RecaptureRule obligorRule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ErrorCount.
    /// </summary>
    [JsonPropertyName("errorCount")]
    public Common ErrorCount
    {
      get => errorCount ??= new();
      set => errorCount = value;
    }

    /// <summary>
    /// A value of PassthruAmtErr.
    /// </summary>
    [JsonPropertyName("passthruAmtErr")]
    public Common PassthruAmtErr
    {
      get => passthruAmtErr ??= new();
      set => passthruAmtErr = value;
    }

    /// <summary>
    /// A value of PassthruPerErr.
    /// </summary>
    [JsonPropertyName("passthruPerErr")]
    public Common PassthruPerErr
    {
      get => passthruPerErr ??= new();
      set => passthruPerErr = value;
    }

    /// <summary>
    /// A value of PassthruMaxErr.
    /// </summary>
    [JsonPropertyName("passthruMaxErr")]
    public Common PassthruMaxErr
    {
      get => passthruMaxErr ??= new();
      set => passthruMaxErr = value;
    }

    /// <summary>
    /// A value of NadcCurrAmtErr.
    /// </summary>
    [JsonPropertyName("nadcCurrAmtErr")]
    public Common NadcCurrAmtErr
    {
      get => nadcCurrAmtErr ??= new();
      set => nadcCurrAmtErr = value;
    }

    /// <summary>
    /// A value of NadcCurrPerErr.
    /// </summary>
    [JsonPropertyName("nadcCurrPerErr")]
    public Common NadcCurrPerErr
    {
      get => nadcCurrPerErr ??= new();
      set => nadcCurrPerErr = value;
    }

    /// <summary>
    /// A value of NadcCurrMaxErr.
    /// </summary>
    [JsonPropertyName("nadcCurrMaxErr")]
    public Common NadcCurrMaxErr
    {
      get => nadcCurrMaxErr ??= new();
      set => nadcCurrMaxErr = value;
    }

    /// <summary>
    /// A value of NadcArrearsAmtErr.
    /// </summary>
    [JsonPropertyName("nadcArrearsAmtErr")]
    public Common NadcArrearsAmtErr
    {
      get => nadcArrearsAmtErr ??= new();
      set => nadcArrearsAmtErr = value;
    }

    /// <summary>
    /// A value of NadcArrearsPerErr.
    /// </summary>
    [JsonPropertyName("nadcArrearsPerErr")]
    public Common NadcArrearsPerErr
    {
      get => nadcArrearsPerErr ??= new();
      set => nadcArrearsPerErr = value;
    }

    /// <summary>
    /// A value of NadcArrearsMaxErr.
    /// </summary>
    [JsonPropertyName("nadcArrearsMaxErr")]
    public Common NadcArrearsMaxErr
    {
      get => nadcArrearsMaxErr ??= new();
      set => nadcArrearsMaxErr = value;
    }

    private Common errorCount;
    private Common passthruAmtErr;
    private Common passthruPerErr;
    private Common passthruMaxErr;
    private Common nadcCurrAmtErr;
    private Common nadcCurrPerErr;
    private Common nadcCurrMaxErr;
    private Common nadcArrearsAmtErr;
    private Common nadcArrearsPerErr;
    private Common nadcArrearsMaxErr;
  }
#endregion
}
