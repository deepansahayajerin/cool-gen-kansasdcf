// Program: OE_CARS_UPDATE_PERSON_VEHICLE, ID: 371813382, model: 746.
// Short name: SWE00879
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CARS_UPDATE_PERSON_VEHICLE.
/// </para>
/// <para>
/// Resp:  OBLGESTB	
/// Action block will update an existing person's vehicle.
/// </para>
/// </summary>
[Serializable]
public partial class OeCarsUpdatePersonVehicle: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CARS_UPDATE_PERSON_VEHICLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCarsUpdatePersonVehicle(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCarsUpdatePersonVehicle.
  /// </summary>
  public OeCarsUpdatePersonVehicle(IContext context, Import import,
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
    if (!ReadCsePersonVehicle())
    {
      ExitState = "CSE_PERSON_VEHICLE_NF";

      return;
    }

    try
    {
      UpdateCsePersonVehicle();
      export.CsePersonVehicle.Assign(entities.CsePersonVehicle);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CSE_PERSON_VEHICLE_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CSE_PERSON_VEHICLE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadCsePersonVehicle()
  {
    entities.CsePersonVehicle.Populated = false;

    return Read("ReadCsePersonVehicle",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.CsePersonVehicle.Identifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.CsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.CsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.CsePersonVehicle.CspCNumber = db.GetNullableString(reader, 3);
        entities.CsePersonVehicle.InactiveInd = db.GetNullableString(reader, 4);
        entities.CsePersonVehicle.VerifiedUserId =
          db.GetNullableString(reader, 5);
        entities.CsePersonVehicle.VehicleRegistrationState =
          db.GetNullableString(reader, 6);
        entities.CsePersonVehicle.VehicleColor =
          db.GetNullableString(reader, 7);
        entities.CsePersonVehicle.VehicleModel =
          db.GetNullableString(reader, 8);
        entities.CsePersonVehicle.VehicleMake = db.GetNullableString(reader, 9);
        entities.CsePersonVehicle.VehicleIdentificationNumber =
          db.GetNullableString(reader, 10);
        entities.CsePersonVehicle.VehicleLicenseTag =
          db.GetNullableString(reader, 11);
        entities.CsePersonVehicle.VehicleYear = db.GetNullableInt32(reader, 12);
        entities.CsePersonVehicle.VehicleOwnedInd =
          db.GetNullableString(reader, 13);
        entities.CsePersonVehicle.VerifiedDate = db.GetNullableDate(reader, 14);
        entities.CsePersonVehicle.CreatedBy = db.GetString(reader, 15);
        entities.CsePersonVehicle.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.CsePersonVehicle.LastUpdatedBy = db.GetString(reader, 17);
        entities.CsePersonVehicle.LastUpdatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.CsePersonVehicle.Populated = true;
      });
  }

  private void UpdateCsePersonVehicle()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonVehicle.Populated);

    var inactiveInd = import.CsePersonVehicle.InactiveInd ?? "";
    var verifiedUserId = import.CsePersonVehicle.VerifiedUserId ?? "";
    var vehicleRegistrationState =
      import.CsePersonVehicle.VehicleRegistrationState ?? "";
    var vehicleColor = import.CsePersonVehicle.VehicleColor ?? "";
    var vehicleModel = import.CsePersonVehicle.VehicleModel ?? "";
    var vehicleMake = import.CsePersonVehicle.VehicleMake ?? "";
    var vehicleIdentificationNumber =
      import.CsePersonVehicle.VehicleIdentificationNumber ?? "";
    var vehicleLicenseTag = import.CsePersonVehicle.VehicleLicenseTag ?? "";
    var vehicleYear = import.CsePersonVehicle.VehicleYear.GetValueOrDefault();
    var vehicleOwnedInd = import.CsePersonVehicle.VehicleOwnedInd ?? "";
    var verifiedDate = import.CsePersonVehicle.VerifiedDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CsePersonVehicle.Populated = false;
    Update("UpdateCsePersonVehicle",
      (db, command) =>
      {
        db.SetNullableString(command, "inactiveInd", inactiveInd);
        db.SetNullableString(command, "verifiedUserId", verifiedUserId);
        db.SetNullableString(command, "vehRegState", vehicleRegistrationState);
        db.SetNullableString(command, "vehicleColor", vehicleColor);
        db.SetNullableString(command, "vehicleModel", vehicleModel);
        db.SetNullableString(command, "vehicleMake", vehicleMake);
        db.
          SetNullableString(command, "vehIdNumber", vehicleIdentificationNumber);
          
        db.SetNullableString(command, "vehLicTag", vehicleLicenseTag);
        db.SetNullableInt32(command, "vehicleYear", vehicleYear);
        db.SetNullableString(command, "vehicleOwnedInd", vehicleOwnedInd);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "cspNumber", entities.CsePersonVehicle.CspNumber);
        db.
          SetInt32(command, "identifier", entities.CsePersonVehicle.Identifier);
          
      });

    entities.CsePersonVehicle.InactiveInd = inactiveInd;
    entities.CsePersonVehicle.VerifiedUserId = verifiedUserId;
    entities.CsePersonVehicle.VehicleRegistrationState =
      vehicleRegistrationState;
    entities.CsePersonVehicle.VehicleColor = vehicleColor;
    entities.CsePersonVehicle.VehicleModel = vehicleModel;
    entities.CsePersonVehicle.VehicleMake = vehicleMake;
    entities.CsePersonVehicle.VehicleIdentificationNumber =
      vehicleIdentificationNumber;
    entities.CsePersonVehicle.VehicleLicenseTag = vehicleLicenseTag;
    entities.CsePersonVehicle.VehicleYear = vehicleYear;
    entities.CsePersonVehicle.VehicleOwnedInd = vehicleOwnedInd;
    entities.CsePersonVehicle.VerifiedDate = verifiedDate;
    entities.CsePersonVehicle.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonVehicle.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonVehicle.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    private CsePerson csePerson;
    private CsePersonVehicle csePersonVehicle;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    private CsePersonVehicle csePersonVehicle;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Conversion.
    /// </summary>
    [JsonPropertyName("conversion")]
    public DateWorkArea Conversion
    {
      get => conversion ??= new();
      set => conversion = value;
    }

    private DateWorkArea conversion;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    private CsePerson csePerson;
    private CsePersonVehicle csePersonVehicle;
  }
#endregion
}
