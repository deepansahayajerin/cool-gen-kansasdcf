// Program: SP_CREATE_OFFICE_SERVICE_PRVDR, ID: 371784416, model: 746.
// Short name: SWE01315
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_OFFICE_SERVICE_PRVDR.
/// </summary>
[Serializable]
public partial class SpCreateOfficeServicePrvdr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OFFICE_SERVICE_PRVDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOfficeServicePrvdr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOfficeServicePrvdr.
  /// </summary>
  public SpCreateOfficeServicePrvdr(IContext context, Import import,
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
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);

    if (ReadOffice())
    {
      export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;

      if (ReadServiceProvider())
      {
        export.ServiceProvider.SystemGeneratedId =
          entities.ServiceProvider.SystemGeneratedId;

        try
        {
          CreateOfficeServiceProvider();
          export.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OFFICE_SERVICE_PROVIDER_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "OFFICE_SERVICE_PROVIDER_PV";

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
        ExitState = "SERVICE_PROVIDER_NF";
      }
    }
    else
    {
      ExitState = "FN0000_OFFICE_NF";
    }
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.WorkPhoneExtension = source.WorkPhoneExtension;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.RoleCode = source.RoleCode;
    target.WorkPhoneNumber = source.WorkPhoneNumber;
    target.WorkFaxNumber = source.WorkFaxNumber;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.LocalContactCodeForIrs = source.LocalContactCodeForIrs;
  }

  private void CreateOfficeServiceProvider()
  {
    var spdGeneratedId = entities.ServiceProvider.SystemGeneratedId;
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var roleCode = import.OfficeServiceProvider.RoleCode;
    var effectiveDate = import.OfficeServiceProvider.EffectiveDate;
    var workPhoneNumber = import.OfficeServiceProvider.WorkPhoneNumber;
    var workFaxNumber =
      import.OfficeServiceProvider.WorkFaxNumber.GetValueOrDefault();
    var discontinueDate = import.OfficeServiceProvider.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var workPhoneExtension =
      import.OfficeServiceProvider.WorkPhoneExtension ?? "";
    var workPhoneAreaCode = import.OfficeServiceProvider.WorkPhoneAreaCode;
    var localContactCodeForIrs =
      import.OfficeServiceProvider.LocalContactCodeForIrs.GetValueOrDefault();

    entities.OfficeServiceProvider.Populated = false;
    Update("CreateOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetInt32(command, "offGeneratedId", offGeneratedId);
        db.SetString(command, "roleCode", roleCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetInt32(command, "workPhoneNumber", workPhoneNumber);
        db.SetNullableInt32(command, "workFaxNumber", workFaxNumber);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdtdDtstamp", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "zdelCertNumber", "");
        db.SetNullableInt32(command, "workFaxAreaCd", 0);
        db.SetNullableString(command, "workPhoneExt", workPhoneExtension);
        db.SetInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "locContForIrs", localContactCodeForIrs);
      });

    entities.OfficeServiceProvider.SpdGeneratedId = spdGeneratedId;
    entities.OfficeServiceProvider.OffGeneratedId = offGeneratedId;
    entities.OfficeServiceProvider.RoleCode = roleCode;
    entities.OfficeServiceProvider.EffectiveDate = effectiveDate;
    entities.OfficeServiceProvider.WorkPhoneNumber = workPhoneNumber;
    entities.OfficeServiceProvider.WorkFaxNumber = workFaxNumber;
    entities.OfficeServiceProvider.DiscontinueDate = discontinueDate;
    entities.OfficeServiceProvider.CreatedBy = createdBy;
    entities.OfficeServiceProvider.CreatedTimestamp = createdTimestamp;
    entities.OfficeServiceProvider.WorkPhoneExtension = workPhoneExtension;
    entities.OfficeServiceProvider.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.OfficeServiceProvider.LocalContactCodeForIrs =
      localContactCodeForIrs;
    entities.OfficeServiceProvider.Populated = true;
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

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CseOrganization cseOrganization;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CseOrganization cseOrganization;
    private Office office;
  }
#endregion
}
