// Program: EAB_EXTERNAL_REPORT_WRITER_SMALL, ID: 1902581635, model: 746.
// Short name: SWEXEW31
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_EXTERNAL_REPORT_WRITER_SMALL.
/// </summary>
[Serializable]
public partial class EabExternalReportWriterSmall: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_EXTERNAL_REPORT_WRITER_SMALL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabExternalReportWriterSmall(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabExternalReportWriterSmall.
  /// </summary>
  public EabExternalReportWriterSmall(IContext context, Import import,
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
      "SWEXEW31", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "TextLine80" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private EabFileHandling eabFileHandling;
    private External external;
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
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "NumericReturnCode", "TextReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
