// Program: EAB_WRITE_REPORT, ID: 371787514, model: 746.
// Short name: SWEXGW98
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_REPORT.
/// </summary>
[Serializable]
public partial class EabWriteReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteReport.
  /// </summary>
  public EabWriteReport(IContext context, Import import, Export export):
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
      "SWEXGW98", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "ReportNumber",
      "Command",
      "BlankLineAfterHeading",
      "BlankLineAfterColHead",
      "BlankLineBeforeFooters",
      "NumberOfColHeadings",
      "NumberOfFooters",
      "OverridePageNo",
      "OverrideLinesPerPage",
      "OverrideCarriageControl",
      "ProcessDate",
      "ProgramName",
      "ReportNoPart2",
      "RunDate",
      "RunTime",
      "RptHeading1",
      "RptHeading2",
      "RptHeading3",
      "ColHeading1",
      "ColHeading2",
      "ColHeading3",
      "RptDetail",
      "RptFooter1",
      "RptFooter2",
      "RptFooter3"
    })]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabReportReturn.
    /// </summary>
    [JsonPropertyName("eabReportReturn")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "PageNumber",
      "LineNumber",
      "LinesRemaining"
    })]
    public EabReportReturn EabReportReturn
    {
      get => eabReportReturn ??= new();
      set => eabReportReturn = value;
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

    private EabReportReturn eabReportReturn;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
