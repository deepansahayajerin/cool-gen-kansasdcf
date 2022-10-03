// Program: CAB_FN_GET_TRIBUNAL_ADDRESS, ID: 372312317, model: 746.
// Short name: SWE00049
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FN_GET_TRIBUNAL_ADDRESS.
/// </summary>
[Serializable]
public partial class CabFnGetTribunalAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FN_GET_TRIBUNAL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFnGetTribunalAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFnGetTribunalAddress.
  /// </summary>
  public CabFnGetTribunalAddress(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadTribunalFipsTribAddress())
    {
      export.FipsTribAddress.Assign(entities.FipsTribAddress);
    }
    else
    {
      ExitState = "LE0000_TRIBUNAL_NF";
    }
  }

  private bool ReadTribunalFipsTribAddress()
  {
    entities.Tribunal.Populated = false;
    entities.FipsTribAddress.Populated = false;

    return Read("ReadTribunalFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 0);
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.Tribunal.Populated = true;
        entities.FipsTribAddress.Populated = true;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private Tribunal tribunal;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
  }
#endregion
}
