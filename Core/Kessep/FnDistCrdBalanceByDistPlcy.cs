// Program: FN_DIST_CRD_BALANCE_BY_DIST_PLCY, ID: 372279920, model: 746.
// Short name: SWE02279
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DIST_CRD_BALANCE_BY_DIST_PLCY.
/// </summary>
[Serializable]
public partial class FnDistCrdBalanceByDistPlcy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DIST_CRD_BALANCE_BY_DIST_PLCY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDistCrdBalanceByDistPlcy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDistCrdBalanceByDistPlcy.
  /// </summary>
  public FnDistCrdBalanceByDistPlcy(IContext context, Import import,
    Export export):
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
    local.AmtToDistribute.TotalCurrency = import.AmtToDistribute.TotalCurrency;
    local.Collection.Date = import.PersistantCashReceiptDetail.CollectionDate;

    if (AsChar(import.FutureApplAllowed.Flag) == 'Y')
    {
      if (AsChar(import.ProcessSecObligOnly.Flag) == 'Y')
      {
        local.ProcessStart.Date = local.NullDateWorkArea.Date;
      }
      else
      {
        local.ProcessStart.Date = import.CollMonthStart.Date;
      }

      local.ProcessEnd.Date = AddDays(import.CollMonthEnd.Date, 1);
      local.ProcessEnd.Date = AddMonths(local.ProcessEnd.Date, 1);
      local.ProcessEnd.Date = AddDays(local.ProcessEnd.Date, -1);
    }
    else
    {
      // : Handle setting the Process Date's later based on  the Distribution 
      // Policy Rule's Debt State.
    }

    // : Read the Distribution Policy for that Collection Type.
    if (ReadDistributionPolicy())
    {
      if (Lt(entities.ExistingDistributionPolicy.MaximumProcessedDt,
        import.PersistantCashReceiptDetail.CollectionDate))
      {
        try
        {
          UpdateDistributionPolicy();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DIST_PLCY_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DIST_PLCY_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "FN0000_DIST_POLICY_NF_RB";

      return;
    }

    // : Now we read each Distribution Policy Rule.  We select only those Debt 
    // Details that qualify based on the DPR.
    local.Loop.Count = 0;

    do
    {
      local.ResetDprProcInd.Flag = "N";

      // : Test to prevent an endless loop from occurring.
      ++local.Loop.Count;

      if (local.Loop.Count > 25)
      {
        return;
      }

      local.PrevLoopAmtToDist.TotalCurrency =
        local.AmtToDistribute.TotalCurrency;

      foreach(var item in ReadDistributionPolicyRule())
      {
        // : Get the list of valid Obligation Types for the DPR.
        local.OfObType.Index = 0;
        local.OfObType.Clear();

        foreach(var item1 in ReadObligationType())
        {
          local.OfObType.Update.OfObType1.Assign(
            entities.ExistingDprObligationType);
          local.OfObType.Next();
        }

        // : Get the list of valid Programs for the DPR.
        local.OfPgmType.Index = 0;
        local.OfPgmType.Clear();

        foreach(var item1 in ReadProgramDprProgram())
        {
          local.OfPgmType.Update.OfPgmTypeProgram.Assign(
            entities.ExistingDprProgram);
          MoveDprProgram(entities.ExistingDprDprProgram,
            local.OfPgmType.Update.OfPgmTypeDprProgram);
          local.OfPgmType.Next();
        }

        if (AsChar(import.FutureApplAllowed.Flag) != 'Y')
        {
          if (AsChar(entities.ExistingDistributionPolicyRule.DebtState) == 'C')
          {
            local.ProcessStart.Date = import.CollMonthStart.Date;
            local.ProcessEnd.Date = import.CollMonthEnd.Date;
          }
          else
          {
            local.ProcessStart.Date = local.NullDateWorkArea.Date;
            local.ProcessEnd.Date = import.CollMonthEnd.Date;
          }
        }

        // : Now we read each Debt Detail for the Obligor that has not been 
        // Retired!
        // : Within the READ EACH, we must qualify each Debt Detail as to 
        // whether or not it matches the DPR.
        local.Group.Index = -1;
        local.Group.Count = 0;

        foreach(var item1 in ReadObligationTypeObligationDebtDebtDetail())
        {
          if (AsChar(entities.ExistingObligationType.Classification) == AsChar
            (import.HardcodedVoluntaryClass.Classification))
          {
            continue;
          }

          local.DebtDetail.Assign(entities.ExistingDebtDetail);
          local.Program.Assign(local.NullProgram);
          local.DprProgram.ProgramState = local.NullDprProgram.ProgramState;
          local.SuppPrsnTafInd.Flag = "";

          // : Based on the design of Distribution, we do not actually update 
          // any Debt Details or CRD's until we have built all of the
          // Collections.  So, we need to make sure that if we look at a Debt
          // Detail twice, that we subtract any previous Collections from that
          // balance.
          if (!import.Group.IsEmpty)
          {
            for(import.Group.Index = 0; import.Group.Index < import
              .Group.Count; ++import.Group.Index)
            {
              if (!import.Group.CheckSize())
              {
                break;
              }

              if (import.Group.Item.ObligationType.SystemGeneratedIdentifier ==
                entities.ExistingObligationType.SystemGeneratedIdentifier && import
                .Group.Item.Obligation.SystemGeneratedIdentifier == entities
                .ExistingObligation.SystemGeneratedIdentifier && import
                .Group.Item.Debt.SystemGeneratedIdentifier == entities
                .ExistingDebt.SystemGeneratedIdentifier)
              {
                switch(AsChar(import.Group.Item.Collection.AppliedToCode))
                {
                  case 'C':
                    local.DebtDetail.BalanceDueAmt -= import.Group.Item.
                      Collection.Amount;

                    if (local.DebtDetail.BalanceDueAmt <= 0)
                    {
                      goto ReadEach;
                    }

                    break;
                  case 'A':
                    local.DebtDetail.BalanceDueAmt -= import.Group.Item.
                      Collection.Amount;

                    if (local.DebtDetail.BalanceDueAmt <= 0)
                    {
                      goto ReadEach;
                    }

                    break;
                  case 'I':
                    local.DebtDetail.InterestBalanceDueAmt =
                      local.DebtDetail.InterestBalanceDueAmt.
                        GetValueOrDefault() - import
                      .Group.Item.Collection.Amount;

                    if (local.DebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault() <= 0)
                    {
                      goto ReadEach;
                    }

                    break;
                  default:
                    break;
                }
              }
            }

            import.Group.CheckIndex();
          }

          // : Get the Legal Actio for the Obligation, if one exists.
          if (ReadLegalAction())
          {
            local.LegalAction.StandardNumber =
              entities.ExistingLegalAction.StandardNumber;
          }
          else
          {
            local.LegalAction.StandardNumber =
              local.NullLegalAction.StandardNumber;
          }

          // : We must process Secondary Obligations alone.
          if (AsChar(import.ProcessSecObligOnly.Flag) == 'Y')
          {
            if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) != AsChar
              (import.HardcodedSecondary.PrimarySecondaryCode))
            {
              continue;
            }

            if (!ReadObligation())
            {
              continue;
            }
          }
          else
          {
            if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
              (import.HardcodedSecondary.PrimarySecondaryCode))
            {
              continue;
            }

            // : If the Court Order is passed in, then we can only distribute 
            // payments to Obligations ordered by the Court Order.
            if (!IsEmpty(import.DistBy.CourtOrderNumber))
            {
              if (!Equal(import.DistBy.CourtOrderNumber,
                local.LegalAction.StandardNumber))
              {
                continue;
              }
            }
          }

          // : We need the Supported Person Number so we can prorate 
          // appropriately.
          if (AsChar(entities.ExistingObligationType.SupportedPersonReqInd) == 'Y'
            )
          {
            if (ReadCsePerson())
            {
              local.SuppPrsn.Number = entities.ExistingSupportedPerson.Number;

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

              // : Load the Local Temporary Group view with the specific 
              // Supported Person Program History.
              for(import.PgmHist.Index = 0; import.PgmHist.Index < import
                .PgmHist.Count; ++import.PgmHist.Index)
              {
                if (Equal(import.PgmHist.Item.PgmHistSuppPrsn.Number,
                  entities.ExistingSupportedPerson.Number))
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
                      PgmHistDtlPersonProgram,
                      local.Tmp.Update.TmpPersonProgram);
                    local.Tmp.Next();
                  }
                }
              }

              if (Lt(import.PersistantCashReceiptDetail.CollectionDate,
                import.PrworaDateOfConversion.Date))
              {
                UseFnDeterminePgmForDbtDist1();
              }
              else
              {
                UseFnDeterminePgmForDbtDist2();
              }
            }
            else
            {
              ExitState = "SUPPORTED_PERSON_NF_RB";

              return;
            }
          }
          else
          {
            local.SuppPrsn.Number = local.NullSuppPrsn.Number;
          }

          // : Now we qualify the Debt Detail.
          UseFnValidateDebtAgainstDpr();

          // : If it is qualified, we continue, else we get the next Debt 
          // Detail.
          if (AsChar(local.DebtEligibleForDist.Flag) != 'Y')
          {
            continue;
          }

          // : Now we must handle Set-Off.  We can only apply Set-Off 
          // Collections to Debt Details that have been certified and only up to
          // the amount that has been certified.  This is determined by looking
          // at the Collection Type.  Then, for each Debt Detail, we look based
          // on the Collection Date, what if anything has been certified.
          if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
          {
            break;
          }

          ++local.Group.Index;
          local.Group.CheckSize();

          // : Move our qualified Debt Details into a temporary group view for 
          // later processing and get the next Debt Detail.
          local.Group.Update.Obligation.Assign(entities.ExistingObligation);
          local.Group.Update.ObligationType.Assign(
            entities.ExistingObligationType);
          local.Group.Update.Debt.SystemGeneratedIdentifier =
            entities.ExistingDebt.SystemGeneratedIdentifier;
          local.Group.Update.DebtDetail.Assign(local.DebtDetail);
          local.Group.Update.Program.Assign(local.Program);
          local.Group.Update.DprProgram.ProgramState =
            local.DprProgram.ProgramState;
          local.Group.Update.SupportedPerson.Number = local.SuppPrsn.Number;
          local.Group.Update.LegalAction.StandardNumber =
            local.LegalAction.StandardNumber;

          // : Clear these views.
          MoveCollection(local.NullCollection, local.Group.Update.Collection);

ReadEach:
          ;
        }

        // : If we found no qualified Debt Details, get the next DPR.
        if (local.Group.IsEmpty)
        {
          continue;
        }

        // : Now we must prorate the payment across the qualified debts for each
        // supported person in oldest first order.
        UseFnProrateCollToDebts();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : Move the details of the collections and debts to the export group 
        // view.
        import.Group.Index = import.Group.Count - 1;
        import.Group.CheckSize();

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          if (local.Group.Item.Collection.Amount == 0)
          {
            continue;
          }

          if (import.Group.Index + 1 >= Import.GroupGroup.Capacity)
          {
            return;
          }

          ++import.Group.Index;
          import.Group.CheckSize();

          import.Group.Update.Obligation.Assign(local.Group.Item.Obligation);
          import.Group.Update.ObligationType.Assign(
            local.Group.Item.ObligationType);
          import.Group.Update.Debt.SystemGeneratedIdentifier =
            local.Group.Item.Debt.SystemGeneratedIdentifier;
          MoveCollection(local.Group.Item.Collection,
            import.Group.Update.Collection);
          import.Group.Update.SuppPrsn.Number =
            local.Group.Item.SupportedPerson.Number;
          import.Group.Update.Program.Assign(local.Group.Item.Program);
          import.Group.Update.DprProgram.ProgramState =
            local.Group.Item.DprProgram.ProgramState;
          export.AmtDistributed.TotalCurrency += local.Group.Item.Collection.
            Amount;

          if (AsChar(local.Group.Item.Obligation.PrimarySecondaryCode) == AsChar
            (import.HardcodedPrimary.PrimarySecondaryCode))
          {
            export.AmtDistributedToPrim.TotalCurrency += local.Group.Item.
              Collection.Amount;

            if (export.PrimSummary.IsEmpty)
            {
              export.PrimSummary.Index = 0;
              export.PrimSummary.CheckSize();

              export.PrimSummary.Update.Obligation.SystemGeneratedIdentifier =
                local.Group.Item.Obligation.SystemGeneratedIdentifier;
              export.PrimSummary.Update.Ps.SystemGeneratedIdentifier =
                local.Group.Item.ObligationType.SystemGeneratedIdentifier;
              export.PrimSummary.Update.ByObligation.TotalCurrency =
                local.Group.Item.Collection.Amount;

              goto Test;
            }

            for(export.PrimSummary.Index = 0; export.PrimSummary.Index < export
              .PrimSummary.Count; ++export.PrimSummary.Index)
            {
              if (!export.PrimSummary.CheckSize())
              {
                break;
              }

              if (local.Group.Item.Obligation.SystemGeneratedIdentifier == export
                .PrimSummary.Item.Obligation.SystemGeneratedIdentifier && local
                .Group.Item.ObligationType.SystemGeneratedIdentifier == export
                .PrimSummary.Item.Ps.SystemGeneratedIdentifier)
              {
                export.PrimSummary.Update.ByObligation.TotalCurrency =
                  export.PrimSummary.Item.ByObligation.TotalCurrency + local
                  .Group.Item.Collection.Amount;

                goto Test;
              }
            }

            export.PrimSummary.CheckIndex();

            if (export.PrimSummary.Index >= Export.PrimSummaryGroup.Capacity)
            {
              ExitState = "FN0000_MAX_GRP_VW_FOR_DIST_RB";

              return;
            }

            ++export.PrimSummary.Index;
            export.PrimSummary.CheckSize();

            export.PrimSummary.Update.Obligation.SystemGeneratedIdentifier =
              local.Group.Item.Obligation.SystemGeneratedIdentifier;
            export.PrimSummary.Update.Ps.SystemGeneratedIdentifier =
              local.Group.Item.ObligationType.SystemGeneratedIdentifier;
            export.PrimSummary.Update.ByObligation.TotalCurrency =
              local.Group.Item.Collection.Amount;
          }

Test:
          ;
        }

        local.Group.CheckIndex();

        if (local.AmtToDistribute.TotalCurrency == 0)
        {
          return;
        }
        else if (local.AmtToDistribute.TotalCurrency < 0)
        {
          ExitState = "FN0000_AMT_TO_DIST_GRTR_COLL";

          return;
        }
        else if (AsChar(local.ResetDprProcInd.Flag) == 'Y')
        {
          goto Next;
        }
      }

Next:
      ;
    }
    while(AsChar(local.ResetDprProcInd.Flag) != 'N');
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveDebtsToGroup(FnProrateCollToDebts.Import.
    DebtsGroup source, Local.GroupGroup target)
  {
    target.ObligationType.Assign(source.DebtsObligationType);
    target.Obligation.Assign(source.DebtsObligation);
    target.Debt.SystemGeneratedIdentifier =
      source.DebtsDebt.SystemGeneratedIdentifier;
    target.DebtDetail.Assign(source.DebtsDebtDetail);
    MoveCollection(source.DebtsCollection, target.Collection);
    target.SupportedPerson.Number = source.DebtsSuppPrsn.Number;
    target.Program.Assign(source.DebtsProgram);
    target.DprProgram.ProgramState = source.DebtsDprProgram.ProgramState;
    target.LegalAction.StandardNumber = source.DebtsLegalAction.StandardNumber;
  }

  private static void MoveDprProgram(DprProgram source, DprProgram target)
  {
    target.AssistanceInd = source.AssistanceInd;
    target.ProgramState = source.ProgramState;
  }

  private static void MoveGroupToDebts(Local.GroupGroup source,
    FnProrateCollToDebts.Import.DebtsGroup target)
  {
    target.DebtsObligationType.Assign(source.ObligationType);
    target.DebtsObligation.Assign(source.Obligation);
    target.DebtsDebt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    target.DebtsDebtDetail.Assign(source.DebtDetail);
    MoveCollection(source.Collection, target.DebtsCollection);
    target.DebtsSuppPrsn.Number = source.SupportedPerson.Number;
    target.DebtsProgram.Assign(source.Program);
    target.DebtsDprProgram.ProgramState = source.DprProgram.ProgramState;
    target.DebtsLegalAction.StandardNumber = source.LegalAction.StandardNumber;
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

  private static void MoveHhHist3(Import.HhHistGroup source,
    FnProrateCollToDebts.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl3);
  }

  private static void MoveHhHist4(FnProrateCollToDebts.Import.
    HhHistGroup source, Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl4);
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

  private static void MoveHhHistDtl3(Import.HhHistDtlGroup source,
    FnProrateCollToDebts.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl4(FnProrateCollToDebts.Import.
    HhHistDtlGroup source, Import.HhHistDtlGroup target)
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

  private static void MoveLegal3(Import.LegalGroup source,
    FnProrateCollToDebts.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl3);
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

  private static void MoveLegalDtl3(Import.LegalDtlGroup source,
    FnProrateCollToDebts.Import.LegalDtlGroup target)
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

  private static void MoveOfObType(Local.OfObTypeGroup source,
    FnValidateDebtAgainstDpr.Import.ObTypeGroup target)
  {
    target.ObligationType.Assign(source.OfObType1);
  }

  private static void MoveOfPgmType1(Local.OfPgmTypeGroup source,
    FnValidateDebtAgainstDpr.Import.PgmTypeGroup target)
  {
    target.Program.Assign(source.OfPgmTypeProgram);
    MoveDprProgram(source.OfPgmTypeDprProgram, target.DprProgram);
  }

  private static void MoveOfPgmType2(Local.OfPgmTypeGroup source,
    FnProrateCollToDebts.Import.PgmTypeGroup target)
  {
    target.Program.Assign(source.OfPgmTypeProgram);
    target.DprProgram.ProgramState = source.OfPgmTypeDprProgram.ProgramState;
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePgmHist(Import.PgmHistGroup source,
    FnProrateCollToDebts.Import.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHistDtl(Import.PgmHistDtlGroup source,
    FnProrateCollToDebts.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
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

    MoveObligationType(entities.ExistingObligationType, useImport.ObligationType);
      
    MoveObligation(entities.ExistingObligation, useImport.Obligation);
    MoveDebtDetail(entities.ExistingDebtDetail, useImport.DebtDetail);
    local.Tmp.CopyTo(useImport.PgmHistDtl, MoveTmpToPgmHistDtl1);
    useImport.HardcodedAccruingClass.Classification =
      import.HardcodedAccruingClass.Classification;
    useImport.HardcodedAf.Assign(import.HardcodedAf);
    useImport.HardcodedAfi.Assign(import.HardcodedAfi);
    useImport.HardcodedFc.Assign(import.HardcodedFc);
    useImport.HardcodedFci.Assign(import.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(import.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(import.HardcodedNai);
    useImport.HardcodedNc.Assign(import.HardcodedNc);
    useImport.HardcodedNf.Assign(import.HardcodedNf);
    useImport.HardcodedMai.Assign(import.HardcodedMai);
    useImport.Collection.Date = local.Collection.Date;
    useImport.SuppPrsn.Number = entities.ExistingSupportedPerson.Number;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    import.Legal.CopyTo(useImport.Legal, MoveLegal1);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist1);
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      import.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
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

    Call(FnDeterminePgmForDbtDist1.Execute, useImport, useExport);

    local.Program.Assign(useExport.Program);
    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private void UseFnDeterminePgmForDbtDist2()
  {
    var useImport = new FnDeterminePgmForDbtDist2.Import();
    var useExport = new FnDeterminePgmForDbtDist2.Export();

    useImport.HardcodedMai.Assign(import.HardcodedMai);
    useImport.SuppPrsn.Number = entities.ExistingSupportedPerson.Number;
    MoveObligationType(entities.ExistingObligationType, useImport.ObligationType);
      
    MoveObligation(entities.ExistingObligation, useImport.Obligation);
    MoveDebtDetail(entities.ExistingDebtDetail, useImport.DebtDetail);
    useImport.Collection.Date = local.Collection.Date;
    useImport.CollectionType.SequentialIdentifier =
      import.PersistantCollectionType.SequentialIdentifier;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    local.Tmp.CopyTo(useImport.PgmHistDtl, MoveTmpToPgmHistDtl2);
    import.Legal.CopyTo(useImport.Legal, MoveLegal2);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);
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

    Call(FnDeterminePgmForDbtDist2.Execute, useImport, useExport);

    local.Program.Assign(useExport.Program);
    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private void UseFnProrateCollToDebts()
  {
    var useImport = new FnProrateCollToDebts.Import();
    var useExport = new FnProrateCollToDebts.Export();

    useImport.DistributionPolicyRule.ApplyTo =
      entities.ExistingDistributionPolicyRule.ApplyTo;
    local.Group.CopyTo(useImport.Debts, MoveGroupToDebts);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistribute.TotalCurrency;
    useImport.CollMonthStart.Date = import.CollMonthStart.Date;
    useImport.CollectionType.SequentialIdentifier =
      import.PersistantCollectionType.SequentialIdentifier;
    useImport.Collection.Date = local.Collection.Date;
    useImport.Program.Assign(local.Program);
    useImport.DistBy.CourtOrderNumber = import.DistBy.CourtOrderNumber;
    import.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist);
    local.OfPgmType.CopyTo(useImport.PgmType, MoveOfPgmType2);
    import.Legal.CopyTo(useImport.Legal, MoveLegal3);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist3);
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
    useImport.HardcodedFedFType.SequentialIdentifier =
      import.HardcodedFedFType.SequentialIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    useImport.PrworaDateOfConversion.Date = import.PrworaDateOfConversion.Date;

    Call(FnProrateCollToDebts.Execute, useImport, useExport);

    useImport.Debts.CopyTo(local.Group, MoveDebtsToGroup);
    local.AmtToDistribute.TotalCurrency =
      useImport.AmtToDistribute.TotalCurrency;
    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist4);
    local.ResetDprProcInd.Flag = useExport.ResetDprProcInd.Flag;
  }

  private void UseFnValidateDebtAgainstDpr()
  {
    var useImport = new FnValidateDebtAgainstDpr.Import();
    var useExport = new FnValidateDebtAgainstDpr.Export();

    useImport.SuppPrsnTafInd.Flag = local.SuppPrsnTafInd.Flag;
    useImport.DistributionPolicyRule.Assign(
      entities.ExistingDistributionPolicyRule);
    useImport.Obligation.OrderTypeCode =
      entities.ExistingObligation.OrderTypeCode;
    useImport.ObligationType.Assign(entities.ExistingObligationType);
    useImport.Program.Assign(local.Program);
    useImport.Persistant.Assign(entities.ExistingDebt);
    useImport.DebtDetail.Assign(local.DebtDetail);
    local.OfObType.CopyTo(useImport.ObType, MoveOfObType);
    useImport.CollMonthStart.Date = import.CollMonthStart.Date;
    useImport.CollMonthEnd.Date = import.CollMonthEnd.Date;
    useImport.FutureApplAllowed.Flag = import.FutureApplAllowed.Flag;
    useImport.DprProgram.ProgramState = local.DprProgram.ProgramState;
    useImport.CollectionType.SequentialIdentifier =
      import.PersistantCollectionType.SequentialIdentifier;
    local.OfPgmType.CopyTo(useImport.PgmType, MoveOfPgmType1);
    useImport.HardcodedIType.SequentialIdentifier =
      import.HardcodedIType.SequentialIdentifier;
    useImport.CashReceiptDetail.CollectionDate =
      import.PersistantCashReceiptDetail.CollectionDate;
    useImport.AllowITypeProcInd.Flag = import.AllowITypeProcInd.Flag;
    useImport.ItypeWindow.Count = import.ItypeWindow.Count;

    Call(FnValidateDebtAgainstDpr.Execute, useImport, useExport);

    entities.ExistingDebt.SystemGeneratedIdentifier =
      useImport.Persistant.SystemGeneratedIdentifier;
    local.DebtEligibleForDist.Flag = useExport.Eligible.Flag;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDebt.Populated);
    entities.ExistingSupportedPerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingDebt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSupportedPerson.Number = db.GetString(reader, 0);
        entities.ExistingSupportedPerson.Populated = true;
      });
  }

  private bool ReadDistributionPolicy()
  {
    entities.ExistingDistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cltIdentifier",
          import.PersistantCollectionType.SequentialIdentifier);
        db.SetDate(
          command, "effectiveDt",
          import.PersistantCashReceiptDetail.CollectionDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDistributionPolicy.EffectiveDt = db.GetDate(reader, 1);
        entities.ExistingDistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.ExistingDistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 3);
        entities.ExistingDistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingDistributionPolicy.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDistributionPolicyRule()
  {
    entities.ExistingDistributionPolicyRule.Populated = false;

    return ReadEach("ReadDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDistributionPolicyRule.DbpGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingDistributionPolicyRule.FirstLastIndicator =
          db.GetNullableString(reader, 2);
        entities.ExistingDistributionPolicyRule.DebtFunctionType =
          db.GetString(reader, 3);
        entities.ExistingDistributionPolicyRule.DebtState =
          db.GetString(reader, 4);
        entities.ExistingDistributionPolicyRule.ApplyTo =
          db.GetString(reader, 5);
        entities.ExistingDistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingDistributionPolicyRule.DistributeToOrderTypeCode =
          db.GetString(reader, 7);
        entities.ExistingDistributionPolicyRule.Populated = true;
        CheckValid<DistributionPolicyRule>("FirstLastIndicator",
          entities.ExistingDistributionPolicyRule.FirstLastIndicator);
        CheckValid<DistributionPolicyRule>("DebtFunctionType",
          entities.ExistingDistributionPolicyRule.DebtFunctionType);
        CheckValid<DistributionPolicyRule>("DebtState",
          entities.ExistingDistributionPolicyRule.DebtState);
        CheckValid<DistributionPolicyRule>("ApplyTo",
          entities.ExistingDistributionPolicyRule.ApplyTo);
        CheckValid<DistributionPolicyRule>("DistributeToOrderTypeCode",
          entities.ExistingDistributionPolicyRule.DistributeToOrderTypeCode);

        return true;
      });
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

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingPrimaryObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          import.PrimaryObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obId", import.PrimaryObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otySecondId", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingPrimaryObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingPrimaryObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingPrimaryObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingPrimaryObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingPrimaryObligation.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.ExistingPrimaryObligation.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicyRule.Populated);

    return ReadEach("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        if (local.OfObType.IsFull)
        {
          return false;
        }

        entities.ExistingDprObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDprObligationType.Code = db.GetString(reader, 1);
        entities.ExistingDprObligationType.Classification =
          db.GetString(reader, 2);
        entities.ExistingDprObligationType.SupportedPersonReqInd =
          db.GetString(reader, 3);
        entities.ExistingDprObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingDprObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingDprObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligationDebtDebtDetail()
  {
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadObligationTypeObligationDebtDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.
          SetDate(command, "date1", local.ProcessStart.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", local.ProcessEnd.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          import.PersistantCashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
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
        entities.ExistingObligation.CspNumber = db.GetString(reader, 5);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 5);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 5);
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
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 12);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 13);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 14);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 17);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 18);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 19);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 20);
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
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

  private IEnumerable<bool> ReadProgramDprProgram()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicyRule.Populated);

    return ReadEach("ReadProgramDprProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        if (local.OfPgmType.IsFull)
        {
          return false;
        }

        entities.ExistingDprProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDprDprProgram.PrgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDprProgram.Code = db.GetString(reader, 1);
        entities.ExistingDprProgram.InterstateIndicator =
          db.GetString(reader, 2);
        entities.ExistingDprProgram.DistributionProgramType =
          db.GetString(reader, 3);
        entities.ExistingDprDprProgram.DbpGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingDprDprProgram.DprGeneratedId = db.GetInt32(reader, 5);
        entities.ExistingDprDprProgram.ProgramState = db.GetString(reader, 6);
        entities.ExistingDprDprProgram.AssistanceInd =
          db.GetNullableString(reader, 7);
        entities.ExistingDprProgram.Populated = true;
        entities.ExistingDprDprProgram.Populated = true;

        return true;
      });
  }

  private void UpdateDistributionPolicy()
  {
    var maximumProcessedDt = import.PersistantCashReceiptDetail.CollectionDate;

    entities.ExistingDistributionPolicy.Populated = false;
    Update("UpdateDistributionPolicy",
      (db, command) =>
      {
        db.SetNullableDate(command, "maxPrcdDt", maximumProcessedDt);
        db.SetInt32(
          command, "distPlcyId",
          entities.ExistingDistributionPolicy.SystemGeneratedIdentifier);
      });

    entities.ExistingDistributionPolicy.MaximumProcessedDt = maximumProcessedDt;
    entities.ExistingDistributionPolicy.Populated = true;
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
    /// A value of PersistantCollectionType.
    /// </summary>
    [JsonPropertyName("persistantCollectionType")]
    public CollectionType PersistantCollectionType
    {
      get => persistantCollectionType ??= new();
      set => persistantCollectionType = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of ProcessSecObligOnly.
    /// </summary>
    [JsonPropertyName("processSecObligOnly")]
    public Common ProcessSecObligOnly
    {
      get => processSecObligOnly ??= new();
      set => processSecObligOnly = value;
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
    /// A value of HardcodedVoluntaryClass.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntaryClass")]
    public ObligationType HardcodedVoluntaryClass
    {
      get => hardcodedVoluntaryClass ??= new();
      set => hardcodedVoluntaryClass = value;
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
    /// A value of HardcodedFedFType.
    /// </summary>
    [JsonPropertyName("hardcodedFedFType")]
    public CollectionType HardcodedFedFType
    {
      get => hardcodedFedFType ??= new();
      set => hardcodedFedFType = value;
    }

    /// <summary>
    /// A value of HardcodedVType.
    /// </summary>
    [JsonPropertyName("hardcodedVType")]
    public CollectionType HardcodedVType
    {
      get => hardcodedVType ??= new();
      set => hardcodedVType = value;
    }

    /// <summary>
    /// A value of HardcodedFcrtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFcrtPmt")]
    public CashReceiptType HardcodedFcrtPmt
    {
      get => hardcodedFcrtPmt ??= new();
      set => hardcodedFcrtPmt = value;
    }

    /// <summary>
    /// A value of HardcodedFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFdirPmt")]
    public CashReceiptType HardcodedFdirPmt
    {
      get => hardcodedFdirPmt ??= new();
      set => hardcodedFdirPmt = value;
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
    /// A value of PrimaryObligationType.
    /// </summary>
    [JsonPropertyName("primaryObligationType")]
    public ObligationType PrimaryObligationType
    {
      get => primaryObligationType ??= new();
      set => primaryObligationType = value;
    }

    /// <summary>
    /// A value of PrimaryObligation.
    /// </summary>
    [JsonPropertyName("primaryObligation")]
    public Obligation PrimaryObligation
    {
      get => primaryObligation ??= new();
      set => primaryObligation = value;
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
    /// A value of PrworaDateOfConversion.
    /// </summary>
    [JsonPropertyName("prworaDateOfConversion")]
    public DateWorkArea PrworaDateOfConversion
    {
      get => prworaDateOfConversion ??= new();
      set => prworaDateOfConversion = value;
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

    private CollectionType persistantCollectionType;
    private Common amtToDistribute;
    private CashReceiptDetail distBy;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail persistantCashReceiptDetail;
    private DateWorkArea collMonthStart;
    private DateWorkArea collMonthEnd;
    private Array<GroupGroup> group;
    private Array<PgmHistGroup> pgmHist;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private Common processSecObligOnly;
    private Common futureApplAllowed;
    private Obligation hardcodedPrimary;
    private Obligation hardcodedSecondary;
    private ObligationType hardcodedAccruingClass;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private ObligationType hardcoded718B;
    private ObligationType hardcodedVoluntaryClass;
    private CollectionType hardcodedIType;
    private CollectionType hardcodedFedFType;
    private CollectionType hardcodedVType;
    private CashReceiptType hardcodedFcrtPmt;
    private CashReceiptType hardcodedFdirPmt;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNaProgram;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private ObligationType primaryObligationType;
    private Obligation primaryObligation;
    private DprProgram hardcodedPa;
    private DprProgram hardcodedTa;
    private DprProgram hardcodedNaDprProgram;
    private DprProgram hardcodedCa;
    private DprProgram hardcodedUd;
    private DprProgram hardcodedUp;
    private DprProgram hardcodedUk;
    private DateWorkArea prworaDateOfConversion;
    private Common allowITypeProcInd;
    private Common itypeWindow;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PrimSummaryGroup group.</summary>
    [Serializable]
    public class PrimSummaryGroup
    {
      /// <summary>
      /// A value of Ps.
      /// </summary>
      [JsonPropertyName("ps")]
      public ObligationType Ps
      {
        get => ps ??= new();
        set => ps = value;
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
      /// A value of ByObligation.
      /// </summary>
      [JsonPropertyName("byObligation")]
      public Common ByObligation
      {
        get => byObligation ??= new();
        set => byObligation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ObligationType ps;
      private Obligation obligation;
      private Common byObligation;
    }

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

    /// <summary>
    /// Gets a value of PrimSummary.
    /// </summary>
    [JsonIgnore]
    public Array<PrimSummaryGroup> PrimSummary => primSummary ??= new(
      PrimSummaryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrimSummary for json serialization.
    /// </summary>
    [JsonPropertyName("primSummary")]
    [Computed]
    public IList<PrimSummaryGroup> PrimSummary_Json
    {
      get => primSummary;
      set => PrimSummary.Assign(value);
    }

    private Common amtDistributed;
    private Common amtDistributedToPrim;
    private Array<PrimSummaryGroup> primSummary;
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

      /// <summary>
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
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
      private LegalAction legalAction;
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

    /// <summary>A OfObTypeGroup group.</summary>
    [Serializable]
    public class OfObTypeGroup
    {
      /// <summary>
      /// A value of OfObType1.
      /// </summary>
      [JsonPropertyName("ofObType1")]
      public ObligationType OfObType1
      {
        get => ofObType1 ??= new();
        set => ofObType1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private ObligationType ofObType1;
    }

    /// <summary>A OfPgmTypeGroup group.</summary>
    [Serializable]
    public class OfPgmTypeGroup
    {
      /// <summary>
      /// A value of OfPgmTypeProgram.
      /// </summary>
      [JsonPropertyName("ofPgmTypeProgram")]
      public Program OfPgmTypeProgram
      {
        get => ofPgmTypeProgram ??= new();
        set => ofPgmTypeProgram = value;
      }

      /// <summary>
      /// A value of OfPgmTypeDprProgram.
      /// </summary>
      [JsonPropertyName("ofPgmTypeDprProgram")]
      public DprProgram OfPgmTypeDprProgram
      {
        get => ofPgmTypeDprProgram ??= new();
        set => ofPgmTypeDprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Program ofPgmTypeProgram;
      private DprProgram ofPgmTypeDprProgram;
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
    /// A value of SuppPrsnTafInd.
    /// </summary>
    [JsonPropertyName("suppPrsnTafInd")]
    public Common SuppPrsnTafInd
    {
      get => suppPrsnTafInd ??= new();
      set => suppPrsnTafInd = value;
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

    /// <summary>
    /// A value of ApplicationOccurredInd.
    /// </summary>
    [JsonPropertyName("applicationOccurredInd")]
    public Common ApplicationOccurredInd
    {
      get => applicationOccurredInd ??= new();
      set => applicationOccurredInd = value;
    }

    /// <summary>
    /// A value of FdsoCertifiedInd.
    /// </summary>
    [JsonPropertyName("fdsoCertifiedInd")]
    public Common FdsoCertifiedInd
    {
      get => fdsoCertifiedInd ??= new();
      set => fdsoCertifiedInd = value;
    }

    /// <summary>
    /// A value of ProcessStart.
    /// </summary>
    [JsonPropertyName("processStart")]
    public DateWorkArea ProcessStart
    {
      get => processStart ??= new();
      set => processStart = value;
    }

    /// <summary>
    /// A value of ProcessEnd.
    /// </summary>
    [JsonPropertyName("processEnd")]
    public DateWorkArea ProcessEnd
    {
      get => processEnd ??= new();
      set => processEnd = value;
    }

    /// <summary>
    /// A value of NullSuppPrsn.
    /// </summary>
    [JsonPropertyName("nullSuppPrsn")]
    public CsePerson NullSuppPrsn
    {
      get => nullSuppPrsn ??= new();
      set => nullSuppPrsn = value;
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
    /// A value of NullDprProgram.
    /// </summary>
    [JsonPropertyName("nullDprProgram")]
    public DprProgram NullDprProgram
    {
      get => nullDprProgram ??= new();
      set => nullDprProgram = value;
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

    /// <summary>
    /// A value of NullCollection.
    /// </summary>
    [JsonPropertyName("nullCollection")]
    public Collection NullCollection
    {
      get => nullCollection ??= new();
      set => nullCollection = value;
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
    /// A value of TotAmtToDistByPerson.
    /// </summary>
    [JsonPropertyName("totAmtToDistByPerson")]
    public Common TotAmtToDistByPerson
    {
      get => totAmtToDistByPerson ??= new();
      set => totAmtToDistByPerson = value;
    }

    /// <summary>
    /// A value of TmpDebtDtlBal.
    /// </summary>
    [JsonPropertyName("tmpDebtDtlBal")]
    public Common TmpDebtDtlBal
    {
      get => tmpDebtDtlBal ??= new();
      set => tmpDebtDtlBal = value;
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
    /// Gets a value of OfObType.
    /// </summary>
    [JsonIgnore]
    public Array<OfObTypeGroup> OfObType => ofObType ??= new(
      OfObTypeGroup.Capacity);

    /// <summary>
    /// Gets a value of OfObType for json serialization.
    /// </summary>
    [JsonPropertyName("ofObType")]
    [Computed]
    public IList<OfObTypeGroup> OfObType_Json
    {
      get => ofObType;
      set => OfObType.Assign(value);
    }

    /// <summary>
    /// Gets a value of OfPgmType.
    /// </summary>
    [JsonIgnore]
    public Array<OfPgmTypeGroup> OfPgmType => ofPgmType ??= new(
      OfPgmTypeGroup.Capacity);

    /// <summary>
    /// Gets a value of OfPgmType for json serialization.
    /// </summary>
    [JsonPropertyName("ofPgmType")]
    [Computed]
    public IList<OfPgmTypeGroup> OfPgmType_Json
    {
      get => ofPgmType;
      set => OfPgmType.Assign(value);
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
    /// A value of TmpForCalc.
    /// </summary>
    [JsonPropertyName("tmpForCalc")]
    public Common TmpForCalc
    {
      get => tmpForCalc ??= new();
      set => tmpForCalc = value;
    }

    /// <summary>
    /// A value of DebtEligibleForDist.
    /// </summary>
    [JsonPropertyName("debtEligibleForDist")]
    public Common DebtEligibleForDist
    {
      get => debtEligibleForDist ??= new();
      set => debtEligibleForDist = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of PgmTypeMatchFound.
    /// </summary>
    [JsonPropertyName("pgmTypeMatchFound")]
    public Common PgmTypeMatchFound
    {
      get => pgmTypeMatchFound ??= new();
      set => pgmTypeMatchFound = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
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
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of PrevLoopAmtToDist.
    /// </summary>
    [JsonPropertyName("prevLoopAmtToDist")]
    public Common PrevLoopAmtToDist
    {
      get => prevLoopAmtToDist ??= new();
      set => prevLoopAmtToDist = value;
    }

    private Common suppPrsnTafInd;
    private Common resetDprProcInd;
    private Common applicationOccurredInd;
    private Common fdsoCertifiedInd;
    private DateWorkArea processStart;
    private DateWorkArea processEnd;
    private CsePerson nullSuppPrsn;
    private Program nullProgram;
    private DprProgram nullDprProgram;
    private CsePerson suppPrsn;
    private Program program;
    private DprProgram dprProgram;
    private Collection nullCollection;
    private Common amtToDistribute;
    private Common totAmtDueForGrp;
    private Common totAmtDistForGrp;
    private Common totAmtToDistByPerson;
    private Common tmpDebtDtlBal;
    private Array<GroupGroup> group;
    private Array<TmpGroup> tmp;
    private Array<NullGroup> null1;
    private Array<OfObTypeGroup> ofObType;
    private Array<OfPgmTypeGroup> ofPgmType;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private Common tmpForCalc;
    private Common debtEligibleForDist;
    private DebtDetail debtDetail;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea collection;
    private Common pgmTypeMatchFound;
    private LegalAction nullLegalAction;
    private LegalAction legalAction;
    private Common loop;
    private Common prevLoopAmtToDist;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingDistributionPolicy.
    /// </summary>
    [JsonPropertyName("existingDistributionPolicy")]
    public DistributionPolicy ExistingDistributionPolicy
    {
      get => existingDistributionPolicy ??= new();
      set => existingDistributionPolicy = value;
    }

    /// <summary>
    /// A value of ExistingDistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("existingDistributionPolicyRule")]
    public DistributionPolicyRule ExistingDistributionPolicyRule
    {
      get => existingDistributionPolicyRule ??= new();
      set => existingDistributionPolicyRule = value;
    }

    /// <summary>
    /// A value of ExistingDprObligationType.
    /// </summary>
    [JsonPropertyName("existingDprObligationType")]
    public ObligationType ExistingDprObligationType
    {
      get => existingDprObligationType ??= new();
      set => existingDprObligationType = value;
    }

    /// <summary>
    /// A value of ExistingDprObligType.
    /// </summary>
    [JsonPropertyName("existingDprObligType")]
    public DprObligType ExistingDprObligType
    {
      get => existingDprObligType ??= new();
      set => existingDprObligType = value;
    }

    /// <summary>
    /// A value of ExistingDprProgram.
    /// </summary>
    [JsonPropertyName("existingDprProgram")]
    public Program ExistingDprProgram
    {
      get => existingDprProgram ??= new();
      set => existingDprProgram = value;
    }

    /// <summary>
    /// A value of ExistingDprDprProgram.
    /// </summary>
    [JsonPropertyName("existingDprDprProgram")]
    public DprProgram ExistingDprDprProgram
    {
      get => existingDprDprProgram ??= new();
      set => existingDprDprProgram = value;
    }

    /// <summary>
    /// A value of ExistingFederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("existingFederalDebtSetoff")]
    public AdministrativeActCertification ExistingFederalDebtSetoff
    {
      get => existingFederalDebtSetoff ??= new();
      set => existingFederalDebtSetoff = value;
    }

    /// <summary>
    /// A value of ExistingAdmActCertDebtDetail.
    /// </summary>
    [JsonPropertyName("existingAdmActCertDebtDetail")]
    public AdmActCertDebtDetail ExistingAdmActCertDebtDetail
    {
      get => existingAdmActCertDebtDetail ??= new();
      set => existingAdmActCertDebtDetail = value;
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
    /// A value of ExistingKeyOnlyFederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyFederalDebtSetoff")]
    public AdministrativeActCertification ExistingKeyOnlyFederalDebtSetoff
    {
      get => existingKeyOnlyFederalDebtSetoff ??= new();
      set => existingKeyOnlyFederalDebtSetoff = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryObligationType.
    /// </summary>
    [JsonPropertyName("existingPrimaryObligationType")]
    public ObligationType ExistingPrimaryObligationType
    {
      get => existingPrimaryObligationType ??= new();
      set => existingPrimaryObligationType = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryObligation.
    /// </summary>
    [JsonPropertyName("existingPrimaryObligation")]
    public Obligation ExistingPrimaryObligation
    {
      get => existingPrimaryObligation ??= new();
      set => existingPrimaryObligation = value;
    }

    /// <summary>
    /// A value of ExistingPrimaryObligationRln.
    /// </summary>
    [JsonPropertyName("existingPrimaryObligationRln")]
    public ObligationRln ExistingPrimaryObligationRln
    {
      get => existingPrimaryObligationRln ??= new();
      set => existingPrimaryObligationRln = value;
    }

    private CsePerson existingObligorKeyOnly;
    private CsePersonAccount existingKeyOnlyObligor;
    private ObligationType existingObligationType;
    private LegalAction existingLegalAction;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private DistributionPolicy existingDistributionPolicy;
    private DistributionPolicyRule existingDistributionPolicyRule;
    private ObligationType existingDprObligationType;
    private DprObligType existingDprObligType;
    private Program existingDprProgram;
    private DprProgram existingDprDprProgram;
    private AdministrativeActCertification existingFederalDebtSetoff;
    private AdmActCertDebtDetail existingAdmActCertDebtDetail;
    private CsePerson existingSupportedPerson;
    private CsePersonAccount existingKeyOnlySupported;
    private AdministrativeActCertification existingKeyOnlyFederalDebtSetoff;
    private ObligationType existingPrimaryObligationType;
    private Obligation existingPrimaryObligation;
    private ObligationRln existingPrimaryObligationRln;
  }
#endregion
}
