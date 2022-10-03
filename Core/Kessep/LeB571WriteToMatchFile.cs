// Program: LE_B571_WRITE_TO_MATCH_FILE, ID: 1902529509, model: 746.
// Short name: SWEXEW28
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B571_WRITE_TO_MATCH_FILE.
/// </summary>
[Serializable]
public partial class LeB571WriteToMatchFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B571_WRITE_TO_MATCH_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB571WriteToMatchFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB571WriteToMatchFile.
  /// </summary>
  public LeB571WriteToMatchFile(IContext context, Import import, Export export):
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
      "SWEXEW28", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text80" })]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    private EabFileHandling eabFileHandling;
    private WorkArea workArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
