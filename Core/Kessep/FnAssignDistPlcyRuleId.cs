// Program: FN_ASSIGN_DIST_PLCY_RULE_ID, ID: 371960110, model: 746.
// Short name: SWE00284
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_DIST_PLCY_RULE_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will assign a unique ID for Distribution Policy Rules.
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignDistPlcyRuleId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_DIST_PLCY_RULE_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignDistPlcyRuleId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignDistPlcyRuleId.
  /// </summary>
  public FnAssignDistPlcyRuleId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDistributionPolicyRule())
    {
      local.DistributionPolicyRule.SystemGeneratedIdentifier =
        entities.DistributionPolicyRule.SystemGeneratedIdentifier;
    }

    export.DistributionPolicyRule.SystemGeneratedIdentifier =
      local.DistributionPolicyRule.SystemGeneratedIdentifier + 1;
  }

  private bool ReadDistributionPolicyRule()
  {
    entities.DistributionPolicyRule.Populated = false;

    return Read("ReadDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          import.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicyRule.DbpGeneratedId = db.GetInt32(reader, 0);
        entities.DistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 2);
        entities.DistributionPolicyRule.Populated = true;
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
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    private DistributionPolicyRule distributionPolicyRule;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    private DistributionPolicyRule distributionPolicyRule;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    private DistributionPolicyRule distributionPolicyRule;
  }
#endregion
}
