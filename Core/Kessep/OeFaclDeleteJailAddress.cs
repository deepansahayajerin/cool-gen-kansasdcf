// Program: OE_FACL_DELETE_JAIL_ADDRESS, ID: 373339596, model: 746.
// Short name: SWE00757
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_FACL_DELETE_JAIL_ADDRESS.
/// </para>
/// <para>
/// This action block contains the code for deleting a jail_address from 
/// CKT_Jail_Addresses table.
/// </para>
/// </summary>
[Serializable]
public partial class OeFaclDeleteJailAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FACL_DELETE_JAIL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFaclDeleteJailAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFaclDeleteJailAddress.
  /// </summary>
  public OeFaclDeleteJailAddress(IContext context, Import import, Export export):
    
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
    if (ReadJailAddresses())
    {
      DeleteJailAddresses();
    }
    else
    {
      ExitState = "OE0000_JAIL_ADDRESS_NF";
    }
  }

  private void DeleteJailAddresses()
  {
    Update("DeleteJailAddresses",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.JailAddresses.Identifier);
      });
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
