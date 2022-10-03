// Program: LE_CREATE_ADMIN_APPEAL_ADDRESS, ID: 372576545, model: 746.
// Short name: SWE00735
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CREATE_ADMIN_APPEAL_ADDRESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates the Administrative Appeal Address.
/// </para>
/// </summary>
[Serializable]
public partial class LeCreateAdminAppealAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_ADMIN_APPEAL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateAdminAppealAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateAdminAppealAddress.
  /// </summary>
  public LeCreateAdminAppealAddress(IContext context, Import import,
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
    if (!ReadAdministrativeAppeal())
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }

    try
    {
      CreateAdminAppealAppellantAddress();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "ADMINISTRATIVE_APPEAL_ADDRES_AE";

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

  private void CreateAdminAppealAppellantAddress()
  {
    var aapIdentifier = entities.AdministrativeAppeal.Identifier;
    var type1 = import.AdminAppealAppellantAddress.Type1;
    var street1 = import.AdminAppealAppellantAddress.Street1;
    var street2 = import.AdminAppealAppellantAddress.Street2 ?? "";
    var city = import.AdminAppealAppellantAddress.City;
    var stateProvince = import.AdminAppealAppellantAddress.StateProvince;
    var country = import.AdminAppealAppellantAddress.Country ?? "";
    var postalCode = import.AdminAppealAppellantAddress.PostalCode ?? "";
    var zipCode = import.AdminAppealAppellantAddress.ZipCode;
    var zip4 = import.AdminAppealAppellantAddress.Zip4 ?? "";
    var zip3 = import.AdminAppealAppellantAddress.Zip3 ?? "";
    var createdBy = global.UserId;
    var createdTstamp = Now();

    entities.AdminAppealAppellantAddress.Populated = false;
    Update("CreateAdminAppealAppellantAddress",
      (db, command) =>
      {
        db.SetInt32(command, "aapIdentifier", aapIdentifier);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "stateProvince", stateProvince);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "postalCd", postalCode);
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
      });

    entities.AdminAppealAppellantAddress.AapIdentifier = aapIdentifier;
    entities.AdminAppealAppellantAddress.Type1 = type1;
    entities.AdminAppealAppellantAddress.Street1 = street1;
    entities.AdminAppealAppellantAddress.Street2 = street2;
    entities.AdminAppealAppellantAddress.City = city;
    entities.AdminAppealAppellantAddress.StateProvince = stateProvince;
    entities.AdminAppealAppellantAddress.Country = country;
    entities.AdminAppealAppellantAddress.PostalCode = postalCode;
    entities.AdminAppealAppellantAddress.ZipCode = zipCode;
    entities.AdminAppealAppellantAddress.Zip4 = zip4;
    entities.AdminAppealAppellantAddress.Zip3 = zip3;
    entities.AdminAppealAppellantAddress.CreatedBy = createdBy;
    entities.AdminAppealAppellantAddress.CreatedTstamp = createdTstamp;
    entities.AdminAppealAppellantAddress.Populated = true;
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Populated = true;
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
    /// A value of AdminAppealAppellantAddress.
    /// </summary>
    [JsonPropertyName("adminAppealAppellantAddress")]
    public AdminAppealAppellantAddress AdminAppealAppellantAddress
    {
      get => adminAppealAppellantAddress ??= new();
      set => adminAppealAppellantAddress = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private AdminAppealAppellantAddress adminAppealAppellantAddress;
    private AdministrativeAppeal administrativeAppeal;
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
    /// A value of AdminAppealAppellantAddress.
    /// </summary>
    [JsonPropertyName("adminAppealAppellantAddress")]
    public AdminAppealAppellantAddress AdminAppealAppellantAddress
    {
      get => adminAppealAppellantAddress ??= new();
      set => adminAppealAppellantAddress = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private AdminAppealAppellantAddress adminAppealAppellantAddress;
    private AdministrativeAppeal administrativeAppeal;
  }
#endregion
}
