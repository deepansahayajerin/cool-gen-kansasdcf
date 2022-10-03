// Program: EAB_CSENET_30_DAY_FILE_READER, ID: 372942403, model: 746.
// Short name: SWEXI730
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CSENET_30_DAY_FILE_READER.
/// </summary>
[Serializable]
public partial class EabCsenet30DayFileReader: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CSENET_30_DAY_FILE_READER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCsenet30DayFileReader(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCsenet30DayFileReader.
  /// </summary>
  public EabCsenet30DayFileReader(IContext context, Import import, Export export)
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
      "SWEXI730", context, import, export, EabOptions.Hpvp);
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

    private ReportParms reportParms;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Csenet30DayExtract.
    /// </summary>
    [JsonPropertyName("csenet30DayExtract")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Office",
      "ServiceProvider",
      "ReceivedDate",
      "ReferralNumber",
      "ReferralType"
    })]
    public Csenet30DayExtract2 Csenet30DayExtract
    {
      get => csenet30DayExtract ??= new();
      set => csenet30DayExtract = value;
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

    private Csenet30DayExtract2 csenet30DayExtract;
    private ReportParms reportParms;
  }
#endregion
}
