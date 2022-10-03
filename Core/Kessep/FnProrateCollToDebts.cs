// Program: FN_PRORATE_COLL_TO_DEBTS, ID: 372280778, model: 746.
// Short name: SWE02283
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PRORATE_COLL_TO_DEBTS.
/// </summary>
[Serializable]
public partial class FnProrateCollToDebts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PRORATE_COLL_TO_DEBTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProrateCollToDebts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProrateCollToDebts.
  /// </summary>
  public FnProrateCollToDebts(IContext context, Import import, Export export):
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
    export.ResetDprProcInd.Flag = "N";

    for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
      import.Debts.Index)
    {
      if (local.ForProrate.IsEmpty)
      {
        local.ForProrate.Index = 0;
        local.ForProrate.CheckSize();

        local.ForProrate.Update.ForProrateSuppPrsn.Number =
          import.Debts.Item.DebtsSuppPrsn.Number;

        if (AsChar(import.DistributionPolicyRule.ApplyTo) == 'D')
        {
          local.ForProrate.Update.ForProrate1.TotalCurrency =
            import.Debts.Item.DebtsDebtDetail.BalanceDueAmt;
          local.TotAmtDueForGrp.TotalCurrency += import.Debts.Item.
            DebtsDebtDetail.BalanceDueAmt;
        }
        else
        {
          local.ForProrate.Update.ForProrate1.TotalCurrency =
            import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
              GetValueOrDefault();
          local.TotAmtDueForGrp.TotalCurrency += import.Debts.Item.
            DebtsDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        }

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
          if (AsChar(import.DistributionPolicyRule.ApplyTo) == 'D')
          {
            local.ForProrate.Update.ForProrate1.TotalCurrency =
              local.ForProrate.Item.ForProrate1.TotalCurrency + import
              .Debts.Item.DebtsDebtDetail.BalanceDueAmt;
            local.TotAmtDueForGrp.TotalCurrency += import.Debts.Item.
              DebtsDebtDetail.BalanceDueAmt;
          }
          else
          {
            local.ForProrate.Update.ForProrate1.TotalCurrency =
              local.ForProrate.Item.ForProrate1.TotalCurrency + import
              .Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
                GetValueOrDefault();
            local.TotAmtDueForGrp.TotalCurrency += import.Debts.Item.
              DebtsDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          }

          goto Next1;
        }
      }

      local.ForProrate.CheckIndex();

      local.ForProrate.Index = local.ForProrate.Count;
      local.ForProrate.CheckSize();

      local.ForProrate.Update.ForProrateSuppPrsn.Number =
        import.Debts.Item.DebtsSuppPrsn.Number;

      if (AsChar(import.DistributionPolicyRule.ApplyTo) == 'D')
      {
        local.ForProrate.Update.ForProrate1.TotalCurrency =
          import.Debts.Item.DebtsDebtDetail.BalanceDueAmt;
        local.TotAmtDueForGrp.TotalCurrency += import.Debts.Item.
          DebtsDebtDetail.BalanceDueAmt;
      }
      else
      {
        local.ForProrate.Update.ForProrate1.TotalCurrency =
          import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
            GetValueOrDefault();
        local.TotAmtDueForGrp.TotalCurrency += import.Debts.Item.
          DebtsDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }

Next1:
      ;
    }

    // : Now we distribute the payment prorated across each Support Person 
    // appling the payment to the oldest Debt Detail within the DPR first.
    // : If the Collection Amount is greater than or equal to the total amount 
    // due for the group of Debt Details - Pay them all off in full!!!!
    if (import.AmtToDistribute.TotalCurrency >= local
      .TotAmtDueForGrp.TotalCurrency)
    {
      for(import.Debts.Index = 0; import.Debts.Index < import.Debts.Count; ++
        import.Debts.Index)
      {
        local.RecheckDebtsAgainstDpr.Flag = "N";

        if (AsChar(import.DistributionPolicyRule.ApplyTo) == 'D')
        {
          if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt == 0)
          {
            continue;
          }

          if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier || import
            .Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
              import.HardcodedUk.ProgramState))
            {
              goto Test1;
            }

            if (AsChar(import.Debts.Item.DebtsObligation.PrimarySecondaryCode) ==
              AsChar(import.HardcodedSecondary.PrimarySecondaryCode))
            {
              goto Test1;
            }

            UseFnDetermineUraForSuppPrsn();

            if (import.Debts.Item.DebtsObligationType.
              SystemGeneratedIdentifier == import
              .HardcodedMcType.SystemGeneratedIdentifier || import
              .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
              .HardcodedMjType.SystemGeneratedIdentifier || import
              .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
              .HardcodedMsType.SystemGeneratedIdentifier)
            {
              if (local.UraMedicalAmount.TotalCurrency <= 0)
              {
                export.ResetDprProcInd.Flag = "Y";

                goto Test7;
              }
              else if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt >= local
                .UraMedicalAmount.TotalCurrency)
              {
                export.ResetDprProcInd.Flag = "Y";
                local.RecheckDebtsAgainstDpr.Flag = "Y";
                import.Debts.Update.DebtsDebtDetail.BalanceDueAmt =
                  local.UraMedicalAmount.TotalCurrency;
              }
            }
            else if (local.UraAmount.TotalCurrency <= 0)
            {
              export.ResetDprProcInd.Flag = "Y";

              goto Test7;
            }
            else if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt >= local
              .UraAmount.TotalCurrency)
            {
              export.ResetDprProcInd.Flag = "Y";
              local.RecheckDebtsAgainstDpr.Flag = "Y";
              import.Debts.Update.DebtsDebtDetail.BalanceDueAmt =
                local.UraAmount.TotalCurrency;
            }
          }

Test1:

          import.Debts.Update.DebtsCollection.Amount =
            import.Debts.Item.DebtsDebtDetail.BalanceDueAmt;
          local.Tmp.TotalCurrency += import.Debts.Item.DebtsCollection.Amount;

          if (!Lt(import.Debts.Item.DebtsDebtDetail.DueDt,
            import.CollMonthStart.Date))
          {
            if (AsChar(import.Debts.Item.DebtsObligationType.Classification) ==
              AsChar(import.HardcodedAccruingClass.Classification))
            {
              import.Debts.Update.DebtsCollection.AppliedToCode = "C";
            }
            else
            {
              import.Debts.Update.DebtsCollection.AppliedToCode = "A";
            }
          }
          else
          {
            import.Debts.Update.DebtsCollection.AppliedToCode = "A";
          }
        }
        else
        {
          if (import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
            GetValueOrDefault() == 0)
          {
            continue;
          }

          if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
            .HardcodedAf.SystemGeneratedIdentifier || import
            .Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
              import.HardcodedUk.ProgramState))
            {
              goto Test2;
            }

            if (AsChar(import.Debts.Item.DebtsObligation.PrimarySecondaryCode) ==
              AsChar(import.HardcodedSecondary.PrimarySecondaryCode))
            {
              goto Test2;
            }

            UseFnDetermineUraForSuppPrsn();

            if (import.Debts.Item.DebtsObligationType.
              SystemGeneratedIdentifier == import
              .HardcodedMcType.SystemGeneratedIdentifier || import
              .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
              .HardcodedMjType.SystemGeneratedIdentifier || import
              .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
              .HardcodedMsType.SystemGeneratedIdentifier)
            {
              if (local.UraMedicalAmount.TotalCurrency <= 0)
              {
                export.ResetDprProcInd.Flag = "Y";

                goto Test7;
              }
              else if (import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
                GetValueOrDefault() >= local.UraMedicalAmount.TotalCurrency)
              {
                export.ResetDprProcInd.Flag = "Y";
                local.RecheckDebtsAgainstDpr.Flag = "Y";
                import.Debts.Update.DebtsDebtDetail.InterestBalanceDueAmt =
                  local.UraMedicalAmount.TotalCurrency;
              }
            }
            else if (local.UraAmount.TotalCurrency <= 0)
            {
              export.ResetDprProcInd.Flag = "Y";

              goto Test7;
            }
            else if (import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
              GetValueOrDefault() >= local.UraAmount.TotalCurrency)
            {
              export.ResetDprProcInd.Flag = "Y";
              local.RecheckDebtsAgainstDpr.Flag = "Y";
              import.Debts.Update.DebtsDebtDetail.InterestBalanceDueAmt =
                local.UraAmount.TotalCurrency;
            }
          }

Test2:

          import.Debts.Update.DebtsCollection.AppliedToCode = "I";
          import.Debts.Update.DebtsCollection.Amount =
            import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
              GetValueOrDefault();
          local.Tmp.TotalCurrency += import.Debts.Item.DebtsCollection.Amount;
        }

        if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
          .HardcodedAf.SystemGeneratedIdentifier || import
          .Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
          .HardcodedFc.SystemGeneratedIdentifier)
        {
          if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
            import.HardcodedUk.ProgramState))
          {
            goto Test3;
          }

          if (AsChar(import.Debts.Item.DebtsObligation.PrimarySecondaryCode) ==
            AsChar(import.HardcodedSecondary.PrimarySecondaryCode))
          {
            goto Test3;
          }

          UseFnApplyCollectionToUra();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(local.RecheckDebtsAgainstDpr.Flag) == 'Y')
          {
            UseFnRecheckDebtsAgainstDpr();
          }
        }

Test3:
        ;
      }
    }
    else
    {
      // : Prorate collection across debts with the same Supported Person by Due
      // Date - Oldest First!!
      local.ForProrate.Index = 0;

      for(var limit = local.ForProrate.Count; local.ForProrate.Index < limit; ++
        local.ForProrate.Index)
      {
        if (!local.ForProrate.CheckSize())
        {
          break;
        }

        if (local.Tmp.TotalCurrency == import.AmtToDistribute.TotalCurrency)
        {
          // -- 02/21/2018 GVandy  CQ56370  If all money has been distributed 
          // then escape.
          //    Corrects issue where there are 4 kids on the obligation and only
          // $.02 remaining to
          //    distribute.  Otherwise, due to the rounding, it was applying .01
          // to each of the
          //    first 3 kids and the last kid got -.01 which caused the payment 
          // to ultimately go
          //    into suspense as a SYSTEM ERROR.
          break;
        }

        local.RecheckDebtsAgainstDpr.Flag = "N";

        if (local.ForProrate.Index + 1 == local.ForProrate.Count)
        {
          local.TotAmtToDistByPerson.TotalCurrency =
            import.AmtToDistribute.TotalCurrency - local.Tmp.TotalCurrency;
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
          if (Equal(import.Debts.Item.DebtsSuppPrsn.Number,
            local.ForProrate.Item.ForProrateSuppPrsn.Number))
          {
            if (AsChar(import.DistributionPolicyRule.ApplyTo) == 'D')
            {
              if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt == 0)
              {
                continue;
              }

              if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
                .HardcodedAf.SystemGeneratedIdentifier || import
                .Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
                .HardcodedFc.SystemGeneratedIdentifier)
              {
                if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
                  import.HardcodedUk.ProgramState))
                {
                  goto Test4;
                }

                if (AsChar(import.Debts.Item.DebtsObligation.
                  PrimarySecondaryCode) == AsChar
                  (import.HardcodedSecondary.PrimarySecondaryCode))
                {
                  goto Test4;
                }

                UseFnDetermineUraForSuppPrsn();

                if (import.Debts.Item.DebtsObligationType.
                  SystemGeneratedIdentifier == import
                  .HardcodedMcType.SystemGeneratedIdentifier || import
                  .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMjType.SystemGeneratedIdentifier || import
                  .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMsType.SystemGeneratedIdentifier)
                {
                  if (local.UraMedicalAmount.TotalCurrency <= 0)
                  {
                    export.ResetDprProcInd.Flag = "Y";

                    goto Test7;
                  }
                  else if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt >= local
                    .UraMedicalAmount.TotalCurrency)
                  {
                    export.ResetDprProcInd.Flag = "Y";
                    import.Debts.Update.DebtsDebtDetail.BalanceDueAmt =
                      local.UraMedicalAmount.TotalCurrency;

                    if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt >= local
                      .TotAmtToDistByPerson.TotalCurrency)
                    {
                      local.RecheckDebtsAgainstDpr.Flag = "Y";
                    }
                  }
                }
                else if (local.UraAmount.TotalCurrency <= 0)
                {
                  export.ResetDprProcInd.Flag = "Y";

                  goto Test7;
                }
                else if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt >= local
                  .UraAmount.TotalCurrency)
                {
                  export.ResetDprProcInd.Flag = "Y";
                  import.Debts.Update.DebtsDebtDetail.BalanceDueAmt =
                    local.UraAmount.TotalCurrency;

                  if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt >= local
                    .TotAmtToDistByPerson.TotalCurrency)
                  {
                    local.RecheckDebtsAgainstDpr.Flag = "Y";
                  }
                }
              }

Test4:

              if (import.Debts.Item.DebtsDebtDetail.BalanceDueAmt <= local
                .TotAmtToDistByPerson.TotalCurrency)
              {
                import.Debts.Update.DebtsCollection.Amount =
                  import.Debts.Item.DebtsDebtDetail.BalanceDueAmt;
              }
              else
              {
                import.Debts.Update.DebtsCollection.Amount =
                  local.TotAmtToDistByPerson.TotalCurrency;
              }

              local.Tmp.TotalCurrency += import.Debts.Item.DebtsCollection.
                Amount;

              if (!Lt(import.Debts.Item.DebtsDebtDetail.DueDt,
                import.CollMonthStart.Date))
              {
                if (AsChar(import.Debts.Item.DebtsObligationType.Classification) ==
                  AsChar(import.HardcodedAccruingClass.Classification))
                {
                  import.Debts.Update.DebtsCollection.AppliedToCode = "C";
                }
                else
                {
                  import.Debts.Update.DebtsCollection.AppliedToCode = "A";
                }
              }
              else
              {
                import.Debts.Update.DebtsCollection.AppliedToCode = "A";
              }
            }
            else
            {
              if (import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
                GetValueOrDefault() == 0)
              {
                continue;
              }

              if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
                .HardcodedAf.SystemGeneratedIdentifier || import
                .Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
                .HardcodedFc.SystemGeneratedIdentifier)
              {
                if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
                  import.HardcodedUk.ProgramState))
                {
                  goto Test5;
                }

                if (AsChar(import.Debts.Item.DebtsObligation.
                  PrimarySecondaryCode) == AsChar
                  (import.HardcodedSecondary.PrimarySecondaryCode))
                {
                  goto Test5;
                }

                UseFnDetermineUraForSuppPrsn();

                if (import.Debts.Item.DebtsObligationType.
                  SystemGeneratedIdentifier == import
                  .HardcodedMcType.SystemGeneratedIdentifier || import
                  .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMjType.SystemGeneratedIdentifier || import
                  .Debts.Item.DebtsObligationType.SystemGeneratedIdentifier == import
                  .HardcodedMsType.SystemGeneratedIdentifier)
                {
                  if (local.UraMedicalAmount.TotalCurrency <= 0)
                  {
                    export.ResetDprProcInd.Flag = "Y";

                    goto Test7;
                  }
                  else if (import.Debts.Item.DebtsDebtDetail.
                    InterestBalanceDueAmt.GetValueOrDefault() >= local
                    .UraMedicalAmount.TotalCurrency)
                  {
                    export.ResetDprProcInd.Flag = "Y";
                    import.Debts.Update.DebtsDebtDetail.InterestBalanceDueAmt =
                      local.UraMedicalAmount.TotalCurrency;

                    if (import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
                      GetValueOrDefault() >= local
                      .TotAmtToDistByPerson.TotalCurrency)
                    {
                      local.RecheckDebtsAgainstDpr.Flag = "Y";
                    }
                  }
                }
                else if (local.UraAmount.TotalCurrency <= 0)
                {
                  export.ResetDprProcInd.Flag = "Y";

                  goto Test7;
                }
                else if (import.Debts.Item.DebtsDebtDetail.
                  InterestBalanceDueAmt.GetValueOrDefault() >= local
                  .UraAmount.TotalCurrency)
                {
                  export.ResetDprProcInd.Flag = "Y";
                  import.Debts.Update.DebtsDebtDetail.InterestBalanceDueAmt =
                    local.UraAmount.TotalCurrency;

                  if (import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
                    GetValueOrDefault() >= local
                    .TotAmtToDistByPerson.TotalCurrency)
                  {
                    local.RecheckDebtsAgainstDpr.Flag = "Y";
                  }
                }
              }

Test5:

              if (import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
                GetValueOrDefault() <= local
                .TotAmtToDistByPerson.TotalCurrency)
              {
                import.Debts.Update.DebtsCollection.Amount =
                  import.Debts.Item.DebtsDebtDetail.InterestBalanceDueAmt.
                    GetValueOrDefault();
              }
              else
              {
                import.Debts.Update.DebtsCollection.Amount =
                  local.TotAmtToDistByPerson.TotalCurrency;
              }

              local.Tmp.TotalCurrency += import.Debts.Item.DebtsCollection.
                Amount;
              import.Debts.Update.DebtsCollection.AppliedToCode = "I";
            }

            local.TotAmtToDistByPerson.TotalCurrency -= import.Debts.Item.
              DebtsCollection.Amount;
            local.TotAmtDistForGrp.TotalCurrency += import.Debts.Item.
              DebtsCollection.Amount;

            if (import.Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
              .HardcodedAf.SystemGeneratedIdentifier || import
              .Debts.Item.DebtsProgram.SystemGeneratedIdentifier == import
              .HardcodedFc.SystemGeneratedIdentifier)
            {
              if (Equal(import.Debts.Item.DebtsDprProgram.ProgramState,
                import.HardcodedUk.ProgramState))
              {
                goto Test6;
              }

              if (AsChar(import.Debts.Item.DebtsObligation.PrimarySecondaryCode) ==
                AsChar(import.HardcodedSecondary.PrimarySecondaryCode))
              {
                goto Test6;
              }

              UseFnApplyCollectionToUra();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (AsChar(local.RecheckDebtsAgainstDpr.Flag) == 'Y')
              {
                UseFnRecheckDebtsAgainstDpr();
              }
            }

Test6:

            if (local.TotAmtToDistByPerson.TotalCurrency == 0)
            {
              goto Next2;
            }
          }
        }

Next2:
        ;
      }

      local.ForProrate.CheckIndex();
    }

Test7:

    import.AmtToDistribute.TotalCurrency -= local.Tmp.TotalCurrency;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveDebts1(Import.DebtsGroup source,
    FnRecheckDebtsAgainstDpr.Import.DebtsGroup target)
  {
    target.DebtsObligationType.Assign(source.DebtsObligationType);
    target.DebtsObligation.Assign(source.DebtsObligation);
    target.DebtsDebt.SystemGeneratedIdentifier =
      source.DebtsDebt.SystemGeneratedIdentifier;
    target.DebtsDebtDetail.Assign(source.DebtsDebtDetail);
    MoveCollection(source.DebtsCollection, target.DebtsCollection);
    target.DebtsSuppPrsn.Number = source.DebtsSuppPrsn.Number;
    target.DebtsProgram.Assign(source.DebtsProgram);
    target.DebtsDprProgram.ProgramState = source.DebtsDprProgram.ProgramState;
    target.DebtsLegalAction.StandardNumber =
      source.DebtsLegalAction.StandardNumber;
  }

  private static void MoveDebts2(FnRecheckDebtsAgainstDpr.Import.
    DebtsGroup source, Import.DebtsGroup target)
  {
    target.DebtsObligationType.Assign(source.DebtsObligationType);
    target.DebtsObligation.Assign(source.DebtsObligation);
    target.DebtsDebt.SystemGeneratedIdentifier =
      source.DebtsDebt.SystemGeneratedIdentifier;
    target.DebtsDebtDetail.Assign(source.DebtsDebtDetail);
    MoveCollection(source.DebtsCollection, target.DebtsCollection);
    target.DebtsSuppPrsn.Number = source.DebtsSuppPrsn.Number;
    target.DebtsProgram.Assign(source.DebtsProgram);
    target.DebtsDprProgram.ProgramState = source.DebtsDprProgram.ProgramState;
    target.DebtsLegalAction.StandardNumber =
      source.DebtsLegalAction.StandardNumber;
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
    FnRecheckDebtsAgainstDpr.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl4);
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
    FnRecheckDebtsAgainstDpr.Import.HhHistDtlGroup target)
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
    FnRecheckDebtsAgainstDpr.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl3);
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
    FnRecheckDebtsAgainstDpr.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
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
    FnRecheckDebtsAgainstDpr.Import.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHistDtl(Import.PgmHistDtlGroup source,
    FnRecheckDebtsAgainstDpr.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MovePgmType(Import.PgmTypeGroup source,
    FnRecheckDebtsAgainstDpr.Import.PgmTypeGroup target)
  {
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private void UseFnApplyCollectionToUra()
  {
    var useImport = new FnApplyCollectionToUra.Import();
    var useExport = new FnApplyCollectionToUra.Export();

    useImport.Collection1.Date = import.Collection.Date;
    import.Legal.CopyTo(useImport.Legal, MoveLegal1);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    MoveObligationType(import.Debts.Item.DebtsObligationType,
      useImport.ObligationType);
    useImport.DebtDetail.DueDt = import.Debts.Item.DebtsDebtDetail.DueDt;
    useImport.Collection2.Amount = import.Debts.Item.DebtsCollection.Amount;
    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;
    useImport.LegalAction.StandardNumber =
      import.Debts.Item.DebtsLegalAction.StandardNumber;

    Call(FnApplyCollectionToUra.Execute, useImport, useExport);

    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist1);
  }

  private void UseFnDetermineUraForSuppPrsn()
  {
    var useImport = new FnDetermineUraForSuppPrsn.Import();
    var useExport = new FnDetermineUraForSuppPrsn.Export();

    useImport.SuppPrsn.Number = import.Debts.Item.DebtsSuppPrsn.Number;
    useImport.LegalAction.StandardNumber =
      import.Debts.Item.DebtsLegalAction.StandardNumber;
    useImport.Collection.Date = import.Collection.Date;
    import.Legal.CopyTo(useImport.Legal, MoveLegal2);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist3);

    Call(FnDetermineUraForSuppPrsn.Execute, useImport, useExport);

    local.UraAmount.TotalCurrency = useExport.UraAmount.TotalCurrency;
    local.UraMedicalAmount.TotalCurrency =
      useExport.UraMedicalAmount.TotalCurrency;
  }

  private void UseFnRecheckDebtsAgainstDpr()
  {
    var useImport = new FnRecheckDebtsAgainstDpr.Import();
    var useExport = new FnRecheckDebtsAgainstDpr.Export();

    useImport.ResetDprProcInd.Flag = export.ResetDprProcInd.Flag;
    useImport.LegalAction.StandardNumber =
      import.Debts.Item.DebtsLegalAction.StandardNumber;
    useImport.DistributionPolicyRule.ApplyTo =
      import.DistributionPolicyRule.ApplyTo;
    import.Debts.CopyTo(useImport.Debts, MoveDebts1);
    import.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist);
    import.Legal.CopyTo(useImport.Legal, MoveLegal3);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist4);
    useImport.CollectionType.SequentialIdentifier =
      import.CollectionType.SequentialIdentifier;
    useImport.Collection.Date = import.Collection.Date;
    import.PgmType.CopyTo(useImport.PgmType, MovePgmType);
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
    useImport.HardcodedUp.ProgramState = import.HardcodedUd.ProgramState;
    useImport.HardcodedUk.ProgramState = import.HardcodedUk.ProgramState;
    useImport.HardcodedFedFType.SequentialIdentifier =
      import.HardcodedFedFType.SequentialIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    useImport.PrworaDateOfConversion.Date = import.PrworaDateOfConversion.Date;

    Call(FnRecheckDebtsAgainstDpr.Execute, useImport, useExport);

    export.ResetDprProcInd.Flag = useImport.ResetDprProcInd.Flag;
    useImport.Debts.CopyTo(import.Debts, MoveDebts2);
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
    /// A value of AmtToDistribute.
    /// </summary>
    [JsonPropertyName("amtToDistribute")]
    public Common AmtToDistribute
    {
      get => amtToDistribute ??= new();
      set => amtToDistribute = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    private DistributionPolicyRule distributionPolicyRule;
    private Array<DebtsGroup> debts;
    private Common amtToDistribute;
    private DateWorkArea collMonthStart;
    private CollectionType collectionType;
    private DateWorkArea collection;
    private Program program;
    private CashReceiptDetail distBy;
    private Array<PgmHistGroup> pgmHist;
    private Array<PgmTypeGroup> pgmType;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ResetDprProcInd.
    /// </summary>
    [JsonPropertyName("resetDprProcInd")]
    public Common ResetDprProcInd
    {
      get => resetDprProcInd ??= new();
      set => resetDprProcInd = value;
    }

    private Common resetDprProcInd;
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
    /// A value of UraExistsForTypeOnly.
    /// </summary>
    [JsonPropertyName("uraExistsForTypeOnly")]
    public TextWorkArea UraExistsForTypeOnly
    {
      get => uraExistsForTypeOnly ??= new();
      set => uraExistsForTypeOnly = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
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
    /// A value of UraAmount.
    /// </summary>
    [JsonPropertyName("uraAmount")]
    public Common UraAmount
    {
      get => uraAmount ??= new();
      set => uraAmount = value;
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

    /// <summary>
    /// A value of AmtToDistPerSuppPrsn.
    /// </summary>
    [JsonPropertyName("amtToDistPerSuppPrsn")]
    public Common AmtToDistPerSuppPrsn
    {
      get => amtToDistPerSuppPrsn ??= new();
      set => amtToDistPerSuppPrsn = value;
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
    /// A value of RecheckDebtsAgainstDpr.
    /// </summary>
    [JsonPropertyName("recheckDebtsAgainstDpr")]
    public Common RecheckDebtsAgainstDpr
    {
      get => recheckDebtsAgainstDpr ??= new();
      set => recheckDebtsAgainstDpr = value;
    }

    private TextWorkArea uraExistsForTypeOnly;
    private Common tmp;
    private Common uraMedicalAmount;
    private Common uraAmount;
    private Common totAmtToDistByPerson;
    private Common tmpForCalc;
    private Common amtToDistPerSuppPrsn;
    private Common totAmtDueForGrp;
    private Common totAmtDistForGrp;
    private Array<ForProrateGroup> forProrate;
    private Common recheckDebtsAgainstDpr;
  }
#endregion
}
