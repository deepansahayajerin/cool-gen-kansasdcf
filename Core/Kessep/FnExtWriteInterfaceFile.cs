// Program: FN_EXT_WRITE_INTERFACE_FILE, ID: 374394812, model: 746.
// Short name: SWEXF201
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EXT_WRITE_INTERFACE_FILE.
/// </summary>
[Serializable]
public partial class FnExtWriteInterfaceFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EXT_WRITE_INTERFACE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnExtWriteInterfaceFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnExtWriteInterfaceFile.
  /// </summary>
  public FnExtWriteInterfaceFile(IContext context, Import import, Export export):
    
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
      "SWEXF201", context, import, export, EabOptions.Hpvp);
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
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    /// <summary>
    /// A value of FileCount.
    /// </summary>
    [JsonPropertyName("fileCount")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Count" })]
    public Common FileCount
    {
      get => fileCount ??= new();
      set => fileCount = value;
    }

    /// <summary>
    /// A value of PrintFileRecord.
    /// </summary>
    [JsonPropertyName("printFileRecord")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "CourtOrderLine" })]
    public PrintFileRecord PrintFileRecord
    {
      get => printFileRecord ??= new();
      set => printFileRecord = value;
    }

    private KpcExternalParms kpcExternalParms;
    private Common fileCount;
    private PrintFileRecord printFileRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    private KpcExternalParms kpcExternalParms;
  }
#endregion
}
