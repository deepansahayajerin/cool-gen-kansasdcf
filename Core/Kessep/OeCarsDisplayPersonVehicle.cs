// Program: OE_CARS_DISPLAY_PERSON_VEHICLE, ID: 371813379, model: 746.
// Short name: SWE00878
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CARS_DISPLAY_PERSON_VEHICLE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block will read the desired CSE Person and their vehicles.  Up 
/// to 3 vehicles will be returned to calling procedure.
/// </para>
/// </summary>
[Serializable]
public partial class OeCarsDisplayPersonVehicle: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CARS_DISPLAY_PERSON_VEHICLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCarsDisplayPersonVehicle(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCarsDisplayPersonVehicle.
  /// </summary>
  public OeCarsDisplayPersonVehicle(IContext context, Import import,
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
    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadCsePersonVehicle())
    {
      export.Group.Update.DetailCsePersonVehicle.Assign(
        entities.ExistingCsePersonVehicle);
      export.Group.Update.DetailCommon.SelectChar = "";
      export.Group.Update.DetailListStates.PromptField = "";
      export.Group.Next();
    }

    if (export.Group.IsEmpty)
    {
      ExitState = "OE0000_NO_MORE_CARS";
    }
    else if (export.Group.IsFull)
    {
      ExitState = "OE0000_LIST_IS_FULL";
    }
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

  private IEnumerable<bool> ReadCsePersonVehicle()
  {
    return ReadEach("ReadCsePersonVehicle",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Starting.Identifier);
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingCsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.ExistingCsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCsePersonVehicle.CspCNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonVehicle.InactiveInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonVehicle.VerifiedUserId =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonVehicle.VehicleRegistrationState =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonVehicle.VehicleColor =
          db.GetNullableString(reader, 7);
        entities.ExistingCsePersonVehicle.VehicleModel =
          db.GetNullableString(reader, 8);
        entities.ExistingCsePersonVehicle.VehicleMake =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonVehicle.VehicleIdentificationNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePersonVehicle.VehicleLicenseTag =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonVehicle.VehicleYear =
          db.GetNullableInt32(reader, 12);
        entities.ExistingCsePersonVehicle.VehicleOwnedInd =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonVehicle.VerifiedDate =
          db.GetNullableDate(reader, 14);
        entities.ExistingCsePersonVehicle.CreatedBy = db.GetString(reader, 15);
        entities.ExistingCsePersonVehicle.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCsePersonVehicle.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.ExistingCsePersonVehicle.LastUpdatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.ExistingCsePersonVehicle.Populated = true;

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePersonVehicle Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    private CsePerson csePerson;
    private CsePersonVehicle starting;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePersonVehicle.
      /// </summary>
      [JsonPropertyName("detailCsePersonVehicle")]
      public CsePersonVehicle DetailCsePersonVehicle
      {
        get => detailCsePersonVehicle ??= new();
        set => detailCsePersonVehicle = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Standard detailListStates;
      private Common detailCommon;
      private CsePersonVehicle detailCsePersonVehicle;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private CsePerson csePerson;
    private Array<GroupGroup> group;
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

    /// <summary>
    /// A value of WorkVehicleFound.
    /// </summary>
    [JsonPropertyName("workVehicleFound")]
    public Common WorkVehicleFound
    {
      get => workVehicleFound ??= new();
      set => workVehicleFound = value;
    }

    private DateWorkArea conversion;
    private Common workVehicleFound;
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
