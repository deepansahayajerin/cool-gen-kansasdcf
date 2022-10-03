// Program: SP_EAB_WRITE_3_YR_MOD_RPT, ID: 1902444899, model: 746.
// Short name: SWEXEW20
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_EAB_WRITE_3_YR_MOD_RPT.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// </para>
/// </summary>
[Serializable]
public partial class SpEabWrite3YrModRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EAB_WRITE_3_YR_MOD_RPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEabWrite3YrModRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEabWrite3YrModRpt.
  /// </summary>
  public SpEabWrite3YrModRpt(IContext context, Import import, Export export):
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
      "SWEXEW20", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 1, Members = new[] { "FileInstruction", "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Import3YrLine.
    /// </summary>
    [JsonPropertyName("import3YrLine")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text166" })]
    public WorkArea Import3YrLine
    {
      get => import3YrLine ??= new();
      set => import3YrLine = value;
    }

    private External external;
    private WorkArea import3YrLine;
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
    [Member(Index = 1, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
