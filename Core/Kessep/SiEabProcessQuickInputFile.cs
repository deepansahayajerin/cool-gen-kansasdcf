// Program: SI_EAB_PROCESS_QUICK_INPUT_FILE, ID: 374552821, model: 746.
// Short name: SWEXQR01
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_EAB_PROCESS_QUICK_INPUT_FILE.
/// </summary>
[Serializable]
public partial class SiEabProcessQuickInputFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_PROCESS_QUICK_INPUT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabProcessQuickInputFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabProcessQuickInputFile.
  /// </summary>
  public SiEabProcessQuickInputFile(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEXQR01", context, import, export, EabOptions.Hpvp);
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

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "CaseId",
      "OsFips",
      "StDate",
      "EndDate"
    })]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private QuickInQuery quickInQuery;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
