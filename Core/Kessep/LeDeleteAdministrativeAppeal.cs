// Program: LE_DELETE_ADMINISTRATIVE_APPEAL, ID: 372605860, model: 746.
// Short name: SWE00751
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_ADMINISTRATIVE_APPEAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block deletes Administrative Appeals.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteAdministrativeAppeal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_ADMINISTRATIVE_APPEAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteAdministrativeAppeal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteAdministrativeAppeal.
  /// </summary>
  public LeDeleteAdministrativeAppeal(IContext context, Import import,
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
    if (ReadAdministrativeAppeal())
    {
      DeleteAdministrativeAppeal();
    }
    else
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
    }
  }

  private void DeleteAdministrativeAppeal()
  {
    Update("DeleteAdministrativeAppeal#1",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", entities.AdministrativeAppeal.Identifier);
      });

    Update("DeleteAdministrativeAppeal#2",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", entities.AdministrativeAppeal.Identifier);
      });

    Update("DeleteAdministrativeAppeal#3",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", entities.AdministrativeAppeal.Identifier);
      });

    Update("DeleteAdministrativeAppeal#4",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", entities.AdministrativeAppeal.Identifier);
      });
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Populated = true;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private AdministrativeAppeal administrativeAppeal;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private AdministrativeAppeal administrativeAppeal;
  }
#endregion
}
