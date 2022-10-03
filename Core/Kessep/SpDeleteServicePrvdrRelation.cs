// Program: SP_DELETE_SERVICE_PRVDR_RELATION, ID: 371783593, model: 746.
// Short name: SWE01341
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_SERVICE_PRVDR_RELATION.
/// </summary>
[Serializable]
public partial class SpDeleteServicePrvdrRelation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_SERVICE_PRVDR_RELATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteServicePrvdrRelation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteServicePrvdrRelation.
  /// </summary>
  public SpDeleteServicePrvdrRelation(IContext context, Import import,
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
    if (ReadOfficeServiceProvRelationship())
    {
      MoveOfficeServiceProvRelationship(entities.OfficeServiceProvRelationship,
        export.OfficeServiceProvRelationship);
      DeleteOfficeServiceProvRelationship();
      ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
    }
    else
    {
      ExitState = "SERVICE_PROVIDER_RELATIONSHI_NF";
    }
  }

  private static void MoveOfficeServiceProvRelationship(
    OfficeServiceProvRelationship source, OfficeServiceProvRelationship target)
  {
    target.ReasonCode = source.ReasonCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedDtstamp = source.CreatedDtstamp;
  }

  private void DeleteOfficeServiceProvRelationship()
  {
    Update("DeleteOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvRelationship.OspEffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "ospRoleCode",
          entities.OfficeServiceProvRelationship.OspRoleCode);
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvRelationship.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvRelationship.SpdGeneratedId);
        db.SetDate(
          command, "ospREffectiveDt",
          entities.OfficeServiceProvRelationship.OspREffectiveDt.
            GetValueOrDefault());
        db.SetString(
          command, "ospRRoleCode",
          entities.OfficeServiceProvRelationship.OspRRoleCode);
        db.SetInt32(
          command, "offRGeneratedId",
          entities.OfficeServiceProvRelationship.OffRGeneratedId);
        db.SetInt32(
          command, "spdRGeneratedId",
          entities.OfficeServiceProvRelationship.SpdRGeneratedId);
      });
  }

  private bool ReadOfficeServiceProvRelationship()
  {
    entities.OfficeServiceProvRelationship.Populated = false;

    return Read("ReadOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "ospRoleCode", import.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdRGeneratedId",
          import.LeaderServiceProvider.SystemGeneratedId);
        db.SetString(
          command, "ospRRoleCode", import.LeaderOfficeServiceProvider.RoleCode);
          
        db.SetDate(
          command, "ospREffectiveDt",
          import.LeaderOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 0);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 1);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 4);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 5);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 6);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 8);
        entities.OfficeServiceProvRelationship.CreatedBy =
          db.GetString(reader, 9);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvRelationship.Populated = true;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderOfficeServiceProvider")]
    public OfficeServiceProvider LeaderOfficeServiceProvider
    {
      get => leaderOfficeServiceProvider ??= new();
      set => leaderOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of Import2.
    /// </summary>
    [JsonPropertyName("import2")]
    public CseOrganization Import2
    {
      get => import2 ??= new();
      set => import2 = value;
    }

    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public Office Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    /// <summary>
    /// A value of LeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderServiceProvider")]
    public ServiceProvider LeaderServiceProvider
    {
      get => leaderServiceProvider ??= new();
      set => leaderServiceProvider = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

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

    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProvider leaderOfficeServiceProvider;
    private CseOrganization import2;
    private Office parent;
    private ServiceProvider leaderServiceProvider;
    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    private OfficeServiceProvRelationship officeServiceProvRelationship;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Persistent2.
    /// </summary>
    [JsonPropertyName("persistent2")]
    public CseOrganization Persistent2
    {
      get => persistent2 ??= new();
      set => persistent2 = value;
    }

    /// <summary>
    /// A value of Parent.
    /// </summary>
    [JsonPropertyName("parent")]
    public Office Parent
    {
      get => parent ??= new();
      set => parent = value;
    }

    /// <summary>
    /// A value of LeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderServiceProvider")]
    public ServiceProvider LeaderServiceProvider
    {
      get => leaderServiceProvider ??= new();
      set => leaderServiceProvider = value;
    }

    /// <summary>
    /// A value of LeaderOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leaderOfficeServiceProvider")]
    public OfficeServiceProvider LeaderOfficeServiceProvider
    {
      get => leaderOfficeServiceProvider ??= new();
      set => leaderOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

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

    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    private CseOrganization persistent2;
    private Office parent;
    private ServiceProvider leaderServiceProvider;
    private OfficeServiceProvider leaderOfficeServiceProvider;
    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
  }
#endregion
}
