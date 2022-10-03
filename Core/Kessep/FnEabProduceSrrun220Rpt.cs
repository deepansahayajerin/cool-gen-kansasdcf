// Program: FN_EAB_PRODUCE_SRRUN220_RPT, ID: 372800960, model: 746.
// Short name: SWEXF222
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_EAB_PRODUCE_SRRUN220_RPT.
/// </para>
/// <para>
/// This eab uses report composer to generate a detail line on the Statement and
/// Coupon Suppression report.
/// </para>
/// </summary>
[Serializable]
public partial class FnEabProduceSrrun220Rpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EAB_PRODUCE_SRRUN220_RPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEabProduceSrrun220Rpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEabProduceSrrun220Rpt.
  /// </summary>
  public FnEabProduceSrrun220Rpt(IContext context, Import import, Export export):
    
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
      "SWEXF222", context, import, export, EabOptions.Hpvp);
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
    /// A value of ReportMonth.
    /// </summary>
    [JsonPropertyName("reportMonth")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea ReportMonth
    {
      get => reportMonth ??= new();
      set => reportMonth = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial",
      "UserId"
    })]
    public ServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "UserId",
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 5, AccessFields = false, Members
      = new[] { "Number", "FormattedName" })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SuppressType.
    /// </summary>
    [JsonPropertyName("suppressType")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea SuppressType
    {
      get => suppressType ??= new();
      set => suppressType = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "StandardNumber" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    [Member(Index = 8, AccessFields = false, Members
      = new[] { "EffectiveDate", "CreatedBy" })]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    /// <summary>
    /// A value of SuppressReason.
    /// </summary>
    [JsonPropertyName("suppressReason")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea SuppressReason
    {
      get => suppressReason ??= new();
      set => suppressReason = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private TextWorkArea reportMonth;
    private Office office;
    private ServiceProvider supervisor;
    private ServiceProvider serviceProvider;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea suppressType;
    private LegalAction legalAction;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private TextWorkArea suppressReason;
    private ReportParms reportParms;
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
