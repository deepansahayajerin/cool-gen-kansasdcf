// Program: OE_VEND_UPD_VENDOR_AND_ADDRESS, ID: 371796236, model: 746.
// Short name: SWE00974
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_VEND_UPD_VENDOR_AND_ADDRESS.
/// </para>
/// <para>
/// This action block updates two entities; vendor and vendor-address. Diagram 
/// compares each existing attributes with imported vendor attributes. In case
/// of any one or more attributes differs from existing one, vendor entity row
/// is updated.
/// And also this action block updates vendor-address. If user enters different 
/// effective-date, current effective address is updated on expiry-date with the
/// date of effective-date entered - 1 day.  Then creates new vendor-address
/// with new effective-date.  Therefore effective date of new vendor-address is
/// always consecutive with previous vendor-address expiry-date.  If user puts
/// future effective date, table may have two effective vendor-address.  So
/// diagram should choose the available effectiv vendor-address. If user leave
/// the effective-date same, then existing current effective vendor-address is
/// updated without changing expiry-date.			
/// </para>
/// </summary>
[Serializable]
public partial class OeVendUpdVendorAndAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VEND_UPD_VENDOR_AND_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeVendUpdVendorAndAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeVendUpdVendorAndAddress.
  /// </summary>
  public OeVendUpdVendorAndAddress(IContext context, Import import,
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
    local.VendorUpdate.Flag = "";

    if (ReadVendor())
    {
      try
      {
        UpdateVendor();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0000_VENDOR_ID_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (ReadVendorAddress())
      {
        try
        {
          UpdateVendorAddress();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
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
        ExitState = "OE0000_VENDOR_ADDRESS_NF";
      }
    }
    else
    {
      ExitState = "OE0000_NF_VENDOR";
    }
  }

  private bool ReadVendor()
  {
    entities.Vendor.Populated = false;

    return Read("ReadVendor",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Vendor.Identifier);
      },
      (db, reader) =>
      {
        entities.Vendor.Identifier = db.GetInt32(reader, 0);
        entities.Vendor.Name = db.GetString(reader, 1);
        entities.Vendor.Number = db.GetNullableString(reader, 2);
        entities.Vendor.PhoneNumber = db.GetNullableInt32(reader, 3);
        entities.Vendor.Fax = db.GetNullableInt32(reader, 4);
        entities.Vendor.ContactPerson = db.GetNullableString(reader, 5);
        entities.Vendor.ServiceTypeCode = db.GetNullableString(reader, 6);
        entities.Vendor.LastUpdatedBy = db.GetString(reader, 7);
        entities.Vendor.LastUpdatedTimestamp = db.GetDateTime(reader, 8);
        entities.Vendor.FaxExt = db.GetNullableString(reader, 9);
        entities.Vendor.PhoneExt = db.GetNullableString(reader, 10);
        entities.Vendor.FaxAreaCode = db.GetNullableInt32(reader, 11);
        entities.Vendor.PhoneAreaCode = db.GetNullableInt32(reader, 12);
        entities.Vendor.Populated = true;
      });
  }

  private bool ReadVendorAddress()
  {
    entities.VendorAddress.Populated = false;

    return Read("ReadVendorAddress",
      (db, command) =>
      {
        db.SetInt32(command, "venIdentifier", entities.Vendor.Identifier);
        db.SetDate(
          command, "effectiveDate",
          import.VendorAddress.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.VendorAddress.VenIdentifier = db.GetInt32(reader, 0);
        entities.VendorAddress.EffectiveDate = db.GetDate(reader, 1);
        entities.VendorAddress.ExpiryDate = db.GetDate(reader, 2);
        entities.VendorAddress.Street1 = db.GetNullableString(reader, 3);
        entities.VendorAddress.Street2 = db.GetNullableString(reader, 4);
        entities.VendorAddress.City = db.GetNullableString(reader, 5);
        entities.VendorAddress.State = db.GetNullableString(reader, 6);
        entities.VendorAddress.ZipCode5 = db.GetNullableString(reader, 7);
        entities.VendorAddress.ZipCode4 = db.GetNullableString(reader, 8);
        entities.VendorAddress.LastUpdatedBy = db.GetString(reader, 9);
        entities.VendorAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.VendorAddress.Populated = true;
      });
  }

  private void UpdateVendor()
  {
    var name = import.Vendor.Name;
    var number = import.Vendor.Number ?? "";
    var phoneNumber = import.Vendor.PhoneNumber.GetValueOrDefault();
    var fax = import.Vendor.Fax.GetValueOrDefault();
    var contactPerson = import.Vendor.ContactPerson ?? "";
    var serviceTypeCode = import.Vendor.ServiceTypeCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var faxExt = import.Vendor.FaxExt ?? "";
    var phoneExt = import.Vendor.PhoneExt ?? "";
    var faxAreaCode = import.Vendor.FaxAreaCode.GetValueOrDefault();
    var phoneAreaCode = import.Vendor.PhoneAreaCode.GetValueOrDefault();

    entities.Vendor.Populated = false;
    Update("UpdateVendor",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetNullableString(command, "numb", number);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "fax", fax);
        db.SetNullableString(command, "contactPerson", contactPerson);
        db.SetNullableString(command, "serviceTypeCode", serviceTypeCode);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "faxExt", faxExt);
        db.SetNullableString(command, "phoneExt", phoneExt);
        db.SetNullableInt32(command, "faxArea", faxAreaCode);
        db.SetNullableInt32(command, "phoneArea", phoneAreaCode);
        db.SetInt32(command, "identifier", entities.Vendor.Identifier);
      });

    entities.Vendor.Name = name;
    entities.Vendor.Number = number;
    entities.Vendor.PhoneNumber = phoneNumber;
    entities.Vendor.Fax = fax;
    entities.Vendor.ContactPerson = contactPerson;
    entities.Vendor.ServiceTypeCode = serviceTypeCode;
    entities.Vendor.LastUpdatedBy = lastUpdatedBy;
    entities.Vendor.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Vendor.FaxExt = faxExt;
    entities.Vendor.PhoneExt = phoneExt;
    entities.Vendor.FaxAreaCode = faxAreaCode;
    entities.Vendor.PhoneAreaCode = phoneAreaCode;
    entities.Vendor.Populated = true;
  }

  private void UpdateVendorAddress()
  {
    System.Diagnostics.Debug.Assert(entities.VendorAddress.Populated);

    var expiryDate = import.VendorAddress.ExpiryDate;
    var street1 = import.VendorAddress.Street1 ?? "";
    var street2 = import.VendorAddress.Street2 ?? "";
    var city = import.VendorAddress.City ?? "";
    var state = import.VendorAddress.State ?? "";
    var zipCode5 = import.VendorAddress.ZipCode5 ?? "";
    var zipCode4 = import.VendorAddress.ZipCode4 ?? "";
    var lastUpdatedBy = import.VendorAddress.LastUpdatedBy;
    var lastUpdatedTimestamp = import.VendorAddress.LastUpdatedTimestamp;

    entities.VendorAddress.Populated = false;
    Update("UpdateVendorAddress",
      (db, command) =>
      {
        db.SetDate(command, "expiryDate", expiryDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "venIdentifier", entities.VendorAddress.VenIdentifier);
        db.SetDate(
          command, "effectiveDate",
          entities.VendorAddress.EffectiveDate.GetValueOrDefault());
      });

    entities.VendorAddress.ExpiryDate = expiryDate;
    entities.VendorAddress.Street1 = street1;
    entities.VendorAddress.Street2 = street2;
    entities.VendorAddress.City = city;
    entities.VendorAddress.State = state;
    entities.VendorAddress.ZipCode5 = zipCode5;
    entities.VendorAddress.ZipCode4 = zipCode4;
    entities.VendorAddress.LastUpdatedBy = lastUpdatedBy;
    entities.VendorAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.VendorAddress.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Hide.
    /// </summary>
    [JsonPropertyName("hide")]
    public VendorAddress Hide
    {
      get => hide ??= new();
      set => hide = value;
    }

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

    private VendorAddress hide;
    private Vendor vendor;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AddrUpdate.
    /// </summary>
    [JsonPropertyName("addrUpdate")]
    public Common AddrUpdate
    {
      get => addrUpdate ??= new();
      set => addrUpdate = value;
    }

    /// <summary>
    /// A value of VendorUpdate.
    /// </summary>
    [JsonPropertyName("vendorUpdate")]
    public Common VendorUpdate
    {
      get => vendorUpdate ??= new();
      set => vendorUpdate = value;
    }

    private Common addrUpdate;
    private Common vendorUpdate;
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
