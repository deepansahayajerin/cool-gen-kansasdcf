// Program: EAB_FOUR_REC_DRIVER, ID: 373024133, model: 746.
// Short name: SWEXPP01
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_FOUR_REC_DRIVER.
/// </summary>
[Serializable]
public partial class EabFourRecDriver: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FOUR_REC_DRIVER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFourRecDriver(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFourRecDriver.
  /// </summary>
  public EabFourRecDriver(IContext context, Import import, Export export):
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
      "SWEXPP01", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "TextReturnCode", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "InterfaceTransId" })]
    public CashReceiptDetail From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "InterfaceTransId" })]
    public CashReceiptDetail To
    {
      get => to ??= new();
      set => to = value;
    }

    private External external;
    private CashReceiptDetail from;
    private CashReceiptDetail to;
  }
#endregion
}
