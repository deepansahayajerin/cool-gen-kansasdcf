// Program: OE_VEND_CREATE_VENDOR_ADDRESS, ID: 371796238, model: 746.
// Short name: SWE01532
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_VEND_CREATE_VENDOR_ADDRESS.
/// </para>
/// <para>
/// This action block creates Vendor-Address.
/// Keeps history of Vendor-Address. Allows only one effective Vendor-Address 
/// for the time frame.  At initial creation time expiry-date is set to 2999-12-
/// 31.  When Vendor changes address, expiry-date is update to new address
/// effective-date - 1 day, so effective-date and
/// previous address expiry-date is always consecutive date.  State is validated
/// with Code-Value entity.
/// </para>
/// </summary>
[Serializable]
public partial class OeVendCreateVendorAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VEND_CREATE_VENDOR_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeVendCreateVendorAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeVendCreateVendorAddress.
  /// </summary>
  public OeVendCreateVendorAddress(IContext context, Import import,
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
    if (!ReadVendor())
    {
      ExitState = "OE0000_NF_VENDOR";

      return;
    }

    try
    {
      CreateVendorAddress();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "RECORD_AE";

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

  private void CreateVendorAddress()
  {
    var venIdentifier = entities.Vendor.Identifier;
    var effectiveDate = import.VendorAddress.EffectiveDate;
    var expiryDate = import.VendorAddress.ExpiryDate;
    var street1 = import.VendorAddress.Street1 ?? "";
    var street2 = import.VendorAddress.Street2 ?? "";
    var city = import.VendorAddress.City ?? "";
    var state = import.VendorAddress.State ?? "";
    var zipCode5 = import.VendorAddress.ZipCode5 ?? "";
    var zipCode4 = import.VendorAddress.ZipCode4 ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.VendorAddress.Populated = false;
    Update("CreateVendorAddress",
      (db, command) =>
      {
        db.SetInt32(command, "venIdentifier", venIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "expiryDate", expiryDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "country", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.VendorAddress.VenIdentifier = venIdentifier;
    entities.VendorAddress.EffectiveDate = effectiveDate;
    entities.VendorAddress.ExpiryDate = expiryDate;
    entities.VendorAddress.Street1 = street1;
    entities.VendorAddress.Street2 = street2;
    entities.VendorAddress.City = city;
    entities.VendorAddress.State = state;
    entities.VendorAddress.ZipCode5 = zipCode5;
    entities.VendorAddress.ZipCode4 = zipCode4;
    entities.VendorAddress.CreatedBy = createdBy;
    entities.VendorAddress.CreatedTimestamp = createdTimestamp;
    entities.VendorAddress.LastUpdatedBy = "";
    entities.VendorAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.VendorAddress.Populated = true;
  }

  private bool ReadVendor()
  {
    entities.Vendor.Populated = false;

    return Read("ReadVendor",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.New1.Identifier);
      },
      (db, reader) =>
      {
        entities.Vendor.Identifier = db.GetInt32(reader, 0);
        entities.Vendor.Populated = true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Vendor New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
    }

    private Vendor new1;
    private VendorAddress vendorAddress;
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
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
    }

    /// <summary>
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
    }

    private Vendor vendor;
    private VendorAddress vendorAddress;
  }
#endregion
}
