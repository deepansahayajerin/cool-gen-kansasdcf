// Program: FN_BFXF_APPLY_COLLECTIONS_TO_HH, ID: 374485526, model: 746.
// Short name: SWEFFXFB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXF_APPLY_COLLECTIONS_TO_HH.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxfApplyCollectionsToHh: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXF_APPLY_COLLECTIONS_TO_HH program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxfApplyCollectionsToHh(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxfApplyCollectionsToHh.
  /// </summary>
  public FnBfxfApplyCollectionsToHh(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Initial Version :- SWSRKXD 06/18/2000
    // This utility will apply collections to Member mnthly summaries.
    // 08/03/00 - Change sort criteria to add Due_dt and Ob_trn_Id.
    // Tweak restart logic to accomodate this change.
    // ----------------------------------------------------------------
    // -------------------------------------------------------
    // 08/06/00
    // Unmap EV Cash_receipt_detail in the call to
    // fn_determine_ura_for_supp_pers.
    // 08/16/00
    // Supply legal action instead.
    // -------------------------------------------------------
    // ---------------------------------------------------------
    // 08/17/00
    // Set MC and MJ hardcoded values and their usage.
    // Supply collection_date attribute view to build_hh routine.
    // Set level to M when creating adj.
    // ---------------------------------------------------------
    // --------------------------------------------------------
    // 08/18/00
    // Remove classification = M to check for medical.
    // Created Adj for oldest yr/month of Supp Person.
    // --------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEFBFXF";
    UseFnBfxfBatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.RestartCollection.CollectionDt =
        IntToDate((int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 8)));
      local.RestartCashReceiptDetail.ObligorPersonNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 9, 10);
      local.RestartCashReceipt.SequentialNumber =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 19, 9));
      local.RestartCashReceiptDetail.SequentialIdentifier =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 28, 4));
      local.EabReportSend.RptDetail =
        "SWEFBFXF will be restarted at Coll Dt: " + NumberToString
        (DateToInt(local.RestartCollection.CollectionDt), 8, 8);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "; Obligor #:";
        
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + (
        local.RestartCashReceiptDetail.ObligorPersonNumber ?? "");
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "; CR #:";
        
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (local.RestartCashReceipt.SequentialNumber, 7, 9);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "; CRD #:";
        
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (local.RestartCashReceiptDetail.SequentialIdentifier, 12, 4);
      UseCabControlReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.HardcodedMs.SystemGeneratedIdentifier = 3;
    local.HardcodedMc.SystemGeneratedIdentifier = 19;
    local.HardcodedMj.SystemGeneratedIdentifier = 10;

    // *********************
    // Main Processing
    // *********************
    // -----------------------------------------------------
    // 08/03/00 - Add due_dt and ob_trn_id to the sort criteria
    // -----------------------------------------------------
    foreach(var item in ReadCollectionCashReceiptDetailCashReceiptObligationTransaction())
      
    {
      if (Equal(entities.Persistent.CollectionDt,
        local.RestartCollection.CollectionDt))
      {
        if (Lt(entities.CashReceiptDetail.ObligorPersonNumber,
          local.RestartCashReceiptDetail.ObligorPersonNumber))
        {
          continue;
        }
        else if (Equal(entities.CashReceiptDetail.ObligorPersonNumber,
          local.RestartCashReceiptDetail.ObligorPersonNumber))
        {
          if (entities.CashReceipt.SequentialNumber < local
            .RestartCashReceipt.SequentialNumber)
          {
            continue;
          }
          else if (entities.CashReceipt.SequentialNumber == local
            .RestartCashReceipt.SequentialNumber)
          {
            if (entities.CashReceiptDetail.SequentialIdentifier < local
              .RestartCashReceiptDetail.SequentialIdentifier)
            {
              continue;
            }
            else if (entities.CashReceiptDetail.SequentialIdentifier == local
              .RestartCashReceiptDetail.SequentialIdentifier)
            {
              // ----------------------------------------------------
              // Ensure a Collection is not applied more than once.
              // ----------------------------------------------------
              if (ReadUraCollectionApplication())
              {
                // -----------------------------------------------------
                // Collection has already been processed. Skip this one.
                // -----------------------------------------------------
                continue;
              }
            }
          }
        }
      }

      if (local.TestCollection.CollectionDt != null)
      {
        if (Lt(entities.Persistent.CollectionDt,
          local.TestCollection.CollectionDt))
        {
          continue;
        }

        if (Lt(local.TestCollection.CollectionDt,
          entities.Persistent.CollectionDt))
        {
          break;
        }
      }

      if (!IsEmpty(local.TestCashReceiptDetail.ObligorPersonNumber))
      {
        if (!Equal(entities.CashReceiptDetail.ObligorPersonNumber,
          local.TestCashReceiptDetail.ObligorPersonNumber))
        {
          continue;
        }
      }

      ++local.CollectionRecordsRead.Count;
      local.CollectionRecordsRead.TotalCurrency += entities.Persistent.Amount;

      if (IsEmpty(local.DummyLoop.Flag))
      {
        if (entities.ObligationType.SystemGeneratedIdentifier == local
          .HardcodedMj.SystemGeneratedIdentifier || entities
          .ObligationType.SystemGeneratedIdentifier == local
          .HardcodedMc.SystemGeneratedIdentifier || entities
          .ObligationType.SystemGeneratedIdentifier == local
          .HardcodedMs.SystemGeneratedIdentifier)
        {
          local.MedicalObligationType.Flag = "Y";
        }
        else
        {
          local.MedicalObligationType.Flag = "N";
        }

        UseFnBfxfBuildHouseholdHistTbl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test2;
        }

        if (local.HhHist.IsEmpty)
        {
          ExitState = "OE0000_HIST_GRP_VIEW_IS_EMPTY";

          goto Test2;
        }

        local.CollectionDate.Date = entities.Persistent.CollectionDt;
        local.LegalAction.StandardNumber =
          entities.Persistent.CourtOrderAppliedTo;

        // ------------------------------
        // 08/06/00
        // Unmap EV Cash_receipt_detail.
        // 08/16/00
        // Map legal action instead.
        // ------------------------------
        UseFnDetermineUraForSuppPrsn();

        // ------------------------------
        // CAB does not set any Exit State
        // ------------------------------
        if (local.UraMedicalAmount.TotalCurrency >= entities
          .Persistent.Amount && AsChar(local.MedicalObligationType.Flag) == 'Y'
          || local.UraAmount.TotalCurrency >= entities.Persistent.Amount && AsChar
          (local.MedicalObligationType.Flag) == 'N')
        {
        }
        else
        {
          // ------------------------------------------------------------
          // Collection amt exceeds URA, need to up URA & create Adj.
          // Read summ for supp person's oldest year/month.
          // ------------------------------------------------------------
          local.HhHist.Index = 0;

          for(var limit = local.HhHist.Count; local.HhHist.Index < limit; ++
            local.HhHist.Index)
          {
            if (!local.HhHist.CheckSize())
            {
              break;
            }

            if (!Equal(local.HhHist.Item.HhHistSuppPrsn.Number,
              entities.Supported.Number))
            {
              continue;
            }

            if (local.HhHist.Item.HhHistDtl.IsEmpty)
            {
              // ----------------------------------------------------------------
              // error
              // ----------------------------------------------------------------
              ExitState = "OE0000_HH_MSUM_NF_FOR_SUPP_PERS";

              goto Test2;
            }

            for(local.HhHist.Item.HhHistDtl.Index = 0; local
              .HhHist.Item.HhHistDtl.Index < local.HhHist.Item.HhHistDtl.Count; ++
              local.HhHist.Item.HhHistDtl.Index)
            {
              if (!local.HhHist.Item.HhHistDtl.CheckSize())
              {
                break;
              }

              // ----------------------------------------------------------------
              // Hist Dtl GV is sorted in asc order. Oldest first. If the very 
              // first
              // record we hit is past coll date yr/mth, it's an error.
              // ----------------------------------------------------------------
              if (local.HhHist.Item.HhHistDtl.Item.
                HhHistDtlImHouseholdMbrMnthlySum.Year > Year
                (entities.Persistent.CollectionDt) || local
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                  Year == Year(entities.Persistent.CollectionDt) && local
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
                  Month > Month(entities.Persistent.CollectionDt))
              {
                ExitState = "OE0000_MTH_SUMM_NF_FOR_SUPP_PERS";

                goto Test2;
              }

              if (!ReadImHouseholdMbrMnthlySum())
              {
                ExitState = "FN0000_IM_HH_MBR_MTHLY_SUM_NF_RB";

                goto Test2;
              }

              MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum,
                local.ForUpdate);

              if (AsChar(local.MedicalObligationType.Flag) == 'Y')
              {
                local.ForAdd.Type1 = "M";
                local.ForAdd.AdjustmentAmount = entities.Persistent.Amount - local
                  .UraMedicalAmount.TotalCurrency;
                local.HhHist.Update.HhHistDtl.Update.
                  HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount =
                    local.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                    GetValueOrDefault() + local.ForAdd.AdjustmentAmount;
                local.ForUpdate.UraMedicalAmount =
                  local.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraMedicalAmount.
                    GetValueOrDefault();
              }
              else
              {
                local.ForAdd.Type1 = "A";
                local.ForAdd.AdjustmentAmount = entities.Persistent.Amount - local
                  .UraAmount.TotalCurrency;
                local.HhHist.Update.HhHistDtl.Update.
                  HhHistDtlImHouseholdMbrMnthlySum.UraAmount =
                    local.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                    GetValueOrDefault() + local.ForAdd.AdjustmentAmount;
                local.ForUpdate.UraAmount =
                  local.HhHist.Item.HhHistDtl.Item.
                    HhHistDtlImHouseholdMbrMnthlySum.UraAmount.
                    GetValueOrDefault();
              }

              // --------------------------------
              // Update URA to reflect adjustment.
              // --------------------------------
              try
              {
                UpdateImHouseholdMbrMnthlySum();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_IM_HH_MBR_MNTH_SUM_PV_RB";

                    goto Test2;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              // -----------------------------
              // Create adjustment.
              // ----------------------------
              try
              {
                CreateImHouseholdMbrMnthlyAdj();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    // ----------------------------------------------------------
                    // Should never happen. Primary key includes created_ts.
                    // ----------------------------------------------------------
                    ExitState = "FN0000_IM_HOUSEHOLD_MMBR_ADJ_AE";

                    goto Test2;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              ++local.CreatedAdjustments.Count;
              local.CreatedAdjustments.TotalCurrency += local.ForAdd.
                AdjustmentAmount;

              // -------------------------------------------------
              // Write this record to the control report.
              // -------------------------------------------------
              local.EabReportSend.RptDetail = "Adj created for AE case #:" + local
                .HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.AeCaseNo + "; ";
                
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "CSE Person #:";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + entities
                .Supported.Number;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "; Year:";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                (entities.ImHouseholdMbrMnthlySum.Year, 12, 4);
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "; Month:";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                (entities.ImHouseholdMbrMnthlySum.Month, 14, 2);
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "; URA Amt: $";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                ((long)(entities.ImHouseholdMbrMnthlySum.UraAmount.
                  GetValueOrDefault() * 100), 5, 11);
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "; URA Med Amt: $";
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                ((long)entities.ImHouseholdMbrMnthlySum.UraMedicalAmount.
                  GetValueOrDefault(), 7, 9);
              UseCabControlReport2();

              if (!Equal(local.Status.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              goto Test1;
            }

            local.HhHist.Item.HhHistDtl.CheckIndex();
          }

          local.HhHist.CheckIndex();

          // ----------------------------------------------------------
          // Can only get here in error. No Adj was created.
          // ----------------------------------------------------------
          ExitState = "OE0000_SUPP_PERS_NF_URA_ADJ_NC";

          goto Test2;
        }

Test1:

        UseFnApplyCollectionToUra();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test2;
        }

        ++local.CollectionsApplied.Count;
        local.CollectionsApplied.TotalCurrency += entities.Persistent.Amount;
      }

Test2:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.ErroredCollectionRecords.Count;
        local.ErroredCollectionRecords.TotalCurrency += entities.Persistent.
          Amount;
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.EabReportSend.RptDetail = "Coll Dt:" + NumberToString
          (DateToInt(entities.Persistent.CollectionDt), 15);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Obligor #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .CashReceiptDetail.ObligorPersonNumber;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; CR #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.CashReceipt.SequentialNumber, 7, 9);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; CRD #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Coll Id:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.Persistent.SystemGeneratedIdentifier, 7, 9);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Coll Amt:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          ((long)(entities.Persistent.Amount * 100), 5, 11);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // -----------------------------------------------------
        // Check if commit count has been reached.
        // -----------------------------------------------------
      }
      else if (local.CommitCnt.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() && AsChar
        (local.TestRun.Flag) != 'Y')
      {
        local.CommitCnt.Count = 0;

        if (!ReadProgramCheckpointRestart())
        {
          ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

          return;
        }

        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(DateToInt(entities.Persistent.CollectionDt), 8, 8);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + entities
          .CashReceiptDetail.ObligorPersonNumber;
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceipt.SequentialNumber, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);

        try
        {
          UpdateProgramCheckpointRestart1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PROGRAM_CHECKPOINT_RESTART_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PROGRAM_CHECKPOINT_RESTART_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        UseExtToDoACommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }

        ++local.NbrOfCheckpoint.Count;
        local.EabReportSend.RptDetail = "Checkpoint #: " + NumberToString
          (local.NbrOfCheckpoint.Count, 10, 6);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Time: ";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (TimeToInt(TimeOfDay(Now())), 10, 6);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Coll Date:" + NumberToString
          (DateToInt(entities.Persistent.CollectionDt), 8, 8);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Obligor #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .CashReceiptDetail.ObligorPersonNumber;
        UseCabControlReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabControlReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (AsChar(local.DisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "*****Coll Date:" + NumberToString
          (DateToInt(entities.Persistent.CollectionDt), 8, 8);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Obligor #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .CashReceiptDetail.ObligorPersonNumber;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; CR #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.CashReceipt.SequentialNumber, 7, 9);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; CRD #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Coll Id:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (entities.Persistent.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "Ob Type:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities
          .ObligationType.Code;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; DDDD:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          (DateToInt(entities.DebtDetail.DueDt), 8, 8);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Supp person #:";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + entities.Supported.Number;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; URA:$";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          ((long)(local.UraAmount.TotalCurrency * 100), 15);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "; Med URA:$";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + NumberToString
          ((long)(local.UraMedicalAmount.TotalCurrency * 100), 15);
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabErrorReport2();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      ++local.CommitCnt.Count;
    }

    if (AsChar(local.TestRun.Flag) == 'Y')
    {
      ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    // *********************
    // Write control totals
    // *********************
    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of Collections read :- " + NumberToString
      (local.CollectionRecordsRead.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Total Amount of collections read :- " + NumberToString
      ((long)(local.CollectionRecordsRead.TotalCurrency * 100), 1, 15);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of records in error :- " + NumberToString
      (local.ErroredCollectionRecords.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Amount of errored collections :- " + NumberToString
      ((long)(local.ErroredCollectionRecords.TotalCurrency * 100), 1, 15);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of Adj records created :- " + NumberToString
      (local.CreatedAdjustments.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Amount of Adj records created :- " + NumberToString
      ((long)(local.CreatedAdjustments.TotalCurrency * 100), 1, 15);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of Collections applied :- " + NumberToString
      (local.CollectionsApplied.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total amount of Collections applied :- " + NumberToString
      ((long)(local.CollectionsApplied.TotalCurrency * 100), 1, 15);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of checkpoints taken :- " + NumberToString
      (local.NbrOfCheckpoint.Count, 7, 9);
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *********************
    // Reset restart flag
    // *********************
    if (!ReadProgramCheckpointRestart())
    {
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF_AB";

      return;
    }

    try
    {
      UpdateProgramCheckpointRestart2();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PROGRAM_CHECKPOINT_RESTART_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PROGRAM_CHECKPOINT_RESTART_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ****************
    // Close the files
    // ****************
    local.EabFileHandling.Action = "CLOSE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();
    UseCabErrorReport1();
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionDate = source.CollectionDate;
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
    target.DistributionMethod = source.DistributionMethod;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.AppliedToOrderTypeCode = source.AppliedToOrderTypeCode;
    target.ManualDistributionReasonText = source.ManualDistributionReasonText;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
    target.CourtNoticeReqInd = source.CourtNoticeReqInd;
    target.CourtNoticeProcessedDate = source.CourtNoticeProcessedDate;
    target.DisburseToArInd = source.DisburseToArInd;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
    target.AppliedToFuture = source.AppliedToFuture;
    target.CsenetOutboundReqInd = source.CsenetOutboundReqInd;
    target.CsenetOutboundAdjProjDt = source.CsenetOutboundAdjProjDt;
    target.CourtNoticeAdjProcessDate = source.CourtNoticeAdjProcessDate;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveHhHist1(Local.HhHistGroup source,
    FnApplyCollectionToUra.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(Local.HhHistGroup source,
    FnDetermineUraForSuppPrsn.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl2);
  }

  private static void MoveHhHist3(FnApplyCollectionToUra.Import.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl3);
  }

  private static void MoveHhHist4(FnBfxfBuildHouseholdHistTbl.Export.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl4);
  }

  private static void MoveHhHistDtl1(Local.HhHistDtlGroup source,
    FnApplyCollectionToUra.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(Local.HhHistDtlGroup source,
    FnDetermineUraForSuppPrsn.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl3(FnApplyCollectionToUra.Import.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl4(FnBfxfBuildHouseholdHistTbl.Export.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.UraAmount = source.UraAmount;
    target.UraMedicalAmount = source.UraMedicalAmount;
  }

  private static void MoveLegal1(Local.LegalGroup source,
    FnApplyCollectionToUra.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl1);
  }

  private static void MoveLegal2(Local.LegalGroup source,
    FnDetermineUraForSuppPrsn.Import.LegalGroup target)
  {
    target.LegalSuppPrsn1.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl2);
  }

  private static void MoveLegal3(FnBfxfBuildHouseholdHistTbl.Export.
    LegalGroup source, Local.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl3);
  }

  private static void MoveLegalDtl1(Local.LegalDtlGroup source,
    FnApplyCollectionToUra.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl2(Local.LegalDtlGroup source,
    FnDetermineUraForSuppPrsn.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl3(FnBfxfBuildHouseholdHistTbl.Export.
    LegalDtlGroup source, Local.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnApplyCollectionToUra()
  {
    var useImport = new FnApplyCollectionToUra.Import();
    var useExport = new FnApplyCollectionToUra.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.Collection1.Date = local.CollectionDate.Date;
    MoveCollection2(entities.Persistent, useImport.Collection2);
    useImport.SuppPrsn.Number = entities.Supported.Number;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    local.Legal.CopyTo(useImport.Legal, MoveLegal1);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist1);
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMs.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMj.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMc.SystemGeneratedIdentifier;
    useImport.PersistantDelMe.Assign(entities.Persistent);

    Call(FnApplyCollectionToUra.Execute, useImport, useExport);

    useImport.HhHist.CopyTo(local.HhHist, MoveHhHist3);
    MoveCollection1(useImport.PersistantDelMe, entities.Persistent);
  }

  private void UseFnBfxfBatchInitialization()
  {
    var useImport = new FnBfxfBatchInitialization.Import();
    var useExport = new FnBfxfBatchInitialization.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(FnBfxfBatchInitialization.Execute, useImport, useExport);

    local.TestCashReceiptDetail.ObligorPersonNumber =
      useExport.TestCashReceiptDetail.ObligorPersonNumber;
    local.TestCollection.CollectionDt = useExport.TestCollection.CollectionDt;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.TestRun.Flag = useExport.TestRunInd.Flag;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
  }

  private void UseFnBfxfBuildHouseholdHistTbl()
  {
    var useImport = new FnBfxfBuildHouseholdHistTbl.Import();
    var useExport = new FnBfxfBuildHouseholdHistTbl.Export();

    useImport.Collection.Amount = entities.Persistent.Amount;
    useImport.SuppPerson.Number = entities.Supported.Number;
    MoveCashReceiptDetail(entities.CashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.Obligor.Number = entities.Obligor1.Number;
    useImport.MedicalObligationType.Flag = local.MedicalObligationType.Flag;

    Call(FnBfxfBuildHouseholdHistTbl.Execute, useImport, useExport);

    useExport.HhHist.CopyTo(local.HhHist, MoveHhHist4);
    useExport.Legal.CopyTo(local.Legal, MoveLegal3);
  }

  private void UseFnDetermineUraForSuppPrsn()
  {
    var useImport = new FnDetermineUraForSuppPrsn.Import();
    var useExport = new FnDetermineUraForSuppPrsn.Export();

    useImport.SuppPrsn.Number = entities.Supported.Number;
    useImport.Collection.Date = local.CollectionDate.Date;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    local.Legal.CopyTo(useImport.Legal, MoveLegal2);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);

    Call(FnDetermineUraForSuppPrsn.Execute, useImport, useExport);

    local.UraAmount.TotalCurrency = useExport.UraAmount.TotalCurrency;
    local.UraMedicalAmount.TotalCurrency =
      useExport.UraMedicalAmount.TotalCurrency;
  }

  private void CreateImHouseholdMbrMnthlyAdj()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var type1 = local.ForAdd.Type1;
    var adjustmentAmount = local.ForAdd.AdjustmentAmount;
    var levelAppliedTo = "M";
    var createdBy = global.UserId;
    var createdTmst = Now();
    var imhAeCaseNo = entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo;
    var cspNumber = entities.ImHouseholdMbrMnthlySum.CspNumber;
    var imsMonth = entities.ImHouseholdMbrMnthlySum.Month;
    var imsYear = entities.ImHouseholdMbrMnthlySum.Year;
    var adjustmentReason = "URA CONVERSION";

    entities.ImHouseholdMbrMnthlyAdj.Populated = false;
    Update("CreateImHouseholdMbrMnthlyAdj",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "adjustmentAmt", adjustmentAmount);
        db.SetString(command, "levelAppliedTo", levelAppliedTo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "imsMonth", imsMonth);
        db.SetInt32(command, "imsYear", imsYear);
        db.SetString(command, "adjustmentReason", adjustmentReason);
      });

    entities.ImHouseholdMbrMnthlyAdj.Type1 = type1;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentAmount = adjustmentAmount;
    entities.ImHouseholdMbrMnthlyAdj.LevelAppliedTo = levelAppliedTo;
    entities.ImHouseholdMbrMnthlyAdj.CreatedBy = createdBy;
    entities.ImHouseholdMbrMnthlyAdj.CreatedTmst = createdTmst;
    entities.ImHouseholdMbrMnthlyAdj.ImhAeCaseNo = imhAeCaseNo;
    entities.ImHouseholdMbrMnthlyAdj.CspNumber = cspNumber;
    entities.ImHouseholdMbrMnthlyAdj.ImsMonth = imsMonth;
    entities.ImHouseholdMbrMnthlyAdj.ImsYear = imsYear;
    entities.ImHouseholdMbrMnthlyAdj.AdjustmentReason = adjustmentReason;
    entities.ImHouseholdMbrMnthlyAdj.Populated = true;
  }

  private IEnumerable<bool>
    ReadCollectionCashReceiptDetailCashReceiptObligationTransaction()
  {
    entities.Persistent.Populated = false;
    entities.Supported.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.Obligor1.Populated = false;

    return ReadEach(
      "ReadCollectionCashReceiptDetailCashReceiptObligationTransaction",
      (db, command) =>
      {
        db.SetDate(
          command, "collDt",
          local.RestartCollection.CollectionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Persistent.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Persistent.AppliedToCode = db.GetString(reader, 1);
        entities.Persistent.CollectionDt = db.GetDate(reader, 2);
        entities.Persistent.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Persistent.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Persistent.ConcurrentInd = db.GetString(reader, 5);
        entities.Persistent.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Persistent.CrtType = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 7);
        entities.Persistent.CstId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 8);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 8);
        entities.Persistent.CrvId = db.GetInt32(reader, 9);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 9);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 9);
        entities.Persistent.CrdId = db.GetInt32(reader, 10);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 10);
        entities.Persistent.ObgId = db.GetInt32(reader, 11);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.Persistent.CspNumber = db.GetString(reader, 12);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.Obligor1.Number = db.GetString(reader, 12);
        entities.Obligor1.Number = db.GetString(reader, 12);
        entities.Obligor1.Number = db.GetString(reader, 12);
        entities.Persistent.CpaType = db.GetString(reader, 13);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.Persistent.OtrId = db.GetInt32(reader, 14);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 14);
        entities.Persistent.OtrType = db.GetString(reader, 15);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 15);
        entities.DebtDetail.OtrType = db.GetString(reader, 15);
        entities.Persistent.OtyId = db.GetInt32(reader, 16);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 16);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 16);
        entities.Persistent.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Persistent.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Persistent.CreatedBy = db.GetString(reader, 19);
        entities.Persistent.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Persistent.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Persistent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Persistent.Amount = db.GetDecimal(reader, 23);
        entities.Persistent.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Persistent.DistributionMethod = db.GetString(reader, 25);
        entities.Persistent.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Persistent.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Persistent.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Persistent.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Persistent.DisburseToArInd = db.GetNullableString(reader, 30);
        entities.Persistent.ManualDistributionReasonText =
          db.GetNullableString(reader, 31);
        entities.Persistent.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 32);
        entities.Persistent.CourtOrderAppliedTo =
          db.GetNullableString(reader, 33);
        entities.Persistent.AppliedToFuture = db.GetString(reader, 34);
        entities.Persistent.CsenetOutboundReqInd = db.GetString(reader, 35);
        entities.Persistent.CsenetOutboundAdjProjDt =
          db.GetNullableDate(reader, 36);
        entities.Persistent.CourtNoticeAdjProcessDate = db.GetDate(reader, 37);
        entities.Persistent.DistPgmStateAppldTo =
          db.GetNullableString(reader, 38);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 39);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 40);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 41);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 42);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 43);
        entities.Supported.Number = db.GetString(reader, 43);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 44);
        entities.DebtDetail.DueDt = db.GetDate(reader, 45);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 46);
        entities.ObligationType.Code = db.GetString(reader, 47);
        entities.ObligationType.Classification = db.GetString(reader, 48);
        entities.Persistent.Populated = true;
        entities.Supported.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.Obligor1.Populated = true;

        return true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;

    return Read("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(
          command, "imhAeCaseNo",
          local.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.AeCaseNo);
        db.SetString(
          command, "cspNumber", local.HhHist.Item.HhHistSuppPrsn.Number);
        db.SetInt32(
          command, "year0",
          local.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
            Year);
        db.SetInt32(
          command, "month0",
          local.HhHist.Item.HhHistDtl.Item.HhHistDtlImHouseholdMbrMnthlySum.
            Month);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.UraAmount =
          db.GetNullableDecimal(reader, 2);
        entities.ImHouseholdMbrMnthlySum.UraMedicalAmount =
          db.GetNullableDecimal(reader, 3);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 6);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 7);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
      });
  }

  private bool ReadProgramCheckpointRestart()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return Read("ReadProgramCheckpointRestart",
      null,
      (db, reader) =>
      {
        entities.ProgramCheckpointRestart.ProgramName = db.GetString(reader, 0);
        entities.ProgramCheckpointRestart.UpdateFrequencyCount =
          db.GetNullableInt32(reader, 1);
        entities.ProgramCheckpointRestart.ReadFrequencyCount =
          db.GetNullableInt32(reader, 2);
        entities.ProgramCheckpointRestart.CheckpointCount =
          db.GetNullableInt32(reader, 3);
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ProgramCheckpointRestart.RestartInd =
          db.GetNullableString(reader, 5);
        entities.ProgramCheckpointRestart.RestartInfo =
          db.GetNullableString(reader, 6);
        entities.ProgramCheckpointRestart.Populated = true;
      });
  }

  private bool ReadUraCollectionApplication()
  {
    System.Diagnostics.Debug.Assert(entities.Persistent.Populated);
    entities.UraCollectionApplication.Populated = false;

    return Read("ReadUraCollectionApplication",
      (db, command) =>
      {
        db.SetInt32(command, "obgIdentifier", entities.Persistent.ObgId);
        db.SetString(command, "cpaType", entities.Persistent.CpaType);
        db.SetInt32(command, "otyIdentifier", entities.Persistent.OtyId);
        db.SetString(command, "cspNumber", entities.Persistent.CspNumber);
        db.SetInt32(command, "otrIdentifier", entities.Persistent.OtrId);
        db.SetInt32(command, "crvIdentifier", entities.Persistent.CrvId);
        db.SetInt32(
          command, "colIdentifier",
          entities.Persistent.SystemGeneratedIdentifier);
        db.SetInt32(command, "crdIdentifier", entities.Persistent.CrdId);
        db.SetInt32(command, "cstIdentifier", entities.Persistent.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Persistent.CrtType);
        db.SetString(command, "otrType", entities.Persistent.OtrType);
      },
      (db, reader) =>
      {
        entities.UraCollectionApplication.CspNumber = db.GetString(reader, 0);
        entities.UraCollectionApplication.CpaType = db.GetString(reader, 1);
        entities.UraCollectionApplication.OtyIdentifier =
          db.GetInt32(reader, 2);
        entities.UraCollectionApplication.ObgIdentifier =
          db.GetInt32(reader, 3);
        entities.UraCollectionApplication.OtrIdentifier =
          db.GetInt32(reader, 4);
        entities.UraCollectionApplication.OtrType = db.GetString(reader, 5);
        entities.UraCollectionApplication.CstIdentifier =
          db.GetInt32(reader, 6);
        entities.UraCollectionApplication.CrvIdentifier =
          db.GetInt32(reader, 7);
        entities.UraCollectionApplication.CrtIdentifier =
          db.GetInt32(reader, 8);
        entities.UraCollectionApplication.CrdIdentifier =
          db.GetInt32(reader, 9);
        entities.UraCollectionApplication.ColIdentifier =
          db.GetInt32(reader, 10);
        entities.UraCollectionApplication.ImhAeCaseNo =
          db.GetString(reader, 11);
        entities.UraCollectionApplication.CspNumber0 = db.GetString(reader, 12);
        entities.UraCollectionApplication.ImsMonth = db.GetInt32(reader, 13);
        entities.UraCollectionApplication.ImsYear = db.GetInt32(reader, 14);
        entities.UraCollectionApplication.CreatedTstamp =
          db.GetDateTime(reader, 15);
        entities.UraCollectionApplication.Populated = true;
      });
  }

  private void UpdateImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);

    var uraAmount = local.ForUpdate.UraAmount.GetValueOrDefault();
    var uraMedicalAmount = local.ForUpdate.UraMedicalAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ImHouseholdMbrMnthlySum.Populated = false;
    Update("UpdateImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "uraAmount", uraAmount);
        db.SetNullableDecimal(command, "uraMedicalAmount", uraMedicalAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "year0", entities.ImHouseholdMbrMnthlySum.Year);
        db.SetInt32(command, "month0", entities.ImHouseholdMbrMnthlySum.Month);
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetString(
          command, "cspNumber", entities.ImHouseholdMbrMnthlySum.CspNumber);
      });

    entities.ImHouseholdMbrMnthlySum.UraAmount = uraAmount;
    entities.ImHouseholdMbrMnthlySum.UraMedicalAmount = uraMedicalAmount;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedBy = lastUpdatedBy;
    entities.ImHouseholdMbrMnthlySum.LastUpdatedTmst = lastUpdatedTmst;
    entities.ImHouseholdMbrMnthlySum.Populated = true;
  }

  private void UpdateProgramCheckpointRestart1()
  {
    var lastCheckpointTimestamp = Now();
    var restartInd = "Y";
    var restartInfo = local.ProgramCheckpointRestart.RestartInfo ?? "";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", restartInfo);
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

    entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
      lastCheckpointTimestamp;
    entities.ProgramCheckpointRestart.RestartInd = restartInd;
    entities.ProgramCheckpointRestart.RestartInfo = restartInfo;
    entities.ProgramCheckpointRestart.Populated = true;
  }

  private void UpdateProgramCheckpointRestart2()
  {
    var lastCheckpointTimestamp = Now();
    var restartInd = "N";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", "");
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

    entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
      lastCheckpointTimestamp;
    entities.ProgramCheckpointRestart.RestartInd = restartInd;
    entities.ProgramCheckpointRestart.RestartInfo = "";
    entities.ProgramCheckpointRestart.Populated = true;
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
        HhHistDtlGroup.Capacity, 0);

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

    /// <summary>
    /// A value of MedicalObligationType.
    /// </summary>
    [JsonPropertyName("medicalObligationType")]
    public Common MedicalObligationType
    {
      get => medicalObligationType ??= new();
      set => medicalObligationType = value;
    }

    /// <summary>
    /// A value of HardcodedMj.
    /// </summary>
    [JsonPropertyName("hardcodedMj")]
    public ObligationType HardcodedMj
    {
      get => hardcodedMj ??= new();
      set => hardcodedMj = value;
    }

    /// <summary>
    /// A value of HardcodedMc.
    /// </summary>
    [JsonPropertyName("hardcodedMc")]
    public ObligationType HardcodedMc
    {
      get => hardcodedMc ??= new();
      set => hardcodedMc = value;
    }

    /// <summary>
    /// A value of RestartObligationTransaction.
    /// </summary>
    [JsonPropertyName("restartObligationTransaction")]
    public ObligationTransaction RestartObligationTransaction
    {
      get => restartObligationTransaction ??= new();
      set => restartObligationTransaction = value;
    }

    /// <summary>
    /// A value of RestartDebtDetail.
    /// </summary>
    [JsonPropertyName("restartDebtDetail")]
    public DebtDetail RestartDebtDetail
    {
      get => restartDebtDetail ??= new();
      set => restartDebtDetail = value;
    }

    /// <summary>
    /// A value of CollectionsApplied.
    /// </summary>
    [JsonPropertyName("collectionsApplied")]
    public Common CollectionsApplied
    {
      get => collectionsApplied ??= new();
      set => collectionsApplied = value;
    }

    /// <summary>
    /// A value of MnthlySummFound.
    /// </summary>
    [JsonPropertyName("mnthlySummFound")]
    public Common MnthlySummFound
    {
      get => mnthlySummFound ??= new();
      set => mnthlySummFound = value;
    }

    /// <summary>
    /// A value of ForAdd.
    /// </summary>
    [JsonPropertyName("forAdd")]
    public ImHouseholdMbrMnthlyAdj ForAdd
    {
      get => forAdd ??= new();
      set => forAdd = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public ImHouseholdMbrMnthlySum ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
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
    /// A value of HardcodedMs.
    /// </summary>
    [JsonPropertyName("hardcodedMs")]
    public ObligationType HardcodedMs
    {
      get => hardcodedMs ??= new();
      set => hardcodedMs = value;
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
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

    /// <summary>
    /// A value of TestCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("testCashReceiptDetail")]
    public CashReceiptDetail TestCashReceiptDetail
    {
      get => testCashReceiptDetail ??= new();
      set => testCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of TestCollection.
    /// </summary>
    [JsonPropertyName("testCollection")]
    public Collection TestCollection
    {
      get => testCollection ??= new();
      set => testCollection = value;
    }

    /// <summary>
    /// A value of RestartCashReceipt.
    /// </summary>
    [JsonPropertyName("restartCashReceipt")]
    public CashReceipt RestartCashReceipt
    {
      get => restartCashReceipt ??= new();
      set => restartCashReceipt = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("restartCashReceiptDetail")]
    public CashReceiptDetail RestartCashReceiptDetail
    {
      get => restartCashReceiptDetail ??= new();
      set => restartCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of RestartCollection.
    /// </summary>
    [JsonPropertyName("restartCollection")]
    public Collection RestartCollection
    {
      get => restartCollection ??= new();
      set => restartCollection = value;
    }

    /// <summary>
    /// Gets a value of HhHist.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity, 0);

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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of TestRun.
    /// </summary>
    [JsonPropertyName("testRun")]
    public Common TestRun
    {
      get => testRun ??= new();
      set => testRun = value;
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
    /// A value of TestHh.
    /// </summary>
    [JsonPropertyName("testHh")]
    public ImHousehold TestHh
    {
      get => testHh ??= new();
      set => testHh = value;
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
    /// A value of CollectionRecordsRead.
    /// </summary>
    [JsonPropertyName("collectionRecordsRead")]
    public Common CollectionRecordsRead
    {
      get => collectionRecordsRead ??= new();
      set => collectionRecordsRead = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of ErroredCollectionRecords.
    /// </summary>
    [JsonPropertyName("erroredCollectionRecords")]
    public Common ErroredCollectionRecords
    {
      get => erroredCollectionRecords ??= new();
      set => erroredCollectionRecords = value;
    }

    /// <summary>
    /// A value of CreatedAdjustments.
    /// </summary>
    [JsonPropertyName("createdAdjustments")]
    public Common CreatedAdjustments
    {
      get => createdAdjustments ??= new();
      set => createdAdjustments = value;
    }

    /// <summary>
    /// A value of NbrOfCheckpoint.
    /// </summary>
    [JsonPropertyName("nbrOfCheckpoint")]
    public Common NbrOfCheckpoint
    {
      get => nbrOfCheckpoint ??= new();
      set => nbrOfCheckpoint = value;
    }

    /// <summary>
    /// A value of DummyLoop.
    /// </summary>
    [JsonPropertyName("dummyLoop")]
    public Common DummyLoop
    {
      get => dummyLoop ??= new();
      set => dummyLoop = value;
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
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    private Common medicalObligationType;
    private ObligationType hardcodedMj;
    private ObligationType hardcodedMc;
    private ObligationTransaction restartObligationTransaction;
    private DebtDetail restartDebtDetail;
    private Common collectionsApplied;
    private Common mnthlySummFound;
    private ImHouseholdMbrMnthlyAdj forAdd;
    private ImHouseholdMbrMnthlySum forUpdate;
    private LegalAction legalAction;
    private ObligationType hardcodedMs;
    private Common uraMedicalAmount;
    private Common uraAmount;
    private DateWorkArea collectionDate;
    private CashReceiptDetail testCashReceiptDetail;
    private Collection testCollection;
    private CashReceipt restartCashReceipt;
    private CashReceiptDetail restartCashReceiptDetail;
    private Collection restartCollection;
    private Array<HhHistGroup> hhHist;
    private Array<LegalGroup> legal;
    private ProgramProcessingInfo programProcessingInfo;
    private Common displayInd;
    private Common testRun;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ImHousehold testHh;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common collectionRecordsRead;
    private EabFileHandling status;
    private Common erroredCollectionRecords;
    private Common createdAdjustments;
    private Common nbrOfCheckpoint;
    private Common dummyLoop;
    private ExitStateWorkArea exitStateWorkArea;
    private Common commitCnt;
    private External forCommit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of UraCollectionApplication.
    /// </summary>
    [JsonPropertyName("uraCollectionApplication")]
    public UraCollectionApplication UraCollectionApplication
    {
      get => uraCollectionApplication ??= new();
      set => uraCollectionApplication = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlyAdj.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlyAdj")]
    public ImHouseholdMbrMnthlyAdj ImHouseholdMbrMnthlyAdj
    {
      get => imHouseholdMbrMnthlyAdj ??= new();
      set => imHouseholdMbrMnthlyAdj = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Collection Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private UraCollectionApplication uraCollectionApplication;
    private ImHouseholdMbrMnthlyAdj imHouseholdMbrMnthlyAdj;
    private Collection persistent;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private CsePersonAccount csePersonAccount;
    private CsePerson supported;
    private DebtDetail debtDetail;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private Collection collection;
    private ProgramCheckpointRestart programCheckpointRestart;
  }
#endregion
}
