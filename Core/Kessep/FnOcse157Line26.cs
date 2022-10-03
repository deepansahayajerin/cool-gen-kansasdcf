// Program: FN_OCSE157_LINE_26, ID: 371115306, model: 746.
// Short name: SWE02955
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_26.
/// </summary>
[Serializable]
public partial class FnOcse157Line26: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_26 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line26(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line26.
  /// </summary>
  public FnOcse157Line26(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // Initial version - 08/2001
    // --------------------------------------------------------------
    // --------------------------------------------------------
    // 10/8/2001
    // - Also read 'C'urrent collections.
    // - Include Concurrent collections.
    // --------------------------------------------------------
    // --------------------------------------------------------
    // 10/11/2001
    // - Clear local_prev view for each iteration.
    // --------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------
    // 04/14/08  GVandy	CQ#2461		Per federal data reliability audit, exclude all
    // incoming interstate obligations.
    // ----------------------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "26";
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "26 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);

      // --------------------------------------------------------------
      // Initialize counters
      // ---------------------------------------------------------------
      local.Line26Curr.Currency152 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 15)) / (
          decimal)100;
      local.Line26Former.Currency152 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 29, 15)) / (
          decimal)100;
      local.Line26Never.Currency152 =
        StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 44, 15)) / (
          decimal)100;
    }

    // -------------------------------------------------------------------
    // Read all Obligors. Maintain a running total for each Supp
    // person and then process a break in person #. This is
    // necessary so we only determine assistance once per Supp
    // person (as opposed to once per Debt or Collection)
    // -------------------------------------------------------------------
    foreach(var item in ReadObligorCsePerson())
    {
      if (Equal(entities.ApCsePerson.Number, local.PrevAp.Number))
      {
        continue;
      }

      local.PrevAp.Number = entities.ApCsePerson.Number;
      local.ActivityAfterFyEnd.Flag = "N";

      // --------------------------------------------------------
      // 10/8/2001
      // - Also read 'C'urrent collections.
      // - Include Concurrent collections.
      // --------------------------------------------------------
      if (ReadCollection1())
      {
        local.ActivityAfterFyEnd.Flag = "Y";
      }
      else if (ReadDebtAdjustment1())
      {
        local.ActivityAfterFyEnd.Flag = "Y";
      }
      else
      {
        // ---------------------
        // Not an error.
        // ---------------------
      }

      local.Prev.Number = "";

      // --------------------------------------------
      // No need to check for overlapping roles.
      // Read Each will fetch distinct rows.
      // --------------------------------------------
      foreach(var item1 in ReadCsePersonSupported())
      {
        if (Equal(entities.Supp.Number, local.Prev.Number))
        {
          continue;
        }

        local.Prev.Number = entities.Supp.Number;
        ++local.CommitCnt.Count;

        // --------------------------------------------
        // Clear local views.
        // --------------------------------------------
        MoveOcse157Verification3(local.NullOcse157Verification,
          local.ForCreateOcse157Verification);
        local.ArrearsForPers.Currency152 = 0;

        // -------------------------------------------------------------------
        // Step #1. - Read debts where bal is due on 'run date'.
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // -READ debts with bal due
        // -Accruing debts must be due atleast 1 month before FY end.
        // -Non accruing debts are due upon creation.(Due Dt is irrelevant)
        // -Skip Fees, Recoveries
        // -Skip debts created after FY end.
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // -Exclude incoming interstate obligations. 04/14/08 CQ2461.
        // -------------------------------------------------------------------
        foreach(var item2 in ReadDebtObligationObligationTypeDebtDetail3())
        {
          // -------------------------------------------------------------------
          // -Skip 718B
          // -------------------------------------------------------------------
          if (Equal(entities.ObligationType.Code, "718B"))
          {
            continue;
          }

          // -------------------------------------------------------------------
          // In a J/S situation, include the first obligation.
          // -------------------------------------------------------------------
          if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
          {
            if (!ReadObligationRln())
            {
              // -------------------------------------------------------------------
              // This must be the second obligation. Skip.
              // -------------------------------------------------------------------
              continue;
            }
          }

          // -------------------------------------------------------------------
          // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
          // -------------------------------------------------------------------
          if (Equal(entities.ObligationType.Code, "MJ"))
          {
            UseFnDeterminePgmForDebtDetail();

            if (Equal(local.Program.Code, "AF") || Equal
              (local.Program.Code, "AFI") || Equal
              (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
            {
              // -----------------------------------------------
              // Skip this debt detail.
              // -----------------------------------------------
              continue;
            }
          }

          // ---------------------------------------------
          // Maintain running totals for this person.
          // --------------------------------------------
          local.ArrearsForPers.Currency152 += entities.DebtDetail.BalanceDueAmt;

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              entities.ApCsePerson.Number;
            local.ForCreateOcse157Verification.ObTypeSgi =
              entities.ObligationType.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.DebtDetailDueDt =
              entities.DebtDetail.DueDt;
            local.ForCreateOcse157Verification.DebtDetailBalanceDue =
              entities.DebtDetail.BalanceDueAmt;
            local.ForCreateOcse157Verification.ObTranSgi =
              entities.Debt.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.ObligationSgi =
              entities.Obligation.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.Supp.Number;
            UseFn157GetAssistanceForPerson();
            local.ForCreateOcse157Verification.PersonProgCode =
              local.ForVerification.Code;

            switch(AsChar(local.Assistance.Flag))
            {
              case 'C':
                local.ForCreateOcse157Verification.Column = "b";

                break;
              case 'F':
                local.ForCreateOcse157Verification.Column = "c";

                break;
              default:
                local.ForCreateOcse157Verification.Column = "d";

                break;
            }

            UseFnCreateOcse157Verification();
            MoveOcse157Verification3(local.NullOcse157Verification,
              local.ForCreateOcse157Verification);
          }

          // --------------------------------------------
          // Process Debt Adj after FY end.
          // --------------------------------------------
          foreach(var item3 in ReadDebtAdjustment2())
          {
            // ------------------------------------------------------------------
            // 'I' type adj increases balance_due. So we need to 'subtract'
            // this amt from balance at run time to get balance due on 9/30.
            // Similarily, we need to add 'D' type adj amount.
            // ------------------------------------------------------------------
            if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
            {
              local.ArrearsForPers.Currency152 -= entities.DebtAdjustment.
                Amount;
            }
            else
            {
              local.ArrearsForPers.Currency152 += entities.DebtAdjustment.
                Amount;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.DebtDetailDueDt =
                entities.DebtDetail.DueDt;
              local.ForCreateOcse157Verification.DebtAdjType =
                entities.DebtAdjustment.DebtAdjustmentType;
              local.ForCreateOcse157Verification.ObTranSgi =
                entities.DebtAdjustment.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.ObligationSgi =
                entities.Obligation.SystemGeneratedIdentifier;

              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.ForCreateOcse157Verification.ObTranAmount =
                  -entities.DebtAdjustment.Amount;
              }
              else
              {
                local.ForCreateOcse157Verification.ObTranAmount =
                  entities.DebtAdjustment.Amount;
              }

              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.Supp.Number;
              UseFn157GetAssistanceForPerson();
              local.ForCreateOcse157Verification.PersonProgCode =
                local.ForVerification.Code;

              switch(AsChar(local.Assistance.Flag))
              {
                case 'C':
                  local.ForCreateOcse157Verification.Column = "b";

                  break;
                case 'F':
                  local.ForCreateOcse157Verification.Column = "c";

                  break;
                default:
                  local.ForCreateOcse157Verification.Column = "d";

                  break;
              }

              UseFnCreateOcse157Verification();
              MoveOcse157Verification3(local.NullOcse157Verification,
                local.ForCreateOcse157Verification);
            }

            // ---------------------------------------------
            // End of Debt Adjustment READ EACH.
            // --------------------------------------------
          }

          // -------------------------------------------------------------------
          // Process collections and coll adj after FY end.
          // Read non-concurrent collections only.
          // -------------------------------------------------------------------
          // --------------------------------------------------------
          // 10/8/2001
          // - Also read 'C'urrent collections.
          // - Include Concurrent collections.
          // --------------------------------------------------------
          foreach(var item3 in ReadCollection5())
          {
            // ---------------------------------------------
            // Maintain running totals for this person.
            // --------------------------------------------
            if (Lt(import.ReportEndDate.Timestamp,
              entities.Collection.CreatedTmst))
            {
              local.ArrearsForPers.Currency152 += entities.Collection.Amount;
            }
            else
            {
              local.ArrearsForPers.Currency152 -= entities.Collection.Amount;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;

              if (Lt(import.ReportEndDate.Timestamp,
                entities.Collection.CreatedTmst))
              {
                local.ForCreateOcse157Verification.CollectionAmount =
                  entities.Collection.Amount;
              }
              else
              {
                local.ForCreateOcse157Verification.CollectionAmount =
                  -entities.Collection.Amount;
              }

              local.ForCreateOcse157Verification.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CollectionDte =
                entities.Collection.CollectionDt;
              local.ForCreateOcse157Verification.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreateOcse157Verification.CollApplToCode =
                entities.Collection.AppliedToCode;
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.DebtDetailDueDt =
                entities.DebtDetail.DueDt;
              local.ForCreateOcse157Verification.ObTranSgi =
                entities.Debt.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.ObligationSgi =
                entities.Obligation.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.Supp.Number;
              UseFn157GetAssistanceForPerson();
              local.ForCreateOcse157Verification.PersonProgCode =
                local.ForVerification.Code;

              switch(AsChar(local.Assistance.Flag))
              {
                case 'C':
                  local.ForCreateOcse157Verification.Column = "b";

                  break;
                case 'F':
                  local.ForCreateOcse157Verification.Column = "c";

                  break;
                default:
                  local.ForCreateOcse157Verification.Column = "d";

                  break;
              }

              UseFnCreateOcse157Verification();
              MoveOcse157Verification3(local.NullOcse157Verification,
                local.ForCreateOcse157Verification);
            }

            // --------------------------------------------
            // End of Collection READ EACH.
            // --------------------------------------------
          }

          // --------------------------------------------
          // End of non-zero bal Debt READ EACH.
          // --------------------------------------------
        }

        // -------------------------------------------------------------------
        // Step # 2. - Look for debts with 'Zero' bal due but where a
        // Collection is applied after FY end.
        // -------------------------------------------------------------------
        // -----------------------------------------------------------------------
        // Only do this if there is any collection/debt activity
        // for AP after FY end. There is no point in spinning through all
        // Zero debts if there is no activity at all for this AP.
        // -----------------------------------------------------------------------
        if (AsChar(local.ActivityAfterFyEnd.Flag) == 'Y')
        {
          // --------------------------------------------------------
          // 10/8/2001
          // - Also read 'C'urrent collections.
          // - Include Concurrent collections.
          // --------------------------------------------------------
          // -------------------------------------------------------------------
          // -Exclude incoming interstate obligations. 04/14/08 CQ2461.
          // -------------------------------------------------------------------
          foreach(var item2 in ReadDebtObligationObligationTypeDebtDetail1())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // In a J/S situation, include the first obligation.
            // -------------------------------------------------------------------
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
            {
              if (!ReadObligationRln())
              {
                // -------------------------------------------------------------------
                // This must be the second obligation. Skip.
                // -------------------------------------------------------------------
                continue;
              }
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // --------------------------------------------
            // Process Debt Adj after FY end.
            // --------------------------------------------
            foreach(var item3 in ReadDebtAdjustment2())
            {
              // ------------------------------------------------------------------
              // 'I' type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily, we need to add 'D' type adj amount.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.ArrearsForPers.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.ArrearsForPers.Currency152 += entities.DebtAdjustment.
                  Amount;
              }

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.DebtDetailDueDt =
                  entities.DebtDetail.DueDt;
                local.ForCreateOcse157Verification.DebtAdjType =
                  entities.DebtAdjustment.DebtAdjustmentType;
                local.ForCreateOcse157Verification.ObTranSgi =
                  entities.DebtAdjustment.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.ObligationSgi =
                  entities.Obligation.SystemGeneratedIdentifier;

                if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
                {
                  local.ForCreateOcse157Verification.ObTranAmount =
                    -entities.DebtAdjustment.Amount;
                }
                else
                {
                  local.ForCreateOcse157Verification.ObTranAmount =
                    entities.DebtAdjustment.Amount;
                }

                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.Supp.Number;
                UseFn157GetAssistanceForPerson();
                local.ForCreateOcse157Verification.PersonProgCode =
                  local.ForVerification.Code;

                switch(AsChar(local.Assistance.Flag))
                {
                  case 'C':
                    local.ForCreateOcse157Verification.Column = "b";

                    break;
                  case 'F':
                    local.ForCreateOcse157Verification.Column = "c";

                    break;
                  default:
                    local.ForCreateOcse157Verification.Column = "d";

                    break;
                }

                UseFnCreateOcse157Verification();
                MoveOcse157Verification3(local.NullOcse157Verification,
                  local.ForCreateOcse157Verification);
              }

              // ---------------------------------------------
              // End of Debt Adjustment READ EACH.
              // --------------------------------------------
            }

            // -------------------------------------------------------------------
            // Process collections and coll adj after FY end.
            // Read non-concurrent collections only.
            // -------------------------------------------------------------------
            // --------------------------------------------------------
            // 10/8/2001
            // - Also read 'C'urrent collections.
            // - Include Concurrent collections.
            // --------------------------------------------------------
            foreach(var item3 in ReadCollection4())
            {
              // ---------------------------------------------
              // Maintain running totals for this person.
              // --------------------------------------------
              if (Lt(import.ReportEndDate.Timestamp,
                entities.Collection.CreatedTmst))
              {
                local.ArrearsForPers.Currency152 += entities.Collection.Amount;
              }
              else
              {
                local.ArrearsForPers.Currency152 -= entities.Collection.Amount;
              }

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;

                if (Lt(import.ReportEndDate.Timestamp,
                  entities.Collection.CreatedTmst))
                {
                  local.ForCreateOcse157Verification.CollectionAmount =
                    entities.Collection.Amount;
                }
                else
                {
                  local.ForCreateOcse157Verification.CollectionAmount =
                    -entities.Collection.Amount;
                }

                local.ForCreateOcse157Verification.CollectionSgi =
                  entities.Collection.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CollectionDte =
                  entities.Collection.CollectionDt;
                local.ForCreateOcse157Verification.CollCreatedDte =
                  Date(entities.Collection.CreatedTmst);
                local.ForCreateOcse157Verification.CollApplToCode =
                  entities.Collection.AppliedToCode;
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.DebtDetailDueDt =
                  entities.DebtDetail.DueDt;
                local.ForCreateOcse157Verification.ObTranSgi =
                  entities.Debt.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.ObligationSgi =
                  entities.Obligation.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.Supp.Number;
                UseFn157GetAssistanceForPerson();
                local.ForCreateOcse157Verification.PersonProgCode =
                  local.ForVerification.Code;

                switch(AsChar(local.Assistance.Flag))
                {
                  case 'C':
                    local.ForCreateOcse157Verification.Column = "b";

                    break;
                  case 'F':
                    local.ForCreateOcse157Verification.Column = "c";

                    break;
                  default:
                    local.ForCreateOcse157Verification.Column = "d";

                    break;
                }

                UseFnCreateOcse157Verification();
                MoveOcse157Verification3(local.NullOcse157Verification,
                  local.ForCreateOcse157Verification);
              }

              // --------------------------------------------
              // End of Collection READ EACH.
              // --------------------------------------------
            }

            // --------------------------------------------
            // End of Debt w/Coll READ EACH.
            // --------------------------------------------
          }

          // -------------------------------------------------------------------
          // Step # 3. - Look for debts with 'Zero' bal due but where a
          // D-type adjustment is made to debt after FY end.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // -Exclude incoming interstate obligations. 04/14/08 CQ2461.
          // -------------------------------------------------------------------
          foreach(var item2 in ReadDebtObligationObligationTypeDebtDetail2())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // In a J/S situation, include the first obligation.
            // -------------------------------------------------------------------
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
            {
              if (!ReadObligationRln())
              {
                // -------------------------------------------------------------------
                // This must be the second obligation. Skip.
                // -------------------------------------------------------------------
                continue;
              }
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // ---------------------------------------------------
            // If a collection is applied to debt after FY end,
            // then debt must have been processed in Step # 2.
            // ---------------------------------------------------
            // --------------------------------------------------------
            // 10/8/2001
            // - Also read 'C'urrent collections.
            // - Include Concurrent collections.
            // --------------------------------------------------------
            foreach(var item3 in ReadCollection3())
            {
              goto ReadEach;
            }

            // --------------------------------------------
            // Process Debt Adj after FY end.
            // --------------------------------------------
            foreach(var item3 in ReadDebtAdjustment2())
            {
              // ------------------------------------------------------------------
              // 'I' type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily, we need to add 'D' type adj amount.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.ArrearsForPers.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.ArrearsForPers.Currency152 += entities.DebtAdjustment.
                  Amount;
              }

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.DebtDetailDueDt =
                  entities.DebtDetail.DueDt;
                local.ForCreateOcse157Verification.DebtAdjType =
                  entities.DebtAdjustment.DebtAdjustmentType;
                local.ForCreateOcse157Verification.ObTranSgi =
                  entities.DebtAdjustment.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.ObligationSgi =
                  entities.Obligation.SystemGeneratedIdentifier;

                if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
                {
                  local.ForCreateOcse157Verification.ObTranAmount =
                    -entities.DebtAdjustment.Amount;
                }
                else
                {
                  local.ForCreateOcse157Verification.ObTranAmount =
                    entities.DebtAdjustment.Amount;
                }

                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.Supp.Number;
                UseFn157GetAssistanceForPerson();
                local.ForCreateOcse157Verification.PersonProgCode =
                  local.ForVerification.Code;

                switch(AsChar(local.Assistance.Flag))
                {
                  case 'C':
                    local.ForCreateOcse157Verification.Column = "b";

                    break;
                  case 'F':
                    local.ForCreateOcse157Verification.Column = "c";

                    break;
                  default:
                    local.ForCreateOcse157Verification.Column = "d";

                    break;
                }

                UseFnCreateOcse157Verification();
                MoveOcse157Verification3(local.NullOcse157Verification,
                  local.ForCreateOcse157Verification);
              }

              // --------------------------------------------
              // End of Debt Adjustment READ EACH.
              // --------------------------------------------
            }

            // -------------------------------------------------------------------
            // Process collection adjustments after FY end.
            // Read non-concurrent collections only.
            // -------------------------------------------------------------------
            // --------------------------------------------------------
            // 10/8/2001
            // - Also read 'C'urrent collections.
            // - Include Concurrent collections.
            // --------------------------------------------------------
            foreach(var item3 in ReadCollection2())
            {
              // ---------------------------------------------
              // Maintain running totals for this person.
              // --------------------------------------------
              local.ArrearsForPers.Currency152 -= entities.Collection.Amount;

              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;
                local.ForCreateOcse157Verification.CollectionAmount =
                  -entities.Collection.Amount;
                local.ForCreateOcse157Verification.CollectionSgi =
                  entities.Collection.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.CollectionDte =
                  entities.Collection.CollectionDt;
                local.ForCreateOcse157Verification.CollCreatedDte =
                  Date(entities.Collection.CreatedTmst);
                local.ForCreateOcse157Verification.CollApplToCode =
                  entities.Collection.AppliedToCode;
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.DebtDetailDueDt =
                  entities.DebtDetail.DueDt;
                local.ForCreateOcse157Verification.ObTranSgi =
                  entities.Debt.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.ObligationSgi =
                  entities.Obligation.SystemGeneratedIdentifier;
                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.Supp.Number;
                UseFn157GetAssistanceForPerson();
                local.ForCreateOcse157Verification.PersonProgCode =
                  local.ForVerification.Code;

                switch(AsChar(local.Assistance.Flag))
                {
                  case 'C':
                    local.ForCreateOcse157Verification.Column = "b";

                    break;
                  case 'F':
                    local.ForCreateOcse157Verification.Column = "c";

                    break;
                  default:
                    local.ForCreateOcse157Verification.Column = "d";

                    break;
                }

                UseFnCreateOcse157Verification();
                MoveOcse157Verification3(local.NullOcse157Verification,
                  local.ForCreateOcse157Verification);
              }

              // --------------------------------------------
              // End of Collection READ EACH.
              // --------------------------------------------
            }

            // --------------------------------------------
            // End of Debt w/ debt adj  READ EACH.
            // --------------------------------------------
ReadEach:
            ;
          }
        }

        // -------------------------------------------------------
        // *** Finished processing all Debts for this person. ***
        // -------------------------------------------------------
        if (local.ArrearsForPers.Currency152 != 0)
        {
          UseFn157GetAssistanceForPerson();

          switch(AsChar(local.Assistance.Flag))
          {
            case 'C':
              local.Line26Curr.Currency152 += local.ArrearsForPers.Currency152;

              break;
            case 'F':
              local.Line26Former.Currency152 += local.ArrearsForPers.
                Currency152;

              break;
            default:
              local.Line26Never.Currency152 += local.ArrearsForPers.Currency152;

              break;
          }
        }

        // --------------------------------------------
        // End of Supp Person READ EACH.
        // --------------------------------------------
      }

      // ---------------------------------------------
      // Check commit counts.
      // --------------------------------------------
      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "26 " + entities
          .ApCsePerson.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          ((long)(local.Line26Curr.Currency152 * 100), 15);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          ((long)(local.Line26Former.Currency152 * 100), 15);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          ((long)(local.Line26Never.Currency152 * 100), 15);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.SuppPersonNumber = entities.ApCsePerson.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      // --------------------------------------------
      // End of driving READ EACH.
      // --------------------------------------------
    }

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "26";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line26Curr.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line26Former.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number =
      (long?)Math.Round(
        local.Line26Never.Currency152, MidpointRounding.AwayFromZero);
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "27 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "26";
      local.ForError.SuppPersonNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveOcse157Verification3(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private void UseFn157GetAssistanceForPerson()
  {
    var useImport = new Fn157GetAssistanceForPerson.Import();
    var useExport = new Fn157GetAssistanceForPerson.Export();

    useImport.CsePerson.Number = entities.Supp.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    Call(Fn157GetAssistanceForPerson.Execute, useImport, useExport);

    local.ForVerification.Code = useExport.Program.Code;
    local.Assistance.Flag = useExport.AssistanceProgram.Flag;
  }

  private void UseFnCreateOcse157Data()
  {
    var useImport = new FnCreateOcse157Data.Import();
    var useExport = new FnCreateOcse157Data.Export();

    useImport.Ocse157Data.Assign(local.ForCreateOcse157Data);

    Call(FnCreateOcse157Data.Execute, useImport, useExport);
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.ForCreateOcse157Verification,
      useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.SupportedPerson.Number = entities.Supp.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    MoveOcse157Verification2(local.ForError, useImport.Ocse157Verification);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.AfterFy.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.AfterFy.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.AfterFy.AppliedToCode = db.GetString(reader, 1);
        entities.AfterFy.CollectionDt = db.GetDate(reader, 2);
        entities.AfterFy.AdjustedInd = db.GetNullableString(reader, 3);
        entities.AfterFy.ConcurrentInd = db.GetString(reader, 4);
        entities.AfterFy.CrtType = db.GetInt32(reader, 5);
        entities.AfterFy.CstId = db.GetInt32(reader, 6);
        entities.AfterFy.CrvId = db.GetInt32(reader, 7);
        entities.AfterFy.CrdId = db.GetInt32(reader, 8);
        entities.AfterFy.ObgId = db.GetInt32(reader, 9);
        entities.AfterFy.CspNumber = db.GetString(reader, 10);
        entities.AfterFy.CpaType = db.GetString(reader, 11);
        entities.AfterFy.OtrId = db.GetInt32(reader, 12);
        entities.AfterFy.OtrType = db.GetString(reader, 13);
        entities.AfterFy.OtyId = db.GetInt32(reader, 14);
        entities.AfterFy.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.AfterFy.CreatedTmst = db.GetDateTime(reader, 16);
        entities.AfterFy.Amount = db.GetDecimal(reader, 17);
        entities.AfterFy.ProgramAppliedTo = db.GetString(reader, 18);
        entities.AfterFy.Populated = true;
        CheckValid<Collection>("AppliedToCode", entities.AfterFy.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.AfterFy.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd", entities.AfterFy.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.AfterFy.CpaType);
        CheckValid<Collection>("OtrType", entities.AfterFy.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.AfterFy.ProgramAppliedTo);
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection4()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection4",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection5()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection5",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonSupported()
  {
    entities.Supp.Populated = false;
    entities.Supported.Populated = false;

    return ReadEach("ReadCsePersonSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supported.CspNumber = db.GetString(reader, 1);
        entities.Supported.Type1 = db.GetString(reader, 2);
        entities.Supp.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

        return true;
      });
  }

  private bool ReadDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetDate(
          command, "debAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "debAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetDate(
          command, "date", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 17);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 19);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail2",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "debAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 17);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 19);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "date", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 17);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 19);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadObligationRln()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 7);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 8);
        entities.ObligationRln.Description = db.GetString(reader, 9);
        entities.ObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType", entities.ObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType", entities.ObligationRln.CpaFType);
      });
  }

  private IEnumerable<bool> ReadObligorCsePerson()
  {
    entities.ApCsePerson.Populated = false;
    entities.Obligor.Populated = false;

    return ReadEach("ReadObligorCsePerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "suppPersonNumber1", import.From.SuppPersonNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber2", import.To.SuppPersonNumber ?? "");
        db.SetString(command, "cspNumber", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

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
    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public Ocse157Verification From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public Ocse157Verification To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    private DateWorkArea reportStartDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private DateWorkArea reportEndDate;
    private Common displayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ActivityAfterFyEnd.
    /// </summary>
    [JsonPropertyName("activityAfterFyEnd")]
    public Common ActivityAfterFyEnd
    {
      get => activityAfterFyEnd ??= new();
      set => activityAfterFyEnd = value;
    }

    /// <summary>
    /// A value of PrevAp.
    /// </summary>
    [JsonPropertyName("prevAp")]
    public CsePerson PrevAp
    {
      get => prevAp ??= new();
      set => prevAp = value;
    }

    /// <summary>
    /// A value of EarliestCaseRole.
    /// </summary>
    [JsonPropertyName("earliestCaseRole")]
    public CaseRole EarliestCaseRole
    {
      get => earliestCaseRole ??= new();
      set => earliestCaseRole = value;
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
    /// A value of EarliestCaseAssignment.
    /// </summary>
    [JsonPropertyName("earliestCaseAssignment")]
    public CaseAssignment EarliestCaseAssignment
    {
      get => earliestCaseAssignment ??= new();
      set => earliestCaseAssignment = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ForCreateOcse157Verification.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Verification")]
    public Ocse157Verification ForCreateOcse157Verification
    {
      get => forCreateOcse157Verification ??= new();
      set => forCreateOcse157Verification = value;
    }

    /// <summary>
    /// A value of ForCreateOcse157Data.
    /// </summary>
    [JsonPropertyName("forCreateOcse157Data")]
    public Ocse157Data ForCreateOcse157Data
    {
      get => forCreateOcse157Data ??= new();
      set => forCreateOcse157Data = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Line26Curr.
    /// </summary>
    [JsonPropertyName("line26Curr")]
    public ReportTotals Line26Curr
    {
      get => line26Curr ??= new();
      set => line26Curr = value;
    }

    /// <summary>
    /// A value of Line26Former.
    /// </summary>
    [JsonPropertyName("line26Former")]
    public ReportTotals Line26Former
    {
      get => line26Former ??= new();
      set => line26Former = value;
    }

    /// <summary>
    /// A value of Line26Never.
    /// </summary>
    [JsonPropertyName("line26Never")]
    public ReportTotals Line26Never
    {
      get => line26Never ??= new();
      set => line26Never = value;
    }

    /// <summary>
    /// A value of NullOcse157Verification.
    /// </summary>
    [JsonPropertyName("nullOcse157Verification")]
    public Ocse157Verification NullOcse157Verification
    {
      get => nullOcse157Verification ??= new();
      set => nullOcse157Verification = value;
    }

    /// <summary>
    /// A value of ForVerification.
    /// </summary>
    [JsonPropertyName("forVerification")]
    public Program ForVerification
    {
      get => forVerification ??= new();
      set => forVerification = value;
    }

    /// <summary>
    /// A value of Assistance.
    /// </summary>
    [JsonPropertyName("assistance")]
    public Common Assistance
    {
      get => assistance ??= new();
      set => assistance = value;
    }

    /// <summary>
    /// A value of ArrearsForPers.
    /// </summary>
    [JsonPropertyName("arrearsForPers")]
    public ReportTotals ArrearsForPers
    {
      get => arrearsForPers ??= new();
      set => arrearsForPers = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    private Common activityAfterFyEnd;
    private CsePerson prevAp;
    private CaseRole earliestCaseRole;
    private DateWorkArea nullDateWorkArea;
    private CaseAssignment earliestCaseAssignment;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private CsePerson restart;
    private ReportTotals line26Curr;
    private ReportTotals line26Former;
    private ReportTotals line26Never;
    private Ocse157Verification nullOcse157Verification;
    private Program forVerification;
    private Common assistance;
    private ReportTotals arrearsForPers;
    private Common commitCnt;
    private Ocse157Verification forError;
    private Program program;
    private CsePerson prev;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
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
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ChOrAr.
    /// </summary>
    [JsonPropertyName("chOrAr")]
    public CaseRole ChOrAr
    {
      get => chOrAr ??= new();
      set => chOrAr = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AfterFy.
    /// </summary>
    [JsonPropertyName("afterFy")]
    public Collection AfterFy
    {
      get => afterFy ??= new();
      set => afterFy = value;
    }

    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction debtAdjustment;
    private ObligationRln obligationRln;
    private DebtDetail debtDetail;
    private Collection collection;
    private CsePerson supp;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount supported;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private CsePerson ch;
    private CsePerson apCsePerson;
    private CsePersonAccount obligor;
    private CaseRole chOrAr;
    private CaseRole apCaseRole;
    private Case1 case1;
    private Collection afterFy;
  }
#endregion
}
