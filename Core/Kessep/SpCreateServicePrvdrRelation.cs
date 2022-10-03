// Program: SP_CREATE_SERVICE_PRVDR_RELATION, ID: 371783594, model: 746.
// Short name: SWE01324
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_SERVICE_PRVDR_RELATION.
/// </summary>
[Serializable]
public partial class SpCreateServicePrvdrRelation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_SERVICE_PRVDR_RELATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateServicePrvdrRelation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateServicePrvdrRelation.
  /// </summary>
  public SpCreateServicePrvdrRelation(IContext context, Import import,
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
    export.OfficeServiceProvRelationship.ReasonCode =
      import.OfficeServiceProvRelationship.ReasonCode;

    if (ReadOfficeServiceProviderServiceProviderOffice())
    {
      export.Leading.EffectiveDate =
        entities.LeadingOfficeServiceProvider.EffectiveDate;

      if (ReadOfficeServiceProviderServiceProvider())
      {
        export.OfficeServiceProvider.EffectiveDate =
          entities.OfficeServiceProvider.EffectiveDate;

        try
        {
          CreateOfficeServiceProvRelationship();
          MoveOfficeServiceProvRelationship(entities.
            OfficeServiceProvRelationship,
            export.OfficeServiceProvRelationship);
          ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SERVICE_PROVIDER_RELATIONSHI_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SERVICE_PROVIDER_RELATIONSHI_PV";

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
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";
      }
    }
    else
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";
    }
  }

  private static void MoveOfficeServiceProvRelationship(
    OfficeServiceProvRelationship source, OfficeServiceProvRelationship target)
  {
    target.ReasonCode = source.ReasonCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedDtstamp = source.CreatedDtstamp;
  }

  private void CreateOfficeServiceProvRelationship()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(
      entities.LeadingOfficeServiceProvider.Populated);

    var ospEffectiveDate = entities.OfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var offGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;
    var ospREffectiveDt = entities.LeadingOfficeServiceProvider.EffectiveDate;
    var ospRRoleCode = entities.LeadingOfficeServiceProvider.RoleCode;
    var offRGeneratedId = entities.LeadingOfficeServiceProvider.OffGeneratedId;
    var spdRGeneratedId = entities.LeadingOfficeServiceProvider.SpdGeneratedId;
    var reasonCode = import.OfficeServiceProvRelationship.ReasonCode;
    var createdBy = global.UserId;
    var createdDtstamp = Now();

    entities.OfficeServiceProvRelationship.Populated = false;
    Update("CreateOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.SetDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetString(command, "ospRoleCode", ospRoleCode);
        db.SetInt32(command, "offGeneratedId", offGeneratedId);
        db.SetInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetDate(command, "ospREffectiveDt", ospREffectiveDt);
        db.SetString(command, "ospRRoleCode", ospRRoleCode);
        db.SetInt32(command, "offRGeneratedId", offRGeneratedId);
        db.SetInt32(command, "spdRGeneratedId", spdRGeneratedId);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdDtstamp", createdDtstamp);
      });

    entities.OfficeServiceProvRelationship.OspEffectiveDate = ospEffectiveDate;
    entities.OfficeServiceProvRelationship.OspRoleCode = ospRoleCode;
    entities.OfficeServiceProvRelationship.OffGeneratedId = offGeneratedId;
    entities.OfficeServiceProvRelationship.SpdGeneratedId = spdGeneratedId;
    entities.OfficeServiceProvRelationship.OspREffectiveDt = ospREffectiveDt;
    entities.OfficeServiceProvRelationship.OspRRoleCode = ospRRoleCode;
    entities.OfficeServiceProvRelationship.OffRGeneratedId = offRGeneratedId;
    entities.OfficeServiceProvRelationship.SpdRGeneratedId = spdRGeneratedId;
    entities.OfficeServiceProvRelationship.ReasonCode = reasonCode;
    entities.OfficeServiceProvRelationship.CreatedBy = createdBy;
    entities.OfficeServiceProvRelationship.CreatedDtstamp = createdDtstamp;
    entities.OfficeServiceProvRelationship.Populated = true;
  }

  private bool ReadOfficeServiceProviderServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice()
  {
    entities.Office.Populated = false;
    entities.LeadingServiceProvider.Populated = false;
    entities.LeadingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          import.LeadingServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.LeadingOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode", import.LeadingOfficeServiceProvider.RoleCode);
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.LeadingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeadingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.LeadingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.LeadingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.LeadingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.Office.Populated = true;
        entities.LeadingServiceProvider.Populated = true;
        entities.LeadingOfficeServiceProvider.Populated = true;
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
    /// A value of LeadingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leadingOfficeServiceProvider")]
    public OfficeServiceProvider LeadingOfficeServiceProvider
    {
      get => leadingOfficeServiceProvider ??= new();
      set => leadingOfficeServiceProvider = value;
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
    /// A value of Import2CseOrganization.
    /// </summary>
    [JsonPropertyName("import2CseOrganization")]
    public CseOrganization Import2CseOrganization
    {
      get => import2CseOrganization ??= new();
      set => import2CseOrganization = value;
    }

    /// <summary>
    /// A value of Import2Office.
    /// </summary>
    [JsonPropertyName("import2Office")]
    public Office Import2Office
    {
      get => import2Office ??= new();
      set => import2Office = value;
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
    /// A value of LeadingServiceProvider.
    /// </summary>
    [JsonPropertyName("leadingServiceProvider")]
    public ServiceProvider LeadingServiceProvider
    {
      get => leadingServiceProvider ??= new();
      set => leadingServiceProvider = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProvider leadingOfficeServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private CseOrganization import2CseOrganization;
    private Office import2Office;
    private ServiceProvider serviceProvider;
    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider leadingServiceProvider;
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
    /// A value of Leading.
    /// </summary>
    [JsonPropertyName("leading")]
    public OfficeServiceProvider Leading
    {
      get => leading ??= new();
      set => leading = value;
    }

    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProvider leading;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of Persistent2CseOrganization.
    /// </summary>
    [JsonPropertyName("persistent2CseOrganization")]
    public CseOrganization Persistent2CseOrganization
    {
      get => persistent2CseOrganization ??= new();
      set => persistent2CseOrganization = value;
    }

    /// <summary>
    /// A value of Persistent2Office.
    /// </summary>
    [JsonPropertyName("persistent2Office")]
    public Office Persistent2Office
    {
      get => persistent2Office ??= new();
      set => persistent2Office = value;
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
    /// A value of LeadingServiceProvider.
    /// </summary>
    [JsonPropertyName("leadingServiceProvider")]
    public ServiceProvider LeadingServiceProvider
    {
      get => leadingServiceProvider ??= new();
      set => leadingServiceProvider = value;
    }

    /// <summary>
    /// A value of LeadingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("leadingOfficeServiceProvider")]
    public OfficeServiceProvider LeadingOfficeServiceProvider
    {
      get => leadingOfficeServiceProvider ??= new();
      set => leadingOfficeServiceProvider = value;
    }

    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private CseOrganization persistent2CseOrganization;
    private Office persistent2Office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider leadingServiceProvider;
    private OfficeServiceProvider leadingOfficeServiceProvider;
  }
#endregion
}
