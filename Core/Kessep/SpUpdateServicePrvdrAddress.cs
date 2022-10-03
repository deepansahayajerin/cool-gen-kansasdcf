// Program: SP_UPDATE_SERVICE_PRVDR_ADDRESS, ID: 371454601, model: 746.
// Short name: SWE01449
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_SERVICE_PRVDR_ADDRESS.
/// </summary>
[Serializable]
public partial class SpUpdateServicePrvdrAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_SERVICE_PRVDR_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateServicePrvdrAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateServicePrvdrAddress.
  /// </summary>
  public SpUpdateServicePrvdrAddress(IContext context, Import import,
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
    if (!ReadServiceProviderAddress())
    {
      ExitState = "SERVICE_PROVIDER_ADDRESS_NF";

      return;
    }

    try
    {
      UpdateServiceProviderAddress();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SERVICE_PROVIDER_ADDRESS_NU";

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

  private bool ReadServiceProviderAddress()
  {
    entities.ServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetString(command, "type", import.ServiceProviderAddress.Type1);
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderAddress.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ServiceProviderAddress.Street1 = db.GetString(reader, 2);
        entities.ServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ServiceProviderAddress.StateProvince = db.GetString(reader, 5);
        entities.ServiceProviderAddress.PostalCode =
          db.GetNullableString(reader, 6);
        entities.ServiceProviderAddress.Zip = db.GetNullableString(reader, 7);
        entities.ServiceProviderAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.ServiceProviderAddress.Country = db.GetString(reader, 9);
        entities.ServiceProviderAddress.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.ServiceProviderAddress.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 11);
        entities.ServiceProviderAddress.Populated = true;
      });
  }

  private void UpdateServiceProviderAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ServiceProviderAddress.Populated);

    var street1 = import.ServiceProviderAddress.Street1;
    var street2 = import.ServiceProviderAddress.Street2 ?? "";
    var city = import.ServiceProviderAddress.City;
    var stateProvince = import.ServiceProviderAddress.StateProvince;
    var postalCode = import.ServiceProviderAddress.PostalCode ?? "";
    var zip = import.ServiceProviderAddress.Zip ?? "";
    var zip4 = import.ServiceProviderAddress.Zip4 ?? "";
    var country = import.ServiceProviderAddress.Country;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();

    entities.ServiceProviderAddress.Populated = false;
    Update("UpdateServiceProviderAddress",
      (db, command) =>
      {
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "stateProvince", stateProvince);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zip", zip);
        db.SetNullableString(command, "zip4", zip4);
        db.SetString(command, "country", country);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProviderAddress.SpdGeneratedId);
        db.SetString(command, "type", entities.ServiceProviderAddress.Type1);
      });

    entities.ServiceProviderAddress.Street1 = street1;
    entities.ServiceProviderAddress.Street2 = street2;
    entities.ServiceProviderAddress.City = city;
    entities.ServiceProviderAddress.StateProvince = stateProvince;
    entities.ServiceProviderAddress.PostalCode = postalCode;
    entities.ServiceProviderAddress.Zip = zip;
    entities.ServiceProviderAddress.Zip4 = zip4;
    entities.ServiceProviderAddress.Country = country;
    entities.ServiceProviderAddress.LastUpdatedBy = lastUpdatedBy;
    entities.ServiceProviderAddress.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.ServiceProviderAddress.Populated = true;
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

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
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

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private ServiceProvider serviceProvider;
    private ServiceProviderAddress serviceProviderAddress;
  }
#endregion
}
