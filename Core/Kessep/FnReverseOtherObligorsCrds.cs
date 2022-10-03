// Program: FN_REVERSE_OTHER_OBLIGORS_CRDS, ID: 372265453, model: 746.
// Short name: SWE02350
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REVERSE_OTHER_OBLIGORS_CRDS.
/// </summary>
[Serializable]
public partial class FnReverseOtherObligorsCrds: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REVERSE_OTHER_OBLIGORS_CRDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReverseOtherObligorsCrds(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReverseOtherObligorsCrds.
  /// </summary>
  public FnReverseOtherObligorsCrds(IContext context, Import import,
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
    // ************************************************************************
    //   Date      Programmer   Problem #   Description
    // ----------  ----------   ---------
    // 
    // ----------------------------------
    // 02/01/1999  Ed Lyman                 Cloned action block.
    // 10/05/1999  Ed Lyman     H00074221   Removed logic to update the
    //                                      
    // recompute date in obligor.
    //                                      
    // Fixed view matching with the cab
    //                                      
    // FN_CAB_REPLICATE_ONE_COLLECTION
    // 10/27/1999  E. Lyman     H00A74221   Set Disbursement Processing Need Ind
    //                                      
    // to 'N' on Collection.
    // 12/01/1999  E. Lyman     PR# 81189   Process Cash Receipt Details that
    //                                      
    // are in Released status.
    // 06/01/2000  E. Lyman     WR# 164-G   Modify logic for new URA adjustments
    //                                      
    // trigger as part of PRWORA.
    // 09/01/2000  E. Lyman     PR#102554   Modify logic for new trigger type
    //                                      
    // for Court Order Number changes.
    // 10/30/2000  M Brown      WR#000197   Added Collection dist method of 'W'
    //                                      
    // wherever dist method of 'M' is
    // referred to.
    //                                      
    // They mean the same thing in batch
    // processing.
    //                                      
    // The 'W' is set by debt adjustment
    // when a
    //                                      
    // debt is written off, to enable us to
    // set it back
    //                                      
    // to 'A'utomatic if the debt is
    // reinstated.
    // 11/30/2000  M Brown      PR#105381   bug fix in read of collection. It 
    // was reading
    //                                      
    // for a debt detail due date greater
    // than
    //                                      
    // trigger date, and should be greater
    // or equal to.
    //                                      
    // Search for '105381' to find the
    // actual read.
    // 12/15/2001  E. Lyman     WR#010504   Modify logic for new trigger type
    //                                      
    // for deactivated collection
    // protection.
    export.NoCashRecptDtlUpdated.Count = import.NoCashRecptDtlUpdated.Count;
    export.NoOfIncrementalUpdates.Count = import.NoOfIncrementalUpdates.Count;
    export.NoCollectionsReversed.Count = import.NoCollectionsReversed.Count;

    if (!ReadCollectionAdjustmentReason())
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF";

      return;
    }

    foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatHistory2())
    {
      local.Revised.DistributedAmount =
        entities.CashReceiptDetail.DistributedAmount;

      if (AsChar(import.ReportNeeded.Flag) == 'Y')
      {
        // *** Format Refunded Amount ***
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.CashReceiptDetail.RefundedAmount.
            GetValueOrDefault() * 100), 15);

        if (Lt(entities.CashReceiptDetail.RefundedAmount, 0))
        {
          local.EabConvertNumeric.SendSign = "-";
        }

        UseEabConvertNumeric1();
        local.Refunded.ReturnCurrencySigned =
          local.EabConvertNumeric.ReturnCurrencySigned;
        local.EabConvertNumeric.Assign(local.Clear);

        // *** Format Receipt Amount ***
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.CashReceiptDetail.CollectionAmount * 100
          ), 15);

        if (entities.CashReceiptDetail.CollectionAmount < 0)
        {
          local.EabConvertNumeric.SendSign = "-";
        }

        UseEabConvertNumeric1();
        local.Receipt.ReturnCurrencySigned =
          local.EabConvertNumeric.ReturnCurrencySigned;
        local.EabConvertNumeric.Assign(local.Clear);

        // *** Format Distributed Amount ***
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(entities.CashReceiptDetail.DistributedAmount.
            GetValueOrDefault() * 100), 15);

        if (Lt(entities.CashReceiptDetail.DistributedAmount, 0))
        {
          local.EabConvertNumeric.SendSign = "-";
        }

        UseEabConvertNumeric1();
        local.Distributed.ReturnCurrencySigned =
          local.EabConvertNumeric.ReturnCurrencySigned;
        local.EabConvertNumeric.Assign(local.Clear);
        local.FormatDate.Text10 =
          NumberToString(Month(entities.CashReceiptDetail.CollectionDate), 14, 2)
          + "-" + NumberToString
          (Day(entities.CashReceiptDetail.CollectionDate), 14, 2) + "-" + NumberToString
          (Year(entities.CashReceiptDetail.CollectionDate), 12, 4);
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          entities.CashReceiptDetailStatus.Code + "  " + entities
          .CashReceiptDetail.ObligorPersonNumber + "   " + local
          .FormatDate.Text10 + "    " + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 13, 3) + "  " + Substring
          (local.Receipt.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " + "            " +
          " " + Substring
          (local.Distributed.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " + Substring
          (local.Refunded.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + "   " + entities
          .CashReceiptDetail.CourtOrderNumber;
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // ***  In  the following logic, we are using the timestamp to make sure
      // ***  that we do not read a collection that we have just replicated.
      local.CollRevThisCrd.Count = 0;

      foreach(var item1 in ReadCollectionObligationTypeCsePersonObligor())
      {
        if (AsChar(entities.Collection.AdjustedInd) == 'Y')
        {
          continue;
        }

        if (Equal(entities.ObligationType.Code, "VOL"))
        {
          continue;
        }
        else
        {
        }

        if (AsChar(entities.Collection.DistributionMethod) == 'C' || AsChar
          (entities.Collection.DistributionMethod) == 'M' || AsChar
          (entities.Collection.DistributionMethod) == 'P' || AsChar
          (entities.Collection.DistributionMethod) == 'W' || AsChar
          (entities.Collection.AppliedToCode) == 'G')
        {
          // ***  C = CLOSED CASE          ***
          // ***  M = MANUALLY DISTRIBUTED ***
          // ***  P = PROTECTED COLLECTION ***
          // ***  W = WRITE OFF COLLECTION ***
          // ***  G = GIFT                 ***
          continue;
        }

        UseFnCabReverseOneCollection();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(entities.Collection.ConcurrentInd) == 'Y')
          {
            // *****************************************************************
            // *  This is a memo only collection applied to either the
            // *  Secondary debt (in a Primary, Secondary situation)
            // *  or to the other obligor's debt (in a Joint & Several
            // *  situation).  The Cash Receipt Detail is not updated for
            // *  these memo collections that are reversed.
            // *****************************************************************
          }
          else
          {
            local.Revised.DistributedAmount =
              local.Revised.DistributedAmount.GetValueOrDefault() - entities
              .Collection.Amount;
            ++local.CollRevThisCrd.Count;
          }

          ++export.NoCollectionsReversed.Count;
          export.NoOfIncrementalUpdates.Count += 3;
        }
        else if (IsExitState("ACO_NI0000_DATA_WAS_NOT_CHANGED"))
        {
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else
        {
          return;
        }

        if (AsChar(import.ReportNeeded.Flag) == 'Y')
        {
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)(entities.Collection.Amount * 100), 15);

          if (entities.Collection.Amount < 0)
          {
            local.EabConvertNumeric.SendSign = "-";
          }

          UseEabConvertNumeric1();
          local.Collection.ReturnCurrencySigned =
            local.EabConvertNumeric.ReturnCurrencySigned;
          local.EabConvertNumeric.Assign(local.Clear);
          local.FormatDate.Text10 =
            NumberToString(Month(entities.Collection.CollectionDt), 14, 2) + "-"
            + NumberToString(Day(entities.Collection.CollectionDt), 14, 2) + "-"
            + NumberToString(Year(entities.Collection.CollectionDt), 12, 4);
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "  reversed" + "  " + entities
            .CsePerson.Number + "   " + local.FormatDate.Text10 + "    " + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 13, 3) + "  " + "            " +
            " " + " " + Substring
            (local.Collection.ReturnCurrencySigned,
            EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " + " " +
            " " + " " + " ";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabConvertNumeric.Assign(local.Clear);
        }
      }

      if (local.CollRevThisCrd.Count > 0)
      {
        try
        {
          UpdateCashReceiptDetail();
          ++export.NoCashRecptDtlUpdated.Count;
          ++export.NoOfIncrementalUpdates.Count;

          // ***** Set the Cash Receipt Detail Status to "Released".  The 
          // current Status History must be inactivated and a new Status History
          // must be created.  Use CABs to set the system generated identifier
          // and the maximum discontinued date.
          if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == import
            .RefundedStatus.SystemGeneratedIdentifier || entities
            .CashReceiptDetailStatus.SystemGeneratedIdentifier == import
            .ReleasedStatus.SystemGeneratedIdentifier)
          {
          }
          else
          {
            try
            {
              UpdateCashReceiptDetailStatHistory();
              ++export.NoOfIncrementalUpdates.Count;

              if (Lt(0, entities.CashReceiptDetail.RefundedAmount))
              {
                if (!ReadCashReceiptDetailStatus1())
                {
                  ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

                  return;
                }
              }
              else if (!ReadCashReceiptDetailStatus2())
              {
                ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

                return;
              }

              try
              {
                CreateCashReceiptDetailStatHistory();
                ++export.NoOfIncrementalUpdates.Count;
              }
              catch(Exception e2)
              {
                switch(GetErrorCode(e2))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0063_CASH_RCPT_DTL_STAT_HST_AE";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0065_CASH_RCPT_DTL_STAT_HST_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0054_CASH_RCPT_DTL_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0056_CASH_RCPT_DTL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    // *** If collections with a collection date prior to the import date have 
    // been
    // *** applied to future debts that are due after the new or adjusted debt, 
    // then
    // *** reverse those collections as well.
    if (AsChar(import.TypeOfChange.SelectChar) == 'D' || AsChar
      (import.TypeOfChange.SelectChar) == 'P' || AsChar
      (import.TypeOfChange.SelectChar) == 'U' || AsChar
      (import.TypeOfChange.SelectChar) == 'N' || AsChar
      (import.TypeOfChange.SelectChar) == 'X')
    {
      // ***  TYPE OF CHANGE (D = Debts have changed)
      // ***  TYPE OF CHANGE (P = Program has changed)
      // ***  TYPE OF CHANGE (U = URA has changed)
      // ***  TYPE OF CHANGE (N = Court Order Number has changed)
      // ***  TYPE OF CHANGE (X = Deactivated Collection Protection)
      // ***  TYPE OF CHANGE (C = Collection has changed)
      // ***  TYPE OF CHANGE (R = Case Role has changed)
      // PR# 105381, M Brown, Nov 2000 - This read was reading for a debt detail
      // due date greater than trigger date, and it needs to be greater or
      // equal to.
      foreach(var item in ReadDebtDetailCollection())
      {
        if (AsChar(entities.Collection.DistributionMethod) == 'C' || AsChar
          (entities.Collection.DistributionMethod) == 'M' || AsChar
          (entities.Collection.DistributionMethod) == 'P' || AsChar
          (entities.Collection.DistributionMethod) == 'W' || AsChar
          (entities.Collection.AppliedToCode) == 'G')
        {
          // ***  C = CLOSED CASE          ***
          // ***  M = MANUALLY DISTRIBUTED ***
          // ***  P = PROTECTED COLLECTION ***
          // ***  W = WRITE OFF COLLECTION ***
          // ***  G = GIFT                 ***
          continue;
        }

        if (ReadObligationTypeCsePersonObligor())
        {
          if (Equal(entities.ObligationType.Code, "VOL"))
          {
            continue;
          }
          else
          {
          }
        }
        else
        {
          ExitState = "FN0000_OBLIG_TYPE_NF";

          return;
        }

        if (ReadCashReceiptDetailCashReceiptDetailStatHistory1())
        {
          local.Revised.DistributedAmount =
            entities.CashReceiptDetail.DistributedAmount;
          local.CollRevThisCrd.Count = 0;
        }
        else
        {
          ExitState = "FN0052_CASH_RCPT_DTL_NF";

          return;
        }

        if (AsChar(import.ReportNeeded.Flag) == 'Y')
        {
          // *** Format Refunded Amount ***
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)(entities.CashReceiptDetail.RefundedAmount.
              GetValueOrDefault() * 100), 15);

          if (Lt(entities.CashReceiptDetail.RefundedAmount, 0))
          {
            local.EabConvertNumeric.SendSign = "-";
          }

          UseEabConvertNumeric1();
          local.Refunded.ReturnCurrencySigned =
            local.EabConvertNumeric.ReturnCurrencySigned;
          local.EabConvertNumeric.Assign(local.Clear);

          // *** Format Receipt Amount ***
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)(entities.CashReceiptDetail.CollectionAmount *
            100), 15);

          if (entities.CashReceiptDetail.CollectionAmount < 0)
          {
            local.EabConvertNumeric.SendSign = "-";
          }

          UseEabConvertNumeric1();
          local.Receipt.ReturnCurrencySigned =
            local.EabConvertNumeric.ReturnCurrencySigned;
          local.EabConvertNumeric.Assign(local.Clear);

          // *** Format Distributed Amount ***
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)(entities.CashReceiptDetail.DistributedAmount.
              GetValueOrDefault() * 100), 15);

          if (Lt(entities.CashReceiptDetail.DistributedAmount, 0))
          {
            local.EabConvertNumeric.SendSign = "-";
          }

          UseEabConvertNumeric1();
          local.Distributed.ReturnCurrencySigned =
            local.EabConvertNumeric.ReturnCurrencySigned;
          local.EabConvertNumeric.Assign(local.Clear);
          local.FormatDate.Text10 =
            NumberToString(Month(entities.CashReceiptDetail.CollectionDate), 14,
            2) + "-" + NumberToString
            (Day(entities.CashReceiptDetail.CollectionDate), 14, 2) + "-" + NumberToString
            (Year(entities.CashReceiptDetail.CollectionDate), 12, 4);
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            entities.CashReceiptDetailStatus.Code + "  " + entities
            .CashReceiptDetail.ObligorPersonNumber + "   " + local
            .FormatDate.Text10 + "    " + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 13, 3) + "  " + Substring
            (local.Receipt.ReturnCurrencySigned,
            EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " + "            " +
            " " + Substring
            (local.Distributed.ReturnCurrencySigned,
            EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " + Substring
            (local.Refunded.ReturnCurrencySigned,
            EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + "   " +
            entities.CashReceiptDetail.CourtOrderNumber;
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        UseFnCabReverseOneCollection();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(entities.Collection.ConcurrentInd) == 'Y')
          {
            // *****************************************************************
            // *  This is a memo only collection applied to a secondary debt
            // *  or to the other debt on a Joint and Several.
            // *  Only reduce the Cash Receipt Detail for primary debt
            // *  collections that are reversed.
            // *****************************************************************
          }
          else
          {
            local.Revised.DistributedAmount =
              local.Revised.DistributedAmount.GetValueOrDefault() - entities
              .Collection.Amount;
            ++local.CollRevThisCrd.Count;
          }
        }
        else if (IsExitState("ACO_NI0000_DATA_WAS_NOT_CHANGED"))
        {
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else
        {
          return;
        }

        if (AsChar(import.ReportNeeded.Flag) == 'Y')
        {
          local.EabConvertNumeric.SendAmount =
            NumberToString((long)(entities.Collection.Amount * 100), 15);

          if (entities.Collection.Amount < 0)
          {
            local.EabConvertNumeric.SendSign = "-";
          }

          UseEabConvertNumeric1();
          local.Collection.ReturnCurrencySigned =
            local.EabConvertNumeric.ReturnCurrencySigned;
          local.EabConvertNumeric.Assign(local.Clear);
          local.FormatDate.Text10 =
            NumberToString(Month(entities.Collection.CollectionDt), 14, 2) + "-"
            + NumberToString(Day(entities.Collection.CollectionDt), 14, 2) + "-"
            + NumberToString(Year(entities.Collection.CollectionDt), 12, 4);
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "  reversed" + "  " + entities
            .CsePerson.Number + "   " + local.FormatDate.Text10 + "    " + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 13, 3) + "  " + "            " +
            " " + " " + Substring
            (local.Collection.ReturnCurrencySigned,
            EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) + " " + " " +
            " " + " " + " ";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabConvertNumeric.Assign(local.Clear);
        }

        if (local.CollRevThisCrd.Count > 0)
        {
          try
          {
            UpdateCashReceiptDetail();
            ++export.NoCashRecptDtlUpdated.Count;
            ++export.NoOfIncrementalUpdates.Count;

            // ***** Set the Cash Receipt Detail Status to "Released" or "
            // Refunded".  The current Status History must be inactivated and a
            // new Status History must be created.  Use CABs to set the system
            // generated identifier and the maximum discontinued date.
            if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == import
              .RefundedStatus.SystemGeneratedIdentifier || entities
              .CashReceiptDetailStatus.SystemGeneratedIdentifier == import
              .ReleasedStatus.SystemGeneratedIdentifier)
            {
            }
            else
            {
              try
              {
                UpdateCashReceiptDetailStatHistory();
                ++export.NoOfIncrementalUpdates.Count;

                if (Lt(0, entities.CashReceiptDetail.RefundedAmount))
                {
                  if (!ReadCashReceiptDetailStatus1())
                  {
                    ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

                    return;
                  }
                }
                else if (!ReadCashReceiptDetailStatus2())
                {
                  ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

                  return;
                }

                try
                {
                  CreateCashReceiptDetailStatHistory();
                  ++export.NoOfIncrementalUpdates.Count;
                }
                catch(Exception e2)
                {
                  switch(GetErrorCode(e2))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "FN0063_CASH_RCPT_DTL_STAT_HST_AE";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0065_CASH_RCPT_DTL_STAT_HST_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0054_CASH_RCPT_DTL_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0056_CASH_RCPT_DTL_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
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
    target.AeNotifiedDate = source.AeNotifiedDate;
    target.BalForIntCompBefColl = source.BalForIntCompBefColl;
    target.CumIntChargedUptoColl = source.CumIntChargedUptoColl;
    target.CumIntCollAfterThisColl = source.CumIntCollAfterThisColl;
    target.IntBalAftThisColl = source.IntBalAftThisColl;
    target.DisburseToArInd = source.DisburseToArInd;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
    target.AppliedToFuture = source.AppliedToFuture;
    target.CsenetOutboundReqInd = source.CsenetOutboundReqInd;
    target.CsenetOutboundProcDt = source.CsenetOutboundProcDt;
    target.CsenetOutboundAdjProjDt = source.CsenetOutboundAdjProjDt;
    target.CourtNoticeAdjProcessDate = source.CourtNoticeAdjProcessDate;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
    target.OtyId = source.OtyId;
    target.OtrType = source.OtrType;
    target.OtrId = source.OtrId;
    target.CpaType = source.CpaType;
    target.CspNumber = source.CspNumber;
    target.ObgId = source.ObgId;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnCurrencySigned = source.ReturnCurrencySigned;
    target.ReturnCurrencyNegInParens = source.ReturnCurrencyNegInParens;
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
    target.ReturnOkFlag = source.ReturnOkFlag;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.Assign(local.EabConvertNumeric);

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseFnCabReverseOneCollection()
  {
    var useImport = new FnCabReverseOneCollection.Import();
    var useExport = new FnCabReverseOneCollection.Export();

    useImport.Persistent.Assign(entities.CollectionAdjustmentReason);
    useImport.Pers.Assign(entities.Collection);
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    MoveCollection2(import.Collection, useImport.Collection);
    useImport.Max.Date = import.Max.Date;
    useImport.TypeOfChange.SelectChar = import.TypeOfChange.SelectChar;

    Call(FnCabReverseOneCollection.Execute, useImport, useExport);

    entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      useImport.Persistent.SystemGeneratedIdentifier;
    MoveCollection(useImport.Pers, entities.Collection);
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier =
      entities.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var createdBy = import.ProgramProcessingInfo.Name;
    var discontinueDate = import.Max.Date;
    var reasonText = Spaces(240);

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.CashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.CashReceiptDetailStatHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailStatHistory.ReasonCodeId = "";
    entities.CashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.ReasonText = reasonText;
    entities.CashReceiptDetailStatHistory.Populated = true;
  }

  private bool ReadCashReceiptDetailCashReceiptDetailStatHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptDetailStatHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 15);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 18);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 21);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 22);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 23);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 24);
        entities.CashReceiptDetailStatus.CreatedBy = db.GetString(reader, 25);
        entities.CashReceiptDetailStatus.CreatedTimestamp =
          db.GetDateTime(reader, 26);
        entities.CashReceiptDetailStatus.LastUpdateBy =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetailStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 28);
        entities.CashReceiptDetailStatus.Description =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatHistory2()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetailStatHistory.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatHistory2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetDate(
          command, "collectionDate",
          import.CashReceiptDetail.CollectionDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.ProgramStart.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          import.DistributedStatus.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          import.ReleasedStatus.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          import.SuspendedStatus.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          import.RefundedStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 15);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 18);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 21);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 22);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 23);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 24);
        entities.CashReceiptDetailStatus.CreatedBy = db.GetString(reader, 25);
        entities.CashReceiptDetailStatus.CreatedTimestamp =
          db.GetDateTime(reader, 26);
        entities.CashReceiptDetailStatus.LastUpdateBy =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetailStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 28);
        entities.CashReceiptDetailStatus.Description =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatus1()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.RefundedStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailStatus.CreatedBy = db.GetString(reader, 5);
        entities.CashReceiptDetailStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CashReceiptDetailStatus.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.CashReceiptDetailStatus.Description =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus2()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.ReleasedStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailStatus.CreatedBy = db.GetString(reader, 5);
        entities.CashReceiptDetailStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CashReceiptDetailStatus.LastUpdateBy =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.CashReceiptDetailStatus.Description =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          import.CollectionAdjustmentReason.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTypeCsePersonObligor()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadCollectionObligationTypeCsePersonObligor",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDateTime(
          command, "createdTmst",
          import.ProgramStart.Timestamp.GetValueOrDefault());
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
        entities.CsePerson.Number = db.GetString(reader, 12);
        entities.Obligor.CspNumber = db.GetString(reader, 12);
        entities.Obligor.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Obligor.Type1 = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedBy = db.GetString(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Collection.DistributionMethod = db.GetString(reader, 25);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 30);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 31);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 32);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 33);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 34);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 35);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 36);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 37);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 38);
        entities.Collection.AppliedToFuture = db.GetString(reader, 39);
        entities.Collection.CsenetOutboundReqInd = db.GetString(reader, 40);
        entities.Collection.CsenetOutboundProcDt =
          db.GetNullableDate(reader, 41);
        entities.Collection.CsenetOutboundAdjProjDt =
          db.GetNullableDate(reader, 42);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 43);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 44);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 45);
        entities.ObligationType.Code = db.GetString(reader, 46);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<Collection>("CsenetOutboundReqInd",
          entities.Collection.CsenetOutboundReqInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailCollection()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetailCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt",
          import.CashReceiptDetail.CollectionDate.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst",
          import.ProgramStart.Timestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
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
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.Collection.AppliedToCode = db.GetString(reader, 15);
        entities.Collection.CollectionDt = db.GetDate(reader, 16);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 17);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 18);
        entities.Collection.ConcurrentInd = db.GetString(reader, 19);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 20);
        entities.Collection.CrtType = db.GetInt32(reader, 21);
        entities.Collection.CstId = db.GetInt32(reader, 22);
        entities.Collection.CrvId = db.GetInt32(reader, 23);
        entities.Collection.CrdId = db.GetInt32(reader, 24);
        entities.Collection.ObgId = db.GetInt32(reader, 25);
        entities.Collection.CspNumber = db.GetString(reader, 26);
        entities.Collection.CpaType = db.GetString(reader, 27);
        entities.Collection.OtrId = db.GetInt32(reader, 28);
        entities.Collection.OtrType = db.GetString(reader, 29);
        entities.Collection.OtyId = db.GetInt32(reader, 30);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 31);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 32);
        entities.Collection.CreatedBy = db.GetString(reader, 33);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 34);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 35);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 36);
        entities.Collection.Amount = db.GetDecimal(reader, 37);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 38);
        entities.Collection.DistributionMethod = db.GetString(reader, 39);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 40);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 41);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 42);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 43);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 44);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 45);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 46);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 47);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 48);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 49);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 50);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 51);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 52);
        entities.Collection.AppliedToFuture = db.GetString(reader, 53);
        entities.Collection.CsenetOutboundReqInd = db.GetString(reader, 54);
        entities.Collection.CsenetOutboundProcDt =
          db.GetNullableDate(reader, 55);
        entities.Collection.CsenetOutboundAdjProjDt =
          db.GetNullableDate(reader, 56);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 57);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 58);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
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
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<Collection>("CsenetOutboundReqInd",
          entities.Collection.CsenetOutboundReqInd);

        return true;
      });
  }

  private bool ReadObligationTypeCsePersonObligor()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationTypeCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "debtTypId", entities.Collection.OtyId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 2);
        entities.Obligor.CspNumber = db.GetString(reader, 2);
        entities.Obligor.Type1 = db.GetString(reader, 3);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = Now();
    var distributedAmount = local.Revised.DistributedAmount.GetValueOrDefault();

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd", "");
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "distributedAmt", distributedAmount);
        db.SetNullableString(command, "collamtApplInd", "");
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.DistributedAmount = distributedAmount;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd = "";
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailStatHistory.Populated);

    var discontinueDate = import.ProgramProcessingInfo.ProcessDate;

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("UpdateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptDetailStatHistory.CrtIdentifier);
        db.SetInt32(
          command, "cdsIdentifier",
          entities.CashReceiptDetailStatHistory.CdsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptDetailStatHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.Populated = true;
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
    /// A value of ProgramStart.
    /// </summary>
    [JsonPropertyName("programStart")]
    public DateWorkArea ProgramStart
    {
      get => programStart ??= new();
      set => programStart = value;
    }

    /// <summary>
    /// A value of NoCashRecptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRecptDtlUpdated")]
    public Common NoCashRecptDtlUpdated
    {
      get => noCashRecptDtlUpdated ??= new();
      set => noCashRecptDtlUpdated = value;
    }

    /// <summary>
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    /// <summary>
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
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
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
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
    /// A value of RefundedStatus.
    /// </summary>
    [JsonPropertyName("refundedStatus")]
    public CashReceiptDetailStatus RefundedStatus
    {
      get => refundedStatus ??= new();
      set => refundedStatus = value;
    }

    /// <summary>
    /// A value of ReleasedStatus.
    /// </summary>
    [JsonPropertyName("releasedStatus")]
    public CashReceiptDetailStatus ReleasedStatus
    {
      get => releasedStatus ??= new();
      set => releasedStatus = value;
    }

    /// <summary>
    /// A value of DistributedStatus.
    /// </summary>
    [JsonPropertyName("distributedStatus")]
    public CashReceiptDetailStatus DistributedStatus
    {
      get => distributedStatus ??= new();
      set => distributedStatus = value;
    }

    /// <summary>
    /// A value of SuspendedStatus.
    /// </summary>
    [JsonPropertyName("suspendedStatus")]
    public CashReceiptDetailStatus SuspendedStatus
    {
      get => suspendedStatus ??= new();
      set => suspendedStatus = value;
    }

    private DateWorkArea programStart;
    private Common noCashRecptDtlUpdated;
    private Common noOfIncrementalUpdates;
    private Common noCollectionsReversed;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private ProgramProcessingInfo programProcessingInfo;
    private CashReceiptDetail cashReceiptDetail;
    private Common reportNeeded;
    private Collection collection;
    private Common typeOfChange;
    private DateWorkArea max;
    private CashReceiptDetailStatus refundedStatus;
    private CashReceiptDetailStatus releasedStatus;
    private CashReceiptDetailStatus distributedStatus;
    private CashReceiptDetailStatus suspendedStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NoCashRecptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRecptDtlUpdated")]
    public Common NoCashRecptDtlUpdated
    {
      get => noCashRecptDtlUpdated ??= new();
      set => noCashRecptDtlUpdated = value;
    }

    /// <summary>
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    /// <summary>
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
    }

    private Common noCashRecptDtlUpdated;
    private Common noOfIncrementalUpdates;
    private Common noCollectionsReversed;
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
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Revised.
    /// </summary>
    [JsonPropertyName("revised")]
    public CashReceiptDetail Revised
    {
      get => revised ??= new();
      set => revised = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Refunded.
    /// </summary>
    [JsonPropertyName("refunded")]
    public EabConvertNumeric2 Refunded
    {
      get => refunded ??= new();
      set => refunded = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public EabConvertNumeric2 Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of Receipt.
    /// </summary>
    [JsonPropertyName("receipt")]
    public EabConvertNumeric2 Receipt
    {
      get => receipt ??= new();
      set => receipt = value;
    }

    /// <summary>
    /// A value of Distributed.
    /// </summary>
    [JsonPropertyName("distributed")]
    public EabConvertNumeric2 Distributed
    {
      get => distributed ??= new();
      set => distributed = value;
    }

    /// <summary>
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
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
    /// A value of CollRevThisCrd.
    /// </summary>
    [JsonPropertyName("collRevThisCrd")]
    public Common CollRevThisCrd
    {
      get => collRevThisCrd ??= new();
      set => collRevThisCrd = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public EabConvertNumeric2 Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private DateWorkArea null1;
    private CashReceiptDetail revised;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabConvertNumeric2 refunded;
    private EabConvertNumeric2 clear;
    private EabConvertNumeric2 receipt;
    private EabConvertNumeric2 distributed;
    private TextWorkArea formatDate;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common collRevThisCrd;
    private EabConvertNumeric2 collection;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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

    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Collection collection;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private DebtDetail debtDetail;
  }
#endregion
}
