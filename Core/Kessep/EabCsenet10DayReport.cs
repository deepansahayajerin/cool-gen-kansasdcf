// Program: EAB_CSENET_10_DAY_REPORT, ID: 372942384, model: 746.
// Short name: SWEXJ710
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CSENET_10_DAY_REPORT.
/// </summary>
[Serializable]
public partial class EabCsenet10DayReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CSENET_10_DAY_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCsenet10DayReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCsenet10DayReport.
  /// </summary>
  public EabCsenet10DayReport(IContext context, Import import, Export export):
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
      "SWEXJ710", context, import, export, EabOptions.Hpvp);
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
    /// A value of Csenet10DayExtract.
    /// </summary>
    [JsonPropertyName("csenet10DayExtract")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Office",
      "ServiceProviderName",
      "ReferralNumber",
      "StartDate",
      "NonComplianceDate"
    })]
    public Csenet10DayExtract2 Csenet10DayExtract
    {
      get => csenet10DayExtract ??= new();
      set => csenet10DayExtract = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private Csenet10DayExtract2 csenet10DayExtract;
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
