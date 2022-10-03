// Program: EAB_CONVERT_NUMERIC, ID: 372132420, model: 746.
// Short name: SWEXGW96
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CONVERT_NUMERIC.
/// </summary>
[Serializable]
public partial class EabConvertNumeric1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CONVERT_NUMERIC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabConvertNumeric1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabConvertNumeric1.
  /// </summary>
  public EabConvertNumeric1(IContext context, Import import, Export export):
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
      "SWEXGW96", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "SendSign",
      "SendAmount",
      "SendNonSuppressPos"
    })]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    private EabConvertNumeric2 eabConvertNumeric;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "ReturnCurrencySigned",
      "ReturnCurrencyNegInParens",
      "ReturnAmountNonDecimalSigned",
      "ReturnNoCommasInNonDecimal",
      "ReturnOkFlag"
    })]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    private EabConvertNumeric2 eabConvertNumeric;
  }
#endregion
}
