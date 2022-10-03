// Program: LE_GEN_ADMIN_ACTION_CRITERIA_ID, ID: 372616054, model: 746.
// Short name: SWE00774
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_GEN_ADMIN_ACTION_CRITERIA_ID.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block generates the sequential Administrative Action Criteria 
/// Identifier.
/// </para>
/// </summary>
[Serializable]
public partial class LeGenAdminActionCriteriaId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_GEN_ADMIN_ACTION_CRITERIA_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeGenAdminActionCriteriaId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeGenAdminActionCriteriaId.
  /// </summary>
  public LeGenAdminActionCriteriaId(IContext context, Import import,
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
    export.AdministrativeActionCriteria.Identifier = 0;

    if (ReadAdministrativeActionCriteria())
    {
      // *********************************************
      // Get the last Administrative Action Identifier
      // assigned.
      // *********************************************
    }

    export.AdministrativeActionCriteria.Identifier =
      entities.AdministrativeActionCriteria.Identifier + 1;
  }

  private bool ReadAdministrativeActionCriteria()
  {
    entities.AdministrativeActionCriteria.Populated = false;

    return Read("ReadAdministrativeActionCriteria",
      (db, command) =>
      {
        db.SetString(command, "aatType", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeActionCriteria.AatType = db.GetString(reader, 0);
        entities.AdministrativeActionCriteria.Identifier =
          db.GetInt32(reader, 1);
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

    private AdministrativeAction administrativeAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AdministrativeActionCriteria.
    /// </summary>
    [JsonPropertyName("administrativeActionCriteria")]
    public AdministrativeActionCriteria AdministrativeActionCriteria
    {
      get => administrativeActionCriteria ??= new();
      set => administrativeActionCriteria = value;
    }

    private AdministrativeActionCriteria administrativeActionCriteria;
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
