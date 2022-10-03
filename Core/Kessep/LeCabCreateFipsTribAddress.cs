// Program: LE_CAB_CREATE_FIPS_TRIB_ADDRESS, ID: 372019604, model: 746.
// Short name: SWE01934
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_CREATE_FIPS_TRIB_ADDRESS.
/// </para>
/// <para>
/// This CAB will take validated input data for FIPS/TRIB address, along with a 
/// persistent view for Tribunal, FIPS or both, and create an occurrence of FIPS
/// /TRIB address and relate it to either or both populated persistent views.
/// RVW 1/31/97
/// </para>
/// </summary>
[Serializable]
public partial class LeCabCreateFipsTribAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_CREATE_FIPS_TRIB_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabCreateFipsTribAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabCreateFipsTribAddress.
  /// </summary>
  public LeCabCreateFipsTribAddress(IContext context, Import import,
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
    if (ReadFipsTribAddress3())
    {
      local.Last.Identifier = entities.FipsTribAddress.Identifier;
    }

    if (!ReadFips1())
    {
      ExitState = "FIPS_NF";

      return;
    }

    if (ReadFipsTribAddress1())
    {
      ExitState = "LE0000_FIPS_TRIB_ADDR_4_TYPE_AE";

      return;
    }

    try
    {
      CreateFipsTribAddress();
      export.FipsTribAddress.Assign(entities.New1);

      // --- Check if a tribunal is assoc with it. If so assoc the address with 
      // the tribunal. This is required to maintain the redundant relationship
      // between tribunal and fips trib address for US tribunals.
      if (ReadTribunal())
      {
        if (ReadFipsTribAddress2())
        {
          // --- An address of the same type already exists. Disassociate it 
          // first.
          if (ReadFips2())
          {
            // --- Some other fips is assoc with this address. So don't delete 
            // it; just disassoc from tribunal only.
            DisassociateFipsTribAddress();
          }
          else
          {
            // --- This address is not related to any fips. So clean it up.
            DeleteFipsTribAddress();
          }
        }

        AssociateTribunal();
      }
      else
      {
        // --- No action needed.
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "TRIBUNAL_ADDRESS_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "TRIBUNAL_ADDRESS_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void AssociateTribunal()
  {
    var trbId = entities.Tribunal.Identifier;

    entities.New1.Populated = false;
    Update("AssociateTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", trbId);
        db.SetInt32(command, "identifier", entities.New1.Identifier);
      });

    entities.New1.TrbId = trbId;
    entities.New1.Populated = true;
  }

  private void CreateFipsTribAddress()
  {
    var identifier = local.Last.Identifier + 1;
    var faxExtension = import.ForCreate.FaxExtension ?? "";
    var faxAreaCode = import.ForCreate.FaxAreaCode.GetValueOrDefault();
    var phoneExtension = import.ForCreate.PhoneExtension ?? "";
    var areaCode = import.ForCreate.AreaCode.GetValueOrDefault();
    var type1 = import.ForCreate.Type1;
    var street1 = import.ForCreate.Street1;
    var street2 = import.ForCreate.Street2 ?? "";
    var city = import.ForCreate.City;
    var state = entities.Fips.StateAbbreviation;
    var zipCode = import.ForCreate.ZipCode;
    var zip4 = import.ForCreate.Zip4 ?? "";
    var zip3 = import.ForCreate.Zip3 ?? "";
    var county = entities.Fips.CountyAbbreviation;
    var street3 = import.ForCreate.Street3 ?? "";
    var street4 = import.ForCreate.Street4 ?? "";
    var postalCode = import.ForCreate.PostalCode ?? "";
    var country = import.ForCreate.Country ?? "";
    var phoneNumber = import.ForCreate.PhoneNumber.GetValueOrDefault();
    var faxNumber = import.ForCreate.FaxNumber.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var fipState = entities.Fips.State;
    var fipCounty = entities.Fips.County;
    var fipLocation = entities.Fips.Location;

    entities.New1.Populated = false;
    Update("CreateFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "faxExtension", faxExtension);
        db.SetNullableInt32(command, "faxAreaCd", faxAreaCode);
        db.SetNullableString(command, "phoneExtension", phoneExtension);
        db.SetNullableInt32(command, "areaCd", areaCode);
        db.SetString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetNullableInt32(command, "faxNumber", faxNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetNullableInt32(command, "fipState", fipState);
        db.SetNullableInt32(command, "fipCounty", fipCounty);
        db.SetNullableInt32(command, "fipLocation", fipLocation);
      });

    entities.New1.Identifier = identifier;
    entities.New1.FaxExtension = faxExtension;
    entities.New1.FaxAreaCode = faxAreaCode;
    entities.New1.PhoneExtension = phoneExtension;
    entities.New1.AreaCode = areaCode;
    entities.New1.Type1 = type1;
    entities.New1.Street1 = street1;
    entities.New1.Street2 = street2;
    entities.New1.City = city;
    entities.New1.State = state;
    entities.New1.ZipCode = zipCode;
    entities.New1.Zip4 = zip4;
    entities.New1.Zip3 = zip3;
    entities.New1.County = county;
    entities.New1.Street3 = street3;
    entities.New1.Street4 = street4;
    entities.New1.Province = "";
    entities.New1.PostalCode = postalCode;
    entities.New1.Country = country;
    entities.New1.PhoneNumber = phoneNumber;
    entities.New1.FaxNumber = faxNumber;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTstamp = null;
    entities.New1.FipState = fipState;
    entities.New1.FipCounty = fipCounty;
    entities.New1.FipLocation = fipLocation;
    entities.New1.TrbId = null;
    entities.New1.Populated = true;
  }

  private void DeleteFipsTribAddress()
  {
    Update("DeleteFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });
  }

  private void DisassociateFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;
    Update("DisassociateFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });

    entities.FipsTribAddress.TrbId = null;
    entities.FipsTribAddress.Populated = true;
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.CreatedBy = db.GetString(reader, 6);
        entities.Fips.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Fips.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Fips.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Fips.StateAbbreviation = db.GetString(reader, 10);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    System.Diagnostics.Debug.Assert(entities.FipsTribAddress.Populated);
    entities.ExistingSomeOther.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.FipsTribAddress.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.FipsTribAddress.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.FipsTribAddress.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingSomeOther.State = db.GetInt32(reader, 0);
        entities.ExistingSomeOther.County = db.GetInt32(reader, 1);
        entities.ExistingSomeOther.Location = db.GetInt32(reader, 2);
        entities.ExistingSomeOther.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
        db.SetString(command, "type", import.ForCreate.Type1);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.City = db.GetString(reader, 8);
        entities.FipsTribAddress.State = db.GetString(reader, 9);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 15);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 16);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 17);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 18);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 19);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 25);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 26);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 27);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 28);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetString(command, "type", import.ForCreate.Type1);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.City = db.GetString(reader, 8);
        entities.FipsTribAddress.State = db.GetString(reader, 9);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 15);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 16);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 17);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 18);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 19);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 25);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 26);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 27);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 28);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress3()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress3",
      null,
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.City = db.GetString(reader, 8);
        entities.FipsTribAddress.State = db.GetString(reader, 9);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 15);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 16);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 17);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 18);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 19);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 25);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 26);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 27);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 28);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Fips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Fips.County);
        db.SetNullableInt32(command, "fipState", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.CreatedBy = db.GetString(reader, 4);
        entities.Tribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Tribunal.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Tribunal.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.Tribunal.Populated = true;
      });
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
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public FipsTribAddress ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private FipsTribAddress forCreate;
    private Fips fips;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    private FipsTribAddress fipsTribAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public FipsTribAddress Last
    {
      get => last ??= new();
      set => last = value;
    }

    private FipsTribAddress last;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingSomeOther.
    /// </summary>
    [JsonPropertyName("existingSomeOther")]
    public Fips ExistingSomeOther
    {
      get => existingSomeOther ??= new();
      set => existingSomeOther = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public FipsTribAddress New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Fips existingSomeOther;
    private Fips fips;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private FipsTribAddress new1;
  }
#endregion
}
