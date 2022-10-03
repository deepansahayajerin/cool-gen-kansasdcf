// Program: SC_CAB_SECURITY_VIOLATION_CHECK, ID: 371453007, model: 746.
// Short name: SWE01079
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_CAB_SECURITY_VIOLATION_CHECK.
/// </para>
/// <para>
/// RESP: SECUR
/// </para>
/// </summary>
[Serializable]
public partial class ScCabSecurityViolationCheck: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_SECURITY_VIOLATION_CHECK program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabSecurityViolationCheck(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabSecurityViolationCheck.
  /// </summary>
  public ScCabSecurityViolationCheck(IContext context, Import import,
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
    switch(TrimEnd(import.Security.Command))
    {
      case "NOTAUTH":
        ExitState = "SC0004_USER_NOT_AUTH_MENU_SEL";

        break;
      case "NOTRAN":
        ExitState = "SC0002_SCREEN_ID_NF";

        break;
      case "XXNOKEY":
        ExitState = "SC0025_KEY_NF_RB";

        break;
      case "NOLINK":
        ExitState = "SC0028_USER_NOT_AUTH_LINK_TRAN";

        break;
      case "NONEXT":
        ExitState = "SC0001_USER_NOT_AUTH_TRAN";

        break;
      default:
        break;
    }
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
  }
#endregion
}
