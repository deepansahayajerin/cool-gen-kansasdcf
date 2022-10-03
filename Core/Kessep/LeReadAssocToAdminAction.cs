// Program: LE_READ_ASSOC_TO_ADMIN_ACTION, ID: 372614856, model: 746.
// Short name: SWE00812
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_READ_ASSOC_TO_ADMIN_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This acion block reads to see if an Administrative Action is associated to 
/// any other entity type.
/// </para>
/// </summary>
[Serializable]
public partial class LeReadAssocToAdminAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_READ_ASSOC_TO_ADMIN_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeReadAssocToAdminAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeReadAssocToAdminAction.
  /// </summary>
  public LeReadAssocToAdminAction(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadAdministrativeAction1())
    {
      ExitState = "ADMIN_ACTION_ASSOC_TO_CERT";

      return;
    }

    if (ReadAdministrativeAction2())
    {
      ExitState = "ADMIN_ACTION_ASSOC_TO_OBLIG_ADM";

      return;
    }

    if (ReadAdministrativeAction3())
    {
      ExitState = "ADMIN_ACTION_ASSOC_TO_CRITERIA";

      return;
    }

    if (ReadAdministrativeAction4())
    {
      ExitState = "ADMIN_ACTION_ASSOC_TO_EXEMPT";
    }
  }

  private bool ReadAdministrativeAction1()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction1",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadAdministrativeAction2()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction2",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadAdministrativeAction3()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction3",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadAdministrativeAction4()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction4",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
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
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
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

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    private ObligationAdministrativeAction obligationAdministrativeAction;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeActionCriteria administrativeActionCriteria;
    private AdministrativeAction administrativeAction;
  }
#endregion
}
