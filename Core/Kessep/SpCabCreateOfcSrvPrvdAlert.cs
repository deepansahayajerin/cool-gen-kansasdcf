// Program: SP_CAB_CREATE_OFC_SRV_PRVD_ALERT, ID: 371748104, model: 746.
// Short name: SWE01779
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_OFC_SRV_PRVD_ALERT.
/// </summary>
[Serializable]
public partial class SpCabCreateOfcSrvPrvdAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_OFC_SRV_PRVD_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateOfcSrvPrvdAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateOfcSrvPrvdAlert.
  /// </summary>
  public SpCabCreateOfcSrvPrvdAlert(IContext context, Import import,
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
    // CHANGE LOG:
    // ----------------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	-------------	
    // ------------------------------------------------------------------------------
    // ??/??/??  ??????	????????	Initial Development
    // 03/21/00  PMcElderry			Added logic to create a random number for 
    // System_Generated_Identifier for
    // 					OFFICE_SERVICE_PROVIDER due to performance problems; removed 
    // references to
    // 					CONTROL_TABLE
    // 02/05/09  GVandy	CQ#8958		Re-structured for better readability and 
    // performance.
    // ----------------------------------------------------------------------------------------------------------------------
    if (!ReadOfficeServiceProvider())
    {
      if (ReadOffice())
      {
        if (ReadServiceProvider())
        {
          ExitState = "CO0000_SRVC_PRVDR_NOT_W_OFFICE";
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";
        }
      }
      else
      {
        ExitState = "OFFICE_NF";
      }

      return;
    }

    if (import.Alerts.SystemGeneratedIdentifier != 0)
    {
      if (!ReadInfrastructure())
      {
        ExitState = "SP0000_HISTORY_INFRASTRUCTURE_NF";

        return;
      }
    }

    do
    {
      try
      {
        CreateOfficeServiceProviderAlert();

        if (entities.Infrastructure.Populated)
        {
          AssociateOfficeServiceProviderAlert();
        }

        export.OfficeServiceProviderAlert.Assign(
          entities.OfficeServiceProviderAlert);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            // -- repeat until successful
            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_OSP_ALERT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(export.OfficeServiceProviderAlert.SystemGeneratedIdentifier == 0);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void AssociateOfficeServiceProviderAlert()
  {
    var infId = entities.Infrastructure.SystemGeneratedIdentifier;

    entities.OfficeServiceProviderAlert.Populated = false;
    Update("AssociateOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetNullableInt32(command, "infId", infId);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      });

    entities.OfficeServiceProviderAlert.InfId = infId;
    entities.OfficeServiceProviderAlert.Populated = true;
  }

  private void CreateOfficeServiceProviderAlert()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var typeCode = import.OfficeServiceProviderAlert.TypeCode;
    var message = import.OfficeServiceProviderAlert.Message;
    var description = import.OfficeServiceProviderAlert.Description ?? "";
    var distributionDate = import.OfficeServiceProviderAlert.DistributionDate;
    var situationIdentifier =
      import.OfficeServiceProviderAlert.SituationIdentifier;
    var prioritizationCode =
      import.OfficeServiceProviderAlert.PrioritizationCode.GetValueOrDefault();
    var optimizationInd = import.OfficeServiceProviderAlert.OptimizationInd ?? ""
      ;
    var optimizedFlag = import.OfficeServiceProviderAlert.OptimizedFlag ?? "";
    var recipientUserId = import.OfficeServiceProviderAlert.RecipientUserId;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.OfficeServiceProviderAlert.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp =
      import.OfficeServiceProviderAlert.LastUpdatedTimestamp;
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;

    entities.OfficeServiceProviderAlert.Populated = false;
    Update("CreateOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "typeCode", typeCode);
        db.SetString(command, "message", message);
        db.SetNullableString(command, "description", description);
        db.SetDate(command, "distributionDate", distributionDate);
        db.SetString(command, "situationIdentifi", situationIdentifier);
        db.SetNullableInt32(command, "prioritizationCod", prioritizationCode);
        db.SetNullableString(command, "optimizationInd", optimizationInd);
        db.SetNullableString(command, "optimizedFlag", optimizedFlag);
        db.SetString(command, "userId", recipientUserId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableInt32(command, "spdId", spdId);
        db.SetNullableInt32(command, "offId", offId);
        db.SetNullableString(command, "ospCode", ospCode);
        db.SetNullableDate(command, "ospDate", ospDate);
      });

    entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.OfficeServiceProviderAlert.TypeCode = typeCode;
    entities.OfficeServiceProviderAlert.Message = message;
    entities.OfficeServiceProviderAlert.Description = description;
    entities.OfficeServiceProviderAlert.DistributionDate = distributionDate;
    entities.OfficeServiceProviderAlert.SituationIdentifier =
      situationIdentifier;
    entities.OfficeServiceProviderAlert.PrioritizationCode = prioritizationCode;
    entities.OfficeServiceProviderAlert.OptimizationInd = optimizationInd;
    entities.OfficeServiceProviderAlert.OptimizedFlag = optimizedFlag;
    entities.OfficeServiceProviderAlert.RecipientUserId = recipientUserId;
    entities.OfficeServiceProviderAlert.CreatedBy = createdBy;
    entities.OfficeServiceProviderAlert.CreatedTimestamp = createdTimestamp;
    entities.OfficeServiceProviderAlert.LastUpdatedBy = lastUpdatedBy;
    entities.OfficeServiceProviderAlert.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.OfficeServiceProviderAlert.InfId = null;
    entities.OfficeServiceProviderAlert.SpdId = spdId;
    entities.OfficeServiceProviderAlert.OffId = offId;
    entities.OfficeServiceProviderAlert.OspCode = ospCode;
    entities.OfficeServiceProviderAlert.OspDate = ospDate;
    entities.OfficeServiceProviderAlert.Populated = true;
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI", import.Alerts.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.Populated = true;
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
    /// A value of Alerts.
    /// </summary>
    [JsonPropertyName("alerts")]
    public Infrastructure Alerts
    {
      get => alerts ??= new();
      set => alerts = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private Infrastructure alerts;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public SystemGenerated Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    private SystemGenerated zdel;
    private DateWorkArea zdelLocalCurrent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ControlTable Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private ControlTable zdel;
    private Infrastructure infrastructure;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
  }
#endregion
}
