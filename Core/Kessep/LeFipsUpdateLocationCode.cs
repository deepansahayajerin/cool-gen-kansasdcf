// Program: LE_FIPS_UPDATE_LOCATION_CODE, ID: 374350835, model: 746.
// Short name: SWE02528
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_FIPS_UPDATE_LOCATION_CODE.
/// </summary>
[Serializable]
public partial class LeFipsUpdateLocationCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_FIPS_UPDATE_LOCATION_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeFipsUpdateLocationCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeFipsUpdateLocationCode.
  /// </summary>
  public LeFipsUpdateLocationCode(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Maintenance Log
    // ----------------------------------------------------------------------
    // 04/04/2000	M Ramirez	WR 163		Initial creation
    // ----------------------------------------------------------------------
    if (import.Old.State != import.New1.State)
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      return;
    }

    if (import.Old.County != import.New1.County)
    {
      ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

      return;
    }

    if (import.Old.Location == import.New1.Location)
    {
      ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

      return;
    }

    if (ReadFips2())
    {
      local.Fips.Assign(entities.Old);
    }
    else
    {
      ExitState = "FIPS_NF";

      return;
    }

    if (ReadFips1())
    {
      ExitState = "FIPS_AE";

      return;
    }
    else
    {
      local.Fips.Location = import.New1.Location;
    }

    try
    {
      CreateFips();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FIPS_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FIPS_AE";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    foreach(var item in ReadFipsTribAddress())
    {
      DisassociateFipsTribAddress();
      AssociateFipsTribAddress();
    }

    foreach(var item in ReadTribunal())
    {
      DisassociateTribunal();
      AssociateTribunal();
    }

    if (ReadCsePerson())
    {
      DisassociateCsePerson();
      AssociateCsePerson();
    }

    if (ReadOffice())
    {
      DisassociateOffice();
      AssociateOffice();
    }

    // mjr
    // ---------------------------------------------------------------
    // Remove old FIPS
    // ------------------------------------------------------------------
    if (ReadFips2())
    {
      DeleteFips();
    }
    else
    {
      ExitState = "FIPS_NF";
    }
  }

  private void AssociateCsePerson()
  {
    var cspNumber = entities.CsePerson.Number;

    entities.New1.Populated = false;
    Update("AssociateCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "state", entities.New1.State);
        db.SetInt32(command, "county", entities.New1.County);
        db.SetInt32(command, "location", entities.New1.Location);
      });

    entities.New1.CspNumber = cspNumber;
    entities.New1.Populated = true;
  }

  private void AssociateFipsTribAddress()
  {
    var fipState = entities.New1.State;
    var fipCounty = entities.New1.County;
    var fipLocation = entities.New1.Location;

    entities.FipsTribAddress.Populated = false;
    Update("AssociateFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", fipState);
        db.SetNullableInt32(command, "fipCounty", fipCounty);
        db.SetNullableInt32(command, "fipLocation", fipLocation);
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });

    entities.FipsTribAddress.FipState = fipState;
    entities.FipsTribAddress.FipCounty = fipCounty;
    entities.FipsTribAddress.FipLocation = fipLocation;
    entities.FipsTribAddress.Populated = true;
  }

  private void AssociateOffice()
  {
    var offIdentifier = entities.Office.SystemGeneratedId;

    entities.New1.Populated = false;
    Update("AssociateOffice",
      (db, command) =>
      {
        db.SetNullableInt32(command, "offIdentifier", offIdentifier);
        db.SetInt32(command, "state", entities.New1.State);
        db.SetInt32(command, "county", entities.New1.County);
        db.SetInt32(command, "location", entities.New1.Location);
      });

    entities.New1.OffIdentifier = offIdentifier;
    entities.New1.Populated = true;
  }

  private void AssociateTribunal()
  {
    var fipLocation = entities.New1.Location;
    var fipCounty = entities.New1.County;
    var fipState = entities.New1.State;

    entities.Tribunal.Populated = false;
    Update("AssociateTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", fipLocation);
        db.SetNullableInt32(command, "fipCounty", fipCounty);
        db.SetNullableInt32(command, "fipState", fipState);
        db.SetInt32(command, "identifier", entities.Tribunal.Identifier);
      });

    entities.Tribunal.FipLocation = fipLocation;
    entities.Tribunal.FipCounty = fipCounty;
    entities.Tribunal.FipState = fipState;
    entities.Tribunal.Populated = true;
  }

  private void CreateFips()
  {
    var state = local.Fips.State;
    var county = local.Fips.County;
    var location = local.Fips.Location;
    var stateDescription = local.Fips.StateDescription ?? "";
    var countyDescription = local.Fips.CountyDescription ?? "";
    var locationDescription = local.Fips.LocationDescription ?? "";
    var createdBy = local.Fips.CreatedBy;
    var createdTstamp = local.Fips.CreatedTstamp;
    var lastUpdatedBy = local.Fips.LastUpdatedBy ?? "";
    var lastUpdatedTstamp = local.Fips.LastUpdatedTstamp;
    var stateAbbreviation = local.Fips.StateAbbreviation;
    var countyAbbreviation = local.Fips.CountyAbbreviation ?? "";

    entities.New1.Populated = false;
    Update("CreateFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", state);
        db.SetInt32(command, "county", county);
        db.SetInt32(command, "location", location);
        db.SetNullableString(command, "stateDesc", stateDescription);
        db.SetNullableString(command, "countyDesc", countyDescription);
        db.SetNullableString(command, "locationDesc", locationDescription);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "stateAbbreviation", stateAbbreviation);
        db.SetNullableString(command, "countyAbbr", countyAbbreviation);
      });

    entities.New1.State = state;
    entities.New1.County = county;
    entities.New1.Location = location;
    entities.New1.StateDescription = stateDescription;
    entities.New1.CountyDescription = countyDescription;
    entities.New1.LocationDescription = locationDescription;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.LastUpdatedBy = lastUpdatedBy;
    entities.New1.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.New1.OffIdentifier = null;
    entities.New1.StateAbbreviation = stateAbbreviation;
    entities.New1.CountyAbbreviation = countyAbbreviation;
    entities.New1.CspNumber = null;
    entities.New1.Populated = true;
  }

  private void DeleteFips()
  {
    bool exists;

    Update("DeleteFips#1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", entities.Old.State);
        db.SetNullableInt32(command, "fipCounty", entities.Old.County);
        db.SetNullableInt32(command, "fipLocation", entities.Old.Location);
      });

    exists = Read("DeleteFips#2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", entities.Old.State);
        db.SetNullableInt32(command, "fipCounty", entities.Old.County);
        db.SetNullableInt32(command, "fipLocation", entities.Old.Location);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_TRIBUNAL\".", "50001");
    }

    Update("DeleteFips#3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", entities.Old.State);
        db.SetNullableInt32(command, "fipCounty", entities.Old.County);
        db.SetNullableInt32(command, "fipLocation", entities.Old.Location);
      });
  }

  private void DisassociateCsePerson()
  {
    entities.Old.Populated = false;
    Update("DisassociateCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "state", entities.Old.State);
        db.SetInt32(command, "county", entities.Old.County);
        db.SetInt32(command, "location", entities.Old.Location);
      });

    entities.Old.CspNumber = null;
    entities.Old.Populated = true;
  }

  private void DisassociateFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;
    Update("DisassociateFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });

    entities.FipsTribAddress.FipState = null;
    entities.FipsTribAddress.FipCounty = null;
    entities.FipsTribAddress.FipLocation = null;
    entities.FipsTribAddress.Populated = true;
  }

  private void DisassociateOffice()
  {
    entities.Old.Populated = false;
    Update("DisassociateOffice",
      (db, command) =>
      {
        db.SetInt32(command, "state", entities.Old.State);
        db.SetInt32(command, "county", entities.Old.County);
        db.SetInt32(command, "location", entities.Old.Location);
      });

    entities.Old.OffIdentifier = null;
    entities.Old.Populated = true;
  }

  private void DisassociateTribunal()
  {
    entities.Tribunal.Populated = false;
    Update("DisassociateTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.Tribunal.Identifier);
      });

    entities.Tribunal.FipLocation = null;
    entities.Tribunal.FipCounty = null;
    entities.Tribunal.FipState = null;
    entities.Tribunal.Populated = true;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Old.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadFips1()
  {
    entities.New1.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.New1.State);
        db.SetInt32(command, "county", import.New1.County);
        db.SetInt32(command, "location", import.New1.Location);
      },
      (db, reader) =>
      {
        entities.New1.State = db.GetInt32(reader, 0);
        entities.New1.County = db.GetInt32(reader, 1);
        entities.New1.Location = db.GetInt32(reader, 2);
        entities.New1.StateDescription = db.GetNullableString(reader, 3);
        entities.New1.CountyDescription = db.GetNullableString(reader, 4);
        entities.New1.LocationDescription = db.GetNullableString(reader, 5);
        entities.New1.CreatedBy = db.GetString(reader, 6);
        entities.New1.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.New1.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.New1.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.New1.OffIdentifier = db.GetNullableInt32(reader, 10);
        entities.New1.StateAbbreviation = db.GetString(reader, 11);
        entities.New1.CountyAbbreviation = db.GetNullableString(reader, 12);
        entities.New1.CspNumber = db.GetNullableString(reader, 13);
        entities.New1.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Old.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Old.State);
        db.SetInt32(command, "county", import.Old.County);
        db.SetInt32(command, "location", import.Old.Location);
      },
      (db, reader) =>
      {
        entities.Old.State = db.GetInt32(reader, 0);
        entities.Old.County = db.GetInt32(reader, 1);
        entities.Old.Location = db.GetInt32(reader, 2);
        entities.Old.StateDescription = db.GetNullableString(reader, 3);
        entities.Old.CountyDescription = db.GetNullableString(reader, 4);
        entities.Old.LocationDescription = db.GetNullableString(reader, 5);
        entities.Old.CreatedBy = db.GetString(reader, 6);
        entities.Old.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Old.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Old.OffIdentifier = db.GetNullableInt32(reader, 10);
        entities.Old.StateAbbreviation = db.GetString(reader, 11);
        entities.Old.CountyAbbreviation = db.GetNullableString(reader, 12);
        entities.Old.CspNumber = db.GetNullableString(reader, 13);
        entities.Old.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Old.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Old.County);
        db.SetNullableInt32(command, "fipState", entities.Old.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 1);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.Old.OffIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return ReadEach("ReadTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Old.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Old.County);
        db.SetNullableInt32(command, "fipState", entities.Old.State);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 2);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 3);
        entities.Tribunal.Populated = true;

        return true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Fips New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Fips Old
    {
      get => old ??= new();
      set => old = value;
    }

    private Fips new1;
    private Fips old;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips fips;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public Fips Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Fips New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Office office;
    private CsePerson csePerson;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips old;
    private Fips new1;
  }
#endregion
}
