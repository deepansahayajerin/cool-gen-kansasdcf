// Program: FN_B602_EAB_PRODUCE_REPORT, ID: 372706126, model: 746.
// Short name: SWEXF238
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B602_EAB_PRODUCE_REPORT.
/// </para>
/// <para>
/// This eab uses report composer to generate the Recovery Obligation Collection
/// report.
/// </para>
/// </summary>
[Serializable]
public partial class FnB602EabProduceReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B602_EAB_PRODUCE_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB602EabProduceReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB602EabProduceReport.
  /// </summary>
  public FnB602EabProduceReport(IContext context, Import import, Export export):
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
      "SWEXF238", context, import, export, EabOptions.Hpvp);
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Code" })]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "UserId",
      "LastName",
      "FirstName"
    })]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of HighDate.
    /// </summary>
    [JsonPropertyName("highDate")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea HighDate
    {
      get => highDate ??= new();
      set => highDate = value;
    }

    /// <summary>
    /// A value of LowDate.
    /// </summary>
    [JsonPropertyName("lowDate")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea LowDate
    {
      get => lowDate ??= new();
      set => lowDate = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 5, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of TotalColl.
    /// </summary>
    [JsonPropertyName("totalColl")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common TotalColl
    {
      get => totalColl ??= new();
      set => totalColl = value;
    }

    /// <summary>
    /// A value of BalOwed.
    /// </summary>
    [JsonPropertyName("balOwed")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common BalOwed
    {
      get => balOwed ??= new();
      set => balOwed = value;
    }

    /// <summary>
    /// A value of PriorBalOwed.
    /// </summary>
    [JsonPropertyName("priorBalOwed")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common PriorBalOwed
    {
      get => priorBalOwed ??= new();
      set => priorBalOwed = value;
    }

    /// <summary>
    /// A value of IntOwed.
    /// </summary>
    [JsonPropertyName("intOwed")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common IntOwed
    {
      get => intOwed ??= new();
      set => intOwed = value;
    }

    /// <summary>
    /// A value of TotOsBal.
    /// </summary>
    [JsonPropertyName("totOsBal")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common TotOsBal
    {
      get => totOsBal ??= new();
      set => totOsBal = value;
    }

    /// <summary>
    /// A value of PrcntCollected.
    /// </summary>
    [JsonPropertyName("prcntCollected")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common PrcntCollected
    {
      get => prcntCollected ??= new();
      set => prcntCollected = value;
    }

    /// <summary>
    /// A value of PrcntCourtOrd.
    /// </summary>
    [JsonPropertyName("prcntCourtOrd")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common PrcntCourtOrd
    {
      get => prcntCourtOrd ??= new();
      set => prcntCourtOrd = value;
    }

    /// <summary>
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Count" })]
    public Common CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Count" })]
    public Common Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private ObligationType obligationType;
    private ServiceProvider serviceProvider;
    private DateWorkArea highDate;
    private DateWorkArea lowDate;
    private ReportParms reportParms;
    private Common totalColl;
    private Common balOwed;
    private Common priorBalOwed;
    private Common intOwed;
    private Common totOsBal;
    private Common prcntCollected;
    private Common prcntCourtOrd;
    private Common courtOrder;
    private Common obligation;
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
