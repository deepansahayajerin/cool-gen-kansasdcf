// Program: SP_UPDATE_OFFICE_ADDRESS, ID: 371782200, model: 746.
// Short name: SWE01442
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_OFFICE_ADDRESS.
/// </summary>
[Serializable]
public partial class SpUpdateOfficeAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_OFFICE_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateOfficeAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateOfficeAddress.
  /// </summary>
  public SpUpdateOfficeAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadOfficeAddress())
    {
      ExitState = "OFFICE_ADDRESS_NF";

      return;
    }

    try
    {
      UpdateOfficeAddress();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OFFICE_ADDRESS_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OFFICE_ADDRESS_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadOfficeAddress()
  {
    entities.OfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.SetString(command, "type", import.OfficeAddress.Type1);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAddress.Type1 = db.GetString(reader, 1);
        entities.OfficeAddress.Street1 = db.GetString(reader, 2);
        entities.OfficeAddress.Street2 = db.GetNullableString(reader, 3);
        entities.OfficeAddress.City = db.GetString(reader, 4);
        entities.OfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.OfficeAddress.PostalCode = db.GetNullableString(reader, 6);
        entities.OfficeAddress.Zip = db.GetNullableString(reader, 7);
        entities.OfficeAddress.Zip4 = db.GetNullableString(reader, 8);
        entities.OfficeAddress.Country = db.GetString(reader, 9);
        entities.OfficeAddress.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.OfficeAddress.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 11);
        entities.OfficeAddress.Populated = true;
      });
  }

  private void UpdateOfficeAddress()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeAddress.Populated);

    var street1 = import.OfficeAddress.Street1;
    var street2 = import.OfficeAddress.Street2 ?? "";
    var city = import.OfficeAddress.City;
    var stateProvince = import.OfficeAddress.StateProvince;
    var postalCode = import.OfficeAddress.PostalCode ?? "";
    var zip = import.OfficeAddress.Zip ?? "";
    var zip4 = import.OfficeAddress.Zip4 ?? "";
    var country = import.OfficeAddress.Country;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();

    entities.OfficeAddress.Populated = false;
    Update("UpdateOfficeAddress",
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
          command, "offGeneratedId", entities.OfficeAddress.OffGeneratedId);
        db.SetString(command, "type", entities.OfficeAddress.Type1);
      });

    entities.OfficeAddress.Street1 = street1;
    entities.OfficeAddress.Street2 = street2;
    entities.OfficeAddress.City = city;
    entities.OfficeAddress.StateProvince = stateProvince;
    entities.OfficeAddress.PostalCode = postalCode;
    entities.OfficeAddress.Zip = zip;
    entities.OfficeAddress.Zip4 = zip4;
    entities.OfficeAddress.Country = country;
    entities.OfficeAddress.LastUpdatedBy = lastUpdatedBy;
    entities.OfficeAddress.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.OfficeAddress.Populated = true;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private Office office;
    private OfficeAddress officeAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private OfficeAddress officeAddress;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    private CseOrganization cseOrganization;
    private Office office;
    private OfficeAddress officeAddress;
  }
#endregion
}
