// Program: EAB_EXTERNAL_REPORT_WRITER_LARGE, ID: 1625374354, model: 746.
// Short name: SWEXEW34
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_EXTERNAL_REPORT_WRITER_LARGE.
/// </summary>
[Serializable]
public partial class EabExternalReportWriterLarge: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_EXTERNAL_REPORT_WRITER_LARGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabExternalReportWriterLarge(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabExternalReportWriterLarge.
  /// </summary>
  public EabExternalReportWriterLarge(IContext context, Import import,
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
      "SWEXEW34", context, import, export, EabOptions.Hpvp);
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
    /// A value of RtpDetail.
    /// </summary>
    [JsonPropertyName("rtpDetail")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text200" })]
    public WorkArea RtpDetail
    {
      get => rtpDetail ??= new();
      set => rtpDetail = value;
    }

    private EabFileHandling eabFileHandling;
    private WorkArea rtpDetail;
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
