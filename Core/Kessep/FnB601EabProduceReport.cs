// Program: FN_B601_EAB_PRODUCE_REPORT, ID: 372706093, model: 746.
// Short name: SWEXF237
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B601_EAB_PRODUCE_REPORT.
/// </para>
/// <para>
/// This eab uses report composer to generate the Recovery Obligation Collection
/// report.
/// </para>
/// </summary>
[Serializable]
public partial class FnB601EabProduceReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B601_EAB_PRODUCE_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB601EabProduceReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB601EabProduceReport.
  /// </summary>
  public FnB601EabProduceReport(IContext context, Import import, Export export):
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
      "SWEXF237", context, import, export, EabOptions.Hpvp);
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
    /// A value of HighDate.
    /// </summary>
    [JsonPropertyName("highDate")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea HighDate
    {
      get => highDate ??= new();
      set => highDate = value;
    }

    /// <summary>
    /// A value of LowDate.
    /// </summary>
    [JsonPropertyName("lowDate")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea LowDate
    {
      get => lowDate ??= new();
      set => lowDate = value;
    }

    /// <summary>
    /// A value of ReportLiteral.
    /// </summary>
    [JsonPropertyName("reportLiteral")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea ReportLiteral
    {
      get => reportLiteral ??= new();
      set => reportLiteral = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 4, AccessFields = false, Members
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
    [Member(Index = 5, AccessFields = false, Members = new[] { "TotalCurrency" })
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
    [Member(Index = 6, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common BalOwed
    {
      get => balOwed ??= new();
      set => balOwed = value;
    }

    /// <summary>
    /// A value of IntOwed.
    /// </summary>
    [JsonPropertyName("intOwed")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "TotalCurrency" })
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
    [Member(Index = 8, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common TotOsBal
    {
      get => totOsBal ??= new();
      set => totOsBal = value;
    }

    /// <summary>
    /// A value of Prcnt.
    /// </summary>
    [JsonPropertyName("prcnt")]
    [Member(Index = 9, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common Prcnt
    {
      get => prcnt ??= new();
      set => prcnt = value;
    }

    /// <summary>
    /// A value of PriorRecoveryDue.
    /// </summary>
    [JsonPropertyName("priorRecoveryDue")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common PriorRecoveryDue
    {
      get => priorRecoveryDue ??= new();
      set => priorRecoveryDue = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Code" })]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of NewDebts.
    /// </summary>
    [JsonPropertyName("newDebts")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "Count", "TotalCurrency" })]
    public Common NewDebts
    {
      get => newDebts ??= new();
      set => newDebts = value;
    }

    private DateWorkArea highDate;
    private DateWorkArea lowDate;
    private TextWorkArea reportLiteral;
    private ReportParms reportParms;
    private Common totalColl;
    private Common balOwed;
    private Common intOwed;
    private Common totOsBal;
    private Common prcnt;
    private Common priorRecoveryDue;
    private ObligationType obligationType;
    private Common newDebts;
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
