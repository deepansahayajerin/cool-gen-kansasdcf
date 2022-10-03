// Program: FN_EXT_GET_PARMS_THRU_JCL_SYSIN, ID: 372802424, model: 746.
// Short name: SWEXG001
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EXT_GET_PARMS_THRU_JCL_SYSIN.
/// </summary>
[Serializable]
public partial class FnExtGetParmsThruJclSysin: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EXT_GET_PARMS_THRU_JCL_SYSIN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnExtGetParmsThruJclSysin(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnExtGetParmsThruJclSysin.
  /// </summary>
  public FnExtGetParmsThruJclSysin(IContext context, Import import,
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
      "SWEXG001", context, import, export, EabOptions.Hpvp);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "ParameterList" })
      ]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "NumericReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private External external;
  }
#endregion
}
