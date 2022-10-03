// Program: SP_UPDATE_OFFICE_SERVICE_PRVDR, ID: 371784822, model: 746.
// Short name: SWE01445
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_OFFICE_SERVICE_PRVDR.
/// </summary>
[Serializable]
public partial class SpUpdateOfficeServicePrvdr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_OFFICE_SERVICE_PRVDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateOfficeServicePrvdr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateOfficeServicePrvdr.
  /// </summary>
  public SpUpdateOfficeServicePrvdr(IContext context, Import import,
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
    if (ReadOfficeServiceProvider())
    {
      try
      {
        UpdateOfficeServiceProvider();
        export.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OFFICE_SERVICE_PROVIDER_NU";

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
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";
    }
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 9);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 10);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 11);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private void UpdateOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var workPhoneNumber = import.OfficeServiceProvider.WorkPhoneNumber;
    var workFaxNumber =
      import.OfficeServiceProvider.WorkFaxNumber.GetValueOrDefault();
    var discontinueDate = import.OfficeServiceProvider.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedDtstamp = Now();
    var workPhoneExtension =
      import.OfficeServiceProvider.WorkPhoneExtension ?? "";
    var workPhoneAreaCode = import.OfficeServiceProvider.WorkPhoneAreaCode;
    var localContactCodeForIrs =
      import.OfficeServiceProvider.LocalContactCodeForIrs.GetValueOrDefault();

    entities.OfficeServiceProvider.Populated = false;
    Update("UpdateOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "workPhoneNumber", workPhoneNumber);
        db.SetNullableInt32(command, "workFaxNumber", workFaxNumber);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdtdDtstamp", lastUpdatedDtstamp);
        db.SetNullableString(command, "workPhoneExt", workPhoneExtension);
        db.SetInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "locContForIrs", localContactCodeForIrs);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetString(
          command, "roleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "effectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      });

    entities.OfficeServiceProvider.WorkPhoneNumber = workPhoneNumber;
    entities.OfficeServiceProvider.WorkFaxNumber = workFaxNumber;
    entities.OfficeServiceProvider.DiscontinueDate = discontinueDate;
    entities.OfficeServiceProvider.LastUpdatedBy = lastUpdatedBy;
    entities.OfficeServiceProvider.LastUpdatedDtstamp = lastUpdatedDtstamp;
    entities.OfficeServiceProvider.WorkPhoneExtension = workPhoneExtension;
    entities.OfficeServiceProvider.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.OfficeServiceProvider.LocalContactCodeForIrs =
      localContactCodeForIrs;
    entities.OfficeServiceProvider.Populated = true;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private OfficeServiceProvider officeServiceProvider;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
