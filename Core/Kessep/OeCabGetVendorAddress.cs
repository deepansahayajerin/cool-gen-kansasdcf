// Program: OE_CAB_GET_VENDOR_ADDRESS, ID: 371794222, model: 746.
// Short name: SWE00874
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CAB_GET_VENDOR_ADDRESS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block returns the latest address for the vendor.
/// </para>
/// </summary>
[Serializable]
public partial class OeCabGetVendorAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_GET_VENDOR_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabGetVendorAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabGetVendorAddress.
  /// </summary>
  public OeCabGetVendorAddress(IContext context, Import import, Export export):
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
      ExitState = "FN0000_VENDOR_NF";

      return;
    }

    if (ReadVendorAddress())
    {
      export.VendorAddress.Assign(entities.ExistingVendorAddress);
    }
  }

  private bool ReadVendor()
  {
    entities.ExistingVendor.Populated = false;

    return Read("ReadVendor",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Vendor.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingVendor.Populated = true;
      });
  }

  private bool ReadVendorAddress()
  {
    entities.ExistingVendorAddress.Populated = false;

    return Read("ReadVendorAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "venIdentifier", entities.ExistingVendor.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingVendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.ExistingVendorAddress.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingVendorAddress.ExpiryDate = db.GetDate(reader, 2);
        entities.ExistingVendorAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingVendorAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingVendorAddress.City = db.GetNullableString(reader, 5);
        entities.ExistingVendorAddress.State = db.GetNullableString(reader, 6);
        entities.ExistingVendorAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingVendorAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingVendorAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingVendorAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingVendorAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.ExistingVendorAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingVendorAddress.CreatedBy = db.GetString(reader, 13);
        entities.ExistingVendorAddress.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingVendorAddress.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingVendorAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingVendorAddress.Populated = true;
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
    /// A value of Vendor.
    /// </summary>
    [JsonPropertyName("vendor")]
    public Vendor Vendor
    {
      get => vendor ??= new();
      set => vendor = value;
    }

    private Vendor vendor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of VendorAddress.
    /// </summary>
    [JsonPropertyName("vendorAddress")]
    public VendorAddress VendorAddress
    {
      get => vendorAddress ??= new();
      set => vendorAddress = value;
    }

    private VendorAddress vendorAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingVendor.
    /// </summary>
    [JsonPropertyName("existingVendor")]
    public Vendor ExistingVendor
    {
      get => existingVendor ??= new();
      set => existingVendor = value;
    }

    /// <summary>
    /// A value of ExistingVendorAddress.
    /// </summary>
    [JsonPropertyName("existingVendorAddress")]
    public VendorAddress ExistingVendorAddress
    {
      get => existingVendorAddress ??= new();
      set => existingVendorAddress = value;
    }

    private Vendor existingVendor;
    private VendorAddress existingVendorAddress;
  }
#endregion
}
