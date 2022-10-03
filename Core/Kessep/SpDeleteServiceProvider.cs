// Program: SP_DELETE_SERVICE_PROVIDER, ID: 371454599, model: 746.
// Short name: SWE01339
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_SERVICE_PROVIDER.
/// </summary>
[Serializable]
public partial class SpDeleteServiceProvider: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_SERVICE_PROVIDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteServiceProvider(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteServiceProvider.
  /// </summary>
  public SpDeleteServiceProvider(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadServiceProvider())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    DeleteServiceProvider();
  }

  private void DeleteServiceProvider()
  {
    bool exists;

    exists = Read("DeleteServiceProvider#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ADMIN_ACT_CERT\".",
        "50001");
    }

    Update("DeleteServiceProvider#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", import.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.Populated = true;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private ServiceProvider serviceProvider;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private ServiceProvider serviceProvider;
  }
#endregion
}
