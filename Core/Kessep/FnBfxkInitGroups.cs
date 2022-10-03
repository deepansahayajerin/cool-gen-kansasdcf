// Program: FN_BFXK_INIT_GROUPS, ID: 371408032, model: 746.
// Short name: SWE02069
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXK_INIT_GROUPS.
/// </summary>
[Serializable]
public partial class FnBfxkInitGroups: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXK_INIT_GROUPS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxkInitGroups(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxkInitGroups.
  /// </summary>
  public FnBfxkInitGroups(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -- No statements required.  Just using to initiatlize the group views.
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// <summary>A DistributionPolicyGroup group.</summary>
    [Serializable]
    public class DistributionPolicyGroup
    {
      /// <summary>
      /// A value of GrCollectionType.
      /// </summary>
      [JsonPropertyName("grCollectionType")]
      public CollectionType GrCollectionType
      {
        get => grCollectionType ??= new();
        set => grCollectionType = value;
      }

      /// <summary>
      /// A value of GrDistributionPolicy.
      /// </summary>
      [JsonPropertyName("grDistributionPolicy")]
      public DistributionPolicy GrDistributionPolicy
      {
        get => grDistributionPolicy ??= new();
        set => grDistributionPolicy = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CollectionType grCollectionType;
      private DistributionPolicy grDistributionPolicy;
    }

    /// <summary>A DistPolicyRuleGroup group.</summary>
    [Serializable]
    public class DistPolicyRuleGroup
    {
      /// <summary>
      /// A value of Gr.
      /// </summary>
      [JsonPropertyName("gr")]
      public DistributionPolicyRule Gr
      {
        get => gr ??= new();
        set => gr = value;
      }

      /// <summary>
      /// Gets a value of Obligation.
      /// </summary>
      [JsonIgnore]
      public Array<ObligationGroup> Obligation => obligation ??= new(
        ObligationGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Obligation for json serialization.
      /// </summary>
      [JsonPropertyName("obligation")]
      [Computed]
      public IList<ObligationGroup> Obligation_Json
      {
        get => obligation;
        set => Obligation.Assign(value);
      }

      /// <summary>
      /// Gets a value of DprProgram.
      /// </summary>
      [JsonIgnore]
      public Array<DprProgramGroup> DprProgram => dprProgram ??= new(
        DprProgramGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DprProgram for json serialization.
      /// </summary>
      [JsonPropertyName("dprProgram")]
      [Computed]
      public IList<DprProgramGroup> DprProgram_Json
      {
        get => dprProgram;
        set => DprProgram.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private DistributionPolicyRule gr;
      private Array<ObligationGroup> obligation;
      private Array<DprProgramGroup> dprProgram;
    }

    /// <summary>A ObligationGroup group.</summary>
    [Serializable]
    public class ObligationGroup
    {
      /// <summary>
      /// A value of GrObligationType.
      /// </summary>
      [JsonPropertyName("grObligationType")]
      public ObligationType GrObligationType
      {
        get => grObligationType ??= new();
        set => grObligationType = value;
      }

      /// <summary>
      /// A value of GrDprObligType.
      /// </summary>
      [JsonPropertyName("grDprObligType")]
      public DprObligType GrDprObligType
      {
        get => grDprObligType ??= new();
        set => grDprObligType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ObligationType grObligationType;
      private DprObligType grDprObligType;
    }

    /// <summary>A DprProgramGroup group.</summary>
    [Serializable]
    public class DprProgramGroup
    {
      /// <summary>
      /// A value of GrProgram.
      /// </summary>
      [JsonPropertyName("grProgram")]
      public Program GrProgram
      {
        get => grProgram ??= new();
        set => grProgram = value;
      }

      /// <summary>
      /// A value of GrDprProgram.
      /// </summary>
      [JsonPropertyName("grDprProgram")]
      public DprProgram GrDprProgram
      {
        get => grDprProgram ??= new();
        set => grDprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Program grProgram;
      private DprProgram grDprProgram;
    }

    /// <summary>
    /// Gets a value of DistributionPolicy.
    /// </summary>
    [JsonIgnore]
    public Array<DistributionPolicyGroup> DistributionPolicy =>
      distributionPolicy ??= new(DistributionPolicyGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DistributionPolicy for json serialization.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    [Computed]
    public IList<DistributionPolicyGroup> DistributionPolicy_Json
    {
      get => distributionPolicy;
      set => DistributionPolicy.Assign(value);
    }

    /// <summary>
    /// Gets a value of DistPolicyRule.
    /// </summary>
    [JsonIgnore]
    public Array<DistPolicyRuleGroup> DistPolicyRule => distPolicyRule ??= new(
      DistPolicyRuleGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DistPolicyRule for json serialization.
    /// </summary>
    [JsonPropertyName("distPolicyRule")]
    [Computed]
    public IList<DistPolicyRuleGroup> DistPolicyRule_Json
    {
      get => distPolicyRule;
      set => DistPolicyRule.Assign(value);
    }

    private Array<DistributionPolicyGroup> distributionPolicy;
    private Array<DistPolicyRuleGroup> distPolicyRule;
  }
#endregion
}
