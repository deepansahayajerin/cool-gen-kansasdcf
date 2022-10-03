// Program: FN_OCSE157_LINE_28, ID: 371118069, model: 746.
// Short name: SWE02957
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_28.
/// </summary>
[Serializable]
public partial class FnOcse157Line28: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_28 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line28(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line28.
  /// </summary>
  public FnOcse157Line28(IContext context, Import import, Export export):
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
    // --------------------------------------------------------------
    // 08/30/2001
    // Include CSENet collections.
    // --------------------------------------------------------------
    // -------------------------------------------------------------------
    // Deviation from other lines on 157 report.
    // On line 28, include 'both' cases in J/S situation.
    // Also 'include' Concurrent Collections.
    // -------------------------------------------------------------------
    // ----------------------------------------------------------------------
    // 09/14/2010   RMathews  CQ21451
    // If the only activity on a case is an adjustment in the current FY
    // on a collection created in a previous FY, exclude the case.
    // -----------------------------------------------------------------------
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "28";
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "28 "))
    {
      local.RestartCase.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line28.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
    }

    foreach(var item in ReadCase())
    {
      if (Equal(entities.Case1.Number, local.Prev.Number))
      {
        continue;
      }

      local.Prev.Number = entities.Case1.Number;
      local.CountCase.Flag = "N";
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);
      ++local.CommitCnt.Count;

      // ----------------------------------------------------------------------
      // Case must be Open 'during' FY.
      // -----------------------------------------------------------------------
      if (!ReadCaseAssignment())
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.Comment =
            "Skipped-case not open during FY.";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);
        }

        continue;
      }

      // ----------------------------------------------------------------------
      // Read all valid AP/CH and AP/AR combos - active or not.
      // READ EACH property is set to fetch 'distinct' rows to avoid
      // spinning through same AP/CH or AP/AR combo multiple times.
      // Date checks are to ensure we retrieve overlapping roles only.
      // -----------------------------------------------------------------------
      foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
      {
        // -------------------------------------------------------------------
        // Step #1. - Read debts where bal is due on 'run date'. There
        // is a good chance that balance was also due on 9/30. Count
        // case if bal is due on 9/30.
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

          // ---------------------------------------------------------------
          // You are now looking at a debt with balance due on run date
          // and it meets ob type criteria.
          // We now need to 'undo' debt adjustments, collections and
          // collection adjustments to obtain balance due as of FY end.
          // ---------------------------------------------------------------
          local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

          // -----------------------------------------------
          // Process Debt Adjustments first.
          // -----------------------------------------------
          foreach(var item3 in ReadDebtAdjustment())
          {
            // ------------------------------------------------------------------
            // I-type adj increases balance_due. So we need to 'subtract'
            // this amt from balance at run time to get balance due on 9/30.
            // Similarily 'add' D-type adjustments back.
            // ------------------------------------------------------------------
            if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
            {
              local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.Amount;
            }
            else
            {
              local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.Amount;
            }
          }

          // -------------------------------------------------------------
          // Next - process collections and collection adjustments.
          // Read un-adj collections created after FY end.
          // Read adj collections created during or before FY,
          // but adjusted 'after' FY end.
          // Ok to read concurrent collection.
          // --------------------------------------------------------------
          foreach(var item3 in ReadCollection3())
          {
            // --------------------------------------------------------------
            // Subtract un-adj collections. Add adjusted collections.
            // --------------------------------------------------------------
            if (AsChar(entities.Collection.AdjustedInd) == 'N')
            {
              local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
            }
            else
            {
              local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
            }
          }

          // --------------------------------------------------
          // Count case if balance > 0  on FY end.
          // --------------------------------------------------
          if (local.BalDueOnFyEnd.Currency152 > 0)
          {
            local.CountCase.Flag = "Y";

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
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
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.CaseRoleType =
                entities.ChOrArCaseRole.Type1;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
            }

            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Column = "a";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);

            goto ReadEach;
          }
        }

        // -------------------------------------------------------------------
        // We got here because there is either no debt with balance
        // due as of run date or balance is due on run date but
        // nothing was due at the end of FY.
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // Step #2. - Check if Arrears Collection was created 'during' FY.
        // -Skip direct payments through REIP (CRT = 2 or 7)
        // -Include concurrent collections.
        // -Skip collections created and adjusted during FY.
        // -------------------------------------------------------------------
        foreach(var item2 in ReadCollection2())
        {
          if (!ReadObligationType())
          {
            continue;
          }

          // --------------------------
          // Skip Fees, Recoveries, 718B.
          // --------------------------
          if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
            (entities.ObligationType.Classification) == 'R' || Equal
            (entities.ObligationType.Code, "718B"))
          {
            continue;
          }

          // -----------------------------------------
          // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
          // -----------------------------------------
          if (Equal(entities.ObligationType.Code, "MJ") && (
            Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
            (entities.Collection.ProgramAppliedTo, "AFI") || Equal
            (entities.Collection.ProgramAppliedTo, "FC") || Equal
            (entities.Collection.ProgramAppliedTo, "FCI")))
          {
            continue;
          }

          // -------------------------------------------------------------------------
          // 09/14/2010  CQ21451
          // If the only activity on a case is an adjustment in the current FY
          // on a collection created in a previous FY, exclude the case.
          // --------------------------------------------------------------------------
          if (ReadCollection1())
          {
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.CollectionAmount =
                entities.Collection.Amount;
              local.ForCreateOcse157Verification.CollApplToCode =
                entities.Collection.AppliedToCode;
              local.ForCreateOcse157Verification.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreateOcse157Verification.CollectionDte =
                entities.Collection.CollectionDt;
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.CaseRoleType =
                entities.ChOrArCaseRole.Type1;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.Comment =
                "Skipped-Result of Adjusted Coll";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }

          // -----------------------------------------------------------
          // Yipeee! We found an Arrears collection created during FY.
          // -----------------------------------------------------------
          local.CountCase.Flag = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.ObTypeSgi =
              entities.ObligationType.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.ChOrArCsePerson.Number;
            local.ForCreateOcse157Verification.CaseRoleType =
              entities.ChOrArCaseRole.Type1;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              entities.ApCsePerson.Number;
          }

          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.Column = "a";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);

          goto ReadEach;
        }

        // -------------------------------------------------------------------
        // We got here because there is no debt with balance due as of
        // run date and no Arrears Collection is created 'during' FY.
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // Step # 3. - Check for D-type adjustments created during FY.
        // -------------------------------------------------------------------
        foreach(var item2 in ReadDebtObligationObligationTypeDebtDetailDebtAdjustment())
          
        {
          // -----------------------------------------------------------------
          // For Accruing debts, include if adj occurs atleast 1 month after
          // due date. (Remember - accruing debts are not considered
          // 'arrears' until 1 month after due date)
          // For Non-accruing debts, include all D-type adjustments.
          // ----------------------------------------------------------------
          if (AsChar(entities.ObligationType.Classification) == 'A' && !
            Lt(AddMonths(entities.DebtDetail.DueDt, 1),
            entities.DebtAdjustment.DebtAdjustmentDt))
          {
            continue;
          }

          // -------------------------------------------------------------------
          // -Skip 718B
          // -------------------------------------------------------------------
          if (Equal(entities.ObligationType.Code, "718B"))
          {
            continue;
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

          // -------------------------------------------------------------------
          // Yipee! D-type adj found, count case.
          // -------------------------------------------------------------------
          local.CountCase.Flag = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.ObTypeSgi =
              entities.ObligationType.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.DebtDetailDueDt =
              entities.DebtDetail.DueDt;
            local.ForCreateOcse157Verification.DebtDetailBalanceDue =
              entities.DebtDetail.BalanceDueAmt;
            local.ForCreateOcse157Verification.ObTranSgi =
              entities.DebtAdjustment.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.ObligationSgi =
              entities.Obligation.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.ObTranAmount =
              entities.DebtAdjustment.Amount;
            local.ForCreateOcse157Verification.DebtAdjType =
              entities.DebtAdjustment.DebtAdjustmentType;
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.ChOrArCsePerson.Number;
            local.ForCreateOcse157Verification.CaseRoleType =
              entities.ChOrArCaseRole.Type1;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              entities.ApCsePerson.Number;
          }

          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.Column = "a";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);

          goto ReadEach;
        }

        // -------------------------------------------------------------------
        // We got here because
        // No balance is due as of run date  and
        // No arrears collection was created during FY  and
        // No D-type adj is done during FY.
        // Debts could have an outstanding balance as of FY end but
        // zero balance is due on run date. This could happen if
        // 1. Collection is applied after FY end  or
        // 2. D-type debt adj is done after FY end.
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // Step # 4. - Look for debts with 'Zero' bal due but where a
        // Coll is applied to debt after FY end.
        // READ EACH properties are set to fetch distinct rows - so we
        // process each debt only once
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

          // --------------------------------------------------------------
          // DD balance due is 0. In effect, this SET statement resets
          // local view for each parse.
          // --------------------------------------------------------------
          local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

          // ---------------------------------------------------------------
          // You are now looking at a debt with zero balance on run date
          // and it meets ob type criteria. We also know that there is
          // atleast one coll applied to current debt after FY end.
          // We now need to 'undo' debt adjustments, collections and
          // collection adjustments to obtain balance due as of FY end.
          // ---------------------------------------------------------------
          // -----------------------------------------------
          // Process Debt Adjustments first.
          // -----------------------------------------------
          foreach(var item3 in ReadDebtAdjustment())
          {
            // ------------------------------------------------------------------
            // I-type adj increases balance_due. So we need to 'subtract'
            // this amt from balance at run time to get balance due on 9/30.
            // Similarily 'add' D-type adjustments back.
            // ------------------------------------------------------------------
            if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
            {
              local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.Amount;
            }
            else
            {
              local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.Amount;
            }
          }

          // -------------------------------------------------------------
          // Next, process collections and collection adjustments.
          // Read un-adjusted collections created after FY end.
          // Read adj collections created during or before FY,
          // but adjusted 'after' FY end.
          // Include concurrent collections.
          // -------------------------------------------------------------
          foreach(var item3 in ReadCollection3())
          {
            // -----------------------------------------------------------
            // Subtract collections. Add collection adjustments.
            // -----------------------------------------------------------
            if (AsChar(entities.Collection.AdjustedInd) == 'N')
            {
              local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
            }
            else
            {
              local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
            }
          }

          // -----------------------------------------------
          // Count case if balance was due on 9/30
          // -----------------------------------------------
          if (local.BalDueOnFyEnd.Currency152 > 0)
          {
            local.CountCase.Flag = "Y";

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
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
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.CaseRoleType =
                entities.ChOrArCaseRole.Type1;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
            }

            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Column = "a";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);

            goto ReadEach;
          }
        }

        // -------------------------------------------------------------------
        // Step # 5. - Look for debts with 'Zero' bal due but where a
        // D-type adjustment is made to debt after FY end.
        // READ EACH properties are set to fetch distinct rows - so we
        // process each debt only once
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

          // --------------------------------------------------------------
          // DD balance due is 0. In effect, this SET statement resets
          // local view for each parse.
          // --------------------------------------------------------------
          local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

          // ---------------------------------------------------------------
          // You are now looking at a debt with zero balance on run date
          // and it meets ob type criteria. We also know that there is
          // atleast one D-type adjustment to debt 'after' FY end.
          // We now need to 'undo' debt adjustments, collections and
          // collection adjustments to obtain balance due as of FY end.
          // ---------------------------------------------------------------
          // -----------------------------------------------
          // Process Debt Adjustments first.
          // -----------------------------------------------
          foreach(var item3 in ReadDebtAdjustment())
          {
            // ------------------------------------------------------------------
            // I-type adj increases balance_due. So we need to 'subtract'
            // this amt from balance at run time to get balance due on 9/30.
            // Similarily 'add' D-type adjustments back.
            // ------------------------------------------------------------------
            if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
            {
              local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.Amount;
            }
            else
            {
              local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.Amount;
            }
          }

          // -------------------------------------------------------------
          // Next, process collections and collection adjustments.
          // Read un-adjusted collections created after FY end.
          // Read adj collections created during or before FY,
          // but adjusted 'after' FY end.
          // Include concurrent collections.
          // -------------------------------------------------------------
          foreach(var item3 in ReadCollection3())
          {
            // -----------------------------------------------------------
            // Subtract collections. Add collection adjustments.
            // -----------------------------------------------------------
            if (AsChar(entities.Collection.AdjustedInd) == 'N')
            {
              local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
            }
            else
            {
              local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
            }
          }

          // -----------------------------------------------
          // Count case if balance was due on 9/30
          // -----------------------------------------------
          if (local.BalDueOnFyEnd.Currency152 > 0)
          {
            local.CountCase.Flag = "Y";

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
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
                entities.ChOrArCsePerson.Number;
              local.ForCreateOcse157Verification.CaseRoleType =
                entities.ChOrArCaseRole.Type1;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
            }

            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Column = "a";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);

            goto ReadEach;
          }
        }

        // -----------------------------------------------
        // End of AP/CH READ EACH.
        // -----------------------------------------------
      }

ReadEach:

      if (AsChar(local.CountCase.Flag) == 'N')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.Comment =
            "Skipped-case does not meet criteria.";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);
        }

        continue;
      }

      // -----------------------------------------------------
      // Increment Counter  and  take checkpoint, if necessary.
      // -----------------------------------------------------
      ++local.Line28.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "28 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line28.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "28";
          local.ForError.CaseNumber = entities.Case1.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

      // -----------------------------------------------
      // End of driving READ EACH.
      // -----------------------------------------------
    }

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "28";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line28.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "29 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "28";
      local.ForError.CaseNumber = "";
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
    target.ObTranType = source.ObTranType;
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
    target.CaseRoleType = source.CaseRoleType;
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
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranType = source.ObTranType;
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
    target.CaseRoleType = source.CaseRoleType;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
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

    useImport.SupportedPerson.Number = entities.ChOrArCsePerson.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    useImport.Ocse157Verification.Assign(local.ForError);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.RestartCase.Number);
        db.SetNullableString(
          command, "caseNumber1", import.From.CaseNumber ?? "");
        db.
          SetNullableString(command, "caseNumber2", import.To.CaseNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 2);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.ReportStartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChOrArCaseRole.Populated = false;
    entities.ChOrArCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ChOrArCsePerson.Number = db.GetString(reader, 8);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 9);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChOrArCaseRole.Populated = true;
        entities.ChOrArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Adjusted.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Adjusted.CrtType = db.GetInt32(reader, 2);
        entities.Adjusted.CstId = db.GetInt32(reader, 3);
        entities.Adjusted.CrvId = db.GetInt32(reader, 4);
        entities.Adjusted.CrdId = db.GetInt32(reader, 5);
        entities.Adjusted.ObgId = db.GetInt32(reader, 6);
        entities.Adjusted.CspNumber = db.GetString(reader, 7);
        entities.Adjusted.CpaType = db.GetString(reader, 8);
        entities.Adjusted.OtrId = db.GetInt32(reader, 9);
        entities.Adjusted.OtrType = db.GetString(reader, 10);
        entities.Adjusted.OtyId = db.GetInt32(reader, 11);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Adjusted.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Adjusted.CpaType);
        CheckValid<Collection>("OtrType", entities.Adjusted.OtrType);
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
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
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
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

  private IEnumerable<bool> ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
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
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "debAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
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
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail2()
  {
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
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
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail3()
  {
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
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
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDebtObligationObligationTypeDebtDetailDebtAdjustment()
  {
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtObligationObligationTypeDebtDetailDebtAdjustment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
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
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 19);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 20);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 21);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 22);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 23);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 24);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 25);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 26);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 27);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 28);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 29);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 30);
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Collection.OtyId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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

    private DateWorkArea reportStartDate;
    private Common displayInd;
    private DateWorkArea reportEndDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Line28.
    /// </summary>
    [JsonPropertyName("line28")]
    public Common Line28
    {
      get => line28 ??= new();
      set => line28 = value;
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
    /// A value of BalDueOnFyEnd.
    /// </summary>
    [JsonPropertyName("balDueOnFyEnd")]
    public ReportTotals BalDueOnFyEnd
    {
      get => balDueOnFyEnd ??= new();
      set => balDueOnFyEnd = value;
    }

    /// <summary>
    /// A value of CountCase.
    /// </summary>
    [JsonPropertyName("countCase")]
    public Common CountCase
    {
      get => countCase ??= new();
      set => countCase = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
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

    private Case1 prev;
    private Ocse157Verification null1;
    private Ocse157Verification forCreateOcse157Verification;
    private Case1 restartCase;
    private Ocse157Data forCreateOcse157Data;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common line28;
    private Program program;
    private ReportTotals balDueOnFyEnd;
    private Common countCase;
    private CsePerson restartCsePerson;
    private Common commitCnt;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public Collection Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ChOrArCaseRole.
    /// </summary>
    [JsonPropertyName("chOrArCaseRole")]
    public CaseRole ChOrArCaseRole
    {
      get => chOrArCaseRole ??= new();
      set => chOrArCaseRole = value;
    }

    /// <summary>
    /// A value of ChOrArCsePerson.
    /// </summary>
    [JsonPropertyName("chOrArCsePerson")]
    public CsePerson ChOrArCsePerson
    {
      get => chOrArCsePerson ??= new();
      set => chOrArCsePerson = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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

    private Collection adjusted;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole chOrArCaseRole;
    private CsePerson chOrArCsePerson;
    private Collection collection;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePersonAccount supported;
    private DebtDetail debtDetail;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRln obligationTransactionRln;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
