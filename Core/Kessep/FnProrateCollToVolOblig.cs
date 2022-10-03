// Program: FN_PRORATE_COLL_TO_VOL_OBLIG, ID: 372280674, model: 746.
// Short name: SWE02366
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PRORATE_COLL_TO_VOL_OBLIG.
/// </summary>
[Serializable]
public partial class FnProrateCollToVolOblig: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PRORATE_COLL_TO_VOL_OBLIG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProrateCollToVolOblig(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProrateCollToVolOblig.
  /// </summary>
  public FnProrateCollToVolOblig(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // : Group debts by Voluntary Obligation.
    for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
      import.Debts.Index)
    {
      if (!import.Debts.CheckSize())
      {
        break;
      }

      if (local.ForProrate.IsEmpty)
      {
        local.ForProrate.Index = 0;
        local.ForProrate.CheckSize();

        local.ForProrate.Update.ForProrate1.SystemGeneratedIdentifier =
          import.Debts.Item.DebtsObligation.SystemGeneratedIdentifier;

        continue;
      }

      for(local.ForProrate.Index = 0; local.ForProrate.Index < local
        .ForProrate.Count; ++local.ForProrate.Index)
      {
        if (!local.ForProrate.CheckSize())
        {
          break;
        }

        if (import.Debts.Item.DebtsObligation.SystemGeneratedIdentifier == local
          .ForProrate.Item.ForProrate1.SystemGeneratedIdentifier)
        {
          goto Next1;
        }
      }

      local.ForProrate.CheckIndex();

      local.ForProrate.Index = local.ForProrate.Count;
      local.ForProrate.CheckSize();

      local.ForProrate.Update.ForProrate1.SystemGeneratedIdentifier =
        import.Debts.Item.DebtsObligation.SystemGeneratedIdentifier;

Next1:
      ;
    }

    import.Debts.CheckIndex();

    // : Now we distribute the payment prorated across each Support Person 
    // within each Voluntary Obligation.
    local.TotAmtToDistByOblig.TotalCurrency =
      import.AmtToDistribute.TotalCurrency / local.ForProrate.Count;

    for(local.ForProrate.Index = 0; local.ForProrate.Index < local
      .ForProrate.Count; ++local.ForProrate.Index)
    {
      if (!local.ForProrate.CheckSize())
      {
        break;
      }

      if (local.ForProrate.Index + 1 == local.ForProrate.Count)
      {
        local.TotAmtToDistByOblig.TotalCurrency =
          import.AmtToDistribute.TotalCurrency - local
          .TotAmtDistributed.TotalCurrency;
      }

      local.TotAmtDistForOblig.TotalCurrency = 0;

      for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
        import.Debts.Index)
      {
        if (!import.Debts.CheckSize())
        {
          break;
        }

        if (import.Debts.Item.DebtsObligation.SystemGeneratedIdentifier == local
          .ForProrate.Item.ForProrate1.SystemGeneratedIdentifier)
        {
          import.Debts.Update.DebtsCollection.AppliedToCode = "C";
          import.Debts.Update.DebtsCollection.Amount =
            local.TotAmtToDistByOblig.TotalCurrency * import
            .Debts.Item.DebtsDebt.VoluntaryPercentageAmount.
              GetValueOrDefault() / 100;
          local.TotAmtDistForOblig.TotalCurrency += import.Debts.Item.
            DebtsCollection.Amount;
          local.TotAmtDistributed.TotalCurrency += import.Debts.Item.
            DebtsCollection.Amount;
        }
      }

      import.Debts.CheckIndex();

      if (local.TotAmtToDistByOblig.TotalCurrency - local
        .TotAmtDistForOblig.TotalCurrency == 0)
      {
        continue;
      }

      for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
        import.Debts.Index)
      {
        if (!import.Debts.CheckSize())
        {
          break;
        }

        if (import.Debts.Item.DebtsObligation.SystemGeneratedIdentifier == local
          .ForProrate.Item.ForProrate1.SystemGeneratedIdentifier)
        {
          import.Debts.Update.DebtsCollection.Amount =
            import.Debts.Item.DebtsCollection.Amount + (
              local.TotAmtToDistByOblig.TotalCurrency - local
            .TotAmtDistForOblig.TotalCurrency);
          local.TotAmtDistributed.TotalCurrency += local.TotAmtToDistByOblig.
            TotalCurrency - local.TotAmtDistForOblig.TotalCurrency;

          goto Next2;
        }
      }

      import.Debts.CheckIndex();

Next2:
      ;
    }

    local.ForProrate.CheckIndex();

    if (import.AmtToDistribute.TotalCurrency - local
      .TotAmtDistributed.TotalCurrency == 0)
    {
      return;
    }

    import.Debts.Index = import.Debts.Count - 1;
    import.Debts.CheckSize();

    import.Debts.Update.DebtsCollection.Amount =
      import.Debts.Item.DebtsCollection.Amount + (
        import.AmtToDistribute.TotalCurrency - local
      .TotAmtDistributed.TotalCurrency);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A DebtsGroup group.</summary>
    [Serializable]
    public class DebtsGroup
    {
      /// <summary>
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of DebtsObligation.
      /// </summary>
      [JsonPropertyName("debtsObligation")]
      public Obligation DebtsObligation
      {
        get => debtsObligation ??= new();
        set => debtsObligation = value;
      }

      /// <summary>
      /// A value of DebtsDebt.
      /// </summary>
      [JsonPropertyName("debtsDebt")]
      public ObligationTransaction DebtsDebt
      {
        get => debtsDebt ??= new();
        set => debtsDebt = value;
      }

      /// <summary>
      /// A value of DebtsCollection.
      /// </summary>
      [JsonPropertyName("debtsCollection")]
      public Collection DebtsCollection
      {
        get => debtsCollection ??= new();
        set => debtsCollection = value;
      }

      /// <summary>
      /// A value of DebtsSuppPrsn.
      /// </summary>
      [JsonPropertyName("debtsSuppPrsn")]
      public CsePerson DebtsSuppPrsn
      {
        get => debtsSuppPrsn ??= new();
        set => debtsSuppPrsn = value;
      }

      /// <summary>
      /// A value of DebtsProgram.
      /// </summary>
      [JsonPropertyName("debtsProgram")]
      public Program DebtsProgram
      {
        get => debtsProgram ??= new();
        set => debtsProgram = value;
      }

      /// <summary>
      /// A value of DprProgram.
      /// </summary>
      [JsonPropertyName("dprProgram")]
      public DprProgram DprProgram
      {
        get => dprProgram ??= new();
        set => dprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private ObligationType obligationType;
      private Obligation debtsObligation;
      private ObligationTransaction debtsDebt;
      private Collection debtsCollection;
      private CsePerson debtsSuppPrsn;
      private Program debtsProgram;
      private DprProgram dprProgram;
    }

    /// <summary>
    /// Gets a value of Debts.
    /// </summary>
    [JsonIgnore]
    public Array<DebtsGroup> Debts => debts ??= new(DebtsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Debts for json serialization.
    /// </summary>
    [JsonPropertyName("debts")]
    [Computed]
    public IList<DebtsGroup> Debts_Json
    {
      get => debts;
      set => Debts.Assign(value);
    }

    /// <summary>
    /// A value of AmtToDistribute.
    /// </summary>
    [JsonPropertyName("amtToDistribute")]
    public Common AmtToDistribute
    {
      get => amtToDistribute ??= new();
      set => amtToDistribute = value;
    }

    private Array<DebtsGroup> debts;
    private Common amtToDistribute;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ForProrateGroup group.</summary>
    [Serializable]
    public class ForProrateGroup
    {
      /// <summary>
      /// A value of ForProrate1.
      /// </summary>
      [JsonPropertyName("forProrate1")]
      public Obligation ForProrate1
      {
        get => forProrate1 ??= new();
        set => forProrate1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private Obligation forProrate1;
    }

    /// <summary>
    /// A value of TotAmtToDistByOblig.
    /// </summary>
    [JsonPropertyName("totAmtToDistByOblig")]
    public Common TotAmtToDistByOblig
    {
      get => totAmtToDistByOblig ??= new();
      set => totAmtToDistByOblig = value;
    }

    /// <summary>
    /// A value of TotAmtDistForOblig.
    /// </summary>
    [JsonPropertyName("totAmtDistForOblig")]
    public Common TotAmtDistForOblig
    {
      get => totAmtDistForOblig ??= new();
      set => totAmtDistForOblig = value;
    }

    /// <summary>
    /// A value of TotAmtDistributed.
    /// </summary>
    [JsonPropertyName("totAmtDistributed")]
    public Common TotAmtDistributed
    {
      get => totAmtDistributed ??= new();
      set => totAmtDistributed = value;
    }

    /// <summary>
    /// Gets a value of ForProrate.
    /// </summary>
    [JsonIgnore]
    public Array<ForProrateGroup> ForProrate => forProrate ??= new(
      ForProrateGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ForProrate for json serialization.
    /// </summary>
    [JsonPropertyName("forProrate")]
    [Computed]
    public IList<ForProrateGroup> ForProrate_Json
    {
      get => forProrate;
      set => ForProrate.Assign(value);
    }

    /// <summary>
    /// A value of DelMe1.
    /// </summary>
    [JsonPropertyName("delMe1")]
    public Common DelMe1
    {
      get => delMe1 ??= new();
      set => delMe1 = value;
    }

    private Common totAmtToDistByOblig;
    private Common totAmtDistForOblig;
    private Common totAmtDistributed;
    private Array<ForProrateGroup> forProrate;
    private Common delMe1;
  }
#endregion
}
