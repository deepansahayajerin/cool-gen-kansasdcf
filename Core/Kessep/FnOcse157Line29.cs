// Program: FN_OCSE157_LINE_29, ID: 371119899, model: 746.
// Short name: SWE02968
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_LINE_29.
/// </summary>
[Serializable]
public partial class FnOcse157Line29: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_LINE_29 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157Line29(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157Line29.
  /// </summary>
  public FnOcse157Line29(IContext context, Import import, Export export):
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
    // 08/xx/2001 				Initial version
    // 08/30/2001				Include CSENet collections. Also include coll
    // 					type=27 for FDSO.
    // 08/30/2001				Change FDSO collection logic to look at unpaid
    // 					debts only.
    // 09/04/2001				Exclude concurrent collections.
    // 09/05/2001				Skip debts created after FY end. (We read debts
    // 					to determine if NA is due as of FDSO collection
    // 					date.)
    // 09/06/2001				Only read FDSO collections that apply to Arrears.
    // 09/18/2001				Performance change - spilt READ EACHes of collection
    // 					to avoid joining collection and ob_trn.
    // 08/06/2006				Change final checkpoint to from Line 38 to Line 33.
    // 07/11/2008  		CQ2461		If the only activity on the Case is an adjustment
    // 					in the current FY on a Collection created in a previous
    // 					FY, exclude the case.
    // 02/04/20  GVandy	CQ66220		Beginning in FY 2022, include only amounts that
    // are
    // 					both distributed and disbursed.
    // ---------------------------------------------------------------------------------------------------
    local.ForCreateOcse157Verification.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Verification.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ForCreateOcse157Verification.LineNumber = "29";
    local.ForCreateOcse157Data.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreateOcse157Data.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    if (import.Ocse157Verification.FiscalYear.GetValueOrDefault() < import
      .Cq66220EffectiveFy.FiscalYear.GetValueOrDefault())
    {
      if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
        (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "29 "))
      {
        local.Restart.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
        local.Line29.Count =
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

        // --------------------------------------------------
        // Was this Case reported in line 28? If not, skip.
        // --------------------------------------------------
        ReadOcse157Data();
        ReadOcse157Verification();

        if (IsEmpty(local.Minimum.Number))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Comment =
              "Skipped-case not included in line 28.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // ----------------------------------------------------------------------
        // Read all APs on Case - active or not.
        // READ EACH property is set to fetch 'distinct' rows to avoid
        // spinning through same person multiple times.
        // -----------------------------------------------------------------------
        foreach(var item1 in ReadCsePerson2())
        {
          // -------------------------------------------------------------------
          // Step # 1 - Look for NA or NAI Collection for current AP
          // -Created 'during' FY.
          // -Applied to Arrears
          // -Skip direct payments through REIP (CRT = 2 or 7)
          // -Skip concurrent collections.(9/4/2001)
          // -Skip collections created and adjusted during FY.
          // -------------------------------------------------------------------
          foreach(var item2 in ReadCollection5())
          {
            if (!ReadCsePerson1())
            {
              continue;
            }

            if (!ReadObligationType())
            {
              continue;
            }

            // ---------------------------------
            // Skip Fees, Recoveries, 718B.
            // ---------------------------------
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

            // -----------------------------------------------------------
            // Ok, we found a desired Arrears collection. But was this
            // collection applied to a CH/AR on this case?
            // -----------------------------------------------------------
            if (!ReadCaseRole())
            {
              continue;
            }

            // ------------------------------------------------------------------
            // 07/11/2008  CQ2461
            // If the only activity on the Case is an adjustment in the current 
            // FY
            // on a Collection created in a previous FY, exclude the case.
            // ------------------------------------------------------------------
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
                  entities.Supp.Number;
                local.ForCreateOcse157Verification.CaseRoleType =
                  entities.ChOrAr.Type1;
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
            else
            {
              // -- Continue...
            }

            // -----------------------------------------------------------
            // Yipeee! We found an NA or NAI collection - Count case.
            // -----------------------------------------------------------
            local.CountCase.Flag = "Y";

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
                entities.Supp.Number;
              local.ForCreateOcse157Verification.CaseRoleType =
                entities.ChOrAr.Type1;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
            }

            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Column = "a";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);

            goto ReadEach1;
          }
        }

ReadEach1:

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received.
          // Now check if we received any non-NA arrears coll during FY.
          // -------------------------------------------------------------------
          foreach(var item1 in ReadCsePerson2())
          {
            // -------------------------------------------------------------------
            // Step #2 - Look for non-NA, non-NAI Collection for current AP
            // -Created 'during' FY.
            // -Applied to Arrears
            // -Skip direct payments through REIP (CRT = 2 or 7)
            // -Skip concurrent collections. (9/4/2001)
            // -Skip collections created and adjusted during FY.
            // -------------------------------------------------------------------
            local.CollFound.Flag = "N";

            foreach(var item2 in ReadCollection7())
            {
              if (!ReadCsePerson1())
              {
                continue;
              }

              if (!ReadObligationType())
              {
                continue;
              }

              // ---------------------------------
              // Skip Fees, Recoveries, 718B.
              // ---------------------------------
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

              // -----------------------------------------------------------
              // Ok, we found a desired Arrears collection for AP. But does
              // the Supp Person participate on this case?
              // -----------------------------------------------------------
              if (!ReadCaseRole())
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
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
                    entities.Supp.Number;
                  local.ForCreateOcse157Verification.CaseRoleType =
                    entities.ChOrAr.Type1;
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
              else
              {
                // -- Continue...
              }

              local.CollFound.Flag = "Y";

              // -----------------------------------------------------------
              // Save these views to create verification record later.
              // -----------------------------------------------------------
              local.Ap.Number = entities.ApCsePerson.Number;
              local.Supp.Number = entities.Supp.Number;
              MoveCollection(entities.Collection, local.NonNa);

              goto ReadEach2;
            }
          }

ReadEach2:

          if (AsChar(local.CollFound.Flag) == 'N')
          {
            // -------------------------------------------------------------
            // No Arrears coll was received during FY. Skip Case.
            // -------------------------------------------------------------
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.Comment =
                "Skipped-No Arrears coll during FY.";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }

          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received
          // but a collection was definitely received.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 3. - Look for FDSO collection (coll type = 3 or 27)
          // -------------------------------------------------------------------
          foreach(var item1 in ReadCsePerson2())
          {
            local.CollectionDate.Date = local.Null1.CollCreatedDte;

            foreach(var item2 in ReadCollection11())
            {
              if (Equal(entities.Collection.CollectionDt,
                local.CollectionDate.Date))
              {
                continue;
              }

              local.CollectionDate.Date = entities.Collection.CollectionDt;

              // -------------------------------------------------------------
              // Check to see if all supp persons have AF, AFI, FC, FCI active
              // as of collection date?
              // -------------------------------------------------------------
              UseFnCheckForActiveAfFcPgm();

              // ----------------------------------------------------------
              // If all supp persons on Case are on assistance as of coll date,
              // we would never derive NA on any debt. Skip Collection.
              // ----------------------------------------------------------
              if (AsChar(local.Assistance.Flag) == 'Y')
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
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
                  local.ForCreateOcse157Verification.ObligorPersonNbr =
                    entities.ApCsePerson.Number;
                  local.ForCreateOcse157Verification.CaseNumber =
                    entities.Case1.Number;
                  local.ForCreateOcse157Verification.Comment =
                    "Skipped-Result of Adjusted FDSO Coll";
                  UseFnCreateOcse157Verification();
                  MoveOcse157Verification2(local.Null1,
                    local.ForCreateOcse157Verification);
                }

                continue;
              }
              else
              {
                // -- Continue...
              }

              // ----------------------------------------------------------
              // Atleast one person on case is not on assistance as of coll
              // date. Spin through each debt with due date <= coll date to
              // check if NA or NAI program is determined.
              // ----------------------------------------------------------
              // ------------------------------------------------------------
              // We will only look for debts with bal due as of FY end.
              // First, read debts with bal due as of today.
              // Later, we'll read debts with zero bal today but bal was due
              // on FY end.
              // -------------------------------------------------------------
              foreach(var item3 in ReadDebtDetailObligationObligationTypeCsePerson2())
                
              {
                UseFnDeterminePgmForDebtDetail();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.CaseNumber =
                      entities.Case1.Number;
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.Supp.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.DebtDetailDueDt =
                      entities.DebtDetail.DueDt;
                    local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                      entities.DebtDetail.BalanceDueAmt;
                    local.ForCreateOcse157Verification.ObligationSgi =
                      entities.Obligation.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.Comment =
                      "Skipped-NA/NAI derived on FDSO coll.";
                    UseFnCreateOcse157Verification();
                    MoveOcse157Verification2(local.Null1,
                      local.ForCreateOcse157Verification);
                  }

                  goto ReadEach3;
                }
              }

              // -----------------------------------------------------------------------
              // We either didn't find any debt with bal due or we found a debt
              // but no NA or NAI was owed on Collection date.
              // Now read for debts with zero bal today but where bal was
              // due on FY end.
              // -----------------------------------------------------------------------
              // -----------------------------------------------------------------------
              // Only do this if there is any collection/debt activity
              // for AP after FY end. There is no point in spinning through all
              // Zero debts if there is no activity at all for this AP.
              // -----------------------------------------------------------------------
              if (!ReadCollection2())
              {
                if (!ReadDebtAdjustment2())
                {
                  // -------------------------------------------------------------------------
                  // No collection/debt activity for AP after FY end.
                  // Process next FDSO Collection.
                  // --------------------------------------------------------------------------
                  continue;
                }
              }

              // -----------------------------------------------------------------------
              // We got here because there was some activity for AP since
              // FY end.
              // Now read for debts with zero bal today. Then determine if bal
              // was due on FY end.
              // -----------------------------------------------------------------------
              foreach(var item3 in ReadDebtDetailObligationObligationTypeCsePerson1())
                
              {
                // -----------------------------------------------------------------------
                // Check if debt has had any activity after FY end.
                // -----------------------------------------------------------------------
                if (!ReadCollection3())
                {
                  if (!ReadDebtAdjustment1())
                  {
                    // -------------------------------------------------------------------------
                    // No activity on debt since FY end. Process next debt.
                    // --------------------------------------------------------------------------
                    continue;
                  }
                }

                // -------------------------------------------------------------------------
                // You are looking at a debt with zero bal today but balance
                // was due on FY end.
                // --------------------------------------------------------------------------
                UseFnDeterminePgmForDebtDetail();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.CaseNumber =
                      entities.Case1.Number;
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.Supp.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.DebtDetailDueDt =
                      entities.DebtDetail.DueDt;
                    local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                      entities.DebtDetail.BalanceDueAmt;
                    local.ForCreateOcse157Verification.ObligationSgi =
                      entities.Obligation.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.Comment =
                      "Skipped-NA/NAI derived on FDSO coll.";
                    UseFnCreateOcse157Verification();
                    MoveOcse157Verification2(local.Null1,
                      local.ForCreateOcse157Verification);
                  }

                  goto ReadEach3;
                }
              }

              // ----------------------------------------------------------
              // We got here because we didn't derive NA or NAI on any debt
              // for current FDSO collection date. Process next FDSO coll.
              // ----------------------------------------------------------
            }
          }

          // ----------------------------------------------------------
          // We got here because we either didn't find any FDSO
          // collection or we found FDSO collections but none of the
          // debts derived NA or NAI.
          // We also know that non-NA arrears collection was definitely
          // received. No further processing is necessary. Count case!
          // ----------------------------------------------------------
          local.CountCase.Flag = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CollectionSgi =
              local.NonNa.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.CollectionAmount =
              local.NonNa.Amount;
            local.ForCreateOcse157Verification.CollApplToCode =
              local.NonNa.AppliedToCode;
            local.ForCreateOcse157Verification.CollCreatedDte =
              Date(local.NonNa.CreatedTmst);
            local.ForCreateOcse157Verification.CollectionDte =
              local.NonNa.CollectionDt;
            local.ForCreateOcse157Verification.SuppPersonNumber =
              local.Supp.Number;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              local.Ap.Number;
          }

          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.Column = "a";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);
        }

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
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
        ++local.Line29.Count;

        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "29 " + entities
            .Case1.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (local.Line29.Count, 6, 10);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.LineNumber = "29";
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
ReadEach3:
        ;
      }
    }
    else
    {
      if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
        (import.ProgramCheckpointRestart.RestartInfo, 1, 3, "29 "))
      {
        local.Restart.Number =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 10);
        local.Line29.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 14, 10));
      }

      // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts 
      // that are both
      // distributed and disbursed.
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

        // --------------------------------------------------
        // Was this Case reported in line 28? If not, skip.
        // --------------------------------------------------
        ReadOcse157Data();
        ReadOcse157Verification();

        if (IsEmpty(local.Minimum.Number))
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Comment =
              "Skipped-case not included in line 28.";
            UseFnCreateOcse157Verification();
          }

          continue;
        }

        // ----------------------------------------------------------------------
        // Read all APs on Case - active or not.
        // READ EACH property is set to fetch 'distinct' rows to avoid
        // spinning through same person multiple times.
        // -----------------------------------------------------------------------
        foreach(var item1 in ReadCsePerson2())
        {
          // -------------------------------------------------------------------
          // Step # 1 - Look for NA or NAI Collection for current AP
          // -Created 'during' FY.
          // -Applied to Arrears
          // -Skip direct payments through REIP (CRT = 2 or 7)
          // -Skip concurrent collections.(9/4/2001)
          // -Skip collections created and adjusted during FY.
          // -------------------------------------------------------------------
          foreach(var item2 in ReadCollection4())
          {
            if (!ReadCsePerson1())
            {
              continue;
            }

            if (!ReadObligationType())
            {
              continue;
            }

            // ---------------------------------
            // Skip Fees, Recoveries, 718B.
            // ---------------------------------
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

            // -----------------------------------------------------------
            // Ok, we found a desired Arrears collection. But was this
            // collection applied to a CH/AR on this case?
            // -----------------------------------------------------------
            if (!ReadCaseRole())
            {
              continue;
            }

            // ------------------------------------------------------------------
            // 07/11/2008  CQ2461
            // If the only activity on the Case is an adjustment in the current 
            // FY
            // on a Collection created in a previous FY, exclude the case.
            // ------------------------------------------------------------------
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
                  entities.Supp.Number;
                local.ForCreateOcse157Verification.CaseRoleType =
                  entities.ChOrAr.Type1;
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
            else
            {
              // -- Continue...
            }

            // -----------------------------------------------------------
            // Yipeee! We found an NA or NAI collection - Count case.
            // -----------------------------------------------------------
            local.CountCase.Flag = "Y";

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
                entities.Supp.Number;
              local.ForCreateOcse157Verification.CaseRoleType =
                entities.ChOrAr.Type1;
              local.ForCreateOcse157Verification.ObligorPersonNbr =
                entities.ApCsePerson.Number;
            }

            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
            local.ForCreateOcse157Verification.Column = "a";
            UseFnCreateOcse157Verification();
            MoveOcse157Verification2(local.Null1,
              local.ForCreateOcse157Verification);

            goto ReadEach4;
          }
        }

ReadEach4:

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received.
          // Now check if we received any non-NA arrears coll during FY.
          // -------------------------------------------------------------------
          foreach(var item1 in ReadCsePerson2())
          {
            // -------------------------------------------------------------------
            // Step #2 - Look for non-NA, non-NAI Collection for current AP
            // -Created 'during' FY.
            // -Applied to Arrears
            // -Skip direct payments through REIP (CRT = 2 or 7)
            // -Skip concurrent collections. (9/4/2001)
            // -Skip collections created and adjusted during FY.
            // -------------------------------------------------------------------
            local.CollFound.Flag = "N";

            foreach(var item2 in ReadCollection8())
            {
              if (!ReadCsePerson1())
              {
                continue;
              }

              if (!ReadObligationType())
              {
                continue;
              }

              // ---------------------------------
              // Skip Fees, Recoveries, 718B.
              // ---------------------------------
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

              // -----------------------------------------------------------
              // Ok, we found a desired Arrears collection for AP. But does
              // the Supp Person participate on this case?
              // -----------------------------------------------------------
              if (!ReadCaseRole())
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
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
                    entities.Supp.Number;
                  local.ForCreateOcse157Verification.CaseRoleType =
                    entities.ChOrAr.Type1;
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
              else
              {
                // -- Continue...
              }

              local.CollFound.Flag = "Y";

              // -----------------------------------------------------------
              // Save these views to create verification record later.
              // -----------------------------------------------------------
              local.Ap.Number = entities.ApCsePerson.Number;
              local.Supp.Number = entities.Supp.Number;
              MoveCollection(entities.Collection, local.NonNa);

              goto ReadEach5;
            }

            foreach(var item2 in ReadCollection6())
            {
              if (!ReadCsePerson1())
              {
                continue;
              }

              if (!ReadObligationType())
              {
                continue;
              }

              // ---------------------------------
              // Skip Fees, Recoveries, 718B.
              // ---------------------------------
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

              // -----------------------------------------------------------
              // Ok, we found a desired Arrears collection for AP. But does
              // the Supp Person participate on this case?
              // -----------------------------------------------------------
              if (!ReadCaseRole())
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
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
                    entities.Supp.Number;
                  local.ForCreateOcse157Verification.CaseRoleType =
                    entities.ChOrAr.Type1;
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
              else
              {
                // -- Continue...
              }

              local.CollFound.Flag = "Y";

              // -----------------------------------------------------------
              // Save these views to create verification record later.
              // -----------------------------------------------------------
              local.Ap.Number = entities.ApCsePerson.Number;
              local.Supp.Number = entities.Supp.Number;
              MoveCollection(entities.Collection, local.NonNa);

              goto ReadEach5;
            }
          }

ReadEach5:

          if (AsChar(local.CollFound.Flag) == 'N')
          {
            // -------------------------------------------------------------
            // No Arrears coll was received during FY. Skip Case.
            // -------------------------------------------------------------
            if (AsChar(import.DisplayInd.Flag) == 'Y')
            {
              local.ForCreateOcse157Verification.CaseNumber =
                entities.Case1.Number;
              local.ForCreateOcse157Verification.Comment =
                "Skipped-No Arrears coll during FY.";
              UseFnCreateOcse157Verification();
              MoveOcse157Verification2(local.Null1,
                local.ForCreateOcse157Verification);
            }

            continue;
          }

          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received
          // but a collection was definitely received.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 3. - Look for FDSO collection (coll type = 3 or 27)
          // -------------------------------------------------------------------
          foreach(var item1 in ReadCsePerson2())
          {
            local.CollectionDate.Date = local.Null1.CollCreatedDte;

            foreach(var item2 in ReadCollection10())
            {
              if (Equal(entities.Collection.CollectionDt,
                local.CollectionDate.Date))
              {
                continue;
              }

              local.CollectionDate.Date = entities.Collection.CollectionDt;

              // -------------------------------------------------------------
              // Check to see if all supp persons have AF, AFI, FC, FCI active
              // as of collection date?
              // -------------------------------------------------------------
              UseFnCheckForActiveAfFcPgm();

              // ----------------------------------------------------------
              // If all supp persons on Case are on assistance as of coll date,
              // we would never derive NA on any debt. Skip Collection.
              // ----------------------------------------------------------
              if (AsChar(local.Assistance.Flag) == 'Y')
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
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
                  local.ForCreateOcse157Verification.ObligorPersonNbr =
                    entities.ApCsePerson.Number;
                  local.ForCreateOcse157Verification.CaseNumber =
                    entities.Case1.Number;
                  local.ForCreateOcse157Verification.Comment =
                    "Skipped-Result of Adjusted FDSO Coll";
                  UseFnCreateOcse157Verification();
                  MoveOcse157Verification2(local.Null1,
                    local.ForCreateOcse157Verification);
                }

                continue;
              }
              else
              {
                // -- Continue...
              }

              // ----------------------------------------------------------
              // Atleast one person on case is not on assistance as of coll
              // date. Spin through each debt with due date <= coll date to
              // check if NA or NAI program is determined.
              // ----------------------------------------------------------
              // ------------------------------------------------------------
              // We will only look for debts with bal due as of FY end.
              // First, read debts with bal due as of today.
              // Later, we'll read debts with zero bal today but bal was due
              // on FY end.
              // -------------------------------------------------------------
              foreach(var item3 in ReadDebtDetailObligationObligationTypeCsePerson2())
                
              {
                UseFnDeterminePgmForDebtDetail();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.CaseNumber =
                      entities.Case1.Number;
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.Supp.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.DebtDetailDueDt =
                      entities.DebtDetail.DueDt;
                    local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                      entities.DebtDetail.BalanceDueAmt;
                    local.ForCreateOcse157Verification.ObligationSgi =
                      entities.Obligation.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.Comment =
                      "Skipped-NA/NAI derived on FDSO coll.";
                    UseFnCreateOcse157Verification();
                    MoveOcse157Verification2(local.Null1,
                      local.ForCreateOcse157Verification);
                  }

                  goto ReadEach6;
                }
              }

              // -----------------------------------------------------------------------
              // We either didn't find any debt with bal due or we found a debt
              // but no NA or NAI was owed on Collection date.
              // Now read for debts with zero bal today but where bal was
              // due on FY end.
              // -----------------------------------------------------------------------
              // -----------------------------------------------------------------------
              // Only do this if there is any collection/debt activity
              // for AP after FY end. There is no point in spinning through all
              // Zero debts if there is no activity at all for this AP.
              // -----------------------------------------------------------------------
              if (!ReadCollection2())
              {
                if (!ReadDebtAdjustment2())
                {
                  // -------------------------------------------------------------------------
                  // No collection/debt activity for AP after FY end.
                  // Process next FDSO Collection.
                  // --------------------------------------------------------------------------
                  continue;
                }
              }

              // -----------------------------------------------------------------------
              // We got here because there was some activity for AP since
              // FY end.
              // Now read for debts with zero bal today. Then determine if bal
              // was due on FY end.
              // -----------------------------------------------------------------------
              foreach(var item3 in ReadDebtDetailObligationObligationTypeCsePerson1())
                
              {
                // -----------------------------------------------------------------------
                // Check if debt has had any activity after FY end.
                // -----------------------------------------------------------------------
                if (!ReadCollection3())
                {
                  if (!ReadDebtAdjustment1())
                  {
                    // -------------------------------------------------------------------------
                    // No activity on debt since FY end. Process next debt.
                    // --------------------------------------------------------------------------
                    continue;
                  }
                }

                // -------------------------------------------------------------------------
                // You are looking at a debt with zero bal today but balance
                // was due on FY end.
                // --------------------------------------------------------------------------
                UseFnDeterminePgmForDebtDetail();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.CaseNumber =
                      entities.Case1.Number;
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.Supp.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.DebtDetailDueDt =
                      entities.DebtDetail.DueDt;
                    local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                      entities.DebtDetail.BalanceDueAmt;
                    local.ForCreateOcse157Verification.ObligationSgi =
                      entities.Obligation.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.Comment =
                      "Skipped-NA/NAI derived on FDSO coll.";
                    UseFnCreateOcse157Verification();
                    MoveOcse157Verification2(local.Null1,
                      local.ForCreateOcse157Verification);
                  }

                  goto ReadEach6;
                }
              }

              // ----------------------------------------------------------
              // We got here because we didn't derive NA or NAI on any debt
              // for current FDSO collection date. Process next FDSO coll.
              // ----------------------------------------------------------
            }

            local.CollectionDate.Date = local.Null1.CollCreatedDte;

            foreach(var item2 in ReadCollection9())
            {
              if (Equal(entities.Collection.CollectionDt,
                local.CollectionDate.Date))
              {
                continue;
              }

              local.CollectionDate.Date = entities.Collection.CollectionDt;

              // -------------------------------------------------------------
              // Check to see if all supp persons have AF, AFI, FC, FCI active
              // as of collection date?
              // -------------------------------------------------------------
              UseFnCheckForActiveAfFcPgm();

              // ----------------------------------------------------------
              // If all supp persons on Case are on assistance as of coll date,
              // we would never derive NA on any debt. Skip Collection.
              // ----------------------------------------------------------
              if (AsChar(local.Assistance.Flag) == 'Y')
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
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
                  local.ForCreateOcse157Verification.ObligorPersonNbr =
                    entities.ApCsePerson.Number;
                  local.ForCreateOcse157Verification.CaseNumber =
                    entities.Case1.Number;
                  local.ForCreateOcse157Verification.Comment =
                    "Skipped-Result of Adjusted FDSO Coll";
                  UseFnCreateOcse157Verification();
                  MoveOcse157Verification2(local.Null1,
                    local.ForCreateOcse157Verification);
                }

                continue;
              }
              else
              {
                // -- Continue...
              }

              // ----------------------------------------------------------
              // Atleast one person on case is not on assistance as of coll
              // date. Spin through each debt with due date <= coll date to
              // check if NA or NAI program is determined.
              // ----------------------------------------------------------
              // ------------------------------------------------------------
              // We will only look for debts with bal due as of FY end.
              // First, read debts with bal due as of today.
              // Later, we'll read debts with zero bal today but bal was due
              // on FY end.
              // -------------------------------------------------------------
              foreach(var item3 in ReadDebtDetailObligationObligationTypeCsePerson2())
                
              {
                UseFnDeterminePgmForDebtDetail();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.CaseNumber =
                      entities.Case1.Number;
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.Supp.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.DebtDetailDueDt =
                      entities.DebtDetail.DueDt;
                    local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                      entities.DebtDetail.BalanceDueAmt;
                    local.ForCreateOcse157Verification.ObligationSgi =
                      entities.Obligation.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.Comment =
                      "Skipped-NA/NAI derived on FDSO coll.";
                    UseFnCreateOcse157Verification();
                    MoveOcse157Verification2(local.Null1,
                      local.ForCreateOcse157Verification);
                  }

                  goto ReadEach6;
                }
              }

              // -----------------------------------------------------------------------
              // We either didn't find any debt with bal due or we found a debt
              // but no NA or NAI was owed on Collection date.
              // Now read for debts with zero bal today but where bal was
              // due on FY end.
              // -----------------------------------------------------------------------
              // -----------------------------------------------------------------------
              // Only do this if there is any collection/debt activity
              // for AP after FY end. There is no point in spinning through all
              // Zero debts if there is no activity at all for this AP.
              // -----------------------------------------------------------------------
              if (!ReadCollection2())
              {
                if (!ReadDebtAdjustment2())
                {
                  // -------------------------------------------------------------------------
                  // No collection/debt activity for AP after FY end.
                  // Process next FDSO Collection.
                  // --------------------------------------------------------------------------
                  continue;
                }
              }

              // -----------------------------------------------------------------------
              // We got here because there was some activity for AP since
              // FY end.
              // Now read for debts with zero bal today. Then determine if bal
              // was due on FY end.
              // -----------------------------------------------------------------------
              foreach(var item3 in ReadDebtDetailObligationObligationTypeCsePerson1())
                
              {
                // -----------------------------------------------------------------------
                // Check if debt has had any activity after FY end.
                // -----------------------------------------------------------------------
                if (!ReadCollection3())
                {
                  if (!ReadDebtAdjustment1())
                  {
                    // -------------------------------------------------------------------------
                    // No activity on debt since FY end. Process next debt.
                    // --------------------------------------------------------------------------
                    continue;
                  }
                }

                // -------------------------------------------------------------------------
                // You are looking at a debt with zero bal today but balance
                // was due on FY end.
                // --------------------------------------------------------------------------
                UseFnDeterminePgmForDebtDetail();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  if (AsChar(import.DisplayInd.Flag) == 'Y')
                  {
                    local.ForCreateOcse157Verification.CaseNumber =
                      entities.Case1.Number;
                    local.ForCreateOcse157Verification.SuppPersonNumber =
                      entities.Supp.Number;
                    local.ForCreateOcse157Verification.ObligorPersonNbr =
                      entities.ApCsePerson.Number;
                    local.ForCreateOcse157Verification.DebtDetailDueDt =
                      entities.DebtDetail.DueDt;
                    local.ForCreateOcse157Verification.DebtDetailBalanceDue =
                      entities.DebtDetail.BalanceDueAmt;
                    local.ForCreateOcse157Verification.ObligationSgi =
                      entities.Obligation.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.ObTypeSgi =
                      entities.ObligationType.SystemGeneratedIdentifier;
                    local.ForCreateOcse157Verification.Comment =
                      "Skipped-NA/NAI derived on FDSO coll.";
                    UseFnCreateOcse157Verification();
                    MoveOcse157Verification2(local.Null1,
                      local.ForCreateOcse157Verification);
                  }

                  goto ReadEach6;
                }
              }

              // ----------------------------------------------------------
              // We got here because we didn't derive NA or NAI on any debt
              // for current FDSO collection date. Process next FDSO coll.
              // ----------------------------------------------------------
            }

ReadEach6:
            ;
          }

          // ----------------------------------------------------------
          // We got here because we either didn't find any FDSO
          // collection or we found FDSO collections but none of the
          // debts derived NA or NAI.
          // We also know that non-NA arrears collection was definitely
          // received. No further processing is necessary. Count case!
          // ----------------------------------------------------------
          local.CountCase.Flag = "Y";

          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CollectionSgi =
              local.NonNa.SystemGeneratedIdentifier;
            local.ForCreateOcse157Verification.CollectionAmount =
              local.NonNa.Amount;
            local.ForCreateOcse157Verification.CollApplToCode =
              local.NonNa.AppliedToCode;
            local.ForCreateOcse157Verification.CollCreatedDte =
              Date(local.NonNa.CreatedTmst);
            local.ForCreateOcse157Verification.CollectionDte =
              local.NonNa.CollectionDt;
            local.ForCreateOcse157Verification.SuppPersonNumber =
              local.Supp.Number;
            local.ForCreateOcse157Verification.ObligorPersonNbr =
              local.Ap.Number;
          }

          local.ForCreateOcse157Verification.CaseNumber = entities.Case1.Number;
          local.ForCreateOcse157Verification.Column = "a";
          UseFnCreateOcse157Verification();
          MoveOcse157Verification2(local.Null1,
            local.ForCreateOcse157Verification);
        }

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          if (AsChar(import.DisplayInd.Flag) == 'Y')
          {
            local.ForCreateOcse157Verification.CaseNumber =
              entities.Case1.Number;
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
        ++local.Line29.Count;

        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "29 " + entities
            .Case1.Number;
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (local.Line29.Count, 6, 10);
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.LineNumber = "29";
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
    }

    // --------------------------------------------------
    // Processing complete for this line.
    // Take checkpoint and create ocse157_data records.
    // --------------------------------------------------
    local.ForCreateOcse157Data.LineNumber = "29";
    local.ForCreateOcse157Data.Column = "a";
    local.ForCreateOcse157Data.Number = local.Line29.Count;
    UseFnCreateOcse157Data();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "33 " + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "29";
      local.ForError.CaseNumber = "";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CreatedTmst = source.CreatedTmst;
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

  private void UseFnCheckForActiveAfFcPgm()
  {
    var useImport = new FnCheckForActiveAfFcPgm.Import();
    var useExport = new FnCheckForActiveAfFcPgm.Export();

    useImport.Case1.Assign(entities.Case1);
    useImport.AsOfDate.Date = local.CollectionDate.Date;

    Call(FnCheckForActiveAfFcPgm.Execute, useImport, useExport);

    local.CollectionDate.Date = useImport.AsOfDate.Date;
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

    useImport.SupportedPerson.Number = entities.Supp.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.Collection.Date = local.CollectionDate.Date;

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

  private bool ReadCaseRole()
  {
    entities.ChOrAr.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.Supp.Number);
      },
      (db, reader) =>
      {
        entities.ChOrAr.CasNumber = db.GetString(reader, 0);
        entities.ChOrAr.CspNumber = db.GetString(reader, 1);
        entities.ChOrAr.Type1 = db.GetString(reader, 2);
        entities.ChOrAr.Identifier = db.GetInt32(reader, 3);
        entities.ChOrAr.StartDate = db.GetNullableDate(reader, 4);
        entities.ChOrAr.EndDate = db.GetNullableDate(reader, 5);
        entities.ChOrAr.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChOrAr.Type1);
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

  private IEnumerable<bool> ReadCollection10()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection10",
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

  private IEnumerable<bool> ReadCollection11()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection11",
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

  private bool ReadCollection2()
  {
    entities.AfterFy.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
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

  private bool ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.AfterFy.Populated = false;

    return Read("ReadCollection3",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetInt32(command, "otyId", entities.DebtDetail.OtyType);
        db.SetInt32(command, "obgId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
        db.SetInt32(command, "otrId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
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

  private IEnumerable<bool> ReadCollection4()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection4",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
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

  private IEnumerable<bool> ReadCollection5()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection5",
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

  private IEnumerable<bool> ReadCollection6()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection6",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
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

  private IEnumerable<bool> ReadCollection7()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection7",
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

  private IEnumerable<bool> ReadCollection8()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection8",
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

  private IEnumerable<bool> ReadCollection9()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection9",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjDt", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
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

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supp.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supp.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment1",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.DebtDetail.OtyType);
        db.SetInt32(
          command, "obgPGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "otrPType", entities.DebtDetail.OtrType);
        db.SetInt32(
          command, "otrPGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "cpaPType", entities.DebtDetail.CpaType);
        db.SetString(command, "cspPNumber", entities.DebtDetail.CspNumber);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 6);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 7);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 8);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 9);
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

  private bool ReadDebtAdjustment2()
  {
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
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
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 6);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 7);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 8);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 9);
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

  private IEnumerable<bool> ReadDebtDetailObligationObligationTypeCsePerson1()
  {
    entities.Supp.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationObligationTypeCsePerson1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.CollectionDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
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
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 9);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 15);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 16);
        entities.ObligationType.Code = db.GetString(reader, 17);
        entities.ObligationType.Classification = db.GetString(reader, 18);
        entities.Supp.Number = db.GetString(reader, 19);
        entities.Supp.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationObligationTypeCsePerson2()
  {
    entities.Supp.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailObligationObligationTypeCsePerson2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.CollectionDate.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
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
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 9);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 15);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 16);
        entities.ObligationType.Code = db.GetString(reader, 17);
        entities.ObligationType.Classification = db.GetString(reader, 18);
        entities.Supp.Number = db.GetString(reader, 19);
        entities.Supp.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

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
    private Ocse157Verification from;
    private Ocse157Verification to;
    private Common displayInd;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
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
    /// A value of NonNa.
    /// </summary>
    [JsonPropertyName("nonNa")]
    public Collection NonNa
    {
      get => nonNa ??= new();
      set => nonNa = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
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
    /// A value of CollFound.
    /// </summary>
    [JsonPropertyName("collFound")]
    public Common CollFound
    {
      get => collFound ??= new();
      set => collFound = value;
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
    /// A value of Line29.
    /// </summary>
    [JsonPropertyName("line29")]
    public Common Line29
    {
      get => line29 ??= new();
      set => line29 = value;
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
    /// A value of CountCase.
    /// </summary>
    [JsonPropertyName("countCase")]
    public Common CountCase
    {
      get => countCase ??= new();
      set => countCase = value;
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
    /// A value of Minimum.
    /// </summary>
    [JsonPropertyName("minimum")]
    public Case1 Minimum
    {
      get => minimum ??= new();
      set => minimum = value;
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
    /// A value of TbdLocalRestart.
    /// </summary>
    [JsonPropertyName("tbdLocalRestart")]
    public PaymentRequest TbdLocalRestart
    {
      get => tbdLocalRestart ??= new();
      set => tbdLocalRestart = value;
    }

    private Collection nonNa;
    private CsePerson supp;
    private CsePerson ap;
    private DateWorkArea collectionDate;
    private Common assistance;
    private Common collFound;
    private Ocse157Verification forCreateOcse157Verification;
    private Ocse157Data forCreateOcse157Data;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restart;
    private Common line29;
    private Case1 prev;
    private Common countCase;
    private Ocse157Verification null1;
    private Common commitCnt;
    private Ocse157Data max;
    private Case1 minimum;
    private Ocse157Verification forError;
    private Program program;
    private PaymentRequest tbdLocalRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

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
    /// A value of AfterFy.
    /// </summary>
    [JsonPropertyName("afterFy")]
    public Collection AfterFy
    {
      get => afterFy ??= new();
      set => afterFy = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction debit;
    private DisbursementTransaction credit;
    private PaymentRequest paymentRequest;
    private Collection adjusted;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction debtAdjustment;
    private Collection afterFy;
    private CsePerson supp;
    private Case1 case1;
    private Ocse157Data ocse157Data;
    private Ocse157Verification ocse157Verification;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole chOrAr;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction debt;
    private CsePersonAccount obligor;
    private CsePersonAccount supported;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private DebtDetail debtDetail;
  }
#endregion
}
