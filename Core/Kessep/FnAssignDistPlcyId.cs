// Program: FN_ASSIGN_DIST_PLCY_ID, ID: 371960109, model: 746.
// Short name: SWE00283
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_DIST_PLCY_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will assign a unique ID for Distribution Policy.
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignDistPlcyId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_DIST_PLCY_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignDistPlcyId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignDistPlcyId.
  /// </summary>
  public FnAssignDistPlcyId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDistributionPolicy())
    {
      local.DistributionPolicy.SystemGeneratedIdentifier =
        entities.DistributionPolicy.SystemGeneratedIdentifier;
    }

    export.DistributionPolicy.SystemGeneratedIdentifier =
      local.DistributionPolicy.SystemGeneratedIdentifier + 1;
  }

  private bool ReadDistributionPolicy()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      null,
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Populated = true;
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
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private DistributionPolicy distributionPolicy;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private DistributionPolicy distributionPolicy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private DistributionPolicy distributionPolicy;
  }
#endregion
}
