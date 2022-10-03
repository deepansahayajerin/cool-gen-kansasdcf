// Program: OE_FACL_UPDATE_JAIL_ADDRESSES, ID: 373339597, model: 746.
// Short name: SWE00733
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_FACL_UPDATE_JAIL_ADDRESSES.
/// </para>
/// <para>
/// This action block will update a jail_address in CKT_Jail_Addresses table.
/// </para>
/// </summary>
[Serializable]
public partial class OeFaclUpdateJailAddresses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FACL_UPDATE_JAIL_ADDRESSES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFaclUpdateJailAddresses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFaclUpdateJailAddresses.
  /// </summary>
  public OeFaclUpdateJailAddresses(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // 04/04/2002             Vithal Madhira              PR# 140102,140103
    // Initial Development.
    // This CAB will be used only in FACL PSTEP. This CAB is used to READ a 
    // selected 'Jail_Address' and UPDATE the read 'Jail_Address'.
    // ----------------------------------------------------------------------
    if (ReadJailAddresses())
    {
      try
      {
        UpdateJailAddresses();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0000_JAIL_ADDRESS_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OE0000_JAIL_ADDRESS_PV";

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
      ExitState = "OE0000_JAIL_ADDRESS_NF";
    }
  }

  private bool ReadJailAddresses()
  {
    entities.JailAddresses.Populated = false;

    return Read("ReadJailAddresses",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.JailAddresses.Identifier);
      },
      (db, reader) =>
      {
        entities.JailAddresses.Identifier = db.GetInt32(reader, 0);
        entities.JailAddresses.Street1 = db.GetNullableString(reader, 1);
        entities.JailAddresses.Street2 = db.GetNullableString(reader, 2);
        entities.JailAddresses.City = db.GetNullableString(reader, 3);
        entities.JailAddresses.State = db.GetNullableString(reader, 4);
        entities.JailAddresses.Province = db.GetNullableString(reader, 5);
        entities.JailAddresses.PostalCode = db.GetNullableString(reader, 6);
        entities.JailAddresses.ZipCode5 = db.GetNullableString(reader, 7);
        entities.JailAddresses.ZipCode4 = db.GetNullableString(reader, 8);
        entities.JailAddresses.ZipCode3 = db.GetNullableString(reader, 9);
        entities.JailAddresses.Country = db.GetNullableString(reader, 10);
        entities.JailAddresses.CreatedBy = db.GetString(reader, 11);
        entities.JailAddresses.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.JailAddresses.LastUpdatedBy = db.GetString(reader, 13);
        entities.JailAddresses.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.JailAddresses.JailName = db.GetNullableString(reader, 15);
        entities.JailAddresses.Phone = db.GetNullableInt32(reader, 16);
        entities.JailAddresses.PhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.JailAddresses.PhoneExtension =
          db.GetNullableString(reader, 18);
        entities.JailAddresses.Populated = true;
      });
  }

  private void UpdateJailAddresses()
  {
    var street1 = import.JailAddresses.Street1 ?? "";
    var street2 = import.JailAddresses.Street2 ?? "";
    var city = import.JailAddresses.City ?? "";
    var state = import.JailAddresses.State ?? "";
    var province = import.JailAddresses.Province ?? "";
    var postalCode = import.JailAddresses.PostalCode ?? "";
    var zipCode5 = import.JailAddresses.ZipCode5 ?? "";
    var zipCode4 = import.JailAddresses.ZipCode4 ?? "";
    var zipCode3 = import.JailAddresses.ZipCode3 ?? "";
    var country = import.JailAddresses.Country ?? "";
    var createdBy = import.JailAddresses.CreatedBy;
    var createdTimestamp = import.JailAddresses.CreatedTimestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var jailName = import.JailAddresses.JailName ?? "";
    var phone = import.JailAddresses.Phone.GetValueOrDefault();
    var phoneAreaCode = import.JailAddresses.PhoneAreaCode.GetValueOrDefault();
    var phoneExtension = import.JailAddresses.PhoneExtension ?? "";

    entities.JailAddresses.Populated = false;
    Update("UpdateJailAddresses",
      (db, command) =>
      {
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "jailName", jailName);
        db.SetNullableInt32(command, "phone", phone);
        db.SetNullableInt32(command, "phoneAreaCode", phoneAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
        db.SetInt32(command, "identifier", entities.JailAddresses.Identifier);
      });

    entities.JailAddresses.Street1 = street1;
    entities.JailAddresses.Street2 = street2;
    entities.JailAddresses.City = city;
    entities.JailAddresses.State = state;
    entities.JailAddresses.Province = province;
    entities.JailAddresses.PostalCode = postalCode;
    entities.JailAddresses.ZipCode5 = zipCode5;
    entities.JailAddresses.ZipCode4 = zipCode4;
    entities.JailAddresses.ZipCode3 = zipCode3;
    entities.JailAddresses.Country = country;
    entities.JailAddresses.CreatedBy = createdBy;
    entities.JailAddresses.CreatedTimestamp = createdTimestamp;
    entities.JailAddresses.LastUpdatedBy = lastUpdatedBy;
    entities.JailAddresses.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.JailAddresses.JailName = jailName;
    entities.JailAddresses.Phone = phone;
    entities.JailAddresses.PhoneAreaCode = phoneAreaCode;
    entities.JailAddresses.PhoneExtension = phoneExtension;
    entities.JailAddresses.Populated = true;
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
    /// A value of JailAddresses.
    /// </summary>
    [JsonPropertyName("jailAddresses")]
    public JailAddresses JailAddresses
    {
      get => jailAddresses ??= new();
      set => jailAddresses = value;
    }

    private JailAddresses jailAddresses;
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
    /// A value of JailAddresses.
    /// </summary>
    [JsonPropertyName("jailAddresses")]
    public JailAddresses JailAddresses
    {
      get => jailAddresses ??= new();
      set => jailAddresses = value;
    }

    private JailAddresses jailAddresses;
  }
#endregion
}
