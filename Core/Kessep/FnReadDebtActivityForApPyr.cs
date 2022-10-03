// Program: FN_READ_DEBT_ACTIVITY_FOR_AP_PYR, ID: 372116198, model: 746.
// Short name: SWE00550
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_DEBT_ACTIVITY_FOR_AP_PYR.
/// </para>
/// <para>
/// RESP: FINANCE
/// This AB will build a group view of formatted text lines and key values 
/// representing all Debts for a AP/Payor
/// and, optionally, all activity associated with each Debt (i.e. Collections, 
/// Debt Adjustments).
/// </para>
/// </summary>
[Serializable]
public partial class FnReadDebtActivityForApPyr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_DEBT_ACTIVITY_FOR_AP_PYR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadDebtActivityForApPyr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadDebtActivityForApPyr.
  /// </summary>
  public FnReadDebtActivityForApPyr(IContext context, Import import,
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
    // =================================================
    // RBM 08/01/1997
    // Modified the display logic to display Collec-
    // tions and Collection-Adjustments properly.
    // The previous modification (by Unknown) resul-
    // ted in the same record appearing as a Collec-
    // tion and Collection-Adjustment row even though
    // the Collection was not Adjusted.
    // =================================================
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Syed Hasan,MTW   01-09-1998   Prob Report 31851
    // Modified to display information as required on
    // the problem report.
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ======================================================================================
    // RBM  02/20/1998  Fixed Prob. Rpts 37134, 37631, 37638, 37946, 38324, 
    // 38426
    // ADOTY 03/30/1999 Replace calls for summary totals to use new real-time 
    // AB's.
    // ======================================================================================
    // ======================================================================================
    // VMadhira  09/18/00     SET the Summary_Totals based on the AP#  and  '
    // Court Ord#'  in the absense of Ob_ID#. (ie. Read all Obligations for the
    // Court_Ord#)
    // ======================================================================================
    // ======================================================================================
    // Arun Mathias 02/17/09  CQ#9065 Display collections for all debts.
    // ======================================================================================
    // -- 09/20/2010  GVandy  CQ#22040 Emergency change due to optimizer 
    // choosing innefficient index path when rebound due to DB2 upgrade.
    local.NextPageDisplayLineInd.Flag = import.NextPageDisplayLinInd.Flag;
    local.Current.Date = import.Current.Date;
    local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
    local.NextPageCollection.SystemGeneratedIdentifier =
      import.NextPageCollection.SystemGeneratedIdentifier;
    local.Current.YearMonth = UseCabGetYearMonthFromDate2();

    if (Lt(import.NextPageObligationTransactionRln.CreatedTmst,
      import.Max.Timestamp) || import
      .NextPageCollection.SystemGeneratedIdentifier > 0)
    {
      local.SuppressDebtHeading.Flag = "Y";
    }

    UseFnHardcodedDebtDistribution();

    // ***** MAIN-LINE AREA *****
    if (ReadCsePerson1())
    {
      // #################################################
      // ##
      // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
      // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
      // ##
      // #################################################
      if (AsChar(entities.ObligorCsePerson.Type1) == 'C')
      {
        export.CsePersonsWorkSet.Number = entities.ObligorCsePerson.Number;
        UseEabReadCsePerson();
        UseSiFormatCsePersonName();
      }
      else
      {
        export.CsePersonsWorkSet.FormattedName =
          entities.ObligorCsePerson.OrganizationName ?? Spaces(33);
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ***---  select only
    if (ReadCsePersonAccount())
    {
      // : Continue Processing . . .
    }
    else
    {
      ExitState = "CSE_PERSON_NOT_AN_OBLIGOR";

      return;
    }

    // =================================================
    // 2/18/00 - b adams  -  PR# 82600: Explicit scrolling.
    // =================================================
    if (import.NextPageObligation.SystemGeneratedIdentifier == 0)
    {
      local.NextPageObligation.SystemGeneratedIdentifier = 999;
    }
    else
    {
      local.NextPageObligation.SystemGeneratedIdentifier =
        import.NextPageObligation.SystemGeneratedIdentifier;
    }

    if (Equal(import.NextPageObligationType.Code, "VOL"))
    {
      local.NextPageSearchTo.Date = local.Zero.Date;
    }
    else
    {
      local.NextPageSearchTo.Date = import.NextPageSearchTo.Date;
    }

    // #################################################
    // ##
    // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
    // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
    // ##
    // #################################################
    // ====================================================================================
    // Vmadhira  09/18/2000      Screen_owed_amounts will be SET based on the 
    // filters entered on the screen.
    // ====================================================================================
    if (IsEmpty(import.SearchLegalAction.StandardNumber))
    {
      UseFnComputeSummaryTotals2();
    }
    else
    {
      foreach(var item in ReadObligation())
      {
        UseFnComputeSummaryTotals1();

        switch(AsChar(entities.Obligation.PrimarySecondaryCode))
        {
          case 'P':
            export.ScreenOwedAmounts.TotalAmountOwed += local.
              ObligationScreenOwedAmounts.TotalAmountOwed;
            export.ScreenOwedAmounts.InterestAmountOwed += local.
              ObligationScreenOwedAmounts.InterestAmountOwed;
            export.ScreenOwedAmounts.CurrentAmountOwed += local.
              ObligationScreenOwedAmounts.CurrentAmountOwed;
            export.ScreenOwedAmounts.ArrearsAmountOwed += local.
              ObligationScreenOwedAmounts.ArrearsAmountOwed;

            break;
          case 'S':
            // ----------------------------------------------------------
            //        Continue processing...........
            // ---------------------------------------------------------
            break;
          case 'J':
            // ===============================================================================
            // Count the totals only once in case of Joint/several Obligations.
            //                                                          
            // VMadhira (09/18/00)
            // ===============================================================================
            export.ScreenOwedAmounts.TotalAmountOwed += local.
              ObligationScreenOwedAmounts.TotalAmountOwed;
            export.ScreenOwedAmounts.InterestAmountOwed += local.
              ObligationScreenOwedAmounts.InterestAmountOwed;
            export.ScreenOwedAmounts.CurrentAmountOwed += local.
              ObligationScreenOwedAmounts.CurrentAmountOwed;
            export.ScreenOwedAmounts.ArrearsAmountOwed += local.
              ObligationScreenOwedAmounts.ArrearsAmountOwed;

            break;
          default:
            export.ScreenOwedAmounts.TotalAmountOwed += local.
              ObligationScreenOwedAmounts.TotalAmountOwed;
            export.ScreenOwedAmounts.InterestAmountOwed += local.
              ObligationScreenOwedAmounts.InterestAmountOwed;
            export.ScreenOwedAmounts.CurrentAmountOwed += local.
              ObligationScreenOwedAmounts.CurrentAmountOwed;
            export.ScreenOwedAmounts.ArrearsAmountOwed += local.
              ObligationScreenOwedAmounts.ArrearsAmountOwed;

            break;
        }
      }
    }

    // ---------------------------------------------
    // MTW - Chayan 04/04/1997 Change Start
    // Read has been changed to read data for the month and year entered
    // instead of from and to date
    // ---------------------------------------------
    local.ToBeSorted.Index = -1;
    local.SortOrder.Index = -1;

    // =================================================
    // 11/5/99 - b adams  -  Removed
    //   "OR obligation_type classification = "O"
    //    OR obligation_type classification = "S""
    //                     and
    //   "OR obligation_transaction debt_type = "V"
    //   from both Read Each actions since those values are not
    //   ever set in KESSEP.
    // 2/18/00 - bud adams  -  PR# 82600: Added 'next_page'
    //   selection logic to support 'infinite scrolling'.
    // =================================================
    if (AsChar(import.ListDebtsWithAmtOwed.SelectChar) == 'Y')
    {
      // <<<  List only Debts with Balance Owed > 0  >>
      foreach(var item in ReadObligationObligationTransactionDebtDetailObligationType1())
        
      {
        // =================================================
        // 3/23/00 - b adams  -  PR# 81829: This version of the data
        //   retrieval logic will always be for when Obligation_Type and
        //   Obligation are not valued.
        // =================================================
        if (import.ObligationTransaction.SystemGeneratedIdentifier > 0)
        {
          if (entities.ObligationTransaction.SystemGeneratedIdentifier != import
            .ObligationTransaction.SystemGeneratedIdentifier)
          {
            continue;
          }
        }

        // =================================================
        // 2/19/00 - Bud Adams  -  IF this is the same obligation as the
        //   last time through, no need to do this.
        // =================================================
        if (local.Previous.SystemGeneratedIdentifier == entities
          .Obligation.SystemGeneratedIdentifier)
        {
        }
        else
        {
          local.Previous.SystemGeneratedIdentifier =
            entities.Obligation.SystemGeneratedIdentifier;

          if (!IsEmpty(import.SearchLegalAction.StandardNumber))
          {
            // ***---  select only
            if (ReadLegalAction1())
            {
              local.LegalAction.Assign(entities.LegalAction);
            }
            else
            {
              local.LegalAction.Assign(local.InitializedLegalAction);
              local.Previous.SystemGeneratedIdentifier = 0;

              continue;
            }
          }
          else
          {
            // ***---  select only
            if (ReadLegalAction2())
            {
              local.LegalAction.Assign(entities.LegalAction);
            }
            else
            {
              local.LegalAction.Assign(local.InitializedLegalAction);
            }
          }
        }

        if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
        {
          // ***---  select only
          if (ReadCsePerson2())
          {
            local.Supported.Assign(local.InitializedCsePersonsWorkSet);
            local.Supported.Number = entities.SupportedPerson.Number;
            ExitState = "ACO_NN0000_ALL_OK";

            // #################################################
            // ##
            // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
            // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
            // ##
            // #################################################
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.Supported.FirstName = "ZZZZZZZZZZZZ";
              local.Supported.MiddleInitial = "Z";
              local.Supported.LastName = "ZZZZZZZZZZZZZZZZZ";
              local.Supported.FormattedName = "** ADABAS UNAVAILABLE **";
            }
            else if (IsEmpty(local.Supported.FormattedName))
            {
              if (Lt(local.Supported.LastName, "AAAAAAAAAAAAAAAAA") || Lt
                ("ZZZZZZZZZZZZZZZZZ", local.Supported.LastName))
              {
                local.Supported.FormattedName = local.Supported.LastName;
              }
            }

            // =================================================
            // 7/27/99 - bud adams  -  for obligations that exist due to legal
            //   actions, the Case and Worker are determined differently.
            //   Also, for Case information, it doesn't matter if the supported
            //   person is currently active on that case or not.
            //   Added the IF, the READ, and the fn_read_case_and_wrkr_
            //   from_legal.
            // 10/21/99 - E. Parker - Made changes to read Case No and Worker Id
            // similar to Disbursements.  Implemented new Action Block.
            // =================================================
            // #################################################
            // ##
            // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
            // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
            // ##
            // #################################################
            UseFnDetCaseNoAndWrkrForDbt();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            // #################################################
            // ##
            // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
            // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
            // ##
            // #################################################
            UseFnDeterminePgmForDebtDetail();
            local.ProgramScreenAttributes.ProgramTypeInd =
              local.DistributionPgm.Code;
          }
          else
          {
            local.Supported.FormattedName = "SUPPORTED PERSON NF";
          }

          local.DateWorkArea.Date = entities.DebtDetail.DueDt;

          // ----> Store the values in a unsorted local group view.
          //      -Syed Hasan, 01-03-98
          ++local.ToBeSorted.Index;
          local.ToBeSorted.CheckSize();

          local.ToBeSorted.Update.ToBeSortedSeqNo.Count =
            local.ToBeSorted.Index + 1;
          MoveObligation(entities.Obligation,
            local.ToBeSorted.Update.ToBeSortedObligation);
          MoveObligationType1(entities.ObligationType,
            local.ToBeSorted.Update.ToBeSortedObligationType);
          local.ToBeSorted.Update.ToBeSortedLegalAction.
            Assign(local.LegalAction);
          local.ToBeSorted.Update.ToBeSortedObligationTransaction.Assign(
            entities.ObligationTransaction);
          MoveDebtDetail1(entities.DebtDetail,
            local.ToBeSorted.Update.ToBeSortedDebtDetail);
          MoveCsePerson(entities.SupportedPerson,
            local.ToBeSorted.Update.SupportedToBeSortedCsePerson);
          local.ToBeSorted.Update.SupportedToBeSortedCsePersonsWorkSet.Assign(
            local.Supported);
          local.ToBeSorted.Update.ToBeSortedProgramScreenAttributes.
            ProgramTypeInd = local.ProgramScreenAttributes.ProgramTypeInd;
          local.ToBeSorted.Update.ToBeSortedDprProgram.ProgramState =
            local.DprProgram.ProgramState;
          local.ToBeSorted.Update.ToBeSortedCase.Number = local.Case1.Number;
          MoveServiceProvider(local.ServiceProvider,
            local.ToBeSorted.Update.ToBeSortedServiceProvider);

          // ----> Store the sorting criteria values in a local group view as 
          // they are captured.
          //      -Syed Hasan, 01-03-98
          ++local.SortOrder.Index;
          local.SortOrder.CheckSize();

          local.SortOrder.Update.SortOrderSeqNo.Count =
            local.SortOrder.Index + 1;
          local.SortOrder.Update.SortOrderObligationType.Code =
            entities.ObligationType.Code;
          local.SortOrder.Update.SortOrderDebtDetail.DueDt =
            entities.DebtDetail.DueDt;
          MoveCsePersonsWorkSet3(local.Supported,
            local.SortOrder.Update.SortOrderSupported);
        }
        else if (AsChar(entities.ObligationTransaction.DebtType) == 'D')
        {
          // ----> Store the values in a unsorted local group view.
          //      -Syed Hasan, 01-03-98
          ++local.ToBeSorted.Index;
          local.ToBeSorted.CheckSize();

          local.ToBeSorted.Update.ToBeSortedSeqNo.Count =
            local.ToBeSorted.Index + 1;
          MoveObligation(entities.Obligation,
            local.ToBeSorted.Update.ToBeSortedObligation);
          MoveObligationType1(entities.ObligationType,
            local.ToBeSorted.Update.ToBeSortedObligationType);
          local.ToBeSorted.Update.ToBeSortedLegalAction.
            Assign(local.LegalAction);
          local.ToBeSorted.Update.ToBeSortedObligationTransaction.Assign(
            entities.ObligationTransaction);
          MoveDebtDetail1(entities.DebtDetail,
            local.ToBeSorted.Update.ToBeSortedDebtDetail);

          if (ReadServiceProvider())
          {
            MoveServiceProvider(entities.ServiceProvider,
              local.ToBeSorted.Update.ToBeSortedServiceProvider);
          }

          // ----> Store the sorting criteria values in a local group view as 
          // they are captured.
          //      -Syed Hasan, 01-03-98
          ++local.SortOrder.Index;
          local.SortOrder.CheckSize();

          local.SortOrder.Update.SortOrderSeqNo.Count =
            local.SortOrder.Index + 1;
          local.SortOrder.Update.SortOrderObligationType.Code =
            entities.ObligationType.Code;
          local.SortOrder.Update.SortOrderDebtDetail.DueDt =
            entities.DebtDetail.DueDt;
        }

        if (local.SortOrder.Index + 1 == Local.SortOrderGroup.Capacity)
        {
          break;
        }
      }
    }
    else
    {
      // <<< List all Debts irrespective of any Balance owed or not >>
      local.ToBeSorted.Index = -1;
      local.SortOrder.Index = -1;

      // =================================================
      // 3/4/00 - b adams  -  PR# 82600: Added selection criteria
      //   to pick up at the 'next record' for the next group view of
      //   data when the user does a PREV that goes beyond the
      //   60th row.
      // =================================================
      foreach(var item in ReadObligationObligationTransactionDebtDetailObligationType2())
        
      {
        // =================================================
        // 3/23/00 - b adams  -  PR# 81829: This version of the data
        //   retrieval logic will always be for when Obligation_Type and
        //   Obligation are not valued.
        // =================================================
        if (import.ObligationTransaction.SystemGeneratedIdentifier > 0)
        {
          if (entities.ObligationTransaction.SystemGeneratedIdentifier != import
            .ObligationTransaction.SystemGeneratedIdentifier)
          {
            continue;
          }
        }

        if (local.Previous.SystemGeneratedIdentifier == entities
          .Obligation.SystemGeneratedIdentifier)
        {
        }
        else
        {
          local.Previous.SystemGeneratedIdentifier =
            entities.Obligation.SystemGeneratedIdentifier;

          if (!IsEmpty(import.SearchLegalAction.StandardNumber))
          {
            // ***---  select only
            if (ReadLegalAction1())
            {
              local.LegalAction.Assign(entities.LegalAction);
            }
            else
            {
              local.LegalAction.Assign(local.InitializedLegalAction);
              local.Previous.SystemGeneratedIdentifier = 0;

              continue;
            }
          }
          else
          {
            // ***---  select only
            if (ReadLegalAction2())
            {
              local.LegalAction.Assign(entities.LegalAction);
            }
            else
            {
              local.LegalAction.Assign(local.InitializedLegalAction);
            }
          }
        }

        if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
        {
          // ***---  select only
          if (ReadCsePerson2())
          {
            local.Supported.Assign(local.InitializedCsePersonsWorkSet);
            local.Supported.Number = entities.SupportedPerson.Number;
            ExitState = "ACO_NN0000_ALL_OK";

            // #################################################
            // ##
            // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
            // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
            // ##
            // #################################################
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.Supported.FirstName = "ZZZZZZZZZZZZ";
              local.Supported.MiddleInitial = "Z";
              local.Supported.LastName = "ZZZZZZZZZZZZZZZZZ";
              local.Supported.FormattedName = "** ADABAS UNAVAILABLE **";
            }
            else if (IsEmpty(local.Supported.FormattedName))
            {
              if (Lt(local.Supported.LastName, "AAAAAAAAAAAAAAAAA") || Lt
                ("ZZZZZZZZZZZZZZZZZ", local.Supported.LastName))
              {
                local.Supported.FormattedName = local.Supported.LastName;
              }
            }

            // =================================================
            // 7/27/99 - bud adams  -  for obligations that exist due to legal
            //   actions, the Case and Worker are determined differently.
            //   Also, for Case information, it doesn't matter if the supported
            //   person is currently active on that case or not.
            //   Added the IF, the READ, and the fn_read_case_and_wrkr_
            //   from_legal.
            // =================================================
            UseFnDetCaseNoAndWrkrForDbt();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }

            UseFnDeterminePgmForDebtDetail();
            local.ProgramScreenAttributes.ProgramTypeInd =
              local.DistributionPgm.Code;
          }
          else
          {
            local.Supported.FormattedName = "SUPPORTED PERSON NF";
          }

          local.DateWorkArea.Date = entities.DebtDetail.DueDt;

          // ----> Store the values in a unsorted local group view.
          //      -Syed Hasan, 01-03-98
          ++local.ToBeSorted.Index;
          local.ToBeSorted.CheckSize();

          local.ToBeSorted.Update.ToBeSortedSeqNo.Count =
            local.ToBeSorted.Index + 1;
          MoveObligation(entities.Obligation,
            local.ToBeSorted.Update.ToBeSortedObligation);
          MoveObligationType1(entities.ObligationType,
            local.ToBeSorted.Update.ToBeSortedObligationType);
          local.ToBeSorted.Update.ToBeSortedLegalAction.
            Assign(local.LegalAction);
          local.ToBeSorted.Update.ToBeSortedObligationTransaction.Assign(
            entities.ObligationTransaction);
          MoveDebtDetail1(entities.DebtDetail,
            local.ToBeSorted.Update.ToBeSortedDebtDetail);
          MoveCsePerson(entities.SupportedPerson,
            local.ToBeSorted.Update.SupportedToBeSortedCsePerson);
          local.ToBeSorted.Update.SupportedToBeSortedCsePersonsWorkSet.Assign(
            local.Supported);
          local.ToBeSorted.Update.ToBeSortedProgramScreenAttributes.
            ProgramTypeInd = local.ProgramScreenAttributes.ProgramTypeInd;
          local.ToBeSorted.Update.ToBeSortedDprProgram.ProgramState =
            local.DprProgram.ProgramState;
          local.ToBeSorted.Update.ToBeSortedCase.Number = local.Case1.Number;
          MoveServiceProvider(local.ServiceProvider,
            local.ToBeSorted.Update.ToBeSortedServiceProvider);

          // ----> Store the sorting criteria values in a local group view as 
          // they are captured.
          //      -Syed Hasan, 01-03-98
          ++local.SortOrder.Index;
          local.SortOrder.CheckSize();

          local.SortOrder.Update.SortOrderSeqNo.Count =
            local.SortOrder.Index + 1;
          local.SortOrder.Update.SortOrderObligationType.Code =
            entities.ObligationType.Code;
          local.SortOrder.Update.SortOrderDebtDetail.DueDt =
            entities.DebtDetail.DueDt;
          MoveCsePersonsWorkSet3(local.Supported,
            local.SortOrder.Update.SortOrderSupported);
        }
        else if (AsChar(entities.ObligationTransaction.DebtType) == 'D')
        {
          // ----> Store the values in a unsorted local group view.
          //      -Syed Hasan, 01-03-98
          ++local.ToBeSorted.Index;
          local.ToBeSorted.CheckSize();

          local.ToBeSorted.Update.ToBeSortedSeqNo.Count =
            local.ToBeSorted.Index + 1;
          MoveObligation(entities.Obligation,
            local.ToBeSorted.Update.ToBeSortedObligation);
          MoveObligationType1(entities.ObligationType,
            local.ToBeSorted.Update.ToBeSortedObligationType);
          local.ToBeSorted.Update.ToBeSortedLegalAction.
            Assign(local.LegalAction);
          local.ToBeSorted.Update.ToBeSortedObligationTransaction.Assign(
            entities.ObligationTransaction);
          MoveDebtDetail1(entities.DebtDetail,
            local.ToBeSorted.Update.ToBeSortedDebtDetail);

          if (ReadServiceProvider())
          {
            MoveServiceProvider(entities.ServiceProvider,
              local.ToBeSorted.Update.ToBeSortedServiceProvider);
          }

          // ----> Store the sorting criteria values in a local group view as 
          // they are captured.
          //      -Syed Hasan, 01-03-98
          ++local.SortOrder.Index;
          local.SortOrder.CheckSize();

          local.SortOrder.Update.SortOrderSeqNo.Count =
            local.SortOrder.Index + 1;
          local.SortOrder.Update.SortOrderObligationType.Code =
            entities.ObligationType.Code;
          local.SortOrder.Update.SortOrderDebtDetail.DueDt =
            entities.DebtDetail.DueDt;
        }

        if (local.SortOrder.Index + 1 == Local.SortOrderGroup.Capacity)
        {
          break;
        }
      }
    }

    for(local.ToBeSorted.Index = 0; local.ToBeSorted.Index < local
      .ToBeSorted.Count; ++local.ToBeSorted.Index)
    {
      if (!local.ToBeSorted.CheckSize())
      {
        break;
      }

      local.Sorted.Index = local.ToBeSorted.Index;
      local.Sorted.CheckSize();

      MoveObligation(local.ToBeSorted.Item.ToBeSortedObligation,
        local.Sorted.Update.SortedObligation);
      local.Sorted.Update.SortedObligationType.Assign(
        local.ToBeSorted.Item.ToBeSortedObligationType);
      local.Sorted.Update.SortedLegalAction.Assign(
        local.ToBeSorted.Item.ToBeSortedLegalAction);
      local.Sorted.Update.SortedObligationTransaction.Assign(
        local.ToBeSorted.Item.ToBeSortedObligationTransaction);
      local.Sorted.Update.SortedDebtDetail.Assign(
        local.ToBeSorted.Item.ToBeSortedDebtDetail);
      local.Sorted.Update.SupportedSortedCsePerson.Assign(
        local.ToBeSorted.Item.SupportedToBeSortedCsePerson);
      local.Sorted.Update.SupportedSortedCsePersonsWorkSet.Assign(
        local.ToBeSorted.Item.SupportedToBeSortedCsePersonsWorkSet);
      local.Sorted.Update.SortedProgramScreenAttributes.ProgramTypeInd =
        local.ToBeSorted.Item.ToBeSortedProgramScreenAttributes.ProgramTypeInd;
      local.Sorted.Update.SortedDprProgram.ProgramState =
        local.ToBeSorted.Item.ToBeSortedDprProgram.ProgramState;
      local.Sorted.Update.SortedCase.Number =
        local.ToBeSorted.Item.ToBeSortedCase.Number;
      MoveServiceProvider(local.ToBeSorted.Item.ToBeSortedServiceProvider,
        local.Sorted.Update.SortedServiceProvider);
    }

    local.ToBeSorted.CheckIndex();

    // =================================================
    // 3/4/00 - b adams - PR# 82600: Explicit scrolling.  Added
    //   debt_detail due_dt to the export group view so we know
    //   where to pick up the next chunk of data.  That value needs
    //   to be part of every detail line that goes to the screen.
    // =================================================
    // ----> Read additional debt activity values and populate the export views.
    //      -Syed Hasan, 01-03-98
    if (!local.Sorted.IsEmpty)
    {
      export.Export1.Index = -1;
      local.Sorted.Index = 0;

      for(var limit = local.Sorted.Count; local.Sorted.Index < limit; ++
        local.Sorted.Index)
      {
        if (!local.Sorted.CheckSize())
        {
          break;
        }

        // *** Populate the values used in the Format Debt Line 1 cab ***
        local.ObligationScreenOwedAmounts.InterestAmountOwed =
          local.Sorted.Item.SortedDebtDetail.InterestBalanceDueAmt.
            GetValueOrDefault();

        if (AsChar(local.Sorted.Item.SortedObligationType.Classification) == 'A'
          )
        {
          // --- Accruing obligations
          local.DebtDue.Date = local.Sorted.Item.SortedDebtDetail.DueDt;
          local.DebtDue.YearMonth = UseCabGetYearMonthFromDate1();

          if (local.DebtDue.YearMonth < local.Current.YearMonth)
          {
            local.ObligationScreenOwedAmounts.ArrearsAmountOwed =
              local.Sorted.Item.SortedDebtDetail.BalanceDueAmt;
            local.ObligationScreenOwedAmounts.CurrentAmountOwed = 0;
          }
          else
          {
            local.ObligationScreenOwedAmounts.CurrentAmountOwed =
              local.Sorted.Item.SortedDebtDetail.BalanceDueAmt;
            local.ObligationScreenOwedAmounts.ArrearsAmountOwed = 0;
          }
        }
        else
        {
          // --- non accruing, recoveries, fees etc
          local.DebtDue.Date = local.Zero.Date;
          local.DebtDue.YearMonth = 0;
          local.ObligationScreenOwedAmounts.ArrearsAmountOwed =
            local.Sorted.Item.SortedDebtDetail.BalanceDueAmt;
          local.ObligationScreenOwedAmounts.CurrentAmountOwed = 0;
        }

        local.ObligationScreenOwedAmounts.TotalAmountOwed =
          local.ObligationScreenOwedAmounts.ArrearsAmountOwed + local
          .ObligationScreenOwedAmounts.CurrentAmountOwed + local
          .ObligationScreenOwedAmounts.InterestAmountOwed;

        // ---------------------------------------------
        // The EAB moves debt_detail balance due amt to screen field Amt-Due. 
        // But we want the screen field to show the original obligation
        // transaction amount. Until the EAB is fixed use a local view to get
        // around the problem. The EAB does not use any field other than this
        // field.
        // ---------------------------------------------
        local.DebtDetail.Assign(local.Sorted.Item.SortedDebtDetail);
        local.DebtDetail.BalanceDueAmt =
          local.Sorted.Item.SortedObligationTransaction.Amount;

        // ===============================================
        // For infinite scrolling, when we return for the next group of
        //   display lines, we don't want the base obligation line to be
        //   displayed again if it already has been.  Incoming 'next-page'
        //   data is based on display line(71).
        // ===============================================
        if (import.NextPageObligation.SystemGeneratedIdentifier == local
          .Sorted.Item.SortedObligation.SystemGeneratedIdentifier && Equal
          (import.NextPageObligationTransaction.Type1, "DE") && import
          .NextPageObligationTransaction.SystemGeneratedIdentifier == local
          .Sorted.Item.SortedObligationTransaction.SystemGeneratedIdentifier &&
          AsChar(local.NextPageDisplayLineInd.Flag) == 'N' || AsChar
          (local.SuppressDebtHeading.Flag) == 'Y')
        {
        }
        else
        {
          // : Format DEBT line 1 using External here!!!!
          // #################################################
          // ##
          // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
          // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
          // ##
          // #################################################
          UseEabFormatDebtDetailLine1();

          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.DtlObligationType.Assign(
            local.Sorted.Item.SortedObligationType);
          MoveObligation(local.Sorted.Item.SortedObligation,
            export.Export1.Update.DtlObligation);
          MoveObligationTransaction1(local.Sorted.Item.
            SortedObligationTransaction,
            export.Export1.Update.DtlObligationTransaction);
          export.Export1.Update.DtlSupported.Number =
            local.Sorted.Item.SupportedSortedCsePersonsWorkSet.Number;
          MoveLegalAction(local.Sorted.Item.SortedLegalAction,
            export.Export1.Update.DtlLegalAction);
          export.Export1.Update.DtlListScreenWorkArea.TextLine76 =
            local.ScreenWorkArea.TextLine76;
          export.Export1.Update.DtlDisplayLineInd.Flag = "Y";
          export.Export1.Update.DtlDebtDetail.DueDt =
            local.Sorted.Item.SortedDebtDetail.DueDt;
          export.Export1.Update.DtlAdjusted.CreatedTmst = import.Max.Timestamp;
          local.NextPageDisplayLineInd.Flag = "";

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            return;
          }
        }

        if (!IsEmpty(local.Sorted.Item.SupportedSortedCsePersonsWorkSet.Number) ||
          !IsEmpty(local.Sorted.Item.SortedLegalAction.StandardNumber))
        {
          // ===============================================
          // For infinite scrolling, when we return for the next group of
          //   display lines, we don't want the base obligation line to be
          //   displayed again if it already has been.  Incoming 'next-page'
          //   data is based on display line(71).
          // ===============================================
          if (import.NextPageObligation.SystemGeneratedIdentifier == local
            .Sorted.Item.SortedObligation.SystemGeneratedIdentifier && Equal
            (import.NextPageObligationTransaction.Type1, "DE") && import
            .NextPageObligationTransaction.SystemGeneratedIdentifier == local
            .Sorted.Item.SortedObligationTransaction.
              SystemGeneratedIdentifier && AsChar
            (local.NextPageDisplayLineInd.Flag) == 'Y' || AsChar
            (local.SuppressDebtHeading.Flag) == 'Y')
          {
          }
          else
          {
            local.ScreenWorkArea.TextLine76 = "";

            // : Format DEBT line 2 using External here!!!!
            // #################################################
            // ##
            // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
            // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
            // ##
            // #################################################
            UseEabFormatDebtDetailLine2();

            ++export.Export1.Index;
            export.Export1.CheckSize();

            export.Export1.Update.DtlObligationType.Assign(
              local.Sorted.Item.SortedObligationType);
            MoveObligation(local.Sorted.Item.SortedObligation,
              export.Export1.Update.DtlObligation);
            MoveObligationTransaction1(local.Sorted.Item.
              SortedObligationTransaction,
              export.Export1.Update.DtlObligationTransaction);
            export.Export1.Update.DtlSupported.Number =
              local.Sorted.Item.SupportedSortedCsePersonsWorkSet.Number;
            MoveLegalAction(local.Sorted.Item.SortedLegalAction,
              export.Export1.Update.DtlLegalAction);
            export.Export1.Update.DtlListScreenWorkArea.TextLine76 =
              local.ScreenWorkArea.TextLine76;
            export.Export1.Update.DtlDebtDetail.DueDt =
              local.Sorted.Item.SortedDebtDetail.DueDt;
            export.Export1.Update.DtlAdjusted.CreatedTmst =
              import.Max.Timestamp;
            local.NextPageDisplayLineInd.Flag = "";

            // *** 2ND LINE, DON'T DISPLAY SEL FIELD ***
            export.Export1.Update.DtlDisplayLineInd.Flag = "N";

            if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
            {
              return;
            }
          }
        }

        // ***************************************************************************
        // Logic to populate Collection, Collection Adj and Debt Adj lines (if 
        // any)
        // ***************************************************************************
        // *** Creating the Collection_Adjustment Line ***
        // *** CQ#9065 Changes Begin Here ***
        // *** Changed the below READ EACH by removing the next page obligation 
        // local view ***
        // -- 09/20/2010  GVandy  CQ#22040 Emergency change due to optimizer 
        // choosing innefficient index path when rebound due to DB2 upgrade.
        foreach(var item in ReadCollection())
        {
          // *** CQ#9065 Changes End  Here ***
          // ===============================================
          // Collection Identifier is not sequential.  Once the first set of
          //   collections are retrieved for a specific ob-tran, this number
          //   must be reset to zero.  -  bud adams  -  5/22/00
          //   By the way, this is my last day of work on KESSEP.
          // ===============================================
          local.NextPageCollection.SystemGeneratedIdentifier = 0;

          // =============================================================
          // RBM 08/26/97  Do not show Collection Adjustments if
          //               the show-colladj = 'N'
          // =============================================================
          if (AsChar(import.SearchShowCollAdj.Text1) == 'N' && AsChar
            (entities.Collection.AdjustedInd) == 'Y')
          {
            continue;
          }

          // ****mfb 11/5/95  SET local_effective_date date_work_Area date
          // to Collection Adjustment Date. SET local_transaction_amount
          // ief_supplied total_currency to collection amount.
          // THIS NOTE WAS SET BY  BILL PAO ..
          ++export.Export1.Index;
          export.Export1.CheckSize();

          local.CollectionDate.Date = local.Zero.Date;
          local.EffectiveDate.Date = local.Zero.Date;
          local.ObligationTrnType.ActionEntry = "CO";
          local.AdjReasonCode.TextLine10 = "";
          local.TransactionAmount.TotalCurrency = entities.Collection.Amount;
          local.CollectionDate.Date = entities.Collection.CollectionDt;
          local.EffectiveDate.Date = Date(entities.Collection.CreatedTmst);
          local.Collection.AppliedToOrderTypeCode =
            entities.Collection.AppliedToOrderTypeCode;
          local.Collection.DistPgmStateAppldTo =
            entities.Collection.DistPgmStateAppldTo;

          // ===============================================
          // 4/6/00 - bud adams  -  PR# 88043: Add Collection
          //   Distribution_Method to the displayed collection line.
          // 4/7/00 - bud adams  -  PR# 79544: Add Collection Program_
          //   Applied_To to display the Program code on the collection.
          // ===============================================
          local.Collection.DistributionMethod =
            entities.Collection.DistributionMethod;
          local.Collection.ProgramAppliedTo =
            entities.Collection.ProgramAppliedTo;

          // ***---  select only
          if (ReadCashReceiptDetail())
          {
            export.Export1.Update.DtlCashReceiptDetail.SequentialIdentifier =
              entities.CashReceiptDetail.SequentialIdentifier;

            // ---------------------------------------------
            // Changed to set the Collection Amount instead
            // of Cash Receipt Detail Collection Amount
            // ---------------------------------------------
          }

          // ***---  select only
          if (ReadCashReceipt())
          {
            export.Export1.Update.DtlCashReceipt.SequentialNumber =
              entities.CashReceipt.SequentialNumber;

            if (ReadCashReceiptSourceType())
            {
              export.Export1.Update.DtlCashReceiptSourceType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptSourceType.SystemGeneratedIdentifier;
              local.CashReceiptSourceType.Code =
                entities.CashReceiptSourceType.Code;
            }
            else
            {
              local.CashReceiptSourceType.Code = "";
            }

            if (ReadCashReceiptEvent())
            {
              export.Export1.Update.DtlCashReceiptEvent.
                SystemGeneratedIdentifier =
                  entities.CashReceiptEvent.SystemGeneratedIdentifier;
            }
            else
            {
              export.Export1.Update.DtlCashReceiptEvent.
                SystemGeneratedIdentifier = 0;
            }

            if (ReadCashReceiptType())
            {
              export.Export1.Update.DtlCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
            }
            else
            {
              export.Export1.Update.DtlCashReceiptType.
                SystemGeneratedIdentifier = 0;
            }
          }

          // **** Determine Collection Status
          if (AsChar(local.Sorted.Item.SortedObligationType.Classification) == 'A'
            )
          {
            local.CollectionYearMonth.YearMonth =
              DateToInt(entities.Collection.CollectionDt) / 100;
          }
          else
          {
            local.CollectionYearMonth.YearMonth = 0;
          }

          // *------------------------------------------------------------------*
          // * RBM - 08/01/97
          // * To determine the Collection applied to, the Applied_TO_Code from
          //   Collection can be directly used.
          // *------------------------------------------------------------------*
          local.Status.Flag = entities.Collection.AppliedToCode;
          export.Export1.Update.DtlObligationType.Assign(
            local.Sorted.Item.SortedObligationType);
          MoveObligation(local.Sorted.Item.SortedObligation,
            export.Export1.Update.DtlObligation);
          MoveObligationTransaction1(local.Sorted.Item.
            SortedObligationTransaction,
            export.Export1.Update.DtlObligationTransaction);
          export.Export1.Update.DtlDisplayLineInd.Flag = "N";
          MoveCollection(entities.Collection,
            export.Export1.Update.DtlCollection);
          export.Export1.Update.DtlDebtDetail.DueDt =
            local.Sorted.Item.SortedDebtDetail.DueDt;

          // *** Format the COLLECTION Line ***
          // #################################################
          // ##
          // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
          // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
          // ##
          // #################################################
          UseEabFormatNonDebtDetailLine1();
          export.Export1.Update.DtlObligationTransaction.Type1 =
            local.ObligationTrnType.ActionEntry;
          export.Export1.Update.DtlListScreenWorkArea.TextLine76 =
            local.ScreenWorkArea.TextLine76;
          export.Export1.Update.DtlDebtDetail.DueDt =
            local.Sorted.Item.SortedDebtDetail.DueDt;
          export.Export1.Update.DtlDisplayLineInd.Flag = "Y";
          export.Export1.Update.DtlAdjusted.CreatedTmst = import.Max.Timestamp;

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            return;
          }

          if (AsChar(entities.Collection.AdjustedInd) == 'Y' && AsChar
            (import.SearchShowCollAdj.Text1) == 'Y')
          {
            local.CollectionDate.Date = local.Zero.Date;
            local.EffectiveDate.Date = local.Zero.Date;

            ++export.Export1.Index;
            export.Export1.CheckSize();

            local.Collection.DistributionMethod = "";
            local.Collection.ProgramAppliedTo = "";
            local.ObligationTrnType.ActionEntry = "CA";
            local.TransactionAmount.TotalCurrency = -entities.Collection.Amount;
            local.CollectionDate.Date =
              entities.Collection.CollectionAdjustmentDt;
            local.EffectiveDate.Date =
              entities.Collection.CollectionAdjProcessDate;
            local.Collection.DistPgmStateAppldTo = "";

            if (ReadCollectionAdjustmentReason())
            {
              local.AdjReasonCode.TextLine10 =
                entities.CollectionAdjustmentReason.Code;
            }
            else
            {
              ExitState = "FN0000_COLL_ADJUST_REASON_NF";

              return;
            }

            export.Export1.Update.DtlObligationType.Assign(
              local.Sorted.Item.SortedObligationType);
            MoveObligation(local.Sorted.Item.SortedObligation,
              export.Export1.Update.DtlObligation);
            MoveObligationTransaction1(local.Sorted.Item.
              SortedObligationTransaction,
              export.Export1.Update.DtlObligationTransaction);
            MoveCollection(entities.Collection,
              export.Export1.Update.DtlCollection);
            export.Export1.Update.DtlCashReceiptDetail.SequentialIdentifier =
              entities.CashReceiptDetail.SequentialIdentifier;
            export.Export1.Update.DtlCashReceipt.SequentialNumber =
              entities.CashReceipt.SequentialNumber;
            export.Export1.Update.DtlCashReceiptSourceType.
              SystemGeneratedIdentifier =
                entities.CashReceiptSourceType.SystemGeneratedIdentifier;
            export.Export1.Update.DtlDebtDetail.DueDt =
              local.Sorted.Item.SortedDebtDetail.DueDt;
            export.Export1.Update.DtlDisplayLineInd.Flag = "N";
            export.Export1.Update.DtlObligationTransaction.Type1 =
              local.ObligationTrnType.ActionEntry;

            // : Format COLLECTION ADJUSTMENT line using External here!!!!
            // #################################################
            // ##
            // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
            // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
            // ##
            // #################################################
            UseEabFormatNonDebtDetailLine1();
            export.Export1.Update.DtlListScreenWorkArea.TextLine76 =
              local.ScreenWorkArea.TextLine76;
            export.Export1.Update.DtlDisplayLineInd.Flag = "N";
            export.Export1.Update.DtlAdjusted.CreatedTmst =
              import.Max.Timestamp;

            if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
            {
              return;
            }

            continue;
          }
        }

        // *** There should not be any Adjustments against Voluntary Obligations
        // ***
        if (AsChar(local.Sorted.Item.SortedObligationType.Classification) == 'V'
          )
        {
          continue;
        }

        if (AsChar(import.SearchShowDebtAdj.Text1) == 'Y')
        {
          local.CollectionDate.Date = local.Zero.Date;
          local.EffectiveDate.Date = local.Zero.Date;

          // *** Check for Debt Adjustment Obligation Transactions. ***
          foreach(var item in ReadObligationTransactionRln())
          {
            if (ReadObligationTransaction())
            {
              if (Equal(entities.NonDebt.Type1, "DA"))
              {
                if (ReadObligationTransactionRlnRsn())
                {
                  local.AdjReasonCode.TextLine10 =
                    entities.ObligationTransactionRlnRsn.Code;
                }
                else
                {
                  ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF";

                  return;
                }

                ++export.Export1.Index;
                export.Export1.CheckSize();

                MoveObligation(local.Sorted.Item.SortedObligation,
                  export.Export1.Update.DtlObligation);
                export.Export1.Update.DtlObligationType.Assign(
                  local.Sorted.Item.SortedObligationType);
                MoveObligationTransaction1(local.Sorted.Item.
                  SortedObligationTransaction,
                  export.Export1.Update.DtlObligationTransaction);
                export.Export1.Update.DtlDebtDetail.DueDt =
                  local.Sorted.Item.SortedDebtDetail.DueDt;
                MoveCollection(entities.Collection,
                  export.Export1.Update.DtlCollection);
                export.Export1.Update.DtlAdjusted.CreatedTmst =
                  entities.ObligationTransactionRln.CreatedTmst;
                local.Collection.DistributionMethod = "";
                local.Collection.ProgramAppliedTo = "";
                local.Collection.DistPgmStateAppldTo = "";
                local.CollectionDate.Date = entities.NonDebt.DebtAdjustmentDt;
                local.EffectiveDate.Date =
                  entities.NonDebt.DebtAdjustmentProcessDate;
                local.TransactionAmount.TotalCurrency = entities.NonDebt.Amount;
                local.ObligationTrnType.ActionEntry = "DA";

                // **** Determine increase or decrease adjustment
                if (AsChar(entities.NonDebt.DebtAdjustmentType) == 'I')
                {
                  local.Status.Flag = "+";
                }
                else
                {
                  local.Status.Flag = "-";
                }

                // : Format DEBT ADJUSTMENT line using External here!!!!
                // #################################################
                // ##
                // ##  THIS IS DEACTIVATED TO AVOID BLOWING UP IN TRACE.
                // ##     REACTIVATE IT AFTER THIS THING IS WORKING.
                // ##
                // #################################################
                UseEabFormatNonDebtDetailLine2();
                export.Export1.Update.DtlListScreenWorkArea.TextLine76 =
                  local.ScreenWorkArea.TextLine76;

                // *** DEBT ADJS LINE, DON'T DISPLAY SEL FIELD
                export.Export1.Update.DtlDisplayLineInd.Flag = "N";

                if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
                {
                  return;
                }
              }
              else
              {
              }
            }
          }

          // ===============================================
          // After returning to this CAB, the first time through here we
          //   need to avoid duplicating Adjustments.  After that, the
          //   relationships will ensure we have the adjustments for the
          //   proper obligation and point in time.
          // ===============================================
          local.NextPageObligation.SystemGeneratedIdentifier = 999;
        }

        local.SuppressDebtHeading.Flag = "N";
      }

      local.Sorted.CheckIndex();
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
  }

  private static void MoveDebtDetail1(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.AdcDt = source.AdcDt;
    target.RetiredDt = source.RetiredDt;
  }

  private static void MoveDebtDetail2(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SupportedPersonReqInd = source.SupportedPersonReqInd;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Classification = source.Classification;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.InterstateIndicator = source.InterstateIndicator;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private int UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.DebtDue.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private void UseEabFormatDebtDetailLine1()
  {
    var useImport = new EabFormatDebtDetailLine1.Import();
    var useExport = new EabFormatDebtDetailLine1.Export();

    useImport.ObligationType.Code = local.Sorted.Item.SortedObligationType.Code;
    MoveObligationTransaction2(local.Sorted.Item.SortedObligationTransaction,
      useImport.ObligationTransaction);
    useImport.ProgramScreenAttributes.ProgramTypeInd =
      local.Sorted.Item.SortedProgramScreenAttributes.ProgramTypeInd;
    useImport.ScreenOwedAmounts.Assign(local.ObligationScreenOwedAmounts);
    useImport.DebtDetail.Assign(local.DebtDetail);
    useImport.DprProgram.ProgramState =
      local.Sorted.Item.SortedDprProgram.ProgramState;
    useExport.ListScreenWorkArea.TextLine76 = local.ScreenWorkArea.TextLine76;

    Call(EabFormatDebtDetailLine1.Execute, useImport, useExport);

    local.ScreenWorkArea.TextLine76 = useExport.ListScreenWorkArea.TextLine76;
  }

  private void UseEabFormatDebtDetailLine2()
  {
    var useImport = new EabFormatDebtDetailLine2.Import();
    var useExport = new EabFormatDebtDetailLine2.Export();

    MoveObligation(local.Sorted.Item.SortedObligation, useImport.Obligation);
    useImport.LegalAction.Assign(local.Sorted.Item.SortedLegalAction);
    MoveCsePersonsWorkSet2(local.Sorted.Item.SupportedSortedCsePersonsWorkSet,
      useImport.CsePersonsWorkSet);
    useImport.Case1.Number = local.Sorted.Item.SortedCase.Number;
    useImport.ServiceProvider.UserId =
      local.Sorted.Item.SortedServiceProvider.UserId;
    useExport.ListScreenWorkArea.TextLine76 = local.ScreenWorkArea.TextLine76;

    Call(EabFormatDebtDetailLine2.Execute, useImport, useExport);

    local.ScreenWorkArea.TextLine76 = useExport.ListScreenWorkArea.TextLine76;
  }

  private void UseEabFormatNonDebtDetailLine1()
  {
    var useImport = new EabFormatNonDebtDetailLine.Import();
    var useExport = new EabFormatNonDebtDetailLine.Export();

    useImport.CashReceiptSourceType.Code = local.CashReceiptSourceType.Code;
    useImport.ObligationTrnType.ActionEntry =
      local.ObligationTrnType.ActionEntry;
    useImport.CollectionDate.Date = local.CollectionDate.Date;
    useImport.EffectiveDate.Date = local.EffectiveDate.Date;
    useImport.TrnAmount.TotalCurrency = local.TransactionAmount.TotalCurrency;
    useImport.Status.Flag = local.Status.Flag;
    useImport.Collection.Assign(local.Collection);
    useImport.AdjCode.TextLine10 = local.AdjReasonCode.TextLine10;
    useExport.ListScreenWorkArea.TextLine76 = local.ScreenWorkArea.TextLine76;

    Call(EabFormatNonDebtDetailLine.Execute, useImport, useExport);

    local.ScreenWorkArea.TextLine76 = useExport.ListScreenWorkArea.TextLine76;
  }

  private void UseEabFormatNonDebtDetailLine2()
  {
    var useImport = new EabFormatNonDebtDetailLine.Import();
    var useExport = new EabFormatNonDebtDetailLine.Export();

    useImport.ObligationTrnType.ActionEntry =
      local.ObligationTrnType.ActionEntry;
    useImport.CollectionDate.Date = local.CollectionDate.Date;
    useImport.EffectiveDate.Date = local.EffectiveDate.Date;
    useImport.TrnAmount.TotalCurrency = local.TransactionAmount.TotalCurrency;
    useImport.Status.Flag = local.Status.Flag;
    useImport.Collection.Assign(local.Collection);
    useImport.AdjCode.TextLine10 = local.AdjReasonCode.TextLine10;
    useExport.ListScreenWorkArea.TextLine76 = local.ScreenWorkArea.TextLine76;

    Call(EabFormatNonDebtDetailLine.Execute, useImport, useExport);

    local.ScreenWorkArea.TextLine76 = useExport.ListScreenWorkArea.TextLine76;
  }

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet1(export.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      

    Call(EabReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void UseFnComputeSummaryTotals1()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = entities.ObligorCsePerson.Number;
    useImport.Filter.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      local.Required.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    local.ObligationScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.UndistributedAmt.TotalCurrency = useExport.UndistAmt.TotalCurrency;
  }

  private void UseFnComputeSummaryTotals2()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = entities.ObligorCsePerson.Number;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      local.Required.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.UndistributedAmt.TotalCurrency = useExport.UndistAmt.TotalCurrency;
  }

  private void UseFnDetCaseNoAndWrkrForDbt()
  {
    var useImport = new FnDetCaseNoAndWrkrForDbt.Import();
    var useExport = new FnDetCaseNoAndWrkrForDbt.Export();

    useImport.Obligor.Number = entities.ObligorCsePerson.Number;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.Supported.Number = entities.SupportedPerson.Number;

    Call(FnDetCaseNoAndWrkrForDbt.Execute, useImport, useExport);

    local.Case1.Number = useExport.Case1.Number;
    MoveServiceProvider(useExport.ServiceProvider, local.ServiceProvider);
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.SupportedPerson.Number;
    MoveObligationType2(entities.ObligationType, useImport.ObligationType);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    MoveDebtDetail2(entities.DebtDetail, useImport.DebtDetail);

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.DistributionPgm);
    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeSupported.Type1 = useExport.CpaSupportedPerson.Type1;
    local.HardcodeAccount.Type1 = useExport.MosCsePersonAccount.Type1;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Supported.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Supported.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.CashReceipt.ReferenceNumber = db.GetNullableString(reader, 5);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetInt32(command, "crSrceTypeId", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId",
          local.Sorted.Item.SortedObligationTransaction.
            SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType",
          local.Sorted.Item.SortedObligationTransaction.Type1);
        db.SetInt32(
          command, "obgId",
          local.Sorted.Item.SortedObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId",
          local.Sorted.Item.SortedObligationType.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.SetInt32(
          command, "collId",
          local.NextPageCollection.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.CarId = db.GetNullableInt32(reader, 16);
        entities.Collection.OtyId = db.GetInt32(reader, 17);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 18);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.Amount = db.GetDecimal(reader, 21);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 22);
        entities.Collection.DistributionMethod = db.GetString(reader, 23);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 24);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 25);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 26);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.Collection.CarId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ObligorCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ObligorCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.SupportedPerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ObligationTransaction.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SupportedPerson.Number = db.GetString(reader, 0);
        entities.SupportedPerson.Type1 = db.GetString(reader, 1);
        entities.SupportedPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.SupportedPerson.Type1);
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.ObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ObligorCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligorCsePersonAccount.Type1);
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.SearchLegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Type1 = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Type1 = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.SearchLegalAction.StandardNumber ?? ""
          );
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadObligationObligationTransactionDebtDetailObligationType1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach(
      "ReadObligationObligationTransactionDebtDetailObligationType1",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.SetNullableDate(
          command, "retiredDt", local.Zero.Date.GetValueOrDefault());
        db.SetDate(command, "dueDt1", import.SearchTo.Date.GetValueOrDefault());
        db.
          SetDate(command, "dueDt2", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(
          command, "dueDt3", local.NextPageSearchTo.Date.GetValueOrDefault());
        db.SetString(command, "debtTypCd", import.NextPageObligationType.Code);
        db.SetInt32(
          command, "obId", local.NextPageObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obTrnId",
          import.NextPageObligationTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.DebtDetail.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 10);
        entities.DebtDetail.OtrType = db.GetString(reader, 10);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 11);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 12);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 13);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 14);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 15);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 16);
        entities.DebtDetail.DueDt = db.GetDate(reader, 17);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 18);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 19);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 20);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 21);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 22);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 23);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 24);
        entities.ObligationType.Code = db.GetString(reader, 25);
        entities.ObligationType.Classification = db.GetString(reader, 26);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 27);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.ObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadObligationObligationTransactionDebtDetailObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach(
      "ReadObligationObligationTransactionDebtDetailObligationType2",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.NextPageSearchTo.Date.GetValueOrDefault());
        db.SetString(command, "debtTypCd", import.NextPageObligationType.Code);
        db.SetInt32(
          command, "obId", local.NextPageObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obTrnId",
          import.NextPageObligationTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.DebtDetail.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 7);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 8);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 10);
        entities.DebtDetail.OtrType = db.GetString(reader, 10);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 11);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 12);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 13);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 14);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 15);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 16);
        entities.DebtDetail.DueDt = db.GetDate(reader, 17);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 18);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 19);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 20);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 21);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 22);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 23);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 24);
        entities.ObligationType.Code = db.GetString(reader, 25);
        entities.ObligationType.Classification = db.GetString(reader, 26);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 27);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.ObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.
      Assert(entities.ObligationTransactionRln.Populated);
    entities.NonDebt.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationTransactionRln.OtyTypeSecondary);
        db.SetString(
          command, "obTrnTyp", entities.ObligationTransactionRln.OtrType);
        db.SetInt32(
          command, "obTrnId", entities.ObligationTransactionRln.OtrGeneratedId);
          
        db.SetString(
          command, "cpaType", entities.ObligationTransactionRln.CpaType);
        db.SetString(
          command, "cspNumber", entities.ObligationTransactionRln.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransactionRln.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.NonDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.NonDebt.CspNumber = db.GetString(reader, 1);
        entities.NonDebt.CpaType = db.GetString(reader, 2);
        entities.NonDebt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.NonDebt.Type1 = db.GetString(reader, 4);
        entities.NonDebt.Amount = db.GetDecimal(reader, 5);
        entities.NonDebt.DebtAdjustmentType = db.GetString(reader, 6);
        entities.NonDebt.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.NonDebt.CreatedTmst = db.GetDateTime(reader, 8);
        entities.NonDebt.CspSupNumber = db.GetNullableString(reader, 9);
        entities.NonDebt.CpaSupType = db.GetNullableString(reader, 10);
        entities.NonDebt.OtyType = db.GetInt32(reader, 11);
        entities.NonDebt.DebtAdjustmentProcessDate = db.GetDate(reader, 12);
        entities.NonDebt.ReasonCode = db.GetString(reader, 13);
        entities.NonDebt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.NonDebt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.NonDebt.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.NonDebt.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.NonDebt.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.ObligationTransactionRln.Populated = false;

    return ReadEach("ReadObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrPGeneratedId",
          local.Sorted.Item.SortedObligationTransaction.
            SystemGeneratedIdentifier);
        db.SetString(
          command, "otrPType",
          local.Sorted.Item.SortedObligationTransaction.Type1);
        db.SetInt32(
          command, "obgPGeneratedId1",
          local.Sorted.Item.SortedObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyTypePrimary",
          local.Sorted.Item.SortedObligationType.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaPType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspPNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.SetDateTime(
          command, "createdTmst",
          import.NextPageObligationTransactionRln.CreatedTmst.
            GetValueOrDefault());
        db.SetInt32(
          command, "obgPGeneratedId2",
          local.NextPageObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 6);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 9);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationTransactionRln.CreatedTmst =
          db.GetDateTime(reader, 12);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 13);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 14);
        entities.ObligationTransactionRln.Populated = true;
        CheckValid<ObligationTransactionRln>("OtrType",
          entities.ObligationTransactionRln.OtrType);
        CheckValid<ObligationTransactionRln>("CpaType",
          entities.ObligationTransactionRln.CpaType);
        CheckValid<ObligationTransactionRln>("OtrPType",
          entities.ObligationTransactionRln.OtrPType);
        CheckValid<ObligationTransactionRln>("CpaPType",
          entities.ObligationTransactionRln.CpaPType);

        return true;
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    System.Diagnostics.Debug.
      Assert(entities.ObligationTransactionRln.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.ObligationTransactionRln.OnrGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// A value of NextPageDisplayLinInd.
    /// </summary>
    [JsonPropertyName("nextPageDisplayLinInd")]
    public Common NextPageDisplayLinInd
    {
      get => nextPageDisplayLinInd ??= new();
      set => nextPageDisplayLinInd = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of NextPageObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("nextPageObligationTransactionRln")]
    public ObligationTransactionRln NextPageObligationTransactionRln
    {
      get => nextPageObligationTransactionRln ??= new();
      set => nextPageObligationTransactionRln = value;
    }

    /// <summary>
    /// A value of NextPageCollection.
    /// </summary>
    [JsonPropertyName("nextPageCollection")]
    public Collection NextPageCollection
    {
      get => nextPageCollection ??= new();
      set => nextPageCollection = value;
    }

    /// <summary>
    /// A value of NextPageSearchTo.
    /// </summary>
    [JsonPropertyName("nextPageSearchTo")]
    public DateWorkArea NextPageSearchTo
    {
      get => nextPageSearchTo ??= new();
      set => nextPageSearchTo = value;
    }

    /// <summary>
    /// A value of NextPageObligation.
    /// </summary>
    [JsonPropertyName("nextPageObligation")]
    public Obligation NextPageObligation
    {
      get => nextPageObligation ??= new();
      set => nextPageObligation = value;
    }

    /// <summary>
    /// A value of NextPageObligationType.
    /// </summary>
    [JsonPropertyName("nextPageObligationType")]
    public ObligationType NextPageObligationType
    {
      get => nextPageObligationType ??= new();
      set => nextPageObligationType = value;
    }

    /// <summary>
    /// A value of NextPageObligationTransaction.
    /// </summary>
    [JsonPropertyName("nextPageObligationTransaction")]
    public ObligationTransaction NextPageObligationTransaction
    {
      get => nextPageObligationTransaction ??= new();
      set => nextPageObligationTransaction = value;
    }

    /// <summary>
    /// A value of ListDebtsWithAmtOwed.
    /// </summary>
    [JsonPropertyName("listDebtsWithAmtOwed")]
    public Common ListDebtsWithAmtOwed
    {
      get => listDebtsWithAmtOwed ??= new();
      set => listDebtsWithAmtOwed = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of SearchYear.
    /// </summary>
    [JsonPropertyName("searchYear")]
    public Common SearchYear
    {
      get => searchYear ??= new();
      set => searchYear = value;
    }

    /// <summary>
    /// A value of SearchMonth.
    /// </summary>
    [JsonPropertyName("searchMonth")]
    public Common SearchMonth
    {
      get => searchMonth ??= new();
      set => searchMonth = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of SearchShowDebtAdj.
    /// </summary>
    [JsonPropertyName("searchShowDebtAdj")]
    public TextWorkArea SearchShowDebtAdj
    {
      get => searchShowDebtAdj ??= new();
      set => searchShowDebtAdj = value;
    }

    /// <summary>
    /// A value of SearchShowCollAdj.
    /// </summary>
    [JsonPropertyName("searchShowCollAdj")]
    public TextWorkArea SearchShowCollAdj
    {
      get => searchShowCollAdj ??= new();
      set => searchShowCollAdj = value;
    }

    private Common nextPageDisplayLinInd;
    private DateWorkArea current;
    private DateWorkArea max;
    private ObligationTransactionRln nextPageObligationTransactionRln;
    private Collection nextPageCollection;
    private DateWorkArea nextPageSearchTo;
    private Obligation nextPageObligation;
    private ObligationType nextPageObligationType;
    private ObligationTransaction nextPageObligationTransaction;
    private Common listDebtsWithAmtOwed;
    private ObligationTransaction obligationTransaction;
    private Common searchYear;
    private Common searchMonth;
    private CsePerson searchCsePerson;
    private LegalAction searchLegalAction;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private TextWorkArea searchShowDebtAdj;
    private TextWorkArea searchShowCollAdj;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DtlDebtDetail.
      /// </summary>
      [JsonPropertyName("dtlDebtDetail")]
      public DebtDetail DtlDebtDetail
      {
        get => dtlDebtDetail ??= new();
        set => dtlDebtDetail = value;
      }

      /// <summary>
      /// A value of DtlCollection.
      /// </summary>
      [JsonPropertyName("dtlCollection")]
      public Collection DtlCollection
      {
        get => dtlCollection ??= new();
        set => dtlCollection = value;
      }

      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlDisplayLineInd.
      /// </summary>
      [JsonPropertyName("dtlDisplayLineInd")]
      public Common DtlDisplayLineInd
      {
        get => dtlDisplayLineInd ??= new();
        set => dtlDisplayLineInd = value;
      }

      /// <summary>
      /// A value of DtlListScreenWorkArea.
      /// </summary>
      [JsonPropertyName("dtlListScreenWorkArea")]
      public ListScreenWorkArea DtlListScreenWorkArea
      {
        get => dtlListScreenWorkArea ??= new();
        set => dtlListScreenWorkArea = value;
      }

      /// <summary>
      /// A value of DtlSupported.
      /// </summary>
      [JsonPropertyName("dtlSupported")]
      public CsePersonsWorkSet DtlSupported
      {
        get => dtlSupported ??= new();
        set => dtlSupported = value;
      }

      /// <summary>
      /// A value of DtlObligation.
      /// </summary>
      [JsonPropertyName("dtlObligation")]
      public Obligation DtlObligation
      {
        get => dtlObligation ??= new();
        set => dtlObligation = value;
      }

      /// <summary>
      /// A value of DtlObligationType.
      /// </summary>
      [JsonPropertyName("dtlObligationType")]
      public ObligationType DtlObligationType
      {
        get => dtlObligationType ??= new();
        set => dtlObligationType = value;
      }

      /// <summary>
      /// A value of DtlObligationTransaction.
      /// </summary>
      [JsonPropertyName("dtlObligationTransaction")]
      public ObligationTransaction DtlObligationTransaction
      {
        get => dtlObligationTransaction ??= new();
        set => dtlObligationTransaction = value;
      }

      /// <summary>
      /// A value of DtlAdjusted.
      /// </summary>
      [JsonPropertyName("dtlAdjusted")]
      public ObligationTransactionRln DtlAdjusted
      {
        get => dtlAdjusted ??= new();
        set => dtlAdjusted = value;
      }

      /// <summary>
      /// A value of DtlCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("dtlCashReceiptSourceType")]
      public CashReceiptSourceType DtlCashReceiptSourceType
      {
        get => dtlCashReceiptSourceType ??= new();
        set => dtlCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DtlCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("dtlCashReceiptEvent")]
      public CashReceiptEvent DtlCashReceiptEvent
      {
        get => dtlCashReceiptEvent ??= new();
        set => dtlCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DtlCashReceiptType.
      /// </summary>
      [JsonPropertyName("dtlCashReceiptType")]
      public CashReceiptType DtlCashReceiptType
      {
        get => dtlCashReceiptType ??= new();
        set => dtlCashReceiptType = value;
      }

      /// <summary>
      /// A value of DtlCashReceipt.
      /// </summary>
      [JsonPropertyName("dtlCashReceipt")]
      public CashReceipt DtlCashReceipt
      {
        get => dtlCashReceipt ??= new();
        set => dtlCashReceipt = value;
      }

      /// <summary>
      /// A value of DtlCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("dtlCashReceiptDetail")]
      public CashReceiptDetail DtlCashReceiptDetail
      {
        get => dtlCashReceiptDetail ??= new();
        set => dtlCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of DtlLegalAction.
      /// </summary>
      [JsonPropertyName("dtlLegalAction")]
      public LegalAction DtlLegalAction
      {
        get => dtlLegalAction ??= new();
        set => dtlLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 71;

      private DebtDetail dtlDebtDetail;
      private Collection dtlCollection;
      private Common dtlCommon;
      private Common dtlDisplayLineInd;
      private ListScreenWorkArea dtlListScreenWorkArea;
      private CsePersonsWorkSet dtlSupported;
      private Obligation dtlObligation;
      private ObligationType dtlObligationType;
      private ObligationTransaction dtlObligationTransaction;
      private ObligationTransactionRln dtlAdjusted;
      private CashReceiptSourceType dtlCashReceiptSourceType;
      private CashReceiptEvent dtlCashReceiptEvent;
      private CashReceiptType dtlCashReceiptType;
      private CashReceipt dtlCashReceipt;
      private CashReceiptDetail dtlCashReceiptDetail;
      private LegalAction dtlLegalAction;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of UndistributedAmt.
    /// </summary>
    [JsonPropertyName("undistributedAmt")]
    public Common UndistributedAmt
    {
      get => undistributedAmt ??= new();
      set => undistributedAmt = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistributedAmt;
    private ObligationType obligationType;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ToBeSortedGroup group.</summary>
    [Serializable]
    public class ToBeSortedGroup
    {
      /// <summary>
      /// A value of ToBeSortedSeqNo.
      /// </summary>
      [JsonPropertyName("toBeSortedSeqNo")]
      public Common ToBeSortedSeqNo
      {
        get => toBeSortedSeqNo ??= new();
        set => toBeSortedSeqNo = value;
      }

      /// <summary>
      /// A value of ToBeSortedObligation.
      /// </summary>
      [JsonPropertyName("toBeSortedObligation")]
      public Obligation ToBeSortedObligation
      {
        get => toBeSortedObligation ??= new();
        set => toBeSortedObligation = value;
      }

      /// <summary>
      /// A value of ToBeSortedObligationType.
      /// </summary>
      [JsonPropertyName("toBeSortedObligationType")]
      public ObligationType ToBeSortedObligationType
      {
        get => toBeSortedObligationType ??= new();
        set => toBeSortedObligationType = value;
      }

      /// <summary>
      /// A value of ToBeSortedObligationTransaction.
      /// </summary>
      [JsonPropertyName("toBeSortedObligationTransaction")]
      public ObligationTransaction ToBeSortedObligationTransaction
      {
        get => toBeSortedObligationTransaction ??= new();
        set => toBeSortedObligationTransaction = value;
      }

      /// <summary>
      /// A value of ToBeSortedDebtDetail.
      /// </summary>
      [JsonPropertyName("toBeSortedDebtDetail")]
      public DebtDetail ToBeSortedDebtDetail
      {
        get => toBeSortedDebtDetail ??= new();
        set => toBeSortedDebtDetail = value;
      }

      /// <summary>
      /// A value of ToBeSortedLegalAction.
      /// </summary>
      [JsonPropertyName("toBeSortedLegalAction")]
      public LegalAction ToBeSortedLegalAction
      {
        get => toBeSortedLegalAction ??= new();
        set => toBeSortedLegalAction = value;
      }

      /// <summary>
      /// A value of SupportedToBeSortedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedToBeSortedCsePerson")]
      public CsePerson SupportedToBeSortedCsePerson
      {
        get => supportedToBeSortedCsePerson ??= new();
        set => supportedToBeSortedCsePerson = value;
      }

      /// <summary>
      /// A value of SupportedToBeSortedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedToBeSortedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedToBeSortedCsePersonsWorkSet
      {
        get => supportedToBeSortedCsePersonsWorkSet ??= new();
        set => supportedToBeSortedCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ToBeSortedProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("toBeSortedProgramScreenAttributes")]
      public ProgramScreenAttributes ToBeSortedProgramScreenAttributes
      {
        get => toBeSortedProgramScreenAttributes ??= new();
        set => toBeSortedProgramScreenAttributes = value;
      }

      /// <summary>
      /// A value of ToBeSortedCase.
      /// </summary>
      [JsonPropertyName("toBeSortedCase")]
      public Case1 ToBeSortedCase
      {
        get => toBeSortedCase ??= new();
        set => toBeSortedCase = value;
      }

      /// <summary>
      /// A value of ToBeSortedServiceProvider.
      /// </summary>
      [JsonPropertyName("toBeSortedServiceProvider")]
      public ServiceProvider ToBeSortedServiceProvider
      {
        get => toBeSortedServiceProvider ??= new();
        set => toBeSortedServiceProvider = value;
      }

      /// <summary>
      /// A value of ToBeSortedDprProgram.
      /// </summary>
      [JsonPropertyName("toBeSortedDprProgram")]
      public DprProgram ToBeSortedDprProgram
      {
        get => toBeSortedDprProgram ??= new();
        set => toBeSortedDprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common toBeSortedSeqNo;
      private Obligation toBeSortedObligation;
      private ObligationType toBeSortedObligationType;
      private ObligationTransaction toBeSortedObligationTransaction;
      private DebtDetail toBeSortedDebtDetail;
      private LegalAction toBeSortedLegalAction;
      private CsePerson supportedToBeSortedCsePerson;
      private CsePersonsWorkSet supportedToBeSortedCsePersonsWorkSet;
      private ProgramScreenAttributes toBeSortedProgramScreenAttributes;
      private Case1 toBeSortedCase;
      private ServiceProvider toBeSortedServiceProvider;
      private DprProgram toBeSortedDprProgram;
    }

    /// <summary>A SortedGroup group.</summary>
    [Serializable]
    public class SortedGroup
    {
      /// <summary>
      /// A value of SortedObligation.
      /// </summary>
      [JsonPropertyName("sortedObligation")]
      public Obligation SortedObligation
      {
        get => sortedObligation ??= new();
        set => sortedObligation = value;
      }

      /// <summary>
      /// A value of SortedObligationType.
      /// </summary>
      [JsonPropertyName("sortedObligationType")]
      public ObligationType SortedObligationType
      {
        get => sortedObligationType ??= new();
        set => sortedObligationType = value;
      }

      /// <summary>
      /// A value of SortedObligationTransaction.
      /// </summary>
      [JsonPropertyName("sortedObligationTransaction")]
      public ObligationTransaction SortedObligationTransaction
      {
        get => sortedObligationTransaction ??= new();
        set => sortedObligationTransaction = value;
      }

      /// <summary>
      /// A value of SortedDebtDetail.
      /// </summary>
      [JsonPropertyName("sortedDebtDetail")]
      public DebtDetail SortedDebtDetail
      {
        get => sortedDebtDetail ??= new();
        set => sortedDebtDetail = value;
      }

      /// <summary>
      /// A value of SortedLegalAction.
      /// </summary>
      [JsonPropertyName("sortedLegalAction")]
      public LegalAction SortedLegalAction
      {
        get => sortedLegalAction ??= new();
        set => sortedLegalAction = value;
      }

      /// <summary>
      /// A value of SupportedSortedCsePerson.
      /// </summary>
      [JsonPropertyName("supportedSortedCsePerson")]
      public CsePerson SupportedSortedCsePerson
      {
        get => supportedSortedCsePerson ??= new();
        set => supportedSortedCsePerson = value;
      }

      /// <summary>
      /// A value of SupportedSortedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedSortedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedSortedCsePersonsWorkSet
      {
        get => supportedSortedCsePersonsWorkSet ??= new();
        set => supportedSortedCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of SortedProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("sortedProgramScreenAttributes")]
      public ProgramScreenAttributes SortedProgramScreenAttributes
      {
        get => sortedProgramScreenAttributes ??= new();
        set => sortedProgramScreenAttributes = value;
      }

      /// <summary>
      /// A value of SortedCase.
      /// </summary>
      [JsonPropertyName("sortedCase")]
      public Case1 SortedCase
      {
        get => sortedCase ??= new();
        set => sortedCase = value;
      }

      /// <summary>
      /// A value of SortedServiceProvider.
      /// </summary>
      [JsonPropertyName("sortedServiceProvider")]
      public ServiceProvider SortedServiceProvider
      {
        get => sortedServiceProvider ??= new();
        set => sortedServiceProvider = value;
      }

      /// <summary>
      /// A value of SortedDprProgram.
      /// </summary>
      [JsonPropertyName("sortedDprProgram")]
      public DprProgram SortedDprProgram
      {
        get => sortedDprProgram ??= new();
        set => sortedDprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Obligation sortedObligation;
      private ObligationType sortedObligationType;
      private ObligationTransaction sortedObligationTransaction;
      private DebtDetail sortedDebtDetail;
      private LegalAction sortedLegalAction;
      private CsePerson supportedSortedCsePerson;
      private CsePersonsWorkSet supportedSortedCsePersonsWorkSet;
      private ProgramScreenAttributes sortedProgramScreenAttributes;
      private Case1 sortedCase;
      private ServiceProvider sortedServiceProvider;
      private DprProgram sortedDprProgram;
    }

    /// <summary>A TempInGroup group.</summary>
    [Serializable]
    public class TempInGroup
    {
      /// <summary>
      /// A value of TempInSeqNo.
      /// </summary>
      [JsonPropertyName("tempInSeqNo")]
      public Common TempInSeqNo
      {
        get => tempInSeqNo ??= new();
        set => tempInSeqNo = value;
      }

      /// <summary>
      /// A value of TempInObligationType.
      /// </summary>
      [JsonPropertyName("tempInObligationType")]
      public ObligationType TempInObligationType
      {
        get => tempInObligationType ??= new();
        set => tempInObligationType = value;
      }

      /// <summary>
      /// A value of TempInDebtDetail.
      /// </summary>
      [JsonPropertyName("tempInDebtDetail")]
      public DebtDetail TempInDebtDetail
      {
        get => tempInDebtDetail ??= new();
        set => tempInDebtDetail = value;
      }

      /// <summary>
      /// A value of TempInSupported.
      /// </summary>
      [JsonPropertyName("tempInSupported")]
      public CsePersonsWorkSet TempInSupported
      {
        get => tempInSupported ??= new();
        set => tempInSupported = value;
      }

      private Common tempInSeqNo;
      private ObligationType tempInObligationType;
      private DebtDetail tempInDebtDetail;
      private CsePersonsWorkSet tempInSupported;
    }

    /// <summary>A TempOutGroup group.</summary>
    [Serializable]
    public class TempOutGroup
    {
      /// <summary>
      /// A value of TempOutSeqNo.
      /// </summary>
      [JsonPropertyName("tempOutSeqNo")]
      public Common TempOutSeqNo
      {
        get => tempOutSeqNo ??= new();
        set => tempOutSeqNo = value;
      }

      /// <summary>
      /// A value of TempOutObligationType.
      /// </summary>
      [JsonPropertyName("tempOutObligationType")]
      public ObligationType TempOutObligationType
      {
        get => tempOutObligationType ??= new();
        set => tempOutObligationType = value;
      }

      /// <summary>
      /// A value of TempOutDebtDetail.
      /// </summary>
      [JsonPropertyName("tempOutDebtDetail")]
      public DebtDetail TempOutDebtDetail
      {
        get => tempOutDebtDetail ??= new();
        set => tempOutDebtDetail = value;
      }

      /// <summary>
      /// A value of TempOutSupported.
      /// </summary>
      [JsonPropertyName("tempOutSupported")]
      public CsePersonsWorkSet TempOutSupported
      {
        get => tempOutSupported ??= new();
        set => tempOutSupported = value;
      }

      private Common tempOutSeqNo;
      private ObligationType tempOutObligationType;
      private DebtDetail tempOutDebtDetail;
      private CsePersonsWorkSet tempOutSupported;
    }

    /// <summary>A SortOrderGroup group.</summary>
    [Serializable]
    public class SortOrderGroup
    {
      /// <summary>
      /// A value of SortOrderSeqNo.
      /// </summary>
      [JsonPropertyName("sortOrderSeqNo")]
      public Common SortOrderSeqNo
      {
        get => sortOrderSeqNo ??= new();
        set => sortOrderSeqNo = value;
      }

      /// <summary>
      /// A value of SortOrderObligationType.
      /// </summary>
      [JsonPropertyName("sortOrderObligationType")]
      public ObligationType SortOrderObligationType
      {
        get => sortOrderObligationType ??= new();
        set => sortOrderObligationType = value;
      }

      /// <summary>
      /// A value of SortOrderDebtDetail.
      /// </summary>
      [JsonPropertyName("sortOrderDebtDetail")]
      public DebtDetail SortOrderDebtDetail
      {
        get => sortOrderDebtDetail ??= new();
        set => sortOrderDebtDetail = value;
      }

      /// <summary>
      /// A value of SortOrderSupported.
      /// </summary>
      [JsonPropertyName("sortOrderSupported")]
      public CsePersonsWorkSet SortOrderSupported
      {
        get => sortOrderSupported ??= new();
        set => sortOrderSupported = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common sortOrderSeqNo;
      private ObligationType sortOrderObligationType;
      private DebtDetail sortOrderDebtDetail;
      private CsePersonsWorkSet sortOrderSupported;
    }

    /// <summary>
    /// A value of NextPageCollection.
    /// </summary>
    [JsonPropertyName("nextPageCollection")]
    public Collection NextPageCollection
    {
      get => nextPageCollection ??= new();
      set => nextPageCollection = value;
    }

    /// <summary>
    /// A value of SuppressDebtHeading.
    /// </summary>
    [JsonPropertyName("suppressDebtHeading")]
    public Common SuppressDebtHeading
    {
      get => suppressDebtHeading ??= new();
      set => suppressDebtHeading = value;
    }

    /// <summary>
    /// A value of NextPageDisplayLineInd.
    /// </summary>
    [JsonPropertyName("nextPageDisplayLineInd")]
    public Common NextPageDisplayLineInd
    {
      get => nextPageDisplayLineInd ??= new();
      set => nextPageDisplayLineInd = value;
    }

    /// <summary>
    /// A value of NextPageSearchTo.
    /// </summary>
    [JsonPropertyName("nextPageSearchTo")]
    public DateWorkArea NextPageSearchTo
    {
      get => nextPageSearchTo ??= new();
      set => nextPageSearchTo = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Obligation Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of NextPageObligation.
    /// </summary>
    [JsonPropertyName("nextPageObligation")]
    public Obligation NextPageObligation
    {
      get => nextPageObligation ??= new();
      set => nextPageObligation = value;
    }

    /// <summary>
    /// A value of SearchDiscontinue.
    /// </summary>
    [JsonPropertyName("searchDiscontinue")]
    public DateWorkArea SearchDiscontinue
    {
      get => searchDiscontinue ??= new();
      set => searchDiscontinue = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of DistributionPgm.
    /// </summary>
    [JsonPropertyName("distributionPgm")]
    public Program DistributionPgm
    {
      get => distributionPgm ??= new();
      set => distributionPgm = value;
    }

    /// <summary>
    /// A value of InitializedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("initializedCsePersonsWorkSet")]
    public CsePersonsWorkSet InitializedCsePersonsWorkSet
    {
      get => initializedCsePersonsWorkSet ??= new();
      set => initializedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of GroupSortOrderIndex.
    /// </summary>
    [JsonPropertyName("groupSortOrderIndex")]
    public Common GroupSortOrderIndex
    {
      get => groupSortOrderIndex ??= new();
      set => groupSortOrderIndex = value;
    }

    /// <summary>
    /// A value of InitializedLegalAction.
    /// </summary>
    [JsonPropertyName("initializedLegalAction")]
    public LegalAction InitializedLegalAction
    {
      get => initializedLegalAction ??= new();
      set => initializedLegalAction = value;
    }

    /// <summary>
    /// A value of ProgramScreenAttributes.
    /// </summary>
    [JsonPropertyName("programScreenAttributes")]
    public ProgramScreenAttributes ProgramScreenAttributes
    {
      get => programScreenAttributes ??= new();
      set => programScreenAttributes = value;
    }

    /// <summary>
    /// A value of NoExchanges.
    /// </summary>
    [JsonPropertyName("noExchanges")]
    public Common NoExchanges
    {
      get => noExchanges ??= new();
      set => noExchanges = value;
    }

    /// <summary>
    /// A value of SortPassCounter.
    /// </summary>
    [JsonPropertyName("sortPassCounter")]
    public Common SortPassCounter
    {
      get => sortPassCounter ??= new();
      set => sortPassCounter = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of SearchYear.
    /// </summary>
    [JsonPropertyName("searchYear")]
    public Common SearchYear
    {
      get => searchYear ??= new();
      set => searchYear = value;
    }

    /// <summary>
    /// A value of SearchMonth.
    /// </summary>
    [JsonPropertyName("searchMonth")]
    public Common SearchMonth
    {
      get => searchMonth ??= new();
      set => searchMonth = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public ObligationType Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of ObligationScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("obligationScreenOwedAmounts")]
    public ScreenOwedAmounts ObligationScreenOwedAmounts
    {
      get => obligationScreenOwedAmounts ??= new();
      set => obligationScreenOwedAmounts = value;
    }

    /// <summary>
    /// A value of ObligationScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("obligationScreenDueAmounts")]
    public ScreenDueAmounts ObligationScreenDueAmounts
    {
      get => obligationScreenDueAmounts ??= new();
      set => obligationScreenDueAmounts = value;
    }

    /// <summary>
    /// A value of LinesWritten.
    /// </summary>
    [JsonPropertyName("linesWritten")]
    public Common LinesWritten
    {
      get => linesWritten ??= new();
      set => linesWritten = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of AdjReasonCode.
    /// </summary>
    [JsonPropertyName("adjReasonCode")]
    public ListScreenWorkArea AdjReasonCode
    {
      get => adjReasonCode ??= new();
      set => adjReasonCode = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of EffectiveDate.
    /// </summary>
    [JsonPropertyName("effectiveDate")]
    public DateWorkArea EffectiveDate
    {
      get => effectiveDate ??= new();
      set => effectiveDate = value;
    }

    /// <summary>
    /// A value of TransactionAmount.
    /// </summary>
    [JsonPropertyName("transactionAmount")]
    public Common TransactionAmount
    {
      get => transactionAmount ??= new();
      set => transactionAmount = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public Common Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of CollectionYearMonth.
    /// </summary>
    [JsonPropertyName("collectionYearMonth")]
    public DateWorkArea CollectionYearMonth
    {
      get => collectionYearMonth ??= new();
      set => collectionYearMonth = value;
    }

    /// <summary>
    /// A value of ObligationTrnType.
    /// </summary>
    [JsonPropertyName("obligationTrnType")]
    public Common ObligationTrnType
    {
      get => obligationTrnType ??= new();
      set => obligationTrnType = value;
    }

    /// <summary>
    /// A value of DebtDue.
    /// </summary>
    [JsonPropertyName("debtDue")]
    public DateWorkArea DebtDue
    {
      get => debtDue ??= new();
      set => debtDue = value;
    }

    /// <summary>
    /// A value of ValidData.
    /// </summary>
    [JsonPropertyName("validData")]
    public Common ValidData
    {
      get => validData ??= new();
      set => validData = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of ScreenWorkArea.
    /// </summary>
    [JsonPropertyName("screenWorkArea")]
    public ListScreenWorkArea ScreenWorkArea
    {
      get => screenWorkArea ??= new();
      set => screenWorkArea = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of FromDate.
    /// </summary>
    [JsonPropertyName("fromDate")]
    public DateWorkArea FromDate
    {
      get => fromDate ??= new();
      set => fromDate = value;
    }

    /// <summary>
    /// A value of ToDate.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DateWorkArea ToDate
    {
      get => toDate ??= new();
      set => toDate = value;
    }

    /// <summary>
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    /// <summary>
    /// A value of HardcodeSupported.
    /// </summary>
    [JsonPropertyName("hardcodeSupported")]
    public CsePersonAccount HardcodeSupported
    {
      get => hardcodeSupported ??= new();
      set => hardcodeSupported = value;
    }

    /// <summary>
    /// A value of HardcodeAccount.
    /// </summary>
    [JsonPropertyName("hardcodeAccount")]
    public MonthlyObligorSummary HardcodeAccount
    {
      get => hardcodeAccount ??= new();
      set => hardcodeAccount = value;
    }

    /// <summary>
    /// Gets a value of ToBeSorted.
    /// </summary>
    [JsonIgnore]
    public Array<ToBeSortedGroup> ToBeSorted => toBeSorted ??= new(
      ToBeSortedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ToBeSorted for json serialization.
    /// </summary>
    [JsonPropertyName("toBeSorted")]
    [Computed]
    public IList<ToBeSortedGroup> ToBeSorted_Json
    {
      get => toBeSorted;
      set => ToBeSorted.Assign(value);
    }

    /// <summary>
    /// Gets a value of Sorted.
    /// </summary>
    [JsonIgnore]
    public Array<SortedGroup> Sorted => sorted ??= new(SortedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Sorted for json serialization.
    /// </summary>
    [JsonPropertyName("sorted")]
    [Computed]
    public IList<SortedGroup> Sorted_Json
    {
      get => sorted;
      set => Sorted.Assign(value);
    }

    /// <summary>
    /// Gets a value of TempIn.
    /// </summary>
    [JsonPropertyName("tempIn")]
    public TempInGroup TempIn
    {
      get => tempIn ?? (tempIn = new());
      set => tempIn = value;
    }

    /// <summary>
    /// Gets a value of TempOut.
    /// </summary>
    [JsonPropertyName("tempOut")]
    public TempOutGroup TempOut
    {
      get => tempOut ?? (tempOut = new());
      set => tempOut = value;
    }

    /// <summary>
    /// Gets a value of SortOrder.
    /// </summary>
    [JsonIgnore]
    public Array<SortOrderGroup> SortOrder => sortOrder ??= new(
      SortOrderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SortOrder for json serialization.
    /// </summary>
    [JsonPropertyName("sortOrder")]
    [Computed]
    public IList<SortOrderGroup> SortOrder_Json
    {
      get => sortOrder;
      set => SortOrder.Assign(value);
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

    private Collection nextPageCollection;
    private Common suppressDebtHeading;
    private Common nextPageDisplayLineInd;
    private DateWorkArea nextPageSearchTo;
    private Obligation previous;
    private Obligation nextPageObligation;
    private DateWorkArea searchDiscontinue;
    private ProgramProcessingInfo programProcessingInfo;
    private Program distributionPgm;
    private CsePersonsWorkSet initializedCsePersonsWorkSet;
    private Common groupSortOrderIndex;
    private LegalAction initializedLegalAction;
    private ProgramScreenAttributes programScreenAttributes;
    private Common noExchanges;
    private Common sortPassCounter;
    private DateWorkArea dateWorkArea;
    private Common searchYear;
    private Common searchMonth;
    private DebtDetail debtDetail;
    private DateWorkArea zero;
    private ObligationType required;
    private ScreenOwedAmounts obligationScreenOwedAmounts;
    private ScreenDueAmounts obligationScreenDueAmounts;
    private Common linesWritten;
    private Obligation obligation;
    private LegalAction legalAction;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private Collection collection;
    private ListScreenWorkArea adjReasonCode;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private DateWorkArea collectionDate;
    private CashReceiptSourceType cashReceiptSourceType;
    private DateWorkArea effectiveDate;
    private Common transactionAmount;
    private Common status;
    private DateWorkArea collectionYearMonth;
    private Common obligationTrnType;
    private DateWorkArea debtDue;
    private Common validData;
    private CsePersonsWorkSet supported;
    private CsePersonsWorkSet obligor;
    private Case1 case1;
    private ListScreenWorkArea screenWorkArea;
    private ServiceProvider serviceProvider;
    private DateWorkArea current;
    private DateWorkArea fromDate;
    private DateWorkArea toDate;
    private CsePersonAccount hardcodeObligor;
    private CsePersonAccount hardcodeSupported;
    private MonthlyObligorSummary hardcodeAccount;
    private Array<ToBeSortedGroup> toBeSorted;
    private Array<SortedGroup> sorted;
    private TempInGroup tempIn;
    private TempOutGroup tempOut;
    private Array<SortOrderGroup> sortOrder;
    private DprProgram dprProgram;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of NonDebt.
    /// </summary>
    [JsonPropertyName("nonDebt")]
    public ObligationTransaction NonDebt
    {
      get => nonDebt ??= new();
      set => nonDebt = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePerson SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    private LegalActionDetail legalActionDetail;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private CsePersonAccount supported;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private Obligation obligation;
    private ObligationType obligationType;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction nonDebt;
    private DebtDetail debtDetail;
    private ObligationTransactionRln obligationTransactionRln;
    private LegalAction legalAction;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CsePerson supportedPerson;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ObligationAssignment obligationAssignment;
  }
#endregion
}
