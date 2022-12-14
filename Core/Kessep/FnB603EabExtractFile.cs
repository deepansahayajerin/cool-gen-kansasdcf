// Program: FN_B603_EAB_EXTRACT_FILE, ID: 945058077, model: 746.
// Short name: SWEXFE37
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B603_EAB_EXTRACT_FILE.
/// </summary>
[Serializable]
public partial class FnB603EabExtractFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B603_EAB_EXTRACT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB603EabExtractFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB603EabExtractFile.
  /// </summary>
  public FnB603EabExtractFile(IContext context, Import import, Export export):
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
      "SWEXFE37", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "RptDetail" })]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabReportSend eabReportSend;
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

    private EabFileHandling eabFileHandling;
  }
#endregion
}
