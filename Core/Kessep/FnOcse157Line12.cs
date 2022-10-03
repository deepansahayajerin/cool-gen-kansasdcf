// Program: FN_OCSE157_LINE_12, ID: 371092723, model: 746.
// Short name: SWE02918
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_12.
/// </summary>
[Serializable]
public partial class FnOcse157Line12: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_12 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line12(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line12.
  /// </summary>
  public FnOcse157Line12(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------
    // Initial Version - 7/2001
    // -------------------------------------
    // -----------------------------------------------------------------------
    // 7/21/2001
    // Virtual re-write of this CAB.  A Case with multiple children will
    // now be counted in line 12, even if one child requires an order
    // to be established and all other children already have orders established.
    // 7/24/2001
    // Check for active program or no program ever on 'each' CHild.
    // -----------------------------------------------------------------------
    // ----------------------------------------------------------
    // 7/29/2001
    // Include MJ NA in line 12.
    // ----------------------------------------------------------
    // ----------------------------------------------------------
    // 7/30/2001
    // Clear local_for_create view each time.
    // ----------------------------------------------------------
    // ----------------------------------------------------------
    // 8/7/2001
    // Delete attribute debt_detail preconversion_program_state.
    // ----------------------------------------------------------
    // ----------------------------------------------------------
    // 10/29/2001 - PR#s 127343, 127345.
    // - For non-fin ldets, LOPS is optional. Read LROL instead.
    // - Don't count if cash order is established, even if AR waived
    //   insurance flag = "N".
    // ----------------------------------------------------------
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "12 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line12Curr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line12Former.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
      local.Line12Never.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 34, 10));
    }

    foreach(var item in ReadCase())
    {
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);

      // ----------------------------------------------
      // Was this Case reported in line 1? If not, skip.
      // ----------------------------------------------
      ReadOcse157Data();
      ReadOcse157Verification();

      if (IsEmpty(local.Minimum.Number))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "12";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-case not included in line 1.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      local.CountCase.Flag = "N";
      local.ActiveApFound.Flag = "N";

      if (ReadCaseRole())
      {
        local.ActiveApFound.Flag = "Y";
      }

      // ----------------------------------------------------
      // If AP is not active check if PEPR is open for any CH on the
      // case as of FY end.
      // ----------------------------------------------------
      if (AsChar(local.ActiveApFound.Flag) == 'N')
      {
        if (ReadPersonProgram1())
        {
          // ----------------------------------------------------
          // AP is not active but PEPR is open, Count Case.
          // ----------------------------------------------------
          local.CountCase.Flag = "Y";
        }
        else
        {
          // ----------------------------------------------------
          // AP is not active and PEPR is not open, Skip Case. .
          // ----------------------------------------------------
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.LineNumber = "12";
            local.ForCreateOcse157Verification.Comment =
              "Skipped-PEPR not active. AP not active.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }
      }

      // ----------------------------------------------------------------------
      // If AP is not active, then we would have either counted this
      // case or skipped it. No further processing is necessary.
      // -----------------------------------------------------------------------
      if (AsChar(local.ActiveApFound.Flag) == 'Y')
      {
        // ----------------------------------------------------------------------
        // Read all AP/CH combos for current case - active as of the end of FY.
        // -----------------------------------------------------------------------
        foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
        {
          // ----------------------------------------------------------------------
          // CH must have active PEPR as of FY end or no PEPR ever.
          // -----------------------------------------------------------------------
          if (!ReadPersonProgram2())
          {
            if (ReadPersonProgram3())
            {
              if (AsChar(import.DisplayInd.Flag) == 'Y')
              {
                local.ForCreateOcse157Verification.CaseNumber =
                  entities.Case1.Number;
                local.ForCreateOcse157Verification.SuppPersonNumber =
                  entities.ChCsePerson.Number;
                local.ForCreateOcse157Verification.ObligorPersonNbr =
                  entities.ApCsePerson.Number;
                local.ForCreateOcse157Verification.LineNumber = "12";
                local.ForCreateOcse157Verification.Comment =
                  "Skipped-PEPR not active. AP active.";
                UseFnCreateOcse157Verification();
                MoveOcse157Verification2(local.Null1,
                  local.ForCreateOcse157Verification);
              }

              continue;
            }
            else
            {
              // ----------------------------------------------------
              // AP is active but PEPR was 'never' setup. This is acceptable!
              // This may happen when CSE has just received the referral
              // and is still in the process of setting up the case.
              // ----------------------------------------------------
            }
          }

          local.MedLdet.Flag = "N";
          local.FinLdet.Flag = "N";

          // ----------------------------------------------------------------------
          // Read J class LDETs using AP and CH.
          // Skip LAs and LDETs created after the end of FY.
          // Include LAs and LDETs from previous FYs.
          // Read Non-fin LDET first, then Fin LDET.
          // Non-fin LDET end-date must be >= FY end.
          // Fin LDET must have an outstanding balance owed or
          // accrual instructions must be open.
          // -----------------------------------------------------------------------
          if (ReadLegalActionDetailLegalAction1())
          {
            local.MedLdet.Flag = "Y";
          }

          foreach(var item2 in ReadLegalActionDetailLegalAction2())
          {
            // -------------------------------------------------------------------
            // If HIC or UM is not active and AR has not waived insurance,
            // then search for MS, MJ or MC.
            // --------------------------------------------------------------------
            if (AsChar(local.MedLdet.Flag) == 'N')
            {
              foreach(var item3 in ReadObligationTypeObligation2())
              {
                // -------------------------------------------------------------------
                // We found a fin LDET with desired Ob types.
                // Now check if Accrual Instructions are open for Current 
                // Obligation.
                // Qualify Read by supported person.
                // --------------------------------------------------------------------
                if (AsChar(entities.ObligationType.Classification) == 'A')
                {
                  if (ReadAccrualInstructions())
                  {
                    if (AsChar(import.DisplayInd.Flag) == 'Y')
                    {
                      local.ForCreateOcse157Verification.SuppPersonNumber =
                        entities.ChCsePerson.Number;
                      local.ForCreateOcse157Verification.ObligorPersonNbr =
                        entities.ApCsePerson.Number;
                      local.ForCreateOcse157Verification.CourtOrderNumber =
                        entities.LegalAction.StandardNumber;
                      local.ForCreateOcse157Verification.ObTypeSgi =
                        entities.ObligationType.SystemGeneratedIdentifier;
                    }

                    // ----------------------------------------------------------------------
                    // We found an active med LDET for this AP/CH.
                    // If fin ldet is also active or PA med service = MO,
                    // Then no further processing is necessary.
                    // Else keep digging for fin LDET.
                    // ----------------------------------------------------------------------
                    local.MedLdet.Flag = "Y";

                    if (AsChar(local.FinLdet.Flag) == 'Y')
                    {
                      goto ReadEach2;
                    }

                    goto Test1;
                  }
                }

                // -------------------------------------------------------------------
                // We got here because Accrual Instructions are not open for
                // current Obligation.
                // --------------------------------------------------------------------
                // ----------------------------------------------
                // Qualify Debts by Supp person. 7/18/01
                // Only read debts created before FY end.
                // Debt detail due-dt must be <= FY end.
                // ----------------------------------------------
                foreach(var item4 in ReadDebtDebtDetail())
                {
                  // -------------------------------------------------------------------
                  // Now check for an Outstanding Balance as of report run date.
                  // --------------------------------------------------------------------
                  if (entities.DebtDetail.BalanceDueAmt > 0)
                  {
                    // -----------------------------------------------
                    // Skip MJ AF/FC.
                    // -----------------------------------------------
                    if (Equal(entities.ObligationType.Code, "MJ"))
                    {
                      // -----------------------------------------------
                      // CAB defaults Coll date to Current date. So don't pass 
                      // anything.
                      // -----------------------------------------------
                      UseFnDeterminePgmForDebtDetail();

                      if (Equal(local.Program.Code, "AF") || Equal
                        (local.Program.Code, "AFI") || Equal
                        (local.Program.Code, "FC") || Equal
                        (local.Program.Code, "FCI"))
                      {
                        // -----------------------------------------------
                        // Skip this debt detail.
                        // -----------------------------------------------
                        continue;
                      }
                    }

                    if (AsChar(import.DisplayInd.Flag) == 'Y')
                    {
                      local.ForCreateOcse157Verification.SuppPersonNumber =
                        entities.ChCsePerson.Number;
                      local.ForCreateOcse157Verification.ObligorPersonNbr =
                        entities.ApCsePerson.Number;
                      local.ForCreateOcse157Verification.CourtOrderNumber =
                        entities.LegalAction.StandardNumber;
                      local.ForCreateOcse157Verification.ObTypeSgi =
                        entities.ObligationType.SystemGeneratedIdentifier;
                      local.ForCreateOcse157Verification.DebtDetailDueDt =
                        entities.DebtDetail.DueDt;
                      local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                        entities.DebtDetail.BalanceDueAmt;
                    }

                    // ----------------------------------------------------------------------
                    // We found an active med LDET for this AP/CH.
                    // If fin ldet is also active or PA med service = MO,
                    // Then no further processing is necessary.
                    // Else keep digging for fin LDET.
                    // ----------------------------------------------------------------------
                    local.MedLdet.Flag = "Y";

                    if (AsChar(local.FinLdet.Flag) == 'Y')
                    {
                      goto ReadEach2;
                    }

                    goto Test1;
                  }

                  // -------------------------------------------------------------------
                  // We got here because there is no Outstanding Balance on
                  // current Debt as of report run date.
                  // Now check if any Collections were applied to current debt 
                  // since FY end.
                  // Skip Adjusted Collections.
                  // Skip Concurrent Collections.
                  // No need to check if Coll date < FY end date since we are
                  // only reading for Debts created and due before the end of 
                  // FY.
                  // Confirmed on 7/20/2001.
                  // --------------------------------------------------------------------
                  foreach(var item5 in ReadCollection())
                  {
                    // -------------------------------------------------------------------
                    // Skip MJ AF/FC.
                    // --------------------------------------------------------------------
                    if (Equal(entities.ObligationType.Code, "MJ") && (
                      Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
                      (entities.Collection.ProgramAppliedTo, "AFI") || Equal
                      (entities.Collection.ProgramAppliedTo, "FC") || Equal
                      (entities.Collection.ProgramAppliedTo, "FCI")))
                    {
                      continue;
                    }

                    if (AsChar(import.DisplayInd.Flag) == 'Y')
                    {
                      local.ForCreateOcse157Verification.SuppPersonNumber =
                        entities.ChCsePerson.Number;
                      local.ForCreateOcse157Verification.ObligorPersonNbr =
                        entities.ApCsePerson.Number;
                      local.ForCreateOcse157Verification.CourtOrderNumber =
                        entities.LegalAction.StandardNumber;
                      local.ForCreateOcse157Verification.ObTypeSgi =
                        entities.ObligationType.SystemGeneratedIdentifier;
                      local.ForCreateOcse157Verification.CollectionDte =
                        entities.Collection.CollectionDt;
                      local.ForCreateOcse157Verification.CollCreatedDte =
                        Date(entities.Collection.CreatedTmst);
                      local.ForCreateOcse157Verification.CollectionAmount =
                        entities.Collection.Amount;
                    }

                    // ----------------------------------------------------------------------
                    // We found an active med LDET for this AP/CH.
                    // If fin ldet is also active or PA med service = MO,
                    // Then no further processing is necessary.
                    // Else keep digging for fin LDET.
                    // ----------------------------------------------------------------------
                    local.MedLdet.Flag = "Y";

                    if (AsChar(local.FinLdet.Flag) == 'Y')
                    {
                      goto ReadEach2;
                    }

                    goto Test1;
                  }
                }
              }
            }

Test1:

            // ----------------------------------------------------------------------
            // Search for cash ob types if PA Med Service Ind <> 'MO'.
            // ----------------------------------------------------------------------
            if (AsChar(local.FinLdet.Flag) == 'Y')
            {
              break;
            }

            foreach(var item3 in ReadObligationTypeObligation1())
            {
              // -------------------------------------------------------------------
              // We found a finance LDET with desired Ob types.
              // Now check if Accrual Instructions are open for Current 
              // Obligation.
              // Qualify by supported person.
              // --------------------------------------------------------------------
              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (ReadAccrualInstructions())
                {
                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.ChCsePerson.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.CourtOrderNumber =
                      entities.LegalAction.StandardNumber;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                  }

                  // ----------------------------------------------------------------------
                  // We found an active fin LDET for this AP/CH.
                  // If med ldet is active  or  AR has waived insurance,
                  // no further processing is necessary for AP/CH.
                  // Else keep digging for med LDET.
                  // ----------------------------------------------------------------------
                  local.FinLdet.Flag = "Y";

                  if (AsChar(local.MedLdet.Flag) == 'Y')
                  {
                    goto ReadEach2;
                  }

                  break;
                }
              }

              // -------------------------------------------------------------------
              // We got here because Accrual Instructions are not open for
              // current Obligation.
              // --------------------------------------------------------------------
              // ----------------------------------------------
              // Qualify Debts by Supp person. 7/18/01
              // Only read debts created before FY end.
              // Debt detail due-dt must be <= FY end.
              // ----------------------------------------------
              foreach(var item4 in ReadDebtDebtDetail())
              {
                // -------------------------------------------------------------------
                // Now check for an Outstanding Balance as of report run date.
                // -------------------------------------------------------------------
                if (entities.DebtDetail.BalanceDueAmt > 0)
                {
                  // -----------------------------------------------
                  // Skip MJ AF/FC.
                  // -----------------------------------------------
                  if (Equal(entities.ObligationType.Code, "MJ"))
                  {
                    // -----------------------------------------------
                    // CAB defaults Coll date to Current date. So don't pass 
                    // anything.
                    // -----------------------------------------------
                    UseFnDeterminePgmForDebtDetail();

                    if (Equal(local.Program.Code, "AF") || Equal
                      (local.Program.Code, "AFI") || Equal
                      (local.Program.Code, "FC") || Equal
                      (local.Program.Code, "FCI"))
                    {
                      // -----------------------------------------------
                      // Skip this debt detail.
                      // -----------------------------------------------
                      continue;
                    }
                  }

                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.ChCsePerson.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.CourtOrderNumber =
                      entities.LegalAction.StandardNumber;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.DebtDetailDueDt =
                      entities.DebtDetail.DueDt;
                    local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                      entities.DebtDetail.BalanceDueAmt;
                  }

                  // ----------------------------------------------------------------------
                  // We found an active fin LDET for this AP/CH.
                  // If med ldet is active  or  AR has waived insurance,
                  // no further processing is necessary for AP/CH.
                  // Else keep digging for med LDET.
                  // ----------------------------------------------------------------------
                  local.FinLdet.Flag = "Y";

                  if (AsChar(local.MedLdet.Flag) == 'Y')
                  {
                    goto ReadEach2;
                  }

                  goto ReadEach1;
                }

                // -------------------------------------------------------------------
                // We got here because there is no Outstanding Balance as of
                // report run date.
                // Now check if any Collections were applied to current debt 
                // since FY end.
                // Skip Adjusted Collections.
                // Skip Concurrent Collections.
                // No need to check if Coll date < FY end date since we are
                // only reading for Debts created and due before the end of FY.
                // Confirmed on 7/20/2001.
                // --------------------------------------------------------------------
                foreach(var item5 in ReadCollection())
                {
                  // -------------------------------------------------------------------
                  // Skip MJ AF/FC.
                  // --------------------------------------------------------------------
                  if (Equal(entities.ObligationType.Code, "MJ") && (
                    Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
                    (entities.Collection.ProgramAppliedTo, "AFI") || Equal
                    (entities.Collection.ProgramAppliedTo, "FC") || Equal
                    (entities.Collection.ProgramAppliedTo, "FCI")))
                  {
                    continue;
                  }

                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.ChCsePerson.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.CourtOrderNumber =
                      entities.LegalAction.StandardNumber;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.CollectionDte =
                      entities.Collection.CollectionDt;
                    local.ForCreateOcse157Verification.CollCreatedDte =
                      Date(entities.Collection.CreatedTmst);
                    local.ForCreateOcse157Verification.CollectionAmount =
                      entities.Collection.Amount;
                  }

                  // ----------------------------------------------------------------------
                  // We found an active fin LDET for this AP/CH.
                  // If med ldet is active  or  AR has waived insurance,
                  // no further processing is necessary for AP/CH.
                  // Else keep digging for med LDET.
                  // ----------------------------------------------------------------------
                  local.FinLdet.Flag = "Y";

                  if (AsChar(local.MedLdet.Flag) == 'Y')
                  {
                    goto ReadEach2;
                  }

                  goto ReadEach1;
                }
              }
            }

ReadEach1:
            ;
          }

ReadEach2:

          // ----------------------------------------------------------------------
          // If finance LDET is active, skip AP/CH.
          // -----------------------------------------------------------------------
          if (AsChar(local.FinLdet.Flag) == 'Y')
          {
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.LineNumber = "12";
              local.ForCreateOcse157Verification.Comment =
                "Skipped- Finance LDET is active.";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }

          if (AsChar(local.FinLdet.Flag) == 'N' && AsChar
            (local.MedLdet.Flag) == 'N')
          {
            local.CountCase.Flag = "Y";

            goto Test2;
          }

          if (AsChar(local.FinLdet.Flag) == 'N' && AsChar
            (local.MedLdet.Flag) == 'Y')
          {
            // ----------------------------------------------------------------------
            // Include case if PA med service <> MO. (treat spaces as MC)
            // Else Skip Case.
            // -----------------------------------------------------------------------
            if (!Equal(entities.Case1.PaMedicalService, "MO"))
            {
              local.CountCase.Flag = "Y";

              goto Test2;
            }

            // ----------------------------------------------------------------------
            // Med LDET is active.
            // Fin LDET is not active.
            // PA_med_service = 'MO' (Medical Only).
            // Hence, we do not need to establish any fin LDET for this AP/CH. 
            // Skip it.
            // -----------------------------------------------------------------------
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.LineNumber = "12";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-PA Med=MO. Med LDET found.";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }
        }
      }

Test2:

      if (AsChar(local.CountCase.Flag) == 'N')
      {
        continue;
      }

      // -----------------------------------------------------------
      // All conditions are satisifed. Count Case.
      // -----------------------------------------------------------
      UseFn157GetAssistanceForCase();

      // ------------------------------------------
      // No exit state is set in this CAB.
      // ------------------------------------------
      switch(AsChar(local.AssistanceProgram.Flag))
      {
        case 'C':
          ++local.Line12Curr.Count;
          local.ForCreateOcse157Verification.Column = "b";

          break;
        case 'F':
          ++local.Line12Former.Count;
          local.ForCreateOcse157Verification.Column = "c";

          break;
        default:
          ++local.Line12Never.Count;
          local.ForCreateOcse157Verification.Column = "d";

          break;
      }

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.LineNumber = "12";
        local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
        local.ForCreateOcse157Verification.PersonProgCode =
          local.ForVerification.Code;
        UseFnCreateOcse157Verification();
      }

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "12 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line12Curr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line12Former.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line12Never.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "12";
          local.ForError.CaseNumber = entities.Case1.Number;
          local.ForError.SuppPersonNumber = entities.ChCsePerson.Number;
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "12";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line12Curr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line12Former.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line12Never.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "14 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "12";
      local.ForError.CaseNumber = "";
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
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
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
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private void UseFn157GetAssistanceForCase()
  {
    var useImport = new Fn157GetAssistanceForCase.Import();
    var useExport = new Fn157GetAssistanceForCase.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.Case1.Assign(entities.Case1);

    Call(Fn157GetAssistanceForCase.Execute, useImport, useExport);

    local.ForVerification.Code = useExport.Program.Code;
    local.SuppPersForVerification.Number = useExport.CsePerson.Number;
    local.AssistanceProgram.Flag = useExport.AssistanceProgram.Flag;
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

    useImport.SupportedPerson.Number = entities.ChCsePerson.Number;
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);

    MoveObligationType(entities.ObligationType, useImport.ObligationType);

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

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetDate(
          command, "asOfDt", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Restart.Number);
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

  private bool ReadCaseRole()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        entities.ApCaseRole.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChCaseRole.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
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
        entities.ChCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ChCsePerson.Number = db.GetString(reader, 8);
        entities.ChCaseRole.Type1 = db.GetString(reader, 9);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.ChCaseRole.ArWaivedInsurance =
          db.GetNullableString(reader, 13);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 14);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;
        entities.ChCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
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
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 12);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 13);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionDetailLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetailLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier1", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApCaseRole.CasNumber);
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
        db.SetDate(
          command, "effectiveDt",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction2()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNumber1", entities.ApCsePerson.Number);
          
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.
          SetNullableString(command, "cspNumber2", entities.ChCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationTypeObligation1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationTypeObligation2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 3);
        entities.Obligation.CspNumber = db.GetString(reader, 4);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;

        return true;
      });
  }

  private bool ReadOcse157Data()
  {
    local.Max.Populated = false;

    return Read("ReadOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          import.Ocse157Verification.FiscalYear.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.Max.RunNumber = db.GetNullableInt32(reader, 0);
        local.Max.Populated = true;
      });
  }

  private bool ReadOcse157Verification()
  {
    local.Minimum.Populated = false;

    return Read("ReadOcse157Verification",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear",
          import.Ocse157Verification.FiscalYear.GetValueOrDefault());
        db.SetNullableInt32(
          command, "runNumber", local.Max.RunNumber.GetValueOrDefault());
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        local.Minimum.Number = db.GetString(reader, 0);
        local.Minimum.Populated = true;
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram3()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
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
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
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

    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common displayInd;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of FinLdet.
    /// </summary>
    [JsonPropertyName("finLdet")]
    public Common FinLdet
    {
      get => finLdet ??= new();
      set => finLdet = value;
    }

    /// <summary>
    /// A value of MedLdet.
    /// </summary>
    [JsonPropertyName("medLdet")]
    public Common MedLdet
    {
      get => medLdet ??= new();
      set => medLdet = value;
    }

    /// <summary>
    /// A value of ArWaivedInsurance.
    /// </summary>
    [JsonPropertyName("arWaivedInsurance")]
    public Common ArWaivedInsurance
    {
      get => arWaivedInsurance ??= new();
      set => arWaivedInsurance = value;
    }

    /// <summary>
    /// A value of ActiveApFound.
    /// </summary>
    [JsonPropertyName("activeApFound")]
    public Common ActiveApFound
    {
      get => activeApFound ??= new();
      set => activeApFound = value;
    }

    /// <summary>
    /// A value of NbrOfAp.
    /// </summary>
    [JsonPropertyName("nbrOfAp")]
    public Common NbrOfAp
    {
      get => nbrOfAp ??= new();
      set => nbrOfAp = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Line12Curr.
    /// </summary>
    [JsonPropertyName("line12Curr")]
    public Common Line12Curr
    {
      get => line12Curr ??= new();
      set => line12Curr = value;
    }

    /// <summary>
    /// A value of Line12Former.
    /// </summary>
    [JsonPropertyName("line12Former")]
    public Common Line12Former
    {
      get => line12Former ??= new();
      set => line12Former = value;
    }

    /// <summary>
    /// A value of Line12Never.
    /// </summary>
    [JsonPropertyName("line12Never")]
    public Common Line12Never
    {
      get => line12Never ??= new();
      set => line12Never = value;
    }

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
    /// A value of Minimum.
    /// </summary>
    [JsonPropertyName("minimum")]
    public Case1 Minimum
    {
      get => minimum ??= new();
      set => minimum = value;
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
    /// A value of ForVerification.
    /// </summary>
    [JsonPropertyName("forVerification")]
    public Program ForVerification
    {
      get => forVerification ??= new();
      set => forVerification = value;
    }

    /// <summary>
    /// A value of SuppPersForVerification.
    /// </summary>
    [JsonPropertyName("suppPersForVerification")]
    public CsePerson SuppPersForVerification
    {
      get => suppPersForVerification ??= new();
      set => suppPersForVerification = value;
    }

    /// <summary>
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
    }

    /// <summary>
    /// A value of GetMedicaidOnlyProgram.
    /// </summary>
    [JsonPropertyName("getMedicaidOnlyProgram")]
    public Common GetMedicaidOnlyProgram
    {
      get => getMedicaidOnlyProgram ??= new();
      set => getMedicaidOnlyProgram = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Ocse157Data Max
    {
      get => max ??= new();
      set => max = value;
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

    private Program program;
    private Common finLdet;
    private Common medLdet;
    private Common arWaivedInsurance;
    private Common activeApFound;
    private Common nbrOfAp;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restart;
    private Common line12Curr;
    private Common line12Former;
    private Common line12Never;
    private Case1 prev;
    private Ocse157Verification null1;
    private Case1 minimum;
    private Common countCase;
    private Program forVerification;
    private CsePerson suppPersForVerification;
    private Common assistanceProgram;
    private Common getMedicaidOnlyProgram;
    private Common commitCnt;
    private Ocse157Data max;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public LegalActionPerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public LegalActionPerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of SuppPerson.
    /// </summary>
    [JsonPropertyName("suppPerson")]
    public CaseRole SuppPerson
    {
      get => suppPerson ??= new();
      set => suppPerson = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
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
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private LegalActionPerson supp;
    private LegalActionPerson obligor;
    private CaseRole suppPerson;
    private PersonProgram personProgram;
    private Case1 case1;
    private Ocse157Verification ocse157Verification;
    private CaseRole apCaseRole;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CsePerson apCsePerson;
    private LegalActionCaseRole chLegalActionCaseRole;
    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
    private LegalActionDetail legalActionDetail;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private AccrualInstructions accrualInstructions;
    private Ocse157Data ocse157Data;
    private Collection collection;
    private CsePersonAccount supported;
  }
#endregion
}
