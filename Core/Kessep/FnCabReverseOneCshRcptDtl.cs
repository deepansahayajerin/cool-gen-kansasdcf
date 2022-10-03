// Program: FN_CAB_REVERSE_ONE_CSH_RCPT_DTL, ID: 372379098, model: 746.
// Short name: SWE02428
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_REVERSE_ONE_CSH_RCPT_DTL.
/// </summary>
[Serializable]
public partial class FnCabReverseOneCshRcptDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_REVERSE_ONE_CSH_RCPT_DTL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabReverseOneCshRcptDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabReverseOneCshRcptDtl.
  /// </summary>
  public FnCabReverseOneCshRcptDtl(IContext context, Import import,
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
    // -------------------------------------------------
    // DATE      DEVELOPER NAME          REQUEST #  DESCRIPTION
    // 01/07/99  elyman     New action block.
    // 01/14/99  N.Engoor   Changed the READ on the Cash
    // 
    // Receipt  Detail.
    // 03/09/99  N.Engoor   Update the Refunded Amt only if the adj reason code 
    // selected is 'WR ACCT', 'COURTAD', 'BAD CK' or 'ST PMT'.
    // 03/25/99  N.Engoor   Deleted stmnts setting the disbursement processing 
    // need indicator. This attr is not to be set any more.
    // 05/05/00    e.shirk    PRWORA distribution modifications.  Changed A.B. 
    // to call the update_ura_amt A.B. when the collection is adjusted.
    // -------------------------------------------------
    // 02/01/01    P.Phinney    Add logic for "KPC ADJ" 
    // collection_adjustment_reason Code.
    // -------------------------------------------------
    export.NoCollectonReversd.Count = import.NoCollectionsReversed.Count;
    local.Current.Timestamp = Now();

    // ************************************
    // Set the recompute date on CSE Person Account (subtype Obligor) so that 
    // summaries will be recomputed.
    // Do we need to set the recompute date on both obligor's when we have a 
    // Joint and Several obligation?
    // ************************************
    if (ReadCsePersonObligor())
    {
      try
      {
        UpdateObligor();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

      return;
    }

    // ----------------------------------------------
    // N.Engoor  -  01/29/99
    // Changed the READ stmnt so that Cash Receipt detail is retrieved based on 
    // the sequential identifier and the relationship with Cash Receipt
    // ----------------------------------------------
    if (ReadCashReceiptDetail())
    {
      local.Revised.DistributedAmount =
        entities.CashReceiptDetail.DistributedAmount;
      local.CreateCashRecptDtlHist.Assign(entities.CashReceiptDetail);

      if (Equal(local.CreateCashRecptDtlHist.LastUpdatedTmst, null))
      {
        local.CreateCashRecptDtlHist.LastUpdatedTmst =
          local.CreateCashRecptDtlHist.CreatedTmst;
      }

      if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
      {
        foreach(var item in ReadCollection())
        {
          ++export.NoCollectonReversd.Count;

          if (AsChar(entities.Collection.ConcurrentInd) == 'Y')
          {
            // *****************************************************************
            // *  This is a memo only collection applied to a secondary debt
            // *  or to the other debt in Joint and Several obligations.
            // *  Reduce the Cash Receipt Detail distributed amount only for
            // *  primary debt collections that are reversed.
            // *  Subsequent disbursement processing is not needed.
            // *****************************************************************
          }
          else
          {
            local.Revised.DistributedAmount =
              local.Revised.DistributedAmount.GetValueOrDefault() - entities
              .Collection.Amount;
          }

          if (ReadDebtDebtDetail())
          {
            ReadDebtDetailStatusHistory();
            local.Debt.Assign(entities.Debt);
            local.DebtDetail.Assign(entities.DebtDetail);

            if (AsChar(entities.DebtDetailStatusHistory.Code) != 'A')
            {
              local.DebtDetailStatusHistory.Code = "A";
              local.DebtDetailStatusHistory.CreatedBy =
                import.ProgramProcessingInfo.Name;
              local.DebtDetailStatusHistory.EffectiveDt =
                import.ProgramProcessingInfo.ProcessDate;
              local.DebtDetailStatusHistory.ReasonTxt =
                import.Collection.CollectionAdjustmentReasonTxt ?? "";
              UseFnCabUpdateDebtDtlStatus();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            switch(AsChar(entities.Collection.AppliedToCode))
            {
              case 'G':
                // *** If the collection was applied to gift do not update the  
                // debt detail.***
                break;
              case 'I':
                // *** If the collection was applied to interest, back off the 
                // collection amount from the interest balance ***
                local.DebtDetail.InterestBalanceDueAmt =
                  local.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() + entities
                  .Collection.Amount;

                break;
              default:
                ReadObligationType();

                // *** If the collection is of a voluntary obligation type do 
                // not back off the collection amount from the debt balance
                // amount ***
                if (AsChar(entities.ObligationType.Classification) != 'V')
                {
                  local.DebtDetail.BalanceDueAmt += entities.Collection.Amount;
                }

                break;
            }

            local.DebtDetail.RetiredDt = local.Null1.Date;
            local.DebtDetail.LastUpdatedBy = import.ProgramProcessingInfo.Name;
            local.DebtDetail.LastUpdatedTmst = local.Current.Timestamp;
            UseFnCabUpdateDebtDetail();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (ReadCollectionAdjustmentReason())
            {
              try
              {
                UpdateCollection();

                // Continue
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_COLLECTION_NU";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_COLLECTION_PV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              // ****************************************************************************************
              // Adjust the URA amount after successful adjustment of the
              // collection.
              // **************************************************************************************
              if (AsChar(entities.Collection.ConcurrentInd) != 'Y')
              {
                if (Equal(entities.Collection.ProgramAppliedTo, "FC") || Equal
                  (entities.Collection.ProgramAppliedTo, "AF"))
                {
                  UseFnUpdateUraAmount();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }
              }
            }
            else
            {
              ExitState = "FN0000_COLL_ADJUST_REASON_NF";

              return;
            }
          }
          else
          {
            ExitState = "FN0000_OBLIG_TRANS_NF";

            return;
          }
        }

        if (import.CashReceiptDetail.DistributedAmount.GetValueOrDefault() == local
          .Revised.DistributedAmount.GetValueOrDefault())
        {
        }
        else
        {
          // ----------------------------
          // Only if the reason code entered is 'WR ACCT' or 'COURTAD' the 
          // refunded amount in the Cash Receipt detail is to be set to 0 else
          // leave it the same.
          // ----------------------------
          // P.Phinney  Collection_Adjustment_Reason Codes are as follows:   5 
          // = "WR ACT",
          // 
          // 7 = "BAD CHK",  9 = "ST PMT",  24 = "COURTAD",  33 = "KPC ADJ"
          // ----------------------------
          // 02/01/01    P.Phinney    Add logic for "KPC ADJ" (33) 
          // collection_adjustment_reason Code.
          if (import.CashReceiptDetail.RefundedAmount.GetValueOrDefault() != 0
            && (import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 5
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 7
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 9
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 33
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 24
            ))
          {
            local.Refund.RefundedAmount = 0;
          }
          else
          {
            local.Refund.RefundedAmount =
              import.CashReceiptDetail.RefundedAmount.GetValueOrDefault();
          }

          // ----------------------------
          // Only if the reason code entered is 'COURTAD', 'BAD CK', 'ST PMT' or
          // 'REIPADJ' (for all these the cash receipt detail is set to
          // adjusted status 'ADJ') the collection_amt_fully_applied_ind is set
          // to Y. In all other cases it is to be set to spaces.
          // ----------------------------
          // P.Phinney  Collection_Adjustment_Reason Codes are as follows:   30
          // = "REIPADJ",
          // 
          // 7 = "BAD CHK",  9 = "ST PMT",  24 = "COURTAD",  33 = "KPC ADJ"
          // ----------------------------
          // 02/01/01    P.Phinney    Add logic for "KPC ADJ" (33) 
          // collection_adjustment_reason Code.
          if (import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 7
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 9
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 24
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 33
            || import.CollectionAdjustmentReason.SystemGeneratedIdentifier == 30
            )
          {
            local.CollectionAppliedTo.Flag = "Y";
          }
          else
          {
            local.CollectionAppliedTo.Flag = "";
          }

          try
          {
            UpdateCashReceiptDetail();
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

          // ----------------------------
          // N.Engoor  -  04/09/1999
          // Created a Cash Receipt Detail History record for audit purposes.
          // ----------------------------
          try
          {
            CreateCashReceiptDetailHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        // ------------------------------------
        // N.Engoor - 01/20/99
        // Set the Cash Receipt Detail Status to "Adjusted"  if the reason code 
        // is   'BAD CK', 'ST PMT', 'COURTADJ' or 'REIPADJ.
        // If the reason code is 'WR ACCT' or 'WR AMT' set it to "Recorded".
        // If the reason code is 'WR DDTL' or 'REFUND' set it to "Released".
        // History must be inactivated and a new Status History must be created.
        // Use CABs to set the system generated identifier and the maximum
        // discontinued date.
        // ------------------------------------
        try
        {
          UpdateCashReceiptDetailStatHistory();

          if (ReadCashReceiptDetailStatus())
          {
            try
            {
              CreateCashReceiptDetailStatHistory();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
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
          else
          {
            ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
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
      else
      {
        ExitState = "FN0064_CASH_RCPT_DTL_STAT_HST_NF";
      }
    }
    else
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.AdcDt = source.AdcDt;
    target.RetiredDt = source.RetiredDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private void UseFnCabUpdateDebtDetail()
  {
    var useImport = new FnCabUpdateDebtDetail.Import();
    var useExport = new FnCabUpdateDebtDetail.Export();

    useImport.Persistent.Assign(entities.DebtDetail);
    useImport.DebtDetail.Assign(local.DebtDetail);

    Call(FnCabUpdateDebtDetail.Execute, useImport, useExport);

    MoveDebtDetail(useImport.Persistent, entities.DebtDetail);
  }

  private void UseFnCabUpdateDebtDtlStatus()
  {
    var useImport = new FnCabUpdateDebtDtlStatus.Import();
    var useExport = new FnCabUpdateDebtDtlStatus.Export();

    useImport.Persistent.Assign(entities.DebtDetail);
    useImport.Max.Date = import.Max.Date;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.DebtDetailStatusHistory.Assign(local.DebtDetailStatusHistory);

    Call(FnCabUpdateDebtDtlStatus.Execute, useImport, useExport);

    MoveDebtDetail(useImport.Persistent, entities.DebtDetail);
  }

  private void UseFnUpdateUraAmount()
  {
    var useImport = new FnUpdateUraAmount.Import();
    var useExport = new FnUpdateUraAmount.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    MoveCollection(entities.Collection, useImport.Collection);

    Call(FnUpdateUraAmount.Execute, useImport, useExport);
  }

  private void CreateCashReceiptDetailHistory()
  {
    var lastUpdatedTmst = local.CreateCashRecptDtlHist.LastUpdatedTmst;
    var interfaceTransId = local.CreateCashRecptDtlHist.InterfaceTransId ?? "";
    var offsetTaxid =
      local.CreateCashRecptDtlHist.OffsetTaxid.GetValueOrDefault();
    var jointReturnInd = local.CreateCashRecptDtlHist.JointReturnInd ?? "";
    var jointReturnName = local.CreateCashRecptDtlHist.JointReturnName ?? "";
    var refundedAmount = local.Refund.RefundedAmount.GetValueOrDefault();
    var distributedAmount = local.Revised.DistributedAmount.GetValueOrDefault();
    var adjustmentInd = local.CreateCashRecptDtlHist.AdjustmentInd ?? "";
    var sequentialIdentifier =
      local.CreateCashRecptDtlHist.SequentialIdentifier;
    var attribute2SupportedPersonFirstName =
      local.CreateCashRecptDtlHist.Attribute2SupportedPersonFirstName ?? "";
    var attribute2SupportedPersonLastName =
      local.CreateCashRecptDtlHist.Attribute2SupportedPersonLastName ?? "";
    var attribute2SupportedPersonMiddleName =
      local.CreateCashRecptDtlHist.Attribute2SupportedPersonMiddleName ?? "";
    var collectionTypeIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    var cashReceiptEventNumber =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    var cashReceiptNumber = import.CashReceipt.SequentialNumber;
    var collectionDate = local.CreateCashRecptDtlHist.CollectionDate;
    var obligorPersonNumber =
      local.CreateCashRecptDtlHist.ObligorPersonNumber ?? "";
    var courtOrderNumber =
      Substring(local.CreateCashRecptDtlHist.CourtOrderNumber, 1, 17);
    var caseNumber = local.CreateCashRecptDtlHist.CaseNumber ?? "";
    var obligorFirstName = local.CreateCashRecptDtlHist.ObligorFirstName ?? "";
    var obligorLastName = local.CreateCashRecptDtlHist.ObligorLastName ?? "";
    var obligorMiddleName = local.CreateCashRecptDtlHist.ObligorMiddleName ?? ""
      ;
    var obligorPhoneNumber =
      local.CreateCashRecptDtlHist.ObligorPhoneNumber ?? "";
    var obligorSocialSecurityNumber =
      local.CreateCashRecptDtlHist.ObligorSocialSecurityNumber ?? "";
    var offsetTaxYear =
      local.CreateCashRecptDtlHist.OffsetTaxYear.GetValueOrDefault();
    var defaultedCollectionDateInd =
      local.CreateCashRecptDtlHist.DefaultedCollectionDateInd ?? "";
    var multiPayor = local.CreateCashRecptDtlHist.MultiPayor ?? "";
    var receivedAmount = local.CreateCashRecptDtlHist.ReceivedAmount;
    var collectionAmount = local.CreateCashRecptDtlHist.CollectionAmount;
    var payeeFirstName = local.CreateCashRecptDtlHist.PayeeFirstName ?? "";
    var payeeMiddleName = local.CreateCashRecptDtlHist.PayeeMiddleName ?? "";
    var payeeLastName = local.CreateCashRecptDtlHist.PayeeLastName ?? "";
    var attribute1SupportedPersonFirstName =
      local.CreateCashRecptDtlHist.Attribute1SupportedPersonFirstName ?? "";
    var attribute1SupportedPersonMiddleName =
      local.CreateCashRecptDtlHist.Attribute1SupportedPersonMiddleName ?? "";
    var attribute1SupportedPersonLastName =
      local.CreateCashRecptDtlHist.Attribute1SupportedPersonLastName ?? "";
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTmst = local.CreateCashRecptDtlHist.CreatedTmst;
    var cashReceiptType1 = import.CashReceiptType.SystemGeneratedIdentifier;
    var cashReceiptSourceType1 =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    var reference = local.CreateCashRecptDtlHist.Reference ?? "";
    var notes = local.CreateCashRecptDtlHist.Notes ?? "";

    CheckValid<CashReceiptDetailHistory>("JointReturnInd", jointReturnInd);
    CheckValid<CashReceiptDetailHistory>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    CheckValid<CashReceiptDetailHistory>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetailHistory>("CollectionAmtFullyAppliedInd", "");
    entities.CashReceiptDetailHistory.Populated = false;
    Update("CreateCashReceiptDetailHistory",
      (db, command) =>
      {
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "interfaceTransId", interfaceTransId);
        db.SetNullableInt32(command, "offsetTaxid", offsetTaxid);
        db.SetNullableString(command, "jointReturnInd", jointReturnInd);
        db.SetNullableString(command, "jointReturnName", jointReturnName);
        db.SetNullableDecimal(command, "refundedAmount", refundedAmount);
        db.SetNullableDecimal(command, "distributedAmount", distributedAmount);
        db.SetNullableString(command, "adjustmentInd", adjustmentInd);
        db.SetNullableInt32(command, "crdetailHistId", sequentialIdentifier);
        db.SetNullableString(
          command, "suppPrsnFn2", attribute2SupportedPersonFirstName);
        db.SetNullableString(
          command, "suppPrsnLn2", attribute2SupportedPersonLastName);
        db.SetNullableString(
          command, "suppPrsnMn2", attribute2SupportedPersonMiddleName);
        db.SetInt32(command, "collctTypeId", collectionTypeIdentifier);
        db.SetInt32(command, "creventNbrId", cashReceiptEventNumber);
        db.SetInt32(command, "crNbrId", cashReceiptNumber);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "oblgorPersNbrId", obligorPersonNumber);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMiddleNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", obligorPhoneNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableInt32(command, "offsetTaxYear", offsetTaxYear);
        db.SetNullableString(
          command, "dfltCllctnDtInd", defaultedCollectionDateInd);
        db.SetNullableString(command, "multiPayor", multiPayor);
        db.SetNullableDecimal(command, "receivedAmount", receivedAmount);
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableString(command, "payeeFirstName", payeeFirstName);
        db.SetNullableString(command, "payeeMiddleName", payeeMiddleName);
        db.SetNullableString(command, "payeeLastName", payeeLastName);
        db.SetNullableString(
          command, "suppPrsnFn1", attribute1SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnMidNm1", attribute1SupportedPersonMiddleName);
        db.SetNullableString(
          command, "supPrsnLstNm1", attribute1SupportedPersonLastName);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetNullableString(command, "collectionAmtFul", "");
        db.SetInt32(command, "cashRecType", cashReceiptType1);
        db.SetInt32(command, "cashRecSrcType", cashReceiptSourceType1);
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
      });

    entities.CashReceiptDetailHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetailHistory.InterfaceTransId = interfaceTransId;
    entities.CashReceiptDetailHistory.OffsetTaxid = offsetTaxid;
    entities.CashReceiptDetailHistory.JointReturnInd = jointReturnInd;
    entities.CashReceiptDetailHistory.JointReturnName = jointReturnName;
    entities.CashReceiptDetailHistory.RefundedAmount = refundedAmount;
    entities.CashReceiptDetailHistory.DistributedAmount = distributedAmount;
    entities.CashReceiptDetailHistory.AdjustmentInd = adjustmentInd;
    entities.CashReceiptDetailHistory.SequentialIdentifier =
      sequentialIdentifier;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName =
      attribute2SupportedPersonFirstName;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonLastName =
      attribute2SupportedPersonLastName;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName =
      attribute2SupportedPersonMiddleName;
    entities.CashReceiptDetailHistory.CollectionTypeIdentifier =
      collectionTypeIdentifier;
    entities.CashReceiptDetailHistory.CashReceiptEventNumber =
      cashReceiptEventNumber;
    entities.CashReceiptDetailHistory.CashReceiptNumber = cashReceiptNumber;
    entities.CashReceiptDetailHistory.CollectionDate = collectionDate;
    entities.CashReceiptDetailHistory.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetailHistory.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetailHistory.CaseNumber = caseNumber;
    entities.CashReceiptDetailHistory.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetailHistory.ObligorLastName = obligorLastName;
    entities.CashReceiptDetailHistory.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetailHistory.ObligorPhoneNumber = obligorPhoneNumber;
    entities.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetailHistory.OffsetTaxYear = offsetTaxYear;
    entities.CashReceiptDetailHistory.DefaultedCollectionDateInd =
      defaultedCollectionDateInd;
    entities.CashReceiptDetailHistory.MultiPayor = multiPayor;
    entities.CashReceiptDetailHistory.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetailHistory.CollectionAmount = collectionAmount;
    entities.CashReceiptDetailHistory.PayeeFirstName = payeeFirstName;
    entities.CashReceiptDetailHistory.PayeeMiddleName = payeeMiddleName;
    entities.CashReceiptDetailHistory.PayeeLastName = payeeLastName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName =
      attribute1SupportedPersonFirstName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName =
      attribute1SupportedPersonMiddleName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonLastName =
      attribute1SupportedPersonLastName;
    entities.CashReceiptDetailHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailHistory.CreatedTmst = createdTmst;
    entities.CashReceiptDetailHistory.LastUpdatedBy = createdBy;
    entities.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd = "";
    entities.CashReceiptDetailHistory.CashReceiptType = cashReceiptType1;
    entities.CashReceiptDetailHistory.CashReceiptSourceType =
      cashReceiptSourceType1;
    entities.CashReceiptDetailHistory.Reference = reference;
    entities.CashReceiptDetailHistory.Notes = notes;
    entities.CashReceiptDetailHistory.Populated = true;
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
    var createdTimestamp = local.Current.Timestamp;
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

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 11);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 32);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.CashReceiptDetail.SuppPersonNoForVol =
          db.GetNullableString(reader, 39);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 41);
        entities.CashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 42);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.CashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 10);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 11);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 12);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.CashReceiptDetailStatus.CreatedBy = db.GetString(reader, 14);
        entities.CashReceiptDetailStatus.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.CashReceiptDetailStatus.LastUpdateBy =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetailStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 17);
        entities.CashReceiptDetailStatus.Description =
          db.GetNullableString(reader, 18);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.RecAdjStatus.SystemGeneratedIdentifier);
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

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
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
        entities.Collection.CreatedBy = db.GetString(reader, 20);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 21);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 22);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 23);
        entities.Collection.Amount = db.GetDecimal(reader, 24);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 25);
        entities.Collection.DistributionMethod = db.GetString(reader, 26);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 27);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 28);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 29);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 30);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 31);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 32);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 33);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 34);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 35);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 36);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 37);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 38);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 39);
        entities.Collection.AppliedToFuture = db.GetString(reader, 40);
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
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);

        return true;
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

  private bool ReadCsePersonObligor()
  {
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;

    return Read("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Obligor.LastUpdatedTmst = db.GetNullableDateTime(reader, 3);
        entities.Obligor.PgmChgEffectiveDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private bool ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDebtDetail",
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
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedBy = db.GetString(reader, 6);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 7);
        entities.Debt.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Debt.LastUpdatedTmst = db.GetNullableDateTime(reader, 9);
        entities.Debt.ZdelPreconversionReceiptNum =
          db.GetNullableInt64(reader, 10);
        entities.Debt.ZdelPreconversionIsn = db.GetNullableInt64(reader, 11);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 12);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 13);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 17);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 18);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 19);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 20);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 21);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 22);
        entities.DebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 23);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 24);
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(command, "obgId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(command, "otrId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.CreatedBy = db.GetString(reader, 3);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 6);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 8);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 9);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 10);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 12);
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Debt.OtyType);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = local.Current.Timestamp;
    var refundedAmount = local.Refund.RefundedAmount.GetValueOrDefault();
    var distributedAmount = local.Revised.DistributedAmount.GetValueOrDefault();
    var collectionAmtFullyAppliedInd = local.CollectionAppliedTo.Flag;

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "refundedAmt", refundedAmount);
        db.SetNullableDecimal(command, "distributedAmt", distributedAmount);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
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
    entities.CashReceiptDetail.RefundedAmount = refundedAmount;
    entities.CashReceiptDetail.DistributedAmount = distributedAmount;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
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

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var adjustedInd = "Y";
    var carId = entities.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    var collectionAdjustmentDt = import.Collection.CollectionAdjustmentDt;
    var collectionAdjProcessDate = local.Null1.Date;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = local.Current.Timestamp;
    var collectionAdjustmentReasonTxt =
      import.Collection.CollectionAdjustmentReasonTxt ?? "";

    CheckValid<Collection>("AdjustedInd", adjustedInd);
    entities.Collection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "adjInd", adjustedInd);
        db.SetNullableInt32(command, "carId", carId);
        db.SetDate(command, "collAdjDt", collectionAdjustmentDt);
        db.SetDate(command, "collAdjProcDate", collectionAdjProcessDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "colAdjRsnTxt", collectionAdjustmentReasonTxt);
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

    entities.Collection.AdjustedInd = adjustedInd;
    entities.Collection.CarId = carId;
    entities.Collection.CollectionAdjustmentDt = collectionAdjustmentDt;
    entities.Collection.CollectionAdjProcessDate = collectionAdjProcessDate;
    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.CollectionAdjustmentReasonTxt =
      collectionAdjustmentReasonTxt;
    entities.Collection.Populated = true;
  }

  private void UpdateObligor()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);

    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = local.Current.Timestamp;
    var pgmChgEffectiveDate = import.Obligor.PgmChgEffectiveDate;

    entities.Obligor.Populated = false;
    Update("UpdateObligor",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetString(command, "type", entities.Obligor.Type1);
      });

    entities.Obligor.LastUpdatedBy = lastUpdatedBy;
    entities.Obligor.LastUpdatedTmst = lastUpdatedTmst;
    entities.Obligor.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.Obligor.Populated = true;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of RecAdjStatus.
    /// </summary>
    [JsonPropertyName("recAdjStatus")]
    public CashReceiptDetailStatus RecAdjStatus
    {
      get => recAdjStatus ??= new();
      set => recAdjStatus = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private Common noCollectionsReversed;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonAccount obligor;
    private CashReceiptDetail cashReceiptDetail;
    private DateWorkArea max;
    private Collection collection;
    private CashReceiptDetailStatus recAdjStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NoCollectonReversd.
    /// </summary>
    [JsonPropertyName("noCollectonReversd")]
    public Common NoCollectonReversd
    {
      get => noCollectonReversd ??= new();
      set => noCollectonReversd = value;
    }

    private Common noCollectonReversd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CollectionAppliedTo.
    /// </summary>
    [JsonPropertyName("collectionAppliedTo")]
    public Common CollectionAppliedTo
    {
      get => collectionAppliedTo ??= new();
      set => collectionAppliedTo = value;
    }

    /// <summary>
    /// A value of CreateCashRecptDtlHist.
    /// </summary>
    [JsonPropertyName("createCashRecptDtlHist")]
    public CashReceiptDetail CreateCashRecptDtlHist
    {
      get => createCashRecptDtlHist ??= new();
      set => createCashRecptDtlHist = value;
    }

    /// <summary>
    /// A value of Refund.
    /// </summary>
    [JsonPropertyName("refund")]
    public CashReceiptDetail Refund
    {
      get => refund ??= new();
      set => refund = value;
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
    /// A value of Revised.
    /// </summary>
    [JsonPropertyName("revised")]
    public CashReceiptDetail Revised
    {
      get => revised ??= new();
      set => revised = value;
    }

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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private Common collectionAppliedTo;
    private CashReceiptDetail createCashRecptDtlHist;
    private CashReceiptDetail refund;
    private DateWorkArea current;
    private CashReceiptDetail revised;
    private DateWorkArea null1;
    private Collection collection;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private Obligation obligation;
    private ObligationType obligationType;
    private CashReceiptDetailHistory cashReceiptDetailHistory;
    private CollectionType collectionType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Collection collection;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }
#endregion
}
