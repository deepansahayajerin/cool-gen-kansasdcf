// Program: SP_CREATE_OFFICE_ADDRESS, ID: 371781782, model: 746.
// Short name: SWE01312
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_OFFICE_ADDRESS.
/// </summary>
[Serializable]
public partial class SpCreateOfficeAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OFFICE_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOfficeAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOfficeAddress.
  /// </summary>
  public SpCreateOfficeAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadOffice())
    {
      ExitState = "FN0000_OFFICE_NF";

      return;
    }

    try
    {
      CreateOfficeAddress();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OFFICE_ADDRESS_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OFFICE_ADDRESS_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private void CreateOfficeAddress()
  {
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var type1 = import.OfficeAddress.Type1;
    var street1 = import.OfficeAddress.Street1;
    var street2 = import.OfficeAddress.Street2 ?? "";
    var city = import.OfficeAddress.City;
    var stateProvince = import.OfficeAddress.StateProvince;
    var postalCode = import.OfficeAddress.PostalCode ?? "";
    var zip = import.OfficeAddress.Zip ?? "";
    var zip4 = import.OfficeAddress.Zip4 ?? "";
    var country = import.OfficeAddress.Country;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.OfficeAddress.Populated = false;
    Update("CreateOfficeAddress",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", offGeneratedId);
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
        db.SetNullableDateTime(command, "lastUpdatdTstamp", default(DateTime));
        db.SetNullableString(command, "zip3", "");
      });

    entities.OfficeAddress.OffGeneratedId = offGeneratedId;
    entities.OfficeAddress.Type1 = type1;
    entities.OfficeAddress.Street1 = street1;
    entities.OfficeAddress.Street2 = street2;
    entities.OfficeAddress.City = city;
    entities.OfficeAddress.StateProvince = stateProvince;
    entities.OfficeAddress.PostalCode = postalCode;
    entities.OfficeAddress.Zip = zip;
    entities.OfficeAddress.Zip4 = zip4;
    entities.OfficeAddress.Country = country;
    entities.OfficeAddress.CreatedBy = createdBy;
    entities.OfficeAddress.CreatedTimestamp = createdTimestamp;
    entities.OfficeAddress.Populated = true;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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

    private OfficeAddress officeAddress;
    private Office office;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
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

    private OfficeAddress officeAddress;
    private Office office;
  }
#endregion
}
