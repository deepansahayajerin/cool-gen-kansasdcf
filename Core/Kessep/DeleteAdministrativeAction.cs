// Program: DELETE_ADMINISTRATIVE_ACTION, ID: 372614853, model: 746.
// Short name: SWE00172
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DELETE_ADMINISTRATIVE_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process deletes ADMINISTRATIVE ACTION.
/// </para>
/// </summary>
[Serializable]
public partial class DeleteAdministrativeAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_ADMINISTRATIVE_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteAdministrativeAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteAdministrativeAction.
  /// </summary>
  public DeleteAdministrativeAction(IContext context, Import import,
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
    if (ReadAdministrativeAction())
    {
      DeleteAdministrativeAction1();
    }
    else
    {
      ExitState = "ADMINISTRATIVE_ACTION_NF";
    }
  }

  private void DeleteAdministrativeAction1()
  {
    bool exists;

    exists = Read("DeleteAdministrativeAction#1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "aatType", entities.AdministrativeAction.Type1);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ADMIN_ACT_CERT\".",
        "50001");
    }

    exists = Read("DeleteAdministrativeAction#2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "aatType", entities.AdministrativeAction.Type1);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_OBLIG_ADMIN_AC\".",
        "50001");
    }

    Update("DeleteAdministrativeAction#3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "aatType", entities.AdministrativeAction.Type1);
      });
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
