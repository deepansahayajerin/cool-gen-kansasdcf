// Program: FN_B650_CREATE_DISB_CREDITS, ID: 372543875, model: 746.
// Short name: SWEF650B
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
/// A program: FN_B650_CREATE_DISB_CREDITS.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB650CreateDisbCredits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B650_CREATE_DISB_CREDITS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB650CreateDisbCredits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB650CreateDisbCredits.
  /// </summary>
  public FnB650CreateDisbCredits(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // 121298   RK -
    // 1. Error situation should commit the good records and error out the 
    // current record to an error file instead of the error table which is being
    // replaced.
    // 2. Took out 9 unneeded Reads.
    // 3.  Put in Automatic Suppression code based on AF involvement in last 90 
    // days.
    // 4. See Phase II assessement form for complete list of changes.
    // 9/11/99  Fangman        Removed commits & control report lines that occur
    // with every non-fatal error.  Changed error msg format to show more of
    // the error msg.  Added additional information to the control report.
    // Added test indicator to rollback all DB2 updates.  Removed dead code.
    // Changed logic so that when an error occurred during the update of a
    // adjusted "same day" collection the error exit state would not be
    // overriden with a all_ok exit state causing the program to try to process
    // the collection.  Removed restart logic which did not work.
    // 9/15/99   Fangman  Changed code to display more information on 
    // collections that errored out (PR 74155)
    // 10/22/99 - SWSRKXD PR#77874
    // Error off 'NC' collections. Money will never be disbursed to NC program 
    // codes.
    // 10/29/99  Fangman   Removed old code that was used to  correct a problem 
    // (that no longer exists) with not setting the disburesment adj process
    // date.
    // 11/12/99  Fangman  PR 78745  Added code to only process the collection if
    // the cash receipt detail has been fully paid.
    // 12/27/99  Fangman  PR 83163  Added read of Cash Receipt Type to pass the
    // "Cash" category indicator to FN_DET_OBLIGEE_FOR_OBLIG_COLL
    // 12/30/99  Fangman  PR 83372  Added logic to not error off non-cash 
    // collections in the "CRD not fully applied" logic.
    // 05/04/00  Fangman  PRWORA  Added pgm applied to state attrib to 
    // collection entity.
    // -------------------------------------------
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // 09/12/00  Fangman  013323
    //     Added parameter to process by AP ranges.
    //     Added counts of error break downs
    //     Added cash non-cash indicator to the error details
    //     These changes were put in with the changes to fix the disb suppr with
    // past discontinue dates.
    // -------------------------------------------
    // -----------------------------------------------------------------
    // 06/15/01  Fangman  SR 010507
    //      Added the AR Number to the entity view of the Collection table.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 11/26/02 - Fangman  161935
    // Add new counts to break up the Cash counts into TAF and Non-TAF.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 01/26/04 - Fangman  197465
    // Add code for PR to change how we determine the AR - looking at all open 
    // cases before looking at any closed cases regardles of how the case role
    // dates match to DDDD.
    // -----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB650Initialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.HardcodePass.HardcodeSpousalSupport.SystemGeneratedIdentifier = 2;
    local.HardcodePass.HardcodeSpArrearsJudgemt.SystemGeneratedIdentifier = 17;
    local.HardcodePass.HardcodeVoluntary.SystemGeneratedIdentifier = 16;

    // ***** Process the "unprocessed" collection records in groups based upon 
    // the commit frequencies.  Do a DB2 commit at the end of each group.
    foreach(var item in ReadCollectionDebtObligationObligationTypeObligor())
    {
      if (!IsEmpty(local.TestFirstObligor.Number) || !
        IsEmpty(local.TestLastObligor.Number))
      {
        if (Lt(entities.Obligor2.Number, local.TestFirstObligor.Number))
        {
          continue;
        }

        if (Lt(local.TestLastObligor.Number, entities.Obligor2.Number))
        {
          break;
        }
      }

      if (local.Test.SystemGeneratedIdentifier > 0)
      {
        if (entities.Collection.SystemGeneratedIdentifier != local
          .Test.SystemGeneratedIdentifier)
        {
          continue;
        }
      }

      if (!Equal(entities.Obligor2.Number, local.HoldObligor.Number))
      {
        local.HoldObligor.Number = entities.Obligor2.Number;
        ++local.TempTotals.TempNbrOfAps.Count;

        if (AsChar(local.DecreaseApApplyRcvCnt.Flag) == 'Y')
        {
          --local.ApsWithColToApplyToRcv.LastUsedNumber;
          local.DecreaseApApplyRcvCnt.Flag = "N";
        }

        if (local.ApsWithColToApplyToRcv.LastUsedNumber > 0)
        {
          local.ApplyApCollToRcvInd.Flag = "Y";
        }
        else
        {
          local.ApplyApCollToRcvInd.Flag = "N";
        }
      }

      ++local.TempTotals.TempNbrOfCollRead.Count;
      local.TempTotals.TempAmtOfCollRead.TotalCurrency += entities.Collection.
        Amount;

      if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) == 'Y'
        || AsChar(entities.CashReceiptType.CategoryIndicator) == 'N')
      {
        if (AsChar(entities.Collection.AdjustedInd) == 'Y')
        {
          if (Lt(local.Initialized.Date, entities.Collection.DisbursementDt))
          {
            // This collection was previously processed and is being backed off 
            // today.
            local.ForUpdate.DisbursementDt = entities.Collection.DisbursementDt;
            local.ForUpdate.DisbursementAdjProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
          }
          else
          {
            // This collection was created & backed off on the same day so set 
            // the dates without creating any disbursements.
            try
            {
              UpdateCollection();
              ++local.TempTotals.TempNbrOfCollBackedOff.Count;
              local.TempTotals.TempAmtOfCollsBackedOff.
                TotalCurrency += entities.Collection.Amount;

              continue;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_COLLECTION_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_COLLECTION_PV";

                  break;
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
          // This is a normal collection that is not being backed off.
          local.ForUpdate.DisbursementDt =
            local.ProgramProcessingInfo.ProcessDate;
          local.ForUpdate.DisbursementAdjProcessDate = local.Initialized.Date;
        }
      }
      else
      {
        if (AsChar(entities.Collection.AdjustedInd) == 'Y')
        {
          if (Lt(local.Initialized.Date, entities.Collection.DisbursementDt))
          {
            // The cash receipt is not fully applied and there has been an 
            // adjustment so do not process this collection yet.
            local.CollectionSign.Text1 = "-";
          }
          else
          {
            // This collection was created & backed off on the same day so set 
            // the dates without creating any disbursements.
            try
            {
              UpdateCollection();
              ++local.TempTotals.TempNbrOfCollBackedOff.Count;
              local.TempTotals.TempAmtOfCollsBackedOff.
                TotalCurrency += entities.Collection.Amount;

              continue;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_COLLECTION_NU";

                  goto Test2;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_COLLECTION_PV";

                  goto Test2;
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
          if (ReadCollection())
          {
            // The cash receipt is not fully applied and there has been an 
            // adjustment so do NOT process this collection yet.
            local.CollectionSign.Text1 = "";

            goto Test1;
          }

          // The cash receipt is not fully applied but there has NOT been an 
          // adjustment so process this collection.
          local.ForUpdate.DisbursementDt =
            local.ProgramProcessingInfo.ProcessDate;
          local.ForUpdate.DisbursementAdjProcessDate = local.Initialized.Date;

          goto Test2;
        }

Test1:

        // Print info on bypassed collection.
        ++local.TempTotals.TempNbrOfCollNotFulApl.Count;
        local.TempTotals.TempAmtOfCollNotFulApl.TotalCurrency += entities.
          Collection.Amount;

        if (!Equal(entities.Obligor2.Number, local.HoldCollNotFullyAppAp.Number))
          
        {
          local.HoldCollNotFullyAppAp.Number = entities.Obligor2.Number;
          ++local.TempTotals.TempNbrOfApWoColFulAp.Count;
          local.EabReportSend.RptDetail = "";
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        UseFnAbConcatCrAndCrd();
        local.UnformattedDate.Date = Date(entities.Collection.CreatedTmst);
        UseCabDate2TextWithHyphens();
        local.Loc112AmountField.Number112 = entities.Collection.Amount;
        UseCabFormat112AmtFieldTo1();
        local.Loc112AmountField.Number112 =
          entities.CashReceiptDetail.CollectionAmount;
        UseCabFormat112AmtFieldTo2();
        local.Loc112AmountField.Number112 =
          entities.CashReceiptDetail.CollectionAmount - (
            entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault());
        UseCabFormat112AmtFieldTo3();
        local.EabReportSend.RptDetail = "Coll not fully appl " + entities
          .Obligor2.Number + "  Ref # " + local.CrdCrComboNo.CrdCrCombo + "Created " +
          local.FormattedDate.Text10 + "  Coll Amt " + local
          .CollectionSign.Text1 + local.LolFormatted82AmtField1.Text9 + " " + entities
          .Collection.ProgramAppliedTo + " CRD Amt " + local
          .Formatted82AmtField2.Text9 + "  Undistr " + local
          .Formatted82AmtField3.Text9;
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        continue;
      }

Test2:

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -----------------------------------------------------------------
        // 10/22/99 - SWSRKXD PR#77874
        // Error off collections with program_applied_to_code = 'NC'.
        // -----------------------------------------------------------------
        if (Equal(entities.Collection.ProgramAppliedTo, "NC"))
        {
          ExitState = "FN0000_NC_COLL_CANNOT_BE_DISB";

          goto Test3;
        }

        local.AppliedApCollToRcvInd.Flag = "N";

        if (Equal(entities.Obligor2.Number, local.DispObligor.Number))
        {
          local.DispObligor.Number = local.DispObligor.Number;
        }

        UseFnProcessADistCollTran();

        if (AsChar(local.AppliedApCollToRcvInd.Flag) == 'Y')
        {
          local.DecreaseApApplyRcvCnt.Flag = "Y";
        }
      }

Test3:

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -----------------------------------------------------------------
        // 10/22/99 - SWSRKXD PR#77874
        // CAB fn_process_a_dist_coll_tran will never supply
        // local_state_collection_ind. This code has been disabled
        // and views left here just incase business change their mind.
        // -----------------------------------------------------------------
        if (AsChar(local.StateCollectionInd.Flag) == 'Y')
        {
        }
        else
        {
          ++local.TempTotals.TempNbrOfCreditsCreated.Count;
          local.TempTotals.TempAmtOfCreditsCreated.TotalCurrency += entities.
            Collection.Amount;
        }

        if (AsChar(local.DisplayInd.Flag) == 'Y')
        {
          UseFnB650WriteOutCollectionErr();
        }
      }
      else
      {
        UseFnB650WriteOutCollectionErr();
        ++local.TempTotals.TempNbrOfErrorsCreated.Count;
        local.TempTotals.TempAmtOfErrorsCreated.TotalCurrency += entities.
          Collection.Amount;

        if (AsChar(local.Abort.Flag) == 'Y')
        {
          UseFnB650WriteTotals();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (AsChar(entities.CashReceiptType.CategoryIndicator) == 'C')
        {
          // C = Cash
          if (Equal(entities.Collection.ProgramAppliedTo, "NA") || Equal
            (entities.Collection.ProgramAppliedTo, "NAI"))
          {
            // NA or NAI = Non-TAF
            if (IsExitState("FN0000_STATE_CANT_BE_AR_ON_NA"))
            {
              ++local.TempTotals.TempNbrCNaKsAr.Count;
              local.TempTotals.TempAmtCNaKsAr.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("FN0000_JJA_CANT_BE_AN_AR"))
            {
              ++local.TempTotals.TempNbrCNaJjAr.Count;
              local.TempTotals.TempAmtCNaJjAr.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("FN0000_COULD_NOT_DET_OBLGEE"))
            {
              ++local.TempTotals.TempNbrCNaNotDeterm.Count;
              local.TempTotals.TempAmtCNaNotDeterm.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("FN0000_CURRENT_INTER_ST_RQST_NF"))
            {
              ++local.TempTotals.TempNbrCNaInterSt.Count;
              local.TempTotals.TempAmtCNaInterSt.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("CASE_NF"))
            {
              ++local.TempTotals.TempNbrCNaCaseNf.Count;
              local.TempTotals.TempAmtCNaCaseNf.TotalCurrency += entities.
                Collection.Amount;
            }
            else
            {
              ++local.TempTotals.TempNbrCNaAllOther.Count;
              local.TempTotals.TempAmtCNaAllOther.TotalCurrency += entities.
                Collection.Amount;
            }
          }
          else
          {
            // AF or AFI or FCI = TAF
            if (IsExitState("FN0000_STATE_CANT_BE_AR_ON_NA"))
            {
              ++local.TempTotals.TempNbrCAfKsAr.Count;
              local.TempTotals.TempAmtCAfKsAr.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("FN0000_JJA_CANT_BE_AN_AR"))
            {
              ++local.TempTotals.TempNbrCAfJjAr.Count;
              local.TempTotals.TempAmtCAfJjAr.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("FN0000_COULD_NOT_DET_OBLGEE"))
            {
              ++local.TempTotals.TempNbrCAfNotDeterm.Count;
              local.TempTotals.TempAmtCAfNotDeter.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("FN0000_CURRENT_INTER_ST_RQST_NF"))
            {
              ++local.TempTotals.TempNbrCAfInterSt.Count;
              local.TempTotals.TempAmtCAfInterSt.TotalCurrency += entities.
                Collection.Amount;
            }
            else if (IsExitState("CASE_NF"))
            {
              ++local.TempTotals.TempNbrCAfCaseNf.Count;
              local.TempTotals.TempAmtCAfCaseNf.TotalCurrency += entities.
                Collection.Amount;
            }
            else
            {
              ++local.TempTotals.TempNbrCAfAllOther.Count;
              local.TempTotals.TempAmtCAfAllOther.TotalCurrency += entities.
                Collection.Amount;
            }
          }
        }
        else
        {
          // N = Non-Cash
          if (IsExitState("FN0000_STATE_CANT_BE_AR_ON_NA"))
          {
            ++local.TempTotals.TempNbrNKsAr.Count;
            local.TempTotals.TempAmtNKsAr.TotalCurrency += entities.Collection.
              Amount;
          }
          else if (IsExitState("FN0000_JJA_CANT_BE_AN_AR"))
          {
            ++local.TempTotals.TempNbrNJjAr.Count;
            local.TempTotals.TempAmtNJjAr.TotalCurrency += entities.Collection.
              Amount;
          }
          else if (IsExitState("FN0000_COULD_NOT_DET_OBLGEE"))
          {
            ++local.TempTotals.TempNbrNNotDeterm.Count;
            local.TempTotals.TempAmtNNotDeterm.TotalCurrency += entities.
              Collection.Amount;
          }
          else if (IsExitState("FN0000_CURRENT_INTER_ST_RQST_NF"))
          {
            ++local.TempTotals.TempNbrNInterSt.Count;
            local.TempTotals.TempAmtNInterSt.TotalCurrency += entities.
              Collection.Amount;
          }
          else if (IsExitState("CASE_NF"))
          {
            ++local.TempTotals.TempNbrNCaseNf.Count;
            local.TempTotals.TempAmtNCaseNf.TotalCurrency += entities.
              Collection.Amount;
          }
          else
          {
            ++local.TempTotals.TempNbrNAllOther.Count;
            local.TempTotals.TempAmtNAllOther.TotalCurrency += entities.
              Collection.Amount;
          }
        }
      }

      if (local.TempTotals.TempNbrOfCollRead.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // ***** Call an external that does a DB2 commit using a Cobol program.
        if (AsChar(local.TestRunInd.Flag) != 'Y')
        {
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail =
              "Error occurred in EXT_TO_DO_A_COMMIT  " + NumberToString
              (local.PassArea.NumericReturnCode, 15, 2);
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          UseFnB650AccumulateTotals();
        }
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (AsChar(local.TestRunInd.Flag) == 'Y')
    {
      ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    UseFnB650AccumulateTotals();
    UseFnB650WriteTotals();
    local.EabReportSend.RptDetail =
      "  ** Total number of Collections elligible for rcv " + NumberToString
      (local.CollsElligibleForRcvCnt.Count, 7, 9);
    UseCabErrorReport1();

    // ****************
    // Close the files
    // ****************
    local.EabFileHandling.Action = "CLOSE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();
    UseCabErrorReport2();
  }

  private static void MoveCollection1(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.DisbursementDt = source.DisbursementDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjProcessDate = source.CollectionAdjProcessDate;
    target.DisbursementAdjProcessDate = source.DisbursementAdjProcessDate;
    target.DisbursementProcessingNeedInd = source.DisbursementProcessingNeedInd;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.AppliedToOrderTypeCode = source.AppliedToOrderTypeCode;
    target.AppliedToFuture = source.AppliedToFuture;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.DisbursementDt = source.DisbursementDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjProcessDate = source.CollectionAdjProcessDate;
    target.DisbursementAdjProcessDate = source.DisbursementAdjProcessDate;
    target.DisbursementProcessingNeedInd = source.DisbursementProcessingNeedInd;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.AppliedToOrderTypeCode = source.AppliedToOrderTypeCode;
    target.AppliedToFuture = source.AppliedToFuture;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
    target.ArNumber = source.ArNumber;
  }

  private static void MoveCollection3(Collection source, Collection target)
  {
    target.DisbursementDt = source.DisbursementDt;
    target.DisbursementAdjProcessDate = source.DisbursementAdjProcessDate;
  }

  private static void MoveFinalTotals1(FnB650AccumulateTotals.Export.
    FinalTotalsGroup source, Local.FinalTotalsGroup target)
  {
    target.FinalNbrOfCollRead.Count = source.FinalNbrOfCollRead.Count;
    target.FinalAmtOfCollRead.TotalCurrency =
      source.FinalAmtOfCollRead.TotalCurrency;
    target.FinalNbrOfAps.Count = source.FinalNbrOfAps.Count;
    target.FinalNbrOfCollBackedOff.Count = source.FinalNbrOfCollBackedOff.Count;
    target.FinalAmtOfCollBackedOff.TotalCurrency =
      source.FinalAmtOfCollBackedOff.TotalCurrency;
    target.FinalNbrOfErrorsCreated.Count = source.FinalNbrOfErrorsCreated.Count;
    target.FinalAmtOfErrorsCreated.TotalCurrency =
      source.FinalAmtOfErrorsCreated.TotalCurrency;
    target.FinalNbrOfCollNotFulAp.Count = source.FinalNbrOfCollNotFulAp.Count;
    target.FinalAmtOfCollNotFulAp.TotalCurrency =
      source.FinalAmtOfCollNotFulAp.TotalCurrency;
    target.FinalNbrOfCreditsCreated.Count =
      source.FinalNbrOfCreditsCreated.Count;
    target.FinalAmtOfCreditsCreated.TotalCurrency =
      source.FinalAmtOfCreditsCreated.TotalCurrency;
    target.FinalNbrOfApWoColFlAp.Count = source.FinalNbrOfApsWoColFlA.Count;
    target.FinalNbrCNaKsAr.Count = source.FinalNbrCNaKsAr.Count;
    target.FinalAmtCNaKsAr.TotalCurrency = source.FinalAmtCNaKsAr.TotalCurrency;
    target.FinalNbrCNaJjAr.Count = source.FinalNbrCNaJjAr.Count;
    target.FinalAmtCNaJjAr.TotalCurrency = source.FinalAmtCNaJjAr.TotalCurrency;
    target.FinalNbrCNaNotDeterm.Count = source.FinalNbrCNaNotDeterm.Count;
    target.FinalAmtCNaNotDeterm.TotalCurrency =
      source.FinalAmtCNaNotDeterm.TotalCurrency;
    target.FinalNbrCNaCaseNf.Count = source.FinalNbrCNaCaseNf.Count;
    target.FinalAmtCNaCaseNf.TotalCurrency =
      source.FinalAmtCNaCaseNf.TotalCurrency;
    target.FinalNbrCNaInterSt.Count = source.FinalNbrCNaInterSt.Count;
    target.FinalAmtCNaInterSt.TotalCurrency =
      source.FinalAmtCNaInterSt.TotalCurrency;
    target.FinalNbrCNaAllOther.Count = source.FinalNbrCNaAllOther.Count;
    target.FinalAmtCNaAllOther.TotalCurrency =
      source.FinalAmtCNaAllOther.TotalCurrency;
    target.FinalNbrCAfKsAr.Count = source.FinalNbrCAfKsAr.Count;
    target.FinalAmtCAfKsAr.TotalCurrency = source.FinalAmtCAfKsAr.TotalCurrency;
    target.FinalNbrCAfJjAr.Count = source.FinalNbrCAfJjAr.Count;
    target.FinalAmtCAfJjAr.TotalCurrency = source.FinalAmtCAfJjAr.TotalCurrency;
    target.FinalNbrCAfNotDeterm.Count = source.FinalNbrCAfNotDeterm.Count;
    target.FinalAmtCAfNotDeterm.TotalCurrency =
      source.FinalAmtCAfNotDeterm.TotalCurrency;
    target.FinalNbrCAfCaseNf.Count = source.FinalNbrCAfCaseNf.Count;
    target.FinalAmtCAfCaseNf.TotalCurrency =
      source.FinalAmtCAfCaseNf.TotalCurrency;
    target.FinalNbrCAfInterSt.Count = source.FinalNbrCAfInterSt.Count;
    target.FinalAmtCAfInterSt.TotalCurrency =
      source.FinalAmtCAfInterSt.TotalCurrency;
    target.FinalNbrCAfAllOther.Count = source.FinalNbrCAfAllOther.Count;
    target.FinalAmtCAfAllOther.TotalCurrency =
      source.FinalAmtCAfAllOther.TotalCurrency;
    target.FinalNbrNKsAr.Count = source.FinalNbrNKsAr.Count;
    target.FinalAmtNKsAr.TotalCurrency = source.FinalAmtNKsAr.TotalCurrency;
    target.FinalNbrNJjAr.Count = source.FinalNbrNJjAr.Count;
    target.FinalAmtNJjAr.TotalCurrency = source.FinalAmtNJjAr.TotalCurrency;
    target.FinalNbrNNotDeterm.Count = source.FinalNbrNNotDeterm.Count;
    target.FinalAmtNNotDeterm.TotalCurrency =
      source.FinalAmtNNotDeterm.TotalCurrency;
    target.FinalNbrNCaseNf.Count = source.FinalNbrNCaseNf.Count;
    target.FinalAmtNCaseNf.TotalCurrency = source.FinalAmtNCaseNf.TotalCurrency;
    target.FinalNbrNInterSt.Count = source.FinalNbrNInterSt.Count;
    target.FinalAmtNInterSt.TotalCurrency =
      source.FinalAmtNInterSt.TotalCurrency;
    target.FinalNbrNAllOther.Count = source.FinalNbrNAllOther.Count;
    target.FinalAmtNAllOther.TotalCurrency =
      source.FinalAmtNAllOther.TotalCurrency;
  }

  private static void MoveFinalTotals2(Local.FinalTotalsGroup source,
    FnB650AccumulateTotals.Export.FinalTotalsGroup target)
  {
    target.FinalNbrOfCollRead.Count = source.FinalNbrOfCollRead.Count;
    target.FinalAmtOfCollRead.TotalCurrency =
      source.FinalAmtOfCollRead.TotalCurrency;
    target.FinalNbrOfAps.Count = source.FinalNbrOfAps.Count;
    target.FinalNbrOfCollBackedOff.Count = source.FinalNbrOfCollBackedOff.Count;
    target.FinalAmtOfCollBackedOff.TotalCurrency =
      source.FinalAmtOfCollBackedOff.TotalCurrency;
    target.FinalNbrOfErrorsCreated.Count = source.FinalNbrOfErrorsCreated.Count;
    target.FinalAmtOfErrorsCreated.TotalCurrency =
      source.FinalAmtOfErrorsCreated.TotalCurrency;
    target.FinalNbrOfCollNotFulAp.Count = source.FinalNbrOfCollNotFulAp.Count;
    target.FinalAmtOfCollNotFulAp.TotalCurrency =
      source.FinalAmtOfCollNotFulAp.TotalCurrency;
    target.FinalNbrOfCreditsCreated.Count =
      source.FinalNbrOfCreditsCreated.Count;
    target.FinalAmtOfCreditsCreated.TotalCurrency =
      source.FinalAmtOfCreditsCreated.TotalCurrency;
    target.FinalNbrOfApsWoColFlA.Count = source.FinalNbrOfApWoColFlAp.Count;
    target.FinalNbrCNaKsAr.Count = source.FinalNbrCNaKsAr.Count;
    target.FinalAmtCNaKsAr.TotalCurrency = source.FinalAmtCNaKsAr.TotalCurrency;
    target.FinalNbrCNaJjAr.Count = source.FinalNbrCNaJjAr.Count;
    target.FinalAmtCNaJjAr.TotalCurrency = source.FinalAmtCNaJjAr.TotalCurrency;
    target.FinalNbrCNaNotDeterm.Count = source.FinalNbrCNaNotDeterm.Count;
    target.FinalAmtCNaNotDeterm.TotalCurrency =
      source.FinalAmtCNaNotDeterm.TotalCurrency;
    target.FinalNbrCNaCaseNf.Count = source.FinalNbrCNaCaseNf.Count;
    target.FinalAmtCNaCaseNf.TotalCurrency =
      source.FinalAmtCNaCaseNf.TotalCurrency;
    target.FinalNbrCNaInterSt.Count = source.FinalNbrCNaInterSt.Count;
    target.FinalAmtCNaInterSt.TotalCurrency =
      source.FinalAmtCNaInterSt.TotalCurrency;
    target.FinalNbrCNaAllOther.Count = source.FinalNbrCNaAllOther.Count;
    target.FinalAmtCNaAllOther.TotalCurrency =
      source.FinalAmtCNaAllOther.TotalCurrency;
    target.FinalNbrCAfKsAr.Count = source.FinalNbrCAfKsAr.Count;
    target.FinalAmtCAfKsAr.TotalCurrency = source.FinalAmtCAfKsAr.TotalCurrency;
    target.FinalNbrCAfJjAr.Count = source.FinalNbrCAfJjAr.Count;
    target.FinalAmtCAfJjAr.TotalCurrency = source.FinalAmtCAfJjAr.TotalCurrency;
    target.FinalNbrCAfNotDeterm.Count = source.FinalNbrCAfNotDeterm.Count;
    target.FinalAmtCAfNotDeterm.TotalCurrency =
      source.FinalAmtCAfNotDeterm.TotalCurrency;
    target.FinalNbrCAfCaseNf.Count = source.FinalNbrCAfCaseNf.Count;
    target.FinalAmtCAfCaseNf.TotalCurrency =
      source.FinalAmtCAfCaseNf.TotalCurrency;
    target.FinalNbrCAfInterSt.Count = source.FinalNbrCAfInterSt.Count;
    target.FinalAmtCAfInterSt.TotalCurrency =
      source.FinalAmtCAfInterSt.TotalCurrency;
    target.FinalNbrCAfAllOther.Count = source.FinalNbrCAfAllOther.Count;
    target.FinalAmtCAfAllOther.TotalCurrency =
      source.FinalAmtCAfAllOther.TotalCurrency;
    target.FinalNbrNKsAr.Count = source.FinalNbrNKsAr.Count;
    target.FinalAmtNKsAr.TotalCurrency = source.FinalAmtNKsAr.TotalCurrency;
    target.FinalNbrNJjAr.Count = source.FinalNbrNJjAr.Count;
    target.FinalAmtNJjAr.TotalCurrency = source.FinalAmtNJjAr.TotalCurrency;
    target.FinalNbrNNotDeterm.Count = source.FinalNbrNNotDeterm.Count;
    target.FinalAmtNNotDeterm.TotalCurrency =
      source.FinalAmtNNotDeterm.TotalCurrency;
    target.FinalNbrNCaseNf.Count = source.FinalNbrNCaseNf.Count;
    target.FinalAmtNCaseNf.TotalCurrency = source.FinalAmtNCaseNf.TotalCurrency;
    target.FinalNbrNInterSt.Count = source.FinalNbrNInterSt.Count;
    target.FinalAmtNInterSt.TotalCurrency =
      source.FinalAmtNInterSt.TotalCurrency;
    target.FinalNbrNAllOther.Count = source.FinalNbrNAllOther.Count;
    target.FinalAmtNAllOther.TotalCurrency =
      source.FinalAmtNAllOther.TotalCurrency;
  }

  private static void MoveFinalTotals3(Local.FinalTotalsGroup source,
    FnB650WriteTotals.Import.TotalsGroup target)
  {
    target.NbrOfCollRead.Count = source.FinalNbrOfCollRead.Count;
    target.AmtOfCollRead.TotalCurrency =
      source.FinalAmtOfCollRead.TotalCurrency;
    target.NbrOfAps.Count = source.FinalNbrOfAps.Count;
    target.NbrOfCollBackedOff.Count = source.FinalNbrOfCollBackedOff.Count;
    target.AmtOfCollBackedOff.TotalCurrency =
      source.FinalAmtOfCollBackedOff.TotalCurrency;
    target.NbrOfErrorsCreated.Count = source.FinalNbrOfErrorsCreated.Count;
    target.AmtOfErrorsCreated.TotalCurrency =
      source.FinalAmtOfErrorsCreated.TotalCurrency;
    target.NbrOfCollNotFulApl.Count = source.FinalNbrOfCollNotFulAp.Count;
    target.AmtOfCollNotFulApl.TotalCurrency =
      source.FinalAmtOfCollNotFulAp.TotalCurrency;
    target.NbrOfCreditsCreated.Count = source.FinalNbrOfCreditsCreated.Count;
    target.AmtOfCreditsCreated.TotalCurrency =
      source.FinalAmtOfCreditsCreated.TotalCurrency;
    target.NbrOfApWoCollFullyAppl.Count = source.FinalNbrOfApWoColFlAp.Count;
    target.NbrCNaKsAr.Count = source.FinalNbrCNaKsAr.Count;
    target.AmtCNaKsAr.TotalCurrency = source.FinalAmtCNaKsAr.TotalCurrency;
    target.NbrCNaJjAr.Count = source.FinalNbrCNaJjAr.Count;
    target.AmtCNaJjAr.TotalCurrency = source.FinalAmtCNaJjAr.TotalCurrency;
    target.NbrCNaNotDeterm.Count = source.FinalNbrCNaNotDeterm.Count;
    target.AmtCNaNotDeterm.TotalCurrency =
      source.FinalAmtCNaNotDeterm.TotalCurrency;
    target.NbrCNaCaseNf.Count = source.FinalNbrCNaCaseNf.Count;
    target.AmtCNaCaseNf.TotalCurrency = source.FinalAmtCNaCaseNf.TotalCurrency;
    target.NbrCNaInterSt.Count = source.FinalNbrCNaInterSt.Count;
    target.AmtCNaInterSt.TotalCurrency =
      source.FinalAmtCNaInterSt.TotalCurrency;
    target.NbrCNaAllOther.Count = source.FinalNbrCNaAllOther.Count;
    target.AmtCNaAllOther.TotalCurrency =
      source.FinalAmtCNaAllOther.TotalCurrency;
    target.NbrCAfKsAr.Count = source.FinalNbrCAfKsAr.Count;
    target.AmtCAfKsAr.TotalCurrency = source.FinalAmtCAfKsAr.TotalCurrency;
    target.NbrCAfJjAr.Count = source.FinalNbrCAfJjAr.Count;
    target.AmtCAfJjAr.TotalCurrency = source.FinalAmtCAfJjAr.TotalCurrency;
    target.NbrCAfNotDeterm.Count = source.FinalNbrCAfNotDeterm.Count;
    target.AmtCAfNotDeterm.TotalCurrency =
      source.FinalAmtCAfNotDeterm.TotalCurrency;
    target.NbrCAfCaseNf.Count = source.FinalNbrCAfCaseNf.Count;
    target.AmtCAfCaseNf.TotalCurrency = source.FinalAmtCAfCaseNf.TotalCurrency;
    target.NbrCAfInterSt.Count = source.FinalNbrCAfInterSt.Count;
    target.AmtCAfInterSt.TotalCurrency =
      source.FinalAmtCAfInterSt.TotalCurrency;
    target.NbrCAfAllOther.Count = source.FinalNbrCAfAllOther.Count;
    target.AmtCAfAllOther.TotalCurrency =
      source.FinalAmtCAfAllOther.TotalCurrency;
    target.NbrNKsAr.Count = source.FinalNbrNKsAr.Count;
    target.AmtNKsAr.TotalCurrency = source.FinalAmtNKsAr.TotalCurrency;
    target.NbrNJjAr.Count = source.FinalNbrNJjAr.Count;
    target.AmtNJjAr.TotalCurrency = source.FinalAmtNJjAr.TotalCurrency;
    target.NbrNNotDeterm.Count = source.FinalNbrNNotDeterm.Count;
    target.AmtNNotDeterm.TotalCurrency =
      source.FinalAmtNNotDeterm.TotalCurrency;
    target.NbrNCaseNf.Count = source.FinalNbrNCaseNf.Count;
    target.AmtNCaseNf.TotalCurrency = source.FinalAmtNCaseNf.TotalCurrency;
    target.NbrNInterSt.Count = source.FinalNbrNInterSt.Count;
    target.AmtNInterSt.TotalCurrency = source.FinalAmtNInterSt.TotalCurrency;
    target.NbrNAllOther.Count = source.FinalNbrNAllOther.Count;
    target.AmtNAllOther.TotalCurrency = source.FinalAmtNAllOther.TotalCurrency;
  }

  private static void MoveHardcodePass1(Local.HardcodePassGroup source,
    FnB650WriteOutCollectionErr.Import.HardcodeGroup target)
  {
    target.HardcodeSpousalSupport.SystemGeneratedIdentifier =
      source.HardcodeSpousalSupport.SystemGeneratedIdentifier;
    target.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier =
      source.HardcodeSpArrearsJudgemt.SystemGeneratedIdentifier;
    target.HardcodeVoluntary.SystemGeneratedIdentifier =
      source.HardcodeVoluntary.SystemGeneratedIdentifier;
  }

  private static void MoveHardcodePass2(Local.HardcodePassGroup source,
    FnProcessADistCollTran.Import.HardcodeGroup target)
  {
    target.HardcodeSpousalSupport.SystemGeneratedIdentifier =
      source.HardcodeSpousalSupport.SystemGeneratedIdentifier;
    target.HardcodeSpArrearsJudgemt.SystemGeneratedIdentifier =
      source.HardcodeSpArrearsJudgemt.SystemGeneratedIdentifier;
    target.HardcodeVoluntary.SystemGeneratedIdentifier =
      source.HardcodeVoluntary.SystemGeneratedIdentifier;
  }

  private static void MoveTempTotals1(FnB650AccumulateTotals.Export.
    TempTotalsGroup source, Local.TempTotalsGroup target)
  {
    target.TempNbrOfCollRead.Count = source.TempNbrOfCollRead.Count;
    target.TempAmtOfCollRead.TotalCurrency =
      source.TempAmtOfCollRead.TotalCurrency;
    target.TempNbrOfAps.Count = source.TrmpNbrOfAps.Count;
    target.TempNbrOfCollBackedOff.Count = source.TempNbrOfCollBackedOff.Count;
    target.TempAmtOfCollsBackedOff.TotalCurrency =
      source.TempAmtOfCollBackedOff.TotalCurrency;
    target.TempNbrOfErrorsCreated.Count = source.TempNbrOfErrorsCreated.Count;
    target.TempAmtOfErrorsCreated.TotalCurrency =
      source.TempAmtOfErrorsCreated.TotalCurrency;
    target.TempNbrOfCollNotFulApl.Count = source.TempNbrOfCollNotFulApl.Count;
    target.TempAmtOfCollNotFulApl.TotalCurrency =
      source.TempAmtOfCollNotFulApl.TotalCurrency;
    target.TempNbrOfCreditsCreated.Count = source.TempNbrOfCreditsCreated.Count;
    target.TempAmtOfCreditsCreated.TotalCurrency =
      source.TempAmtOfCreditsCreated.TotalCurrency;
    target.TempNbrOfApWoColFulAp.Count = source.TempNbrOfApsWoCollFlA.Count;
    target.TempNbrCNaKsAr.Count = source.TempNbrCNaKsAr.Count;
    target.TempAmtCNaKsAr.TotalCurrency = source.TempAmtCNaKsAr.TotalCurrency;
    target.TempNbrCNaJjAr.Count = source.TempNbrCNaJjAr.Count;
    target.TempAmtCNaJjAr.TotalCurrency = source.TempAmtCNaJjAr.TotalCurrency;
    target.TempNbrCNaNotDeterm.Count = source.TempNbrCNaNotDeterm.Count;
    target.TempAmtCNaNotDeterm.TotalCurrency =
      source.TempAmtCNaNotDeterm.TotalCurrency;
    target.TempNbrCNaCaseNf.Count = source.TempNbrCNaCaseNf.Count;
    target.TempAmtCNaCaseNf.TotalCurrency =
      source.TempAmtCNaCaseNf.TotalCurrency;
    target.TempNbrCNaInterSt.Count = source.TempNbrCNaInterSt.Count;
    target.TempAmtCNaInterSt.TotalCurrency =
      source.TempAmtCNaInterSt.TotalCurrency;
    target.TempNbrCNaAllOther.Count = source.TempNbrCNaAllOther.Count;
    target.TempAmtCNaAllOther.TotalCurrency =
      source.TempAmtCNaAllOther.TotalCurrency;
    target.TempNbrCAfKsAr.Count = source.TempNbrCAfKsAr.Count;
    target.TempAmtCAfKsAr.TotalCurrency = source.TempAmtCAfKsAr.TotalCurrency;
    target.TempNbrCAfJjAr.Count = source.TempNbrCAfJjAr.Count;
    target.TempAmtCAfJjAr.TotalCurrency = source.TempAmtCAfJjAr.TotalCurrency;
    target.TempNbrCAfNotDeterm.Count = source.TempNbrCAfNotDeterm.Count;
    target.TempAmtCAfNotDeter.TotalCurrency =
      source.TempAmtCAfNotDeterm.TotalCurrency;
    target.TempNbrCAfCaseNf.Count = source.TempNbrCAfCaseNf.Count;
    target.TempAmtCAfCaseNf.TotalCurrency =
      source.TempAmtCAfCaseNf.TotalCurrency;
    target.TempNbrCAfInterSt.Count = source.TempNbrCAfInterSt.Count;
    target.TempAmtCAfInterSt.TotalCurrency =
      source.TempAmtCAfInterSt.TotalCurrency;
    target.TempNbrCAfAllOther.Count = source.TempNbrCAfAllOther.Count;
    target.TempAmtCAfAllOther.TotalCurrency =
      source.TempAmtCAfAllOther.TotalCurrency;
    target.TempNbrNKsAr.Count = source.TempNbrNKsAr.Count;
    target.TempAmtNKsAr.TotalCurrency = source.TempAmtNKsAr.TotalCurrency;
    target.TempNbrNJjAr.Count = source.TempNbrNJjAr.Count;
    target.TempAmtNJjAr.TotalCurrency = source.TempAmtNJjAr.TotalCurrency;
    target.TempNbrNNotDeterm.Count = source.TempNbrNNotDeterm.Count;
    target.TempAmtNNotDeterm.TotalCurrency =
      source.TempAmtNNotDeterm.TotalCurrency;
    target.TempNbrNCaseNf.Count = source.TempNbrNCaseNf.Count;
    target.TempAmtNCaseNf.TotalCurrency = source.TempAmtNCaseNf.TotalCurrency;
    target.TempNbrNInterSt.Count = source.TempNbrNInterSt.Count;
    target.TempAmtNInterSt.TotalCurrency = source.TempAmtNInterSt.TotalCurrency;
    target.TempNbrNAllOther.Count = source.TempNbrNAllOther.Count;
    target.TempAmtNAllOther.TotalCurrency =
      source.TempAmtNAllOther.TotalCurrency;
  }

  private static void MoveTempTotals2(Local.TempTotalsGroup source,
    FnB650AccumulateTotals.Export.TempTotalsGroup target)
  {
    target.TempNbrOfCollRead.Count = source.TempNbrOfCollRead.Count;
    target.TempAmtOfCollRead.TotalCurrency =
      source.TempAmtOfCollRead.TotalCurrency;
    target.TrmpNbrOfAps.Count = source.TempNbrOfAps.Count;
    target.TempNbrOfCollBackedOff.Count = source.TempNbrOfCollBackedOff.Count;
    target.TempAmtOfCollBackedOff.TotalCurrency =
      source.TempAmtOfCollsBackedOff.TotalCurrency;
    target.TempNbrOfErrorsCreated.Count = source.TempNbrOfErrorsCreated.Count;
    target.TempAmtOfErrorsCreated.TotalCurrency =
      source.TempAmtOfErrorsCreated.TotalCurrency;
    target.TempNbrOfCollNotFulApl.Count = source.TempNbrOfCollNotFulApl.Count;
    target.TempAmtOfCollNotFulApl.TotalCurrency =
      source.TempAmtOfCollNotFulApl.TotalCurrency;
    target.TempNbrOfCreditsCreated.Count = source.TempNbrOfCreditsCreated.Count;
    target.TempAmtOfCreditsCreated.TotalCurrency =
      source.TempAmtOfCreditsCreated.TotalCurrency;
    target.TempNbrOfApsWoCollFlA.Count = source.TempNbrOfApWoColFulAp.Count;
    target.TempNbrCNaKsAr.Count = source.TempNbrCNaKsAr.Count;
    target.TempAmtCNaKsAr.TotalCurrency = source.TempAmtCNaKsAr.TotalCurrency;
    target.TempNbrCNaJjAr.Count = source.TempNbrCNaJjAr.Count;
    target.TempAmtCNaJjAr.TotalCurrency = source.TempAmtCNaJjAr.TotalCurrency;
    target.TempNbrCNaNotDeterm.Count = source.TempNbrCNaNotDeterm.Count;
    target.TempAmtCNaNotDeterm.TotalCurrency =
      source.TempAmtCNaNotDeterm.TotalCurrency;
    target.TempNbrCNaCaseNf.Count = source.TempNbrCNaCaseNf.Count;
    target.TempAmtCNaCaseNf.TotalCurrency =
      source.TempAmtCNaCaseNf.TotalCurrency;
    target.TempNbrCNaInterSt.Count = source.TempNbrCNaInterSt.Count;
    target.TempAmtCNaInterSt.TotalCurrency =
      source.TempAmtCNaInterSt.TotalCurrency;
    target.TempNbrCNaAllOther.Count = source.TempNbrCNaAllOther.Count;
    target.TempAmtCNaAllOther.TotalCurrency =
      source.TempAmtCNaAllOther.TotalCurrency;
    target.TempNbrCAfKsAr.Count = source.TempNbrCAfKsAr.Count;
    target.TempAmtCAfKsAr.TotalCurrency = source.TempAmtCAfKsAr.TotalCurrency;
    target.TempNbrCAfJjAr.Count = source.TempNbrCAfJjAr.Count;
    target.TempAmtCAfJjAr.TotalCurrency = source.TempAmtCAfJjAr.TotalCurrency;
    target.TempNbrCAfNotDeterm.Count = source.TempNbrCAfNotDeterm.Count;
    target.TempAmtCAfNotDeterm.TotalCurrency =
      source.TempAmtCAfNotDeter.TotalCurrency;
    target.TempNbrCAfCaseNf.Count = source.TempNbrCAfCaseNf.Count;
    target.TempAmtCAfCaseNf.TotalCurrency =
      source.TempAmtCAfCaseNf.TotalCurrency;
    target.TempNbrCAfInterSt.Count = source.TempNbrCAfInterSt.Count;
    target.TempAmtCAfInterSt.TotalCurrency =
      source.TempAmtCAfInterSt.TotalCurrency;
    target.TempNbrCAfAllOther.Count = source.TempNbrCAfAllOther.Count;
    target.TempAmtCAfAllOther.TotalCurrency =
      source.TempAmtCAfAllOther.TotalCurrency;
    target.TempNbrNKsAr.Count = source.TempNbrNKsAr.Count;
    target.TempAmtNKsAr.TotalCurrency = source.TempAmtNKsAr.TotalCurrency;
    target.TempNbrNJjAr.Count = source.TempNbrNJjAr.Count;
    target.TempAmtNJjAr.TotalCurrency = source.TempAmtNJjAr.TotalCurrency;
    target.TempNbrNNotDeterm.Count = source.TempNbrNNotDeterm.Count;
    target.TempAmtNNotDeterm.TotalCurrency =
      source.TempAmtNNotDeterm.TotalCurrency;
    target.TempNbrNCaseNf.Count = source.TempNbrNCaseNf.Count;
    target.TempAmtNCaseNf.TotalCurrency = source.TempAmtNCaseNf.TotalCurrency;
    target.TempNbrNInterSt.Count = source.TempNbrNInterSt.Count;
    target.TempAmtNInterSt.TotalCurrency = source.TempAmtNInterSt.TotalCurrency;
    target.TempNbrNAllOther.Count = source.TempNbrNAllOther.Count;
    target.TempAmtNAllOther.TotalCurrency =
      source.TempAmtNAllOther.TotalCurrency;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.UnformattedDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.FormattedDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabFormat112AmtFieldTo1()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.Loc112AmountField.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.LolFormatted82AmtField1.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo2()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.Loc112AmountField.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.Formatted82AmtField2.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo3()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.Loc112AmountField.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.Formatted82AmtField3.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    local.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnB650AccumulateTotals()
  {
    var useImport = new FnB650AccumulateTotals.Import();
    var useExport = new FnB650AccumulateTotals.Export();

    MoveTempTotals2(local.TempTotals, useExport.TempTotals);
    MoveFinalTotals2(local.FinalTotals, useExport.FinalTotals);

    Call(FnB650AccumulateTotals.Execute, useImport, useExport);

    MoveTempTotals1(useExport.TempTotals, local.TempTotals);
    MoveFinalTotals1(useExport.FinalTotals, local.FinalTotals);
  }

  private void UseFnB650Initialization()
  {
    var useImport = new FnB650Initialization.Import();
    var useExport = new FnB650Initialization.Export();

    Call(FnB650Initialization.Execute, useImport, useExport);

    local.TestLastObligor.Number = useExport.TestLastObligor.Number;
    local.ApsWithColToApplyToRcv.LastUsedNumber =
      useExport.ApsWithColToApplyToRcv.LastUsedNumber;
    local.TestRunInd.Flag = useExport.TestRunInd.Flag;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    local.TestFirstObligor.Number = useExport.TestFirstObligor.Number;
    local.ProgramProcessingInfo.ProcessDate =
      useExport.ProgramProcessingInfo.ProcessDate;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseFnB650WriteOutCollectionErr()
  {
    var useImport = new FnB650WriteOutCollectionErr.Import();
    var useExport = new FnB650WriteOutCollectionErr.Export();

    useImport.Per.Assign(entities.Debt);
    useImport.CashReceiptType.CategoryIndicator =
      entities.CashReceiptType.CategoryIndicator;
    MoveHardcodePass1(local.HardcodePass, useImport.Hardcode);
    MoveCollection1(entities.Collection, useImport.Collection);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.Obligor.Number = entities.Obligor2.Number;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.DebtDetail.DueDt = local.DebtDetail.DueDt;
    useImport.Case1.Number = local.Case1.Number;
    useImport.Obligee.Number = local.Obligee.Number;
    useImport.Child.Number = local.Child.Number;

    Call(FnB650WriteOutCollectionErr.Execute, useImport, useExport);

    entities.Debt.SystemGeneratedIdentifier =
      useImport.Per.SystemGeneratedIdentifier;
  }

  private void UseFnB650WriteTotals()
  {
    var useImport = new FnB650WriteTotals.Import();
    var useExport = new FnB650WriteTotals.Export();

    MoveFinalTotals3(local.FinalTotals, useImport.Totals);

    Call(FnB650WriteTotals.Execute, useImport, useExport);
  }

  private void UseFnProcessADistCollTran()
  {
    var useImport = new FnProcessADistCollTran.Import();
    var useExport = new FnProcessADistCollTran.Export();

    useImport.ExpCollsEligForRcvCnt.Count = local.CollsElligibleForRcvCnt.Count;
    useImport.ApplyApCollToRcvInd.Flag = local.ApplyApCollToRcvInd.Flag;
    useImport.DisplayInd.Flag = local.DisplayInd.Flag;
    MoveCollection3(local.ForUpdate, useImport.CollectionUpdateValues);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    MoveHardcodePass2(local.HardcodePass, useImport.Hardcode);
    useImport.PerCollection.Assign(entities.Collection);
    useImport.PerDebt.Assign(entities.Debt);
    useImport.PerObligation.Assign(entities.Obligation);
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.Obligor.Number = entities.Obligor2.Number;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.PerCashReceiptType.Assign(entities.CashReceiptType);
    useExport.AppliedApCollToRcvInd.Flag = local.AppliedApCollToRcvInd.Flag;

    Call(FnProcessADistCollTran.Execute, useImport, useExport);

    local.CollsElligibleForRcvCnt.Count = useImport.ExpCollsEligForRcvCnt.Count;
    local.ApplyApCollToRcvInd.Flag = useImport.ApplyApCollToRcvInd.Flag;
    MoveCollection2(useImport.PerCollection, entities.Collection);
    entities.Debt.SystemGeneratedIdentifier =
      useImport.PerDebt.SystemGeneratedIdentifier;
    entities.Obligation.SystemGeneratedIdentifier =
      useImport.PerObligation.SystemGeneratedIdentifier;
    entities.CashReceiptType.CategoryIndicator =
      useImport.PerCashReceiptType.CategoryIndicator;
    local.AppliedApCollToRcvInd.Flag = useExport.AppliedApCollToRcvInd.Flag;
    local.Abort.Flag = useExport.Abort.Flag;
    local.Obligee.Number = useExport.Obligee.Number;
    local.Child.Number = useExport.Child.Number;
    local.Case1.Number = useExport.Case1.Number;
    local.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    local.StateCollectionInd.Flag = useExport.StateCollecionInd.Flag;
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ReadForAdjustments.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ReadForAdjustments.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForAdjustments.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ReadForAdjustments.CrtType = db.GetInt32(reader, 2);
        entities.ReadForAdjustments.CstId = db.GetInt32(reader, 3);
        entities.ReadForAdjustments.CrvId = db.GetInt32(reader, 4);
        entities.ReadForAdjustments.CrdId = db.GetInt32(reader, 5);
        entities.ReadForAdjustments.ObgId = db.GetInt32(reader, 6);
        entities.ReadForAdjustments.CspNumber = db.GetString(reader, 7);
        entities.ReadForAdjustments.CpaType = db.GetString(reader, 8);
        entities.ReadForAdjustments.OtrId = db.GetInt32(reader, 9);
        entities.ReadForAdjustments.OtrType = db.GetString(reader, 10);
        entities.ReadForAdjustments.OtyId = db.GetInt32(reader, 11);
        entities.ReadForAdjustments.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ReadForAdjustments.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.ReadForAdjustments.CpaType);
        CheckValid<Collection>("OtrType", entities.ReadForAdjustments.OtrType);
      });
  }

  private IEnumerable<bool> ReadCollectionDebtObligationObligationTypeObligor()
  {
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligor1.Populated = false;
    entities.Obligor2.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

    return ReadEach("ReadCollectionDebtObligationObligationTypeObligor",
      (db, command) =>
      {
        db.SetDate(
          command, "disbAdjProcDate",
          local.Initialized.Date.GetValueOrDefault());
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
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 8);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 8);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 8);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 8);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 9);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 9);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Debt.CspNumber = db.GetString(reader, 12);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligor1.CspNumber = db.GetString(reader, 12);
        entities.Obligor1.CspNumber = db.GetString(reader, 12);
        entities.Obligor1.CspNumber = db.GetString(reader, 12);
        entities.Obligor2.Number = db.GetString(reader, 12);
        entities.Obligor2.Number = db.GetString(reader, 12);
        entities.Obligor2.Number = db.GetString(reader, 12);
        entities.Obligor2.Number = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Debt.CpaType = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 13);
        entities.Obligor1.Type1 = db.GetString(reader, 13);
        entities.Obligor1.Type1 = db.GetString(reader, 13);
        entities.Obligor1.Type1 = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Debt.Type1 = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Debt.OtyType = db.GetInt32(reader, 16);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 16);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 21);
        entities.Collection.Amount = db.GetDecimal(reader, 22);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 23);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 24);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 25);
        entities.Collection.AppliedToFuture = db.GetString(reader, 26);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 27);
        entities.Collection.ArNumber = db.GetNullableString(reader, 28);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 29);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 30);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 31);
        entities.ObligationType.Code = db.GetString(reader, 32);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 33);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 34);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 35);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 37);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 38);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 39);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligor1.Populated = true;
        entities.Obligor2.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor1.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor1.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor1.Type1);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var disbursementDt = local.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.Collection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableDate(command, "disbDt", disbursementDt);
        db.SetDate(command, "disbAdjProcDate", disbursementDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.DisbursementDt = disbursementDt;
    entities.Collection.DisbursementAdjProcessDate = disbursementDt;
    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A HardcodePassGroup group.</summary>
    [Serializable]
    public class HardcodePassGroup
    {
      /// <summary>
      /// A value of HardcodeSpousalSupport.
      /// </summary>
      [JsonPropertyName("hardcodeSpousalSupport")]
      public ObligationType HardcodeSpousalSupport
      {
        get => hardcodeSpousalSupport ??= new();
        set => hardcodeSpousalSupport = value;
      }

      /// <summary>
      /// A value of HardcodeSpArrearsJudgemt.
      /// </summary>
      [JsonPropertyName("hardcodeSpArrearsJudgemt")]
      public ObligationType HardcodeSpArrearsJudgemt
      {
        get => hardcodeSpArrearsJudgemt ??= new();
        set => hardcodeSpArrearsJudgemt = value;
      }

      /// <summary>
      /// A value of HardcodeVoluntary.
      /// </summary>
      [JsonPropertyName("hardcodeVoluntary")]
      public ObligationType HardcodeVoluntary
      {
        get => hardcodeVoluntary ??= new();
        set => hardcodeVoluntary = value;
      }

      private ObligationType hardcodeSpousalSupport;
      private ObligationType hardcodeSpArrearsJudgemt;
      private ObligationType hardcodeVoluntary;
    }

    /// <summary>A TempTotalsGroup group.</summary>
    [Serializable]
    public class TempTotalsGroup
    {
      /// <summary>
      /// A value of TempNbrOfCollRead.
      /// </summary>
      [JsonPropertyName("tempNbrOfCollRead")]
      public Common TempNbrOfCollRead
      {
        get => tempNbrOfCollRead ??= new();
        set => tempNbrOfCollRead = value;
      }

      /// <summary>
      /// A value of TempAmtOfCollRead.
      /// </summary>
      [JsonPropertyName("tempAmtOfCollRead")]
      public Common TempAmtOfCollRead
      {
        get => tempAmtOfCollRead ??= new();
        set => tempAmtOfCollRead = value;
      }

      /// <summary>
      /// A value of TempNbrOfAps.
      /// </summary>
      [JsonPropertyName("tempNbrOfAps")]
      public Common TempNbrOfAps
      {
        get => tempNbrOfAps ??= new();
        set => tempNbrOfAps = value;
      }

      /// <summary>
      /// A value of TempNbrOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("tempNbrOfCollBackedOff")]
      public Common TempNbrOfCollBackedOff
      {
        get => tempNbrOfCollBackedOff ??= new();
        set => tempNbrOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of TempAmtOfCollsBackedOff.
      /// </summary>
      [JsonPropertyName("tempAmtOfCollsBackedOff")]
      public Common TempAmtOfCollsBackedOff
      {
        get => tempAmtOfCollsBackedOff ??= new();
        set => tempAmtOfCollsBackedOff = value;
      }

      /// <summary>
      /// A value of TempNbrOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("tempNbrOfErrorsCreated")]
      public Common TempNbrOfErrorsCreated
      {
        get => tempNbrOfErrorsCreated ??= new();
        set => tempNbrOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of TempAmtOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("tempAmtOfErrorsCreated")]
      public Common TempAmtOfErrorsCreated
      {
        get => tempAmtOfErrorsCreated ??= new();
        set => tempAmtOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of TempNbrOfCollNotFulApl.
      /// </summary>
      [JsonPropertyName("tempNbrOfCollNotFulApl")]
      public Common TempNbrOfCollNotFulApl
      {
        get => tempNbrOfCollNotFulApl ??= new();
        set => tempNbrOfCollNotFulApl = value;
      }

      /// <summary>
      /// A value of TempAmtOfCollNotFulApl.
      /// </summary>
      [JsonPropertyName("tempAmtOfCollNotFulApl")]
      public Common TempAmtOfCollNotFulApl
      {
        get => tempAmtOfCollNotFulApl ??= new();
        set => tempAmtOfCollNotFulApl = value;
      }

      /// <summary>
      /// A value of TempNbrOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("tempNbrOfCreditsCreated")]
      public Common TempNbrOfCreditsCreated
      {
        get => tempNbrOfCreditsCreated ??= new();
        set => tempNbrOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of TempAmtOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("tempAmtOfCreditsCreated")]
      public Common TempAmtOfCreditsCreated
      {
        get => tempAmtOfCreditsCreated ??= new();
        set => tempAmtOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of TempNbrOfApWoColFulAp.
      /// </summary>
      [JsonPropertyName("tempNbrOfApWoColFulAp")]
      public Common TempNbrOfApWoColFulAp
      {
        get => tempNbrOfApWoColFulAp ??= new();
        set => tempNbrOfApWoColFulAp = value;
      }

      /// <summary>
      /// A value of TempNbrCNaKsAr.
      /// </summary>
      [JsonPropertyName("tempNbrCNaKsAr")]
      public Common TempNbrCNaKsAr
      {
        get => tempNbrCNaKsAr ??= new();
        set => tempNbrCNaKsAr = value;
      }

      /// <summary>
      /// A value of TempAmtCNaKsAr.
      /// </summary>
      [JsonPropertyName("tempAmtCNaKsAr")]
      public Common TempAmtCNaKsAr
      {
        get => tempAmtCNaKsAr ??= new();
        set => tempAmtCNaKsAr = value;
      }

      /// <summary>
      /// A value of TempNbrCNaJjAr.
      /// </summary>
      [JsonPropertyName("tempNbrCNaJjAr")]
      public Common TempNbrCNaJjAr
      {
        get => tempNbrCNaJjAr ??= new();
        set => tempNbrCNaJjAr = value;
      }

      /// <summary>
      /// A value of TempAmtCNaJjAr.
      /// </summary>
      [JsonPropertyName("tempAmtCNaJjAr")]
      public Common TempAmtCNaJjAr
      {
        get => tempAmtCNaJjAr ??= new();
        set => tempAmtCNaJjAr = value;
      }

      /// <summary>
      /// A value of TempNbrCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("tempNbrCNaNotDeterm")]
      public Common TempNbrCNaNotDeterm
      {
        get => tempNbrCNaNotDeterm ??= new();
        set => tempNbrCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of TempAmtCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("tempAmtCNaNotDeterm")]
      public Common TempAmtCNaNotDeterm
      {
        get => tempAmtCNaNotDeterm ??= new();
        set => tempAmtCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of TempNbrCNaCaseNf.
      /// </summary>
      [JsonPropertyName("tempNbrCNaCaseNf")]
      public Common TempNbrCNaCaseNf
      {
        get => tempNbrCNaCaseNf ??= new();
        set => tempNbrCNaCaseNf = value;
      }

      /// <summary>
      /// A value of TempAmtCNaCaseNf.
      /// </summary>
      [JsonPropertyName("tempAmtCNaCaseNf")]
      public Common TempAmtCNaCaseNf
      {
        get => tempAmtCNaCaseNf ??= new();
        set => tempAmtCNaCaseNf = value;
      }

      /// <summary>
      /// A value of TempNbrCNaInterSt.
      /// </summary>
      [JsonPropertyName("tempNbrCNaInterSt")]
      public Common TempNbrCNaInterSt
      {
        get => tempNbrCNaInterSt ??= new();
        set => tempNbrCNaInterSt = value;
      }

      /// <summary>
      /// A value of TempAmtCNaInterSt.
      /// </summary>
      [JsonPropertyName("tempAmtCNaInterSt")]
      public Common TempAmtCNaInterSt
      {
        get => tempAmtCNaInterSt ??= new();
        set => tempAmtCNaInterSt = value;
      }

      /// <summary>
      /// A value of TempNbrCNaAllOther.
      /// </summary>
      [JsonPropertyName("tempNbrCNaAllOther")]
      public Common TempNbrCNaAllOther
      {
        get => tempNbrCNaAllOther ??= new();
        set => tempNbrCNaAllOther = value;
      }

      /// <summary>
      /// A value of TempAmtCNaAllOther.
      /// </summary>
      [JsonPropertyName("tempAmtCNaAllOther")]
      public Common TempAmtCNaAllOther
      {
        get => tempAmtCNaAllOther ??= new();
        set => tempAmtCNaAllOther = value;
      }

      /// <summary>
      /// A value of TempNbrCAfKsAr.
      /// </summary>
      [JsonPropertyName("tempNbrCAfKsAr")]
      public Common TempNbrCAfKsAr
      {
        get => tempNbrCAfKsAr ??= new();
        set => tempNbrCAfKsAr = value;
      }

      /// <summary>
      /// A value of TempAmtCAfKsAr.
      /// </summary>
      [JsonPropertyName("tempAmtCAfKsAr")]
      public Common TempAmtCAfKsAr
      {
        get => tempAmtCAfKsAr ??= new();
        set => tempAmtCAfKsAr = value;
      }

      /// <summary>
      /// A value of TempNbrCAfJjAr.
      /// </summary>
      [JsonPropertyName("tempNbrCAfJjAr")]
      public Common TempNbrCAfJjAr
      {
        get => tempNbrCAfJjAr ??= new();
        set => tempNbrCAfJjAr = value;
      }

      /// <summary>
      /// A value of TempAmtCAfJjAr.
      /// </summary>
      [JsonPropertyName("tempAmtCAfJjAr")]
      public Common TempAmtCAfJjAr
      {
        get => tempAmtCAfJjAr ??= new();
        set => tempAmtCAfJjAr = value;
      }

      /// <summary>
      /// A value of TempNbrCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("tempNbrCAfNotDeterm")]
      public Common TempNbrCAfNotDeterm
      {
        get => tempNbrCAfNotDeterm ??= new();
        set => tempNbrCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of TempAmtCAfNotDeter.
      /// </summary>
      [JsonPropertyName("tempAmtCAfNotDeter")]
      public Common TempAmtCAfNotDeter
      {
        get => tempAmtCAfNotDeter ??= new();
        set => tempAmtCAfNotDeter = value;
      }

      /// <summary>
      /// A value of TempNbrCAfCaseNf.
      /// </summary>
      [JsonPropertyName("tempNbrCAfCaseNf")]
      public Common TempNbrCAfCaseNf
      {
        get => tempNbrCAfCaseNf ??= new();
        set => tempNbrCAfCaseNf = value;
      }

      /// <summary>
      /// A value of TempAmtCAfCaseNf.
      /// </summary>
      [JsonPropertyName("tempAmtCAfCaseNf")]
      public Common TempAmtCAfCaseNf
      {
        get => tempAmtCAfCaseNf ??= new();
        set => tempAmtCAfCaseNf = value;
      }

      /// <summary>
      /// A value of TempNbrCAfInterSt.
      /// </summary>
      [JsonPropertyName("tempNbrCAfInterSt")]
      public Common TempNbrCAfInterSt
      {
        get => tempNbrCAfInterSt ??= new();
        set => tempNbrCAfInterSt = value;
      }

      /// <summary>
      /// A value of TempAmtCAfInterSt.
      /// </summary>
      [JsonPropertyName("tempAmtCAfInterSt")]
      public Common TempAmtCAfInterSt
      {
        get => tempAmtCAfInterSt ??= new();
        set => tempAmtCAfInterSt = value;
      }

      /// <summary>
      /// A value of TempNbrCAfAllOther.
      /// </summary>
      [JsonPropertyName("tempNbrCAfAllOther")]
      public Common TempNbrCAfAllOther
      {
        get => tempNbrCAfAllOther ??= new();
        set => tempNbrCAfAllOther = value;
      }

      /// <summary>
      /// A value of TempAmtCAfAllOther.
      /// </summary>
      [JsonPropertyName("tempAmtCAfAllOther")]
      public Common TempAmtCAfAllOther
      {
        get => tempAmtCAfAllOther ??= new();
        set => tempAmtCAfAllOther = value;
      }

      /// <summary>
      /// A value of TempNbrNKsAr.
      /// </summary>
      [JsonPropertyName("tempNbrNKsAr")]
      public Common TempNbrNKsAr
      {
        get => tempNbrNKsAr ??= new();
        set => tempNbrNKsAr = value;
      }

      /// <summary>
      /// A value of TempAmtNKsAr.
      /// </summary>
      [JsonPropertyName("tempAmtNKsAr")]
      public Common TempAmtNKsAr
      {
        get => tempAmtNKsAr ??= new();
        set => tempAmtNKsAr = value;
      }

      /// <summary>
      /// A value of TempNbrNJjAr.
      /// </summary>
      [JsonPropertyName("tempNbrNJjAr")]
      public Common TempNbrNJjAr
      {
        get => tempNbrNJjAr ??= new();
        set => tempNbrNJjAr = value;
      }

      /// <summary>
      /// A value of TempAmtNJjAr.
      /// </summary>
      [JsonPropertyName("tempAmtNJjAr")]
      public Common TempAmtNJjAr
      {
        get => tempAmtNJjAr ??= new();
        set => tempAmtNJjAr = value;
      }

      /// <summary>
      /// A value of TempNbrNNotDeterm.
      /// </summary>
      [JsonPropertyName("tempNbrNNotDeterm")]
      public Common TempNbrNNotDeterm
      {
        get => tempNbrNNotDeterm ??= new();
        set => tempNbrNNotDeterm = value;
      }

      /// <summary>
      /// A value of TempAmtNNotDeterm.
      /// </summary>
      [JsonPropertyName("tempAmtNNotDeterm")]
      public Common TempAmtNNotDeterm
      {
        get => tempAmtNNotDeterm ??= new();
        set => tempAmtNNotDeterm = value;
      }

      /// <summary>
      /// A value of TempNbrNCaseNf.
      /// </summary>
      [JsonPropertyName("tempNbrNCaseNf")]
      public Common TempNbrNCaseNf
      {
        get => tempNbrNCaseNf ??= new();
        set => tempNbrNCaseNf = value;
      }

      /// <summary>
      /// A value of TempAmtNCaseNf.
      /// </summary>
      [JsonPropertyName("tempAmtNCaseNf")]
      public Common TempAmtNCaseNf
      {
        get => tempAmtNCaseNf ??= new();
        set => tempAmtNCaseNf = value;
      }

      /// <summary>
      /// A value of TempNbrNInterSt.
      /// </summary>
      [JsonPropertyName("tempNbrNInterSt")]
      public Common TempNbrNInterSt
      {
        get => tempNbrNInterSt ??= new();
        set => tempNbrNInterSt = value;
      }

      /// <summary>
      /// A value of TempAmtNInterSt.
      /// </summary>
      [JsonPropertyName("tempAmtNInterSt")]
      public Common TempAmtNInterSt
      {
        get => tempAmtNInterSt ??= new();
        set => tempAmtNInterSt = value;
      }

      /// <summary>
      /// A value of TempNbrNAllOther.
      /// </summary>
      [JsonPropertyName("tempNbrNAllOther")]
      public Common TempNbrNAllOther
      {
        get => tempNbrNAllOther ??= new();
        set => tempNbrNAllOther = value;
      }

      /// <summary>
      /// A value of TempAmtNAllOther.
      /// </summary>
      [JsonPropertyName("tempAmtNAllOther")]
      public Common TempAmtNAllOther
      {
        get => tempAmtNAllOther ??= new();
        set => tempAmtNAllOther = value;
      }

      private Common tempNbrOfCollRead;
      private Common tempAmtOfCollRead;
      private Common tempNbrOfAps;
      private Common tempNbrOfCollBackedOff;
      private Common tempAmtOfCollsBackedOff;
      private Common tempNbrOfErrorsCreated;
      private Common tempAmtOfErrorsCreated;
      private Common tempNbrOfCollNotFulApl;
      private Common tempAmtOfCollNotFulApl;
      private Common tempNbrOfCreditsCreated;
      private Common tempAmtOfCreditsCreated;
      private Common tempNbrOfApWoColFulAp;
      private Common tempNbrCNaKsAr;
      private Common tempAmtCNaKsAr;
      private Common tempNbrCNaJjAr;
      private Common tempAmtCNaJjAr;
      private Common tempNbrCNaNotDeterm;
      private Common tempAmtCNaNotDeterm;
      private Common tempNbrCNaCaseNf;
      private Common tempAmtCNaCaseNf;
      private Common tempNbrCNaInterSt;
      private Common tempAmtCNaInterSt;
      private Common tempNbrCNaAllOther;
      private Common tempAmtCNaAllOther;
      private Common tempNbrCAfKsAr;
      private Common tempAmtCAfKsAr;
      private Common tempNbrCAfJjAr;
      private Common tempAmtCAfJjAr;
      private Common tempNbrCAfNotDeterm;
      private Common tempAmtCAfNotDeter;
      private Common tempNbrCAfCaseNf;
      private Common tempAmtCAfCaseNf;
      private Common tempNbrCAfInterSt;
      private Common tempAmtCAfInterSt;
      private Common tempNbrCAfAllOther;
      private Common tempAmtCAfAllOther;
      private Common tempNbrNKsAr;
      private Common tempAmtNKsAr;
      private Common tempNbrNJjAr;
      private Common tempAmtNJjAr;
      private Common tempNbrNNotDeterm;
      private Common tempAmtNNotDeterm;
      private Common tempNbrNCaseNf;
      private Common tempAmtNCaseNf;
      private Common tempNbrNInterSt;
      private Common tempAmtNInterSt;
      private Common tempNbrNAllOther;
      private Common tempAmtNAllOther;
    }

    /// <summary>A FinalTotalsGroup group.</summary>
    [Serializable]
    public class FinalTotalsGroup
    {
      /// <summary>
      /// A value of FinalNbrOfCollRead.
      /// </summary>
      [JsonPropertyName("finalNbrOfCollRead")]
      public Common FinalNbrOfCollRead
      {
        get => finalNbrOfCollRead ??= new();
        set => finalNbrOfCollRead = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCollRead.
      /// </summary>
      [JsonPropertyName("finalAmtOfCollRead")]
      public Common FinalAmtOfCollRead
      {
        get => finalAmtOfCollRead ??= new();
        set => finalAmtOfCollRead = value;
      }

      /// <summary>
      /// A value of FinalNbrOfAps.
      /// </summary>
      [JsonPropertyName("finalNbrOfAps")]
      public Common FinalNbrOfAps
      {
        get => finalNbrOfAps ??= new();
        set => finalNbrOfAps = value;
      }

      /// <summary>
      /// A value of FinalNbrOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("finalNbrOfCollBackedOff")]
      public Common FinalNbrOfCollBackedOff
      {
        get => finalNbrOfCollBackedOff ??= new();
        set => finalNbrOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("finalAmtOfCollBackedOff")]
      public Common FinalAmtOfCollBackedOff
      {
        get => finalAmtOfCollBackedOff ??= new();
        set => finalAmtOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of FinalNbrOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("finalNbrOfErrorsCreated")]
      public Common FinalNbrOfErrorsCreated
      {
        get => finalNbrOfErrorsCreated ??= new();
        set => finalNbrOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of FinalAmtOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("finalAmtOfErrorsCreated")]
      public Common FinalAmtOfErrorsCreated
      {
        get => finalAmtOfErrorsCreated ??= new();
        set => finalAmtOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of FinalNbrOfCollNotFulAp.
      /// </summary>
      [JsonPropertyName("finalNbrOfCollNotFulAp")]
      public Common FinalNbrOfCollNotFulAp
      {
        get => finalNbrOfCollNotFulAp ??= new();
        set => finalNbrOfCollNotFulAp = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCollNotFulAp.
      /// </summary>
      [JsonPropertyName("finalAmtOfCollNotFulAp")]
      public Common FinalAmtOfCollNotFulAp
      {
        get => finalAmtOfCollNotFulAp ??= new();
        set => finalAmtOfCollNotFulAp = value;
      }

      /// <summary>
      /// A value of FinalNbrOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("finalNbrOfCreditsCreated")]
      public Common FinalNbrOfCreditsCreated
      {
        get => finalNbrOfCreditsCreated ??= new();
        set => finalNbrOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of FinalAmtOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("finalAmtOfCreditsCreated")]
      public Common FinalAmtOfCreditsCreated
      {
        get => finalAmtOfCreditsCreated ??= new();
        set => finalAmtOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of FinalNbrOfApWoColFlAp.
      /// </summary>
      [JsonPropertyName("finalNbrOfApWoColFlAp")]
      public Common FinalNbrOfApWoColFlAp
      {
        get => finalNbrOfApWoColFlAp ??= new();
        set => finalNbrOfApWoColFlAp = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaKsAr.
      /// </summary>
      [JsonPropertyName("finalNbrCNaKsAr")]
      public Common FinalNbrCNaKsAr
      {
        get => finalNbrCNaKsAr ??= new();
        set => finalNbrCNaKsAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaKsAr.
      /// </summary>
      [JsonPropertyName("finalAmtCNaKsAr")]
      public Common FinalAmtCNaKsAr
      {
        get => finalAmtCNaKsAr ??= new();
        set => finalAmtCNaKsAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaJjAr.
      /// </summary>
      [JsonPropertyName("finalNbrCNaJjAr")]
      public Common FinalNbrCNaJjAr
      {
        get => finalNbrCNaJjAr ??= new();
        set => finalNbrCNaJjAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaJjAr.
      /// </summary>
      [JsonPropertyName("finalAmtCNaJjAr")]
      public Common FinalAmtCNaJjAr
      {
        get => finalAmtCNaJjAr ??= new();
        set => finalAmtCNaJjAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("finalNbrCNaNotDeterm")]
      public Common FinalNbrCNaNotDeterm
      {
        get => finalNbrCNaNotDeterm ??= new();
        set => finalNbrCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("finalAmtCNaNotDeterm")]
      public Common FinalAmtCNaNotDeterm
      {
        get => finalAmtCNaNotDeterm ??= new();
        set => finalAmtCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaCaseNf.
      /// </summary>
      [JsonPropertyName("finalNbrCNaCaseNf")]
      public Common FinalNbrCNaCaseNf
      {
        get => finalNbrCNaCaseNf ??= new();
        set => finalNbrCNaCaseNf = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaCaseNf.
      /// </summary>
      [JsonPropertyName("finalAmtCNaCaseNf")]
      public Common FinalAmtCNaCaseNf
      {
        get => finalAmtCNaCaseNf ??= new();
        set => finalAmtCNaCaseNf = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaInterSt.
      /// </summary>
      [JsonPropertyName("finalNbrCNaInterSt")]
      public Common FinalNbrCNaInterSt
      {
        get => finalNbrCNaInterSt ??= new();
        set => finalNbrCNaInterSt = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaInterSt.
      /// </summary>
      [JsonPropertyName("finalAmtCNaInterSt")]
      public Common FinalAmtCNaInterSt
      {
        get => finalAmtCNaInterSt ??= new();
        set => finalAmtCNaInterSt = value;
      }

      /// <summary>
      /// A value of FinalNbrCNaAllOther.
      /// </summary>
      [JsonPropertyName("finalNbrCNaAllOther")]
      public Common FinalNbrCNaAllOther
      {
        get => finalNbrCNaAllOther ??= new();
        set => finalNbrCNaAllOther = value;
      }

      /// <summary>
      /// A value of FinalAmtCNaAllOther.
      /// </summary>
      [JsonPropertyName("finalAmtCNaAllOther")]
      public Common FinalAmtCNaAllOther
      {
        get => finalAmtCNaAllOther ??= new();
        set => finalAmtCNaAllOther = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfKsAr.
      /// </summary>
      [JsonPropertyName("finalNbrCAfKsAr")]
      public Common FinalNbrCAfKsAr
      {
        get => finalNbrCAfKsAr ??= new();
        set => finalNbrCAfKsAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfKsAr.
      /// </summary>
      [JsonPropertyName("finalAmtCAfKsAr")]
      public Common FinalAmtCAfKsAr
      {
        get => finalAmtCAfKsAr ??= new();
        set => finalAmtCAfKsAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfJjAr.
      /// </summary>
      [JsonPropertyName("finalNbrCAfJjAr")]
      public Common FinalNbrCAfJjAr
      {
        get => finalNbrCAfJjAr ??= new();
        set => finalNbrCAfJjAr = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfJjAr.
      /// </summary>
      [JsonPropertyName("finalAmtCAfJjAr")]
      public Common FinalAmtCAfJjAr
      {
        get => finalAmtCAfJjAr ??= new();
        set => finalAmtCAfJjAr = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("finalNbrCAfNotDeterm")]
      public Common FinalNbrCAfNotDeterm
      {
        get => finalNbrCAfNotDeterm ??= new();
        set => finalNbrCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("finalAmtCAfNotDeterm")]
      public Common FinalAmtCAfNotDeterm
      {
        get => finalAmtCAfNotDeterm ??= new();
        set => finalAmtCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfCaseNf.
      /// </summary>
      [JsonPropertyName("finalNbrCAfCaseNf")]
      public Common FinalNbrCAfCaseNf
      {
        get => finalNbrCAfCaseNf ??= new();
        set => finalNbrCAfCaseNf = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfCaseNf.
      /// </summary>
      [JsonPropertyName("finalAmtCAfCaseNf")]
      public Common FinalAmtCAfCaseNf
      {
        get => finalAmtCAfCaseNf ??= new();
        set => finalAmtCAfCaseNf = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfInterSt.
      /// </summary>
      [JsonPropertyName("finalNbrCAfInterSt")]
      public Common FinalNbrCAfInterSt
      {
        get => finalNbrCAfInterSt ??= new();
        set => finalNbrCAfInterSt = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfInterSt.
      /// </summary>
      [JsonPropertyName("finalAmtCAfInterSt")]
      public Common FinalAmtCAfInterSt
      {
        get => finalAmtCAfInterSt ??= new();
        set => finalAmtCAfInterSt = value;
      }

      /// <summary>
      /// A value of FinalNbrCAfAllOther.
      /// </summary>
      [JsonPropertyName("finalNbrCAfAllOther")]
      public Common FinalNbrCAfAllOther
      {
        get => finalNbrCAfAllOther ??= new();
        set => finalNbrCAfAllOther = value;
      }

      /// <summary>
      /// A value of FinalAmtCAfAllOther.
      /// </summary>
      [JsonPropertyName("finalAmtCAfAllOther")]
      public Common FinalAmtCAfAllOther
      {
        get => finalAmtCAfAllOther ??= new();
        set => finalAmtCAfAllOther = value;
      }

      /// <summary>
      /// A value of FinalNbrNKsAr.
      /// </summary>
      [JsonPropertyName("finalNbrNKsAr")]
      public Common FinalNbrNKsAr
      {
        get => finalNbrNKsAr ??= new();
        set => finalNbrNKsAr = value;
      }

      /// <summary>
      /// A value of FinalAmtNKsAr.
      /// </summary>
      [JsonPropertyName("finalAmtNKsAr")]
      public Common FinalAmtNKsAr
      {
        get => finalAmtNKsAr ??= new();
        set => finalAmtNKsAr = value;
      }

      /// <summary>
      /// A value of FinalNbrNJjAr.
      /// </summary>
      [JsonPropertyName("finalNbrNJjAr")]
      public Common FinalNbrNJjAr
      {
        get => finalNbrNJjAr ??= new();
        set => finalNbrNJjAr = value;
      }

      /// <summary>
      /// A value of FinalAmtNJjAr.
      /// </summary>
      [JsonPropertyName("finalAmtNJjAr")]
      public Common FinalAmtNJjAr
      {
        get => finalAmtNJjAr ??= new();
        set => finalAmtNJjAr = value;
      }

      /// <summary>
      /// A value of FinalNbrNNotDeterm.
      /// </summary>
      [JsonPropertyName("finalNbrNNotDeterm")]
      public Common FinalNbrNNotDeterm
      {
        get => finalNbrNNotDeterm ??= new();
        set => finalNbrNNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalAmtNNotDeterm.
      /// </summary>
      [JsonPropertyName("finalAmtNNotDeterm")]
      public Common FinalAmtNNotDeterm
      {
        get => finalAmtNNotDeterm ??= new();
        set => finalAmtNNotDeterm = value;
      }

      /// <summary>
      /// A value of FinalNbrNCaseNf.
      /// </summary>
      [JsonPropertyName("finalNbrNCaseNf")]
      public Common FinalNbrNCaseNf
      {
        get => finalNbrNCaseNf ??= new();
        set => finalNbrNCaseNf = value;
      }

      /// <summary>
      /// A value of FinalAmtNCaseNf.
      /// </summary>
      [JsonPropertyName("finalAmtNCaseNf")]
      public Common FinalAmtNCaseNf
      {
        get => finalAmtNCaseNf ??= new();
        set => finalAmtNCaseNf = value;
      }

      /// <summary>
      /// A value of FinalNbrNInterSt.
      /// </summary>
      [JsonPropertyName("finalNbrNInterSt")]
      public Common FinalNbrNInterSt
      {
        get => finalNbrNInterSt ??= new();
        set => finalNbrNInterSt = value;
      }

      /// <summary>
      /// A value of FinalAmtNInterSt.
      /// </summary>
      [JsonPropertyName("finalAmtNInterSt")]
      public Common FinalAmtNInterSt
      {
        get => finalAmtNInterSt ??= new();
        set => finalAmtNInterSt = value;
      }

      /// <summary>
      /// A value of FinalNbrNAllOther.
      /// </summary>
      [JsonPropertyName("finalNbrNAllOther")]
      public Common FinalNbrNAllOther
      {
        get => finalNbrNAllOther ??= new();
        set => finalNbrNAllOther = value;
      }

      /// <summary>
      /// A value of FinalAmtNAllOther.
      /// </summary>
      [JsonPropertyName("finalAmtNAllOther")]
      public Common FinalAmtNAllOther
      {
        get => finalAmtNAllOther ??= new();
        set => finalAmtNAllOther = value;
      }

      private Common finalNbrOfCollRead;
      private Common finalAmtOfCollRead;
      private Common finalNbrOfAps;
      private Common finalNbrOfCollBackedOff;
      private Common finalAmtOfCollBackedOff;
      private Common finalNbrOfErrorsCreated;
      private Common finalAmtOfErrorsCreated;
      private Common finalNbrOfCollNotFulAp;
      private Common finalAmtOfCollNotFulAp;
      private Common finalNbrOfCreditsCreated;
      private Common finalAmtOfCreditsCreated;
      private Common finalNbrOfApWoColFlAp;
      private Common finalNbrCNaKsAr;
      private Common finalAmtCNaKsAr;
      private Common finalNbrCNaJjAr;
      private Common finalAmtCNaJjAr;
      private Common finalNbrCNaNotDeterm;
      private Common finalAmtCNaNotDeterm;
      private Common finalNbrCNaCaseNf;
      private Common finalAmtCNaCaseNf;
      private Common finalNbrCNaInterSt;
      private Common finalAmtCNaInterSt;
      private Common finalNbrCNaAllOther;
      private Common finalAmtCNaAllOther;
      private Common finalNbrCAfKsAr;
      private Common finalAmtCAfKsAr;
      private Common finalNbrCAfJjAr;
      private Common finalAmtCAfJjAr;
      private Common finalNbrCAfNotDeterm;
      private Common finalAmtCAfNotDeterm;
      private Common finalNbrCAfCaseNf;
      private Common finalAmtCAfCaseNf;
      private Common finalNbrCAfInterSt;
      private Common finalAmtCAfInterSt;
      private Common finalNbrCAfAllOther;
      private Common finalAmtCAfAllOther;
      private Common finalNbrNKsAr;
      private Common finalAmtNKsAr;
      private Common finalNbrNJjAr;
      private Common finalAmtNJjAr;
      private Common finalNbrNNotDeterm;
      private Common finalAmtNNotDeterm;
      private Common finalNbrNCaseNf;
      private Common finalAmtNCaseNf;
      private Common finalNbrNInterSt;
      private Common finalAmtNInterSt;
      private Common finalNbrNAllOther;
      private Common finalAmtNAllOther;
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Collection Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of DispObligor.
    /// </summary>
    [JsonPropertyName("dispObligor")]
    public CsePerson DispObligor
    {
      get => dispObligor ??= new();
      set => dispObligor = value;
    }

    /// <summary>
    /// A value of CollsElligibleForRcvCnt.
    /// </summary>
    [JsonPropertyName("collsElligibleForRcvCnt")]
    public Common CollsElligibleForRcvCnt
    {
      get => collsElligibleForRcvCnt ??= new();
      set => collsElligibleForRcvCnt = value;
    }

    /// <summary>
    /// A value of DecreaseApApplyRcvCnt.
    /// </summary>
    [JsonPropertyName("decreaseApApplyRcvCnt")]
    public Common DecreaseApApplyRcvCnt
    {
      get => decreaseApApplyRcvCnt ??= new();
      set => decreaseApApplyRcvCnt = value;
    }

    /// <summary>
    /// A value of ApsWithColToApplyToRcv.
    /// </summary>
    [JsonPropertyName("apsWithColToApplyToRcv")]
    public ControlTable ApsWithColToApplyToRcv
    {
      get => apsWithColToApplyToRcv ??= new();
      set => apsWithColToApplyToRcv = value;
    }

    /// <summary>
    /// A value of ApplyApCollToRcvInd.
    /// </summary>
    [JsonPropertyName("applyApCollToRcvInd")]
    public Common ApplyApCollToRcvInd
    {
      get => applyApCollToRcvInd ??= new();
      set => applyApCollToRcvInd = value;
    }

    /// <summary>
    /// A value of AppliedApCollToRcvInd.
    /// </summary>
    [JsonPropertyName("appliedApCollToRcvInd")]
    public Common AppliedApCollToRcvInd
    {
      get => appliedApCollToRcvInd ??= new();
      set => appliedApCollToRcvInd = value;
    }

    /// <summary>
    /// A value of HoldObligor.
    /// </summary>
    [JsonPropertyName("holdObligor")]
    public CsePerson HoldObligor
    {
      get => holdObligor ??= new();
      set => holdObligor = value;
    }

    /// <summary>
    /// A value of CollectionSign.
    /// </summary>
    [JsonPropertyName("collectionSign")]
    public TextWorkArea CollectionSign
    {
      get => collectionSign ??= new();
      set => collectionSign = value;
    }

    /// <summary>
    /// A value of LolFormatted82AmtField1.
    /// </summary>
    [JsonPropertyName("lolFormatted82AmtField1")]
    public WorkArea LolFormatted82AmtField1
    {
      get => lolFormatted82AmtField1 ??= new();
      set => lolFormatted82AmtField1 = value;
    }

    /// <summary>
    /// A value of Formatted82AmtField2.
    /// </summary>
    [JsonPropertyName("formatted82AmtField2")]
    public WorkArea Formatted82AmtField2
    {
      get => formatted82AmtField2 ??= new();
      set => formatted82AmtField2 = value;
    }

    /// <summary>
    /// A value of Formatted82AmtField3.
    /// </summary>
    [JsonPropertyName("formatted82AmtField3")]
    public WorkArea Formatted82AmtField3
    {
      get => formatted82AmtField3 ??= new();
      set => formatted82AmtField3 = value;
    }

    /// <summary>
    /// A value of HoldCollNotFullyAppAp.
    /// </summary>
    [JsonPropertyName("holdCollNotFullyAppAp")]
    public CsePerson HoldCollNotFullyAppAp
    {
      get => holdCollNotFullyAppAp ??= new();
      set => holdCollNotFullyAppAp = value;
    }

    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public TextWorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of UnformattedDate.
    /// </summary>
    [JsonPropertyName("unformattedDate")]
    public DateWorkArea UnformattedDate
    {
      get => unformattedDate ??= new();
      set => unformattedDate = value;
    }

    /// <summary>
    /// A value of Loc112AmountField.
    /// </summary>
    [JsonPropertyName("loc112AmountField")]
    public NumericWorkSet Loc112AmountField
    {
      get => loc112AmountField ??= new();
      set => loc112AmountField = value;
    }

    /// <summary>
    /// A value of UndistributedAmt.
    /// </summary>
    [JsonPropertyName("undistributedAmt")]
    public WorkArea UndistributedAmt
    {
      get => undistributedAmt ??= new();
      set => undistributedAmt = value;
    }

    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of CashReceiptNbrFormatted.
    /// </summary>
    [JsonPropertyName("cashReceiptNbrFormatted")]
    public WorkArea CashReceiptNbrFormatted
    {
      get => cashReceiptNbrFormatted ??= new();
      set => cashReceiptNbrFormatted = value;
    }

    /// <summary>
    /// A value of StateCollectionInd.
    /// </summary>
    [JsonPropertyName("stateCollectionInd")]
    public Common StateCollectionInd
    {
      get => stateCollectionInd ??= new();
      set => stateCollectionInd = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of FormattedAmount.
    /// </summary>
    [JsonPropertyName("formattedAmount")]
    public TextWorkArea FormattedAmount
    {
      get => formattedAmount ??= new();
      set => formattedAmount = value;
    }

    /// <summary>
    /// A value of TestRunInd.
    /// </summary>
    [JsonPropertyName("testRunInd")]
    public Common TestRunInd
    {
      get => testRunInd ??= new();
      set => testRunInd = value;
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
    /// A value of TestFirstObligor.
    /// </summary>
    [JsonPropertyName("testFirstObligor")]
    public CsePerson TestFirstObligor
    {
      get => testFirstObligor ??= new();
      set => testFirstObligor = value;
    }

    /// <summary>
    /// A value of TestLastObligor.
    /// </summary>
    [JsonPropertyName("testLastObligor")]
    public CsePerson TestLastObligor
    {
      get => testLastObligor ??= new();
      set => testLastObligor = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public Collection ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// Gets a value of HardcodePass.
    /// </summary>
    [JsonPropertyName("hardcodePass")]
    public HardcodePassGroup HardcodePass
    {
      get => hardcodePass ?? (hardcodePass = new());
      set => hardcodePass = value;
    }

    /// <summary>
    /// Gets a value of TempTotals.
    /// </summary>
    [JsonPropertyName("tempTotals")]
    public TempTotalsGroup TempTotals
    {
      get => tempTotals ?? (tempTotals = new());
      set => tempTotals = value;
    }

    /// <summary>
    /// Gets a value of FinalTotals.
    /// </summary>
    [JsonPropertyName("finalTotals")]
    public FinalTotalsGroup FinalTotals
    {
      get => finalTotals ?? (finalTotals = new());
      set => finalTotals = value;
    }

    private Collection test;
    private CsePerson dispObligor;
    private Common collsElligibleForRcvCnt;
    private Common decreaseApApplyRcvCnt;
    private ControlTable apsWithColToApplyToRcv;
    private Common applyApCollToRcvInd;
    private Common appliedApCollToRcvInd;
    private CsePerson holdObligor;
    private TextWorkArea collectionSign;
    private WorkArea lolFormatted82AmtField1;
    private WorkArea formatted82AmtField2;
    private WorkArea formatted82AmtField3;
    private CsePerson holdCollNotFullyAppAp;
    private TextWorkArea formattedDate;
    private DateWorkArea unformattedDate;
    private NumericWorkSet loc112AmountField;
    private WorkArea undistributedAmt;
    private CrdCrComboNo crdCrComboNo;
    private WorkArea cashReceiptNbrFormatted;
    private Common stateCollectionInd;
    private DebtDetail debtDetail;
    private Case1 case1;
    private CsePerson obligee;
    private CsePerson child;
    private TextWorkArea formattedAmount;
    private Common testRunInd;
    private Common displayInd;
    private CsePerson testFirstObligor;
    private CsePerson testLastObligor;
    private Collection forUpdate;
    private Common abort;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea initialized;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private HardcodePassGroup hardcodePass;
    private TempTotalsGroup tempTotals;
    private FinalTotalsGroup finalTotals;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReadForAdjustments.
    /// </summary>
    [JsonPropertyName("readForAdjustments")]
    public Collection ReadForAdjustments
    {
      get => readForAdjustments ??= new();
      set => readForAdjustments = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    private Collection readForAdjustments;
    private Collection collection;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligor1;
    private CsePerson obligor2;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private DisbursementTransaction disbursementTransaction;
  }
#endregion
}
