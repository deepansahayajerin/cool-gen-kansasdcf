// Program: SP_CREATE_SERVICE_PRVDR_ADDRESS, ID: 371454602, model: 746.
// Short name: SWE01323
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_SERVICE_PRVDR_ADDRESS.
/// </summary>
[Serializable]
public partial class SpCreateServicePrvdrAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_SERVICE_PRVDR_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateServicePrvdrAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateServicePrvdrAddress.
  /// </summary>
  public SpCreateServicePrvdrAddress(IContext context, Import import,
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
    if (!ReadServiceProvider())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    try
    {
      CreateServiceProviderAddress();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SERVICE_PROVIDER_ADDRESS_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SERVICE_PROVIDER_ADDRESS_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateServiceProviderAddress()
  {
    var spdGeneratedId = entities.ServiceProvider.SystemGeneratedId;
    var type1 = import.ServiceProviderAddress.Type1;
    var street1 = import.ServiceProviderAddress.Street1;
    var street2 = import.ServiceProviderAddress.Street2 ?? "";
    var city = import.ServiceProviderAddress.City;
    var stateProvince = import.ServiceProviderAddress.StateProvince;
    var postalCode = import.ServiceProviderAddress.PostalCode ?? "";
    var zip = import.ServiceProviderAddress.Zip ?? "";
    var zip4 = import.ServiceProviderAddress.Zip4 ?? "";
    var country = import.ServiceProviderAddress.Country;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.ServiceProviderAddress.Populated = false;
    Update("CreateServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "stateProvince", stateProvince);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zip", zip);
        db.SetNullableString(command, "zip4", zip4);
        db.SetString(command, "country", country);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetNullableString(command, "zip3", "");
      });

    entities.ServiceProviderAddress.SpdGeneratedId = spdGeneratedId;
    entities.ServiceProviderAddress.Type1 = type1;
    entities.ServiceProviderAddress.Street1 = street1;
    entities.ServiceProviderAddress.Street2 = street2;
    entities.ServiceProviderAddress.City = city;
    entities.ServiceProviderAddress.StateProvince = stateProvince;
    entities.ServiceProviderAddress.PostalCode = postalCode;
    entities.ServiceProviderAddress.Zip = zip;
    entities.ServiceProviderAddress.Zip4 = zip4;
    entities.ServiceProviderAddress.Country = country;
    entities.ServiceProviderAddress.CreatedBy = createdBy;
    entities.ServiceProviderAddress.CreatedTimestamp = createdTimestamp;
    entities.ServiceProviderAddress.LastUpdatedBy = "";
    entities.ServiceProviderAddress.LastUpdatdTstamp = null;
    entities.ServiceProviderAddress.Populated = true;
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
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

    private ServiceProviderAddress serviceProviderAddress;
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
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
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

    private ServiceProviderAddress serviceProviderAddress;
    private ServiceProvider serviceProvider;
  }
#endregion
}
