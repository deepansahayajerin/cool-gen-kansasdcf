// Program: FN_OCSE157_LINE_2, ID: 371092710, model: 746.
// Short name: SWE02924
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_2.
/// </summary>
[Serializable]
public partial class FnOcse157Line2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line2.
  /// </summary>
  public FnOcse157Line2(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // ??/??/??  KDoshi			Initial Development
    // 08/07/01				Delete attribute debt_detail preconversion_program_state.
    // 09/17/01				For non-fin ldets, LOPS is optional.  Read LROL instead.
    // 09/10/02				Include cases with inactive order.
    // 09/08/04  MLBrown	PR00215987	Changed the qualification for the read of 
    // the legal
    // 					action detail table to include 'paternity establishment
    // 					only' (non_fin_oblg_type of 'EP'). This is for the
    // 					count generated in Line 2 of the OCSE157 report for
    // 					the Feds.
    // 03/09/06  GVandy	WR00230751	Federally mandated changes.
    // 					1) Don't include EP details in line 2c.
    // 					2) Check O class legal actions as well as J class actions.
    // 					3) Include tribal and foreign interstate cases in
    // 					   lines 2a and 2b.
    // 					4) Include zero support orders in line 2c.
    // 					5) New lines 2e, 2f, 2g, 2h, 2i
    // 02/04/20  GVandy	CQ66220		Beginning in FY 2022, exclude cases to tribal 
    // and
    // 					international child support agencies from Lines 2a
    // 					and 2b.
    // ---------------------------------------------------------------------------------------------------
    local.GetMedicaidOnlyProgram.Flag = "Y";
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "02 "))
    {
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
      local.Line2Curr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      local.Line2Former.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 24, 10));
      local.Line2Never.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 34, 10));
      local.Line2ACurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 44, 10));
      local.Line2AFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 54, 10));
      local.Line2ANever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 64, 10));
      local.Line2BCurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 74, 10));
      local.Line2BFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 84, 10));
      local.Line2BNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 94, 10));
      local.Line2CCurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 104, 10));
      local.Line2CFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 114, 10));
      local.Line2CNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 124, 10));
      local.Line2DNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 134, 10));
      local.Line2E.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 144, 10));
      local.Line2FCurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 154, 10));
      local.Line2FFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 164, 10));
      local.Line2FNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 174, 10));
      local.Line2GCurr.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 184, 10));
      local.Line2GFormer.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 194, 10));
      local.Line2GNever.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 204, 10));
      local.Line2H.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 214, 10));
      local.Line2I.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 224, 10));
    }

    ReadOcse157Data();

    foreach(var item in ReadCase())
    {
      MoveOcse157Verification2(local.NullOcse157Verification,
        local.ForCreateOcse157Verification);

      // ----------------------------------------------
      // Was this Case reported in line 1? If not, skip.
      // ----------------------------------------------
      ReadOcse157Verification();

      if (IsEmpty(local.Minimum.Number))
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "02";
          local.ForCreateOcse157Verification.Comment =
            "Skipped-case not included in line 1.";
          UseFnCreateOcse157Verification();
        }

        continue;
      }

      local.NonFinLdet.Flag = "N";
      local.FinLdet.Flag = "N";
      local.AccrualInstrFound.Flag = "N";

      // ----------------------------------------------------------------------
      // For current case, read all valid AP/CH combos - active or not.
      // Date checks will ensure we read overlapping AP/CH roles only.
      // If fin ldet found, count in line 2. Don't count case in 2c.
      // If non-fin ldet found, count in line 2. We still need to check if
      // fin LDET exists. This is necessary for line 2c.
      // -----------------------------------------------------------------------
      foreach(var item1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
      {
        // ----------------------------------------------------------------------
        // Using LROL, read J-class HIC or UM ldet - active or not.
        // Skip Legal Actions created after the end of FY.
        // Skip LDETs created after the end of FY.
        // Also include LDETs created in previous FYs.
        // ----------------------------------------------------------------------
        if (AsChar(local.NonFinLdet.Flag) == 'N')
        {
          if (ReadLegalActionDetailLegalAction1())
          {
            local.NonFinLdet.Flag = "Y";

            if (AsChar(local.FinLdet.Flag) == 'Y')
            {
              // ----------------------------------------------------------------------
              // We found a fin and non-fin LDET for this case.
              // No further processing is necessary for this case.
              // ----------------------------------------------------------------------
              break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.SuppPersonNumber =
                entities.ChCsePerson.Number;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
              local.ForCreateOcse157Verification.CourtOrderNumber =
                entities.LegalAction.StandardNumber;
              local.ForCreateOcse157Verification.ObTypeSgi = 999;
            }

            // ----------------------------------------------------------------------
            // Now check if financial LDET exists.
            // ----------------------------------------------------------------------
          }
        }

        // ----------------------------------------------------------------------
        // Using LOPS, read all J-class fin LDETs - active or not.
        // Read for Obligations with specific ob types.
        // Skip Legal Actions created after the end of FY.
        // Skip LDETs created after the end of FY.
        // Also include LDETs created in previous FYs.
        // ----------------------------------------------------------------------
        if (AsChar(local.FinLdet.Flag) == 'N')
        {
          foreach(var item2 in ReadLegalActionDetailLegalAction2())
          {
            foreach(var item3 in ReadObligationTypeObligation())
            {
              // -------------------------------------------------------------------
              // We found a finance LDET with desired Ob types.
              // Now check if Accrual Instructions were 'ever' setup for Current
              // Obligation.
              // Qualify by supported person.
              // --------------------------------------------------------------------
              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (ReadAccrualInstructions2())
                {
                  local.FinLdet.Flag = "Y";

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
                  // We found a fin-LDET for this case.
                  // No further processing is necessary for this case.
                  // ----------------------------------------------------------------------
                  goto ReadEach;
                }
              }

              // -------------------------------------------------------------------
              // We got here because Accrual Instructions were never setup
              // on current Obligation.
              // Now check if debt was 'ever' owed on this obligation.
              // -------------------------------------------------------------------
              // ----------------------------------------------
              // Qualify Debts by Supp person. 7/18/01
              // Only read debts created before FY end.
              // ----------------------------------------------
              foreach(var item4 in ReadDebtDebtDetail())
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

                local.FinLdet.Flag = "Y";

                // ----------------------------------------------------------------------
                // We found a fin LDET for this case.
                // No further processing is necessary for this case.
                // ----------------------------------------------------------------------
                goto ReadEach;
              }
            }
          }
        }
      }

ReadEach:

      if (AsChar(local.NonFinLdet.Flag) == 'N' && AsChar
        (local.FinLdet.Flag) == 'N')
      {
        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.LineNumber = "02";
          local.ForCreateOcse157Verification.Comment = "Skipped-LDET nf.";
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
          ++local.Line2Curr.Count;
          local.ForCreateOcse157Verification.Column = "b";

          break;
        case 'F':
          ++local.Line2Former.Count;
          local.ForCreateOcse157Verification.Column = "c";

          break;
        default:
          ++local.Line2Never.Count;
          local.ForCreateOcse157Verification.Column = "d";

          break;
      }

      local.AssistanceBasedColumn.Column =
        local.ForCreateOcse157Verification.Column ?? "";
      local.ForCreateOcse157Verification.LineNumber = "02";
      local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreateOcse157Verification.PersonProgCode =
          local.ForVerification.Code;
      }

      UseFnCreateOcse157Verification();

      // --------------------------------------------------
      // Logic for lines 2a, 2b, 2f, 2g, 2h, and 2i.
      // --------------------------------------------------
      local.CountedIn2A.Flag = "N";
      local.CountedIn2B.Flag = "N";
      local.CountedIn2F.Flag = "N";
      local.CountedIn2G.Flag = "N";
      local.CountedIn2H.Flag = "N";
      local.CountedIn2I.Flag = "N";

      foreach(var item1 in ReadInterstateRequest())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          if (AsChar(local.CountedIn2A.Flag) == 'N')
          {
            if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() >= import
              .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault() && (
                !IsEmpty(entities.InterstateRequest.Country) || !
              IsEmpty(entities.InterstateRequest.TribalAgency)))
            {
              // 02/04/20 GVandy  CQ66220  Beginning in FY 2022, exclude cases 
              // to tribal and
              // international child support agencies from Lines 2a and 2b.
              goto Test1;
            }

            local.CountedIn2A.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line2ACurr.Count;

                break;
              case 'F':
                ++local.Line2AFormer.Count;

                break;
              default:
                ++local.Line2ANever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "02a";
              UseFnCreateOcse157Verification();
            }
          }

Test1:

          if (AsChar(local.CountedIn2F.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.TribalAgency))
          {
            local.CountedIn2F.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line2FCurr.Count;

                break;
              case 'F':
                ++local.Line2FFormer.Count;

                break;
              default:
                ++local.Line2FNever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "02f";
              UseFnCreateOcse157Verification();
            }
          }

          if (AsChar(local.CountedIn2H.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.Country))
          {
            local.CountedIn2H.Flag = "Y";
            ++local.Line2H.Count;

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column = "a";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "02h";
              UseFnCreateOcse157Verification();
            }
          }
        }
        else if (AsChar(entities.InterstateRequest.KsCaseInd) == 'N')
        {
          if (AsChar(local.CountedIn2B.Flag) == 'N')
          {
            if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() >= import
              .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault() && (
                !IsEmpty(entities.InterstateRequest.Country) || !
              IsEmpty(entities.InterstateRequest.TribalAgency)))
            {
              // 02/04/20 GVandy  CQ66220  Beginning in FY 2022, exclude cases 
              // to tribal and
              // international child support agencies from Lines 2a and 2b.
              goto Test2;
            }

            local.CountedIn2B.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line2BCurr.Count;

                break;
              case 'F':
                ++local.Line2BFormer.Count;

                break;
              default:
                ++local.Line2BNever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "02b";
              UseFnCreateOcse157Verification();
            }
          }

Test2:

          if (AsChar(local.CountedIn2G.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.TribalAgency))
          {
            local.CountedIn2G.Flag = "Y";

            switch(AsChar(local.AssistanceProgram.Flag))
            {
              case 'C':
                ++local.Line2GCurr.Count;

                break;
              case 'F':
                ++local.Line2GFormer.Count;

                break;
              default:
                ++local.Line2GNever.Count;

                break;
            }

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column =
                local.AssistanceBasedColumn.Column ?? "";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "02g";
              UseFnCreateOcse157Verification();
            }
          }

          if (AsChar(local.CountedIn2I.Flag) == 'N' && !
            IsEmpty(entities.InterstateRequest.Country))
          {
            local.CountedIn2I.Flag = "Y";
            ++local.Line2I.Count;

            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.Column = "a";
              local.ForCreateOcse157Verification.IntRequestIdent =
                entities.InterstateRequest.IntHGeneratedId;
              local.ForCreateOcse157Verification.KansasCaseInd =
                entities.InterstateRequest.KsCaseInd;
              local.ForCreateOcse157Verification.LineNumber = "02i";
              UseFnCreateOcse157Verification();
            }
          }
        }
      }

      // -----------------------------
      // Logic for line 2c.
      // -----------------------------
      if (AsChar(local.NonFinLdet.Flag) == 'Y' && AsChar
        (local.FinLdet.Flag) == 'N')
      {
        switch(AsChar(local.AssistanceProgram.Flag))
        {
          case 'C':
            ++local.Line2CCurr.Count;

            break;
          case 'F':
            ++local.Line2CFormer.Count;

            break;
          default:
            ++local.Line2CNever.Count;

            break;
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.Column =
            local.AssistanceBasedColumn.Column ?? "";
          local.ForCreateOcse157Verification.LineNumber = "02c";
          UseFnCreateOcse157Verification();
        }
      }

      // -----------------------------
      // Logic for line 2d.
      // -----------------------------
      if (AsChar(local.AssistanceProgram.Flag) == 'M')
      {
        ++local.Line2DNever.Count;

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreateOcse157Verification.Column =
            local.AssistanceBasedColumn.Column ?? "";
          local.ForCreateOcse157Verification.LineNumber = "02d";
          UseFnCreateOcse157Verification();
        }
      }

      // -----------------------------
      // Logic for line 2e.
      // -----------------------------
      if (AsChar(local.FinLdet.Flag) == 'Y')
      {
        // -- Check for any open accrual instructions.
        if (ReadAccrualInstructions1())
        {
          // -- One or more active accrual instruction exists.  The case is not 
          // arrears only.  Do not include in Line 2e.
          goto Test3;
        }

        // -- Check for debt due at the end of the reporting period.
        foreach(var item1 in ReadDebtDetail())
        {
          // --  check for correct obligation type and that it is not MJ (AF,
          // AFI,FC,FCI)
          if (ReadObligationType())
          {
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              // -----------------------------------------------
              // CAB defaults Coll date to Current date. So don't pass anything.
              // -----------------------------------------------
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
          }
          else
          {
            continue;
          }

          // The case is arrears only.  Count in Line 2e.
          ++local.Line2E.Count;
          local.ForCreateOcse157Verification.LineNumber = "02e";
          local.ForCreateOcse157Verification.Column = "a";
          UseFnCreateOcse157Verification();

          break;
        }
      }

Test3:

      ++local.CommitCnt.Count;

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "02 " + entities
          .Case1.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2Curr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2Former.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2Never.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2ACurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2AFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2ANever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2BCurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2BFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2BNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2CCurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2CFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2CNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2DNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2E.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2FCurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2FFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2FNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2GCurr.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2GFormer.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2GNever.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2H.Count, 6, 10);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (local.Line2I.Count, 6, 10);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "02";
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
    // Processing complete for line 2.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "02";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line2Curr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line2Former.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line2Never.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02a";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line2ACurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line2AFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line2ANever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02b";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line2BCurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line2BFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line2BNever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02c";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line2CCurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line2CFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line2CNever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02d";
    local.ForCreateOcse157Data.Number = local.Line2DNever.Count;
    local.ForCreateOcse157Data.Column = "d";
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02e";
    local.ForCreateOcse157Data.Number = local.Line2E.Count;
    local.ForCreateOcse157Data.Column = "a";
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02f";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line2FCurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line2FFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line2FNever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02g";
    local.ForCreateOcse157Data.Column = "b";
    local.ForCreateOcse157Data.Number = local.Line2GCurr.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "c";
    local.ForCreateOcse157Data.Number = local.Line2GFormer.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.Column = "d";
    local.ForCreateOcse157Data.Number = local.Line2GNever.Count;
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02h";
    local.ForCreateOcse157Data.Number = local.Line2H.Count;
    local.ForCreateOcse157Data.Column = "a";
    UseFnCreateOcse157Data();
    local.ForCreateOcse157Data.LineNumber = "02i";
    local.ForCreateOcse157Data.Number = local.Line2I.Count;
    local.ForCreateOcse157Data.Column = "a";
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "03 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "02";
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

    useImport.GetMedicaidOnlyProgram.Flag = local.GetMedicaidOnlyProgram.Flag;
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

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.SupportedPerson.Number = entities.ChCsePerson.Number;

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

  private bool ReadAccrualInstructions1()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt",
          import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
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
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualInstructions2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions2",
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
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
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
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ChCaseRole.Populated = false;
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

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
        entities.ChCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ChCsePerson.Number = db.GetString(reader, 8);
        entities.ChCaseRole.Type1 = db.GetString(reader, 9);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.ChCaseRole.DateOfEmancipation = db.GetNullableDate(reader, 13);
        entities.ChCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);

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
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 10);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "othStateClsDte",
          import.ReportEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 2);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 3);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 6);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 7);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionDetailLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionDetailLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier1", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApCaseRole.CasNumber);
        db.SetNullableDate(
          command, "filedDt", local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
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
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalAction2()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

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
          
        db.SetNullableDate(
          command, "filedDt", local.NullDateWorkArea.Date.GetValueOrDefault());
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
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.DebtDetail.OtyType);
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

  private IEnumerable<bool> ReadObligationTypeObligation()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationTypeObligation",
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
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

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
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
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
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    [JsonPropertyName("cq66220EffectiveFy")]
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private Ocse157Verification ocse157Verification;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private Common displayInd;
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Ocse157Verification cq66220EffectiveFy;
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
    /// A value of AssistanceBasedColumn.
    /// </summary>
    [JsonPropertyName("assistanceBasedColumn")]
    public Ocse157Verification AssistanceBasedColumn
    {
      get => assistanceBasedColumn ??= new();
      set => assistanceBasedColumn = value;
    }

    /// <summary>
    /// A value of AccrualInstrFound.
    /// </summary>
    [JsonPropertyName("accrualInstrFound")]
    public Common AccrualInstrFound
    {
      get => accrualInstrFound ??= new();
      set => accrualInstrFound = value;
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
    /// A value of Line2I.
    /// </summary>
    [JsonPropertyName("line2I")]
    public Common Line2I
    {
      get => line2I ??= new();
      set => line2I = value;
    }

    /// <summary>
    /// A value of Line2H.
    /// </summary>
    [JsonPropertyName("line2H")]
    public Common Line2H
    {
      get => line2H ??= new();
      set => line2H = value;
    }

    /// <summary>
    /// A value of Line2GCurr.
    /// </summary>
    [JsonPropertyName("line2GCurr")]
    public Common Line2GCurr
    {
      get => line2GCurr ??= new();
      set => line2GCurr = value;
    }

    /// <summary>
    /// A value of Line2GFormer.
    /// </summary>
    [JsonPropertyName("line2GFormer")]
    public Common Line2GFormer
    {
      get => line2GFormer ??= new();
      set => line2GFormer = value;
    }

    /// <summary>
    /// A value of Line2GNever.
    /// </summary>
    [JsonPropertyName("line2GNever")]
    public Common Line2GNever
    {
      get => line2GNever ??= new();
      set => line2GNever = value;
    }

    /// <summary>
    /// A value of Line2FCurr.
    /// </summary>
    [JsonPropertyName("line2FCurr")]
    public Common Line2FCurr
    {
      get => line2FCurr ??= new();
      set => line2FCurr = value;
    }

    /// <summary>
    /// A value of Line2FFormer.
    /// </summary>
    [JsonPropertyName("line2FFormer")]
    public Common Line2FFormer
    {
      get => line2FFormer ??= new();
      set => line2FFormer = value;
    }

    /// <summary>
    /// A value of Line2FNever.
    /// </summary>
    [JsonPropertyName("line2FNever")]
    public Common Line2FNever
    {
      get => line2FNever ??= new();
      set => line2FNever = value;
    }

    /// <summary>
    /// A value of Line2E.
    /// </summary>
    [JsonPropertyName("line2E")]
    public Common Line2E
    {
      get => line2E ??= new();
      set => line2E = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public Ocse157Data Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of DetermineMedicaidOnly.
    /// </summary>
    [JsonPropertyName("determineMedicaidOnly")]
    public Common DetermineMedicaidOnly
    {
      get => determineMedicaidOnly ??= new();
      set => determineMedicaidOnly = value;
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
    /// A value of Line2Curr.
    /// </summary>
    [JsonPropertyName("line2Curr")]
    public Common Line2Curr
    {
      get => line2Curr ??= new();
      set => line2Curr = value;
    }

    /// <summary>
    /// A value of Line2Former.
    /// </summary>
    [JsonPropertyName("line2Former")]
    public Common Line2Former
    {
      get => line2Former ??= new();
      set => line2Former = value;
    }

    /// <summary>
    /// A value of Line2Never.
    /// </summary>
    [JsonPropertyName("line2Never")]
    public Common Line2Never
    {
      get => line2Never ??= new();
      set => line2Never = value;
    }

    /// <summary>
    /// A value of Line2ACurr.
    /// </summary>
    [JsonPropertyName("line2ACurr")]
    public Common Line2ACurr
    {
      get => line2ACurr ??= new();
      set => line2ACurr = value;
    }

    /// <summary>
    /// A value of Line2AFormer.
    /// </summary>
    [JsonPropertyName("line2AFormer")]
    public Common Line2AFormer
    {
      get => line2AFormer ??= new();
      set => line2AFormer = value;
    }

    /// <summary>
    /// A value of Line2ANever.
    /// </summary>
    [JsonPropertyName("line2ANever")]
    public Common Line2ANever
    {
      get => line2ANever ??= new();
      set => line2ANever = value;
    }

    /// <summary>
    /// A value of Line2BCurr.
    /// </summary>
    [JsonPropertyName("line2BCurr")]
    public Common Line2BCurr
    {
      get => line2BCurr ??= new();
      set => line2BCurr = value;
    }

    /// <summary>
    /// A value of Line2BFormer.
    /// </summary>
    [JsonPropertyName("line2BFormer")]
    public Common Line2BFormer
    {
      get => line2BFormer ??= new();
      set => line2BFormer = value;
    }

    /// <summary>
    /// A value of Line2BNever.
    /// </summary>
    [JsonPropertyName("line2BNever")]
    public Common Line2BNever
    {
      get => line2BNever ??= new();
      set => line2BNever = value;
    }

    /// <summary>
    /// A value of Line2CCurr.
    /// </summary>
    [JsonPropertyName("line2CCurr")]
    public Common Line2CCurr
    {
      get => line2CCurr ??= new();
      set => line2CCurr = value;
    }

    /// <summary>
    /// A value of Line2CFormer.
    /// </summary>
    [JsonPropertyName("line2CFormer")]
    public Common Line2CFormer
    {
      get => line2CFormer ??= new();
      set => line2CFormer = value;
    }

    /// <summary>
    /// A value of Line2CNever.
    /// </summary>
    [JsonPropertyName("line2CNever")]
    public Common Line2CNever
    {
      get => line2CNever ??= new();
      set => line2CNever = value;
    }

    /// <summary>
    /// A value of Line2DNever.
    /// </summary>
    [JsonPropertyName("line2DNever")]
    public Common Line2DNever
    {
      get => line2DNever ??= new();
      set => line2DNever = value;
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
    /// A value of NonFinLdet.
    /// </summary>
    [JsonPropertyName("nonFinLdet")]
    public Common NonFinLdet
    {
      get => nonFinLdet ??= new();
      set => nonFinLdet = value;
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
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
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
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    /// <summary>
    /// A value of CountedIn2A.
    /// </summary>
    [JsonPropertyName("countedIn2A")]
    public Common CountedIn2A
    {
      get => countedIn2A ??= new();
      set => countedIn2A = value;
    }

    /// <summary>
    /// A value of CountedIn2B.
    /// </summary>
    [JsonPropertyName("countedIn2B")]
    public Common CountedIn2B
    {
      get => countedIn2B ??= new();
      set => countedIn2B = value;
    }

    /// <summary>
    /// A value of CountedIn2F.
    /// </summary>
    [JsonPropertyName("countedIn2F")]
    public Common CountedIn2F
    {
      get => countedIn2F ??= new();
      set => countedIn2F = value;
    }

    /// <summary>
    /// A value of CountedIn2G.
    /// </summary>
    [JsonPropertyName("countedIn2G")]
    public Common CountedIn2G
    {
      get => countedIn2G ??= new();
      set => countedIn2G = value;
    }

    /// <summary>
    /// A value of CountedIn2H.
    /// </summary>
    [JsonPropertyName("countedIn2H")]
    public Common CountedIn2H
    {
      get => countedIn2H ??= new();
      set => countedIn2H = value;
    }

    /// <summary>
    /// A value of CountedIn2I.
    /// </summary>
    [JsonPropertyName("countedIn2I")]
    public Common CountedIn2I
    {
      get => countedIn2I ??= new();
      set => countedIn2I = value;
    }

    private Ocse157Verification assistanceBasedColumn;
    private Common accrualInstrFound;
    private DateWorkArea nullDateWorkArea;
    private Common line2I;
    private Common line2H;
    private Common line2GCurr;
    private Common line2GFormer;
    private Common line2GNever;
    private Common line2FCurr;
    private Common line2FFormer;
    private Common line2FNever;
    private Common line2E;
    private Ocse157Verification nullOcse157Verification;
    private Ocse157Data max;
    private Common finLdet;
    private Common determineMedicaidOnly;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restart;
    private Common line2Curr;
    private Common line2Former;
    private Common line2Never;
    private Common line2ACurr;
    private Common line2AFormer;
    private Common line2ANever;
    private Common line2BCurr;
    private Common line2BFormer;
    private Common line2BNever;
    private Common line2CCurr;
    private Common line2CFormer;
    private Common line2CNever;
    private Common line2DNever;
    private Case1 minimum;
    private Common nonFinLdet;
    private Case1 prev;
    private Program forVerification;
    private CsePerson suppPersForVerification;
    private Common assistanceProgram;
    private Common getMedicaidOnlyProgram;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private Common commitCnt;
    private Program program;
    private Ocse157Verification forError;
    private Common countedIn2A;
    private Common countedIn2B;
    private Common countedIn2F;
    private Common countedIn2G;
    private Common countedIn2H;
    private Common countedIn2I;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
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
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
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
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public LegalActionPerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public LegalActionPerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private CsePersonAccount obligor1;
    private LegalActionCaseRole chLegalActionCaseRole;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CsePersonAccount supported;
    private LegalActionPerson supp;
    private LegalActionPerson obligor2;
    private Collection collection;
    private ObligationTransaction debt;
    private AccrualInstructions accrualInstructions;
    private DebtDetail debtDetail;
    private Ocse157Data ocse157Data;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private Obligation obligation;
    private Ocse157Verification ocse157Verification;
    private Case1 case1;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private LegalActionDetail legalActionDetail;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private InterstateRequest interstateRequest;
  }
#endregion
}
