// Program: FN_VALIDATE_DEBT_AGAINST_DPR, ID: 372280777, model: 746.
// Short name: SWE02264
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_VALIDATE_DEBT_AGAINST_DPR.
/// </summary>
[Serializable]
public partial class FnValidateDebtAgainstDpr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_VALIDATE_DEBT_AGAINST_DPR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnValidateDebtAgainstDpr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnValidateDebtAgainstDpr.
  /// </summary>
  public FnValidateDebtAgainstDpr(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ------------  ------------	
    // -------------------------------------------------------------------------------------
    // ??/??/??  ????????????			Initial Code.
    // 11/01/08  GVandy	CQ#4387		Distribution 2009
    // -------------------------------------------------------------------------------------------------------------------------------
    export.Eligible.Flag = "N";
    local.CollMonthEndIType.Date = AddDays(import.CollMonthEnd.Date, 1);
    local.CollMonthEndIType.Date = AddMonths(local.CollMonthEndIType.Date, 1);
    local.CollMonthEndIType.Date = AddDays(local.CollMonthEndIType.Date, -1);

    // : Check the Apply-To Code for Balance or Interest.
    if (!IsEmpty(import.DistributionPolicyRule.ApplyTo))
    {
      switch(AsChar(import.DistributionPolicyRule.ApplyTo))
      {
        case 'D':
          if (import.DebtDetail.BalanceDueAmt <= 0)
          {
            return;
          }

          break;
        case 'I':
          if (import.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() <= 0)
          {
            return;
          }

          break;
        default:
          ExitState = "FN0000_INVALID_DIST_PLCY_RUL_RB";

          return;
      }
    }

    // : Check the Debt Function Type for Accruing, Non-Accruing w/ Payment 
    // Schedule, Non-Accuring w/o Payment Schedule & Recovery.
    if (!IsEmpty(import.DistributionPolicyRule.DebtFunctionType))
    {
      switch(AsChar(import.DistributionPolicyRule.DebtFunctionType))
      {
        case 'A':
          if (AsChar(import.ObligationType.Classification) != 'A')
          {
            return;
          }

          break;
        case 'R':
          if (AsChar(import.ObligationType.Classification) != 'R')
          {
            return;
          }

          break;
        case 'W':
          if (AsChar(import.ObligationType.Classification) == 'A')
          {
            return;
          }

          break;
        case 'O':
          if (AsChar(import.ObligationType.Classification) == 'A')
          {
            return;
          }

          break;
        default:
          ExitState = "FN0000_INVALID_DIST_PLCY_RUL_RB";

          return;
      }
    }

    // : Check the Debt Status for Current or Arrears.
    // : Date checks are based on whether or not we are to apply to future.
    switch(AsChar(import.DistributionPolicyRule.DebtState))
    {
      case 'C':
        if (AsChar(import.ObligationType.Classification) == 'A')
        {
          if (AsChar(import.FutureApplAllowed.Flag) == 'Y')
          {
            // : Already qualified the Debts by date in the calling action 
            // block.
          }
          else if (import.CollectionType.SequentialIdentifier == import
            .HardcodedIType.SequentialIdentifier && AsChar
            (import.AllowITypeProcInd.Flag) == 'Y' && !
            Lt(import.CashReceiptDetail.CollectionDate,
            AddDays(import.CollMonthEnd.Date, -import.ItypeWindow.Count)))
          {
            if (!Lt(import.DebtDetail.DueDt, import.CollMonthStart.Date) && !
              Lt(local.CollMonthEndIType.Date, import.DebtDetail.DueDt))
            {
            }
            else
            {
              return;
            }
          }
          else if (!Lt(import.DebtDetail.DueDt, import.CollMonthStart.Date) && !
            Lt(import.CollMonthEnd.Date, import.DebtDetail.DueDt))
          {
          }
          else
          {
            return;
          }
        }
        else
        {
          return;
        }

        break;
      case 'A':
        if (AsChar(import.ObligationType.Classification) == 'A')
        {
          if (Lt(import.DebtDetail.DueDt, import.CollMonthStart.Date))
          {
          }
          else
          {
            return;
          }
        }
        else
        {
          if (!Lt(import.CollMonthEnd.Date, import.DebtDetail.DueDt))
          {
          }
          else
          {
            return;
          }
        }

        break;
      default:
        ExitState = "FN0000_INVALID_DIST_PLCY_RUL_RB";

        return;
    }

    // : Check the Distribute to Order Type Code for Kansas or Interstate.
    if (!IsEmpty(import.DistributionPolicyRule.DistributeToOrderTypeCode))
    {
      if (AsChar(import.Obligation.OrderTypeCode) != AsChar
        (import.DistributionPolicyRule.DistributeToOrderTypeCode))
      {
        return;
      }
    }

    // : Validate the Distribution Policy Rule associated Obligation Types 
    // against the Debt Detail Obligation Type.
    if (!import.ObType.IsEmpty)
    {
      for(import.ObType.Index = 0; import.ObType.Index < import.ObType.Count; ++
        import.ObType.Index)
      {
        if (import.ObligationType.SystemGeneratedIdentifier == import
          .ObType.Item.ObligationType.SystemGeneratedIdentifier)
        {
          goto Test1;
        }
      }

      return;
    }

Test1:

    // : Valid the Distribution Policy Rule associated Programs against the Debt
    // Detail Program.
    if (AsChar(import.ObligationType.SupportedPersonReqInd) == 'Y')
    {
      if (!import.PgmType.IsEmpty)
      {
        for(import.PgmType.Index = 0; import.PgmType.Index < import
          .PgmType.Count; ++import.PgmType.Index)
        {
          if (import.Program.SystemGeneratedIdentifier == import
            .PgmType.Item.Program.SystemGeneratedIdentifier && Equal
            (import.DprProgram.ProgramState,
            import.PgmType.Item.DprProgram.ProgramState))
          {
            // 11/01/08  GVandy  CQ#4387   Distribution 2009
            // - Add checks for TAF / non-TAF supported persons and distribution
            // policy rules.
            switch(AsChar(import.PgmType.Item.DprProgram.AssistanceInd))
            {
              case 'T':
                if (AsChar(import.SuppPrsnTafInd.Flag) != 'Y')
                {
                  return;
                }

                break;
              case 'N':
                if (AsChar(import.SuppPrsnTafInd.Flag) != 'N')
                {
                  return;
                }

                break;
              default:
                break;
            }

            goto Test2;
          }
        }

        return;
      }
    }

Test2:

    // : Debt is eligible for distribution.
    export.Eligible.Flag = "Y";
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
    /// <summary>A ObTypeGroup group.</summary>
    [Serializable]
    public class ObTypeGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private ObligationType obligationType;
    }

    /// <summary>A PgmTypeGroup group.</summary>
    [Serializable]
    public class PgmTypeGroup
    {
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
      public const int Capacity = 200;

      private Program program;
      private DprProgram dprProgram;
    }

    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

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

    /// <summary>
    /// A value of Persistant.
    /// </summary>
    [JsonPropertyName("persistant")]
    public ObligationTransaction Persistant
    {
      get => persistant ??= new();
      set => persistant = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// Gets a value of ObType.
    /// </summary>
    [JsonIgnore]
    public Array<ObTypeGroup> ObType => obType ??= new(ObTypeGroup.Capacity);

    /// <summary>
    /// Gets a value of ObType for json serialization.
    /// </summary>
    [JsonPropertyName("obType")]
    [Computed]
    public IList<ObTypeGroup> ObType_Json
    {
      get => obType;
      set => ObType.Assign(value);
    }

    /// <summary>
    /// Gets a value of PgmType.
    /// </summary>
    [JsonIgnore]
    public Array<PgmTypeGroup> PgmType =>
      pgmType ??= new(PgmTypeGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmType for json serialization.
    /// </summary>
    [JsonPropertyName("pgmType")]
    [Computed]
    public IList<PgmTypeGroup> PgmType_Json
    {
      get => pgmType;
      set => PgmType.Assign(value);
    }

    /// <summary>
    /// A value of CollMonthStart.
    /// </summary>
    [JsonPropertyName("collMonthStart")]
    public DateWorkArea CollMonthStart
    {
      get => collMonthStart ??= new();
      set => collMonthStart = value;
    }

    /// <summary>
    /// A value of CollMonthEnd.
    /// </summary>
    [JsonPropertyName("collMonthEnd")]
    public DateWorkArea CollMonthEnd
    {
      get => collMonthEnd ??= new();
      set => collMonthEnd = value;
    }

    /// <summary>
    /// A value of FutureApplAllowed.
    /// </summary>
    [JsonPropertyName("futureApplAllowed")]
    public Common FutureApplAllowed
    {
      get => futureApplAllowed ??= new();
      set => futureApplAllowed = value;
    }

    /// <summary>
    /// A value of HardcodedIType.
    /// </summary>
    [JsonPropertyName("hardcodedIType")]
    public CollectionType HardcodedIType
    {
      get => hardcodedIType ??= new();
      set => hardcodedIType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of AllowITypeProcInd.
    /// </summary>
    [JsonPropertyName("allowITypeProcInd")]
    public Common AllowITypeProcInd
    {
      get => allowITypeProcInd ??= new();
      set => allowITypeProcInd = value;
    }

    /// <summary>
    /// A value of ItypeWindow.
    /// </summary>
    [JsonPropertyName("itypeWindow")]
    public Common ItypeWindow
    {
      get => itypeWindow ??= new();
      set => itypeWindow = value;
    }

    /// <summary>
    /// A value of SuppPrsnTafInd.
    /// </summary>
    [JsonPropertyName("suppPrsnTafInd")]
    public Common SuppPrsnTafInd
    {
      get => suppPrsnTafInd ??= new();
      set => suppPrsnTafInd = value;
    }

    private DistributionPolicyRule distributionPolicyRule;
    private Obligation obligation;
    private ObligationType obligationType;
    private Program program;
    private DprProgram dprProgram;
    private ObligationTransaction persistant;
    private DebtDetail debtDetail;
    private CollectionType collectionType;
    private Array<ObTypeGroup> obType;
    private Array<PgmTypeGroup> pgmType;
    private DateWorkArea collMonthStart;
    private DateWorkArea collMonthEnd;
    private Common futureApplAllowed;
    private CollectionType hardcodedIType;
    private CashReceiptDetail cashReceiptDetail;
    private Common allowITypeProcInd;
    private Common itypeWindow;
    private Common suppPrsnTafInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Eligible.
    /// </summary>
    [JsonPropertyName("eligible")]
    public Common Eligible
    {
      get => eligible ??= new();
      set => eligible = value;
    }

    private Common eligible;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ObTypeMatchFound.
    /// </summary>
    [JsonPropertyName("obTypeMatchFound")]
    public Common ObTypeMatchFound
    {
      get => obTypeMatchFound ??= new();
      set => obTypeMatchFound = value;
    }

    /// <summary>
    /// A value of PgmTypeMatchFound.
    /// </summary>
    [JsonPropertyName("pgmTypeMatchFound")]
    public Common PgmTypeMatchFound
    {
      get => pgmTypeMatchFound ??= new();
      set => pgmTypeMatchFound = value;
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
    /// A value of CollMonthEndIType.
    /// </summary>
    [JsonPropertyName("collMonthEndIType")]
    public DateWorkArea CollMonthEndIType
    {
      get => collMonthEndIType ??= new();
      set => collMonthEndIType = value;
    }

    private Common obTypeMatchFound;
    private Common pgmTypeMatchFound;
    private Program program;
    private DateWorkArea collMonthEndIType;
  }
#endregion
}
