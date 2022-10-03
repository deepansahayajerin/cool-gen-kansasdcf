// Program: LE_SET_EXITSTATE_FOR_LIST_OBLIGA, ID: 372578860, model: 746.
// Short name: SWE00820
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_SET_EXITSTATE_FOR_LIST_OBLIGA.
/// </summary>
[Serializable]
public partial class LeSetExitstateForListObliga: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_SET_EXITSTATE_FOR_LIST_OBLIGA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeSetExitstateForListObliga(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeSetExitstateForListObliga.
  /// </summary>
  public LeSetExitstateForListObliga(IContext context, Import import,
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
    export.Error.Flag = "N";

    switch(TrimEnd(import.AdministrativeActCertification.Type1))
    {
      case "COAG":
        ExitState = "ECO_LNK_TO_COAG_REFERRAL";

        break;
      case "CRED":
        ExitState = "ECO_LNK_TO_CRED_REPORTING";

        break;
      default:
        export.Error.Flag = "Y";
        ExitState = "LE0000_NO_DETAIL_SCREEN";

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
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    private AdministrativeActCertification administrativeActCertification;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    private Common error;
  }
#endregion
}
