// Program: CAB_READ_ADMIN_ACTION, ID: 372605740, model: 746.
// Short name: SWE00070
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_ADMIN_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This Action Block reads ADMINISTRATIVE_ACTION to check if an Administrative 
/// Action Type is valid.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdminAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADMIN_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdminAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdminAction.
  /// </summary>
  public CabReadAdminAction(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;

    if (ReadAdministrativeAction())
    {
      export.AdministrativeAction.Assign(entities.AdministrativeAction);
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      ExitState = "ADMINISTRATIVE_ACTION_NF";
    }
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Indicator = db.GetString(reader, 2);
        entities.AdministrativeAction.Populated = true;
      });
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
    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    private AdministrativeAction administrativeAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    private AdministrativeAction administrativeAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    private AdministrativeAction administrativeAction;
  }
#endregion
}
