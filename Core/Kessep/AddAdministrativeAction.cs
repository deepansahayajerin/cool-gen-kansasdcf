// Program: ADD_ADMINISTRATIVE_ACTION, ID: 372614855, model: 746.
// Short name: SWE00005
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: ADD_ADMINISTRATIVE_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates ADMINISTRATIVE ACTION.
/// </para>
/// </summary>
[Serializable]
public partial class AddAdministrativeAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ADD_ADMINISTRATIVE_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new AddAdministrativeAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of AddAdministrativeAction.
  /// </summary>
  public AddAdministrativeAction(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    try
    {
      CreateAdministrativeAction();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "ADMINISTRATIVE_ACTION_AE";

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

  private void CreateAdministrativeAction()
  {
    var type1 = import.AdministrativeAction.Type1;
    var description = import.AdministrativeAction.Description;
    var indicator = import.AdministrativeAction.Indicator;
    var createdBy = global.UserId;
    var createdTstamp = Now();

    entities.AdministrativeAction.Populated = false;
    Update("CreateAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetString(command, "description", description);
        db.SetString(command, "indicatr", indicator);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
      });

    entities.AdministrativeAction.Type1 = type1;
    entities.AdministrativeAction.Description = description;
    entities.AdministrativeAction.Indicator = indicator;
    entities.AdministrativeAction.CreatedBy = createdBy;
    entities.AdministrativeAction.CreatedTstamp = createdTstamp;
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
