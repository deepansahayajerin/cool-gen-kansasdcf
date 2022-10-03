// Program: LE_UPDATE_ADMIN_APPEAL_ADDRESS, ID: 372576543, model: 746.
// Short name: SWE00826
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_UPDATE_ADMIN_APPEAL_ADDRESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block updates the Administrative Appeal Address.
/// </para>
/// </summary>
[Serializable]
public partial class LeUpdateAdminAppealAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_ADMIN_APPEAL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateAdminAppealAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateAdminAppealAddress.
  /// </summary>
  public LeUpdateAdminAppealAddress(IContext context, Import import,
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
    // ************************************************************
    // 03/24/98	Siraj Konkader		ZDEL cleanup
    // ************************************************************
    if (!ReadAdminAppealAppellantAddress())
    {
      ExitState = "ADMINISTRATIVE_APPEAL_ADDRES_NF";

      return;
    }

    try
    {
      UpdateAdminAppealAppellantAddress();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "ADMINISTRATIVE_APPEAL_ADDRES_NU";

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

  private bool ReadAdminAppealAppellantAddress()
  {
    entities.AdminAppealAppellantAddress.Populated = false;

    return Read("ReadAdminAppealAppellantAddress",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdminAppealAppellantAddress.Type1);
        db.SetInt32(
          command, "aapIdentifier", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdminAppealAppellantAddress.AapIdentifier =
          db.GetInt32(reader, 0);
        entities.AdminAppealAppellantAddress.Type1 = db.GetString(reader, 1);
        entities.AdminAppealAppellantAddress.Street1 = db.GetString(reader, 2);
        entities.AdminAppealAppellantAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.AdminAppealAppellantAddress.City = db.GetString(reader, 4);
        entities.AdminAppealAppellantAddress.StateProvince =
          db.GetString(reader, 5);
        entities.AdminAppealAppellantAddress.Country =
          db.GetNullableString(reader, 6);
        entities.AdminAppealAppellantAddress.PostalCode =
          db.GetNullableString(reader, 7);
        entities.AdminAppealAppellantAddress.ZipCode = db.GetString(reader, 8);
        entities.AdminAppealAppellantAddress.Zip4 =
          db.GetNullableString(reader, 9);
        entities.AdminAppealAppellantAddress.Zip3 =
          db.GetNullableString(reader, 10);
        entities.AdminAppealAppellantAddress.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.AdminAppealAppellantAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.AdminAppealAppellantAddress.Populated = true;
      });
  }

  private void UpdateAdminAppealAppellantAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdminAppealAppellantAddress.Populated);

    var street1 = import.AdminAppealAppellantAddress.Street1;
    var street2 = import.AdminAppealAppellantAddress.Street2 ?? "";
    var city = import.AdminAppealAppellantAddress.City;
    var stateProvince = import.AdminAppealAppellantAddress.StateProvince;
    var country = import.AdminAppealAppellantAddress.Country ?? "";
    var postalCode = import.AdminAppealAppellantAddress.PostalCode ?? "";
    var zipCode = import.AdminAppealAppellantAddress.ZipCode;
    var zip4 = import.AdminAppealAppellantAddress.Zip4 ?? "";
    var zip3 = import.AdminAppealAppellantAddress.Zip3 ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.AdminAppealAppellantAddress.Populated = false;
    Update("UpdateAdminAppealAppellantAddress",
      (db, command) =>
      {
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "stateProvince", stateProvince);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "postalCd", postalCode);
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(
          command, "aapIdentifier",
          entities.AdminAppealAppellantAddress.AapIdentifier);
        db.
          SetString(command, "type", entities.AdminAppealAppellantAddress.Type1);
          
      });

    entities.AdminAppealAppellantAddress.Street1 = street1;
    entities.AdminAppealAppellantAddress.Street2 = street2;
    entities.AdminAppealAppellantAddress.City = city;
    entities.AdminAppealAppellantAddress.StateProvince = stateProvince;
    entities.AdminAppealAppellantAddress.Country = country;
    entities.AdminAppealAppellantAddress.PostalCode = postalCode;
    entities.AdminAppealAppellantAddress.ZipCode = zipCode;
    entities.AdminAppealAppellantAddress.Zip4 = zip4;
    entities.AdminAppealAppellantAddress.Zip3 = zip3;
    entities.AdminAppealAppellantAddress.LastUpdatedBy = lastUpdatedBy;
    entities.AdminAppealAppellantAddress.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.AdminAppealAppellantAddress.Populated = true;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AdminAppealAppellantAddress.
    /// </summary>
    [JsonPropertyName("adminAppealAppellantAddress")]
    public AdminAppealAppellantAddress AdminAppealAppellantAddress
    {
      get => adminAppealAppellantAddress ??= new();
      set => adminAppealAppellantAddress = value;
    }

    private AdministrativeAppeal administrativeAppeal;
    private AdminAppealAppellantAddress adminAppealAppellantAddress;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AdminAppealAppellantAddress.
    /// </summary>
    [JsonPropertyName("adminAppealAppellantAddress")]
    public AdminAppealAppellantAddress AdminAppealAppellantAddress
    {
      get => adminAppealAppellantAddress ??= new();
      set => adminAppealAppellantAddress = value;
    }

    private AdministrativeAppeal administrativeAppeal;
    private AdminAppealAppellantAddress adminAppealAppellantAddress;
  }
#endregion
}
