// Program: SWCOTRP_PROXY_TEST_GET_REGION, ID: 374536108, model: 746.
// Short name: IMP35922
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SWCOTRP_PROXY_TEST_GET_REGION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server)]
public partial class SwcotrpProxyTestGetRegion: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SWCOTRP_PROXY_TEST_GET_REGION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SwcotrpProxyTestGetRegion(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SwcotrpProxyTestGetRegion.
  /// </summary>
  public SwcotrpProxyTestGetRegion(IContext context, Import import,
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
    export.CicsUserId.Command = global.UserId;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
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
    /// A value of CicsRegion.
    /// </summary>
    [JsonPropertyName("cicsRegion")]
    public Common CicsRegion
    {
      get => cicsRegion ??= new();
      set => cicsRegion = value;
    }

    /// <summary>
    /// A value of CicsUserId.
    /// </summary>
    [JsonPropertyName("cicsUserId")]
    public Common CicsUserId
    {
      get => cicsUserId ??= new();
      set => cicsUserId = value;
    }

    private Common cicsRegion;
    private Common cicsUserId;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private Code code;
  }
#endregion
}
