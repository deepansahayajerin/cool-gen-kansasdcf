// Program: SC_CAB_VALIDATE_USER_TOP_SECRET, ID: 371453010, model: 746.
// Short name: SWE01083
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_CAB_VALIDATE_USER_TOP_SECRET.
/// </para>
/// <para>
/// RESP: SECUR
/// </para>
/// </summary>
[Serializable]
public partial class ScCabValidateUserTopSecret: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_VALIDATE_USER_TOP_SECRET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabValidateUserTopSecret(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabValidateUserTopSecret.
  /// </summary>
  public ScCabValidateUserTopSecret(IContext context, Import import,
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
    UseScEabValidateTopSecret();

    if (AsChar(local.Security.Indicator) == 'Y')
    {
    }
    else
    {
      ExitState = "SC0030_USERID_NOT_DEFINED";
    }
  }

  private void UseScEabValidateTopSecret()
  {
    var useImport = new ScEabValidateTopSecret.Import();
    var useExport = new ScEabValidateTopSecret.Export();

    useImport.Security.Userid = import.Security.Userid;

    Call(ScEabValidateTopSecret.Execute, useImport, useExport);

    local.Security.Assign(useExport.Security);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
