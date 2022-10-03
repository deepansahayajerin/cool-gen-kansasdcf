// Program: FN_DIST_CRD_BALANCE_AS_GIFT, ID: 372279913, model: 746.
// Short name: SWE02362
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DIST_CRD_BALANCE_AS_GIFT.
/// </summary>
[Serializable]
public partial class FnDistCrdBalanceAsGift: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DIST_CRD_BALANCE_AS_GIFT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDistCrdBalanceAsGift(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDistCrdBalanceAsGift.
  /// </summary>
  public FnDistCrdBalanceAsGift(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ProrateToCsOnly.Flag = "N";

    import.Group.Index = import.Group.Count - 1;
    import.Group.CheckSize();

    local.AmtToDistribute.TotalCurrency = import.AmtToDistribute.TotalCurrency;
    local.Collection.Date = import.PersistantCashReceiptDetail.CollectionDate;

    local.Group.Index = 0;
    local.Group.Clear();

    foreach(var item in ReadObligationTypeObligationDebtDebtDetailObligor())
    {
      if (AsChar(entities.ExistingObligationType.Classification) == AsChar
        (import.HardcodedVolClass.Classification))
      {
        local.Group.Next();

        continue;
      }

      if (entities.ExistingObligationType.SystemGeneratedIdentifier == import
        .HardcodedCsType.SystemGeneratedIdentifier)
      {
        local.ProrateToCsOnly.Flag = "Y";
      }

      if (AsChar(import.ProcessSecObligOnly.Flag) == 'Y')
      {
        if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) != AsChar
          (import.HardcodedSecondary.PrimarySecondaryCode))
        {
          local.Group.Next();

          continue;
        }
      }
      else
      {
        if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
          (import.HardcodedSecondary.PrimarySecondaryCode))
        {
          local.Group.Next();

          continue;
        }

        // : If the Court Order is on the CRD, then we can only distribute 
        // payments to Obligations ordered by the Court Order.
        if (!IsEmpty(import.DistBy.CourtOrderNumber))
        {
          if (!ReadLegalAction())
          {
            local.Group.Next();

            continue;
          }
        }
      }

      local.Group.Update.Obligation.Assign(entities.ExistingObligation);
      local.Group.Update.ObligationType.Assign(entities.ExistingObligationType);
      MoveObligationTransaction(entities.ExistingDebt, local.Group.Update.Debt);
      MoveDebtDetail(entities.ExistingDebtDetail, local.Group.Update.DebtDetail);
        
      local.Group.Update.SupportedPerson.Number =
        entities.ExistingKeyOnlySuppPrsn.Number;
      local.Group.Next();
    }

    if (local.Group.IsEmpty)
    {
      return;
    }

    if (AsChar(local.ProrateToCsOnly.Flag) == 'Y')
    {
      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (local.Group.Item.ObligationType.SystemGeneratedIdentifier != import
          .HardcodedCsType.SystemGeneratedIdentifier)
        {
          local.Group.Update.Debt.Amount = 0;
        }
      }
    }

    UseFnProrateCollAsGift();

    // : Move the details of the collections and debts to the export group view.
    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (local.Group.Item.Collection.Amount == 0)
      {
        continue;
      }

      if (import.Group.Index + 1 >= Import.GroupGroup.Capacity)
      {
        ExitState = "FN0000_MAX_GRP_VW_FOR_DIST_RB";

        return;
      }

      ++import.Group.Index;
      import.Group.CheckSize();

      import.Group.Update.Obligation.Assign(local.Group.Item.Obligation);
      import.Group.Update.ObligationType.
        Assign(local.Group.Item.ObligationType);
      import.Group.Update.Debt.SystemGeneratedIdentifier =
        local.Group.Item.Debt.SystemGeneratedIdentifier;
      MoveCollection(local.Group.Item.Collection, import.Group.Update.Collection);
        
      import.Group.Update.SuppPrsn.Number =
        local.Group.Item.SupportedPerson.Number;
      import.Group.Update.Program.Assign(local.NullProgram);
      import.Group.Update.DprProgram.ProgramState =
        local.NullDprProgram.ProgramState;
      import.Group.Update.Collection.AppliedToCode = "G";
      export.AmtDistributed.TotalCurrency += local.Group.Item.Collection.Amount;

      if (AsChar(local.Group.Item.Obligation.PrimarySecondaryCode) == AsChar
        (import.HardcodedPrimary.PrimarySecondaryCode))
      {
        export.AmtDistributedToPrim.TotalCurrency += local.Group.Item.
          Collection.Amount;
      }
    }

    UseFnDeterminePgmForColl();
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
  }

  private static void MoveDebtsToGroup1(FnProrateCollAsGift.Import.
    DebtsGroup source, Local.GroupGroup target)
  {
    MoveObligationType(source.DebtsObligationType, target.ObligationType);
    target.Obligation.Assign(source.DebtsObligation);
    MoveObligationTransaction(source.DebtsDebt, target.Debt);
    target.DebtDetail.Assign(source.DebtsDebtDetail);
    MoveCollection(source.DebtsCollection, target.Collection);
    target.SupportedPerson.Number = source.DebtsSuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveDebtsToGroup2(FnDeterminePgmForColl.Import.
    DebtsGroup source, Import.GroupGroup target)
  {
    target.ObligationType.Assign(source.DebtsObligationType);
    target.Obligation.Assign(source.DebtsObligation);
    target.Debt.SystemGeneratedIdentifier =
      source.DebtsDebt.SystemGeneratedIdentifier;
    MoveCollection(source.DebtsCollection, target.Collection);
    target.SuppPrsn.Number = source.DebtsSuppPrsn.Number;
    target.Program.Assign(source.DebtsProgram);
    target.DprProgram.ProgramState = source.DebtsDprProgram.ProgramState;
  }

  private static void MoveGroupToDebts1(Import.GroupGroup source,
    FnDeterminePgmForColl.Import.DebtsGroup target)
  {
    target.DebtsObligationType.Assign(source.ObligationType);
    target.DebtsObligation.Assign(source.Obligation);
    target.DebtsDebt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection(source.Collection, target.DebtsCollection);
    target.DebtsSuppPrsn.Number = source.SuppPrsn.Number;
    target.DebtsProgram.Assign(source.Program);
    target.DebtsDprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroupToDebts2(Local.GroupGroup source,
    FnProrateCollAsGift.Import.DebtsGroup target)
  {
    MoveObligationType(source.ObligationType, target.DebtsObligationType);
    target.DebtsObligation.Assign(source.Obligation);
    MoveObligationTransaction(source.Debt, target.DebtsDebt);
    target.DebtsDebtDetail.Assign(source.DebtDetail);
    MoveCollection(source.Collection, target.DebtsCollection);
    target.DebtsSuppPrsn.Number = source.SupportedPerson.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveHhHist1(Import.HhHistGroup source,
    FnDeterminePgmForColl.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(FnDeterminePgmForColl.Import.
    HhHistGroup source, Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl2);
  }

  private static void MoveHhHistDtl1(Import.HhHistDtlGroup source,
    FnDeterminePgmForColl.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(FnDeterminePgmForColl.Import.
    HhHistDtlGroup source, Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveLegal(Import.LegalGroup source,
    FnDeterminePgmForColl.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl);
  }

  private static void MoveLegalDtl(Import.LegalDtlGroup source,
    FnDeterminePgmForColl.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
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

  private static void MovePgmHist(Import.PgmHistGroup source,
    FnDeterminePgmForColl.Import.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHistDtl(Import.PgmHistDtlGroup source,
    FnDeterminePgmForColl.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private void UseFnDeterminePgmForColl()
  {
    var useImport = new FnDeterminePgmForColl.Import();
    var useExport = new FnDeterminePgmForColl.Export();

    useImport.Obligor.Number = import.Obligor.Number;
    useImport.PersistantObligor.Assign(import.PersistantObligor);
    useImport.PersistantCashReceiptDetail.Assign(
      import.PersistantCashReceiptDetail);
    import.Group.CopyTo(useImport.Debts, MoveGroupToDebts1);
    import.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist);
    import.Legal.CopyTo(useImport.Legal, MoveLegal);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist1);
    useImport.CollectionType.SequentialIdentifier =
      import.CollectionType.SequentialIdentifier;
    useImport.Coll.Date = local.Collection.Date;
    useImport.HardcodedAccruingClass.Classification =
      import.HardcodedAccruingClass.Classification;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      import.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(import.HardcodedAf);
    useImport.HardcodedFc.Assign(import.HardcodedFc);
    useImport.HardcodedAfi.Assign(import.HardcodedAfi);
    useImport.HardcodedFci.Assign(import.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(import.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(import.HardcodedNai);
    useImport.HardcodedNc.Assign(import.HardcodedNc);
    useImport.HardcodedNf.Assign(import.HardcodedNf);
    useImport.HardcodedMai.Assign(import.HardcodedMai);
    useImport.HardcodedNaDprProgram.ProgramState =
      import.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedPa.ProgramState = import.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = import.HardcodedTa.ProgramState;
    useImport.HardcodedCa.ProgramState = import.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = import.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = import.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = import.HardcodedUk.ProgramState;
    useImport.HardcodedFType.SequentialIdentifier =
      import.HardcodedFFedType.SequentialIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    useImport.PrworaDateOfConversion.Date = import.PrworaDateOfConversion.Date;

    Call(FnDeterminePgmForColl.Execute, useImport, useExport);

    import.PersistantObligor.Assign(useImport.PersistantObligor);
    import.PersistantCashReceiptDetail.Assign(
      useImport.PersistantCashReceiptDetail);
    useImport.Debts.CopyTo(import.Group, MoveDebtsToGroup2);
    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist2);
  }

  private void UseFnProrateCollAsGift()
  {
    var useImport = new FnProrateCollAsGift.Import();
    var useExport = new FnProrateCollAsGift.Export();

    local.Group.CopyTo(useImport.Debts, MoveGroupToDebts2);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistribute.TotalCurrency;

    Call(FnProrateCollAsGift.Execute, useImport, useExport);

    useImport.Debts.CopyTo(local.Group, MoveDebtsToGroup1);
    local.AmtToDistribute.TotalCurrency =
      useImport.AmtToDistribute.TotalCurrency;
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.DistBy.CourtOrderNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligationDebtDebtDetailObligor()
  {
    return ReadEach("ReadObligationTypeObligationDebtDebtDetailObligor",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", import.CurrentMonthStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.CurrentMonthEnd.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          import.PersistantCashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetString(
          command, "debtTypClass",
          import.HardcodedAccruingClass.Classification);
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 2);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 3);
        entities.ExistingObligation.CpaType = db.GetString(reader, 4);
        entities.ExistingDebt.CpaType = db.GetString(reader, 4);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 4);
        entities.ExistingKeyOnlyObligor.Type1 = db.GetString(reader, 4);
        entities.ExistingKeyOnlyObligor.Type1 = db.GetString(reader, 4);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 5);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 5);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 5);
        entities.ExistingKeyOnlyObligor.CspNumber = db.GetString(reader, 5);
        entities.ExistingKeyOnlyObligor.CspNumber = db.GetString(reader, 5);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 7);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 8);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 9);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 10);
        entities.ExistingDebt.Type1 = db.GetString(reader, 11);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 11);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 12);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 13);
        entities.ExistingKeyOnlySuppPrsn.Number = db.GetString(reader, 13);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 14);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 15);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 18);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 19);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 20);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 21);
        entities.ExistingDebtDetail.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingDebt.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingKeyOnlySuppPrsn.Populated = true;
        entities.ExistingKeyOnlyObligor.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingKeyOnlyObligor.Type1);
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingKeyOnlyObligor.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingObligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);

        return true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
      }

      /// <summary>
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
      }

      /// <summary>
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>
      /// A value of SuppPrsn.
      /// </summary>
      [JsonPropertyName("suppPrsn")]
      public CsePerson SuppPrsn
      {
        get => suppPrsn ??= new();
        set => suppPrsn = value;
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

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private Collection collection;
      private CsePerson suppPrsn;
      private Program program;
      private DprProgram dprProgram;
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

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of PersistantObligor.
    /// </summary>
    [JsonPropertyName("persistantObligor")]
    public CsePersonAccount PersistantObligor
    {
      get => persistantObligor ??= new();
      set => persistantObligor = value;
    }

    /// <summary>
    /// A value of PersistantCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("persistantCashReceiptDetail")]
    public CashReceiptDetail PersistantCashReceiptDetail
    {
      get => persistantCashReceiptDetail ??= new();
      set => persistantCashReceiptDetail = value;
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

    /// <summary>
    /// A value of DistBy.
    /// </summary>
    [JsonPropertyName("distBy")]
    public CashReceiptDetail DistBy
    {
      get => distBy ??= new();
      set => distBy = value;
    }

    /// <summary>
    /// A value of CurrentMonthStart.
    /// </summary>
    [JsonPropertyName("currentMonthStart")]
    public DateWorkArea CurrentMonthStart
    {
      get => currentMonthStart ??= new();
      set => currentMonthStart = value;
    }

    /// <summary>
    /// A value of CurrentMonthEnd.
    /// </summary>
    [JsonPropertyName("currentMonthEnd")]
    public DateWorkArea CurrentMonthEnd
    {
      get => currentMonthEnd ??= new();
      set => currentMonthEnd = value;
    }

    /// <summary>
    /// A value of ProcessSecObligOnly.
    /// </summary>
    [JsonPropertyName("processSecObligOnly")]
    public Common ProcessSecObligOnly
    {
      get => processSecObligOnly ??= new();
      set => processSecObligOnly = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of HardcodedPrimary.
    /// </summary>
    [JsonPropertyName("hardcodedPrimary")]
    public Obligation HardcodedPrimary
    {
      get => hardcodedPrimary ??= new();
      set => hardcodedPrimary = value;
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
    /// A value of HardcodedCsType.
    /// </summary>
    [JsonPropertyName("hardcodedCsType")]
    public ObligationType HardcodedCsType
    {
      get => hardcodedCsType ??= new();
      set => hardcodedCsType = value;
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
    /// A value of Hardcoded718B.
    /// </summary>
    [JsonPropertyName("hardcoded718B")]
    public ObligationType Hardcoded718B
    {
      get => hardcoded718B ??= new();
      set => hardcoded718B = value;
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
    /// A value of HardcodedVolClass.
    /// </summary>
    [JsonPropertyName("hardcodedVolClass")]
    public ObligationType HardcodedVolClass
    {
      get => hardcodedVolClass ??= new();
      set => hardcodedVolClass = value;
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
    /// A value of HardcodedNaDprProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaDprProgram")]
    public DprProgram HardcodedNaDprProgram
    {
      get => hardcodedNaDprProgram ??= new();
      set => hardcodedNaDprProgram = value;
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
    /// A value of HardcodedFFedType.
    /// </summary>
    [JsonPropertyName("hardcodedFFedType")]
    public CollectionType HardcodedFFedType
    {
      get => hardcodedFFedType ??= new();
      set => hardcodedFFedType = value;
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

    private CsePerson obligor;
    private CsePersonAccount persistantObligor;
    private CashReceiptDetail persistantCashReceiptDetail;
    private Common amtToDistribute;
    private CashReceiptDetail distBy;
    private DateWorkArea currentMonthStart;
    private DateWorkArea currentMonthEnd;
    private Common processSecObligOnly;
    private CollectionType collectionType;
    private Array<GroupGroup> group;
    private Array<PgmHistGroup> pgmHist;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private Obligation hardcodedPrimary;
    private Obligation hardcodedSecondary;
    private ObligationType hardcodedCsType;
    private ObligationType hardcodedAccruingClass;
    private ObligationType hardcoded718B;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private ObligationType hardcodedVolClass;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNaProgram;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private DprProgram hardcodedNaDprProgram;
    private DprProgram hardcodedPa;
    private DprProgram hardcodedTa;
    private DprProgram hardcodedCa;
    private DprProgram hardcodedUd;
    private DprProgram hardcodedUp;
    private DprProgram hardcodedUk;
    private CollectionType hardcodedFFedType;
    private DateWorkArea prworaDateOfConversion;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AmtDistributed.
    /// </summary>
    [JsonPropertyName("amtDistributed")]
    public Common AmtDistributed
    {
      get => amtDistributed ??= new();
      set => amtDistributed = value;
    }

    /// <summary>
    /// A value of AmtDistributedToPrim.
    /// </summary>
    [JsonPropertyName("amtDistributedToPrim")]
    public Common AmtDistributedToPrim
    {
      get => amtDistributedToPrim ??= new();
      set => amtDistributedToPrim = value;
    }

    private Common amtDistributed;
    private Common amtDistributedToPrim;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
      }

      /// <summary>
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
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
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>
      /// A value of SupportedPerson.
      /// </summary>
      [JsonPropertyName("supportedPerson")]
      public CsePerson SupportedPerson
      {
        get => supportedPerson ??= new();
        set => supportedPerson = value;
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

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private Collection collection;
      private CsePerson supportedPerson;
      private Program program;
      private DprProgram dprProgram;
    }

    /// <summary>
    /// A value of NullProgram.
    /// </summary>
    [JsonPropertyName("nullProgram")]
    public Program NullProgram
    {
      get => nullProgram ??= new();
      set => nullProgram = value;
    }

    /// <summary>
    /// A value of ProrateToCsOnly.
    /// </summary>
    [JsonPropertyName("prorateToCsOnly")]
    public Common ProrateToCsOnly
    {
      get => prorateToCsOnly ??= new();
      set => prorateToCsOnly = value;
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

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of NullDprProgram.
    /// </summary>
    [JsonPropertyName("nullDprProgram")]
    public DprProgram NullDprProgram
    {
      get => nullDprProgram ??= new();
      set => nullDprProgram = value;
    }

    private Program nullProgram;
    private Common prorateToCsOnly;
    private Common amtToDistribute;
    private Array<GroupGroup> group;
    private DateWorkArea collection;
    private DprProgram nullDprProgram;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingAccrualInstructions.
    /// </summary>
    [JsonPropertyName("existingAccrualInstructions")]
    public AccrualInstructions ExistingAccrualInstructions
    {
      get => existingAccrualInstructions ??= new();
      set => existingAccrualInstructions = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

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
    /// A value of ExistingKeyOnlySuppPrsn.
    /// </summary>
    [JsonPropertyName("existingKeyOnlySuppPrsn")]
    public CsePerson ExistingKeyOnlySuppPrsn
    {
      get => existingKeyOnlySuppPrsn ??= new();
      set => existingKeyOnlySuppPrsn = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlySupported.
    /// </summary>
    [JsonPropertyName("existingKeyOnlySupported")]
    public CsePersonAccount ExistingKeyOnlySupported
    {
      get => existingKeyOnlySupported ??= new();
      set => existingKeyOnlySupported = value;
    }

    /// <summary>
    /// A value of ExistingObligorKeyOnly.
    /// </summary>
    [JsonPropertyName("existingObligorKeyOnly")]
    public CsePerson ExistingObligorKeyOnly
    {
      get => existingObligorKeyOnly ??= new();
      set => existingObligorKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePersonAccount ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    private AccrualInstructions existingAccrualInstructions;
    private LegalAction existingLegalAction;
    private DebtDetail existingDebtDetail;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private ObligationType existingObligationType;
    private CsePerson existingKeyOnlySuppPrsn;
    private CsePersonAccount existingKeyOnlySupported;
    private CsePerson existingObligorKeyOnly;
    private CsePersonAccount existingKeyOnlyObligor;
  }
#endregion
}
