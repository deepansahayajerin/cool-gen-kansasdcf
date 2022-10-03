// Program: UPDATE_ADMINISTRATIVE_ACTION, ID: 372614854, model: 746.
// Short name: SWE01463
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_ADMINISTRATIVE_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates ADMINISTRATIVE ACTION.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateAdministrativeAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_ADMINISTRATIVE_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateAdministrativeAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateAdministrativeAction.
  /// </summary>
  public UpdateAdministrativeAction(IContext context, Import import,
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
      try
      {
        UpdateAdministrativeAction1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "ADMINISTRATIVE_ACTION_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
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
        entities.AdministrativeAction.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.AdministrativeAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private void UpdateAdministrativeAction1()
  {
    var description = import.AdministrativeAction.Description;
    var indicator = import.AdministrativeAction.Indicator;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.AdministrativeAction.Populated = false;
    Update("UpdateAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "description", description);
        db.SetString(command, "indicatr", indicator);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "type", entities.AdministrativeAction.Type1);
      });

    entities.AdministrativeAction.Description = description;
    entities.AdministrativeAction.Indicator = indicator;
    entities.AdministrativeAction.LastUpdatedBy = lastUpdatedBy;
    entities.AdministrativeAction.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.AdministrativeAction.Populated = true;
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
