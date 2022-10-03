// Program: FN_LCDA_BUILD_DISPLAY_LIST, ID: 373392213, model: 746.
// Short name: SWE02739
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_LCDA_BUILD_DISPLAY_LIST.
/// </summary>
[Serializable]
public partial class FnLcdaBuildDisplayList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LCDA_BUILD_DISPLAY_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLcdaBuildDisplayList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLcdaBuildDisplayList.
  /// </summary>
  public FnLcdaBuildDisplayList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************************************************
    // 08/27/07  G. Pan   PR313211
    // In the first condition check after read the case_receipt and 
    // cash_receipt_detail,
    // added new codes -
    // OR  (import_pg_cntl cash_receipt sequential_number IS EQUAL TO existing 
    // cash_receipt sequential_number
    // AND  existing cash_receipt_detail sequential_identifier IS LESS OR EQUAL 
    // TO import_pg_cntl cash_receipt_detail
    // sequential_identifier)
    // to replaced old code -
    // OR import_pg_cntl cash_receipt_detail sequential_identifier IS NOT EQUAL 
    // existing cash_receipt_detail sequential_identifier
    // *********************************************************************************
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Group.Index = -1;

    foreach(var item in ReadCashReceiptCashReceiptDetail())
    {
      if (AsChar(import.ShowHistory.Text1) == 'N')
      {
        if (AsChar(entities.ExistingCashReceiptDetail.AdjustmentInd) == 'Y')
        {
          continue;
        }
      }

      if (!IsEmpty(import.LegalAction.StandardNumber))
      {
        if (!IsEmpty(entities.ExistingCashReceiptDetail.CourtOrderNumber))
        {
          if (!Equal(entities.ExistingCashReceiptDetail.CourtOrderNumber,
            import.LegalAction.StandardNumber))
          {
            continue;
          }
        }
      }

      if (import.PgCntlCashReceipt.SequentialNumber != entities
        .ExistingCashReceipt.SequentialNumber || import
        .PgCntlCashReceipt.SequentialNumber == entities
        .ExistingCashReceipt.SequentialNumber && entities
        .ExistingCashReceiptDetail.SequentialIdentifier <= import
        .PgCntlCashReceiptDetail.SequentialIdentifier)
      {
        ++local.CrdsReadCnt.Count;
      }

      if (local.HoldCashReceipt.SequentialNumber != entities
        .ExistingCashReceipt.SequentialNumber || local
        .HoldCashReceiptDetail.SequentialIdentifier != entities
        .ExistingCashReceiptDetail.SequentialIdentifier)
      {
        local.HoldCashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        local.HoldCashReceiptDetail.SequentialIdentifier =
          entities.ExistingCashReceiptDetail.SequentialIdentifier;

        if (local.CrdsReadCnt.Count >= Export.GroupGroup.Capacity)
        {
          break;
        }
      }

      if (local.Group.Index >= 0)
      {
        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.LineType.Text2 = "BL";

        if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
        {
          break;
        }
      }

      ++local.Group.Index;
      local.Group.CheckSize();

      local.Group.Update.LineType.Text2 = "CR";
      local.Group.Update.CashReceipt.SequentialNumber =
        entities.ExistingCashReceipt.SequentialNumber;
      local.Group.Update.CashReceiptDetail.Assign(
        entities.ExistingCashReceiptDetail);
      local.Group.Update.Process.Date =
        entities.ExistingCashReceipt.ReceiptDate;
      local.Group.Update.DtlRecId.TotalInteger =
        (long)entities.ExistingCashReceipt.SequentialNumber * 10000 + entities
        .ExistingCashReceiptDetail.SequentialIdentifier;
      UseFnAbConcatCrAndCrd();

      if (AsChar(entities.ExistingCashReceiptDetail.AdjustmentInd) == 'Y')
      {
        local.Group.Update.Pgm.Text6 = "ADJ";
      }

      if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
      {
        local.Group.Update.Pgm.Text6 =
          entities.ExistingCashReceiptDetailStatus.Code;
        local.Group.Update.PayeeRsn.Text10 =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId ?? Spaces
          (10);
      }
      else
      {
        local.Group.Update.PayeeRsn.Text10 = "*ERROR*";

        continue;
      }

      if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
      {
        break;
      }

      foreach(var item1 in ReadObligationObligationTypeCollectionCsePerson())
      {
        if (AsChar(import.ShowHistory.Text1) == 'N' && AsChar
          (entities.ExistingCollection.AdjustedInd) == 'Y')
        {
          continue;
        }

        if (!IsEmpty(import.LegalAction.StandardNumber))
        {
          if (!Equal(entities.ExistingCollection.CourtOrderAppliedTo,
            import.LegalAction.StandardNumber))
          {
            continue;
          }
        }

        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.LineType.Text2 = "CO";
        local.Group.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        local.Group.Update.CashReceiptDetail.Assign(
          entities.ExistingCashReceiptDetail);
        MoveObligation(entities.ExistingObligation,
          local.Group.Update.Obligation);
        local.Group.Update.ObligationType.
          Assign(entities.ExistingObligationType);
        local.Group.Update.Debt.SystemGeneratedIdentifier =
          entities.ExistingDebt.SystemGeneratedIdentifier;
        local.Group.Update.DebtDetail.DueDt = entities.ExistingDebtDetail.DueDt;
        MoveCollection(entities.ExistingCollection,
          local.Group.Update.Collection);
        local.Group.Update.DtlRecId.TotalInteger =
          entities.ExistingCollection.SystemGeneratedIdentifier;
        local.Group.Update.Process.Date =
          Date(entities.ExistingCollection.CreatedTmst);
        local.Group.Update.Amt.TotalCurrency =
          entities.ExistingCollection.Amount;
        MoveLegalAction(local.NullLegalAction, local.TmpLegalAction);

        if (!IsEmpty(entities.ExistingCollection.CourtOrderAppliedTo))
        {
          local.Group.Update.CrdCrComboNo.CrdCrCombo =
            entities.ExistingCollection.CourtOrderAppliedTo ?? Spaces(14);

          if (ReadLegalAction())
          {
            MoveLegalAction(entities.ExistingLegalAction,
              local.Group.Update.LegalAction);
            MoveLegalAction(entities.ExistingLegalAction, local.TmpLegalAction);
          }
          else
          {
            local.Group.Update.CrdCrComboNo.CrdCrCombo = "ER: NO CO FND";
          }
        }

        if (IsEmpty(entities.ExistingCollection.DistPgmStateAppldTo))
        {
          local.Group.Update.Pgm.Text6 =
            entities.ExistingCollection.ProgramAppliedTo;
        }
        else if (IsEmpty(Substring(
          entities.ExistingCollection.ProgramAppliedTo, 3, 1)))
        {
          local.Group.Update.Pgm.Text6 =
            Substring(entities.ExistingCollection.ProgramAppliedTo,
            Collection.ProgramAppliedTo_MaxLength, 1, 2) + "-" + entities
            .ExistingCollection.DistPgmStateAppldTo;
        }
        else
        {
          local.Group.Update.Pgm.Text6 =
            entities.ExistingCollection.ProgramAppliedTo + "-" + entities
            .ExistingCollection.DistPgmStateAppldTo;
        }

        if (AsChar(entities.ExistingObligationType.SupportedPersonReqInd) == 'Y'
          )
        {
          if (AsChar(entities.ExistingCollection.ConcurrentInd) == 'Y')
          {
            goto Test1;
          }

          if (Equal(entities.ExistingCollection.ProgramAppliedTo, "NA") || Equal
            (entities.ExistingCollection.ProgramAppliedTo, "NAI") || Equal
            (entities.ExistingCollection.ProgramAppliedTo, "AFI") || Equal
            (entities.ExistingCollection.ProgramAppliedTo, "FCI"))
          {
            if (IsEmpty(entities.ExistingCollection.ArNumber))
            {
              if (ReadCsePerson())
              {
                local.CsePersonsWorkSet.Number =
                  entities.ExistingObligee.Number;
              }
              else
              {
                local.CsePersonsWorkSet.Number = "*ERROR*";
              }
            }
            else
            {
              local.CsePersonsWorkSet.Number =
                entities.ExistingCollection.ArNumber ?? Spaces(10);
            }

            if (!IsEmpty(local.CsePersonsWorkSet.Number))
            {
              if (AsChar(import.TraceInd.Flag) == 'Y')
              {
                local.CsePersonsWorkSet.FormattedName = "TRACE MODE";
              }
              else
              {
                UseSiReadCsePerson();
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ALL_OK";
                local.CsePersonsWorkSet.FormattedName = "*UNAVAIL*";
              }

              local.Group.Update.PayeeRsn.Text10 =
                local.CsePersonsWorkSet.FormattedName;
            }
          }
          else if (Equal(entities.ExistingCollection.ProgramAppliedTo, "NC"))
          {
            local.Group.Update.PayeeRsn.Text10 = "JJA";
          }
          else
          {
            local.Group.Update.PayeeRsn.Text10 = "ST KANSAS";
          }
        }

Test1:

        if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
        {
          goto ReadEach;
        }

        if (Equal(entities.ExistingCollection.ProgramAppliedTo, "NA"))
        {
          if (ReadDisbursement1())
          {
            local.Group.Update.Amt.TotalCurrency =
              local.Group.Item.Amt.TotalCurrency - entities
              .ExistingDisbursement.Amount;

            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.LineType.Text2 = "CF";
            local.Group.Update.CashReceipt.SequentialNumber =
              entities.ExistingCashReceipt.SequentialNumber;
            local.Group.Update.CashReceiptDetail.Assign(
              entities.ExistingCashReceiptDetail);
            MoveCollection(entities.ExistingCollection,
              local.Group.Update.Collection);
            local.Group.Update.Amt.TotalCurrency =
              entities.ExistingDisbursement.Amount;
            MoveLegalAction(local.TmpLegalAction, local.Group.Update.LegalAction);
              
            local.Group.Update.PayeeRsn.Text10 = "CR FEE";
            local.TmpDateWorkArea.Timestamp =
              entities.ExistingCollection.CreatedTmst;
            local.TmpDtlRecId.TotalInteger = UseFnConvertTmstToNum();
            local.Group.Update.DtlRecId.TotalInteger =
              entities.ExistingCollection.SystemGeneratedIdentifier + local
              .TmpDtlRecId.TotalInteger + 1;

            if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
            {
              goto ReadEach;
            }
          }
          else
          {
            // : No CR Fee Found, Continue Processing.
          }
        }

        if (AsChar(entities.ExistingCollection.AdjustedInd) == 'Y')
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.LineType.Text2 = "CA";
          local.Group.Update.CashReceipt.SequentialNumber =
            entities.ExistingCashReceipt.SequentialNumber;
          local.Group.Update.CashReceiptDetail.Assign(
            entities.ExistingCashReceiptDetail);
          MoveObligation(entities.ExistingObligation,
            local.Group.Update.Obligation);
          local.Group.Update.ObligationType.Assign(
            entities.ExistingObligationType);
          local.Group.Update.Debt.SystemGeneratedIdentifier =
            entities.ExistingDebt.SystemGeneratedIdentifier;
          local.Group.Update.DebtDetail.DueDt =
            entities.ExistingDebtDetail.DueDt;
          MoveCollection(entities.ExistingCollection,
            local.Group.Update.Collection);
          local.TmpDateWorkArea.Timestamp =
            entities.ExistingCollection.CreatedTmst;
          local.TmpDtlRecId.TotalInteger = UseFnConvertTmstToNum();
          local.Group.Update.DtlRecId.TotalInteger =
            entities.ExistingCollection.SystemGeneratedIdentifier + local
            .TmpDtlRecId.TotalInteger;
          local.Group.Update.Process.Date =
            entities.ExistingCollection.CollectionAdjustmentDt;
          local.Group.Update.Amt.TotalCurrency =
            -local.Group.Item.Collection.Amount;
          local.Group.Update.CrdCrComboNo.CrdCrCombo =
            entities.ExistingCollection.CourtOrderAppliedTo ?? Spaces(14);
          MoveLegalAction(local.TmpLegalAction, local.Group.Update.LegalAction);

          if (IsEmpty(entities.ExistingCollection.DistPgmStateAppldTo))
          {
            local.Group.Update.Pgm.Text6 =
              entities.ExistingCollection.ProgramAppliedTo;
          }
          else if (IsEmpty(Substring(
            entities.ExistingCollection.ProgramAppliedTo, 3, 1)))
          {
            local.Group.Update.Pgm.Text6 =
              Substring(entities.ExistingCollection.ProgramAppliedTo,
              Collection.ProgramAppliedTo_MaxLength, 1, 2) + "-" + entities
              .ExistingCollection.DistPgmStateAppldTo;
          }
          else
          {
            local.Group.Update.Pgm.Text6 =
              entities.ExistingCollection.ProgramAppliedTo + "-" + entities
              .ExistingCollection.DistPgmStateAppldTo;
          }

          if (ReadCollectionAdjustmentReason())
          {
            local.Group.Update.PayeeRsn.Text10 =
              entities.ExistingCollectionAdjustmentReason.Code;
          }
          else
          {
            local.Group.Update.PayeeRsn.Text10 = "*UNKNOWN*";
          }

          if (ReadRecovery())
          {
            local.Group.Update.RcvryExists.Text1 = "Y";
          }
          else
          {
            // : No Recovery Found for Collection - Continue Processing.
          }

          if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
          {
            goto ReadEach;
          }

          if (Equal(local.Group.Item.Collection.ProgramAppliedTo, "NA"))
          {
            if (ReadDisbursement2())
            {
              local.Group.Update.Amt.TotalCurrency =
                local.Group.Item.Amt.TotalCurrency - entities
                .ExistingDisbursement.Amount;

              ++local.Group.Index;
              local.Group.CheckSize();

              local.Group.Update.LineType.Text2 = "CF";
              local.Group.Update.CashReceipt.SequentialNumber =
                entities.ExistingCashReceipt.SequentialNumber;
              local.Group.Update.CashReceiptDetail.Assign(
                entities.ExistingCashReceiptDetail);
              MoveCollection(entities.ExistingCollection,
                local.Group.Update.Collection);
              local.Group.Update.Amt.TotalCurrency =
                entities.ExistingDisbursement.Amount;
              MoveLegalAction(local.TmpLegalAction,
                local.Group.Update.LegalAction);
              local.Group.Update.PayeeRsn.Text10 = "CR FEE";
              local.TmpDateWorkArea.Timestamp =
                entities.ExistingCollection.CreatedTmst;
              local.TmpDtlRecId.TotalInteger = UseFnConvertTmstToNum();
              local.Group.Update.DtlRecId.TotalInteger =
                entities.ExistingCollection.SystemGeneratedIdentifier + local
                .TmpDtlRecId.TotalInteger + 2;

              if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
              {
                goto ReadEach;
              }
            }
            else
            {
              // : No CR Fee Found, Continue Processing.
            }
          }
        }
      }

      if (Lt(0, entities.ExistingCashReceiptDetail.RefundedAmount))
      {
        local.RefFndInd.Flag = "N";

        foreach(var item1 in ReadReceiptRefund())
        {
          local.RefFndInd.Flag = "Y";

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.LineType.Text2 = "RF";
          local.Group.Update.CashReceipt.SequentialNumber =
            entities.ExistingCashReceipt.SequentialNumber;
          local.Group.Update.CashReceiptDetail.Assign(
            entities.ExistingCashReceiptDetail);
          local.Group.Update.Pgm.Text6 = "REF";
          local.Group.Update.PayeeRsn.Text10 =
            entities.ExistingReceiptRefund.PayeeName ?? Spaces(10);
          local.Group.Update.Amt.TotalCurrency =
            entities.ExistingReceiptRefund.Amount;
          local.Group.Update.Process.Date =
            entities.ExistingReceiptRefund.RequestDate;
          local.TmpDateWorkArea.Timestamp =
            entities.ExistingReceiptRefund.CreatedTimestamp;
          local.Group.Update.DtlRecId.TotalInteger = UseFnConvertTmstToNum();

          if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
          {
            goto ReadEach;
          }
        }

        if (AsChar(local.RefFndInd.Flag) == 'N')
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.LineType.Text2 = "RF";
          local.Group.Update.CashReceipt.SequentialNumber =
            entities.ExistingCashReceipt.SequentialNumber;
          local.Group.Update.CashReceiptDetail.Assign(
            entities.ExistingCashReceiptDetail);
          local.Group.Update.Pgm.Text6 = "REF";
          local.Group.Update.PayeeRsn.Text10 = "RCPT ERROR";
          local.Group.Update.DtlRecId.TotalInteger = 1;

          if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
          {
            break;
          }
        }
      }

      if (IsEmpty(entities.ExistingCashReceiptDetail.
        CollectionAmtFullyAppliedInd))
      {
        if (AsChar(entities.ExistingCashReceiptDetail.AdjustmentInd) == 'Y')
        {
          goto Test2;
        }

        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.LineType.Text2 = "UD";
        local.Group.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        local.Group.Update.CashReceiptDetail.Assign(
          entities.ExistingCashReceiptDetail);
        local.Group.Update.Process.Date = local.NullDateWorkArea.Date;
        local.Group.Update.PayeeRsn.Text10 = "UNDIST";
        local.Group.Update.Amt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        local.Group.Update.DtlRecId.TotalInteger = 2;

        if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
        {
          break;
        }
      }

Test2:
      ;
    }

ReadEach:

    if (local.Group.IsEmpty)
    {
      return;
    }

    local.CrdPos.Index = -1;
    local.TmpStartPos.Count = 1;

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      if (Equal(local.Group.Item.LineType.Text2, "CR"))
      {
        local.TmpStartPos.Count = local.Group.Index + 2;

        continue;
      }

      if (Equal(local.Group.Item.LineType.Text2, "BL"))
      {
        if (local.Group.Index <= local.TmpStartPos.Count)
        {
          continue;
        }

        ++local.CrdPos.Index;
        local.CrdPos.CheckSize();

        local.CrdPos.Update.CrdPosStart.Count = local.TmpStartPos.Count;
        local.CrdPos.Update.CrdPosEnd.Count = local.Group.Index;
        local.TmpStartPos.Count = 0;

        if (local.CrdPos.Index + 1 == Local.CrdPosGroup.Capacity)
        {
          break;
        }
      }
    }

    local.Group.CheckIndex();

    if (local.TmpStartPos.Count > 0)
    {
      ++local.CrdPos.Index;
      local.CrdPos.CheckSize();

      local.CrdPos.Update.CrdPosStart.Count = local.TmpStartPos.Count;
      local.CrdPos.Update.CrdPosEnd.Count = local.Group.Count;
    }

    for(local.CrdPos.Index = 0; local.CrdPos.Index < local.CrdPos.Count; ++
      local.CrdPos.Index)
    {
      if (!local.CrdPos.CheckSize())
      {
        break;
      }

      do
      {
        local.KeepLooping.Flag = "N";
        local.Group.Index = local.CrdPos.Item.CrdPosStart.Count - 1;

        for(var limit = local.CrdPos.Item.CrdPosEnd.Count - 1; local
          .Group.Index < limit; ++local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          local.TestProcess.Date = local.Group.Item.Process.Date;

          ++local.Group.Index;
          local.Group.CheckSize();

          if (!Lt(local.TestProcess.Date, local.Group.Item.Process.Date))
          {
            --local.Group.Index;
            local.Group.CheckSize();

            continue;
          }

          local.KeepLooping.Flag = "Y";
          local.Sort1.Sort1LineType.Text2 = local.Group.Item.LineType.Text2;
          local.Sort1.Sort1CashReceipt.SequentialNumber =
            local.Group.Item.CashReceipt.SequentialNumber;
          local.Sort1.Sort1CashReceiptDetail.Assign(
            local.Group.Item.CashReceiptDetail);
          local.Sort1.Sort1Collection.Assign(local.Group.Item.Collection);
          local.Sort1.Sort1CrdCrComboNo.CrdCrCombo =
            local.Group.Item.CrdCrComboNo.CrdCrCombo;
          local.Sort1.Sort1Common.SelectChar =
            local.Group.Item.Common.SelectChar;
          MoveObligation(local.Group.Item.Obligation,
            local.Sort1.Sort1Obligation);
          local.Sort1.Sort1ObligationType.
            Assign(local.Group.Item.ObligationType);
          local.Sort1.Sort1Debt.SystemGeneratedIdentifier =
            local.Group.Item.Debt.SystemGeneratedIdentifier;
          local.Sort1.Sort1DebtDetail.DueDt = local.Group.Item.DebtDetail.DueDt;
          MoveLegalAction(local.Group.Item.LegalAction,
            local.Sort1.Sort1LegalAction);
          local.Sort1.Sort1Amt.TotalCurrency =
            local.Group.Item.Amt.TotalCurrency;
          local.Sort1.Sort1PayeeRsn.Text10 = local.Group.Item.PayeeRsn.Text10;
          local.Sort1.Sort1Pgm.Text6 = local.Group.Item.Pgm.Text6;
          local.Sort1.Sort1Process.Date = local.Group.Item.Process.Date;
          local.Sort1.Sort1RcvryExists.Text1 =
            local.Group.Item.RcvryExists.Text1;
          local.Sort1.Sort1DtlRecId.TotalInteger =
            local.Group.Item.DtlRecId.TotalInteger;

          --local.Group.Index;
          local.Group.CheckSize();

          local.Sort2.Sort2LineType.Text2 = local.Group.Item.LineType.Text2;
          local.Sort2.Sort2CashReceipt.SequentialNumber =
            local.Group.Item.CashReceipt.SequentialNumber;
          local.Sort2.Sort2CashReceiptDetail.Assign(
            local.Group.Item.CashReceiptDetail);
          local.Sort2.Sort2Collection.Assign(local.Group.Item.Collection);
          local.Sort2.Sort2CrdCrComboNo.CrdCrCombo =
            local.Group.Item.CrdCrComboNo.CrdCrCombo;
          local.Sort2.Sort2Common.SelectChar =
            local.Group.Item.Common.SelectChar;
          MoveObligation(local.Group.Item.Obligation,
            local.Sort2.Sort2Obligation);
          local.Sort2.Sort2ObligationType.
            Assign(local.Group.Item.ObligationType);
          local.Sort2.Sort2Debt.SystemGeneratedIdentifier =
            local.Group.Item.Debt.SystemGeneratedIdentifier;
          local.Sort2.Sort2DebtDetail.DueDt = local.Group.Item.DebtDetail.DueDt;
          MoveLegalAction(local.Group.Item.LegalAction,
            local.Sort2.Sort2LegalAction);
          local.Sort2.Sort2Amt.TotalCurrency =
            local.Group.Item.Amt.TotalCurrency;
          local.Sort2.Sort2PayeeRsn.Text10 = local.Group.Item.PayeeRsn.Text10;
          local.Sort2.Sort2Pgm.Text6 = local.Group.Item.Pgm.Text6;
          local.Sort2.Sort2Process.Date = local.Group.Item.Process.Date;
          local.Sort2.Sort2RcvryExists.Text1 =
            local.Group.Item.RcvryExists.Text1;
          local.Sort2.Sort2DtlRecId.TotalInteger =
            local.Group.Item.DtlRecId.TotalInteger;

          ++local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.LineType.Text2 = local.Sort2.Sort2LineType.Text2;
          local.Group.Update.CashReceipt.SequentialNumber =
            local.Sort2.Sort2CashReceipt.SequentialNumber;
          local.Group.Update.CashReceiptDetail.Assign(
            local.Sort2.Sort2CashReceiptDetail);
          local.Group.Update.Collection.Assign(local.Sort2.Sort2Collection);
          local.Group.Update.CrdCrComboNo.CrdCrCombo =
            local.Sort2.Sort2CrdCrComboNo.CrdCrCombo;
          local.Group.Update.Common.SelectChar =
            local.Sort2.Sort2Common.SelectChar;
          MoveObligation(local.Sort2.Sort2Obligation,
            local.Group.Update.Obligation);
          local.Group.Update.ObligationType.Assign(
            local.Sort2.Sort2ObligationType);
          local.Group.Update.Debt.SystemGeneratedIdentifier =
            local.Sort2.Sort2Debt.SystemGeneratedIdentifier;
          local.Group.Update.DebtDetail.DueDt =
            local.Sort2.Sort2DebtDetail.DueDt;
          MoveLegalAction(local.Sort2.Sort2LegalAction,
            local.Group.Update.LegalAction);
          local.Group.Update.Amt.TotalCurrency =
            local.Sort2.Sort2Amt.TotalCurrency;
          local.Group.Update.PayeeRsn.Text10 = local.Sort2.Sort2PayeeRsn.Text10;
          local.Group.Update.Pgm.Text6 = local.Sort2.Sort2Pgm.Text6;
          local.Group.Update.Process.Date = local.Sort2.Sort2Process.Date;
          local.Group.Update.RcvryExists.Text1 =
            local.Sort2.Sort2RcvryExists.Text1;
          local.Group.Update.DtlRecId.TotalInteger =
            local.Sort2.Sort2DtlRecId.TotalInteger;

          --local.Group.Index;
          local.Group.CheckSize();

          local.Group.Update.LineType.Text2 = local.Sort1.Sort1LineType.Text2;
          local.Group.Update.CashReceipt.SequentialNumber =
            local.Sort1.Sort1CashReceipt.SequentialNumber;
          local.Group.Update.CashReceiptDetail.Assign(
            local.Sort1.Sort1CashReceiptDetail);
          local.Group.Update.Collection.Assign(local.Sort1.Sort1Collection);
          local.Group.Update.CrdCrComboNo.CrdCrCombo =
            local.Sort1.Sort1CrdCrComboNo.CrdCrCombo;
          local.Group.Update.Common.SelectChar =
            local.Sort1.Sort1Common.SelectChar;
          MoveObligation(local.Sort1.Sort1Obligation,
            local.Group.Update.Obligation);
          local.Group.Update.ObligationType.Assign(
            local.Sort1.Sort1ObligationType);
          local.Group.Update.Debt.SystemGeneratedIdentifier =
            local.Sort1.Sort1Debt.SystemGeneratedIdentifier;
          local.Group.Update.DebtDetail.DueDt =
            local.Sort1.Sort1DebtDetail.DueDt;
          MoveLegalAction(local.Sort1.Sort1LegalAction,
            local.Group.Update.LegalAction);
          local.Group.Update.Amt.TotalCurrency =
            local.Sort1.Sort1Amt.TotalCurrency;
          local.Group.Update.PayeeRsn.Text10 = local.Sort1.Sort1PayeeRsn.Text10;
          local.Group.Update.Pgm.Text6 = local.Sort1.Sort1Pgm.Text6;
          local.Group.Update.Process.Date = local.Sort1.Sort1Process.Date;
          local.Group.Update.RcvryExists.Text1 =
            local.Sort1.Sort1RcvryExists.Text1;
          local.Group.Update.DtlRecId.TotalInteger =
            local.Sort1.Sort1DtlRecId.TotalInteger;
        }

        local.Group.CheckIndex();
      }
      while(AsChar(local.KeepLooping.Flag) != 'N');
    }

    local.CrdPos.CheckIndex();

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      if (Equal(local.Group.Item.LineType.Text2, "CR"))
      {
        continue;
      }

      local.Group.Update.CashReceiptDetail.CollectionAmount = 0;
    }

    local.Group.CheckIndex();
    local.TmpStartPos.Count = 1;

    for(var limit = local.Group.Count - 1; local.TmpStartPos.Count <= limit; ++
      local.TmpStartPos.Count)
    {
      local.Group.Index = local.TmpStartPos.Count - 1;
      local.Group.CheckSize();

      if (Equal(local.Group.Item.LineType.Text2, "DL") || Equal
        (local.Group.Item.LineType.Text2, "BL"))
      {
        continue;
      }

      local.Hold.HoldGrpLineType.Text2 = local.Group.Item.LineType.Text2;
      local.Hold.HoldCashReceipt.SequentialNumber =
        local.Group.Item.CashReceipt.SequentialNumber;
      local.Hold.HoldCashReceiptDetail.
        Assign(local.Group.Item.CashReceiptDetail);
      local.Hold.HoldCollection.Assign(local.Group.Item.Collection);
      MoveObligation(local.Group.Item.Obligation, local.Hold.HoldObligation);
      local.Hold.HoldGrpAmt.TotalCurrency = local.Group.Item.Amt.TotalCurrency;
      local.Hold.HoldGrpPayeeRsn.Text10 = local.Group.Item.PayeeRsn.Text10;
      local.Hold.HoldGrpPgm.Text6 = local.Group.Item.Pgm.Text6;
      local.Hold.HoldGrpProcess.Date = local.Group.Item.Process.Date;
      local.RowCombinedInd.Flag = "N";

      for(local.Group.Index = local.TmpStartPos.Count; local.Group.Index < local
        .Group.Count; ++local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (Equal(local.Group.Item.LineType.Text2, "DL") || Equal
          (local.Group.Item.LineType.Text2, "BL"))
        {
          continue;
        }

        if (local.Group.Item.CashReceipt.SequentialNumber != local
          .Hold.HoldCashReceipt.SequentialNumber || local
          .Group.Item.CashReceiptDetail.SequentialIdentifier != local
          .Hold.HoldCashReceiptDetail.SequentialIdentifier)
        {
          break;
        }

        if (local.Group.Item.Obligation.SystemGeneratedIdentifier == local
          .Hold.HoldObligation.SystemGeneratedIdentifier && Equal
          (local.Group.Item.PayeeRsn.Text10, local.Hold.HoldGrpPayeeRsn.Text10) &&
          Equal(local.Group.Item.Pgm.Text6, local.Hold.HoldGrpPgm.Text6) && Equal
          (local.Group.Item.LineType.Text2, local.Hold.HoldGrpLineType.Text2))
        {
          if (Equal(local.Group.Item.LineType.Text2, "CF"))
          {
            if (local.Group.Item.Amt.TotalCurrency < 0 && local
              .Hold.HoldGrpAmt.TotalCurrency > 0)
            {
              goto Test3;
            }

            if (local.Group.Item.Amt.TotalCurrency > 0 && local
              .Hold.HoldGrpAmt.TotalCurrency < 0)
            {
              goto Test3;
            }
          }
          else if (!Equal(local.Group.Item.Process.Date,
            local.Hold.HoldGrpProcess.Date))
          {
            goto Test3;
          }

          local.RowCombinedInd.Flag = "Y";
          local.Group.Update.LineType.Text2 = "DL";
          local.Hold.HoldGrpAmt.TotalCurrency += local.Group.Item.Amt.
            TotalCurrency;
        }

Test3:
        ;
      }

      local.Group.CheckIndex();

      if (AsChar(local.RowCombinedInd.Flag) == 'N')
      {
        continue;
      }

      local.Group.Index = local.TmpStartPos.Count - 1;
      local.Group.CheckSize();

      local.Group.Update.Amt.TotalCurrency =
        local.Hold.HoldGrpAmt.TotalCurrency;
    }

    if (import.PgCntlCashReceipt.SequentialNumber == 999999999)
    {
      local.CopyNow.Flag = "Y";
    }
    else
    {
      local.CopyNow.Flag = "N";
    }

    export.Group.Index = -1;

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      if (Equal(local.Group.Item.LineType.Text2, "DL"))
      {
        continue;
      }

      if (!Equal(local.Group.Item.LineType.Text2, "BL"))
      {
        export.PgCntlCashReceipt.SequentialNumber =
          local.Group.Item.CashReceipt.SequentialNumber;
        MoveCashReceiptDetail(local.Group.Item.CashReceiptDetail,
          export.PgCntlCashReceiptDetail);
        export.PgCntlDtlRecId.TotalInteger =
          local.Group.Item.DtlRecId.TotalInteger;
      }

      if (AsChar(local.CopyNow.Flag) == 'N')
      {
        if (import.PgCntlCashReceipt.SequentialNumber == local
          .Group.Item.CashReceipt.SequentialNumber && import
          .PgCntlCashReceiptDetail.SequentialIdentifier == local
          .Group.Item.CashReceiptDetail.SequentialIdentifier && import
          .PgCntlDtlRecId.TotalInteger == local
          .Group.Item.DtlRecId.TotalInteger)
        {
          local.CopyNow.Flag = "Y";
        }

        continue;
      }

      if (export.Group.Index == -1 && Equal
        (local.Group.Item.LineType.Text2, "BL"))
      {
        continue;
      }

      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.LineType.Text2 = local.Group.Item.LineType.Text2;
      export.Group.Update.CashReceipt.SequentialNumber =
        local.Group.Item.CashReceipt.SequentialNumber;
      export.Group.Update.CashReceiptDetail.Assign(
        local.Group.Item.CashReceiptDetail);
      export.Group.Update.Collection.Assign(local.Group.Item.Collection);
      export.Group.Update.CrdCrComboNo.CrdCrCombo =
        local.Group.Item.CrdCrComboNo.CrdCrCombo;
      export.Group.Update.Common.SelectChar =
        local.Group.Item.Common.SelectChar;
      MoveObligation(local.Group.Item.Obligation, export.Group.Update.Obligation);
        
      export.Group.Update.ObligationType.
        Assign(local.Group.Item.ObligationType);
      export.Group.Update.Debt.SystemGeneratedIdentifier =
        local.Group.Item.Debt.SystemGeneratedIdentifier;
      export.Group.Update.DebtDetail.DueDt = local.Group.Item.DebtDetail.DueDt;
      MoveLegalAction(local.Group.Item.LegalAction,
        export.Group.Update.LegalAction);
      export.Group.Update.Amt.TotalCurrency =
        local.Group.Item.Amt.TotalCurrency;
      export.Group.Update.PayeeRsn.Text10 = local.Group.Item.PayeeRsn.Text10;
      export.Group.Update.Pgm.Text6 = local.Group.Item.Pgm.Text6;
      export.Group.Update.Process.Date = local.Group.Item.Process.Date;
      export.Group.Update.RcvryExists.Text1 =
        local.Group.Item.RcvryExists.Text1;
      export.Group.Update.DtlRecId.TotalInteger =
        local.Group.Item.DtlRecId.TotalInteger;

      if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
      {
        return;
      }
    }

    local.Group.CheckIndex();
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionDate = source.CollectionDate;
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
    target.AppliedToOrderTypeCode = source.AppliedToOrderTypeCode;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
    target.AppliedToFuture = source.AppliedToFuture;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
    target.ArNumber = source.ArNumber;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.ExistingCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.ExistingCashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    local.Group.Update.CrdCrComboNo.CrdCrCombo =
      useExport.CrdCrComboNo.CrdCrCombo;
  }

  private long UseFnConvertTmstToNum()
  {
    var useImport = new FnConvertTmstToNum.Import();
    var useExport = new FnConvertTmstToNum.Export();

    useImport.DateWorkArea.Timestamp = local.TmpDateWorkArea.Timestamp;

    Call(FnConvertTmstToNum.Execute, useImport, useExport);

    return useExport.Common.TotalInteger;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetail()
  {
    entities.ExistingCashReceipt.Populated = false;
    entities.ExistingCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.Obligor.Number);
        db.SetDate(
          command, "date1", import.CollectionFrom.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.CollectionTo.Date.GetValueOrDefault());
        db.SetDate(
          command, "collectionDate",
          import.PgCntlCashReceiptDetail.CollectionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 13);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatusCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCashReceiptDetailStatus.Populated = false;
    entities.ExistingCashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatusCashReceiptDetailStatHistory",
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
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.ExistingCollection.CarId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollectionAdjustmentReason.Code =
          db.GetString(reader, 1);
        entities.ExistingCollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingObligee.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.
          SetNullableInt32(command, "otyId", entities.ExistingCollection.OtyId);
          
        db.
          SetNullableInt32(command, "obgId", entities.ExistingCollection.ObgId);
          
        db.SetNullableString(
          command, "cspNumberDisb", entities.ExistingCollection.CspNumber);
        db.SetNullableString(
          command, "cpaTypeDisb", entities.ExistingCollection.CpaType);
        db.
          SetNullableInt32(command, "otrId", entities.ExistingCollection.OtrId);
          
        db.SetNullableString(
          command, "otrTypeDisb", entities.ExistingCollection.OtrType);
        db.SetNullableInt32(
          command, "crtId", entities.ExistingCollection.CrtType);
        db.
          SetNullableInt32(command, "cstId", entities.ExistingCollection.CstId);
          
        db.
          SetNullableInt32(command, "crvId", entities.ExistingCollection.CrvId);
          
        db.
          SetNullableInt32(command, "crdId", entities.ExistingCollection.CrdId);
          
      },
      (db, reader) =>
      {
        entities.ExistingObligee.Number = db.GetString(reader, 0);
        entities.ExistingObligee.Populated = true;
      });
  }

  private bool ReadDisbursement1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingDisbursement.Populated = false;

    return Read("ReadDisbursement1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.
          SetNullableInt32(command, "otyId", entities.ExistingCollection.OtyId);
          
        db.
          SetNullableInt32(command, "obgId", entities.ExistingCollection.ObgId);
          
        db.SetNullableString(
          command, "cspNumberDisb", entities.ExistingCollection.CspNumber);
        db.SetNullableString(
          command, "cpaTypeDisb", entities.ExistingCollection.CpaType);
        db.
          SetNullableInt32(command, "otrId", entities.ExistingCollection.OtrId);
          
        db.SetNullableString(
          command, "otrTypeDisb", entities.ExistingCollection.OtrType);
        db.SetNullableInt32(
          command, "crtId", entities.ExistingCollection.CrtType);
        db.
          SetNullableInt32(command, "cstId", entities.ExistingCollection.CstId);
          
        db.
          SetNullableInt32(command, "crvId", entities.ExistingCollection.CrvId);
          
        db.
          SetNullableInt32(command, "crdId", entities.ExistingCollection.CrdId);
          
      },
      (db, reader) =>
      {
        entities.ExistingDisbursement.CpaType = db.GetString(reader, 0);
        entities.ExistingDisbursement.CspNumber = db.GetString(reader, 1);
        entities.ExistingDisbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDisbursement.Amount = db.GetDecimal(reader, 3);
        entities.ExistingDisbursement.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingDisbursement.DbtGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingDisbursement.PrqGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingDisbursement.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingDisbursement.CpaType);
      });
  }

  private bool ReadDisbursement2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingDisbursement.Populated = false;

    return Read("ReadDisbursement2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.
          SetNullableInt32(command, "otyId", entities.ExistingCollection.OtyId);
          
        db.
          SetNullableInt32(command, "obgId", entities.ExistingCollection.ObgId);
          
        db.SetNullableString(
          command, "cspNumberDisb", entities.ExistingCollection.CspNumber);
        db.SetNullableString(
          command, "cpaTypeDisb", entities.ExistingCollection.CpaType);
        db.
          SetNullableInt32(command, "otrId", entities.ExistingCollection.OtrId);
          
        db.SetNullableString(
          command, "otrTypeDisb", entities.ExistingCollection.OtrType);
        db.SetNullableInt32(
          command, "crtId", entities.ExistingCollection.CrtType);
        db.
          SetNullableInt32(command, "cstId", entities.ExistingCollection.CstId);
          
        db.
          SetNullableInt32(command, "crvId", entities.ExistingCollection.CrvId);
          
        db.
          SetNullableInt32(command, "crdId", entities.ExistingCollection.CrdId);
          
      },
      (db, reader) =>
      {
        entities.ExistingDisbursement.CpaType = db.GetString(reader, 0);
        entities.ExistingDisbursement.CspNumber = db.GetString(reader, 1);
        entities.ExistingDisbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDisbursement.Amount = db.GetDecimal(reader, 3);
        entities.ExistingDisbursement.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingDisbursement.DbtGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingDisbursement.PrqGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingDisbursement.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingDisbursement.CpaType);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo",
          entities.ExistingCollection.CourtOrderAppliedTo ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTypeCollectionCsePerson()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingObligor1.Populated = false;
    entities.ExistingCollection.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligationType.Populated = false;
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadObligationObligationTypeCollectionCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetInt32(
          command, "crdId",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvId", entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstId", entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtType", entities.ExistingCashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingDebt.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligor1.Number = db.GetString(reader, 1);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ExistingObligationType.Code = db.GetString(reader, 6);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 7);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 8);
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.ExistingCollection.AppliedToCode = db.GetString(reader, 10);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 11);
        entities.ExistingCollection.DisbursementDt =
          db.GetNullableDate(reader, 12);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 13);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 14);
        entities.ExistingCollection.DisbursementAdjProcessDate =
          db.GetDate(reader, 15);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 16);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 17);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 18);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 19);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 20);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 20);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 21);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 21);
        entities.ExistingCollection.CpaType = db.GetString(reader, 22);
        entities.ExistingDebt.CpaType = db.GetString(reader, 22);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 23);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 23);
        entities.ExistingCollection.OtrType = db.GetString(reader, 24);
        entities.ExistingDebt.Type1 = db.GetString(reader, 24);
        entities.ExistingCollection.CarId = db.GetNullableInt32(reader, 25);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 26);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 26);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 27);
        entities.ExistingCollection.CollectionAdjProcessDate =
          db.GetDate(reader, 28);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 29);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 30);
        entities.ExistingCollection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 31);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 32);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 33);
        entities.ExistingCollection.AppliedToOrderTypeCode =
          db.GetString(reader, 34);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 35);
        entities.ExistingCollection.AppliedToFuture = db.GetString(reader, 36);
        entities.ExistingCollection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 37);
        entities.ExistingCollection.ArNumber = db.GetNullableString(reader, 38);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 39);
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 39);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 40);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 40);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 41);
        entities.ExistingDebt.CpaType = db.GetString(reader, 41);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 42);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 42);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 43);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 43);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 44);
        entities.ExistingDebt.Type1 = db.GetString(reader, 44);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 45);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 46);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 47);
        entities.ExistingObligor1.Populated = true;
        entities.ExistingCollection.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
        CheckValid<Collection>("AppliedToCode",
          entities.ExistingCollection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.ExistingCollection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.ExistingCollection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.ExistingCollection.AppliedToOrderTypeCode);
        CheckValid<Collection>("AppliedToFuture",
          entities.ExistingCollection.AppliedToFuture);
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingReceiptRefund.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingReceiptRefund.PayeeName =
          db.GetNullableString(reader, 1);
        entities.ExistingReceiptRefund.Amount = db.GetDecimal(reader, 2);
        entities.ExistingReceiptRefund.RequestDate = db.GetDate(reader, 3);
        entities.ExistingReceiptRefund.CrvIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingReceiptRefund.CrdIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingReceiptRefund.CrtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingReceiptRefund.CstIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingReceiptRefund.Populated = true;

        return true;
      });
  }

  private bool ReadRecovery()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingRecovery.Populated = false;

    return Read("ReadRecovery",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.
          SetNullableInt32(command, "otyId", entities.ExistingCollection.OtyId);
          
        db.
          SetNullableInt32(command, "obgId", entities.ExistingCollection.ObgId);
          
        db.SetNullableString(
          command, "cspNumberDisb", entities.ExistingCollection.CspNumber);
        db.SetNullableString(
          command, "cpaTypeDisb", entities.ExistingCollection.CpaType);
        db.
          SetNullableInt32(command, "otrId", entities.ExistingCollection.OtrId);
          
        db.SetNullableString(
          command, "otrTypeDisb", entities.ExistingCollection.OtrType);
        db.SetNullableInt32(
          command, "crtId", entities.ExistingCollection.CrtType);
        db.
          SetNullableInt32(command, "cstId", entities.ExistingCollection.CstId);
          
        db.
          SetNullableInt32(command, "crvId", entities.ExistingCollection.CrvId);
          
        db.
          SetNullableInt32(command, "crdId", entities.ExistingCollection.CrdId);
          
      },
      (db, reader) =>
      {
        entities.ExistingRecovery.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingRecovery.PrqRGeneratedId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingRecovery.Populated = true;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of CollectionFrom.
    /// </summary>
    [JsonPropertyName("collectionFrom")]
    public DateWorkArea CollectionFrom
    {
      get => collectionFrom ??= new();
      set => collectionFrom = value;
    }

    /// <summary>
    /// A value of CollectionTo.
    /// </summary>
    [JsonPropertyName("collectionTo")]
    public DateWorkArea CollectionTo
    {
      get => collectionTo ??= new();
      set => collectionTo = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public TextWorkArea ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of PgCntlCashReceipt.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceipt")]
    public CashReceipt PgCntlCashReceipt
    {
      get => pgCntlCashReceipt ??= new();
      set => pgCntlCashReceipt = value;
    }

    /// <summary>
    /// A value of PgCntlCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceiptDetail")]
    public CashReceiptDetail PgCntlCashReceiptDetail
    {
      get => pgCntlCashReceiptDetail ??= new();
      set => pgCntlCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PgCntlDtlRecId.
    /// </summary>
    [JsonPropertyName("pgCntlDtlRecId")]
    public Common PgCntlDtlRecId
    {
      get => pgCntlDtlRecId ??= new();
      set => pgCntlDtlRecId = value;
    }

    /// <summary>
    /// A value of TraceInd.
    /// </summary>
    [JsonPropertyName("traceInd")]
    public Common TraceInd
    {
      get => traceInd ??= new();
      set => traceInd = value;
    }

    private CsePerson obligor;
    private LegalAction legalAction;
    private DateWorkArea collectionFrom;
    private DateWorkArea collectionTo;
    private TextWorkArea showHistory;
    private CashReceipt pgCntlCashReceipt;
    private CashReceiptDetail pgCntlCashReceiptDetail;
    private Common pgCntlDtlRecId;
    private Common traceInd;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of LineType.
      /// </summary>
      [JsonPropertyName("lineType")]
      public TextWorkArea LineType
      {
        get => lineType ??= new();
        set => lineType = value;
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
      /// A value of CrdCrComboNo.
      /// </summary>
      [JsonPropertyName("crdCrComboNo")]
      public CrdCrComboNo CrdCrComboNo
      {
        get => crdCrComboNo ??= new();
        set => crdCrComboNo = value;
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
      /// A value of Process.
      /// </summary>
      [JsonPropertyName("process")]
      public DateWorkArea Process
      {
        get => process ??= new();
        set => process = value;
      }

      /// <summary>
      /// A value of PayeeRsn.
      /// </summary>
      [JsonPropertyName("payeeRsn")]
      public TextWorkArea PayeeRsn
      {
        get => payeeRsn ??= new();
        set => payeeRsn = value;
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
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
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
      /// A value of Amt.
      /// </summary>
      [JsonPropertyName("amt")]
      public Common Amt
      {
        get => amt ??= new();
        set => amt = value;
      }

      /// <summary>
      /// A value of Pgm.
      /// </summary>
      [JsonPropertyName("pgm")]
      public WorkArea Pgm
      {
        get => pgm ??= new();
        set => pgm = value;
      }

      /// <summary>
      /// A value of RcvryExists.
      /// </summary>
      [JsonPropertyName("rcvryExists")]
      public TextWorkArea RcvryExists
      {
        get => rcvryExists ??= new();
        set => rcvryExists = value;
      }

      /// <summary>
      /// A value of DtlRecId.
      /// </summary>
      [JsonPropertyName("dtlRecId")]
      public Common DtlRecId
      {
        get => dtlRecId ??= new();
        set => dtlRecId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common common;
      private TextWorkArea lineType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private CrdCrComboNo crdCrComboNo;
      private Collection collection;
      private DateWorkArea process;
      private TextWorkArea payeeRsn;
      private Obligation obligation;
      private ObligationType obligationType;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private Common amt;
      private WorkArea pgm;
      private TextWorkArea rcvryExists;
      private Common dtlRecId;
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
    /// A value of PgCntlCashReceipt.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceipt")]
    public CashReceipt PgCntlCashReceipt
    {
      get => pgCntlCashReceipt ??= new();
      set => pgCntlCashReceipt = value;
    }

    /// <summary>
    /// A value of PgCntlCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("pgCntlCashReceiptDetail")]
    public CashReceiptDetail PgCntlCashReceiptDetail
    {
      get => pgCntlCashReceiptDetail ??= new();
      set => pgCntlCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PgCntlDtlRecId.
    /// </summary>
    [JsonPropertyName("pgCntlDtlRecId")]
    public Common PgCntlDtlRecId
    {
      get => pgCntlDtlRecId ??= new();
      set => pgCntlDtlRecId = value;
    }

    private Array<GroupGroup> group;
    private CashReceipt pgCntlCashReceipt;
    private CashReceiptDetail pgCntlCashReceiptDetail;
    private Common pgCntlDtlRecId;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CrdPosGroup group.</summary>
    [Serializable]
    public class CrdPosGroup
    {
      /// <summary>
      /// A value of CrdPosStart.
      /// </summary>
      [JsonPropertyName("crdPosStart")]
      public Common CrdPosStart
      {
        get => crdPosStart ??= new();
        set => crdPosStart = value;
      }

      /// <summary>
      /// A value of CrdPosEnd.
      /// </summary>
      [JsonPropertyName("crdPosEnd")]
      public Common CrdPosEnd
      {
        get => crdPosEnd ??= new();
        set => crdPosEnd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private Common crdPosStart;
      private Common crdPosEnd;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of LineType.
      /// </summary>
      [JsonPropertyName("lineType")]
      public TextWorkArea LineType
      {
        get => lineType ??= new();
        set => lineType = value;
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
      /// A value of CrdCrComboNo.
      /// </summary>
      [JsonPropertyName("crdCrComboNo")]
      public CrdCrComboNo CrdCrComboNo
      {
        get => crdCrComboNo ??= new();
        set => crdCrComboNo = value;
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
      /// A value of Process.
      /// </summary>
      [JsonPropertyName("process")]
      public DateWorkArea Process
      {
        get => process ??= new();
        set => process = value;
      }

      /// <summary>
      /// A value of PayeeRsn.
      /// </summary>
      [JsonPropertyName("payeeRsn")]
      public TextWorkArea PayeeRsn
      {
        get => payeeRsn ??= new();
        set => payeeRsn = value;
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
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
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
      /// A value of Amt.
      /// </summary>
      [JsonPropertyName("amt")]
      public Common Amt
      {
        get => amt ??= new();
        set => amt = value;
      }

      /// <summary>
      /// A value of Pgm.
      /// </summary>
      [JsonPropertyName("pgm")]
      public WorkArea Pgm
      {
        get => pgm ??= new();
        set => pgm = value;
      }

      /// <summary>
      /// A value of RcvryExists.
      /// </summary>
      [JsonPropertyName("rcvryExists")]
      public TextWorkArea RcvryExists
      {
        get => rcvryExists ??= new();
        set => rcvryExists = value;
      }

      /// <summary>
      /// A value of DtlRecId.
      /// </summary>
      [JsonPropertyName("dtlRecId")]
      public Common DtlRecId
      {
        get => dtlRecId ??= new();
        set => dtlRecId = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private Common common;
      private TextWorkArea lineType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private CrdCrComboNo crdCrComboNo;
      private Collection collection;
      private DateWorkArea process;
      private TextWorkArea payeeRsn;
      private Obligation obligation;
      private ObligationType obligationType;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private Common amt;
      private WorkArea pgm;
      private TextWorkArea rcvryExists;
      private Common dtlRecId;
    }

    /// <summary>A Sort1Group group.</summary>
    [Serializable]
    public class Sort1Group
    {
      /// <summary>
      /// A value of Sort1Common.
      /// </summary>
      [JsonPropertyName("sort1Common")]
      public Common Sort1Common
      {
        get => sort1Common ??= new();
        set => sort1Common = value;
      }

      /// <summary>
      /// A value of Sort1LineType.
      /// </summary>
      [JsonPropertyName("sort1LineType")]
      public TextWorkArea Sort1LineType
      {
        get => sort1LineType ??= new();
        set => sort1LineType = value;
      }

      /// <summary>
      /// A value of Sort1CashReceipt.
      /// </summary>
      [JsonPropertyName("sort1CashReceipt")]
      public CashReceipt Sort1CashReceipt
      {
        get => sort1CashReceipt ??= new();
        set => sort1CashReceipt = value;
      }

      /// <summary>
      /// A value of Sort1CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("sort1CashReceiptDetail")]
      public CashReceiptDetail Sort1CashReceiptDetail
      {
        get => sort1CashReceiptDetail ??= new();
        set => sort1CashReceiptDetail = value;
      }

      /// <summary>
      /// A value of Sort1CrdCrComboNo.
      /// </summary>
      [JsonPropertyName("sort1CrdCrComboNo")]
      public CrdCrComboNo Sort1CrdCrComboNo
      {
        get => sort1CrdCrComboNo ??= new();
        set => sort1CrdCrComboNo = value;
      }

      /// <summary>
      /// A value of Sort1Collection.
      /// </summary>
      [JsonPropertyName("sort1Collection")]
      public Collection Sort1Collection
      {
        get => sort1Collection ??= new();
        set => sort1Collection = value;
      }

      /// <summary>
      /// A value of Sort1Process.
      /// </summary>
      [JsonPropertyName("sort1Process")]
      public DateWorkArea Sort1Process
      {
        get => sort1Process ??= new();
        set => sort1Process = value;
      }

      /// <summary>
      /// A value of Sort1PayeeRsn.
      /// </summary>
      [JsonPropertyName("sort1PayeeRsn")]
      public TextWorkArea Sort1PayeeRsn
      {
        get => sort1PayeeRsn ??= new();
        set => sort1PayeeRsn = value;
      }

      /// <summary>
      /// A value of Sort1Obligation.
      /// </summary>
      [JsonPropertyName("sort1Obligation")]
      public Obligation Sort1Obligation
      {
        get => sort1Obligation ??= new();
        set => sort1Obligation = value;
      }

      /// <summary>
      /// A value of Sort1ObligationType.
      /// </summary>
      [JsonPropertyName("sort1ObligationType")]
      public ObligationType Sort1ObligationType
      {
        get => sort1ObligationType ??= new();
        set => sort1ObligationType = value;
      }

      /// <summary>
      /// A value of Sort1Debt.
      /// </summary>
      [JsonPropertyName("sort1Debt")]
      public ObligationTransaction Sort1Debt
      {
        get => sort1Debt ??= new();
        set => sort1Debt = value;
      }

      /// <summary>
      /// A value of Sort1DebtDetail.
      /// </summary>
      [JsonPropertyName("sort1DebtDetail")]
      public DebtDetail Sort1DebtDetail
      {
        get => sort1DebtDetail ??= new();
        set => sort1DebtDetail = value;
      }

      /// <summary>
      /// A value of Sort1LegalAction.
      /// </summary>
      [JsonPropertyName("sort1LegalAction")]
      public LegalAction Sort1LegalAction
      {
        get => sort1LegalAction ??= new();
        set => sort1LegalAction = value;
      }

      /// <summary>
      /// A value of Sort1Amt.
      /// </summary>
      [JsonPropertyName("sort1Amt")]
      public Common Sort1Amt
      {
        get => sort1Amt ??= new();
        set => sort1Amt = value;
      }

      /// <summary>
      /// A value of Sort1Pgm.
      /// </summary>
      [JsonPropertyName("sort1Pgm")]
      public WorkArea Sort1Pgm
      {
        get => sort1Pgm ??= new();
        set => sort1Pgm = value;
      }

      /// <summary>
      /// A value of Sort1RcvryExists.
      /// </summary>
      [JsonPropertyName("sort1RcvryExists")]
      public TextWorkArea Sort1RcvryExists
      {
        get => sort1RcvryExists ??= new();
        set => sort1RcvryExists = value;
      }

      /// <summary>
      /// A value of Sort1DtlRecId.
      /// </summary>
      [JsonPropertyName("sort1DtlRecId")]
      public Common Sort1DtlRecId
      {
        get => sort1DtlRecId ??= new();
        set => sort1DtlRecId = value;
      }

      private Common sort1Common;
      private TextWorkArea sort1LineType;
      private CashReceipt sort1CashReceipt;
      private CashReceiptDetail sort1CashReceiptDetail;
      private CrdCrComboNo sort1CrdCrComboNo;
      private Collection sort1Collection;
      private DateWorkArea sort1Process;
      private TextWorkArea sort1PayeeRsn;
      private Obligation sort1Obligation;
      private ObligationType sort1ObligationType;
      private ObligationTransaction sort1Debt;
      private DebtDetail sort1DebtDetail;
      private LegalAction sort1LegalAction;
      private Common sort1Amt;
      private WorkArea sort1Pgm;
      private TextWorkArea sort1RcvryExists;
      private Common sort1DtlRecId;
    }

    /// <summary>A Sort2Group group.</summary>
    [Serializable]
    public class Sort2Group
    {
      /// <summary>
      /// A value of Sort2Common.
      /// </summary>
      [JsonPropertyName("sort2Common")]
      public Common Sort2Common
      {
        get => sort2Common ??= new();
        set => sort2Common = value;
      }

      /// <summary>
      /// A value of Sort2LineType.
      /// </summary>
      [JsonPropertyName("sort2LineType")]
      public TextWorkArea Sort2LineType
      {
        get => sort2LineType ??= new();
        set => sort2LineType = value;
      }

      /// <summary>
      /// A value of Sort2CashReceipt.
      /// </summary>
      [JsonPropertyName("sort2CashReceipt")]
      public CashReceipt Sort2CashReceipt
      {
        get => sort2CashReceipt ??= new();
        set => sort2CashReceipt = value;
      }

      /// <summary>
      /// A value of Sort2CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("sort2CashReceiptDetail")]
      public CashReceiptDetail Sort2CashReceiptDetail
      {
        get => sort2CashReceiptDetail ??= new();
        set => sort2CashReceiptDetail = value;
      }

      /// <summary>
      /// A value of Sort2CrdCrComboNo.
      /// </summary>
      [JsonPropertyName("sort2CrdCrComboNo")]
      public CrdCrComboNo Sort2CrdCrComboNo
      {
        get => sort2CrdCrComboNo ??= new();
        set => sort2CrdCrComboNo = value;
      }

      /// <summary>
      /// A value of Sort2Collection.
      /// </summary>
      [JsonPropertyName("sort2Collection")]
      public Collection Sort2Collection
      {
        get => sort2Collection ??= new();
        set => sort2Collection = value;
      }

      /// <summary>
      /// A value of Sort2Process.
      /// </summary>
      [JsonPropertyName("sort2Process")]
      public DateWorkArea Sort2Process
      {
        get => sort2Process ??= new();
        set => sort2Process = value;
      }

      /// <summary>
      /// A value of Sort2PayeeRsn.
      /// </summary>
      [JsonPropertyName("sort2PayeeRsn")]
      public TextWorkArea Sort2PayeeRsn
      {
        get => sort2PayeeRsn ??= new();
        set => sort2PayeeRsn = value;
      }

      /// <summary>
      /// A value of Sort2Obligation.
      /// </summary>
      [JsonPropertyName("sort2Obligation")]
      public Obligation Sort2Obligation
      {
        get => sort2Obligation ??= new();
        set => sort2Obligation = value;
      }

      /// <summary>
      /// A value of Sort2ObligationType.
      /// </summary>
      [JsonPropertyName("sort2ObligationType")]
      public ObligationType Sort2ObligationType
      {
        get => sort2ObligationType ??= new();
        set => sort2ObligationType = value;
      }

      /// <summary>
      /// A value of Sort2Debt.
      /// </summary>
      [JsonPropertyName("sort2Debt")]
      public ObligationTransaction Sort2Debt
      {
        get => sort2Debt ??= new();
        set => sort2Debt = value;
      }

      /// <summary>
      /// A value of Sort2DebtDetail.
      /// </summary>
      [JsonPropertyName("sort2DebtDetail")]
      public DebtDetail Sort2DebtDetail
      {
        get => sort2DebtDetail ??= new();
        set => sort2DebtDetail = value;
      }

      /// <summary>
      /// A value of Sort2LegalAction.
      /// </summary>
      [JsonPropertyName("sort2LegalAction")]
      public LegalAction Sort2LegalAction
      {
        get => sort2LegalAction ??= new();
        set => sort2LegalAction = value;
      }

      /// <summary>
      /// A value of Sort2Amt.
      /// </summary>
      [JsonPropertyName("sort2Amt")]
      public Common Sort2Amt
      {
        get => sort2Amt ??= new();
        set => sort2Amt = value;
      }

      /// <summary>
      /// A value of Sort2Pgm.
      /// </summary>
      [JsonPropertyName("sort2Pgm")]
      public WorkArea Sort2Pgm
      {
        get => sort2Pgm ??= new();
        set => sort2Pgm = value;
      }

      /// <summary>
      /// A value of Sort2RcvryExists.
      /// </summary>
      [JsonPropertyName("sort2RcvryExists")]
      public TextWorkArea Sort2RcvryExists
      {
        get => sort2RcvryExists ??= new();
        set => sort2RcvryExists = value;
      }

      /// <summary>
      /// A value of Sort2DtlRecId.
      /// </summary>
      [JsonPropertyName("sort2DtlRecId")]
      public Common Sort2DtlRecId
      {
        get => sort2DtlRecId ??= new();
        set => sort2DtlRecId = value;
      }

      private Common sort2Common;
      private TextWorkArea sort2LineType;
      private CashReceipt sort2CashReceipt;
      private CashReceiptDetail sort2CashReceiptDetail;
      private CrdCrComboNo sort2CrdCrComboNo;
      private Collection sort2Collection;
      private DateWorkArea sort2Process;
      private TextWorkArea sort2PayeeRsn;
      private Obligation sort2Obligation;
      private ObligationType sort2ObligationType;
      private ObligationTransaction sort2Debt;
      private DebtDetail sort2DebtDetail;
      private LegalAction sort2LegalAction;
      private Common sort2Amt;
      private WorkArea sort2Pgm;
      private TextWorkArea sort2RcvryExists;
      private Common sort2DtlRecId;
    }

    /// <summary>A HoldGroup group.</summary>
    [Serializable]
    public class HoldGroup
    {
      /// <summary>
      /// A value of HoldCommon.
      /// </summary>
      [JsonPropertyName("holdCommon")]
      public Common HoldCommon
      {
        get => holdCommon ??= new();
        set => holdCommon = value;
      }

      /// <summary>
      /// A value of HoldGrpLineType.
      /// </summary>
      [JsonPropertyName("holdGrpLineType")]
      public TextWorkArea HoldGrpLineType
      {
        get => holdGrpLineType ??= new();
        set => holdGrpLineType = value;
      }

      /// <summary>
      /// A value of HoldCashReceipt.
      /// </summary>
      [JsonPropertyName("holdCashReceipt")]
      public CashReceipt HoldCashReceipt
      {
        get => holdCashReceipt ??= new();
        set => holdCashReceipt = value;
      }

      /// <summary>
      /// A value of HoldCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("holdCashReceiptDetail")]
      public CashReceiptDetail HoldCashReceiptDetail
      {
        get => holdCashReceiptDetail ??= new();
        set => holdCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of HoldCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("holdCrdCrComboNo")]
      public CrdCrComboNo HoldCrdCrComboNo
      {
        get => holdCrdCrComboNo ??= new();
        set => holdCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of HoldCollection.
      /// </summary>
      [JsonPropertyName("holdCollection")]
      public Collection HoldCollection
      {
        get => holdCollection ??= new();
        set => holdCollection = value;
      }

      /// <summary>
      /// A value of HoldGrpProcess.
      /// </summary>
      [JsonPropertyName("holdGrpProcess")]
      public DateWorkArea HoldGrpProcess
      {
        get => holdGrpProcess ??= new();
        set => holdGrpProcess = value;
      }

      /// <summary>
      /// A value of HoldGrpPayeeRsn.
      /// </summary>
      [JsonPropertyName("holdGrpPayeeRsn")]
      public TextWorkArea HoldGrpPayeeRsn
      {
        get => holdGrpPayeeRsn ??= new();
        set => holdGrpPayeeRsn = value;
      }

      /// <summary>
      /// A value of HoldObligation.
      /// </summary>
      [JsonPropertyName("holdObligation")]
      public Obligation HoldObligation
      {
        get => holdObligation ??= new();
        set => holdObligation = value;
      }

      /// <summary>
      /// A value of HoldObligationType.
      /// </summary>
      [JsonPropertyName("holdObligationType")]
      public ObligationType HoldObligationType
      {
        get => holdObligationType ??= new();
        set => holdObligationType = value;
      }

      /// <summary>
      /// A value of HoldDebt.
      /// </summary>
      [JsonPropertyName("holdDebt")]
      public ObligationTransaction HoldDebt
      {
        get => holdDebt ??= new();
        set => holdDebt = value;
      }

      /// <summary>
      /// A value of HoldDebtDetail.
      /// </summary>
      [JsonPropertyName("holdDebtDetail")]
      public DebtDetail HoldDebtDetail
      {
        get => holdDebtDetail ??= new();
        set => holdDebtDetail = value;
      }

      /// <summary>
      /// A value of HoldLegalAction.
      /// </summary>
      [JsonPropertyName("holdLegalAction")]
      public LegalAction HoldLegalAction
      {
        get => holdLegalAction ??= new();
        set => holdLegalAction = value;
      }

      /// <summary>
      /// A value of HoldGrpAmt.
      /// </summary>
      [JsonPropertyName("holdGrpAmt")]
      public Common HoldGrpAmt
      {
        get => holdGrpAmt ??= new();
        set => holdGrpAmt = value;
      }

      /// <summary>
      /// A value of HoldGrpPgm.
      /// </summary>
      [JsonPropertyName("holdGrpPgm")]
      public WorkArea HoldGrpPgm
      {
        get => holdGrpPgm ??= new();
        set => holdGrpPgm = value;
      }

      /// <summary>
      /// A value of HoldGrpRcvryExists.
      /// </summary>
      [JsonPropertyName("holdGrpRcvryExists")]
      public TextWorkArea HoldGrpRcvryExists
      {
        get => holdGrpRcvryExists ??= new();
        set => holdGrpRcvryExists = value;
      }

      /// <summary>
      /// A value of HoldGrpDtlRecId.
      /// </summary>
      [JsonPropertyName("holdGrpDtlRecId")]
      public Common HoldGrpDtlRecId
      {
        get => holdGrpDtlRecId ??= new();
        set => holdGrpDtlRecId = value;
      }

      private Common holdCommon;
      private TextWorkArea holdGrpLineType;
      private CashReceipt holdCashReceipt;
      private CashReceiptDetail holdCashReceiptDetail;
      private CrdCrComboNo holdCrdCrComboNo;
      private Collection holdCollection;
      private DateWorkArea holdGrpProcess;
      private TextWorkArea holdGrpPayeeRsn;
      private Obligation holdObligation;
      private ObligationType holdObligationType;
      private ObligationTransaction holdDebt;
      private DebtDetail holdDebtDetail;
      private LegalAction holdLegalAction;
      private Common holdGrpAmt;
      private WorkArea holdGrpPgm;
      private TextWorkArea holdGrpRcvryExists;
      private Common holdGrpDtlRecId;
    }

    /// <summary>
    /// A value of TmpCrFee.
    /// </summary>
    [JsonPropertyName("tmpCrFee")]
    public Common TmpCrFee
    {
      get => tmpCrFee ??= new();
      set => tmpCrFee = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
    }

    /// <summary>
    /// A value of TmpLegalAction.
    /// </summary>
    [JsonPropertyName("tmpLegalAction")]
    public LegalAction TmpLegalAction
    {
      get => tmpLegalAction ??= new();
      set => tmpLegalAction = value;
    }

    /// <summary>
    /// A value of CrdsReadCnt.
    /// </summary>
    [JsonPropertyName("crdsReadCnt")]
    public Common CrdsReadCnt
    {
      get => crdsReadCnt ??= new();
      set => crdsReadCnt = value;
    }

    /// <summary>
    /// A value of CopyNow.
    /// </summary>
    [JsonPropertyName("copyNow")]
    public Common CopyNow
    {
      get => copyNow ??= new();
      set => copyNow = value;
    }

    /// <summary>
    /// A value of HoldCashReceipt.
    /// </summary>
    [JsonPropertyName("holdCashReceipt")]
    public CashReceipt HoldCashReceipt
    {
      get => holdCashReceipt ??= new();
      set => holdCashReceipt = value;
    }

    /// <summary>
    /// A value of HoldCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("holdCashReceiptDetail")]
    public CashReceiptDetail HoldCashReceiptDetail
    {
      get => holdCashReceiptDetail ??= new();
      set => holdCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of TmpStartPos.
    /// </summary>
    [JsonPropertyName("tmpStartPos")]
    public Common TmpStartPos
    {
      get => tmpStartPos ??= new();
      set => tmpStartPos = value;
    }

    /// <summary>
    /// Gets a value of CrdPos.
    /// </summary>
    [JsonIgnore]
    public Array<CrdPosGroup> CrdPos => crdPos ??= new(CrdPosGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CrdPos for json serialization.
    /// </summary>
    [JsonPropertyName("crdPos")]
    [Computed]
    public IList<CrdPosGroup> CrdPos_Json
    {
      get => crdPos;
      set => CrdPos.Assign(value);
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of KeepLooping.
    /// </summary>
    [JsonPropertyName("keepLooping")]
    public Common KeepLooping
    {
      get => keepLooping ??= new();
      set => keepLooping = value;
    }

    /// <summary>
    /// A value of RefFndInd.
    /// </summary>
    [JsonPropertyName("refFndInd")]
    public Common RefFndInd
    {
      get => refFndInd ??= new();
      set => refFndInd = value;
    }

    /// <summary>
    /// A value of CollectionFrom.
    /// </summary>
    [JsonPropertyName("collectionFrom")]
    public DateWorkArea CollectionFrom
    {
      get => collectionFrom ??= new();
      set => collectionFrom = value;
    }

    /// <summary>
    /// A value of CollectionTo.
    /// </summary>
    [JsonPropertyName("collectionTo")]
    public DateWorkArea CollectionTo
    {
      get => collectionTo ??= new();
      set => collectionTo = value;
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
    /// A value of TestProcess.
    /// </summary>
    [JsonPropertyName("testProcess")]
    public DateWorkArea TestProcess
    {
      get => testProcess ??= new();
      set => testProcess = value;
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
    /// Gets a value of Sort1.
    /// </summary>
    [JsonPropertyName("sort1")]
    public Sort1Group Sort1
    {
      get => sort1 ?? (sort1 = new());
      set => sort1 = value;
    }

    /// <summary>
    /// Gets a value of Sort2.
    /// </summary>
    [JsonPropertyName("sort2")]
    public Sort2Group Sort2
    {
      get => sort2 ?? (sort2 = new());
      set => sort2 = value;
    }

    /// <summary>
    /// Gets a value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public HoldGroup Hold
    {
      get => hold ?? (hold = new());
      set => hold = value;
    }

    /// <summary>
    /// A value of TmpDtlRecId.
    /// </summary>
    [JsonPropertyName("tmpDtlRecId")]
    public Common TmpDtlRecId
    {
      get => tmpDtlRecId ??= new();
      set => tmpDtlRecId = value;
    }

    /// <summary>
    /// A value of TmpDateWorkArea.
    /// </summary>
    [JsonPropertyName("tmpDateWorkArea")]
    public DateWorkArea TmpDateWorkArea
    {
      get => tmpDateWorkArea ??= new();
      set => tmpDateWorkArea = value;
    }

    /// <summary>
    /// A value of RowCombinedInd.
    /// </summary>
    [JsonPropertyName("rowCombinedInd")]
    public Common RowCombinedInd
    {
      get => rowCombinedInd ??= new();
      set => rowCombinedInd = value;
    }

    private Common tmpCrFee;
    private LegalAction nullLegalAction;
    private LegalAction tmpLegalAction;
    private Common crdsReadCnt;
    private Common copyNow;
    private CashReceipt holdCashReceipt;
    private CashReceiptDetail holdCashReceiptDetail;
    private Common tmpStartPos;
    private Array<CrdPosGroup> crdPos;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea max;
    private Common keepLooping;
    private Common refFndInd;
    private DateWorkArea collectionFrom;
    private DateWorkArea collectionTo;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea testProcess;
    private Array<GroupGroup> group;
    private Sort1Group sort1;
    private Sort2Group sort2;
    private HoldGroup hold;
    private Common tmpDtlRecId;
    private DateWorkArea tmpDateWorkArea;
    private Common rowCombinedInd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
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
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
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
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
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
    /// A value of ExistingCollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("existingCollectionAdjustmentReason")]
    public CollectionAdjustmentReason ExistingCollectionAdjustmentReason
    {
      get => existingCollectionAdjustmentReason ??= new();
      set => existingCollectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of ExistingObligee.
    /// </summary>
    [JsonPropertyName("existingObligee")]
    public CsePerson ExistingObligee
    {
      get => existingObligee ??= new();
      set => existingObligee = value;
    }

    /// <summary>
    /// A value of Exsting.
    /// </summary>
    [JsonPropertyName("exsting")]
    public CsePersonAccount Exsting
    {
      get => exsting ??= new();
      set => exsting = value;
    }

    /// <summary>
    /// A value of ExistingDisbCollection.
    /// </summary>
    [JsonPropertyName("existingDisbCollection")]
    public DisbursementTransaction ExistingDisbCollection
    {
      get => existingDisbCollection ??= new();
      set => existingDisbCollection = value;
    }

    /// <summary>
    /// A value of ExistingReceiptRefund.
    /// </summary>
    [JsonPropertyName("existingReceiptRefund")]
    public ReceiptRefund ExistingReceiptRefund
    {
      get => existingReceiptRefund ??= new();
      set => existingReceiptRefund = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingRecovery.
    /// </summary>
    [JsonPropertyName("existingRecovery")]
    public PaymentRequest ExistingRecovery
    {
      get => existingRecovery ??= new();
      set => existingRecovery = value;
    }

    /// <summary>
    /// A value of ExistingDisbursement.
    /// </summary>
    [JsonPropertyName("existingDisbursement")]
    public DisbursementTransaction ExistingDisbursement
    {
      get => existingDisbursement ??= new();
      set => existingDisbursement = value;
    }

    /// <summary>
    /// A value of ExistingDisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("existingDisbursementTransactionRln")]
    public DisbursementTransactionRln ExistingDisbursementTransactionRln
    {
      get => existingDisbursementTransactionRln ??= new();
      set => existingDisbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of ExistingDisbursementType.
    /// </summary>
    [JsonPropertyName("existingDisbursementType")]
    public DisbursementType ExistingDisbursementType
    {
      get => existingDisbursementType ??= new();
      set => existingDisbursementType = value;
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
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private Collection existingCollection;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private CollectionAdjustmentReason existingCollectionAdjustmentReason;
    private CsePerson existingObligee;
    private CsePersonAccount exsting;
    private DisbursementTransaction existingDisbCollection;
    private ReceiptRefund existingReceiptRefund;
    private LegalAction existingLegalAction;
    private PaymentRequest existingRecovery;
    private DisbursementTransaction existingDisbursement;
    private DisbursementTransactionRln existingDisbursementTransactionRln;
    private DisbursementType existingDisbursementType;
    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
  }
#endregion
}
