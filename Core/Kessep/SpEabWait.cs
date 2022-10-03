// Program: SP_EAB_WAIT, ID: 372067477, model: 746.
// Short name: SWEXPE25
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_EAB_WAIT.
/// </para>
/// <para>
/// This external CAB will generate wait states while the infrastructure queue 
/// fills up.
/// </para>
/// </summary>
[Serializable]
public partial class SpEabWait: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EAB_WAIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEabWait(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEabWait.
  /// </summary>
  public SpEabWait(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXPE25", context, import, export, 0);
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
    /// A value of WaitSeconds.
    /// </summary>
    [JsonPropertyName("waitSeconds")]
    [Member(Index = 1, Members = new[] { "Text10" })]
    public TextWorkArea WaitSeconds
    {
      get => waitSeconds ??= new();
      set => waitSeconds = value;
    }

    private TextWorkArea waitSeconds;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }
#endregion
}
