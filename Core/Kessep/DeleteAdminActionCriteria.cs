// Program: DELETE_ADMIN_ACTION_CRITERIA, ID: 372615761, model: 746.
// Short name: SWE00171
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DELETE_ADMIN_ACTION_CRITERIA.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process deletes ADMINISTRATIVE ACTION CRITERIA.
/// </para>
/// </summary>
[Serializable]
public partial class DeleteAdminActionCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_ADMIN_ACTION_CRITERIA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteAdminActionCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteAdminActionCriteria.
  /// </summary>
  public DeleteAdminActionCriteria(IContext context, Import import,
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
    if (ReadAdministrativeActionCriteria())
    {
      DeleteAdministrativeActionCriteria();
    }
    else
    {
      ExitState = "ADMIN_ACTION_CRITERIA_NF";
    }
  }

  private void DeleteAdministrativeActionCriteria()
  {
    Update("DeleteAdministrativeActionCriteria",
      (db, command) =>
      {
        db.SetString(
          command, "aatType", entities.AdministrativeActionCriteria.AatType);
        db.SetInt32(
          command, "adminActionId",
          entities.AdministrativeActionCriteria.Identifier);
      });
  }

  private bool ReadAdministrativeActionCriteria()
  {
    entities.AdministrativeActionCriteria.Populated = false;

    return Read("ReadAdministrativeActionCriteria",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminActionId",
          import.AdministrativeActionCriteria.Identifier);
        db.SetString(command, "aatType", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeActionCriteria.AatType = db.GetString(reader, 0);
        entities.AdministrativeActionCriteria.Identifier =
          db.GetInt32(reader, 1);
        entities.AdministrativeActionCriteria.Description =
          db.GetString(reader, 2);
        entities.AdministrativeActionCriteria.Populated = true;
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

    /// <summary>
    /// A value of AdministrativeActionCriteria.
    /// </summary>
    [JsonPropertyName("administrativeActionCriteria")]
    public AdministrativeActionCriteria AdministrativeActionCriteria
    {
      get => administrativeActionCriteria ??= new();
      set => administrativeActionCriteria = value;
    }

    private AdministrativeAction administrativeAction;
    private AdministrativeActionCriteria administrativeActionCriteria;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    /// <summary>
    /// A value of AdministrativeActionCriteria.
    /// </summary>
    [JsonPropertyName("administrativeActionCriteria")]
    public AdministrativeActionCriteria AdministrativeActionCriteria
    {
      get => administrativeActionCriteria ??= new();
      set => administrativeActionCriteria = value;
    }

    private AdministrativeAction administrativeAction;
    private AdministrativeActionCriteria administrativeActionCriteria;
  }
#endregion
}
