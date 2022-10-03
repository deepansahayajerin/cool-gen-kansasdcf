// Program: FN_B796_WRITE_TO_TEXT_MSG_FILE, ID: 1902526454, model: 746.
// Short name: SWEXEW26
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B796_WRITE_TO_TEXT_MSG_FILE.
/// </summary>
[Serializable]
public partial class FnB796WriteToTextMsgFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B796_WRITE_TO_TEXT_MSG_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB796WriteToTextMsgFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB796WriteToTextMsgFile.
  /// </summary>
  public FnB796WriteToTextMsgFile(IContext context, Import import, Export export)
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
      "SWEXEW26", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text200" })]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of RecordLength.
    /// </summary>
    [JsonPropertyName("recordLength")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
    public Common RecordLength
    {
      get => recordLength ??= new();
      set => recordLength = value;
    }

    private EabFileHandling eabFileHandling;
    private WorkArea workArea;
    private Common recordLength;
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
