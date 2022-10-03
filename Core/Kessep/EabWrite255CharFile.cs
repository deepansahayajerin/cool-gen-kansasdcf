// Program: EAB_WRITE_255_CHAR_FILE, ID: 1625344816, model: 746.
// Short name: SWEXEW33
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_255_CHAR_FILE.
/// </summary>
[Serializable]
public partial class EabWrite255CharFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_255_CHAR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWrite255CharFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWrite255CharFile.
  /// </summary>
  public EabWrite255CharFile(IContext context, Import import, Export export):
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
      "SWEXEW33", context, import, export, EabOptions.Hpvp);
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
    /// A value of Data255CharacterTextRecord.
    /// </summary>
    [JsonPropertyName("data255CharacterTextRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Data" })]
    public Data255CharacterTextRecord Data255CharacterTextRecord
    {
      get => data255CharacterTextRecord ??= new();
      set => data255CharacterTextRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private Data255CharacterTextRecord data255CharacterTextRecord;
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
