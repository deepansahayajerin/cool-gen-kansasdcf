// Program: FN_ASSIGN_MAX_PASSTHRU_ID, ID: 371808427, model: 746.
// Short name: SWE00285
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_MAX_PASSTHRU_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignMaxPassthruId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_MAX_PASSTHRU_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignMaxPassthruId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignMaxPassthruId.
  /// </summary>
  public FnAssignMaxPassthruId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadMaximumPassthru())
    {
      local.MaximumPassthru.SystemGeneratedIdentifier =
        entities.MaximumPassthru.SystemGeneratedIdentifier;
    }

    export.MaximumPassthru.SystemGeneratedIdentifier =
      local.MaximumPassthru.SystemGeneratedIdentifier + 1;
  }

  private bool ReadMaximumPassthru()
  {
    entities.MaximumPassthru.Populated = false;

    return Read("ReadMaximumPassthru",
      null,
      (db, reader) =>
      {
        entities.MaximumPassthru.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MaximumPassthru.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
