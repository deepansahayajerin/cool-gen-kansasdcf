// Program: FN_B798_READ_TEXT_OPT_OUT_FILE, ID: 1902556501, model: 746.
// Short name: SWEXER24
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B798_READ_TEXT_OPT_OUT_FILE.
/// </summary>
[Serializable]
public partial class FnB798ReadTextOptOutFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B798_READ_TEXT_OPT_OUT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB798ReadTextOptOutFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB798ReadTextOptOutFile.
  /// </summary>
  public FnB798ReadTextOptOutFile(IContext context, Import import, Export export)
    :
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
      "SWEXER24", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of FileRecord.
    /// </summary>
    [JsonPropertyName("fileRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text12" })]
    public WorkArea FileRecord
    {
      get => fileRecord ??= new();
      set => fileRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private WorkArea fileRecord;
  }
#endregion
}
