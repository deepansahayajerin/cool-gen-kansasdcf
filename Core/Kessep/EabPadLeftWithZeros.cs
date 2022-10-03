// Program: EAB_PAD_LEFT_WITH_ZEROS, ID: 371456880, model: 746.
// Short name: SWEXGW03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_PAD_LEFT_WITH_ZEROS.
/// </summary>
[Serializable]
public partial class EabPadLeftWithZeros: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_PAD_LEFT_WITH_ZEROS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabPadLeftWithZeros(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabPadLeftWithZeros.
  /// </summary>
  public EabPadLeftWithZeros(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

  #region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    #region MANUAL CODE by ADVANCED, REVIEWED by ... at ...
    // Original code begin    
    // GetService<IEabStub>().Execute("SWEXGW03", context, import, export, 0);
    // Original code end

    // Manually changed code begin
    if (Functions.IsEmpty(import.TextWorkArea.Text10))
    {
      export.TextWorkArea.Text10 = "";
    }
    else
    {
      export.TextWorkArea.Text10 = Functions.NumberToString(Functions.StringToNumber(import.TextWorkArea.Text10), 10);
    }
    // Manually changed code end
    #endregion
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    [Member(Index = 1, Members = new[] { "Text10" })]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    [Member(Index = 1, Members = new[] { "Text10" })]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private TextWorkArea textWorkArea;
  }
#endregion
}
