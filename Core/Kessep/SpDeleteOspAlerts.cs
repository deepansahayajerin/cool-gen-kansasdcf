// Program: SP_DELETE_OSP_ALERTS, ID: 371748105, model: 746.
// Short name: SWE01785
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_OSP_ALERTS.
/// </summary>
[Serializable]
public partial class SpDeleteOspAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_OSP_ALERTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteOspAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteOspAlerts.
  /// </summary>
  public SpDeleteOspAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadOfficeServiceProviderAlert())
    {
      DeleteOfficeServiceProviderAlert();
    }
    else
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_ALER_NF";
    }
  }

  private void DeleteOfficeServiceProviderAlert()
  {
    Update("DeleteOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      });
  }

  private bool ReadOfficeServiceProviderAlert()
  {
    entities.OfficeServiceProviderAlert.Populated = false;

    return Read("ReadOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.Populated = true;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    private OfficeServiceProviderAlert officeServiceProviderAlert;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    private OfficeServiceProviderAlert officeServiceProviderAlert;
  }
#endregion
}
