// Program: FN_SWEX0003_READ_REPORT_FILE, ID: 371130050, model: 746.
// Short name: SWEX0003
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_SWEX0003_READ_REPORT_FILE.
/// </para>
/// <para>
/// This cab reads a flat file containing data for the REPORT_DATA table.
/// </para>
/// </summary>
[Serializable]
public partial class FnSwex0003ReadReportFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SWEX0003_READ_REPORT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSwex0003ReadReportFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSwex0003ReadReportFile.
  /// </summary>
  public FnSwex0003ReadReportFile(IContext context, Import import, Export export)
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
      "SWEX0003", context, import, export, EabOptions.Hpvp);
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
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Type1",
      "SequenceNumber",
      "FirstPageOnlyInd",
      "LineControl",
      "LineText"
    })]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
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

    private ReportData reportData;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
