// Program: EAB_RPT_MONTHLY_DISBURSEMENT, ID: 372816578, model: 746.
// Short name: SWEXF223
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_RPT_MONTHLY_DISBURSEMENT.
/// </para>
/// <para>
/// External action block for the monthly report of disbursements by 
/// disbursement type.
/// </para>
/// </summary>
[Serializable]
public partial class EabRptMonthlyDisbursement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_RPT_MONTHLY_DISBURSEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabRptMonthlyDisbursement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabRptMonthlyDisbursement.
  /// </summary>
  public EabRptMonthlyDisbursement(IContext context, Import import,
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
      "SWEXF223", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of RunDate.
    /// </summary>
    [JsonPropertyName("runDate")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea RunDate
    {
      get => runDate ??= new();
      set => runDate = value;
    }

    /// <summary>
    /// A value of BegOfRpt.
    /// </summary>
    [JsonPropertyName("begOfRpt")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea BegOfRpt
    {
      get => begOfRpt ??= new();
      set => begOfRpt = value;
    }

    /// <summary>
    /// A value of EndOfRpt.
    /// </summary>
    [JsonPropertyName("endOfRpt")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea EndOfRpt
    {
      get => endOfRpt ??= new();
      set => endOfRpt = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Code", "Name" })]
      
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of MonthlyDisb.
    /// </summary>
    [JsonPropertyName("monthlyDisb")]
    [Member(Index = 6, AccessFields = false, Members
      = new[] { "Count", "TotalCurrency" })]
    public Common MonthlyDisb
    {
      get => monthlyDisb ??= new();
      set => monthlyDisb = value;
    }

    private ReportParms reportParms;
    private DateWorkArea runDate;
    private DateWorkArea begOfRpt;
    private DateWorkArea endOfRpt;
    private DisbursementType disbursementType;
    private Common monthlyDisb;
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
