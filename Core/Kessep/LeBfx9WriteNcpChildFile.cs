// Program: LE_BFX9_WRITE_NCP_CHILD_FILE, ID: 1902448795, model: 746.
// Short name: SWEXLE20
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX9_WRITE_NCP_CHILD_FILE.
/// </summary>
[Serializable]
public partial class LeBfx9WriteNcpChildFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX9_WRITE_NCP_CHILD_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx9WriteNcpChildFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx9WriteNcpChildFile.
  /// </summary>
  public LeBfx9WriteNcpChildFile(IContext context, Import import, Export export):
    
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
      "SWEXLE20", context, import, export, EabOptions.Hpvp);
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
    /// A value of CssiWorkset.
    /// </summary>
    [JsonPropertyName("cssiWorkset")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "NcpChildFileLayout" })]
    public CssiWorkset CssiWorkset
    {
      get => cssiWorkset ??= new();
      set => cssiWorkset = value;
    }

    private EabFileHandling eabFileHandling;
    private CssiWorkset cssiWorkset;
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
