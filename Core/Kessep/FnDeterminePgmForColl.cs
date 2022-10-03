// Program: FN_DETERMINE_PGM_FOR_COLL, ID: 374492890, model: 746.
// Short name: SWE02531
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_PGM_FOR_COLL.
/// </summary>
[Serializable]
public partial class FnDeterminePgmForColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_PGM_FOR_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeterminePgmForColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeterminePgmForColl.
  /// </summary>
  public FnDeterminePgmForColl(IContext context, Import import, Export export):
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
    // 03/01/09  GVandy	CQ#9601		718B collections are being assigned a blank 
    // program applied to when the collection
    // 					amount exceeds available URA.  The resulting program derivation is "
    // -UK",
    // 					should be "AF-UK" or "FC-UK".
    // -------------------------------------------------------------------------------------------------------------------------------
    import.Debts.Index = 0;

    for(var limit = import.Debts.Count; import.Debts.Index < limit; ++
      import.Debts.Index)
    {
      if (!import.Debts.CheckSize())
      {
        break;
      }

      if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier != 0)
      {
        continue;
      }

      if (AsChar(import.Debts.Item.DebtsObligationType.SupportedPersonReqInd) ==
        'N')
      {
        continue;
      }

      if (!ReadObligationDebtDetail())
      {
        ExitState = "FN0211_DEBT_DETAIL_NF";

        return;
      }

      if (!IsEmpty(import.PersistantCashReceiptDetail.CourtOrderNumber))
      {
        local.LegalAction.StandardNumber =
          import.PersistantCashReceiptDetail.CourtOrderNumber;
      }
      else if (ReadLegalAction())
      {
        local.LegalAction.StandardNumber =
          entities.ExistingLegalAction.StandardNumber;
      }
      else
      {
        local.LegalAction.StandardNumber = local.Null2.StandardNumber;
      }

      local.DistBy.CourtOrderNumber = local.LegalAction.StandardNumber ?? "";

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

      for(import.PgmHist.Index = 0; import.PgmHist.Index < import
        .PgmHist.Count; ++import.PgmHist.Index)
      {
        if (Equal(import.Debts.Item.DebtsSuppPrsn.Number,
          import.PgmHist.Item.PgmHistSuppPrsn.Number))
        {
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

          break;
        }
      }

      if (Lt(import.Coll.Date, import.PrworaDateOfConversion.Date))
      {
        UseFnDeterminePgmForDbtDist1();
      }
      else
      {
        UseFnDeterminePgmForDbtDist2();
      }

      if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
        .HardcodedAf.SystemGeneratedIdentifier || import
        .Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
        .HardcodedFc.SystemGeneratedIdentifier)
      {
      }
      else
      {
        continue;
      }

      if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
        import.HardcodedUk.ProgramState))
      {
        continue;
      }

      if (AsChar(import.Debts.Item.DebtsObligation.PrimarySecondaryCode) == AsChar
        (import.HardcodedSecondary.PrimarySecondaryCode))
      {
        continue;
      }

      UseFnDetermineUraForSuppPrsn();

      if (import.Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
        .HardcodedMcType.SystemGeneratedIdentifier || import
        .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
        .HardcodedMjType.SystemGeneratedIdentifier || import
        .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
        .HardcodedMsType.SystemGeneratedIdentifier)
      {
        if (import.Debts.Item.DebtsCollection.Amount > local
          .UraMedicalAmount.TotalCurrency)
        {
          if (local.AddOn.IsEmpty)
          {
            local.AddOn.Index = 0;
            local.AddOn.CheckSize();
          }
          else
          {
            local.AddOn.Index = local.AddOn.Count;
            local.AddOn.CheckSize();
          }

          MoveCollection(import.Debts.Item.DebtsCollection,
            local.AddOn.Update.AddOnCollection);
          local.AddOn.Update.AddOnDebt.SystemGeneratedIdentifier =
            import.Debts.Item.DebtsDebt.SystemGeneratedIdentifier;
          local.AddOn.Update.AddOnObligation.Assign(
            import.Debts.Item.DebtsObligation);
          local.AddOn.Update.AddOnObligationType.Assign(
            import.Debts.Item.DebtsObligationType);
          local.AddOn.Update.AddOnSuppPrsn.Number =
            import.Debts.Item.DebtsSuppPrsn.Number;
          local.AddOn.Update.AddOnCollection.Amount =
            local.AddOn.Item.AddOnCollection.Amount - local
            .UraMedicalAmount.TotalCurrency;
          import.Debts.Update.DebtsCollection.Amount =
            local.UraMedicalAmount.TotalCurrency;
          local.AddOn.Update.AddOnProgram.Assign(import.HardcodedNaProgram);

          if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
            import.HardcodedPa.ProgramState))
          {
            local.AddOn.Update.AddOnDprProgram.ProgramState =
              import.HardcodedUd.ProgramState;
          }
          else
          {
            local.AddOn.Update.AddOnDprProgram.ProgramState =
              import.HardcodedUp.ProgramState;
          }
        }
      }
      else if (import.Debts.Item.DebtsCollection.Amount > local
        .UraAmount.TotalCurrency)
      {
        if (local.AddOn.IsEmpty)
        {
          local.AddOn.Index = 0;
          local.AddOn.CheckSize();
        }
        else
        {
          local.AddOn.Index = local.AddOn.Count;
          local.AddOn.CheckSize();
        }

        MoveCollection(import.Debts.Item.DebtsCollection,
          local.AddOn.Update.AddOnCollection);
        local.AddOn.Update.AddOnDebt.SystemGeneratedIdentifier =
          import.Debts.Item.DebtsDebt.SystemGeneratedIdentifier;
        local.AddOn.Update.AddOnObligation.Assign(
          import.Debts.Item.DebtsObligation);
        local.AddOn.Update.AddOnObligationType.Assign(
          import.Debts.Item.DebtsObligationType);
        local.AddOn.Update.AddOnSuppPrsn.Number =
          import.Debts.Item.DebtsSuppPrsn.Number;
        local.AddOn.Update.AddOnCollection.Amount =
          local.AddOn.Item.AddOnCollection.Amount - local
          .UraAmount.TotalCurrency;
        import.Debts.Update.DebtsCollection.Amount =
          local.UraAmount.TotalCurrency;

        if (import.Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
          .Hardcoded718B.SystemGeneratedIdentifier)
        {
          // 03/01/09 GVandy CQ9601 Move the import group debts program to the 
          // local group add on program.
          local.AddOn.Update.AddOnProgram.
            Assign(import.Debts.Item.DebtsProgram);
          local.AddOn.Update.AddOnDprProgram.ProgramState =
            import.HardcodedUk.ProgramState;
        }
        else
        {
          local.AddOn.Update.AddOnProgram.Assign(import.HardcodedNaProgram);

          if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
            import.HardcodedPa.ProgramState))
          {
            local.AddOn.Update.AddOnDprProgram.ProgramState =
              import.HardcodedUd.ProgramState;
          }
          else
          {
            local.AddOn.Update.AddOnDprProgram.ProgramState =
              import.HardcodedUp.ProgramState;
          }
        }
      }

      UseFnApplyCollectionToUra();
    }

    import.Debts.CheckIndex();

    if (local.AddOn.IsEmpty)
    {
      return;
    }

    for(local.AddOn.Index = 0; local.AddOn.Index < local.AddOn.Count; ++
      local.AddOn.Index)
    {
      if (!local.AddOn.CheckSize())
      {
        break;
      }

      import.Debts.Index = import.Debts.Count;
      import.Debts.CheckSize();

      MoveCollection(local.AddOn.Item.AddOnCollection,
        import.Debts.Update.DebtsCollection);
      import.Debts.Update.DebtsDebt.SystemGeneratedIdentifier =
        local.AddOn.Item.AddOnDebt.SystemGeneratedIdentifier;
      import.Debts.Update.DebtsDprProgram.ProgramState =
        local.AddOn.Item.AddOnDprProgram.ProgramState;
      import.Debts.Update.DebtsObligation.Assign(
        local.AddOn.Item.AddOnObligation);
      import.Debts.Update.DebtsObligationType.Assign(
        local.AddOn.Item.AddOnObligationType);
      import.Debts.Update.DebtsProgram.Assign(local.AddOn.Item.AddOnProgram);
      import.Debts.Update.DebtsSuppPrsn.Number =
        local.AddOn.Item.AddOnSuppPrsn.Number;
    }

    local.AddOn.CheckIndex();
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveHhHist1(FnApplyCollectionToUra.Import.
    HhHistGroup source, Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(Import.HhHistGroup source,
    FnApplyCollectionToUra.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl2);
  }

  private static void MoveHhHist3(Import.HhHistGroup source,
    FnDetermineUraForSuppPrsn.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl3);
  }

  private static void MoveHhHist4(Import.HhHistGroup source,
    FnDeterminePgmForDbtDist1.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl4);
  }

  private static void MoveHhHist5(Import.HhHistGroup source,
    FnDeterminePgmForDbtDist2.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl5);
  }

  private static void MoveHhHistDtl1(FnApplyCollectionToUra.Import.
    HhHistDtlGroup source, Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(Import.HhHistDtlGroup source,
    FnApplyCollectionToUra.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl3(Import.HhHistDtlGroup source,
    FnDetermineUraForSuppPrsn.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl4(Import.HhHistDtlGroup source,
    FnDeterminePgmForDbtDist1.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl5(Import.HhHistDtlGroup source,
    FnDeterminePgmForDbtDist2.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveLegal1(Import.LegalGroup source,
    FnApplyCollectionToUra.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl1);
  }

  private static void MoveLegal2(Import.LegalGroup source,
    FnDetermineUraForSuppPrsn.Import.LegalGroup target)
  {
    target.LegalSuppPrsn1.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl2);
  }

  private static void MoveLegal3(Import.LegalGroup source,
    FnDeterminePgmForDbtDist1.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl3);
  }

  private static void MoveLegal4(Import.LegalGroup source,
    FnDeterminePgmForDbtDist2.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl4);
  }

  private static void MoveLegalDtl1(Import.LegalDtlGroup source,
    FnApplyCollectionToUra.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl2(Import.LegalDtlGroup source,
    FnDetermineUraForSuppPrsn.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl3(Import.LegalDtlGroup source,
    FnDeterminePgmForDbtDist1.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl4(Import.LegalDtlGroup source,
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

  private void UseFnApplyCollectionToUra()
  {
    var useImport = new FnApplyCollectionToUra.Import();
    var useExport = new FnApplyCollectionToUra.Export();

    useImport.DebtDetail.DueDt = entities.ExistingDebtDetail.DueDt;
    import.Legal.CopyTo(useImport.Legal, MoveLegal1);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);
    useImport.Collection1.Date = import.Coll.Date;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    MoveObligationType(import.Debts.Item.DebtsObligationType,
      useImport.ObligationType);
    useImport.Collection2.Amount = import.Debts.Item.DebtsCollection.Amount;
    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;

    Call(FnApplyCollectionToUra.Execute, useImport, useExport);

    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist1);
  }

  private void UseFnDeterminePgmForDbtDist1()
  {
    var useImport = new FnDeterminePgmForDbtDist1.Import();
    var useExport = new FnDeterminePgmForDbtDist1.Export();

    useImport.DebtDetail.Assign(entities.ExistingDebtDetail);
    useImport.Collection.Date = import.Coll.Date;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    local.Tmp.CopyTo(useImport.PgmHistDtl, MoveTmpToPgmHistDtl1);
    import.Legal.CopyTo(useImport.Legal, MoveLegal3);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist4);
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
    useImport.HardcodedAfi.Assign(import.HardcodedAfi);
    useImport.HardcodedFc.Assign(import.HardcodedFc);
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
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    MoveObligationType(import.Debts.Item.DebtsObligationType,
      useImport.ObligationType);
    MoveObligation(import.Debts.Item.DebtsObligation, useImport.Obligation);
    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;

    Call(FnDeterminePgmForDbtDist1.Execute, useImport, useExport);

    import.Debts.Update.DebtsProgram.Assign(useExport.Program);
    import.Debts.Update.DebtsDprProgram.ProgramState =
      useExport.DprProgram.ProgramState;
  }

  private void UseFnDeterminePgmForDbtDist2()
  {
    var useImport = new FnDeterminePgmForDbtDist2.Import();
    var useExport = new FnDeterminePgmForDbtDist2.Export();

    useImport.DebtDetail.Assign(entities.ExistingDebtDetail);
    useImport.Collection.Date = import.Coll.Date;
    useImport.CollectionType.SequentialIdentifier =
      import.CollectionType.SequentialIdentifier;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    local.Tmp.CopyTo(useImport.PgmHistDtl, MoveTmpToPgmHistDtl2);
    import.Legal.CopyTo(useImport.Legal, MoveLegal4);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist5);
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
      import.HardcodedFType.SequentialIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    MoveObligationType(import.Debts.Item.DebtsObligationType,
      useImport.ObligationType);
    MoveObligation(import.Debts.Item.DebtsObligation, useImport.Obligation);
    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;

    Call(FnDeterminePgmForDbtDist2.Execute, useImport, useExport);

    import.Debts.Update.DebtsProgram.Assign(useExport.Program);
    import.Debts.Update.DebtsDprProgram.ProgramState =
      useExport.DprProgram.ProgramState;
  }

  private void UseFnDetermineUraForSuppPrsn()
  {
    var useImport = new FnDetermineUraForSuppPrsn.Import();
    var useExport = new FnDetermineUraForSuppPrsn.Export();

    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;
    useImport.Collection.Date = import.Coll.Date;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    import.Legal.CopyTo(useImport.Legal, MoveLegal2);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist3);

    Call(FnDetermineUraForSuppPrsn.Execute, useImport, useExport);

    local.UraAmount.TotalCurrency = useExport.UraAmount.TotalCurrency;
    local.UraMedicalAmount.TotalCurrency =
      useExport.UraMedicalAmount.TotalCurrency;
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
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadObligationDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.PersistantObligor.Populated);
    entities.ExistingDebtDetail.Populated = false;
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligationDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PersistantObligor.Type1);
        db.SetString(command, "cspNumber", import.PersistantObligor.CspNumber);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.Debts.Item.DebtsObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otrGeneratedId",
          import.Debts.Item.DebtsDebt.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 6);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 7);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 8);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 10);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 11);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 12);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 13);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 14);
        entities.ExistingDebtDetail.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private ObligationType debtsObligationType;
      private Obligation debtsObligation;
      private ObligationTransaction debtsDebt;
      private Collection debtsCollection;
      private CsePerson debtsSuppPrsn;
      private Program debtsProgram;
      private DprProgram debtsDprProgram;
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
        LegalDtlGroup.Capacity, 0);

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
        HhHistDtlGroup.Capacity, 0);

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
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity, 0);

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
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity, 0);

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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of Coll.
    /// </summary>
    [JsonPropertyName("coll")]
    public DateWorkArea Coll
    {
      get => coll ??= new();
      set => coll = value;
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
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
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
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
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
    /// A value of HardcodedFType.
    /// </summary>
    [JsonPropertyName("hardcodedFType")]
    public CollectionType HardcodedFType
    {
      get => hardcodedFType ??= new();
      set => hardcodedFType = value;
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

    private CsePerson obligor;
    private CsePersonAccount persistantObligor;
    private CashReceiptDetail persistantCashReceiptDetail;
    private Array<DebtsGroup> debts;
    private Array<PgmHistGroup> pgmHist;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private CollectionType collectionType;
    private DateWorkArea coll;
    private ObligationType hardcodedAccruingClass;
    private ObligationType hardcoded718B;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private Program hardcodedAf;
    private Program hardcodedFc;
    private Program hardcodedAfi;
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
    private CollectionType hardcodedFType;
    private Obligation hardcodedSecondary;
    private DateWorkArea prworaDateOfConversion;
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
    /// <summary>A AddOnGroup group.</summary>
    [Serializable]
    public class AddOnGroup
    {
      /// <summary>
      /// A value of AddOnObligationType.
      /// </summary>
      [JsonPropertyName("addOnObligationType")]
      public ObligationType AddOnObligationType
      {
        get => addOnObligationType ??= new();
        set => addOnObligationType = value;
      }

      /// <summary>
      /// A value of AddOnObligation.
      /// </summary>
      [JsonPropertyName("addOnObligation")]
      public Obligation AddOnObligation
      {
        get => addOnObligation ??= new();
        set => addOnObligation = value;
      }

      /// <summary>
      /// A value of AddOnDebt.
      /// </summary>
      [JsonPropertyName("addOnDebt")]
      public ObligationTransaction AddOnDebt
      {
        get => addOnDebt ??= new();
        set => addOnDebt = value;
      }

      /// <summary>
      /// A value of AddOnCollection.
      /// </summary>
      [JsonPropertyName("addOnCollection")]
      public Collection AddOnCollection
      {
        get => addOnCollection ??= new();
        set => addOnCollection = value;
      }

      /// <summary>
      /// A value of AddOnSuppPrsn.
      /// </summary>
      [JsonPropertyName("addOnSuppPrsn")]
      public CsePerson AddOnSuppPrsn
      {
        get => addOnSuppPrsn ??= new();
        set => addOnSuppPrsn = value;
      }

      /// <summary>
      /// A value of AddOnProgram.
      /// </summary>
      [JsonPropertyName("addOnProgram")]
      public Program AddOnProgram
      {
        get => addOnProgram ??= new();
        set => addOnProgram = value;
      }

      /// <summary>
      /// A value of AddOnDprProgram.
      /// </summary>
      [JsonPropertyName("addOnDprProgram")]
      public DprProgram AddOnDprProgram
      {
        get => addOnDprProgram ??= new();
        set => addOnDprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private ObligationType addOnObligationType;
      private Obligation addOnObligation;
      private ObligationTransaction addOnDebt;
      private Collection addOnCollection;
      private CsePerson addOnSuppPrsn;
      private Program addOnProgram;
      private DprProgram addOnDprProgram;
    }

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
    /// A value of Null2.
    /// </summary>
    [JsonPropertyName("null2")]
    public LegalAction Null2
    {
      get => null2 ??= new();
      set => null2 = value;
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
    /// Gets a value of AddOn.
    /// </summary>
    [JsonIgnore]
    public Array<AddOnGroup> AddOn => addOn ??= new(AddOnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AddOn for json serialization.
    /// </summary>
    [JsonPropertyName("addOn")]
    [Computed]
    public IList<AddOnGroup> AddOn_Json
    {
      get => addOn;
      set => AddOn.Assign(value);
    }

    /// <summary>
    /// A value of UraAmount.
    /// </summary>
    [JsonPropertyName("uraAmount")]
    public Common UraAmount
    {
      get => uraAmount ??= new();
      set => uraAmount = value;
    }

    /// <summary>
    /// A value of UraMedicalAmount.
    /// </summary>
    [JsonPropertyName("uraMedicalAmount")]
    public Common UraMedicalAmount
    {
      get => uraMedicalAmount ??= new();
      set => uraMedicalAmount = value;
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
    /// A value of DistBy.
    /// </summary>
    [JsonPropertyName("distBy")]
    public CashReceiptDetail DistBy
    {
      get => distBy ??= new();
      set => distBy = value;
    }

    private LegalAction null2;
    private LegalAction legalAction;
    private Array<AddOnGroup> addOn;
    private Common uraAmount;
    private Common uraMedicalAmount;
    private Array<TmpGroup> tmp;
    private Array<NullGroup> null1;
    private CashReceiptDetail distBy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    private LegalAction existingLegalAction;
    private DebtDetail existingDebtDetail;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private CashReceiptDetail existingCashReceiptDetail;
  }
#endregion
}
