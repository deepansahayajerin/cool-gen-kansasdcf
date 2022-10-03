// Program: SC_EAB_VALIDATE_TOP_SECRET, ID: 371453252, model: 746.
// Short name: SWE01085
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: SC_EAB_VALIDATE_TOP_SECRET.
/// </summary>
[Serializable]
public partial class ScEabValidateTopSecret: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_EAB_VALIDATE_TOP_SECRET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScEabValidateTopSecret(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScEabValidateTopSecret.
  /// </summary>
  public ScEabValidateTopSecret(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Security.Indicator = "";
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
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    private Security2 security;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    private Security2 security;
  }
#endregion
}
