// Program: OE_VEND_CREATE_VENDOR, ID: 371796237, model: 746.
// Short name: SWE00973
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_VEND_CREATE_VENDOR.
/// </para>
/// <para>
/// Creates Vendor row.  This is generic action block for all Vendors who has 
/// contracts with SRS.  Valid Vendor code is verified from CODE entity and its
/// description is fetched from CODE-VALUE entity.
/// Primary key is derived by adding 1 to highest existing identifier.
/// </para>
/// </summary>
[Serializable]
public partial class OeVendCreateVendor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VEND_CREATE_VENDOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeVendCreateVendor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeVendCreateVendor.
  /// </summary>
  public OeVendCreateVendor(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************
    //            SYSTEM:        KESSEP
    //            MODULE:
    //            MODULE TYPE:
    // ENTITY TYPE USED:
    //            VENDOR         -R- -C-	
    // DATABASE FILES USED:
    // CREATED BY: Grace P. Kim
    // Date Created: Jan 24, 1995
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR         DATE       CHG REQ#      DESCRIPTION
    // *********************************************
    ReadVendor();

    try
    {
      CreateVendor();
      export.Vendor.Identifier = entities.Entity.Identifier;
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

  private void CreateVendor()
  {
    var identifier = entities.ExistingLast.Identifier + 1;
    var name = import.Vendor.Name;
    var number = import.Vendor.Number ?? "";
    var phoneNumber = import.Vendor.PhoneNumber.GetValueOrDefault();
    var fax = import.Vendor.Fax.GetValueOrDefault();
    var contactPerson = import.Vendor.ContactPerson ?? "";
    var serviceTypeCode = import.Vendor.ServiceTypeCode ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var faxExt = import.Vendor.FaxExt ?? "";
    var phoneExt = import.Vendor.PhoneExt ?? "";
    var faxAreaCode = import.Vendor.FaxAreaCode.GetValueOrDefault();
    var phoneAreaCode = import.Vendor.PhoneAreaCode.GetValueOrDefault();

    entities.Entity.Populated = false;
    Update("CreateVendor",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "name", name);
        db.SetNullableString(command, "numb", number);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "fax", fax);
        db.SetNullableString(command, "contactPerson", contactPerson);
        db.SetNullableString(command, "serviceTypeCode", serviceTypeCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "faxExt", faxExt);
        db.SetNullableString(command, "phoneExt", phoneExt);
        db.SetNullableInt32(command, "faxArea", faxAreaCode);
        db.SetNullableInt32(command, "phoneArea", phoneAreaCode);
      });

    entities.Entity.Identifier = identifier;
    entities.Entity.Name = name;
    entities.Entity.Number = number;
    entities.Entity.PhoneNumber = phoneNumber;
    entities.Entity.Fax = fax;
    entities.Entity.ContactPerson = contactPerson;
    entities.Entity.ServiceTypeCode = serviceTypeCode;
    entities.Entity.CreatedBy = createdBy;
    entities.Entity.CreatedTimestamp = createdTimestamp;
    entities.Entity.LastUpdatedBy = "";
    entities.Entity.LastUpdatedTimestamp = createdTimestamp;
    entities.Entity.FaxExt = faxExt;
    entities.Entity.PhoneExt = phoneExt;
    entities.Entity.FaxAreaCode = faxAreaCode;
    entities.Entity.PhoneAreaCode = phoneAreaCode;
    entities.Entity.Populated = true;
  }

  private bool ReadVendor()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadVendor",
      null,
      (db, reader) =>
      {
        entities.ExistingLast.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLast.Populated = true;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public Vendor ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of Entity.
    /// </summary>
    [JsonPropertyName("entity")]
    public Vendor Entity
    {
      get => entity ??= new();
      set => entity = value;
    }

    private Vendor existingLast;
    private Vendor entity;
  }
#endregion
}
