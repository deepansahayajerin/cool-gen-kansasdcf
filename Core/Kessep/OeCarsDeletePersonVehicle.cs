// Program: OE_CARS_DELETE_PERSON_VEHICLE, ID: 371813384, model: 746.
// Short name: SWE00877
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CARS_DELETE_PERSON_VEHICLE.
/// </para>
/// <para>
/// RESP: OBLGESTB		
/// Update existing person resource and address for resource location and/or 
/// lien holder.
/// </para>
/// </summary>
[Serializable]
public partial class OeCarsDeletePersonVehicle: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CARS_DELETE_PERSON_VEHICLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCarsDeletePersonVehicle(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCarsDeletePersonVehicle.
  /// </summary>
  public OeCarsDeletePersonVehicle(IContext context, Import import,
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
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadCsePersonVehicle())
    {
      ExitState = "CSE_PERSON_VEHICLE_NF";

      return;
    }

    // --------------------------------------------------
    // CSE_PERSON_VEHICLE is set for cascade delete so
    // all associations will be removed with
    // the one delete of vehicle.
    // --------------------------------------------------
    DeleteCsePersonVehicle();
  }

  private void DeleteCsePersonVehicle()
  {
    Update("DeleteCsePersonVehicle#1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber1", entities.ExistingCsePersonVehicle.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingCsePersonVehicle.Identifier);
      });

    Update("DeleteCsePersonVehicle#2",
      (db, command) =>
      {
        db.SetInt32(
          command, "resourceNo",
          entities.ExistingCsePersonVehicle.CprCResourceNo.GetValueOrDefault());
          
        db.SetString(
          command, "cspNumber2",
          entities.ExistingCsePersonVehicle.CspCNumber ?? "");
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonVehicle()
  {
    entities.ExistingCsePersonVehicle.Populated = false;

    return Read("ReadCsePersonVehicle",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.CsePersonVehicle.Identifier);
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.ExistingCsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCsePersonVehicle.CspCNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonVehicle.Populated = true;
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
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("existingCsePersonVehicle")]
    public CsePersonVehicle ExistingCsePersonVehicle
    {
      get => existingCsePersonVehicle ??= new();
      set => existingCsePersonVehicle = value;
    }

    private CsePerson existingCsePerson;
    private CsePersonVehicle existingCsePersonVehicle;
  }
#endregion
}
