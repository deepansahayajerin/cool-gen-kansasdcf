// Program: LE_CAB_UPDATE_FIPS_TRIB_ADDRESS, ID: 372019605, model: 746.
// Short name: SWE01935
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_UPDATE_FIPS_TRIB_ADDRESS.
/// </para>
/// <para>
/// This common action block updates the FIPS/Tribunal Address table from an 
/// import view of all attributes.  All attributes must be loaded, as they will
/// be updated to the import view.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabUpdateFipsTribAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_UPDATE_FIPS_TRIB_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabUpdateFipsTribAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabUpdateFipsTribAddress.
  /// </summary>
  public LeCabUpdateFipsTribAddress(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 10/08/98	D. JEAN			        Remove unused and persistent views
    // ------------------------------------------------------------
    if (ReadFipsTribAddress())
    {
      if (IsEmpty(import.ForUpdate.Street1) && IsEmpty
        (import.ForUpdate.Street2) && IsEmpty(import.ForUpdate.Street3) && IsEmpty
        (import.ForUpdate.Street4) && IsEmpty(import.ForUpdate.City) && IsEmpty
        (import.ForUpdate.ZipCode) && IsEmpty(import.ForUpdate.Zip4) && IsEmpty
        (import.ForUpdate.Zip3) && import
        .ForUpdate.AreaCode.GetValueOrDefault() == 0 && import
        .ForUpdate.PhoneNumber.GetValueOrDefault() == 0 && import
        .ForUpdate.FaxAreaCode.GetValueOrDefault() == 0 && import
        .ForUpdate.FaxNumber.GetValueOrDefault() == 0 && IsEmpty
        (import.ForUpdate.PhoneExtension) && IsEmpty
        (import.ForUpdate.FaxExtension))
      {
        DeleteFipsTribAddress();

        return;
      }

      try
      {
        UpdateFipsTribAddress();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_FIPS_TRIB_ADDR_NU";

            break;
          case ErrorCode.PermittedValueViolation:
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
      ExitState = "CO0000_FIPS_TRIB_ADDR_NF_DB_ERRR";
    }
  }

  private void DeleteFipsTribAddress()
  {
    Update("DeleteFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.ForUpdate.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.City = db.GetString(reader, 8);
        entities.FipsTribAddress.State = db.GetString(reader, 9);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 15);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 16);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 17);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 18);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 19);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private void UpdateFipsTribAddress()
  {
    var faxExtension = import.ForUpdate.FaxExtension ?? "";
    var faxAreaCode = import.ForUpdate.FaxAreaCode.GetValueOrDefault();
    var phoneExtension = import.ForUpdate.PhoneExtension ?? "";
    var areaCode = import.ForUpdate.AreaCode.GetValueOrDefault();
    var type1 = import.ForUpdate.Type1;
    var street1 = import.ForUpdate.Street1;
    var street2 = import.ForUpdate.Street2 ?? "";
    var city = import.ForUpdate.City;
    var state = import.ForUpdate.State;
    var zipCode = import.ForUpdate.ZipCode;
    var zip4 = import.ForUpdate.Zip4 ?? "";
    var zip3 = import.ForUpdate.Zip3 ?? "";
    var county = import.ForUpdate.County ?? "";
    var street3 = import.ForUpdate.Street3 ?? "";
    var street4 = import.ForUpdate.Street4 ?? "";
    var province = import.ForUpdate.Province ?? "";
    var postalCode = import.ForUpdate.PostalCode ?? "";
    var country = import.ForUpdate.Country ?? "";
    var phoneNumber = import.ForUpdate.PhoneNumber.GetValueOrDefault();
    var faxNumber = import.ForUpdate.FaxNumber.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.FipsTribAddress.Populated = false;
    Update("UpdateFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "faxExtension", faxExtension);
        db.SetNullableInt32(command, "faxAreaCd", faxAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
        db.SetNullableInt32(command, "areaCd", areaCode);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });

    entities.FipsTribAddress.FaxExtension = faxExtension;
    entities.FipsTribAddress.FaxAreaCode = faxAreaCode;
    entities.FipsTribAddress.PhoneExtension = phoneExtension;
    entities.FipsTribAddress.AreaCode = areaCode;
    entities.FipsTribAddress.Type1 = type1;
    entities.FipsTribAddress.Street1 = street1;
    entities.FipsTribAddress.Street2 = street2;
    entities.FipsTribAddress.City = city;
    entities.FipsTribAddress.State = state;
    entities.FipsTribAddress.ZipCode = zipCode;
    entities.FipsTribAddress.Zip4 = zip4;
    entities.FipsTribAddress.Zip3 = zip3;
    entities.FipsTribAddress.County = county;
    entities.FipsTribAddress.Street3 = street3;
    entities.FipsTribAddress.Street4 = street4;
    entities.FipsTribAddress.Province = province;
    entities.FipsTribAddress.PostalCode = postalCode;
    entities.FipsTribAddress.Country = country;
    entities.FipsTribAddress.PhoneNumber = phoneNumber;
    entities.FipsTribAddress.FaxNumber = faxNumber;
    entities.FipsTribAddress.LastUpdatedBy = lastUpdatedBy;
    entities.FipsTribAddress.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.FipsTribAddress.Populated = true;
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
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public FipsTribAddress ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    private FipsTribAddress forUpdate;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    private FipsTribAddress fipsTribAddress;
  }
#endregion
}
