// Program: FN_B717_GET_SUPERVISOR_SP, ID: 373349244, model: 746.
// Short name: SWE03033
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_GET_SUPERVISOR_SP.
/// </summary>
[Serializable]
public partial class FnB717GetSupervisorSp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_GET_SUPERVISOR_SP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717GetSupervisorSp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717GetSupervisorSp.
  /// </summary>
  public FnB717GetSupervisorSp(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (Equal(import.OfficeServiceProvider.RoleCode, "CH"))
    {
      export.Sup.SystemGeneratedId = import.ServiceProvider.SystemGeneratedId;

      return;
    }

    if (Equal(import.OfficeServiceProvider.RoleCode, "SS"))
    {
      export.Sup.SystemGeneratedId = import.ServiceProvider.SystemGeneratedId;
    }

    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      local.OfficeServiceProvider);
    local.ServiceProvider.SystemGeneratedId =
      import.ServiceProvider.SystemGeneratedId;

    do
    {
      foreach(var item in ReadServiceProviderOfficeServiceProvRelationship())
      {
        if (Equal(entities.ParentOfficeServiceProvider.RoleCode, "SS"))
        {
          export.Sup.SystemGeneratedId =
            entities.ParentServiceProvider.SystemGeneratedId;

          return;
        }

        if (Equal(entities.ParentOfficeServiceProvider.RoleCode, "CH") && !
          Equal(import.OfficeServiceProvider.RoleCode, "SS"))
        {
          export.Sup.SystemGeneratedId =
            entities.ParentServiceProvider.SystemGeneratedId;

          return;
        }

        MoveOfficeServiceProvider(entities.ParentOfficeServiceProvider,
          local.OfficeServiceProvider);
        local.ServiceProvider.SystemGeneratedId =
          entities.ParentServiceProvider.SystemGeneratedId;
      }

      ++local.Common.Count;
    }
    while(local.Common.Count <= 5);
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProvRelationship()
  {
    entities.OfficeServiceProvRelationship.Populated = false;
    entities.ParentServiceProvider.Populated = false;
    entities.ParentOfficeServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetString(
          command, "ospRoleCode", local.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          local.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId", local.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ParentServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 0);
        entities.ParentOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ParentOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 1);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 2);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 3);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 4);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 5);
        entities.ParentOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 5);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 6);
        entities.ParentOfficeServiceProvider.RoleCode = db.GetString(reader, 6);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 7);
        entities.ParentOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 8);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 9);
        entities.OfficeServiceProvRelationship.Populated = true;
        entities.ParentServiceProvider.Populated = true;
        entities.ParentOfficeServiceProvider.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Sup.
    /// </summary>
    [JsonPropertyName("sup")]
    public ServiceProvider Sup
    {
      get => sup ??= new();
      set => sup = value;
    }

    private ServiceProvider sup;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CoServiceProvider.
    /// </summary>
    [JsonPropertyName("coServiceProvider")]
    public ServiceProvider CoServiceProvider
    {
      get => coServiceProvider ??= new();
      set => coServiceProvider = value;
    }

    /// <summary>
    /// A value of CoOffice.
    /// </summary>
    [JsonPropertyName("coOffice")]
    public Office CoOffice
    {
      get => coOffice ??= new();
      set => coOffice = value;
    }

    /// <summary>
    /// A value of CoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("coOfficeServiceProvider")]
    public OfficeServiceProvider CoOfficeServiceProvider
    {
      get => coOfficeServiceProvider ??= new();
      set => coOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of ParentServiceProvider.
    /// </summary>
    [JsonPropertyName("parentServiceProvider")]
    public ServiceProvider ParentServiceProvider
    {
      get => parentServiceProvider ??= new();
      set => parentServiceProvider = value;
    }

    /// <summary>
    /// A value of ParentOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("parentOfficeServiceProvider")]
    public OfficeServiceProvider ParentOfficeServiceProvider
    {
      get => parentOfficeServiceProvider ??= new();
      set => parentOfficeServiceProvider = value;
    }

    private ServiceProvider coServiceProvider;
    private Office coOffice;
    private OfficeServiceProvider coOfficeServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private ServiceProvider parentServiceProvider;
    private OfficeServiceProvider parentOfficeServiceProvider;
  }
#endregion
}
