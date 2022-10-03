// Program: LE_BFX9_READ_NCP_ADDRESS_FILE, ID: 1902452632, model: 746.
// Short name: SWEXLE18
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX9_READ_NCP_ADDRESS_FILE.
/// </summary>
[Serializable]
public partial class LeBfx9ReadNcpAddressFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX9_READ_NCP_ADDRESS_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx9ReadNcpAddressFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx9ReadNcpAddressFile.
  /// </summary>
  public LeBfx9ReadNcpAddressFile(IContext context, Import import, Export export)
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
      "SWEXLE18", context, import, export, EabOptions.Hpvp);
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
    /// A value of CssiWorkset.
    /// </summary>
    [JsonPropertyName("cssiWorkset")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "NcpAddressFileLayout" })]
    public CssiWorkset CssiWorkset
    {
      get => cssiWorkset ??= new();
      set => cssiWorkset = value;
    }

    private EabFileHandling eabFileHandling;
    private CssiWorkset cssiWorkset;
  }
#endregion
}
