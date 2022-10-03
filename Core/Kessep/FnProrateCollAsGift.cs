// Program: FN_PRORATE_COLL_AS_GIFT, ID: 372280590, model: 746.
// Short name: SWE02314
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PRORATE_COLL_AS_GIFT.
/// </summary>
[Serializable]
public partial class FnProrateCollAsGift: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PRORATE_COLL_AS_GIFT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProrateCollAsGift(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProrateCollAsGift.
  /// </summary>
  public FnProrateCollAsGift(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // : Group debts by Supported Person.
    local.TotAmtDueForGrp.TotalCurrency = 0;
    local.TotAmtDistForGrp.TotalCurrency = 0;

    for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
      import.Debts.Index)
    {
      if (import.Debts.Item.DebtsDebt.Amount == 0)
      {
        continue;
      }

      if (local.ForProrate.IsEmpty)
      {
        local.ForProrate.Index = 0;
        local.ForProrate.CheckSize();

        local.ForProrate.Update.ForProrateSuppPrsn.Number =
          import.Debts.Item.DebtsSuppPrsn.Number;
        local.ForProrate.Update.ForProrate1.TotalCurrency =
          import.Debts.Item.DebtsDebt.Amount;
        local.TotAmtDueForGrp.TotalCurrency += local.ForProrate.Item.
          ForProrate1.TotalCurrency;

        continue;
      }

      for(local.ForProrate.Index = 0; local.ForProrate.Index < local
        .ForProrate.Count; ++local.ForProrate.Index)
      {
        if (!local.ForProrate.CheckSize())
        {
          break;
        }

        if (Equal(import.Debts.Item.DebtsSuppPrsn.Number,
          local.ForProrate.Item.ForProrateSuppPrsn.Number))
        {
          local.ForProrate.Update.ForProrate1.TotalCurrency =
            local.ForProrate.Item.ForProrate1.TotalCurrency + import
            .Debts.Item.DebtsDebt.Amount;
          local.TotAmtDueForGrp.TotalCurrency += local.ForProrate.Item.
            ForProrate1.TotalCurrency;

          goto Next1;
        }
      }

      local.ForProrate.CheckIndex();

      local.ForProrate.Index = local.ForProrate.Count;
      local.ForProrate.CheckSize();

      local.ForProrate.Update.ForProrateSuppPrsn.Number =
        import.Debts.Item.DebtsSuppPrsn.Number;
      local.ForProrate.Update.ForProrate1.TotalCurrency =
        import.Debts.Item.DebtsDebt.Amount;
      local.TotAmtDueForGrp.TotalCurrency += local.ForProrate.Item.ForProrate1.
        TotalCurrency;

Next1:
      ;
    }

    // : Now we distribute the payment prorated across each Support Person 
    // appling the payment to the oldest Debt Detail first.
    // : Prorate collection across debts with the same Supported Person by Due 
    // Date - Oldest First!!
    for(local.ForProrate.Index = 0; local.ForProrate.Index < local
      .ForProrate.Count; ++local.ForProrate.Index)
    {
      if (!local.ForProrate.CheckSize())
      {
        break;
      }

      if (local.TotAmtDistForGrp.TotalCurrency == import
        .AmtToDistribute.TotalCurrency)
      {
        // -- 02/21/2018 GVandy  CQ56370  If all money has been distributed then
        // escape.
        //    Corrects issue where there are 4 kids on the obligation and only 
        // $.02 remaining to
        //    distribute.  Otherwise, due to the rounding, it was applying .01 
        // to each of the
        //    first 3 kids and the last kid got -.01 which caused the payment to
        // ultimately go
        //    into suspense as a SYSTEM ERROR.
        break;
      }

      if (local.ForProrate.Index + 1 == local.ForProrate.Count)
      {
        local.TotAmtToDistByPerson.TotalCurrency =
          import.AmtToDistribute.TotalCurrency - local
          .TotAmtDistForGrp.TotalCurrency;
      }
      else
      {
        local.TmpForCalc.TotalReal = import.AmtToDistribute.TotalCurrency * local
          .ForProrate.Item.ForProrate1.TotalCurrency;
        local.TotAmtToDistByPerson.TotalCurrency =
          Math.Round(
            local.TmpForCalc.TotalReal /
          local.TotAmtDueForGrp.TotalCurrency, 2,
          MidpointRounding.AwayFromZero);
      }

      for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
        import.Debts.Index)
      {
        if (import.Debts.Item.DebtsDebt.Amount == 0)
        {
          continue;
        }

        if (Equal(import.Debts.Item.DebtsSuppPrsn.Number,
          local.ForProrate.Item.ForProrateSuppPrsn.Number))
        {
          import.Debts.Update.DebtsCollection.AppliedToCode = "G";
          import.Debts.Update.DebtsCollection.Amount =
            local.TotAmtToDistByPerson.TotalCurrency;
          local.TotAmtDistForGrp.TotalCurrency += import.Debts.Item.
            DebtsCollection.Amount;

          goto Next2;
        }
      }

Next2:
      ;
    }

    local.ForProrate.CheckIndex();
    import.AmtToDistribute.TotalCurrency = 0;
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
      /// A value of DebtsObligationType.
      /// </summary>
      [JsonPropertyName("debtsObligationType")]
      public ObligationType DebtsObligationType
      {
        get => debtsObligationType ??= new();
        set => debtsObligationType = value;
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
      /// A value of DebtsDebtDetail.
      /// </summary>
      [JsonPropertyName("debtsDebtDetail")]
      public DebtDetail DebtsDebtDetail
      {
        get => debtsDebtDetail ??= new();
        set => debtsDebtDetail = value;
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
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
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

      private ObligationType debtsObligationType;
      private Obligation debtsObligation;
      private ObligationTransaction debtsDebt;
      private DebtDetail debtsDebtDetail;
      private Collection debtsCollection;
      private CsePerson debtsSuppPrsn;
      private Program program;
      private DprProgram dprProgram;
    }

    /// <summary>
    /// Gets a value of Debts.
    /// </summary>
    [JsonIgnore]
    public Array<DebtsGroup> Debts => debts ??= new(DebtsGroup.Capacity);

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
      /// A value of ForProrateSuppPrsn.
      /// </summary>
      [JsonPropertyName("forProrateSuppPrsn")]
      public CsePerson ForProrateSuppPrsn
      {
        get => forProrateSuppPrsn ??= new();
        set => forProrateSuppPrsn = value;
      }

      /// <summary>
      /// A value of ForProrate1.
      /// </summary>
      [JsonPropertyName("forProrate1")]
      public Common ForProrate1
      {
        get => forProrate1 ??= new();
        set => forProrate1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private CsePerson forProrateSuppPrsn;
      private Common forProrate1;
    }

    /// <summary>
    /// A value of TotAmtDueForGrp.
    /// </summary>
    [JsonPropertyName("totAmtDueForGrp")]
    public Common TotAmtDueForGrp
    {
      get => totAmtDueForGrp ??= new();
      set => totAmtDueForGrp = value;
    }

    /// <summary>
    /// A value of TotAmtDistForGrp.
    /// </summary>
    [JsonPropertyName("totAmtDistForGrp")]
    public Common TotAmtDistForGrp
    {
      get => totAmtDistForGrp ??= new();
      set => totAmtDistForGrp = value;
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
    /// A value of TotAmtToDistByPerson.
    /// </summary>
    [JsonPropertyName("totAmtToDistByPerson")]
    public Common TotAmtToDistByPerson
    {
      get => totAmtToDistByPerson ??= new();
      set => totAmtToDistByPerson = value;
    }

    /// <summary>
    /// A value of TmpForCalc.
    /// </summary>
    [JsonPropertyName("tmpForCalc")]
    public Common TmpForCalc
    {
      get => tmpForCalc ??= new();
      set => tmpForCalc = value;
    }

    private Common totAmtDueForGrp;
    private Common totAmtDistForGrp;
    private Array<ForProrateGroup> forProrate;
    private Common totAmtToDistByPerson;
    private Common tmpForCalc;
  }
#endregion
}
