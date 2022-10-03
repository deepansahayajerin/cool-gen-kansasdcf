// Program: FN_DELETE_MAXIMUM_PASSTHRU, ID: 371808067, model: 746.
// Short name: SWE00419
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_MAXIMUM_PASSTHRU.
/// </summary>
[Serializable]
public partial class FnDeleteMaximumPassthru: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_MAXIMUM_PASSTHRU program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteMaximumPassthru(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteMaximumPassthru.
  /// </summary>
  public FnDeleteMaximumPassthru(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** EDIT AREA *****
    if (ReadMaximumPassthru())
    {
      // ***** MAIN-LINE AREA *****
      DeleteMaximumPassthru();
    }
    else
    {
      ExitState = "MAXIMUM_PASSTHRU_NF";
    }
  }

  private void DeleteMaximumPassthru()
  {
    Update("DeleteMaximumPassthru",
      (db, command) =>
      {
        db.SetInt32(
          command, "maxPassthruId",
          entities.MaximumPassthru.SystemGeneratedIdentifier);
      });
  }

  private bool ReadMaximumPassthru()
  {
    entities.MaximumPassthru.Populated = false;

    return Read("ReadMaximumPassthru",
      (db, command) =>
      {
        db.SetInt32(
          command, "maxPassthruId",
          import.MaximumPassthru.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MaximumPassthru.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MaximumPassthru.EffectiveDate = db.GetDate(reader, 1);
        entities.MaximumPassthru.Populated = true;
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
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    private MaximumPassthru maximumPassthru;
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
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    private MaximumPassthru maximumPassthru;
  }
#endregion
}
