// Program: FN_OCSE157_LINE_17, ID: 371092715, model: 746.
// Short name: SWE02921
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_17.
/// </summary>
[Serializable]
public partial class FnOcse157Line17: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_17 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line17(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line17.
  /// </summary>
  public FnOcse157Line17(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    // 07/31/2001
    // Significant changes to cater for business rule changes.
    // -----------------------------------------------------------
    // -----------------------------------------------------------
    // 08/02/2001
    // Fix ESCAPE statement in the prev legal detail logic.
    // Set correct case asin end date on verification record.
    // -----------------------------------------------------------
    // ----------------------------------------------------------
    // 8/7/2001
    // Delete attribute debt_detail preconversion_program_state.
    // ----------------------------------------------------------
    // ----------------------------------------------------------
    // 11/6/2001
    // Significant changes to read LROL instead of LOPS for
    // non-fin ldets.
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "17 "))
    {
      local.RestartCase.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line17Curr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 13, 10));
      local.Line17Former.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 23, 10));
      local.Line17Never.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 33, 10));
    }

    foreach(var item in ReadCase())
    {
      MoveOcse157Verification2(local.Null1, local.ForCreateOcse157Verification);

      // -----------------------------------------------------------
      // Case Assignment must be open at some point during the FY.
      // -----------------------------------------------------------
      if (!ReadCaseAssignment())
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "17";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-Case not Open during FY.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      local.CountCase.Flag = "N";

      // ----------------------------------------------------------------------
      // For current case, read all valid AP/CH combos  - active at
      // some point during the FY.
      // ----------------------------------------------------------------------
      foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
      {
        // --------------------------------------------------
        // Read  'non-fin'  ldets where
        // LDET is created within FY.
        // Legal Action Classification = J
        // LA Established_by is either CS, CT or OS.
        // --------------------------------------------------
        foreach(var item2 in ReadLegalActionDetailLegalAction3())
        {
          // ----------------------------------------------------------------------
          // Ok, we found a  non-fin  ldet. Now check if there is another
          // ldet(fin or non) created in prev FY on the same court order.
          // ----------------------------------------------------------------------
          foreach(var item3 in ReadLegalActionDetailLegalAction1())
          {
            switch(AsChar(entities.PrevLegalActionDetail.DetailType))
            {
              case 'N':
                // ----------------------------------------------------------------------
                // For non-fin LDETs read HIC and UM ob types only.
                // ----------------------------------------------------------------------
                if (!Equal(entities.PrevLegalActionDetail.NonFinOblgType, "HIC") &&
                  !Equal(entities.PrevLegalActionDetail.NonFinOblgType, "UM"))
                {
                  continue;
                }

                // ----------------------------------------------------------------------
                // We found a non-fin ldet for this court order and it is 
                // created
                // in prev FY.
                // ----------------------------------------------------------------------
                // ----------------------------------------------------------------------
                // Don't skip yet!
                // We need to check if current AP and CH were 'tied' to this
                // prev ldet using LROL.
                // ----------------------------------------------------------------------
                if (!ReadLegalActionCaseRole1())
                {
                  // -------------------------------------------------------------
                  // This prev ldet must have been established for another AP.
                  // -------------------------------------------------------------
                  continue;
                }

                if (!ReadLegalActionCaseRole2())
                {
                  // -------------------------------------------------------------
                  // This prev ldet must have been established for another CH.
                  // -------------------------------------------------------------
                  continue;
                }

                break;
              case 'F':
                // ----------------------------------------------------------------------
                // We are only interested in specific ob types.
                // ----------------------------------------------------------------------
                foreach(var item4 in ReadObligationTypeObligation2())
                {
                  if (Equal(entities.ObligationType.Code, "MJ"))
                  {
                    // ----------------------------------------------
                    // Qualify Debts by Supp person.
                    // Only read debts created before FY end.
                    // Debt detail due-dt must be <= FY end.
                    // ----------------------------------------------
                    foreach(var item5 in ReadDebtDebtDetail())
                    {
                      // -----------------------------------------------
                      // CAB defaults Coll date to Current date. So don't pass 
                      // anything.
                      // -----------------------------------------------
                      UseFnDeterminePgmForDebtDetail();

                      // -----------------------------------------------
                      // Skip MJ AF/FC.
                      // -----------------------------------------------
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

                      // -----------------------------------------------
                      // So, this must be MJ NA. Continue processing.
                      // -----------------------------------------------
                      goto Test1;
                    }

                    // -----------------------------------------------
                    // We got here because all MJ debts are applied to AF.
                    // Skip this Obligation.
                    // -----------------------------------------------
                    continue;
                  }

Test1:

                  // -----------------------------------------------
                  // We found a prev ldet with desired ob type.
                  // -----------------------------------------------
                  // ----------------------------------------------------------------------
                  // Don't skip yet!
                  // We need to check if current AP and CH were obligated on
                  // this prev ldet.
                  // ----------------------------------------------------------------------
                  if (!ReadLegalActionPerson2())
                  {
                    // ----------------------------------------------------------------------
                    // This prev ldet must have been established for another CH.
                    // ----------------------------------------------------------------------
                    goto ReadEach1;
                  }

                  if (!ReadLegalActionPerson1())
                  {
                    // ----------------------------------------------------------------------
                    // This prev ldet must have been established for another AP.
                    // ----------------------------------------------------------------------
                    goto ReadEach1;
                  }

                  // ----------------------------------------------------------------------
                  // We found a prev ldet on this court order and it meets our 
                  // ob
                  // type criteria and current AP/CH are obligated on this prev
                  // ldet. This means that we must have counted this order for
                  // current AP/CH in a prev FY. Skip this LDET.
                  // ----------------------------------------------------------------------
                  goto Test2;
                }

                // ----------------------------------------------------------------------
                // We got here because we found a prev fin LDET for AP/CH
                // that is not tied to a desirable ob type. Skip this LDET.
                // ----------------------------------------------------------------------
                continue;
              default:
                break;
            }

Test2:

            // ----------------------------------------------------------------------
            // We found a prev ldet on this court order and it meets our ob
            // type criteria and current AP/CH are obligated on this prev
            // ldet. This means that we must have counted this order for
            // current AP/CH in the prev FY.
            // ----------------------------------------------------------------------
            // ----------------------------------------------------------------------
            // Skip this LDET.
            // ----------------------------------------------------------------------
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.CourtOrderNumber =
                entities.LegalAction.StandardNumber;

              if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
              {
                local.ForCreateOcse157Verification.ObTypeSgi = 999;
              }
              else
              {
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
              }

              local.ForCreateOcse157Verification.LineNumber = "17";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-ldet setup on an old order";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            goto ReadEach2;

ReadEach1:
            ;
          }

          // ----------------------------------------------------------------------
          // We did not find a prev ldet on the same court order.
          // Count Case.
          // ----------------------------------------------------------------------
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.ChCsePerson.Number;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              entities.ApCsePerson.Number;
            local.ForCreateOcse157Verification.CourtOrderNumber =
              entities.LegalAction.StandardNumber;

            if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
            {
              local.ForCreateOcse157Verification.ObTypeSgi = 999;
            }
            else
            {
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
            }
          }

          local.CountCase.Flag = "Y";

          goto ReadEach6;

ReadEach2:
          ;
        }

        // ----------------------------------------------------------------------
        // We didn't find a non-fin ldet that meets our criteria.
        // Now look for fin ldet.
        // ----------------------------------------------------------------------
        foreach(var item2 in ReadLegalActionDetailLegalAction2())
        {
          // ----------------------------------------------------------------------
          // We are only interested in specific ob types.
          // Skip LDET if a desirable ob type is nf.
          // ----------------------------------------------------------------------
          foreach(var item3 in ReadObligationTypeObligation1())
          {
            if (!Equal(entities.ObligationType.Code, "CS") && !
              Equal(entities.ObligationType.Code, "CRCH") && !
              Equal(entities.ObligationType.Code, "MS") && !
              Equal(entities.ObligationType.Code, "AJ") && !
              Equal(entities.ObligationType.Code, "MJ") && !
              Equal(entities.ObligationType.Code, "MC"))
            {
              // ----------------------------------------------------------------------
              // Not a desired ob type. Skip this LDET.
              // ----------------------------------------------------------------------
              goto ReadEach5;
            }

            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              // ----------------------------------------------
              // Qualify Debts by Supp person.
              // Only read debts created before FY end.
              // Debt detail due-dt must be <= FY end.
              // ----------------------------------------------
              foreach(var item4 in ReadDebtDebtDetail())
              {
                // -----------------------------------------------
                // CAB defaults Coll date to Current date. So don't pass 
                // anything.
                // -----------------------------------------------
                UseFnDeterminePgmForDebtDetail();

                // -----------------------------------------------
                // Skip MJ AF/FC.
                // -----------------------------------------------
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

                // -----------------------------------------------
                // So, this must be MJ NA. Include this ldet.
                // -----------------------------------------------
                goto ReadEach3;
              }

              // ----------------------------------------------------
              // We got here because all MJ debts are applied to AF.
              // Skip this Obligation.
              // ----------------------------------------------------
              continue;
            }

            // ----------------------------------------------------------------------
            // We found a fin LDET of desired ob type. Continue processing!.
            // ----------------------------------------------------------------------
            break;
          }

ReadEach3:

          if (!entities.Obligation.Populated)
          {
            // -----------------------------------------------
            // No obligations setup yet. Skip this ldet!
            // -----------------------------------------------
            continue;
          }

          // ----------------------------------------------------------------------
          // You are now looking at an ldet(fin or non) which is created in
          // current FY and it meets our ob type requirement.
          // Now check if there is another ldet(fin or non) created in prev
          // FY on the same court order.
          // ----------------------------------------------------------------------
          foreach(var item3 in ReadLegalActionDetailLegalAction1())
          {
            switch(AsChar(entities.PrevLegalActionDetail.DetailType))
            {
              case 'N':
                // ----------------------------------------------------------------------
                // For non-fin LDETs read HIC and UM ob types only.
                // ----------------------------------------------------------------------
                if (!Equal(entities.PrevLegalActionDetail.NonFinOblgType, "HIC") &&
                  !Equal(entities.PrevLegalActionDetail.NonFinOblgType, "UM"))
                {
                  continue;
                }

                // ----------------------------------------------------------------------
                // We found a non-fin ldet for this court order and it is 
                // created
                // in prev FY.
                // ----------------------------------------------------------------------
                // ----------------------------------------------------------------------
                // Don't skip yet!
                // We need to check if current AP and CH were 'tied' to this
                // prev ldet using LROL.
                // ----------------------------------------------------------------------
                if (!ReadLegalActionCaseRole1())
                {
                  // -------------------------------------------------------------
                  // This prev ldet must have been established for another AP.
                  // -------------------------------------------------------------
                  continue;
                }

                if (!ReadLegalActionCaseRole2())
                {
                  // -------------------------------------------------------------
                  // This prev ldet must have been established for another CH.
                  // -------------------------------------------------------------
                  continue;
                }

                break;
              case 'F':
                // ----------------------------------------------------------------------
                // We are only interested in specific ob types.
                // ----------------------------------------------------------------------
                foreach(var item4 in ReadObligationTypeObligation2())
                {
                  if (Equal(entities.ObligationType.Code, "MJ"))
                  {
                    // ----------------------------------------------
                    // Qualify Debts by Supp person.
                    // Only read debts created before FY end.
                    // Debt detail due-dt must be <= FY end.
                    // ----------------------------------------------
                    foreach(var item5 in ReadDebtDebtDetail())
                    {
                      // -----------------------------------------------
                      // CAB defaults Coll date to Current date. So don't pass 
                      // anything.
                      // -----------------------------------------------
                      UseFnDeterminePgmForDebtDetail();

                      // -----------------------------------------------
                      // Skip MJ AF/FC.
                      // -----------------------------------------------
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

                      // -----------------------------------------------
                      // So, this must be MJ NA. Continue processing.
                      // -----------------------------------------------
                      goto Test3;
                    }

                    // -----------------------------------------------
                    // We got here because all MJ debts are applied to AF.
                    // Skip this Obligation.
                    // -----------------------------------------------
                    continue;
                  }

Test3:

                  // -----------------------------------------------
                  // We found a prev ldet with desired ob type.
                  // -----------------------------------------------
                  // ----------------------------------------------------------------------
                  // Don't skip yet!
                  // We need to check if current AP and CH were obligated on
                  // this prev ldet.
                  // ----------------------------------------------------------------------
                  if (!ReadLegalActionPerson2())
                  {
                    // ----------------------------------------------------------------------
                    // This prev ldet must have been established for another CH.
                    // ----------------------------------------------------------------------
                    goto ReadEach4;
                  }

                  if (!ReadLegalActionPerson1())
                  {
                    // ----------------------------------------------------------------------
                    // This prev ldet must have been established for another AP.
                    // ----------------------------------------------------------------------
                    goto ReadEach4;
                  }

                  // ----------------------------------------------------------------------
                  // We found a prev ldet on this court order and it meets our 
                  // ob
                  // type criteria and current AP/CH are obligated on this prev
                  // ldet. This means that we must have counted this order for
                  // current AP/CH in a prev FY. Skip this LDET.
                  // ----------------------------------------------------------------------
                  goto Test4;
                }

                // ----------------------------------------------------------------------
                // We got here because we found a prev fin LDET for AP/CH
                // that is not tied to a desirable ob type. Skip this prev LDET.
                // ----------------------------------------------------------------------
                continue;
              default:
                break;
            }

Test4:

            // ----------------------------------------------------------------------
            // We found a prev ldet on this court order and it meets our ob
            // type criteria and current AP/CH are obligated on this prev
            // ldet. This means that we must have counted this order for
            // current AP/CH in the prev FY.
            // ----------------------------------------------------------------------
            // ----------------------------------------------------------------------
            // Skip this LDET.
            // ----------------------------------------------------------------------
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.CourtOrderNumber =
                entities.LegalAction.StandardNumber;

              if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
              {
                local.ForCreateOcse157Verification.ObTypeSgi = 999;
              }
              else
              {
                local.ForCreateOcse157Verification.ObTypeSgi =
                  entities.ObligationType.SystemGeneratedIdentifier;
              }

              local.ForCreateOcse157Verification.LineNumber = "17";
              local.ForCreateOcse157Verification.Comment =
                "Skipped-ldet setup on an old order";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            goto ReadEach5;

ReadEach4:
            ;
          }

          // ----------------------------------------------------------------------
          // We did not find a prev ldet on the same court order.
          // Count Case.
          // ----------------------------------------------------------------------
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.SuppPersonNumber =
              entities.ChCsePerson.Number;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              entities.ApCsePerson.Number;
            local.ForCreateOcse157Verification.CourtOrderNumber =
              entities.LegalAction.StandardNumber;

            if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
            {
              local.ForCreateOcse157Verification.ObTypeSgi = 999;
            }
            else
            {
              local.ForCreateOcse157Verification.ObTypeSgi =
                entities.ObligationType.SystemGeneratedIdentifier;
            }
          }

          local.CountCase.Flag = "Y";

          goto ReadEach6;

ReadEach5:
          ;
        }
      }

ReadEach6:

      if (AsChar(local.CountCase.Flag) == 'N')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "17";
          local.ForCreateOcse157Verification.Comment =
            "Skipped- LDETs do not meet criteria.";
          UseFnCreateOcse157Verification();
        }

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
          ++local.Line17Curr.Count;
          local.ForCreateOcse157Verification.Column = "b";

          break;
        case 'F':
          ++local.Line17Former.Count;
          local.ForCreateOcse157Verification.Column = "c";

          break;
        default:
          ++local.Line17Never.Count;
          local.ForCreateOcse157Verification.Column = "d";

          break;
      }

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.LineNumber = "17";
        local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
        local.ForCreateOcse157Verification.CaseAsinEffDte =
          entities.CaseAssignment.EffectiveDate;
        local.ForCreateOcse157Verification.CaseAsinEndDte =
          entities.CaseAssignment.DiscontinueDate;
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
        local.ProgramCheckpointRestart.RestartInfo = "17 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line17Curr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line17Former.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line17Never.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "17";
          local.ForError.CaseNumber = entities.Case1.Number;
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
    local.ForCreateOcse157Data.LineNumber = "17";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line17Curr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line17Former.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line17Never.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "18 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "17";
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

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = entities.ChCsePerson.Number;

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
    entities.ChCaseRole.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
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

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

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
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    entities.PrevLegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNumber", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.ApCaseRole.CasNumber);
        db.SetInt32(command, "lgaId", entities.PrevLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.PrevLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.PrevLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.PrevLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.PrevLegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.PrevLegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.PrevLegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 5);
        entities.PrevLegalActionCaseRole.Populated = true;
      });
  }

  private bool ReadLegalActionCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.PrevLegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole2",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.ChCaseRole.CasNumber);
        db.SetInt32(command, "lgaId", entities.PrevLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.PrevLegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.PrevLegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.PrevLegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.PrevLegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.PrevLegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.PrevLegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 5);
        entities.PrevLegalActionCaseRole.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction1()
  {
    entities.PrevLegalAction.Populated = false;
    entities.PrevLegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PrevLegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.PrevLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.PrevLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.PrevLegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.PrevLegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.PrevLegalActionDetail.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.PrevLegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 5);
        entities.PrevLegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.PrevLegalAction.Classification = db.GetString(reader, 7);
        entities.PrevLegalAction.StandardNumber =
          db.GetNullableString(reader, 8);
        entities.PrevLegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.PrevLegalAction.EstablishmentCode =
          db.GetNullableString(reader, 10);
        entities.PrevLegalAction.Populated = true;
        entities.PrevLegalActionDetail.Populated = true;

        return true;
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
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
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
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction3()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetailLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier1", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApCaseRole.CasNumber);
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
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
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 10);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    System.Diagnostics.Debug.Assert(entities.PrevLegalActionDetail.Populated);
    entities.PrevObligor.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.PrevLegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier",
          entities.PrevLegalActionDetail.LgaIdentifier);
        db.SetNullableString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PrevObligor.Identifier = db.GetInt32(reader, 0);
        entities.PrevObligor.CspNumber = db.GetNullableString(reader, 1);
        entities.PrevObligor.LgaRIdentifier = db.GetNullableInt32(reader, 2);
        entities.PrevObligor.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.PrevObligor.AccountType = db.GetNullableString(reader, 4);
        entities.PrevObligor.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    System.Diagnostics.Debug.Assert(entities.PrevLegalActionDetail.Populated);
    entities.PrevSupp.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.PrevLegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier",
          entities.PrevLegalActionDetail.LgaIdentifier);
        db.SetNullableString(command, "cspNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PrevSupp.Identifier = db.GetInt32(reader, 0);
        entities.PrevSupp.CspNumber = db.GetNullableString(reader, 1);
        entities.PrevSupp.LgaRIdentifier = db.GetNullableInt32(reader, 2);
        entities.PrevSupp.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.PrevSupp.AccountType = db.GetNullableString(reader, 4);
        entities.PrevSupp.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

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
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTypeObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.PrevLegalActionDetail.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationTypeObligation2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.PrevLegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier",
          entities.PrevLegalActionDetail.LgaIdentifier);
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
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;

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

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Ocse157Verification from;
    private Ocse157Verification to;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of MonthEndDate.
    /// </summary>
    [JsonPropertyName("monthEndDate")]
    public DateWorkArea MonthEndDate
    {
      get => monthEndDate ??= new();
      set => monthEndDate = value;
    }

    /// <summary>
    /// A value of MonthStartDte.
    /// </summary>
    [JsonPropertyName("monthStartDte")]
    public DateWorkArea MonthStartDte
    {
      get => monthStartDte ??= new();
      set => monthStartDte = value;
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
    /// A value of Line17Curr.
    /// </summary>
    [JsonPropertyName("line17Curr")]
    public Common Line17Curr
    {
      get => line17Curr ??= new();
      set => line17Curr = value;
    }

    /// <summary>
    /// A value of Line17Former.
    /// </summary>
    [JsonPropertyName("line17Former")]
    public Common Line17Former
    {
      get => line17Former ??= new();
      set => line17Former = value;
    }

    /// <summary>
    /// A value of Line17Never.
    /// </summary>
    [JsonPropertyName("line17Never")]
    public Common Line17Never
    {
      get => line17Never ??= new();
      set => line17Never = value;
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
    /// A value of AssistanceProgram.
    /// </summary>
    [JsonPropertyName("assistanceProgram")]
    public Common AssistanceProgram
    {
      get => assistanceProgram ??= new();
      set => assistanceProgram = value;
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
    /// A value of ForVerification.
    /// </summary>
    [JsonPropertyName("forVerification")]
    public Program ForVerification
    {
      get => forVerification ??= new();
      set => forVerification = value;
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

    private Ocse157Verification null1;
    private Case1 restartCase;
    private Case1 prev;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restartCsePerson;
    private DateWorkArea monthEndDate;
    private DateWorkArea monthStartDte;
    private Common countCase;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private Common line17Curr;
    private Common line17Former;
    private Common line17Never;
    private Program program;
    private Common assistanceProgram;
    private CsePerson suppPersForVerification;
    private Program forVerification;
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
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of PrevLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("prevLegalActionCaseRole")]
    public LegalActionCaseRole PrevLegalActionCaseRole
    {
      get => prevLegalActionCaseRole ??= new();
      set => prevLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of PrevObligor.
    /// </summary>
    [JsonPropertyName("prevObligor")]
    public LegalActionPerson PrevObligor
    {
      get => prevObligor ??= new();
      set => prevObligor = value;
    }

    /// <summary>
    /// A value of PrevSupp.
    /// </summary>
    [JsonPropertyName("prevSupp")]
    public LegalActionPerson PrevSupp
    {
      get => prevSupp ??= new();
      set => prevSupp = value;
    }

    /// <summary>
    /// A value of PrevLegalAction.
    /// </summary>
    [JsonPropertyName("prevLegalAction")]
    public LegalAction PrevLegalAction
    {
      get => prevLegalAction ??= new();
      set => prevLegalAction = value;
    }

    /// <summary>
    /// A value of PrevObligation.
    /// </summary>
    [JsonPropertyName("prevObligation")]
    public Obligation PrevObligation
    {
      get => prevObligation ??= new();
      set => prevObligation = value;
    }

    /// <summary>
    /// A value of PrevObligationType.
    /// </summary>
    [JsonPropertyName("prevObligationType")]
    public ObligationType PrevObligationType
    {
      get => prevObligationType ??= new();
      set => prevObligationType = value;
    }

    /// <summary>
    /// A value of PrevLegalActionDetail.
    /// </summary>
    [JsonPropertyName("prevLegalActionDetail")]
    public LegalActionDetail PrevLegalActionDetail
    {
      get => prevLegalActionDetail ??= new();
      set => prevLegalActionDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public LegalActionPerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private LegalActionCaseRole chLegalActionCaseRole;
    private LegalActionCaseRole prevLegalActionCaseRole;
    private LegalActionPerson prevObligor;
    private LegalActionPerson prevSupp;
    private LegalAction prevLegalAction;
    private Obligation prevObligation;
    private ObligationType prevObligationType;
    private LegalActionDetail prevLegalActionDetail;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CaseRole caseRole;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole chCaseRole;
    private CsePerson chCsePerson;
    private Obligation obligation;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private LegalActionPerson obligor;
    private LegalActionPerson supp;
    private CsePersonAccount supported;
  }
#endregion
}
