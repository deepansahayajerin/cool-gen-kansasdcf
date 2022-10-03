// Program: FN_RECHECK_DEBTS_AGAINST_DPR, ID: 374493199, model: 746.
// Short name: SWE02530
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RECHECK_DEBTS_AGAINST_DPR.
/// </summary>
[Serializable]
public partial class FnRecheckDebtsAgainstDpr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RECHECK_DEBTS_AGAINST_DPR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRecheckDebtsAgainstDpr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRecheckDebtsAgainstDpr.
  /// </summary>
  public FnRecheckDebtsAgainstDpr(IContext context, Import import, Export export)
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
    for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
      import.Debts.Index)
    {
      if (AsChar(import.Debts.Item.DebtsObligationType.SupportedPersonReqInd) ==
        'Y')
      {
        // : Clear the Local Temporary Group view.
        local.Tmp.Index = 0;
        local.Tmp.Clear();

        for(local.Null1.Index = 0; local.Null1.Index < local.Null1.Count; ++
          local.Null1.Index)
        {
          if (local.Tmp.IsFull)
          {
            break;
          }

          local.Tmp.Next();
        }

        // : Load the Local Temporary Group view with the specific Supported 
        // Person Program History.
        local.SuppPrsnTafInd.Flag = "";

        for(import.PgmHist.Index = 0; import.PgmHist.Index < import
          .PgmHist.Count; ++import.PgmHist.Index)
        {
          if (Equal(import.PgmHist.Item.PgmHistSuppPrsn.Number,
            import.Debts.Item.DebtsSuppPrsn.Number))
          {
            local.SuppPrsnTafInd.Flag = import.PgmHist.Item.TafInd.Flag;

            local.Tmp.Index = 0;
            local.Tmp.Clear();

            for(import.PgmHist.Item.PgmHistDtl.Index = 0; import
              .PgmHist.Item.PgmHistDtl.Index < import
              .PgmHist.Item.PgmHistDtl.Count; ++
              import.PgmHist.Item.PgmHistDtl.Index)
            {
              if (local.Tmp.IsFull)
              {
                break;
              }

              local.Tmp.Update.TmpProgram.Assign(
                import.PgmHist.Item.PgmHistDtl.Item.PgmHistDtlProgram);
              MovePersonProgram(import.PgmHist.Item.PgmHistDtl.Item.
                PgmHistDtlPersonProgram, local.Tmp.Update.TmpPersonProgram);
              local.Tmp.Next();
            }
          }
        }

        if (Lt(import.Collection.Date, import.PrworaDateOfConversion.Date))
        {
          UseFnDeterminePgmForDbtDist1();
        }
        else
        {
          UseFnDeterminePgmForDbtDist2();
        }

        if (!import.PgmType.IsEmpty)
        {
          for(import.PgmType.Index = 0; import.PgmType.Index < import
            .PgmType.Count; ++import.PgmType.Index)
          {
            if (local.Program.SystemGeneratedIdentifier == import
              .PgmType.Item.Program.SystemGeneratedIdentifier && Equal
              (local.DprProgram.ProgramState,
              import.PgmType.Item.DprProgram.ProgramState))
            {
              // 11/01/08  GVandy  CQ#4387   Distribution 2009
              // - Add checks for TAF / non-TAF supported persons and 
              // distribution policy rules.
              switch(AsChar(import.PgmType.Item.DprProgram.AssistanceInd))
              {
                case 'T':
                  if (AsChar(local.SuppPrsnTafInd.Flag) == 'Y')
                  {
                  }
                  else
                  {
                    goto Test;
                  }

                  break;
                case 'N':
                  if (AsChar(local.SuppPrsnTafInd.Flag) == 'N')
                  {
                  }
                  else
                  {
                    goto Test;
                  }

                  break;
                default:
                  break;
              }

              goto Next;
            }

Test:
            ;
          }

          import.ResetDprProcInd.Flag = "Y";

          // : See the Debt Detail balance to zero - That way no additional 
          // collections will be applied to the debt during the processing of
          // this DPR.
          if (AsChar(import.DistributionPolicyRule.ApplyTo) == 'D')
          {
            import.Debts.Update.DebtsDebtDetail.BalanceDueAmt = 0;
          }
          else
          {
            import.Debts.Update.DebtsDebtDetail.InterestBalanceDueAmt = 0;
          }
        }
      }

Next:
      ;
    }
  }

  private static void MoveHhHist1(Import.HhHistGroup source,
    FnDeterminePgmForDbtDist1.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(Import.HhHistGroup source,
    FnDeterminePgmForDbtDist2.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl2);
  }

  private static void MoveHhHistDtl1(Import.HhHistDtlGroup source,
    FnDeterminePgmForDbtDist1.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(Import.HhHistDtlGroup source,
    FnDeterminePgmForDbtDist2.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveLegal1(Import.LegalGroup source,
    FnDeterminePgmForDbtDist1.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl1);
  }

  private static void MoveLegal2(Import.LegalGroup source,
    FnDeterminePgmForDbtDist2.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl2);
  }

  private static void MoveLegalDtl1(Import.LegalDtlGroup source,
    FnDeterminePgmForDbtDist1.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl2(Import.LegalDtlGroup source,
    FnDeterminePgmForDbtDist2.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveTmpToPgmHistDtl1(Local.TmpGroup source,
    FnDeterminePgmForDbtDist1.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.TmpProgram);
    MovePersonProgram(source.TmpPersonProgram, target.PgmHistDtlPersonProgram);
  }

  private static void MoveTmpToPgmHistDtl2(Local.TmpGroup source,
    FnDeterminePgmForDbtDist2.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.TmpProgram);
    MovePersonProgram(source.TmpPersonProgram, target.PgmHistDtlPersonProgram);
  }

  private void UseFnDeterminePgmForDbtDist1()
  {
    var useImport = new FnDeterminePgmForDbtDist1.Import();
    var useExport = new FnDeterminePgmForDbtDist1.Export();

    import.Legal.CopyTo(useImport.Legal, MoveLegal1);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist1);
    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    useImport.Collection.Date = import.Collection.Date;
    useImport.HardcodedAccruingClass.Classification =
      import.HardcodedAccruingClass.Classification;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      import.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(import.HardcodedAf);
    useImport.HardcodedAfi.Assign(import.HardcodedAfi);
    useImport.HardcodedFc.Assign(import.HardcodedFc);
    useImport.HardcodedFci.Assign(import.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(import.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(import.HardcodedNai);
    useImport.HardcodedNc.Assign(import.HardcodedNc);
    useImport.HardcodedNf.Assign(import.HardcodedNf);
    useImport.HardcodedMai.Assign(import.HardcodedMai);
    useImport.HardcodedPa.ProgramState = import.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = import.HardcodedTa.ProgramState;
    useImport.HardcodedNaDprProgram.ProgramState =
      import.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedCa.ProgramState = import.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = import.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = import.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = import.HardcodedUk.ProgramState;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    local.Tmp.CopyTo(useImport.PgmHistDtl, MoveTmpToPgmHistDtl1);
    MoveObligationType(import.Debts.Item.DebtsObligationType,
      useImport.ObligationType);
    MoveObligation(import.Debts.Item.DebtsObligation, useImport.Obligation);
    useImport.DebtDetail.Assign(import.Debts.Item.DebtsDebtDetail);
    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;

    Call(FnDeterminePgmForDbtDist1.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
  }

  private void UseFnDeterminePgmForDbtDist2()
  {
    var useImport = new FnDeterminePgmForDbtDist2.Import();
    var useExport = new FnDeterminePgmForDbtDist2.Export();

    import.Legal.CopyTo(useImport.Legal, MoveLegal2);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);
    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    useImport.CollectionType.SequentialIdentifier =
      import.CollectionType.SequentialIdentifier;
    useImport.Collection.Date = import.Collection.Date;
    useImport.HardcodedAccruingClass.Classification =
      import.HardcodedAccruingClass.Classification;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      import.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(import.HardcodedAf);
    useImport.HardcodedAfi.Assign(import.HardcodedAfi);
    useImport.HardcodedFc.Assign(import.HardcodedFc);
    useImport.HardcodedFci.Assign(import.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(import.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(import.HardcodedNai);
    useImport.HardcodedNc.Assign(import.HardcodedNc);
    useImport.HardcodedNf.Assign(import.HardcodedNf);
    useImport.HardcodedMai.Assign(import.HardcodedMai);
    useImport.HardcodedPa.ProgramState = import.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = import.HardcodedTa.ProgramState;
    useImport.HardcodedNaDprProgram.ProgramState =
      import.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedCa.ProgramState = import.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = import.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = import.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = import.HardcodedUk.ProgramState;
    useImport.HardcodedFFedType.SequentialIdentifier =
      import.HardcodedFedFType.SequentialIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    local.Tmp.CopyTo(useImport.PgmHistDtl, MoveTmpToPgmHistDtl2);
    MoveObligationType(import.Debts.Item.DebtsObligationType,
      useImport.ObligationType);
    MoveObligation(import.Debts.Item.DebtsObligation, useImport.Obligation);
    useImport.DebtDetail.Assign(import.Debts.Item.DebtsDebtDetail);
    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;

    Call(FnDeterminePgmForDbtDist2.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Assign(useExport.Program);
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
      /// A value of DebtsProgram.
      /// </summary>
      [JsonPropertyName("debtsProgram")]
      public Program DebtsProgram
      {
        get => debtsProgram ??= new();
        set => debtsProgram = value;
      }

      /// <summary>
      /// A value of DebtsDprProgram.
      /// </summary>
      [JsonPropertyName("debtsDprProgram")]
      public DprProgram DebtsDprProgram
      {
        get => debtsDprProgram ??= new();
        set => debtsDprProgram = value;
      }

      /// <summary>
      /// A value of DebtsLegalAction.
      /// </summary>
      [JsonPropertyName("debtsLegalAction")]
      public LegalAction DebtsLegalAction
      {
        get => debtsLegalAction ??= new();
        set => debtsLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private ObligationType debtsObligationType;
      private Obligation debtsObligation;
      private ObligationTransaction debtsDebt;
      private DebtDetail debtsDebtDetail;
      private Collection debtsCollection;
      private CsePerson debtsSuppPrsn;
      private Program debtsProgram;
      private DprProgram debtsDprProgram;
      private LegalAction debtsLegalAction;
    }

    /// <summary>A PgmHistGroup group.</summary>
    [Serializable]
    public class PgmHistGroup
    {
      /// <summary>
      /// A value of PgmHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("pgmHistSuppPrsn")]
      public CsePerson PgmHistSuppPrsn
      {
        get => pgmHistSuppPrsn ??= new();
        set => pgmHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of PgmHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<PgmHistDtlGroup> PgmHistDtl => pgmHistDtl ??= new(
        PgmHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of PgmHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("pgmHistDtl")]
      [Computed]
      public IList<PgmHistDtlGroup> PgmHistDtl_Json
      {
        get => pgmHistDtl;
        set => PgmHistDtl.Assign(value);
      }

      /// <summary>
      /// A value of TafInd.
      /// </summary>
      [JsonPropertyName("tafInd")]
      public Common TafInd
      {
        get => tafInd ??= new();
        set => tafInd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson pgmHistSuppPrsn;
      private Array<PgmHistDtlGroup> pgmHistDtl;
      private Common tafInd;
    }

    /// <summary>A PgmHistDtlGroup group.</summary>
    [Serializable]
    public class PgmHistDtlGroup
    {
      /// <summary>
      /// A value of PgmHistDtlProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlProgram")]
      public Program PgmHistDtlProgram
      {
        get => pgmHistDtlProgram ??= new();
        set => pgmHistDtlProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlPersonProgram")]
      public PersonProgram PgmHistDtlPersonProgram
      {
        get => pgmHistDtlPersonProgram ??= new();
        set => pgmHistDtlPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlProgram;
      private PersonProgram pgmHistDtlPersonProgram;
    }

    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of LegalSuppPrsn.
      /// </summary>
      [JsonPropertyName("legalSuppPrsn")]
      public CsePerson LegalSuppPrsn
      {
        get => legalSuppPrsn ??= new();
        set => legalSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of LegalDtl.
      /// </summary>
      [JsonIgnore]
      public Array<LegalDtlGroup> LegalDtl => legalDtl ??= new(
        LegalDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of LegalDtl for json serialization.
      /// </summary>
      [JsonPropertyName("legalDtl")]
      [Computed]
      public IList<LegalDtlGroup> LegalDtl_Json
      {
        get => legalDtl;
        set => LegalDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson legalSuppPrsn;
      private Array<LegalDtlGroup> legalDtl;
    }

    /// <summary>A LegalDtlGroup group.</summary>
    [Serializable]
    public class LegalDtlGroup
    {
      /// <summary>
      /// A value of LegalDtl1.
      /// </summary>
      [JsonPropertyName("legalDtl1")]
      public LegalAction LegalDtl1
      {
        get => legalDtl1 ??= new();
        set => legalDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legalDtl1;
    }

    /// <summary>A HhHistGroup group.</summary>
    [Serializable]
    public class HhHistGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsn")]
      public CsePerson HhHistSuppPrsn
      {
        get => hhHistSuppPrsn ??= new();
        set => hhHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlGroup> HhHistDtl => hhHistDtl ??= new(
        HhHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtl")]
      [Computed]
      public IList<HhHistDtlGroup> HhHistDtl_Json
      {
        get => hhHistDtl;
        set => HhHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsn;
      private Array<HhHistDtlGroup> hhHistDtl;
    }

    /// <summary>A HhHistDtlGroup group.</summary>
    [Serializable]
    public class HhHistDtlGroup
    {
      /// <summary>
      /// A value of HhHistDtlImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHousehold")]
      public ImHousehold HhHistDtlImHousehold
      {
        get => hhHistDtlImHousehold ??= new();
        set => hhHistDtlImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlImHouseholdMbrMnthlySum
      {
        get => hhHistDtlImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlImHouseholdMbrMnthlySum;
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
    /// Gets a value of PgmHist.
    /// </summary>
    [JsonIgnore]
    public Array<PgmHistGroup> PgmHist =>
      pgmHist ??= new(PgmHistGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmHist for json serialization.
    /// </summary>
    [JsonPropertyName("pgmHist")]
    [Computed]
    public IList<PgmHistGroup> PgmHist_Json
    {
      get => pgmHist;
      set => PgmHist.Assign(value);
    }

    /// <summary>
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHist.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHist for json serialization.
    /// </summary>
    [JsonPropertyName("hhHist")]
    [Computed]
    public IList<HhHistGroup> HhHist_Json
    {
      get => hhHist;
      set => HhHist.Assign(value);
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMjType.
    /// </summary>
    [JsonPropertyName("hardcodedMjType")]
    public ObligationType HardcodedMjType
    {
      get => hardcodedMjType ??= new();
      set => hardcodedMjType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of Hardcoded718B.
    /// </summary>
    [JsonPropertyName("hardcoded718B")]
    public ObligationType Hardcoded718B
    {
      get => hardcoded718B ??= new();
      set => hardcoded718B = value;
    }

    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNaProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaProgram")]
    public Program HardcodedNaProgram
    {
      get => hardcodedNaProgram ??= new();
      set => hardcodedNaProgram = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    /// <summary>
    /// A value of HardcodedPa.
    /// </summary>
    [JsonPropertyName("hardcodedPa")]
    public DprProgram HardcodedPa
    {
      get => hardcodedPa ??= new();
      set => hardcodedPa = value;
    }

    /// <summary>
    /// A value of HardcodedTa.
    /// </summary>
    [JsonPropertyName("hardcodedTa")]
    public DprProgram HardcodedTa
    {
      get => hardcodedTa ??= new();
      set => hardcodedTa = value;
    }

    /// <summary>
    /// A value of HardcodedNaDprProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaDprProgram")]
    public DprProgram HardcodedNaDprProgram
    {
      get => hardcodedNaDprProgram ??= new();
      set => hardcodedNaDprProgram = value;
    }

    /// <summary>
    /// A value of HardcodedCa.
    /// </summary>
    [JsonPropertyName("hardcodedCa")]
    public DprProgram HardcodedCa
    {
      get => hardcodedCa ??= new();
      set => hardcodedCa = value;
    }

    /// <summary>
    /// A value of HardcodedUd.
    /// </summary>
    [JsonPropertyName("hardcodedUd")]
    public DprProgram HardcodedUd
    {
      get => hardcodedUd ??= new();
      set => hardcodedUd = value;
    }

    /// <summary>
    /// A value of HardcodedUp.
    /// </summary>
    [JsonPropertyName("hardcodedUp")]
    public DprProgram HardcodedUp
    {
      get => hardcodedUp ??= new();
      set => hardcodedUp = value;
    }

    /// <summary>
    /// A value of HardcodedUk.
    /// </summary>
    [JsonPropertyName("hardcodedUk")]
    public DprProgram HardcodedUk
    {
      get => hardcodedUk ??= new();
      set => hardcodedUk = value;
    }

    /// <summary>
    /// A value of HardcodedFedFType.
    /// </summary>
    [JsonPropertyName("hardcodedFedFType")]
    public CollectionType HardcodedFedFType
    {
      get => hardcodedFedFType ??= new();
      set => hardcodedFedFType = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    /// <summary>
    /// A value of PrworaDateOfConversion.
    /// </summary>
    [JsonPropertyName("prworaDateOfConversion")]
    public DateWorkArea PrworaDateOfConversion
    {
      get => prworaDateOfConversion ??= new();
      set => prworaDateOfConversion = value;
    }

    /// <summary>
    /// A value of ResetDprProcInd.
    /// </summary>
    [JsonPropertyName("resetDprProcInd")]
    public Common ResetDprProcInd
    {
      get => resetDprProcInd ??= new();
      set => resetDprProcInd = value;
    }

    private DistributionPolicyRule distributionPolicyRule;
    private Array<DebtsGroup> debts;
    private Array<PgmHistGroup> pgmHist;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private LegalAction legalAction;
    private CollectionType collectionType;
    private DateWorkArea collection;
    private Array<PgmTypeGroup> pgmType;
    private ObligationType hardcodedAccruingClass;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private ObligationType hardcoded718B;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNaProgram;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private DprProgram hardcodedPa;
    private DprProgram hardcodedTa;
    private DprProgram hardcodedNaDprProgram;
    private DprProgram hardcodedCa;
    private DprProgram hardcodedUd;
    private DprProgram hardcodedUp;
    private DprProgram hardcodedUk;
    private CollectionType hardcodedFedFType;
    private Obligation hardcodedSecondary;
    private DateWorkArea prworaDateOfConversion;
    private Common resetDprProcInd;
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
    /// <summary>A TmpGroup group.</summary>
    [Serializable]
    public class TmpGroup
    {
      /// <summary>
      /// A value of TmpProgram.
      /// </summary>
      [JsonPropertyName("tmpProgram")]
      public Program TmpProgram
      {
        get => tmpProgram ??= new();
        set => tmpProgram = value;
      }

      /// <summary>
      /// A value of TmpPersonProgram.
      /// </summary>
      [JsonPropertyName("tmpPersonProgram")]
      public PersonProgram TmpPersonProgram
      {
        get => tmpPersonProgram ??= new();
        set => tmpPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program tmpProgram;
      private PersonProgram tmpPersonProgram;
    }

    /// <summary>A NullGroup group.</summary>
    [Serializable]
    public class NullGroup
    {
      /// <summary>
      /// A value of Null2.
      /// </summary>
      [JsonPropertyName("null2")]
      public TextWorkArea Null2
      {
        get => null2 ??= new();
        set => null2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private TextWorkArea null2;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// Gets a value of Tmp.
    /// </summary>
    [JsonIgnore]
    public Array<TmpGroup> Tmp => tmp ??= new(TmpGroup.Capacity);

    /// <summary>
    /// Gets a value of Tmp for json serialization.
    /// </summary>
    [JsonPropertyName("tmp")]
    [Computed]
    public IList<TmpGroup> Tmp_Json
    {
      get => tmp;
      set => Tmp.Assign(value);
    }

    /// <summary>
    /// Gets a value of Null1.
    /// </summary>
    [JsonIgnore]
    public Array<NullGroup> Null1 => null1 ??= new(NullGroup.Capacity);

    /// <summary>
    /// Gets a value of Null1 for json serialization.
    /// </summary>
    [JsonPropertyName("null1")]
    [Computed]
    public IList<NullGroup> Null1_Json
    {
      get => null1;
      set => Null1.Assign(value);
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private Common suppPrsnTafInd;
    private DprProgram dprProgram;
    private Program program;
    private Array<TmpGroup> tmp;
    private Array<NullGroup> null1;
    private DateWorkArea collection;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingSupportedPerson.
    /// </summary>
    [JsonPropertyName("existingSupportedPerson")]
    public CsePerson ExistingSupportedPerson
    {
      get => existingSupportedPerson ??= new();
      set => existingSupportedPerson = value;
    }

    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private DebtDetail existingDebtDetail;
    private CsePerson existingSupportedPerson;
  }
#endregion
}
