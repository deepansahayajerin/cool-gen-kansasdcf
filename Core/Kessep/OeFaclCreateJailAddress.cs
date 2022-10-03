// Program: OE_FACL_CREATE_JAIL_ADDRESS, ID: 373339595, model: 746.
// Short name: SWE00732
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FACL_CREATE_JAIL_ADDRESS.
/// </summary>
[Serializable]
public partial class OeFaclCreateJailAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FACL_CREATE_JAIL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFaclCreateJailAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFaclCreateJailAddress.
  /// </summary>
  public OeFaclCreateJailAddress(IContext context, Import import, Export export):
    
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
    // selected 'Jail_Address' and DELETE the read 'Jail_Address'.
    // ----------------------------------------------------------------------
    export.JailAddresses.Assign(import.JailAddresses);
    ReadJailAddresses();
    export.JailAddresses.Identifier = entities.JailAddresses.Identifier + 1;

    try
    {
      CreateJailAddresses();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OE0000_JAIL_ADDRESS_AE";

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

  private void CreateJailAddresses()
  {
    var identifier = export.JailAddresses.Identifier;
    var street1 = export.JailAddresses.Street1 ?? "";
    var street2 = export.JailAddresses.Street2 ?? "";
    var city = export.JailAddresses.City ?? "";
    var state = export.JailAddresses.State ?? "";
    var province = export.JailAddresses.Province ?? "";
    var postalCode = export.JailAddresses.PostalCode ?? "";
    var zipCode5 = export.JailAddresses.ZipCode5 ?? "";
    var zipCode4 = export.JailAddresses.ZipCode4 ?? "";
    var zipCode3 = export.JailAddresses.ZipCode3 ?? "";
    var country = export.JailAddresses.Country ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var jailName = export.JailAddresses.JailName ?? "";
    var phone = export.JailAddresses.Phone.GetValueOrDefault();
    var phoneAreaCode = export.JailAddresses.PhoneAreaCode.GetValueOrDefault();
    var phoneExtension = export.JailAddresses.PhoneExtension ?? "";

    entities.JailAddresses.Populated = false;
    Update("CreateJailAddresses",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
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
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "jailName", jailName);
        db.SetNullableInt32(command, "phone", phone);
        db.SetNullableInt32(command, "phoneAreaCode", phoneAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
      });

    entities.JailAddresses.Identifier = identifier;
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
    entities.JailAddresses.LastUpdatedBy = createdBy;
    entities.JailAddresses.LastUpdatedTimestamp = createdTimestamp;
    entities.JailAddresses.JailName = jailName;
    entities.JailAddresses.Phone = phone;
    entities.JailAddresses.PhoneAreaCode = phoneAreaCode;
    entities.JailAddresses.PhoneExtension = phoneExtension;
    entities.JailAddresses.Populated = true;
  }

  private bool ReadJailAddresses()
  {
    entities.JailAddresses.Populated = false;

    return Read("ReadJailAddresses",
      null,
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
