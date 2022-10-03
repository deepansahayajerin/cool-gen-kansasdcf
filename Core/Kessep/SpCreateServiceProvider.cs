// Program: SP_CREATE_SERVICE_PROVIDER, ID: 371454600, model: 746.
// Short name: SWE01322
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_SERVICE_PROVIDER.
/// </summary>
[Serializable]
public partial class SpCreateServiceProvider: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_SERVICE_PROVIDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateServiceProvider(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateServiceProvider.
  /// </summary>
  public SpCreateServiceProvider(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ------------------------------------------------------------------
    // 02/12/2001	M Ramirez	WR# 187		Add email address.
    // 02/14/2001      Madhu Kumar     WR#286 A   Add certification number.
    // ------------------------------------------------------------------
    local.ControlTable.Identifier = "SERVICE PROVIDER";
    UseAccessControlTable();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    try
    {
      CreateServiceProvider();
      export.ServiceProvider.SystemGeneratedId =
        entities.ServiceProvider.SystemGeneratedId;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SERVICE_PROVIDER_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SERVICE_PROVIDER_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void CreateServiceProvider()
  {
    var systemGeneratedId = local.ControlTable.LastUsedNumber;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var userId = import.ServiceProvider.UserId;
    var lastName = import.ServiceProvider.LastName;
    var firstName = import.ServiceProvider.FirstName;
    var middleInitial = import.ServiceProvider.MiddleInitial;
    var emailAddress = import.ServiceProvider.EmailAddress ?? "";
    var certificationNumber = import.ServiceProvider.CertificationNumber ?? "";
    var roleCode = import.ServiceProvider.RoleCode ?? "";
    var effectiveDate = import.ServiceProvider.EffectiveDate;
    var discontinueDate = import.ServiceProvider.DiscontinueDate;
    var phoneAreaCode =
      import.ServiceProvider.PhoneAreaCode.GetValueOrDefault();
    var phoneNumber = import.ServiceProvider.PhoneNumber.GetValueOrDefault();
    var phoneExtension =
      import.ServiceProvider.PhoneExtension.GetValueOrDefault();

    entities.ServiceProvider.Populated = false;
    Update("CreateServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "servicePrvderId", systemGeneratedId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", default(DateTime));
        db.SetString(command, "userId", userId);
        db.SetString(command, "lastName", lastName);
        db.SetString(command, "firstName", firstName);
        db.SetString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetNullableString(command, "certificationNo", certificationNumber);
        db.SetNullableString(command, "roleCode", roleCode);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableInt32(command, "phoneAreaCode", phoneAreaCode);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "phoneExtension", phoneExtension);
      });

    entities.ServiceProvider.SystemGeneratedId = systemGeneratedId;
    entities.ServiceProvider.CreatedBy = createdBy;
    entities.ServiceProvider.CreatedTimestamp = createdTimestamp;
    entities.ServiceProvider.UserId = userId;
    entities.ServiceProvider.LastName = lastName;
    entities.ServiceProvider.FirstName = firstName;
    entities.ServiceProvider.MiddleInitial = middleInitial;
    entities.ServiceProvider.EmailAddress = emailAddress;
    entities.ServiceProvider.CertificationNumber = certificationNumber;
    entities.ServiceProvider.RoleCode = roleCode;
    entities.ServiceProvider.EffectiveDate = effectiveDate;
    entities.ServiceProvider.DiscontinueDate = discontinueDate;
    entities.ServiceProvider.PhoneAreaCode = phoneAreaCode;
    entities.ServiceProvider.PhoneNumber = phoneNumber;
    entities.ServiceProvider.PhoneExtension = phoneExtension;
    entities.ServiceProvider.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
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
