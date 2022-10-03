// Program: LE_DELETE_ADMIN_APPEAL_ADDRESS, ID: 372576542, model: 746.
// Short name: SWE00750
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_ADMIN_APPEAL_ADDRESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block deletes Administrative Appeal Address.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteAdminAppealAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_ADMIN_APPEAL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteAdminAppealAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteAdminAppealAddress.
  /// </summary>
  public LeDeleteAdminAppealAddress(IContext context, Import import,
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
    if (ReadAdminAppealAppellantAddress())
    {
      DeleteAdminAppealAppellantAddress();
    }
    else
    {
      ExitState = "ADMINISTRATIVE_APPEAL_ADDRES_NF";
    }
  }

  private void DeleteAdminAppealAppellantAddress()
  {
    Update("DeleteAdminAppealAppellantAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "aapIdentifier",
          entities.AdminAppealAppellantAddress.AapIdentifier);
        db.
          SetString(command, "type", entities.AdminAppealAppellantAddress.Type1);
          
      });
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
        entities.AdminAppealAppellantAddress.CreatedBy =
          db.GetString(reader, 11);
        entities.AdminAppealAppellantAddress.CreatedTstamp =
          db.GetDateTime(reader, 12);
        entities.AdminAppealAppellantAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.AdminAppealAppellantAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 14);
        entities.AdminAppealAppellantAddress.Populated = true;
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
