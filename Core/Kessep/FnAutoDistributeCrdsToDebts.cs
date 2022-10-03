// Program: FN_AUTO_DISTRIBUTE_CRDS_TO_DEBTS, ID: 372279501, model: 746.
// Short name: SWE02364
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AUTO_DISTRIBUTE_CRDS_TO_DEBTS.
/// </summary>
[Serializable]
public partial class FnAutoDistributeCrdsToDebts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AUTO_DISTRIBUTE_CRDS_TO_DEBTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAutoDistributeCrdsToDebts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAutoDistributeCrdsToDebts.
  /// </summary>
  public FnAutoDistributeCrdsToDebts(IContext context, Import import,
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
    // Date	   Developer	  Request #	  Description
    // --------   -------------  -------------	  
    // -------------------------------------------------------------------------------------
    // ?	   ?		  ?		  Original Development.
    // 10/14/05   GVandy	  PR243248	  Reduce cursor open time of main READ EACH 
    // when executing in batch (currently 30+ minutes)
    // 11/01/08   GVandy	  CQ#4387	  Distribution 2009
    // 01/17/14   GVandy	  CQ42413  	  Allow I and U type payments to apply to 
    // Future.
    // 					  Overpayment instructions will not be considered,
    // 					  they will always apply to Future.
    // -------------------------------------------------------------------------------------------------------------------------------
    // : Set-up hardcoded values that are not supported in the hardcoded action 
    // blocks.
    local.HardcodedOverpymtNtcSnt.OverpaymentInd = "N";
    local.HardcodedOverpymtFuture.OverpaymentInd = "F";
    local.HardcodedAType.SequentialIdentifier = 15;
    local.HardcodedCType.SequentialIdentifier = 1;
    local.HardcodedFType.SequentialIdentifier = 3;
    local.HardcodedIType.SequentialIdentifier = 6;
    local.HardcodedKType.SequentialIdentifier = 10;
    local.HardcodedRType.SequentialIdentifier = 11;
    local.HardcodedSType.SequentialIdentifier = 4;
    local.HardcodedTType.SequentialIdentifier = 19;
    local.HardcodedUType.SequentialIdentifier = 5;
    local.HardcodedVType.SequentialIdentifier = 23;
    local.HardcodedYType.SequentialIdentifier = 25;
    local.HardcodedZType.SequentialIdentifier = 26;
    local.Hardcoded4Type.SequentialIdentifier = 14;
    local.Hardcoded5Type.SequentialIdentifier = 20;
    local.Hardcoded6Type.SequentialIdentifier = 28;
    local.Hardcoded9Type.SequentialIdentifier = 29;
    local.HardcodedNaDprProgram.ProgramState = "NA";
    local.HardcodedPa.ProgramState = "PA";
    local.HardcodedTa.ProgramState = "TA";
    local.HardcodedCa.ProgramState = "CA";
    local.HardcodedUd.ProgramState = "UD";
    local.HardcodedUp.ProgramState = "UP";
    local.HardcodedUk.ProgramState = "UK";

    // : Set hardcoded values.
    UseFnHardcodedCashReceipting();
    UseFnHardcodedDebtDistribution();

    // : Set default values.
    local.AutoOrManual.DistributionMethod = "A";
    local.UserId.Text8 = global.UserId;
    local.Current.Date = import.ProcessDate.Date;
    local.Current.Timestamp = Now();
    UseCabFirstAndLastDateOfMonth1();
    local.Current.YearMonth = UseCabGetYearMonthFromDate();
    local.MaximumDiscontinue.Date = UseCabSetMaximumDiscontinueDate();

    // : Build a list of program values to be used in determining the program 
    // for a debt detail.
    UseFnBuildProgramValues();

    // : Build a list of programs.
    local.OfPgms.Index = 0;
    local.OfPgms.Clear();

    foreach(var item in ReadProgram())
    {
      local.OfPgms.Update.OfPgms1.Assign(entities.ExistingProgram);
      local.OfPgms.Next();
    }

    // : Main Loop - Read each undistributed CRD.
    // ------------------------------------------------------------------------------------------------------------
    //  10/14/05  GVandy  PR243248  Performance change to the main read each.  
    // Previous version is commented out below.
    // Also new index CKI09107 added to CR_DETAIL table and attribute 
    // CDS_IDENTIFIER added to index CKI03104 on the CKT_CRDTL_STATHIST table.
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // NOTE:  This change is specifically targeted to improve batch performance.
    // It is recognized that this change will cause a decrease in performance
    // when executed from the DIST screen.  When time permits a second round of
    // changes should be done to bring the online execution back to it's
    // original speed.
    // A possible solution would be to check the import_appl_run_mode and when 
    // it is "ONLINE" then use the old version of the READ EACH (which is very
    // fast due to the parameters passed from the screen) and otherwise ("BATCH"
    // ) use the new version.  The logic inside the READ EACH should be moved to
    // a new cab and then that cab would be USEd inside both the old and new
    // read each.  This would prevent duplication and dual maintenance of the
    // code within the old and new READ EACH's.  Be careful to insure that
    // everything still works the same in batch (specifically the export counts
    // and commits).
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // ------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------
    // Cash Receipt Detail Status
    //    4 = Distributed
    //    5 = Refunded
    //    6 = Released
    // -----------------------------------------------------------------------------------------------------------
    foreach(var item in ReadCashReceiptDetailCashReceiptCashReceiptType())
    {
      // : CRD search limits are ONLY valid when only one CR is selected for the
      // search.
      if (import.StartCashReceipt.SequentialNumber == import
        .EndCashReceipt.SequentialNumber)
      {
        if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
          .StartCashReceiptDetail.SequentialIdentifier || entities
          .ExistingCashReceiptDetail.SequentialIdentifier > import
          .EndCashReceiptDetail.SequentialIdentifier)
        {
          continue;
        }
      }

      export.LastProcessedCashReceipt.SequentialNumber =
        entities.ExistingCashReceipt.SequentialNumber;
      export.LastProcessedCashReceiptDetail.SequentialIdentifier =
        entities.ExistingCashReceiptDetail.SequentialIdentifier;
      ++export.TotalCrdRead.TotalInteger;
      export.TotalAmtAttempted.TotalCurrency += entities.
        ExistingCashReceiptDetail.CollectionAmount - entities
        .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
        .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

      // : Handle DB2 Commits.
      //   The commit frequency is based on the number of CRD
      //   processed, NOT the number of reads & updates!
      ++local.ProcCntForCommit.TotalInteger;

      if (local.ProcCntForCommit.TotalInteger > import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.CommitReturnCode.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }

        ++export.TotalCommitsTaken.TotalInteger;
        local.ProcCntForCommit.TotalInteger = 0;

        if (Equal(import.ApplRunMode.Text8, "BATCH"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "Commit Taken - Time : " + NumberToString
            (TimeToInt(Time(Now())), 15) + " - Read:  " + NumberToString
            (export.TotalCrdRead.TotalInteger, 15) + ", Proc: " + NumberToString
            (export.TotalCrdProcessed.TotalInteger, 15) + ", Commit: " + NumberToString
            (export.TotalCommitsTaken.TotalInteger, 15);
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.NeededToWrite.RptDetail =
            "------------------------------------------------------------------------------------------------------------------------------------";
            
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      // : Now we must verify the Obligor.
      //   If the CRD has been released, the Obligor should be valid,
      //   but we can't count on that!!!
      if (ReadCsePerson2())
      {
        // : Continue Processing.
      }
      else
      {
        export.SuspendedReason.ReasonCodeId = "INVPERSNBR";
        export.SuspendedReason.ReasonText = "Invalid Person Number";
        UseFnSuspendCashRcptDtl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.TotalCrdSuspended.TotalInteger;
        export.TotalAmtSuspended.TotalCurrency += entities.
          ExistingCashReceiptDetail.CollectionAmount - entities
          .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

        continue;
      }

      if (ReadObligor())
      {
        // : Continue Processing.
      }
      else
      {
        export.SuspendedReason.ReasonCodeId = "NOPERSNACC";
        export.SuspendedReason.ReasonText = "No Obligor Account";
        UseFnSuspendCashRcptDtl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.TotalCrdSuspended.TotalInteger;
        export.TotalAmtSuspended.TotalCurrency += entities.
          ExistingCashReceiptDetail.CollectionAmount - entities
          .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

        continue;
      }

      MoveOverpaymentHistory(local.Null1, local.OverpaymentHistory);

      if (ReadOverpaymentHistory())
      {
        MoveOverpaymentHistory(entities.ExistingOverpaymentHistory,
          local.OverpaymentHistory);
      }

      // : Now we must verify the Court Order (if it is on the CRD).
      //   Again, this should be valid if the CRD has been release,
      //   but we can't count on it!!!!
      if (!IsEmpty(entities.ExistingCashReceiptDetail.CourtOrderNumber))
      {
        local.LegalAction.StandardNumber =
          entities.ExistingCashReceiptDetail.CourtOrderNumber;
        UseFnVerifyCourtOrderFilter();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
          export.SuspendedReason.ReasonCodeId = "INVCTORDER";
          export.SuspendedReason.ReasonText = "Invalid Court Order";
          UseFnSuspendCashRcptDtl();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++export.TotalCrdSuspended.TotalInteger;
          export.TotalAmtSuspended.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - entities
            .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

          continue;
        }
      }

      // : Now we must verify the Collection Type.
      //   Again, this should be valid if the CRD has been released,
      //   but we can't count on it!!!!
      if (ReadCollectionType())
      {
        // : Continue Processing.
      }
      else
      {
        export.SuspendedReason.ReasonCodeId = "INVCOLTYPE";
        export.SuspendedReason.ReasonText = "Invalid Collection Type";
        UseFnSuspendCashRcptDtl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.TotalCrdSuspended.TotalInteger;
        export.TotalAmtSuspended.TotalCurrency += entities.
          ExistingCashReceiptDetail.CollectionAmount - entities
          .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

        continue;
      }

      // : Check for Manual Distribution Instructions.
      //   Bypass this check if the Manual Distribution has been overridden.
      if (AsChar(entities.ExistingCashReceiptDetail.OverrideManualDistInd) != 'Y'
        )
      {
        UseFnDetermineMnlDistExist();

        if (AsChar(local.SuspendForMnlDistInst.Flag) == 'Y')
        {
          export.SuspendedReason.ReasonCodeId = "MANUALDIST";
          export.SuspendedReason.ReasonText =
            "Manual Distribution Instructions exist.";
          UseFnSuspendCashRcptDtl();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++export.TotalCrdSuspended.TotalInteger;
          export.TotalAmtSuspended.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - entities
            .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

          continue;
        }
      }
      else
      {
        local.SuspendForMnlDistInst.Flag = "N";
      }

      // : Handle any CRD adjustments.  Basically, read all related adjustments 
      // subtracting the Adj Coll Amt from the Coll Amt.
      local.ForAdjustment.CollectionAmount =
        entities.ExistingCashReceiptDetail.CollectionAmount;

      foreach(var item1 in ReadCashReceiptDetail())
      {
        local.ForAdjustment.CollectionAmount -= entities.ExistingAdjusted.
          CollectionAmount;
        export.TotalAmtAttempted.TotalCurrency -= entities.ExistingAdjusted.
          CollectionAmount;

        if (local.ForAdjustment.CollectionAmount - (
          entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault()) <= 0)
        {
          goto ReadEach;
        }
      }

      local.Collection.Date = entities.ExistingCashReceiptDetail.CollectionDate;
      UseCabFirstAndLastDateOfMonth2();
      local.AmtToDistribute.TotalCurrency =
        local.ForAdjustment.CollectionAmount - (
          entities.ExistingCashReceiptDetail.DistributedAmount.
          GetValueOrDefault() + entities
        .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());

      // : For improved performance, we table up all person/program and URA 
      // balances changes for each supported person prior to attempting
      // distribution.
      UseFnBuildProgramHistoryTable();

      // : Build URA balances by Supported Person/Household/Year/Month.
      //   Only build them if one or more related supported persons have an AF 
      // or FC program in their person/program history.
      local.ProcessHouseholdsInd.Flag = "N";

      for(local.PgmHist.Index = 0; local.PgmHist.Index < local.PgmHist.Count; ++
        local.PgmHist.Index)
      {
        for(local.PgmHist.Item.PgmHistDtl.Index = 0; local
          .PgmHist.Item.PgmHistDtl.Index < local.PgmHist.Item.PgmHistDtl.Count; ++
          local.PgmHist.Item.PgmHistDtl.Index)
        {
          if (local.PgmHist.Item.PgmHistDtl.Item.PgmHistDtlProgram.
            SystemGeneratedIdentifier == local
            .HardcodedAf.SystemGeneratedIdentifier || local
            .PgmHist.Item.PgmHistDtl.Item.PgmHistDtlProgram.
              SystemGeneratedIdentifier == local
            .HardcodedFc.SystemGeneratedIdentifier)
          {
            local.ProcessHouseholdsInd.Flag = "Y";

            goto AfterCycle;
          }
        }
      }

AfterCycle:

      if (AsChar(local.ProcessHouseholdsInd.Flag) == 'N')
      {
        if (ReadDebtDetail())
        {
          local.ProcessHouseholdsInd.Flag = "Y";
        }
      }

      local.HhHist.Index = 0;
      local.HhHist.Clear();

      for(local.HhHistNull.Index = 0; local.HhHistNull.Index < local
        .HhHistNull.Count; ++local.HhHistNull.Index)
      {
        if (local.HhHist.IsFull)
        {
          break;
        }

        local.HhHist.Item.HhHistDtl.Index = 0;
        local.HhHist.Item.HhHistDtl.Clear();

        for(local.HhHistNull.Item.HhHistDtlNull.Index = 0; local
          .HhHistNull.Item.HhHistDtlNull.Index < local
          .HhHistNull.Item.HhHistDtlNull.Count; ++
          local.HhHistNull.Item.HhHistDtlNull.Index)
        {
          if (local.HhHist.Item.HhHistDtl.IsFull)
          {
            break;
          }

          local.HhHist.Item.HhHistDtl.Next();
        }

        local.HhHist.Next();
      }

      local.HhHistSave.Index = 0;
      local.HhHistSave.Clear();

      for(local.HhHistNull.Index = 0; local.HhHistNull.Index < local
        .HhHistNull.Count; ++local.HhHistNull.Index)
      {
        if (local.HhHistSave.IsFull)
        {
          break;
        }

        local.HhHistSave.Item.HhHistDtlSave.Index = 0;
        local.HhHistSave.Item.HhHistDtlSave.Clear();

        for(local.HhHistNull.Item.HhHistDtlNull.Index = 0; local
          .HhHistNull.Item.HhHistDtlNull.Index < local
          .HhHistNull.Item.HhHistDtlNull.Count; ++
          local.HhHistNull.Item.HhHistDtlNull.Index)
        {
          if (local.HhHistSave.Item.HhHistDtlSave.IsFull)
          {
            break;
          }

          local.HhHistSave.Item.HhHistDtlSave.Next();
        }

        local.HhHistSave.Next();
      }

      if (AsChar(local.ProcessHouseholdsInd.Flag) == 'Y')
      {
        UseFnBuildHouseholdHistoryTable();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
          export.SuspendedReason.ReasonText = UseEabExtractExitStateMessage();
          ExitState = "ACO_NN0000_ALL_OK";
          UseFnSuspendCashRcptDtl();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++export.TotalCrdSuspended.TotalInteger;
          export.TotalAmtSuspended.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - entities
            .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

          continue;
        }

        if (local.HhHist.IsEmpty)
        {
          local.ProcessHouseholdsInd.Flag = "N";

          goto Test1;
        }

        local.HhHistSave.Index = 0;
        local.HhHistSave.Clear();

        for(local.HhHist.Index = 0; local.HhHist.Index < local.HhHist.Count; ++
          local.HhHist.Index)
        {
          if (local.HhHistSave.IsFull)
          {
            break;
          }

          local.HhHistSave.Update.HhHistSuppPrsnSave.Number =
            local.HhHist.Item.HhHistSuppPrsn.Number;

          local.HhHistSave.Item.HhHistDtlSave.Index = 0;
          local.HhHistSave.Item.HhHistDtlSave.Clear();

          for(local.HhHist.Item.HhHistDtl.Index = 0; local
            .HhHist.Item.HhHistDtl.Index < local.HhHist.Item.HhHistDtl.Count; ++
            local.HhHist.Item.HhHistDtl.Index)
          {
            if (local.HhHistSave.Item.HhHistDtlSave.IsFull)
            {
              break;
            }

            local.HhHistSave.Update.HhHistDtlSave.Update.
              HhHistDtlSaveImHousehold.AeCaseNo =
                local.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.AeCaseNo;
            local.HhHistSave.Update.HhHistDtlSave.Update.
              HhHistDtlSaveImHouseholdMbrMnthlySum.Assign(
                local.HhHist.Item.HhHistDtl.Item.
                HhHistDtlImHouseholdMbrMnthlySum);
            local.HhHistSave.Item.HhHistDtlSave.Next();
          }

          local.HhHistSave.Next();
        }
      }

Test1:

      local.Group.Index = -1;
      local.Group.Count = 0;
      local.RedistByCourtOrderInd.Flag = "N";

      // : Check to see if the Collection Type is "V" (Voluntary).
      //   If so,  Distribution can distribute the payment ONLY to Voluntary 
      // Obligations.
      if (entities.ExistingCollectionType.SequentialIdentifier == local
        .HardcodedVType.SequentialIdentifier)
      {
        UseFnDistCrdBalanceToVolOblig();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
          export.SuspendedReason.ReasonText = UseEabExtractExitStateMessage();
          ExitState = "ACO_NN0000_ALL_OK";
          UseFnSuspendCashRcptDtl();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++export.TotalCrdSuspended.TotalInteger;
          export.TotalAmtSuspended.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - entities
            .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

          continue;
        }

        export.TotalAmtProcessed.TotalCurrency += local.AmtDistributed.
          TotalCurrency;
      }
      else
      {
        // : Before we can begin looking at Debts to potentially distribute 
        // against, we must first look to see if there have been any previous
        // distributions that have been adjusted back off.  If so, then we must
        // apply the payment back to the same Court Order.
        local.Redist.Index = -1;
        local.Redist.Count = 0;

        if (IsEmpty(entities.ExistingCashReceiptDetail.CourtOrderNumber))
        {
          // *********************************************************************************
          // Determine if the AP number on the CRD has been altered since the 
          // last time the CRD was distributed.
          // *********************************************************************************
          if (ReadCsePerson1())
          {
            // : Continue Processing.
          }
          else
          {
            // : When the AP on the CRD does not match the AP on previous 
            // collections, treat the CRD as if it had never been previously
            // distributed.
            goto Test2;
          }

          // *********************************************************************************
          // Determine the court orders and amounts of previous distributions on
          // this cash receipt detail.
          // *********************************************************************************
          UseFnDetermineRedistAmts();

          if (!local.Redist.IsEmpty)
          {
            local.RedistByCourtOrderInd.Flag = "Y";
          }
        }
        else
        {
          UseFnCheckForSecondaryOblig();
        }

Test2:

        if (local.Redist.IsEmpty)
        {
          local.Redist.Index = 0;
          local.Redist.CheckSize();

          local.Redist.Update.Redist1.Amount =
            local.AmtToDistribute.TotalCurrency;
          local.Redist.Update.Redist1.CourtOrderAppliedTo =
            entities.ExistingCashReceiptDetail.CourtOrderNumber;
        }

        local.Redist.Index = 0;

        for(var limit = local.Redist.Count; local.Redist.Index < limit; ++
          local.Redist.Index)
        {
          if (!local.Redist.CheckSize())
          {
            break;
          }

          if (local.Redist.Item.Redist1.Amount == 0)
          {
            continue;
          }

          local.FutureApplAllowed.Flag = "N";
          local.ProcessSecObligOnly.Flag = "N";
          local.AmtToDistribute.TotalCurrency =
            local.Redist.Item.Redist1.Amount;
          local.DistBy.CourtOrderNumber =
            local.Redist.Item.Redist1.CourtOrderAppliedTo ?? "";

          // : Determine group of debt details to distribute the money too (
          // Excluding the Secondary Obligations).
          UseFnDistCrdBalanceByDistPlcy1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
            export.SuspendedReason.ReasonText = UseEabExtractExitStateMessage();
            ExitState = "ACO_NN0000_ALL_OK";
            UseFnSuspendCashRcptDtl();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            ++export.TotalCrdSuspended.TotalInteger;
            export.TotalAmtSuspended.TotalCurrency += entities.
              ExistingCashReceiptDetail.CollectionAmount - entities
              .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() -
              entities
              .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

            goto ReadEach;
          }

          export.TotalAmtProcessed.TotalCurrency += local.AmtDistributed.
            TotalCurrency;
          local.AmtToDistribute.TotalCurrency -= local.AmtDistributed.
            TotalCurrency;

          // : If we have distributed something, we need to see if there needs 
          // to be any money distributed to the secondary's.
          if (local.AmtDistributedToPrim.TotalCurrency > 0)
          {
            // : Determine group of debt details to distribute the money too (
            // Secondary Obligations ONLY).
            local.FutureApplAllowed.Flag = "N";
            local.ProcessSecObligOnly.Flag = "Y";

            for(local.PrimarySummary.Index = 0; local.PrimarySummary.Index < local
              .PrimarySummary.Count; ++local.PrimarySummary.Index)
            {
              local.AmtToDistributeToSec.TotalCurrency =
                local.PrimarySummary.Item.PrimSummCommon.TotalCurrency;
              UseFnDistCrdBalanceByDistPlcy2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
                export.SuspendedReason.ReasonText =
                  UseEabExtractExitStateMessage();
                ExitState = "ACO_NN0000_ALL_OK";
                UseFnSuspendCashRcptDtl();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                ++export.TotalCrdSuspended.TotalInteger;
                export.TotalAmtSuspended.TotalCurrency += entities.
                  ExistingCashReceiptDetail.CollectionAmount - entities
                  .ExistingCashReceiptDetail.DistributedAmount.
                    GetValueOrDefault() - entities
                  .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
                  

                goto ReadEach;
              }
            }
          }

          // : Check to see if we distributed all of the collection.
          //   If not, we need to see what the overpayment intent is for "C" 
          // type Collections ONLY!!!!
          // -- 01/17/2014  GVandy  CQ42413  Allow I and U type payments to 
          // apply to Future.
          //    Overpayment instructions will not be considered, they will 
          // always apply to Future.
          if (entities.ExistingCollectionType.SequentialIdentifier == local
            .HardcodedCType.SequentialIdentifier || entities
            .ExistingCollectionType.SequentialIdentifier == local
            .HardcodedIType.SequentialIdentifier || entities
            .ExistingCollectionType.SequentialIdentifier == local
            .HardcodedUType.SequentialIdentifier)
          {
            // : If the payment is a REIP payment, bypass overpayment intent 
            // processing!!!
            if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
              .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
              .ExistingCashReceiptType.SystemGeneratedIdentifier == local
              .HardcodedFdirPmt.SystemGeneratedIdentifier)
            {
              goto Test3;
            }

            if (local.AmtToDistribute.TotalCurrency > 0)
            {
              if (entities.ExistingCollectionType.SequentialIdentifier == local
                .HardcodedIType.SequentialIdentifier || entities
                .ExistingCollectionType.SequentialIdentifier == local
                .HardcodedUType.SequentialIdentifier)
              {
                // -- I and U type payments should always apply to Future, 
                // regardless of overpayment instructions.
                //    Set the local overpayment_history overpayment_ind to 
                // spaces so the CASE below
                //    will fall into the OTHERWISE logic and apply as FUTURE.
                local.OverpaymentHistory.OverpaymentInd = "";
              }

              switch(AsChar(local.OverpaymentHistory.OverpaymentInd))
              {
                case 'R':
                  // : No processing here for Refunds.  Fall through and 
                  // suspend.
                  break;
                case 'G':
                  local.ProcessSecObligOnly.Flag = "N";
                  UseFnDistCrdBalanceAsGift();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
                    export.SuspendedReason.ReasonText =
                      UseEabExtractExitStateMessage();
                    ExitState = "ACO_NN0000_ALL_OK";
                    UseFnSuspendCashRcptDtl();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }

                    ++export.TotalCrdSuspended.TotalInteger;
                    export.TotalAmtSuspended.TotalCurrency += entities.
                      ExistingCashReceiptDetail.CollectionAmount - entities
                      .ExistingCashReceiptDetail.DistributedAmount.
                        GetValueOrDefault() - entities
                      .ExistingCashReceiptDetail.RefundedAmount.
                        GetValueOrDefault();

                    goto ReadEach;
                  }

                  export.TotalAmtProcessed.TotalCurrency += local.
                    AmtDistributed.TotalCurrency;
                  local.AmtToDistribute.TotalCurrency -= local.AmtDistributed.
                    TotalCurrency;

                  break;
                default:
                  local.FutureApplAllowed.Flag = "Y";
                  local.ProcessSecObligOnly.Flag = "N";
                  UseFnDistCrdBalanceByDistPlcy1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
                    export.SuspendedReason.ReasonText =
                      UseEabExtractExitStateMessage();
                    ExitState = "ACO_NN0000_ALL_OK";
                    UseFnSuspendCashRcptDtl();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }

                    ++export.TotalCrdSuspended.TotalInteger;
                    export.TotalAmtSuspended.TotalCurrency += entities.
                      ExistingCashReceiptDetail.CollectionAmount - entities
                      .ExistingCashReceiptDetail.DistributedAmount.
                        GetValueOrDefault() - entities
                      .ExistingCashReceiptDetail.RefundedAmount.
                        GetValueOrDefault();

                    goto ReadEach;
                  }

                  export.TotalAmtProcessed.TotalCurrency += local.
                    AmtDistributed.TotalCurrency;
                  local.AmtToDistribute.TotalCurrency -= local.AmtDistributed.
                    TotalCurrency;

                  // : If we have distributed something, we need to see if there
                  // needs to be any money distributed to the secondary's.
                  if (local.AmtDistributedToPrim.TotalCurrency > 0)
                  {
                    // : Determine group of debt details to distribute the money
                    // too (Secondary Obligations ONLY).
                    local.ProcessSecObligOnly.Flag = "Y";

                    for(local.PrimarySummary.Index = 0; local
                      .PrimarySummary.Index < local.PrimarySummary.Count; ++
                      local.PrimarySummary.Index)
                    {
                      local.AmtToDistributeToSec.TotalCurrency =
                        local.PrimarySummary.Item.PrimSummCommon.TotalCurrency;
                      UseFnDistCrdBalanceByDistPlcy2();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
                        export.SuspendedReason.ReasonText =
                          UseEabExtractExitStateMessage();
                        ExitState = "ACO_NN0000_ALL_OK";
                        UseFnSuspendCashRcptDtl();

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          return;
                        }

                        ++export.TotalCrdSuspended.TotalInteger;
                        export.TotalAmtSuspended.TotalCurrency += entities.
                          ExistingCashReceiptDetail.CollectionAmount - entities
                          .ExistingCashReceiptDetail.DistributedAmount.
                            GetValueOrDefault() - entities
                          .ExistingCashReceiptDetail.RefundedAmount.
                            GetValueOrDefault();

                        goto ReadEach;
                      }
                    }
                  }

                  break;
              }
            }
          }

Test3:
          ;
        }

        local.Redist.CheckIndex();
      }

      if (!local.Group.IsEmpty)
      {
        local.DistributedAmount.TotalCurrency = 0;
        local.Group.Index = 0;

        for(var limit = local.Group.Count; local.Group.Index < limit; ++
          local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          // *****   Sum up primaries & normal collections     *****
          if (AsChar(local.Group.Item.Obligation.PrimarySecondaryCode) != AsChar
            (local.HardcodedSecondary.PrimarySecondaryCode))
          {
            // *****   Suspend negative collection amounts.    *****
            if (local.Group.Item.Collection.Amount < 0)
            {
              export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
              export.SuspendedReason.ReasonText =
                "An attempt to create a Collection with a Negative Collection Amount has resulted in an error.";
                
              UseFnSuspendCashRcptDtl();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              ++export.TotalCrdSuspended.TotalInteger;
              export.TotalAmtSuspended.TotalCurrency += entities.
                ExistingCashReceiptDetail.CollectionAmount - entities
                .ExistingCashReceiptDetail.DistributedAmount.
                  GetValueOrDefault() - entities
                .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

              goto ReadEach;
            }

            local.DistributedAmount.TotalCurrency += local.Group.Item.
              Collection.Amount;
          }
        }

        local.Group.CheckIndex();

        // : We've got something to distribute!!!!  Now do it!
        if (local.DistributedAmount.TotalCurrency > local
          .ForAdjustment.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault()))
        {
          export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
          export.SuspendedReason.ReasonText =
            "Amount prepared for distribution is greater than the amount to distribute.";
            
          UseFnSuspendCashRcptDtl();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++export.TotalCrdSuspended.TotalInteger;
          export.TotalAmtSuspended.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - entities
            .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();

          continue;
        }

        if (AsChar(local.ProcessHouseholdsInd.Flag) == 'Y')
        {
          local.HhHist.Index = 0;
          local.HhHist.Clear();

          for(local.HhHistSave.Index = 0; local.HhHistSave.Index < local
            .HhHistSave.Count; ++local.HhHistSave.Index)
          {
            if (local.HhHist.IsFull)
            {
              break;
            }

            local.HhHist.Update.HhHistSuppPrsn.Number =
              local.HhHistSave.Item.HhHistSuppPrsnSave.Number;

            local.HhHist.Item.HhHistDtl.Index = 0;
            local.HhHist.Item.HhHistDtl.Clear();

            for(local.HhHistSave.Item.HhHistDtlSave.Index = 0; local
              .HhHistSave.Item.HhHistDtlSave.Index < local
              .HhHistSave.Item.HhHistDtlSave.Count; ++
              local.HhHistSave.Item.HhHistDtlSave.Index)
            {
              if (local.HhHist.Item.HhHistDtl.IsFull)
              {
                break;
              }

              local.HhHist.Update.HhHistDtl.Update.HhHistDtlImHousehold.
                AeCaseNo =
                  local.HhHistSave.Item.HhHistDtlSave.Item.
                  HhHistDtlSaveImHousehold.AeCaseNo;
              local.HhHist.Update.HhHistDtl.Update.
                HhHistDtlImHouseholdMbrMnthlySum.Assign(
                  local.HhHistSave.Item.HhHistDtlSave.Item.
                  HhHistDtlSaveImHouseholdMbrMnthlySum);
              local.HhHist.Item.HhHistDtl.Next();
            }

            local.HhHist.Next();
          }
        }

        local.CollectionObligor.Number =
          entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
        UseFnProcessDistributionRequest();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.TotalCrdProcessed.TotalInteger;
      }

      // : Did we fully distribute the CRD?
      //   If not, we need to look at the overpayment intent and then suspend 
      // the CRD!
      if (AsChar(entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd)
        != 'Y')
      {
        if (local.Group.IsFull)
        {
          export.SuspendedReason.ReasonCodeId = "SYSTEMERR";
          export.SuspendedReason.ReasonText =
            "Maximum size of an internal group has been met - Partial distribution occurred.";
            
        }
        else if (entities.ExistingCollectionType.SequentialIdentifier == local
          .HardcodedCType.SequentialIdentifier)
        {
          // : We only look at "C" type collections in regards to overpayment 
          // intent.
          // : Determine if there are any open debts or if the obligor is paid 
          // in full (PIF).
          local.Obligor.Number =
            entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? Spaces
            (10);
          UseFnDetermineObligorDebtStatus();

          if (AsChar(local.PifInd.Flag) == 'Y')
          {
            if (AsChar(local.OverpaymentHistory.OverpaymentInd) == 'R')
            {
              export.SuspendedReason.ReasonCodeId = "OPMTREFUND";
              export.SuspendedReason.ReasonText =
                "Overpayment Instructions state a refund is required.";
              ++export.TotalEventsRaised.TotalInteger;
            }
            else
            {
              export.SuspendedReason.ReasonCodeId = "DEBTPIF";
              export.SuspendedReason.ReasonText =
                "Debt Paid In Full - No active debts remain for distribution of the CRD.";
                
              ++export.TotalEventsRaised.TotalInteger;
            }
          }
          else
          {
            switch(AsChar(local.OverpaymentHistory.OverpaymentInd))
            {
              case 'G':
                export.SuspendedReason.ReasonCodeId = "NODEBTOPMT";
                export.SuspendedReason.ReasonText =
                  "No active Debts found for distribution of the CRD based on the Gift Overpayment Instructions.";
                  
                ++export.TotalEventsRaised.TotalInteger;

                break;
              case 'R':
                export.SuspendedReason.ReasonCodeId = "OPMTREFUND";
                export.SuspendedReason.ReasonText =
                  "Overpayment Instructions state a refund is required.";
                ++export.TotalEventsRaised.TotalInteger;

                break;
              default:
                export.SuspendedReason.ReasonCodeId = "FUTURE";
                export.SuspendedReason.ReasonText =
                  "No active Debts found for distribution of the CRD.";
                ++export.TotalEventsRaised.TotalInteger;

                break;
            }
          }
        }
        else if (entities.ExistingCollectionType.SequentialIdentifier == local
          .HardcodedAType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedIType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedKType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedSType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedTType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedUType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedVType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedYType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .HardcodedZType.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .Hardcoded4Type.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .Hardcoded5Type.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .Hardcoded6Type.SequentialIdentifier || entities
          .ExistingCollectionType.SequentialIdentifier == local
          .Hardcoded9Type.SequentialIdentifier)
        {
          // : We only look at A, I, K, S, T, U, Y, Z, 4, 5, 6, 9  type 
          // collections.
          // : Determine if there are any open debts or if the obligor is paid 
          // in full (PIF).
          local.Obligor.Number =
            entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? Spaces
            (10);
          UseFnDetermineObligorDebtStatus();

          if (AsChar(local.PifInd.Flag) == 'Y')
          {
            export.SuspendedReason.ReasonCodeId = "DEBTPIF";
            export.SuspendedReason.ReasonText =
              "Debt Paid In Full - No active debts remain for distribution of the CRD.";
              
            ++export.TotalEventsRaised.TotalInteger;
          }
          else if (IsEmpty(entities.ExistingCashReceiptDetail.CourtOrderNumber))
          {
            if (AsChar(local.RedistByCourtOrderInd.Flag) == 'Y')
            {
              export.SuspendedReason.ReasonCodeId = "MANREAPPLY";
              export.SuspendedReason.ReasonText =
                "No active Debts for the person within the Court Order being Reapplied.";
                
            }
            else
            {
              export.SuspendedReason.ReasonCodeId = "FUTURE";
              export.SuspendedReason.ReasonText =
                "No active Debts for the person.";
            }

            ++export.TotalEventsRaised.TotalInteger;
          }
          else
          {
            export.SuspendedReason.ReasonCodeId = "FUTURE";
            export.SuspendedReason.ReasonText =
              "No active Debts for the Court Order.";
            ++export.TotalEventsRaised.TotalInteger;
          }
        }
        else if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
          .ExistingCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFdirPmt.SystemGeneratedIdentifier)
        {
          export.SuspendedReason.ReasonCodeId = "NODEBTREIP";
          export.SuspendedReason.ReasonText =
            "No Debts exist for distribution of the REIP CRD.";
          ++export.TotalEventsRaised.TotalInteger;
        }
        else
        {
          // : Determine if there are any open debts or if the obligor is paid 
          // in full (PIF).
          local.Obligor.Number =
            entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? Spaces
            (10);
          UseFnDetermineObligorDebtStatus();

          if (AsChar(local.PifInd.Flag) == 'Y')
          {
            export.SuspendedReason.ReasonCodeId = "DEBTPIF";
            export.SuspendedReason.ReasonText =
              "Debt Paid In Full - No active debts remain for distribution of the CRD.";
              
            ++export.TotalEventsRaised.TotalInteger;
          }
          else if (IsEmpty(entities.ExistingCashReceiptDetail.CourtOrderNumber))
          {
            if (AsChar(local.RedistByCourtOrderInd.Flag) == 'Y')
            {
              export.SuspendedReason.ReasonCodeId = "MANREAPPLY";
              export.SuspendedReason.ReasonText =
                "No active Debts for the person within the Court Order being Reapplied.";
                
            }
            else
            {
              export.SuspendedReason.ReasonCodeId = "NODEBTPERS";
              export.SuspendedReason.ReasonText =
                "No active Debts for the person.";
            }

            ++export.TotalEventsRaised.TotalInteger;
          }
          else
          {
            export.SuspendedReason.ReasonCodeId = "NODEBTCTOR";
            export.SuspendedReason.ReasonText =
              "No active Debts for the Court Order.";
            ++export.TotalEventsRaised.TotalInteger;
          }
        }

        UseFnSuspendCashRcptDtl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.TotalCrdSuspended.TotalInteger;
        export.TotalAmtSuspended.TotalCurrency += entities.
          ExistingCashReceiptDetail.CollectionAmount - entities
          .ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
      }

ReadEach:
      ;
    }

    if (!local.Group.IsEmpty)
    {
      export.Group.Index = -1;

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

        ++export.Group.Index;
        export.Group.CheckSize();

        MoveCollection1(local.Group.Item.Collection,
          export.Group.Update.Collection);
        export.Group.Update.Debt.SystemGeneratedIdentifier =
          local.Group.Item.Debt.SystemGeneratedIdentifier;
        export.Group.Update.DprProgram.ProgramState =
          local.Group.Item.DprProgram.ProgramState;
        export.Group.Update.Obligation.Assign(local.Group.Item.Obligation);
        export.Group.Update.ObligationType.Assign(
          local.Group.Item.ObligationType);
        export.Group.Update.Program.Assign(local.Group.Item.Program);
        export.Group.Update.SuppPrsn.Number = local.Group.Item.SuppPrsn.Number;

        if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
        {
          return;
        }
      }

      local.Group.CheckIndex();
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
    target.OverrideManualDistInd = source.OverrideManualDistInd;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MoveCashReceiptDetail3(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CourtInd = source.CourtInd;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CategoryIndicator = source.CategoryIndicator;
  }

  private static void MoveCollection1(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
  }

  private static void MoveCsePersonAccount(CsePersonAccount source,
    CsePersonAccount target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.LastManualDistributionDate = source.LastManualDistributionDate;
    target.LastCollAmt = source.LastCollAmt;
    target.LastCollDt = source.LastCollDt;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveGroup1(Local.GroupGroup source,
    FnDistCrdBalanceAsGift.Import.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection1(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroup2(Local.GroupGroup source,
    FnProcessDistributionRequest.Import.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection1(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroup3(Local.GroupGroup source,
    FnDistCrdBalanceToVolOblig.Import.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection1(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroup4(Local.GroupGroup source,
    FnDistCrdBalanceByDistPlcy.Import.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection1(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroup5(FnDistCrdBalanceAsGift.Import.
    GroupGroup source, Local.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection1(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroup6(FnDistCrdBalanceToVolOblig.Import.
    GroupGroup source, Local.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection1(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroup7(FnDistCrdBalanceByDistPlcy.Import.
    GroupGroup source, Local.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection1(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private static void MoveGroupToRedist1(FnCheckForSecondaryOblig.Export.
    GroupGroup source, Local.RedistGroup target)
  {
    MoveCollection2(source.Collection, target.Redist1);
  }

  private static void MoveGroupToRedist2(FnDetermineRedistAmts.Export.
    GroupGroup source, Local.RedistGroup target)
  {
    MoveCollection2(source.Collection, target.Redist1);
  }

  private static void MoveHhHist1(FnBuildHouseholdHistoryTable.Export.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(Local.HhHistGroup source,
    FnDistCrdBalanceByDistPlcy.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl2);
  }

  private static void MoveHhHist3(Local.HhHistGroup source,
    FnProcessDistributionRequest.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl3);
  }

  private static void MoveHhHist4(Local.HhHistGroup source,
    FnDistCrdBalanceToVolOblig.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl4);
  }

  private static void MoveHhHist5(Local.HhHistGroup source,
    FnDistCrdBalanceAsGift.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl5);
  }

  private static void MoveHhHist6(FnDistCrdBalanceByDistPlcy.Import.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl6);
  }

  private static void MoveHhHist7(FnProcessDistributionRequest.Import.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl7);
  }

  private static void MoveHhHist8(FnDistCrdBalanceToVolOblig.Import.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl8);
  }

  private static void MoveHhHist9(FnDistCrdBalanceAsGift.Import.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl9);
  }

  private static void MoveHhHistDtl1(FnBuildHouseholdHistoryTable.Export.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(Local.HhHistDtlGroup source,
    FnDistCrdBalanceByDistPlcy.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl3(Local.HhHistDtlGroup source,
    FnProcessDistributionRequest.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl4(Local.HhHistDtlGroup source,
    FnDistCrdBalanceToVolOblig.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl5(Local.HhHistDtlGroup source,
    FnDistCrdBalanceAsGift.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl6(FnDistCrdBalanceByDistPlcy.Import.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl7(FnProcessDistributionRequest.Import.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl8(FnDistCrdBalanceToVolOblig.Import.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl9(FnDistCrdBalanceAsGift.Import.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveLegal1(FnBuildHouseholdHistoryTable.Export.
    LegalGroup source, Local.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl1);
  }

  private static void MoveLegal2(Local.LegalGroup source,
    FnDistCrdBalanceByDistPlcy.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl2);
  }

  private static void MoveLegal3(Local.LegalGroup source,
    FnProcessDistributionRequest.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl3);
  }

  private static void MoveLegal4(Local.LegalGroup source,
    FnDistCrdBalanceToVolOblig.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl4);
  }

  private static void MoveLegal5(Local.LegalGroup source,
    FnDistCrdBalanceAsGift.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl5);
  }

  private static void MoveLegalDtl1(FnBuildHouseholdHistoryTable.Export.
    LegalDtlGroup source, Local.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl2(Local.LegalDtlGroup source,
    FnDistCrdBalanceByDistPlcy.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl3(Local.LegalDtlGroup source,
    FnProcessDistributionRequest.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl4(Local.LegalDtlGroup source,
    FnDistCrdBalanceToVolOblig.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl5(Local.LegalDtlGroup source,
    FnDistCrdBalanceAsGift.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveOfPgms(Local.OfPgmsGroup source,
    FnBuildProgramHistoryTable.Import.OfPgmsGroup target)
  {
    target.OfPgms1.Assign(source.OfPgms1);
  }

  private static void MoveOverpaymentHistory(OverpaymentHistory source,
    OverpaymentHistory target)
  {
    target.OverpaymentInd = source.OverpaymentInd;
    target.EffectiveDt = source.EffectiveDt;
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePgmHist1(Local.PgmHistGroup source,
    FnDistCrdBalanceAsGift.Import.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl1);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHist2(Local.PgmHistGroup source,
    FnDistCrdBalanceToVolOblig.Import.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl2);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHist3(Local.PgmHistGroup source,
    FnDistCrdBalanceByDistPlcy.Import.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl3);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHist4(FnBuildProgramHistoryTable.Export.
    PgmHistGroup source, Local.PgmHistGroup target)
  {
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl4);
    target.TafInd.Flag = source.TafInd.Flag;
  }

  private static void MovePgmHistDtl1(Local.PgmHistDtlGroup source,
    FnDistCrdBalanceAsGift.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MovePgmHistDtl2(Local.PgmHistDtlGroup source,
    FnDistCrdBalanceToVolOblig.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MovePgmHistDtl3(Local.PgmHistDtlGroup source,
    FnDistCrdBalanceByDistPlcy.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MovePgmHistDtl4(FnBuildProgramHistoryTable.Export.
    PgmHistDtlGroup source, Local.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MovePrimSummaryToPrimarySummary(
    FnDistCrdBalanceByDistPlcy.Export.PrimSummaryGroup source,
    Local.PrimarySummaryGroup target)
  {
    target.PrimSummObligationType.SystemGeneratedIdentifier =
      source.Ps.SystemGeneratedIdentifier;
    target.PrimSummObligation.SystemGeneratedIdentifier =
      source.Obligation.SystemGeneratedIdentifier;
    target.PrimSummCommon.TotalCurrency = source.ByObligation.TotalCurrency;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.CurrentMonthStart.Date = useExport.First.Date;
    local.CurrentMonthEnd.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Collection.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.CollMonthStart.Date = useExport.First.Date;
    local.CollMonthEnd.Date = useExport.Last.Date;
  }

  private int UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode =
      local.CommitReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.CommitReturnCode.NumericReturnCode =
      useExport.External.NumericReturnCode;
  }

  private void UseFnBuildHouseholdHistoryTable()
  {
    var useImport = new FnBuildHouseholdHistoryTable.Import();
    var useExport = new FnBuildHouseholdHistoryTable.Export();

    MoveCashReceiptDetail2(entities.ExistingCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.Obligor.Number = entities.ExistingKeyOnlyObligor.Number;

    Call(FnBuildHouseholdHistoryTable.Execute, useImport, useExport);

    useExport.Legal.CopyTo(local.Legal, MoveLegal1);
    useExport.HhHist.CopyTo(local.HhHist, MoveHhHist1);
  }

  private void UseFnBuildProgramHistoryTable()
  {
    var useImport = new FnBuildProgramHistoryTable.Import();
    var useExport = new FnBuildProgramHistoryTable.Export();

    useImport.Obligor.Number = entities.ExistingKeyOnlyObligor.Number;
    useImport.MaximumDiscontinue.Date = local.MaximumDiscontinue.Date;
    local.OfPgms.CopyTo(useImport.OfPgms, MoveOfPgms);
    useImport.CashReceiptDetail.CollectionDate =
      entities.ExistingCashReceiptDetail.CollectionDate;

    Call(FnBuildProgramHistoryTable.Execute, useImport, useExport);

    useExport.PgmHist.CopyTo(local.PgmHist, MovePgmHist4);
  }

  private void UseFnBuildProgramValues()
  {
    var useImport = new FnBuildProgramValues.Import();
    var useExport = new FnBuildProgramValues.Export();

    Call(FnBuildProgramValues.Execute, useImport, useExport);

    local.HardcodedAf.Assign(useExport.HardcodedAf);
    local.HardcodedAfi.Assign(useExport.HardcodedAfi);
    local.HardcodedFc.Assign(useExport.HardcodedFc);
    local.HardcodedFci.Assign(useExport.HardcodedFci);
    local.HardcodedNaProgram.Assign(useExport.HardcodedNa);
    local.HardcodedNai.Assign(useExport.HardcodedNai);
    local.HardcodedNc.Assign(useExport.HardcodedNc);
    local.HardcodedNf.Assign(useExport.HardcodedNf);
    local.HardcodedMai.Assign(useExport.HardcodedMai);
  }

  private void UseFnCheckForSecondaryOblig()
  {
    var useImport = new FnCheckForSecondaryOblig.Import();
    var useExport = new FnCheckForSecondaryOblig.Export();

    useImport.Persistant.Assign(entities.ExistingCashReceiptDetail);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistribute.TotalCurrency;
    useImport.HardcodedPriSec.SequentialGeneratedIdentifier =
      local.HardcodedPriSec.SequentialGeneratedIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;

    Call(FnCheckForSecondaryOblig.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.Persistant,
      entities.ExistingCashReceiptDetail);
    useExport.Group.CopyTo(local.Redist, MoveGroupToRedist1);
  }

  private void UseFnDetermineMnlDistExist()
  {
    var useImport = new FnDetermineMnlDistExist.Import();
    var useExport = new FnDetermineMnlDistExist.Export();

    MoveCashReceiptDetail3(entities.ExistingCashReceiptDetail,
      useImport.CashReceiptDetail);

    Call(FnDetermineMnlDistExist.Execute, useImport, useExport);

    local.SuspendForMnlDistInst.Flag = useExport.SuspendForMnlDistInst.Flag;
  }

  private void UseFnDetermineObligorDebtStatus()
  {
    var useImport = new FnDetermineObligorDebtStatus.Import();
    var useExport = new FnDetermineObligorDebtStatus.Export();

    useImport.Obligor.Number = local.Obligor.Number;
    useImport.Process.Date = local.CollMonthEnd.Date;

    Call(FnDetermineObligorDebtStatus.Execute, useImport, useExport);

    local.PifInd.Flag = useExport.PifInd.Flag;
  }

  private void UseFnDetermineRedistAmts()
  {
    var useImport = new FnDetermineRedistAmts.Import();
    var useExport = new FnDetermineRedistAmts.Export();

    useImport.Persistant.Assign(entities.ExistingCashReceiptDetail);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistribute.TotalCurrency;
    useImport.HardcodedWrongAcct.SystemGeneratedIdentifier =
      local.HardcodedWrongAcct.SystemGeneratedIdentifier;

    Call(FnDetermineRedistAmts.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.Persistant,
      entities.ExistingCashReceiptDetail);
    useExport.Group.CopyTo(local.Redist, MoveGroupToRedist2);
  }

  private void UseFnDistCrdBalanceAsGift()
  {
    var useImport = new FnDistCrdBalanceAsGift.Import();
    var useExport = new FnDistCrdBalanceAsGift.Export();

    useImport.PersistantCashReceiptDetail.Assign(
      entities.ExistingCashReceiptDetail);
    useImport.CollectionType.SequentialIdentifier =
      entities.ExistingCollectionType.SequentialIdentifier;
    useImport.Obligor.Number = entities.ExistingKeyOnlyObligor.Number;
    useImport.PersistantObligor.Assign(entities.ExistingObligor);
    useImport.PrworaDateOfConversion.Date = import.PrworaDateOfConversion.Date;
    local.Group.CopyTo(useImport.Group, MoveGroup1);
    local.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist1);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistribute.TotalCurrency;
    useImport.DistBy.CourtOrderNumber = local.DistBy.CourtOrderNumber;
    useImport.CurrentMonthStart.Date = local.CurrentMonthStart.Date;
    useImport.CurrentMonthEnd.Date = local.CurrentMonthEnd.Date;
    useImport.ProcessSecObligOnly.Flag = local.ProcessSecObligOnly.Flag;
    useImport.HardcodedFFedType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;
    useImport.HardcodedVolClass.Classification =
      local.HardcodedVolClass.Classification;
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.HardcodedPrimary.PrimarySecondaryCode =
      local.HardcodedPrimary.PrimarySecondaryCode;
    useImport.HardcodedCsType.SystemGeneratedIdentifier =
      local.HardcodedCsType.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718BType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(local.HardcodedAf);
    useImport.HardcodedAfi.Assign(local.HardcodedAfi);
    useImport.HardcodedFc.Assign(local.HardcodedFc);
    useImport.HardcodedFci.Assign(local.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(local.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(local.HardcodedNai);
    useImport.HardcodedNc.Assign(local.HardcodedNc);
    useImport.HardcodedNf.Assign(local.HardcodedNf);
    useImport.HardcodedMai.Assign(local.HardcodedMai);
    local.Legal.CopyTo(useImport.Legal, MoveLegal5);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist5);
    useImport.HardcodedNaDprProgram.ProgramState =
      local.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedPa.ProgramState = local.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = local.HardcodedTa.ProgramState;
    useImport.HardcodedCa.ProgramState = local.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = local.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = local.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;

    Call(FnDistCrdBalanceAsGift.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.PersistantCashReceiptDetail,
      entities.ExistingCashReceiptDetail);
    MoveCsePersonAccount(useImport.PersistantObligor, entities.ExistingObligor);
    useImport.Group.CopyTo(local.Group, MoveGroup5);
    useImport.HhHist.CopyTo(local.HhHist, MoveHhHist9);
    local.AmtDistributed.TotalCurrency = useExport.AmtDistributed.TotalCurrency;
  }

  private void UseFnDistCrdBalanceByDistPlcy1()
  {
    var useImport = new FnDistCrdBalanceByDistPlcy.Import();
    var useExport = new FnDistCrdBalanceByDistPlcy.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.ExistingCashReceiptType.SystemGeneratedIdentifier;
    useImport.PersistantCashReceiptDetail.Assign(
      entities.ExistingCashReceiptDetail);
    useImport.PersistantCollectionType.Assign(entities.ExistingCollectionType);
    useImport.PrworaDateOfConversion.Date = import.PrworaDateOfConversion.Date;
    useImport.AllowITypeProcInd.Flag = import.AllowITypeProcInd.Flag;
    useImport.ItypeWindow.Count = import.ItypeWindow.Count;
    local.Group.CopyTo(useImport.Group, MoveGroup4);
    local.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist3);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistribute.TotalCurrency;
    useImport.DistBy.CourtOrderNumber = local.DistBy.CourtOrderNumber;
    useImport.CollMonthStart.Date = local.CollMonthStart.Date;
    useImport.CollMonthEnd.Date = local.CollMonthEnd.Date;
    useImport.ProcessSecObligOnly.Flag = local.ProcessSecObligOnly.Flag;
    useImport.FutureApplAllowed.Flag = local.FutureApplAllowed.Flag;
    useImport.HardcodedFedFType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;
    useImport.HardcodedIType.SequentialIdentifier =
      local.HardcodedIType.SequentialIdentifier;
    useImport.HardcodedVType.SequentialIdentifier =
      local.HardcodedVType.SequentialIdentifier;
    useImport.HardcodedVoluntaryClass.Classification =
      local.HardcodedVolClass.Classification;
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.HardcodedPrimary.PrimarySecondaryCode =
      local.HardcodedPrimary.PrimarySecondaryCode;
    useImport.HardcodedFcrtPmt.SystemGeneratedIdentifier =
      local.HardcodedFcourtPmt.SystemGeneratedIdentifier;
    useImport.HardcodedFdirPmt.SystemGeneratedIdentifier =
      local.HardcodedFdirPmt.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718BType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(local.HardcodedAf);
    useImport.HardcodedAfi.Assign(local.HardcodedAfi);
    useImport.HardcodedFc.Assign(local.HardcodedFc);
    useImport.HardcodedFci.Assign(local.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(local.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(local.HardcodedNai);
    useImport.HardcodedNc.Assign(local.HardcodedNc);
    useImport.HardcodedNf.Assign(local.HardcodedNf);
    useImport.HardcodedMai.Assign(local.HardcodedMai);
    local.Legal.CopyTo(useImport.Legal, MoveLegal2);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);
    useImport.HardcodedNaDprProgram.ProgramState =
      local.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedPa.ProgramState = local.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = local.HardcodedTa.ProgramState;
    useImport.HardcodedCa.ProgramState = local.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = local.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = local.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;

    Call(FnDistCrdBalanceByDistPlcy.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.PersistantCashReceiptDetail,
      entities.ExistingCashReceiptDetail);
    entities.ExistingCollectionType.SequentialIdentifier =
      useImport.PersistantCollectionType.SequentialIdentifier;
    useImport.Group.CopyTo(local.Group, MoveGroup7);
    useImport.HhHist.CopyTo(local.HhHist, MoveHhHist6);
    local.AmtDistributed.TotalCurrency = useExport.AmtDistributed.TotalCurrency;
    local.AmtDistributedToPrim.TotalCurrency =
      useExport.AmtDistributedToPrim.TotalCurrency;
    useExport.PrimSummary.CopyTo(
      local.PrimarySummary, MovePrimSummaryToPrimarySummary);
  }

  private void UseFnDistCrdBalanceByDistPlcy2()
  {
    var useImport = new FnDistCrdBalanceByDistPlcy.Import();
    var useExport = new FnDistCrdBalanceByDistPlcy.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.ExistingCashReceiptType.SystemGeneratedIdentifier;
    useImport.PersistantCashReceiptDetail.Assign(
      entities.ExistingCashReceiptDetail);
    useImport.PersistantCollectionType.Assign(entities.ExistingCollectionType);
    useImport.PrworaDateOfConversion.Date = import.PrworaDateOfConversion.Date;
    useImport.AllowITypeProcInd.Flag = import.AllowITypeProcInd.Flag;
    useImport.ItypeWindow.Count = import.ItypeWindow.Count;
    local.Group.CopyTo(useImport.Group, MoveGroup4);
    local.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist3);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistributeToSec.TotalCurrency;
    useImport.DistBy.CourtOrderNumber = local.DistBy.CourtOrderNumber;
    useImport.CollMonthStart.Date = local.CollMonthStart.Date;
    useImport.CollMonthEnd.Date = local.CollMonthEnd.Date;
    useImport.ProcessSecObligOnly.Flag = local.ProcessSecObligOnly.Flag;
    useImport.FutureApplAllowed.Flag = local.FutureApplAllowed.Flag;
    useImport.HardcodedFedFType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;
    useImport.HardcodedIType.SequentialIdentifier =
      local.HardcodedIType.SequentialIdentifier;
    useImport.HardcodedVType.SequentialIdentifier =
      local.HardcodedVType.SequentialIdentifier;
    useImport.HardcodedVoluntaryClass.Classification =
      local.HardcodedVolClass.Classification;
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.HardcodedPrimary.PrimarySecondaryCode =
      local.HardcodedPrimary.PrimarySecondaryCode;
    useImport.HardcodedFcrtPmt.SystemGeneratedIdentifier =
      local.HardcodedFcourtPmt.SystemGeneratedIdentifier;
    useImport.HardcodedFdirPmt.SystemGeneratedIdentifier =
      local.HardcodedFdirPmt.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718BType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(local.HardcodedAf);
    useImport.HardcodedAfi.Assign(local.HardcodedAfi);
    useImport.HardcodedFc.Assign(local.HardcodedFc);
    useImport.HardcodedFci.Assign(local.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(local.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(local.HardcodedNai);
    useImport.HardcodedNc.Assign(local.HardcodedNc);
    useImport.HardcodedNf.Assign(local.HardcodedNf);
    useImport.HardcodedMai.Assign(local.HardcodedMai);
    local.Legal.CopyTo(useImport.Legal, MoveLegal2);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);
    useImport.HardcodedNaDprProgram.ProgramState =
      local.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedPa.ProgramState = local.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = local.HardcodedTa.ProgramState;
    useImport.HardcodedCa.ProgramState = local.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = local.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = local.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;
    useImport.PrimaryObligationType.SystemGeneratedIdentifier =
      local.PrimarySummary.Item.PrimSummObligationType.
        SystemGeneratedIdentifier;
    useImport.PrimaryObligation.SystemGeneratedIdentifier =
      local.PrimarySummary.Item.PrimSummObligation.SystemGeneratedIdentifier;

    Call(FnDistCrdBalanceByDistPlcy.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.PersistantCashReceiptDetail,
      entities.ExistingCashReceiptDetail);
    entities.ExistingCollectionType.SequentialIdentifier =
      useImport.PersistantCollectionType.SequentialIdentifier;
    useImport.Group.CopyTo(local.Group, MoveGroup7);
    useImport.HhHist.CopyTo(local.HhHist, MoveHhHist6);
    local.AmtDistributedToSec.TotalCurrency =
      useExport.AmtDistributed.TotalCurrency;
  }

  private void UseFnDistCrdBalanceToVolOblig()
  {
    var useImport = new FnDistCrdBalanceToVolOblig.Import();
    var useExport = new FnDistCrdBalanceToVolOblig.Export();

    useImport.PersistantCashReceiptDetail.Assign(
      entities.ExistingCashReceiptDetail);
    useImport.CollectionType.SequentialIdentifier =
      entities.ExistingCollectionType.SequentialIdentifier;
    useImport.Obligor.Number = entities.ExistingKeyOnlyObligor.Number;
    useImport.PersistantObligor.Assign(entities.ExistingObligor);
    useImport.PrworaDateOfConversion.Date = import.PrworaDateOfConversion.Date;
    local.Group.CopyTo(useImport.Group, MoveGroup3);
    local.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist2);
    useImport.AmtToDistribute.TotalCurrency =
      local.AmtToDistribute.TotalCurrency;
    useImport.HardcodedFFedType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;
    useImport.HardcodedVolClass.Classification =
      local.HardcodedVolClass.Classification;
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718BType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedAf.Assign(local.HardcodedAf);
    useImport.HardcodedAfi.Assign(local.HardcodedAfi);
    useImport.HardcodedFc.Assign(local.HardcodedFc);
    useImport.HardcodedFci.Assign(local.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(local.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(local.HardcodedNai);
    useImport.HardcodedNc.Assign(local.HardcodedNc);
    useImport.HardcodedNf.Assign(local.HardcodedNf);
    useImport.HardcodedMai.Assign(local.HardcodedMai);
    local.Legal.CopyTo(useImport.Legal, MoveLegal4);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist4);
    useImport.HardcodedNaDprProgram.ProgramState =
      local.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedPa.ProgramState = local.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = local.HardcodedTa.ProgramState;
    useImport.HardcodedCa.ProgramState = local.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = local.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = local.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;

    Call(FnDistCrdBalanceToVolOblig.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.PersistantCashReceiptDetail,
      entities.ExistingCashReceiptDetail);
    MoveCsePersonAccount(useImport.PersistantObligor, entities.ExistingObligor);
    useImport.Group.CopyTo(local.Group, MoveGroup6);
    useImport.HhHist.CopyTo(local.HhHist, MoveHhHist8);
    local.AmtDistributed.TotalCurrency = useExport.AmtDistributed.TotalCurrency;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCashType.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
    MoveCashReceiptType(useExport.CrtSystemId.CrtIdFcrtRec,
      local.HardcodedFcourtPmt);
    MoveCashReceiptType(useExport.CrtSystemId.CrtIdFdirPmt,
      local.HardcodedFdirPmt);
    local.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedDistributed.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    local.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedPriSec.SequentialGeneratedIdentifier =
      useExport.OrrPrimarySecondary.SequentialGeneratedIdentifier;
    local.HardcodedAccrual.SystemGeneratedIdentifier =
      useExport.OtrrAccrual.SystemGeneratedIdentifier;
    local.HardcodedArrears.AppliedToCode = useExport.CollArrears.AppliedToCode;
    local.HardcodedCurrent.AppliedToCode = useExport.CollCurrent.AppliedToCode;
    local.HardcodedDeactivatedStat.Code = useExport.DdshDeactivedStatus.Code;
    local.HardcodedActiveStatus.Code = useExport.DdshActiveStatus.Code;
    local.HardcodedJointSeveralObligation.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    local.HardcodedJointSeveralObligationRlnRsn.SequentialGeneratedIdentifier =
      useExport.OrrJointSeveral.SequentialGeneratedIdentifier;
    local.HardcodedWrongAcct.SystemGeneratedIdentifier =
      useExport.CarPostedToTheWrongAcct.SystemGeneratedIdentifier;
    local.HardcodedVolClass.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodedAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedSecondary.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
    local.HardcodedPrimary.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
    local.HardcodedCsType.SystemGeneratedIdentifier =
      useExport.OtChildSupport.SystemGeneratedIdentifier;
    local.HardcodedMsType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupport.SystemGeneratedIdentifier;
    local.HardcodedMjType.SystemGeneratedIdentifier =
      useExport.OtMedicalJudgement.SystemGeneratedIdentifier;
    local.Hardcoded718BType.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.HardcodedMcType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupportForCash.SystemGeneratedIdentifier;
  }

  private void UseFnProcessDistributionRequest()
  {
    var useImport = new FnProcessDistributionRequest.Import();
    var useExport = new FnProcessDistributionRequest.Export();

    MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
      useImport.CashReceiptSourceType);
    MoveCashReceiptType(entities.ExistingCashReceiptType,
      useImport.CashReceiptType);
    useImport.PersistantCashReceiptDetail.Assign(
      entities.ExistingCashReceiptDetail);
    useImport.CollectionType.SequentialIdentifier =
      entities.ExistingCollectionType.SequentialIdentifier;
    useImport.PersistantObligor.Assign(entities.ExistingObligor);
    local.Group.CopyTo(useImport.Group, MoveGroup2);
    useImport.AutoOrManual.DistributionMethod =
      local.AutoOrManual.DistributionMethod;
    useImport.Current.Assign(local.Current);
    useImport.UserId.Text8 = local.UserId.Text8;
    useImport.Adjusted.CollectionAmount = local.ForAdjustment.CollectionAmount;
    useImport.HardcodedArrears.AppliedToCode =
      local.HardcodedArrears.AppliedToCode;
    useImport.HardcodedCurrent.AppliedToCode =
      local.HardcodedCurrent.AppliedToCode;
    useImport.HardcodedDeactivedStat.Code = local.HardcodedDeactivatedStat.Code;
    useImport.HardcodedCashType.CategoryIndicator =
      local.HardcodedCashType.CategoryIndicator;
    useImport.HardcodedJointSeveralObligation.PrimarySecondaryCode =
      local.HardcodedJointSeveralObligation.PrimarySecondaryCode;
    useImport.HardcodedJointSeveralObligationRlnRsn.
      SequentialGeneratedIdentifier =
        local.HardcodedJointSeveralObligationRlnRsn.
        SequentialGeneratedIdentifier;
    useImport.HardcodedFType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;
    useImport.HardcodedSType.SequentialIdentifier =
      local.HardcodedSType.SequentialIdentifier;
    useImport.HardcodedVolClass.Classification =
      local.HardcodedVolClass.Classification;
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.HardcodedPrimary.PrimarySecondaryCode =
      local.HardcodedPrimary.PrimarySecondaryCode;
    useImport.HardcodedFcourtPmt.SystemGeneratedIdentifier =
      local.HardcodedFcourtPmt.SystemGeneratedIdentifier;
    useImport.HardcodedFdirPmt.SystemGeneratedIdentifier =
      local.HardcodedFdirPmt.SystemGeneratedIdentifier;
    useImport.HardcodedDistributed.SystemGeneratedIdentifier =
      local.HardcodedDistributed.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718BType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedAfType.SystemGeneratedIdentifier =
      local.HardcodedAf.SystemGeneratedIdentifier;
    useImport.HardcodedFcType.SystemGeneratedIdentifier =
      local.HardcodedFc.SystemGeneratedIdentifier;
    useImport.HardcodedNcType.SystemGeneratedIdentifier =
      local.HardcodedNc.SystemGeneratedIdentifier;
    useImport.HardcodedNfType.SystemGeneratedIdentifier =
      local.HardcodedNf.SystemGeneratedIdentifier;
    local.Legal.CopyTo(useImport.Legal, MoveLegal3);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist3);
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;

    Call(FnProcessDistributionRequest.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.PersistantCashReceiptDetail,
      entities.ExistingCashReceiptDetail);
    MoveCsePersonAccount(useImport.PersistantObligor, entities.ExistingObligor);
    useImport.HhHist.CopyTo(local.HhHist, MoveHhHist7);
  }

  private void UseFnSuspendCashRcptDtl()
  {
    var useImport = new FnSuspendCashRcptDtl.Import();
    var useExport = new FnSuspendCashRcptDtl.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.ExistingCashReceipt.SequentialNumber;
    useImport.Persistant.Assign(entities.ExistingCashReceiptDetail);
    useImport.ApplRunMode.Text8 = import.ApplRunMode.Text8;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.UserId.Text8 = local.UserId.Text8;
    useImport.MaximumDiscontinue.Date = local.MaximumDiscontinue.Date;
    useImport.HardcodedSuspended.SystemGeneratedIdentifier =
      local.HardcodedSuspended.SystemGeneratedIdentifier;
    MoveCashReceiptDetailStatHistory(export.SuspendedReason,
      useImport.CashReceiptDetailStatHistory);

    Call(FnSuspendCashRcptDtl.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useImport.Persistant,
      entities.ExistingCashReceiptDetail);
  }

  private void UseFnVerifyCourtOrderFilter()
  {
    var useImport = new FnVerifyCourtOrderFilter.Import();
    var useExport = new FnVerifyCourtOrderFilter.Export();

    useImport.CsePerson.Number = entities.ExistingKeyOnlyObligor.Number;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;

    Call(FnVerifyCourtOrderFilter.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingAdjusted.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingAdjusted.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingAdjusted.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingAdjusted.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingAdjusted.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.ExistingAdjusted.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingAdjusted.CollectionAmount = db.GetDecimal(reader, 5);
        entities.ExistingAdjusted.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptCashReceiptType()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;
    entities.ExistingCashReceiptType.Populated = false;
    entities.ExistingCashReceipt.Populated = false;
    entities.ExistingCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequentialNumber1",
          import.StartCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "sequentialNumber2", import.EndCashReceipt.SequentialNumber);
          
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 11);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 18);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.ExistingCashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 20);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 21);
        entities.ExistingCashReceiptType.CategoryIndicator =
          db.GetString(reader, 22);
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 23);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 24);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.ExistingCashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.ExistingCashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.ExistingCashReceiptDetail.CltIdentifier.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvId", entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstId", entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtType", entities.ExistingCashReceiptDetail.CrtIdentifier);
          
        db.SetString(
          command, "numb",
          entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingKeyOnlyObligor.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingKeyOnlyObligor.Number = db.GetString(reader, 0);
        entities.ExistingKeyOnlyObligor.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligor.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.ExistingObligor.Type1);
        db.SetString(command, "cspNumber", entities.ExistingObligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 6);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
      });
  }

  private bool ReadObligor()
  {
    entities.ExistingObligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingKeyOnlyObligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligor.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligor.Type1 = db.GetString(reader, 1);
        entities.ExistingObligor.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingObligor.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.ExistingObligor.LastManualDistributionDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingObligor.LastCollAmt = db.GetNullableDecimal(reader, 5);
        entities.ExistingObligor.LastCollDt = db.GetNullableDate(reader, 6);
        entities.ExistingObligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.ExistingObligor.Type1);
      });
  }

  private bool ReadOverpaymentHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligor.Populated);
    entities.ExistingOverpaymentHistory.Populated = false;

    return Read("ReadOverpaymentHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.ExistingObligor.Type1);
        db.SetString(command, "cspNumber", entities.ExistingObligor.CspNumber);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingOverpaymentHistory.CpaType = db.GetString(reader, 0);
        entities.ExistingOverpaymentHistory.CspNumber = db.GetString(reader, 1);
        entities.ExistingOverpaymentHistory.EffectiveDt = db.GetDate(reader, 2);
        entities.ExistingOverpaymentHistory.OverpaymentInd =
          db.GetString(reader, 3);
        entities.ExistingOverpaymentHistory.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.ExistingOverpaymentHistory.Populated = true;
        CheckValid<OverpaymentHistory>("CpaType",
          entities.ExistingOverpaymentHistory.CpaType);
        CheckValid<OverpaymentHistory>("OverpaymentInd",
          entities.ExistingOverpaymentHistory.OverpaymentInd);
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    return ReadEach("ReadProgram",
      null,
      (db, reader) =>
      {
        if (local.OfPgms.IsFull)
        {
          return false;
        }

        entities.ExistingProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingProgram.Code = db.GetString(reader, 1);
        entities.ExistingProgram.InterstateIndicator = db.GetString(reader, 2);
        entities.ExistingProgram.Populated = true;

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
    /// A value of StartCashReceipt.
    /// </summary>
    [JsonPropertyName("startCashReceipt")]
    public CashReceipt StartCashReceipt
    {
      get => startCashReceipt ??= new();
      set => startCashReceipt = value;
    }

    /// <summary>
    /// A value of StartCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("startCashReceiptDetail")]
    public CashReceiptDetail StartCashReceiptDetail
    {
      get => startCashReceiptDetail ??= new();
      set => startCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of EndCashReceipt.
    /// </summary>
    [JsonPropertyName("endCashReceipt")]
    public CashReceipt EndCashReceipt
    {
      get => endCashReceipt ??= new();
      set => endCashReceipt = value;
    }

    /// <summary>
    /// A value of EndCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("endCashReceiptDetail")]
    public CashReceiptDetail EndCashReceiptDetail
    {
      get => endCashReceiptDetail ??= new();
      set => endCashReceiptDetail = value;
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
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of ApplRunMode.
    /// </summary>
    [JsonPropertyName("applRunMode")]
    public TextWorkArea ApplRunMode
    {
      get => applRunMode ??= new();
      set => applRunMode = value;
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

    private CashReceipt startCashReceipt;
    private CashReceiptDetail startCashReceiptDetail;
    private CashReceipt endCashReceipt;
    private CashReceiptDetail endCashReceiptDetail;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea processDate;
    private TextWorkArea applRunMode;
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
      public const int Capacity = 100;

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private Collection collection;
      private CsePerson suppPrsn;
      private Program program;
      private DprProgram dprProgram;
    }

    /// <summary>
    /// A value of TotalCrdRead.
    /// </summary>
    [JsonPropertyName("totalCrdRead")]
    public Common TotalCrdRead
    {
      get => totalCrdRead ??= new();
      set => totalCrdRead = value;
    }

    /// <summary>
    /// A value of TotalCrdSuspended.
    /// </summary>
    [JsonPropertyName("totalCrdSuspended")]
    public Common TotalCrdSuspended
    {
      get => totalCrdSuspended ??= new();
      set => totalCrdSuspended = value;
    }

    /// <summary>
    /// A value of TotalCrdProcessed.
    /// </summary>
    [JsonPropertyName("totalCrdProcessed")]
    public Common TotalCrdProcessed
    {
      get => totalCrdProcessed ??= new();
      set => totalCrdProcessed = value;
    }

    /// <summary>
    /// A value of TotalAmtAttempted.
    /// </summary>
    [JsonPropertyName("totalAmtAttempted")]
    public Common TotalAmtAttempted
    {
      get => totalAmtAttempted ??= new();
      set => totalAmtAttempted = value;
    }

    /// <summary>
    /// A value of TotalAmtSuspended.
    /// </summary>
    [JsonPropertyName("totalAmtSuspended")]
    public Common TotalAmtSuspended
    {
      get => totalAmtSuspended ??= new();
      set => totalAmtSuspended = value;
    }

    /// <summary>
    /// A value of TotalAmtProcessed.
    /// </summary>
    [JsonPropertyName("totalAmtProcessed")]
    public Common TotalAmtProcessed
    {
      get => totalAmtProcessed ??= new();
      set => totalAmtProcessed = value;
    }

    /// <summary>
    /// A value of TotalEventsRaised.
    /// </summary>
    [JsonPropertyName("totalEventsRaised")]
    public Common TotalEventsRaised
    {
      get => totalEventsRaised ??= new();
      set => totalEventsRaised = value;
    }

    /// <summary>
    /// A value of TotalCommitsTaken.
    /// </summary>
    [JsonPropertyName("totalCommitsTaken")]
    public Common TotalCommitsTaken
    {
      get => totalCommitsTaken ??= new();
      set => totalCommitsTaken = value;
    }

    /// <summary>
    /// A value of SuspendedReason.
    /// </summary>
    [JsonPropertyName("suspendedReason")]
    public CashReceiptDetailStatHistory SuspendedReason
    {
      get => suspendedReason ??= new();
      set => suspendedReason = value;
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
    /// A value of LastProcessedCashReceipt.
    /// </summary>
    [JsonPropertyName("lastProcessedCashReceipt")]
    public CashReceipt LastProcessedCashReceipt
    {
      get => lastProcessedCashReceipt ??= new();
      set => lastProcessedCashReceipt = value;
    }

    /// <summary>
    /// A value of LastProcessedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("lastProcessedCashReceiptDetail")]
    public CashReceiptDetail LastProcessedCashReceiptDetail
    {
      get => lastProcessedCashReceiptDetail ??= new();
      set => lastProcessedCashReceiptDetail = value;
    }

    private Common totalCrdRead;
    private Common totalCrdSuspended;
    private Common totalCrdProcessed;
    private Common totalAmtAttempted;
    private Common totalAmtSuspended;
    private Common totalAmtProcessed;
    private Common totalEventsRaised;
    private Common totalCommitsTaken;
    private CashReceiptDetailStatHistory suspendedReason;
    private Array<GroupGroup> group;
    private CashReceipt lastProcessedCashReceipt;
    private CashReceiptDetail lastProcessedCashReceiptDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A RedistGroup group.</summary>
    [Serializable]
    public class RedistGroup
    {
      /// <summary>
      /// A value of Redist1.
      /// </summary>
      [JsonPropertyName("redist1")]
      public Collection Redist1
      {
        get => redist1 ??= new();
        set => redist1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private Collection redist1;
    }

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

    /// <summary>A PrimarySummaryGroup group.</summary>
    [Serializable]
    public class PrimarySummaryGroup
    {
      /// <summary>
      /// A value of PrimSummObligationType.
      /// </summary>
      [JsonPropertyName("primSummObligationType")]
      public ObligationType PrimSummObligationType
      {
        get => primSummObligationType ??= new();
        set => primSummObligationType = value;
      }

      /// <summary>
      /// A value of PrimSummObligation.
      /// </summary>
      [JsonPropertyName("primSummObligation")]
      public Obligation PrimSummObligation
      {
        get => primSummObligation ??= new();
        set => primSummObligation = value;
      }

      /// <summary>
      /// A value of PrimSummCommon.
      /// </summary>
      [JsonPropertyName("primSummCommon")]
      public Common PrimSummCommon
      {
        get => primSummCommon ??= new();
        set => primSummCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ObligationType primSummObligationType;
      private Obligation primSummObligation;
      private Common primSummCommon;
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

    /// <summary>A HhHistSaveGroup group.</summary>
    [Serializable]
    public class HhHistSaveGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsnSave.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsnSave")]
      public CsePerson HhHistSuppPrsnSave
      {
        get => hhHistSuppPrsnSave ??= new();
        set => hhHistSuppPrsnSave = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtlSave.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlSaveGroup> HhHistDtlSave => hhHistDtlSave ??= new(
        HhHistDtlSaveGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtlSave for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtlSave")]
      [Computed]
      public IList<HhHistDtlSaveGroup> HhHistDtlSave_Json
      {
        get => hhHistDtlSave;
        set => HhHistDtlSave.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsnSave;
      private Array<HhHistDtlSaveGroup> hhHistDtlSave;
    }

    /// <summary>A HhHistDtlSaveGroup group.</summary>
    [Serializable]
    public class HhHistDtlSaveGroup
    {
      /// <summary>
      /// A value of HhHistDtlSaveImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlSaveImHousehold")]
      public ImHousehold HhHistDtlSaveImHousehold
      {
        get => hhHistDtlSaveImHousehold ??= new();
        set => hhHistDtlSaveImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlSaveImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlSaveImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlSaveImHouseholdMbrMnthlySum
      {
        get => hhHistDtlSaveImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlSaveImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlSaveImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlSaveImHouseholdMbrMnthlySum;
    }

    /// <summary>A HhHistNullGroup group.</summary>
    [Serializable]
    public class HhHistNullGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsnNull.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsnNull")]
      public CsePerson HhHistSuppPrsnNull
      {
        get => hhHistSuppPrsnNull ??= new();
        set => hhHistSuppPrsnNull = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtlNull.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlNullGroup> HhHistDtlNull => hhHistDtlNull ??= new(
        HhHistDtlNullGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtlNull for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtlNull")]
      [Computed]
      public IList<HhHistDtlNullGroup> HhHistDtlNull_Json
      {
        get => hhHistDtlNull;
        set => HhHistDtlNull.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private CsePerson hhHistSuppPrsnNull;
      private Array<HhHistDtlNullGroup> hhHistDtlNull;
    }

    /// <summary>A HhHistDtlNullGroup group.</summary>
    [Serializable]
    public class HhHistDtlNullGroup
    {
      /// <summary>
      /// A value of HhHistDtlNullImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlNullImHousehold")]
      public ImHousehold HhHistDtlNullImHousehold
      {
        get => hhHistDtlNullImHousehold ??= new();
        set => hhHistDtlNullImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlNullImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlNullImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlNullImHouseholdMbrMnthlySum
      {
        get => hhHistDtlNullImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlNullImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private ImHousehold hhHistDtlNullImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlNullImHouseholdMbrMnthlySum;
    }

    /// <summary>A OfPgmsGroup group.</summary>
    [Serializable]
    public class OfPgmsGroup
    {
      /// <summary>
      /// A value of OfPgms1.
      /// </summary>
      [JsonPropertyName("ofPgms1")]
      public Program OfPgms1
      {
        get => ofPgms1 ??= new();
        set => ofPgms1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Program ofPgms1;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public OverpaymentHistory Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of CurrentAmountDue.
    /// </summary>
    [JsonPropertyName("currentAmountDue")]
    public Common CurrentAmountDue
    {
      get => currentAmountDue ??= new();
      set => currentAmountDue = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// Gets a value of Redist.
    /// </summary>
    [JsonIgnore]
    public Array<RedistGroup> Redist => redist ??= new(RedistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Redist for json serialization.
    /// </summary>
    [JsonPropertyName("redist")]
    [Computed]
    public IList<RedistGroup> Redist_Json
    {
      get => redist;
      set => Redist.Assign(value);
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of AccrueFutureInd.
    /// </summary>
    [JsonPropertyName("accrueFutureInd")]
    public Common AccrueFutureInd
    {
      get => accrueFutureInd ??= new();
      set => accrueFutureInd = value;
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
    /// A value of AmtToDistributeToSec.
    /// </summary>
    [JsonPropertyName("amtToDistributeToSec")]
    public Common AmtToDistributeToSec
    {
      get => amtToDistributeToSec ??= new();
      set => amtToDistributeToSec = value;
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
    /// A value of AmtDistributedToSec.
    /// </summary>
    [JsonPropertyName("amtDistributedToSec")]
    public Common AmtDistributedToSec
    {
      get => amtDistributedToSec ??= new();
      set => amtDistributedToSec = value;
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
    /// A value of SecObligProcRequired.
    /// </summary>
    [JsonPropertyName("secObligProcRequired")]
    public Common SecObligProcRequired
    {
      get => secObligProcRequired ??= new();
      set => secObligProcRequired = value;
    }

    /// <summary>
    /// A value of CommitReturnCode.
    /// </summary>
    [JsonPropertyName("commitReturnCode")]
    public External CommitReturnCode
    {
      get => commitReturnCode ??= new();
      set => commitReturnCode = value;
    }

    /// <summary>
    /// A value of ProcCntForCommit.
    /// </summary>
    [JsonPropertyName("procCntForCommit")]
    public Common ProcCntForCommit
    {
      get => procCntForCommit ??= new();
      set => procCntForCommit = value;
    }

    /// <summary>
    /// A value of CurrentMonthStart.
    /// </summary>
    [JsonPropertyName("currentMonthStart")]
    public DateWorkArea CurrentMonthStart
    {
      get => currentMonthStart ??= new();
      set => currentMonthStart = value;
    }

    /// <summary>
    /// A value of CurrentMonthEnd.
    /// </summary>
    [JsonPropertyName("currentMonthEnd")]
    public DateWorkArea CurrentMonthEnd
    {
      get => currentMonthEnd ??= new();
      set => currentMonthEnd = value;
    }

    /// <summary>
    /// A value of AutoOrManual.
    /// </summary>
    [JsonPropertyName("autoOrManual")]
    public Collection AutoOrManual
    {
      get => autoOrManual ??= new();
      set => autoOrManual = value;
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
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
    }

    /// <summary>
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
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
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
    }

    /// <summary>
    /// A value of ForAdjustment.
    /// </summary>
    [JsonPropertyName("forAdjustment")]
    public CashReceiptDetail ForAdjustment
    {
      get => forAdjustment ??= new();
      set => forAdjustment = value;
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
    /// A value of SuspendForMnlDistInst.
    /// </summary>
    [JsonPropertyName("suspendForMnlDistInst")]
    public Common SuspendForMnlDistInst
    {
      get => suspendForMnlDistInst ??= new();
      set => suspendForMnlDistInst = value;
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
    /// A value of CollectionObligor.
    /// </summary>
    [JsonPropertyName("collectionObligor")]
    public CsePerson CollectionObligor
    {
      get => collectionObligor ??= new();
      set => collectionObligor = value;
    }

    /// <summary>
    /// A value of PifInd.
    /// </summary>
    [JsonPropertyName("pifInd")]
    public Common PifInd
    {
      get => pifInd ??= new();
      set => pifInd = value;
    }

    /// <summary>
    /// A value of EventType.
    /// </summary>
    [JsonPropertyName("eventType")]
    public TextWorkArea EventType
    {
      get => eventType ??= new();
      set => eventType = value;
    }

    /// <summary>
    /// A value of HardcodedPriSec.
    /// </summary>
    [JsonPropertyName("hardcodedPriSec")]
    public ObligationRlnRsn HardcodedPriSec
    {
      get => hardcodedPriSec ??= new();
      set => hardcodedPriSec = value;
    }

    /// <summary>
    /// A value of HardcodedAccrual.
    /// </summary>
    [JsonPropertyName("hardcodedAccrual")]
    public ObligationTransactionRlnRsn HardcodedAccrual
    {
      get => hardcodedAccrual ??= new();
      set => hardcodedAccrual = value;
    }

    /// <summary>
    /// A value of HardcodedArrears.
    /// </summary>
    [JsonPropertyName("hardcodedArrears")]
    public Collection HardcodedArrears
    {
      get => hardcodedArrears ??= new();
      set => hardcodedArrears = value;
    }

    /// <summary>
    /// A value of HardcodedCurrent.
    /// </summary>
    [JsonPropertyName("hardcodedCurrent")]
    public Collection HardcodedCurrent
    {
      get => hardcodedCurrent ??= new();
      set => hardcodedCurrent = value;
    }

    /// <summary>
    /// A value of HardcodedDeactivatedStat.
    /// </summary>
    [JsonPropertyName("hardcodedDeactivatedStat")]
    public DebtDetailStatusHistory HardcodedDeactivatedStat
    {
      get => hardcodedDeactivatedStat ??= new();
      set => hardcodedDeactivatedStat = value;
    }

    /// <summary>
    /// A value of HardcodedCashType.
    /// </summary>
    [JsonPropertyName("hardcodedCashType")]
    public CashReceiptType HardcodedCashType
    {
      get => hardcodedCashType ??= new();
      set => hardcodedCashType = value;
    }

    /// <summary>
    /// A value of HardcodedActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodedActiveStatus")]
    public DebtDetailStatusHistory HardcodedActiveStatus
    {
      get => hardcodedActiveStatus ??= new();
      set => hardcodedActiveStatus = value;
    }

    /// <summary>
    /// A value of HardcodedJointSeveralObligation.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveralObligation")]
    public Obligation HardcodedJointSeveralObligation
    {
      get => hardcodedJointSeveralObligation ??= new();
      set => hardcodedJointSeveralObligation = value;
    }

    /// <summary>
    /// A value of HardcodedJointSeveralObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveralObligationRlnRsn")]
    public ObligationRlnRsn HardcodedJointSeveralObligationRlnRsn
    {
      get => hardcodedJointSeveralObligationRlnRsn ??= new();
      set => hardcodedJointSeveralObligationRlnRsn = value;
    }

    /// <summary>
    /// A value of HardcodedWrongAcct.
    /// </summary>
    [JsonPropertyName("hardcodedWrongAcct")]
    public CollectionAdjustmentReason HardcodedWrongAcct
    {
      get => hardcodedWrongAcct ??= new();
      set => hardcodedWrongAcct = value;
    }

    /// <summary>
    /// A value of HardcodedAType.
    /// </summary>
    [JsonPropertyName("hardcodedAType")]
    public CollectionType HardcodedAType
    {
      get => hardcodedAType ??= new();
      set => hardcodedAType = value;
    }

    /// <summary>
    /// A value of HardcodedCType.
    /// </summary>
    [JsonPropertyName("hardcodedCType")]
    public CollectionType HardcodedCType
    {
      get => hardcodedCType ??= new();
      set => hardcodedCType = value;
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
    /// A value of HardcodedIType.
    /// </summary>
    [JsonPropertyName("hardcodedIType")]
    public CollectionType HardcodedIType
    {
      get => hardcodedIType ??= new();
      set => hardcodedIType = value;
    }

    /// <summary>
    /// A value of HardcodedKType.
    /// </summary>
    [JsonPropertyName("hardcodedKType")]
    public CollectionType HardcodedKType
    {
      get => hardcodedKType ??= new();
      set => hardcodedKType = value;
    }

    /// <summary>
    /// A value of HardcodedRType.
    /// </summary>
    [JsonPropertyName("hardcodedRType")]
    public CollectionType HardcodedRType
    {
      get => hardcodedRType ??= new();
      set => hardcodedRType = value;
    }

    /// <summary>
    /// A value of HardcodedSType.
    /// </summary>
    [JsonPropertyName("hardcodedSType")]
    public CollectionType HardcodedSType
    {
      get => hardcodedSType ??= new();
      set => hardcodedSType = value;
    }

    /// <summary>
    /// A value of HardcodedTType.
    /// </summary>
    [JsonPropertyName("hardcodedTType")]
    public CollectionType HardcodedTType
    {
      get => hardcodedTType ??= new();
      set => hardcodedTType = value;
    }

    /// <summary>
    /// A value of HardcodedUType.
    /// </summary>
    [JsonPropertyName("hardcodedUType")]
    public CollectionType HardcodedUType
    {
      get => hardcodedUType ??= new();
      set => hardcodedUType = value;
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
    /// A value of HardcodedYType.
    /// </summary>
    [JsonPropertyName("hardcodedYType")]
    public CollectionType HardcodedYType
    {
      get => hardcodedYType ??= new();
      set => hardcodedYType = value;
    }

    /// <summary>
    /// A value of HardcodedZType.
    /// </summary>
    [JsonPropertyName("hardcodedZType")]
    public CollectionType HardcodedZType
    {
      get => hardcodedZType ??= new();
      set => hardcodedZType = value;
    }

    /// <summary>
    /// A value of Hardcoded4Type.
    /// </summary>
    [JsonPropertyName("hardcoded4Type")]
    public CollectionType Hardcoded4Type
    {
      get => hardcoded4Type ??= new();
      set => hardcoded4Type = value;
    }

    /// <summary>
    /// A value of Hardcoded5Type.
    /// </summary>
    [JsonPropertyName("hardcoded5Type")]
    public CollectionType Hardcoded5Type
    {
      get => hardcoded5Type ??= new();
      set => hardcoded5Type = value;
    }

    /// <summary>
    /// A value of Hardcoded6Type.
    /// </summary>
    [JsonPropertyName("hardcoded6Type")]
    public CollectionType Hardcoded6Type
    {
      get => hardcoded6Type ??= new();
      set => hardcoded6Type = value;
    }

    /// <summary>
    /// A value of Hardcoded9Type.
    /// </summary>
    [JsonPropertyName("hardcoded9Type")]
    public CollectionType Hardcoded9Type
    {
      get => hardcoded9Type ??= new();
      set => hardcoded9Type = value;
    }

    /// <summary>
    /// A value of HardcodedVolClass.
    /// </summary>
    [JsonPropertyName("hardcodedVolClass")]
    public ObligationType HardcodedVolClass
    {
      get => hardcodedVolClass ??= new();
      set => hardcodedVolClass = value;
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
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
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
    /// A value of HardcodedOverpymtNtcSnt.
    /// </summary>
    [JsonPropertyName("hardcodedOverpymtNtcSnt")]
    public OverpaymentHistory HardcodedOverpymtNtcSnt
    {
      get => hardcodedOverpymtNtcSnt ??= new();
      set => hardcodedOverpymtNtcSnt = value;
    }

    /// <summary>
    /// A value of HardcodedOverpymtFuture.
    /// </summary>
    [JsonPropertyName("hardcodedOverpymtFuture")]
    public OverpaymentHistory HardcodedOverpymtFuture
    {
      get => hardcodedOverpymtFuture ??= new();
      set => hardcodedOverpymtFuture = value;
    }

    /// <summary>
    /// A value of HardcodedFcourtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFcourtPmt")]
    public CashReceiptType HardcodedFcourtPmt
    {
      get => hardcodedFcourtPmt ??= new();
      set => hardcodedFcourtPmt = value;
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
    /// A value of HardcodedReleased.
    /// </summary>
    [JsonPropertyName("hardcodedReleased")]
    public CashReceiptDetailStatus HardcodedReleased
    {
      get => hardcodedReleased ??= new();
      set => hardcodedReleased = value;
    }

    /// <summary>
    /// A value of HardcodedRefunded.
    /// </summary>
    [JsonPropertyName("hardcodedRefunded")]
    public CashReceiptDetailStatus HardcodedRefunded
    {
      get => hardcodedRefunded ??= new();
      set => hardcodedRefunded = value;
    }

    /// <summary>
    /// A value of HardcodedDistributed.
    /// </summary>
    [JsonPropertyName("hardcodedDistributed")]
    public CashReceiptDetailStatus HardcodedDistributed
    {
      get => hardcodedDistributed ??= new();
      set => hardcodedDistributed = value;
    }

    /// <summary>
    /// A value of HardcodedSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedSuspended")]
    public CashReceiptDetailStatus HardcodedSuspended
    {
      get => hardcodedSuspended ??= new();
      set => hardcodedSuspended = value;
    }

    /// <summary>
    /// A value of HardcodedRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedRecorded")]
    public CashReceiptDetailStatus HardcodedRecorded
    {
      get => hardcodedRecorded ??= new();
      set => hardcodedRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedCsType.
    /// </summary>
    [JsonPropertyName("hardcodedCsType")]
    public ObligationType HardcodedCsType
    {
      get => hardcodedCsType ??= new();
      set => hardcodedCsType = value;
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
    /// A value of Hardcoded718BType.
    /// </summary>
    [JsonPropertyName("hardcoded718BType")]
    public ObligationType Hardcoded718BType
    {
      get => hardcoded718BType ??= new();
      set => hardcoded718BType = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of RedistByCourtOrderInd.
    /// </summary>
    [JsonPropertyName("redistByCourtOrderInd")]
    public Common RedistByCourtOrderInd
    {
      get => redistByCourtOrderInd ??= new();
      set => redistByCourtOrderInd = value;
    }

    /// <summary>
    /// Gets a value of PrimarySummary.
    /// </summary>
    [JsonIgnore]
    public Array<PrimarySummaryGroup> PrimarySummary => primarySummary ??= new(
      PrimarySummaryGroup.Capacity);

    /// <summary>
    /// Gets a value of PrimarySummary for json serialization.
    /// </summary>
    [JsonPropertyName("primarySummary")]
    [Computed]
    public IList<PrimarySummaryGroup> PrimarySummary_Json
    {
      get => primarySummary;
      set => PrimarySummary.Assign(value);
    }

    /// <summary>
    /// A value of DistributedAmount.
    /// </summary>
    [JsonPropertyName("distributedAmount")]
    public Common DistributedAmount
    {
      get => distributedAmount ??= new();
      set => distributedAmount = value;
    }

    /// <summary>
    /// A value of ProcessHouseholdsInd.
    /// </summary>
    [JsonPropertyName("processHouseholdsInd")]
    public Common ProcessHouseholdsInd
    {
      get => processHouseholdsInd ??= new();
      set => processHouseholdsInd = value;
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
    /// Gets a value of HhHistSave.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistSaveGroup> HhHistSave => hhHistSave ??= new(
      HhHistSaveGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHistSave for json serialization.
    /// </summary>
    [JsonPropertyName("hhHistSave")]
    [Computed]
    public IList<HhHistSaveGroup> HhHistSave_Json
    {
      get => hhHistSave;
      set => HhHistSave.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHistNull.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistNullGroup> HhHistNull => hhHistNull ??= new(
      HhHistNullGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHistNull for json serialization.
    /// </summary>
    [JsonPropertyName("hhHistNull")]
    [Computed]
    public IList<HhHistNullGroup> HhHistNull_Json
    {
      get => hhHistNull;
      set => HhHistNull.Assign(value);
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
    /// A value of PreconversionPgm.
    /// </summary>
    [JsonPropertyName("preconversionPgm")]
    public Common PreconversionPgm
    {
      get => preconversionPgm ??= new();
      set => preconversionPgm = value;
    }

    /// <summary>
    /// Gets a value of OfPgms.
    /// </summary>
    [JsonIgnore]
    public Array<OfPgmsGroup> OfPgms => ofPgms ??= new(OfPgmsGroup.Capacity);

    /// <summary>
    /// Gets a value of OfPgms for json serialization.
    /// </summary>
    [JsonPropertyName("ofPgms")]
    [Computed]
    public IList<OfPgmsGroup> OfPgms_Json
    {
      get => ofPgms;
      set => OfPgms.Assign(value);
    }

    private ExitStateWorkArea exitStateWorkArea;
    private OverpaymentHistory null1;
    private Common currentAmountDue;
    private Document document;
    private SpDocKey spDocKey;
    private Array<RedistGroup> redist;
    private Array<GroupGroup> group;
    private Array<PgmHistGroup> pgmHist;
    private CsePerson obligor;
    private Common accrueFutureInd;
    private Common amtToDistribute;
    private Common amtToDistributeToSec;
    private Common amtDistributed;
    private Common amtDistributedToPrim;
    private Common amtDistributedToSec;
    private CashReceiptDetail distBy;
    private Common secObligProcRequired;
    private External commitReturnCode;
    private Common procCntForCommit;
    private DateWorkArea currentMonthStart;
    private DateWorkArea currentMonthEnd;
    private Collection autoOrManual;
    private DateWorkArea current;
    private TextWorkArea userId;
    private DateWorkArea maximumDiscontinue;
    private LegalAction legalAction;
    private OverpaymentHistory overpaymentHistory;
    private CashReceiptDetail forAdjustment;
    private DateWorkArea collection;
    private DateWorkArea collMonthStart;
    private DateWorkArea collMonthEnd;
    private Common suspendForMnlDistInst;
    private Common processSecObligOnly;
    private Common futureApplAllowed;
    private CsePerson collectionObligor;
    private Common pifInd;
    private TextWorkArea eventType;
    private ObligationRlnRsn hardcodedPriSec;
    private ObligationTransactionRlnRsn hardcodedAccrual;
    private Collection hardcodedArrears;
    private Collection hardcodedCurrent;
    private DebtDetailStatusHistory hardcodedDeactivatedStat;
    private CashReceiptType hardcodedCashType;
    private DebtDetailStatusHistory hardcodedActiveStatus;
    private Obligation hardcodedJointSeveralObligation;
    private ObligationRlnRsn hardcodedJointSeveralObligationRlnRsn;
    private CollectionAdjustmentReason hardcodedWrongAcct;
    private CollectionType hardcodedAType;
    private CollectionType hardcodedCType;
    private CollectionType hardcodedFType;
    private CollectionType hardcodedIType;
    private CollectionType hardcodedKType;
    private CollectionType hardcodedRType;
    private CollectionType hardcodedSType;
    private CollectionType hardcodedTType;
    private CollectionType hardcodedUType;
    private CollectionType hardcodedVType;
    private CollectionType hardcodedYType;
    private CollectionType hardcodedZType;
    private CollectionType hardcoded4Type;
    private CollectionType hardcoded5Type;
    private CollectionType hardcoded6Type;
    private CollectionType hardcoded9Type;
    private ObligationType hardcodedVolClass;
    private ObligationType hardcodedAccruingClass;
    private Obligation hardcodedSecondary;
    private Obligation hardcodedPrimary;
    private OverpaymentHistory hardcodedOverpymtNtcSnt;
    private OverpaymentHistory hardcodedOverpymtFuture;
    private CashReceiptType hardcodedFcourtPmt;
    private CashReceiptType hardcodedFdirPmt;
    private CashReceiptDetailStatus hardcodedReleased;
    private CashReceiptDetailStatus hardcodedRefunded;
    private CashReceiptDetailStatus hardcodedDistributed;
    private CashReceiptDetailStatus hardcodedSuspended;
    private CashReceiptDetailStatus hardcodedRecorded;
    private ObligationType hardcodedCsType;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcoded718BType;
    private ObligationType hardcodedMcType;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNaProgram;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private Common redistByCourtOrderInd;
    private Array<PrimarySummaryGroup> primarySummary;
    private Common distributedAmount;
    private Common processHouseholdsInd;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private Array<HhHistSaveGroup> hhHistSave;
    private Array<HhHistNullGroup> hhHistNull;
    private DprProgram hardcodedNaDprProgram;
    private DprProgram hardcodedPa;
    private DprProgram hardcodedTa;
    private DprProgram hardcodedCa;
    private DprProgram hardcodedUd;
    private DprProgram hardcodedUp;
    private DprProgram hardcodedUk;
    private Common preconversionPgm;
    private Array<OfPgmsGroup> ofPgms;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonAccount.
    /// </summary>
    [JsonPropertyName("existingCsePersonAccount")]
    public CsePersonAccount ExistingCsePersonAccount
    {
      get => existingCsePersonAccount ??= new();
      set => existingCsePersonAccount = value;
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
    /// A value of ExistingObligationTransaction.
    /// </summary>
    [JsonPropertyName("existingObligationTransaction")]
    public ObligationTransaction ExistingObligationTransaction
    {
      get => existingObligationTransaction ??= new();
      set => existingObligationTransaction = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
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

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ExistingCashReceiptDetailStatus
    {
      get => existingCashReceiptDetailStatus ??= new();
      set => existingCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ExistingCollectionType.
    /// </summary>
    [JsonPropertyName("existingCollectionType")]
    public CollectionType ExistingCollectionType
    {
      get => existingCollectionType ??= new();
      set => existingCollectionType = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePerson ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingOverpaymentHistory.
    /// </summary>
    [JsonPropertyName("existingOverpaymentHistory")]
    public OverpaymentHistory ExistingOverpaymentHistory
    {
      get => existingOverpaymentHistory ??= new();
      set => existingOverpaymentHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj ExistingCashReceiptDetailBalanceAdj
    {
      get => existingCashReceiptDetailBalanceAdj ??= new();
      set => existingCashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of ExistingAdjusted.
    /// </summary>
    [JsonPropertyName("existingAdjusted")]
    public CashReceiptDetail ExistingAdjusted
    {
      get => existingAdjusted ??= new();
      set => existingAdjusted = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligation.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligation")]
    public Obligation ExistingKeyOnlyObligation
    {
      get => existingKeyOnlyObligation ??= new();
      set => existingKeyOnlyObligation = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyDebt.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyDebt")]
    public ObligationTransaction ExistingKeyOnlyDebt
    {
      get => existingKeyOnlyDebt ??= new();
      set => existingKeyOnlyDebt = value;
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
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    private CsePerson existingCsePerson;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private ObligationTransaction existingObligationTransaction;
    private Collection existingCollection;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptType existingCashReceiptType;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private CollectionType existingCollectionType;
    private CsePerson existingKeyOnlyObligor;
    private CsePersonAccount existingObligor;
    private OverpaymentHistory existingOverpaymentHistory;
    private CashReceiptDetailBalanceAdj existingCashReceiptDetailBalanceAdj;
    private CashReceiptDetail existingAdjusted;
    private Obligation existingKeyOnlyObligation;
    private ObligationTransaction existingKeyOnlyDebt;
    private DebtDetail existingDebtDetail;
    private Program existingProgram;
  }
#endregion
}
