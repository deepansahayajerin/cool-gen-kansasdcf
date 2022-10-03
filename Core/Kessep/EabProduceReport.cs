// Program: EAB_PRODUCE_REPORT, ID: 372819894, model: 746.
// Short name: SWEXF750
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_PRODUCE_REPORT.
/// </summary>
[Serializable]
public partial class EabProduceReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_PRODUCE_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabProduceReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabProduceReport.
  /// </summary>
  public EabProduceReport(IContext context, Import import, Export export):
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
      "SWEXF750", context, import, export, EabOptions.Hpvp);
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
    /// A value of CollectionsExtract.
    /// </summary>
    [JsonPropertyName("collectionsExtract")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "CollectionOfficer" })]
    public CollectionsExtract CollectionsExtract
    {
      get => collectionsExtract ??= new();
      set => collectionsExtract = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of ReportLiterals.
    /// </summary>
    [JsonPropertyName("reportLiterals")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "SubHeading1",
      "SubHeading2",
      "SubHeading3"
    })]
    public ReportLiterals ReportLiterals
    {
      get => reportLiterals ??= new();
      set => reportLiterals = value;
    }

    /// <summary>
    /// A value of TotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("totalCollectionsExtract")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract TotalCollectionsExtract
    {
      get => totalCollectionsExtract ??= new();
      set => totalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TotalCommon.
    /// </summary>
    [JsonPropertyName("totalCommon")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Count" })]
    public Common TotalCommon
    {
      get => totalCommon ??= new();
      set => totalCommon = value;
    }

    /// <summary>
    /// A value of TafTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafTotalCollectionsExtract")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract TafTotalCollectionsExtract
    {
      get => tafTotalCollectionsExtract ??= new();
      set => tafTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafTotalCommon.
    /// </summary>
    [JsonPropertyName("tafTotalCommon")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Count" })]
    public Common TafTotalCommon
    {
      get => tafTotalCommon ??= new();
      set => tafTotalCommon = value;
    }

    /// <summary>
    /// A value of TafCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafCollectionsExtract")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract TafCollectionsExtract
    {
      get => tafCollectionsExtract ??= new();
      set => tafCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafCommon.
    /// </summary>
    [JsonPropertyName("tafCommon")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Count" })]
    public Common TafCommon
    {
      get => tafCommon ??= new();
      set => tafCommon = value;
    }

    /// <summary>
    /// A value of XtafCollectionsExtract.
    /// </summary>
    [JsonPropertyName("xtafCollectionsExtract")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract XtafCollectionsExtract
    {
      get => xtafCollectionsExtract ??= new();
      set => xtafCollectionsExtract = value;
    }

    /// <summary>
    /// A value of XtafCommon.
    /// </summary>
    [JsonPropertyName("xtafCommon")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Count" })]
    public Common XtafCommon
    {
      get => xtafCommon ??= new();
      set => xtafCommon = value;
    }

    /// <summary>
    /// A value of TafFcCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafFcCollectionsExtract")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract TafFcCollectionsExtract
    {
      get => tafFcCollectionsExtract ??= new();
      set => tafFcCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafFcCommon.
    /// </summary>
    [JsonPropertyName("tafFcCommon")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Count" })]
    public Common TafFcCommon
    {
      get => tafFcCommon ??= new();
      set => tafFcCommon = value;
    }

    /// <summary>
    /// A value of NonTafTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("nonTafTotalCollectionsExtract")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract NonTafTotalCollectionsExtract
    {
      get => nonTafTotalCollectionsExtract ??= new();
      set => nonTafTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of NonTafTotalCommon.
    /// </summary>
    [JsonPropertyName("nonTafTotalCommon")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Count" })]
    public Common NonTafTotalCommon
    {
      get => nonTafTotalCommon ??= new();
      set => nonTafTotalCommon = value;
    }

    /// <summary>
    /// A value of NaCollectionsExtract.
    /// </summary>
    [JsonPropertyName("naCollectionsExtract")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract NaCollectionsExtract
    {
      get => naCollectionsExtract ??= new();
      set => naCollectionsExtract = value;
    }

    /// <summary>
    /// A value of NaCommon.
    /// </summary>
    [JsonPropertyName("naCommon")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Count" })]
    public Common NaCommon
    {
      get => naCommon ??= new();
      set => naCommon = value;
    }

    /// <summary>
    /// A value of PaCollectionsExtract.
    /// </summary>
    [JsonPropertyName("paCollectionsExtract")]
    [Member(Index = 18, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract PaCollectionsExtract
    {
      get => paCollectionsExtract ??= new();
      set => paCollectionsExtract = value;
    }

    /// <summary>
    /// A value of PaCommon.
    /// </summary>
    [JsonPropertyName("paCommon")]
    [Member(Index = 19, AccessFields = false, Members = new[] { "Count" })]
    public Common PaCommon
    {
      get => paCommon ??= new();
      set => paCommon = value;
    }

    /// <summary>
    /// A value of StateOnlyTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("stateOnlyTotalCollectionsExtract")]
    [Member(Index = 20, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract StateOnlyTotalCollectionsExtract
    {
      get => stateOnlyTotalCollectionsExtract ??= new();
      set => stateOnlyTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of StateOnlyTotalCommon.
    /// </summary>
    [JsonPropertyName("stateOnlyTotalCommon")]
    [Member(Index = 21, AccessFields = false, Members = new[] { "Count" })]
    public Common StateOnlyTotalCommon
    {
      get => stateOnlyTotalCommon ??= new();
      set => stateOnlyTotalCommon = value;
    }

    /// <summary>
    /// A value of GaFcCollectionsExtract.
    /// </summary>
    [JsonPropertyName("gaFcCollectionsExtract")]
    [Member(Index = 22, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract GaFcCollectionsExtract
    {
      get => gaFcCollectionsExtract ??= new();
      set => gaFcCollectionsExtract = value;
    }

    /// <summary>
    /// A value of GaFcCommon.
    /// </summary>
    [JsonPropertyName("gaFcCommon")]
    [Member(Index = 23, AccessFields = false, Members = new[] { "Count" })]
    public Common GaFcCommon
    {
      get => gaFcCommon ??= new();
      set => gaFcCommon = value;
    }

    /// <summary>
    /// A value of MhddCollectionsExtract.
    /// </summary>
    [JsonPropertyName("mhddCollectionsExtract")]
    [Member(Index = 24, AccessFields = false, Members = new[] { "Amount1" })]
    public CollectionsExtract MhddCollectionsExtract
    {
      get => mhddCollectionsExtract ??= new();
      set => mhddCollectionsExtract = value;
    }

    /// <summary>
    /// A value of MhddCommon.
    /// </summary>
    [JsonPropertyName("mhddCommon")]
    [Member(Index = 25, AccessFields = false, Members = new[] { "Count" })]
    public Common MhddCommon
    {
      get => mhddCommon ??= new();
      set => mhddCommon = value;
    }

    private CollectionsExtract collectionsExtract;
    private ReportParms reportParms;
    private ReportLiterals reportLiterals;
    private CollectionsExtract totalCollectionsExtract;
    private Common totalCommon;
    private CollectionsExtract tafTotalCollectionsExtract;
    private Common tafTotalCommon;
    private CollectionsExtract tafCollectionsExtract;
    private Common tafCommon;
    private CollectionsExtract xtafCollectionsExtract;
    private Common xtafCommon;
    private CollectionsExtract tafFcCollectionsExtract;
    private Common tafFcCommon;
    private CollectionsExtract nonTafTotalCollectionsExtract;
    private Common nonTafTotalCommon;
    private CollectionsExtract naCollectionsExtract;
    private Common naCommon;
    private CollectionsExtract paCollectionsExtract;
    private Common paCommon;
    private CollectionsExtract stateOnlyTotalCollectionsExtract;
    private Common stateOnlyTotalCommon;
    private CollectionsExtract gaFcCollectionsExtract;
    private Common gaFcCommon;
    private CollectionsExtract mhddCollectionsExtract;
    private Common mhddCommon;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
