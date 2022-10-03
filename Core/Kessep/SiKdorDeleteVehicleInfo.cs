// Program: SI_KDOR_DELETE_VEHICLE_INFO, ID: 1625325282, model: 746.
// Short name: SWE01172
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_KDOR_DELETE_VEHICLE_INFO.
/// </summary>
[Serializable]
public partial class SiKdorDeleteVehicleInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_KDOR_DELETE_VEHICLE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiKdorDeleteVehicleInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiKdorDeleteVehicleInfo.
  /// </summary>
  public SiKdorDeleteVehicleInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 12/01/18  GVandy	CQ61419		Initial Code.
    // -------------------------------------------------------------------------------------
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadKdorVehicle())
    {
      ExitState = "KDOR_VEHICLE_NF";

      return;
    }

    foreach(var item in ReadKdorVehicleOwner())
    {
      DeleteKdorVehicleOwner();
    }

    DeleteKdorVehicle();
  }

  private void DeleteKdorVehicle()
  {
    Update("DeleteKdorVehicle",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.KdorVehicle.Identifier);
        db.SetString(
          command, "fkCktCsePersnumb", entities.KdorVehicle.FkCktCsePersnumb);
      });
  }

  private void DeleteKdorVehicleOwner()
  {
    Update("DeleteKdorVehicleOwner",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", entities.KdorVehicleOwner.Identifier);
          
        db.SetString(
          command, "fkCktKdorVehfkCktCsePers",
          entities.KdorVehicleOwner.FkCktKdorVehfkCktCsePers);
        db.SetInt32(
          command, "fkCktKdorVehidentifier",
          entities.KdorVehicleOwner.FkCktKdorVehidentifier);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadKdorVehicle()
  {
    entities.KdorVehicle.Populated = false;

    return Read("ReadKdorVehicle",
      (db, command) =>
      {
        db.SetString(command, "fkCktCsePersnumb", entities.CsePerson.Number);
        db.SetInt32(command, "identifier", import.KdorVehicle.Identifier);
      },
      (db, reader) =>
      {
        entities.KdorVehicle.Identifier = db.GetInt32(reader, 0);
        entities.KdorVehicle.FkCktCsePersnumb = db.GetString(reader, 1);
        entities.KdorVehicle.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKdorVehicleOwner()
  {
    System.Diagnostics.Debug.Assert(entities.KdorVehicle.Populated);
    entities.KdorVehicleOwner.Populated = false;

    return ReadEach("ReadKdorVehicleOwner",
      (db, command) =>
      {
        db.SetString(
          command, "fkCktKdorVehfkCktCsePers",
          entities.KdorVehicle.FkCktCsePersnumb);
        db.SetInt32(
          command, "fkCktKdorVehidentifier", entities.KdorVehicle.Identifier);
      },
      (db, reader) =>
      {
        entities.KdorVehicleOwner.Identifier = db.GetInt32(reader, 0);
        entities.KdorVehicleOwner.FkCktKdorVehfkCktCsePers =
          db.GetString(reader, 1);
        entities.KdorVehicleOwner.FkCktKdorVehidentifier =
          db.GetInt32(reader, 2);
        entities.KdorVehicleOwner.Populated = true;

        return true;
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
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
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

    private KdorVehicle kdorVehicle;
    private CsePerson csePerson;
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
    /// A value of KdorVehicleOwner.
    /// </summary>
    [JsonPropertyName("kdorVehicleOwner")]
    public KdorVehicleOwner KdorVehicleOwner
    {
      get => kdorVehicleOwner ??= new();
      set => kdorVehicleOwner = value;
    }

    /// <summary>
    /// A value of KdorVehicle.
    /// </summary>
    [JsonPropertyName("kdorVehicle")]
    public KdorVehicle KdorVehicle
    {
      get => kdorVehicle ??= new();
      set => kdorVehicle = value;
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

    private KdorVehicleOwner kdorVehicleOwner;
    private KdorVehicle kdorVehicle;
    private CsePerson csePerson;
  }
#endregion
}
