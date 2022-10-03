// Program: EAB_CSENET_FILE_WRITER, ID: 372942363, model: 746.
// Short name: SWEXI700
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CSENET_FILE_WRITER.
/// </summary>
[Serializable]
public partial class EabCsenetFileWriter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CSENET_FILE_WRITER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCsenetFileWriter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCsenetFileWriter.
  /// </summary>
  public EabCsenetFileWriter(IContext context, Import import, Export export):
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
      "SWEXI700", context, import, export, EabOptions.Hpvp);
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
