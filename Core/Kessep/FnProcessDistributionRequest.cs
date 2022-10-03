// Program: FN_PROCESS_DISTRIBUTION_REQUEST, ID: 372279914, model: 746.
// Short name: SWE02365
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCESS_DISTRIBUTION_REQUEST.
/// </summary>
[Serializable]
public partial class FnProcessDistributionRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_DISTRIBUTION_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessDistributionRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessDistributionRequest.
  /// </summary>
  public FnProcessDistributionRequest(IContext context, Import import,
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
    // ***********************************************************
    // * *    A Change in the CSENete Area of this A/B
    // * *   MAY require a similar change in Tran SWEFB676
    // ***********************************************************
    // --------------------------------------------
    // I00122005    02/05/01    P.Phinney - Remove BLOCK on 
    // Interstate_Request_History
    // 08/13/01 I00122438 - check Interstate Request State against CSEnet ready 
    // states
    // --------------------------------------------
    // --------------------------------------------
    // I00159319    11/18/02    P.Phinney - Remove ALL CseNet Logic
    // -  It will NOW be in only ONE Place SWEFB676 -
    // --------------------------------------------
    MoveCashReceiptDetail(import.PersistantCashReceiptDetail,
      local.ForUpdateCashReceiptDetail);
    local.ProgramProcessingInfo.ProcessDate = import.Current.Date;
    local.MaxDiscontinueDate.Date = UseCabSetMaximumDiscontinueDate();
    local.Collection.Date = import.PersistantCashReceiptDetail.CollectionDate;
    local.Collection.YearMonth = UseCabGetYearMonthFromDate1();
    UseCabFirstAndLastDateOfMonth();

    // : Update the Obligor Account information.
    local.ForUpdateObligor.Assign(import.PersistantObligor);

    if (AsChar(import.AutoOrManual.DistributionMethod) == 'M')
    {
      local.ForUpdateObligor.LastManualDistributionDate = import.Current.Date;
    }

    local.ForUpdateObligor.LastCollAmt = 0;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      local.ForUpdateObligor.LastCollAmt =
        local.ForUpdateObligor.LastCollAmt.GetValueOrDefault() + import
        .Group.Item.Collection.Amount;
    }

    try
    {
      UpdateObligor();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_OBLIGOR_ACCT_NU_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_OBLIGOR_ACCT_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Process the selections.
    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (local.ForUpdateObligation.SystemGeneratedIdentifier == import
        .Group.Item.Obligation.SystemGeneratedIdentifier)
      {
        local.ForUpdateObligation.LastCollAmt =
          local.ForUpdateObligation.LastCollAmt.GetValueOrDefault() + import
          .Group.Item.Collection.Amount;
      }
      else
      {
        local.UpdateMatchingConcurrOb.Flag = "N";

        if (entities.ExistingObligation.Populated)
        {
          try
          {
            UpdateObligation1();

            if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
              (import.HardcodedJointSeveralObligation.PrimarySecondaryCode))
            {
              local.UpdateMatchingConcurrOb.Flag = "Y";
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OBLIGATION_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIGATION_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (ReadObligation1())
        {
          MoveObligation(entities.ExistingObligation, local.ForUpdateObligation);
            
          local.ForUpdateObligation.LastCollAmt = 0;
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF_RB";

          return;
        }
      }

      if (ReadLegalAction())
      {
        local.ForUpdateCollection.CourtOrderAppliedTo =
          entities.ExistingLegalAction.StandardNumber;
        local.LegalAction.StandardNumber =
          entities.ExistingLegalAction.StandardNumber;

        // : Send Court Notice for KS courts only.
        if (ReadFips())
        {
          if (AsChar(import.CashReceiptSourceType.CourtInd) == 'C')
          {
            local.ForUpdateCollection.CourtNoticeReqInd = "N";
          }
          else if (import.CashReceiptType.SystemGeneratedIdentifier == import
            .HardcodedFcourtPmt.SystemGeneratedIdentifier || import
            .CashReceiptType.SystemGeneratedIdentifier == import
            .HardcodedFdirPmt.SystemGeneratedIdentifier)
          {
            local.ForUpdateCollection.CourtNoticeReqInd = "N";
          }
          else
          {
            local.ForUpdateCollection.CourtNoticeReqInd = "Y";
          }
        }
        else
        {
          local.ForUpdateCollection.CourtNoticeReqInd = "N";
        }

        // : Send Court Notice for all courts.
      }
      else
      {
        local.ForUpdateCollection.CourtOrderAppliedTo = "";
        local.ForUpdateCollection.CourtNoticeReqInd = "N";
        local.LegalAction.StandardNumber = local.NullLegalAction.StandardNumber;
      }

      local.ForUpdateCollection.DistributionMethod =
        import.AutoOrManual.DistributionMethod;

      if (AsChar(local.ForUpdateCollection.DistributionMethod) == 'A')
      {
        if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
          (import.HardcodedSecondary.PrimarySecondaryCode))
        {
          foreach(var item in ReadObligCollProtectionHist2())
          {
            if (IsEmpty(entities.ExistingObligCollProtectionHist.ProtectionLevel))
              
            {
              local.ForUpdateCollection.DistributionMethod = "P";

              goto Test1;
            }
            else if (AsChar(entities.ExistingObligCollProtectionHist.
              ProtectionLevel) == AsChar
              (import.Group.Item.Collection.AppliedToCode))
            {
              local.ForUpdateCollection.DistributionMethod = "P";

              goto Test1;
            }
          }

          foreach(var item in ReadObligCollProtectionHist1())
          {
            if (IsEmpty(entities.ExistingObligCollProtectionHist.ProtectionLevel))
              
            {
              local.ForUpdateCollection.DistributionMethod = "P";

              goto Test1;
            }
            else if (AsChar(entities.ExistingObligCollProtectionHist.
              ProtectionLevel) == AsChar
              (import.Group.Item.Collection.AppliedToCode))
            {
              local.ForUpdateCollection.DistributionMethod = "P";

              goto Test1;
            }
          }

          // : No action required - Continue Processing.
        }
        else
        {
          foreach(var item in ReadObligCollProtectionHist3())
          {
            if (IsEmpty(entities.ExistingObligCollProtectionHist.ProtectionLevel))
              
            {
              local.ForUpdateCollection.DistributionMethod = "P";

              goto Test1;
            }
            else if (AsChar(entities.ExistingObligCollProtectionHist.
              ProtectionLevel) == AsChar
              (import.Group.Item.Collection.AppliedToCode))
            {
              local.ForUpdateCollection.DistributionMethod = "P";

              goto Test1;
            }
          }

          // : No action required - Continue Processing.
        }
      }

Test1:

      if (ReadDebtDebtDetail())
      {
        MoveDebtDetail2(entities.ExistingDebtDetail, local.ForUpdateDebtDetail);
        local.DueDate.Date = entities.ExistingDebtDetail.DueDt;
        local.DueDate.YearMonth = UseCabGetYearMonthFromDate2();
      }
      else
      {
        ExitState = "FN0000_OBLIG_TRANS_NF_RB";

        return;
      }

      if (AsChar(import.Group.Item.ObligationType.SupportedPersonReqInd) == 'Y')
      {
        local.ForUpdateCollection.ProgramAppliedTo =
          import.Group.Item.Program.Code;
        local.ForUpdateCollection.DistPgmStateAppldTo =
          import.Group.Item.DprProgram.ProgramState;

        if (AsChar(import.Group.Item.Program.InterstateIndicator) == 'Y')
        {
          local.ForUpdateCollection.AppliedToOrderTypeCode = "I";
        }
        else
        {
          local.ForUpdateCollection.AppliedToOrderTypeCode = "K";
        }
      }
      else
      {
        // : No Supported Person for this Obligation - Continue Processing.
        local.ForUpdateCollection.ProgramAppliedTo = "";
        local.ForUpdateCollection.DistPgmStateAppldTo = "";
        local.ForUpdateCollection.AppliedToOrderTypeCode = "K";
      }

      // : Build the Collection.
      local.ForUpdateCollection.AppliedToCode =
        import.Group.Item.Collection.AppliedToCode;
      local.ForUpdateCollection.AppliedToFuture = "N";

      if (AsChar(import.Group.Item.ObligationType.Classification) == AsChar
        (import.HardcodedVolClass.Classification))
      {
        // : Do not subtract any from the Debt Detail.  It is being applied to a
        // Voluntary Obligation.
      }
      else
      {
        switch(AsChar(import.Group.Item.Collection.AppliedToCode))
        {
          case 'C':
            local.ForUpdateDebtDetail.BalanceDueAmt -= import.Group.Item.
              Collection.Amount;

            if (Lt(local.CollectionMonthEndDt.Date,
              entities.ExistingDebtDetail.DueDt))
            {
              local.ForUpdateCollection.AppliedToFuture = "Y";
            }

            break;
          case 'A':
            local.ForUpdateDebtDetail.BalanceDueAmt -= import.Group.Item.
              Collection.Amount;

            break;
          case 'I':
            local.ForUpdateDebtDetail.InterestBalanceDueAmt =
              local.ForUpdateDebtDetail.InterestBalanceDueAmt.
                GetValueOrDefault() - import.Group.Item.Collection.Amount;

            break;
          case 'G':
            // : Do not subtract any from the Debt Detail.  This is a Gift.
            break;
          default:
            ExitState = "FN0000_INV_CRD_APPLY_TO_CODE_RB";

            return;
        }
      }

      // : Set the Conncurrent Indicator.
      if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
        (import.HardcodedPrimary.PrimarySecondaryCode))
      {
        local.ForUpdateCollection.ConcurrentInd = "N";
      }
      else if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
        (import.HardcodedSecondary.PrimarySecondaryCode))
      {
        local.ForUpdateCollection.ConcurrentInd = "Y";
      }
      else if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
        (import.HardcodedJointSeveralObligation.PrimarySecondaryCode))
      {
        // : The matching Joint & Several Obligation will be handled later.
        local.ForUpdateCollection.ConcurrentInd = "N";
      }
      else
      {
        local.ForUpdateCollection.ConcurrentInd = "N";
      }

      // : Set the Disbursement Processing Needed Indicator.
      // Added by SWSRGAD 10/06/99
      if (AsChar(import.Group.Item.ObligationType.SupportedPersonReqInd) == 'Y')
      {
        if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
          (import.HardcodedSecondary.PrimarySecondaryCode))
        {
          local.ForUpdateCollection.DisbursementProcessingNeedInd = "N";
        }
        else
        {
          local.ForUpdateCollection.DisbursementProcessingNeedInd = "Y";
        }
      }
      else
      {
        local.ForUpdateCollection.DisbursementProcessingNeedInd = "N";
      }

      // : Override the Disbursement Processing Needed Indicator for 718B's.
      // Added by SWSRGAD 9/27/99
      // Only if the program is AF or FC (FC Handled below).
      // Added by SWSRGAD 6/21/00
      if (import.Group.Item.ObligationType.SystemGeneratedIdentifier == import
        .Hardcoded718B.SystemGeneratedIdentifier)
      {
        local.ForUpdateCollection.DisbursementProcessingNeedInd = "N";
      }

      // : Override the Disbursement Processing Needed Indicator for:
      //   1. FC Program Type
      //   2. NF Program Type
      // Added by SWSRGAD 9/27/99
      //   3. NC Program Type
      // Added by SWSRAGD 10/14/99
      //   4. AF Program Type
      // Added by SWSRGAD 07/03/03 - WR030255
      if (import.Group.Item.Program.SystemGeneratedIdentifier == import
        .HardcodedAfType.SystemGeneratedIdentifier || import
        .Group.Item.Program.SystemGeneratedIdentifier == import
        .HardcodedFcType.SystemGeneratedIdentifier || import
        .Group.Item.Program.SystemGeneratedIdentifier == import
        .HardcodedNfType.SystemGeneratedIdentifier || import
        .Group.Item.Program.SystemGeneratedIdentifier == import
        .HardcodedNcType.SystemGeneratedIdentifier)
      {
        local.ForUpdateCollection.DisbursementProcessingNeedInd = "N";
      }

      if (AsChar(import.CashReceiptType.CategoryIndicator) == AsChar
        (import.HardcodedCashType.CategoryIndicator))
      {
        if (AsChar(local.ForUpdateCollection.ConcurrentInd) == 'Y')
        {
          local.ForUpdateCollection.DisburseToArInd = "N";
        }
        else if (AsChar(local.ForUpdateCollection.DisbursementProcessingNeedInd) ==
          'N')
        {
          local.ForUpdateCollection.DisburseToArInd = "N";
        }
        else
        {
          local.ForUpdateCollection.DisburseToArInd = "Y";
        }
      }
      else
      {
        local.ForUpdateCollection.DisburseToArInd = "N";
      }

      local.ForUpdateCollection.Amount = import.Group.Item.Collection.Amount;
      local.ForUpdateCollection.CollectionDt =
        import.PersistantCashReceiptDetail.CollectionDate;
      local.ForUpdateCollection.AdjustedInd = "N";
      local.ForUpdateCollection.ManualDistributionReasonText =
        import.ForMnlDistOnly.ManualDistributionReasonText ?? "";

      if (AsChar(local.ForUpdateCollection.ConcurrentInd) == 'N')
      {
        local.ForUpdateCashReceiptDetail.DistributedAmount =
          local.ForUpdateCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + local.ForUpdateCollection.Amount;
      }

      if (import.Adjusted.CollectionAmount - (
        local.ForUpdateCashReceiptDetail.DistributedAmount.GetValueOrDefault() +
        local.ForUpdateCashReceiptDetail.RefundedAmount.GetValueOrDefault()) < 0
        )
      {
        ExitState = "FN0000_AMT_TO_DIST_GRTR_COLL_RB";

        return;
      }

      // : Handle CSE/Net Collection Reporting.
      //   Read for an Interstate Request.
      // I00159319    11/18/02    P.Phinney - Remove ALL CseNet Logic
      local.ForUpdateCollection.CsenetOutboundReqInd = "N";

      // ^^^^^^^^^^^^^^^^  END OF CSENET PROCESSING  
      // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      UseFnCreateCollAndApplyToDebt1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // : Process the matching concurrent Debts for Joint & Several.
      if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
        (import.HardcodedJointSeveralObligation.PrimarySecondaryCode))
      {
        if (AsChar(import.Group.Item.ObligationType.Classification) == AsChar
          (import.HardcodedVolClass.Classification))
        {
          goto Test2;
        }

        if (AsChar(local.UpdateMatchingConcurrOb.Flag) == 'Y')
        {
          try
          {
            UpdateObligation2();
            local.UpdateMatchingConcurrOb.Flag = "N";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_CONCURRENT_OB_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_CONCURRENT_OB_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (ReadObligationRln1())
        {
          if (!ReadObligation3())
          {
            ExitState = "FN0000_CONCURRENT_OBLIG_NF_RB";

            return;
          }
        }
        else if (ReadObligationRln2())
        {
          if (!ReadObligation2())
          {
            ExitState = "FN0000_CONCURRENT_OBLIG_NF_RB";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_OBLIG_RLN_NF_RB";

          return;
        }

        if (ReadDebtDetailDebt())
        {
          MoveDebtDetail2(entities.ExistingConcurrentDebtDetail,
            local.ForUpdateDebtDetail);
        }
        else
        {
          ExitState = "FN0000_CONCURRENT_DEBT_DTL_NF_RB";

          return;
        }

        if (AsChar(import.Group.Item.ObligationType.Classification) == AsChar
          (import.HardcodedVolClass.Classification))
        {
          // : Do not subtract any from the Debt Detail.  This is a Voluntary.
        }
        else
        {
          switch(AsChar(import.Group.Item.Collection.AppliedToCode))
          {
            case 'C':
              local.ForUpdateDebtDetail.BalanceDueAmt -= import.Group.Item.
                Collection.Amount;

              break;
            case 'A':
              local.ForUpdateDebtDetail.BalanceDueAmt -= import.Group.Item.
                Collection.Amount;

              break;
            case 'I':
              local.ForUpdateDebtDetail.InterestBalanceDueAmt =
                local.ForUpdateDebtDetail.InterestBalanceDueAmt.
                  GetValueOrDefault() - import.Group.Item.Collection.Amount;

              break;
            case 'G':
              // : Do not subtract any from the Debt Detail.  This is a Gift.
              break;
            default:
              ExitState = "FN0000_INV_CRD_APPLY_TO_CODE_RB";

              return;
          }
        }

        local.ForUpdateCollection.ConcurrentInd = "Y";
        local.ForUpdateCollection.DisbursementProcessingNeedInd = "N";
        local.ForUpdateCollection.CourtNoticeReqInd = "N";
        UseFnCreateCollAndApplyToDebt2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

Test2:
      ;
    }

    try
    {
      UpdateObligation1();

      if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == AsChar
        (import.HardcodedJointSeveralObligation.PrimarySecondaryCode))
      {
        try
        {
          UpdateObligation2();
          local.UpdateMatchingConcurrOb.Flag = "N";
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_CONCURRENT_OB_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CONCURRENT_OB_PV_RB";

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
          ExitState = "FN0000_OBLIGATION_NU_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_OBLIGATION_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (import.Adjusted.CollectionAmount - (
      local.ForUpdateCashReceiptDetail.DistributedAmount.GetValueOrDefault() + local
      .ForUpdateCashReceiptDetail.RefundedAmount.GetValueOrDefault()) == 0)
    {
      local.ForUpdateCashReceiptDetail.CollectionAmtFullyAppliedInd = "Y";
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
          ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Create a new Cash Receipt Detail Status History record with a "
    // Distributed" status.
    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      try
      {
        UpdateCashReceiptDetailStatHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CASH_RCPT_DTL_S_HST_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CASH_RCPT_DTL_S_HST_PV_RB";

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
      ExitState = "FN0000_CASH_RCPT_DTL_S_HST_NF_RB";

      return;
    }

    if (ReadCashReceiptDetailStatus())
    {
      try
      {
        CreateCashReceiptDetailStatHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CASH_RCPT_DTL_S_HST_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CASH_RCPT_DTL_S_HST_PV_RB";

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
      ExitState = "FN0000_CASH_RCPT_DTL_S_HST_NF_RB";
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.CollectionAmount = source.CollectionAmount;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDebtDetail1(DebtDetail source, DebtDetail target)
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

  private static void MoveDebtDetail2(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.RetiredDt = source.RetiredDt;
  }

  private static void MoveHhHist1(Import.HhHistGroup source,
    FnCreateCollAndApplyToDebt.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(FnCreateCollAndApplyToDebt.Import.
    HhHistGroup source, Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl2);
  }

  private static void MoveHhHistDtl1(Import.HhHistDtlGroup source,
    FnCreateCollAndApplyToDebt.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(FnCreateCollAndApplyToDebt.Import.
    HhHistDtlGroup source, Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveLegal(Import.LegalGroup source,
    FnCreateCollAndApplyToDebt.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl);
  }

  private static void MoveLegalDtl(Import.LegalDtlGroup source,
    FnCreateCollAndApplyToDebt.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.LastCollAmt = source.LastCollAmt;
    target.LastCollDt = source.LastCollDt;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.OtyType = source.OtyType;
    target.ObgGeneratedId = source.ObgGeneratedId;
    target.CspNumber = source.CspNumber;
    target.CpaType = source.CpaType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Collection.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.CollectionMonthStartDt.Date = useExport.First.Date;
    local.CollectionMonthEndDt.Date = useExport.Last.Date;
  }

  private int UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Collection.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.DueDate.Date;

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

  private void UseFnCreateCollAndApplyToDebt1()
  {
    var useImport = new FnCreateCollAndApplyToDebt.Import();
    var useExport = new FnCreateCollAndApplyToDebt.Export();

    useImport.ForUpdateCollection.Assign(local.ForUpdateCollection);
    useImport.ForUpdateDebtDetail.Assign(local.ForUpdateDebtDetail);
    useImport.PersistantCashReceiptDetail.Assign(
      import.PersistantCashReceiptDetail);
    useImport.PersistantDebt.Assign(entities.ExistingDebt);
    useImport.PersistantDebtDetail.Assign(entities.ExistingDebtDetail);
    useImport.MaxDiscontinueDate.Date = local.MaxDiscontinueDate.Date;
    useImport.HardcodedDeactivedStatu.Code = import.HardcodedDeactivedStat.Code;
    useImport.UserId.Text8 = import.UserId.Text8;
    MoveDateWorkArea(import.Current, useImport.CurrentTimestamp);
    MoveObligationType(import.Group.Item.ObligationType,
      useImport.ObligationType);
    useImport.HardcodedVolClass.Classification =
      import.HardcodedVolClass.Classification;
    useImport.Obligation.PrimarySecondaryCode =
      import.Group.Item.Obligation.PrimarySecondaryCode;
    useImport.SuppPrsn.Number = import.Group.Item.SuppPrsn.Number;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.Collection.Date = local.Collection.Date;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      import.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    import.Legal.CopyTo(useImport.Legal, MoveLegal);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist1);
    useImport.HardcodedUk.ProgramState = import.HardcodedUk.ProgramState;

    Call(FnCreateCollAndApplyToDebt.Execute, useImport, useExport);

    local.ForUpdateCollection.Assign(useImport.ForUpdateCollection);
    import.PersistantCashReceiptDetail.Assign(
      useImport.PersistantCashReceiptDetail);
    entities.ExistingDebt.Assign(useImport.PersistantDebt);
    MoveDebtDetail1(useImport.PersistantDebtDetail, entities.ExistingDebtDetail);
      
    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist2);
  }

  private void UseFnCreateCollAndApplyToDebt2()
  {
    var useImport = new FnCreateCollAndApplyToDebt.Import();
    var useExport = new FnCreateCollAndApplyToDebt.Export();

    useImport.ForUpdateCollection.Assign(local.ForUpdateCollection);
    useImport.ForUpdateDebtDetail.Assign(local.ForUpdateDebtDetail);
    useImport.PersistantCashReceiptDetail.Assign(
      import.PersistantCashReceiptDetail);
    useImport.PersistantDebt.Assign(entities.ExistingConcurrentDebt);
    useImport.PersistantDebtDetail.
      Assign(entities.ExistingConcurrentDebtDetail);
    useImport.MaxDiscontinueDate.Date = local.MaxDiscontinueDate.Date;
    useImport.HardcodedDeactivedStatu.Code = import.HardcodedDeactivedStat.Code;
    useImport.UserId.Text8 = import.UserId.Text8;
    MoveDateWorkArea(import.Current, useImport.CurrentTimestamp);
    MoveObligationType(import.Group.Item.ObligationType,
      useImport.ObligationType);
    useImport.HardcodedVolClass.Classification =
      import.HardcodedVolClass.Classification;
    useImport.Obligation.PrimarySecondaryCode =
      import.Group.Item.Obligation.PrimarySecondaryCode;
    useImport.SuppPrsn.Number = import.Group.Item.SuppPrsn.Number;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useImport.Collection.Date = local.Collection.Date;
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      import.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      import.HardcodedSecondary.PrimarySecondaryCode;
    import.Legal.CopyTo(useImport.Legal, MoveLegal);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist1);
    useImport.HardcodedUk.ProgramState = import.HardcodedUk.ProgramState;

    Call(FnCreateCollAndApplyToDebt.Execute, useImport, useExport);

    local.ForUpdateCollection.Assign(useImport.ForUpdateCollection);
    import.PersistantCashReceiptDetail.Assign(
      useImport.PersistantCashReceiptDetail);
    MoveObligationTransaction(useImport.PersistantDebt,
      entities.ExistingConcurrentDebt);
    MoveDebtDetail1(useImport.PersistantDebtDetail,
      entities.ExistingConcurrentDebtDetail);
    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist2);
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistantCashReceiptDetail.Populated);

    var crdIdentifier = import.PersistantCashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = import.PersistantCashReceiptDetail.CrvIdentifier;
    var cstIdentifier = import.PersistantCashReceiptDetail.CstIdentifier;
    var crtIdentifier = import.PersistantCashReceiptDetail.CrtIdentifier;
    var cdsIdentifier =
      entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = import.Current.Timestamp;
    var createdBy = import.UserId.Text8;
    var discontinueDate = local.MaxDiscontinueDate.Date;

    entities.New1.Populated = false;
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
        db.SetNullableString(command, "reasonText", "");
      });

    entities.New1.CrdIdentifier = crdIdentifier;
    entities.New1.CrvIdentifier = crvIdentifier;
    entities.New1.CstIdentifier = cstIdentifier;
    entities.New1.CrtIdentifier = crtIdentifier;
    entities.New1.CdsIdentifier = cdsIdentifier;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.ReasonCodeId = "";
    entities.New1.CreatedBy = createdBy;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.ReasonText = "";
    entities.New1.Populated = true;
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistantCashReceiptDetail.Populated);
    entities.ExistingCashReceiptDetailStatus.Populated = false;
    entities.ExistingCashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaxDiscontinueDate.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crdIdentifier",
          import.PersistantCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          import.PersistantCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.PersistantCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.PersistantCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.HardcodedDistributed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingDebt.Populated = false;
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetInt32(
          command, "obTrnId", import.Group.Item.Debt.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebt.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebt.Type1 = db.GetString(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 4);
        entities.ExistingDebt.Amount = db.GetDecimal(reader, 5);
        entities.ExistingDebt.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.ExistingDebt.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 8);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 9);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 10);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 10);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 11);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 12);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingDebtDetail.AdcDt = db.GetNullableDate(reader, 14);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 15);
        entities.ExistingDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 16);
        entities.ExistingDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 17);
        entities.ExistingDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.ExistingDebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 19);
        entities.ExistingDebtDetail.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.ExistingDebt.Populated = true;
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);
      });
  }

  private bool ReadDebtDetailDebt()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingConcurrentObligation.Populated);
    entities.ExistingConcurrentDebt.Populated = false;
    entities.ExistingConcurrentDebtDetail.Populated = false;

    return Read("ReadDebtDetailDebt",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ExistingConcurrentObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingConcurrentObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingConcurrentObligation.CspNumber);
        db.SetString(
          command, "cpaType", entities.ExistingConcurrentObligation.CpaType);
        db.SetDate(
          command, "dueDt",
          entities.ExistingDebtDetail.DueDt.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", import.Group.Item.SuppPrsn.Number);
      },
      (db, reader) =>
      {
        entities.ExistingConcurrentDebtDetail.ObgGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingConcurrentDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingConcurrentDebtDetail.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingConcurrentDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingConcurrentDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingConcurrentDebt.CpaType = db.GetString(reader, 2);
        entities.ExistingConcurrentDebtDetail.OtrGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingConcurrentDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingConcurrentDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingConcurrentDebt.OtyType = db.GetInt32(reader, 4);
        entities.ExistingConcurrentDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingConcurrentDebt.Type1 = db.GetString(reader, 5);
        entities.ExistingConcurrentDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingConcurrentDebtDetail.BalanceDueAmt =
          db.GetDecimal(reader, 7);
        entities.ExistingConcurrentDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingConcurrentDebtDetail.AdcDt =
          db.GetNullableDate(reader, 9);
        entities.ExistingConcurrentDebtDetail.RetiredDt =
          db.GetNullableDate(reader, 10);
        entities.ExistingConcurrentDebtDetail.CoveredPrdStartDt =
          db.GetNullableDate(reader, 11);
        entities.ExistingConcurrentDebtDetail.CoveredPrdEndDt =
          db.GetNullableDate(reader, 12);
        entities.ExistingConcurrentDebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.ExistingConcurrentDebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 14);
        entities.ExistingConcurrentDebtDetail.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.ExistingConcurrentDebt.Amount = db.GetDecimal(reader, 16);
        entities.ExistingConcurrentDebt.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.ExistingConcurrentDebt.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 18);
        entities.ExistingConcurrentDebt.CspSupNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingConcurrentDebt.CpaSupType =
          db.GetNullableString(reader, 20);
        entities.ExistingConcurrentDebt.Populated = true;
        entities.ExistingConcurrentDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType",
          entities.ExistingConcurrentDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingConcurrentDebt.CpaType);
        CheckValid<DebtDetail>("OtrType",
          entities.ExistingConcurrentDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ExistingConcurrentDebt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingConcurrentDebt.CpaSupType);
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 3);
        entities.ExistingFips.Populated = true;
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
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligCollProtectionHist1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otySecondId", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.PersistantCashReceiptDetail.CollectionDate.
            GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 5);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ExistingObligCollProtectionHist.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligCollProtectionHist2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyFirstId", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspFNumber", entities.ExistingObligation.CspNumber);
        db.SetString(command, "cpaFType", entities.ExistingObligation.CpaType);
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.PersistantCashReceiptDetail.CollectionDate.
            GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 5);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ExistingObligCollProtectionHist.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligCollProtectionHist3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyIdentifier", entities.ExistingObligation.DtyGeneratedId);
          
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetDate(
          command, "cvrdCollStrtDt",
          import.PersistantCashReceiptDetail.CollectionDate.
            GetValueOrDefault());
        db.SetDate(
          command, "deactivationDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligCollProtectionHist.CvrdCollStrtDt =
          db.GetDate(reader, 0);
        entities.ExistingObligCollProtectionHist.CvrdCollEndDt =
          db.GetDate(reader, 1);
        entities.ExistingObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ExistingObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ExistingObligCollProtectionHist.CspNumber =
          db.GetString(reader, 4);
        entities.ExistingObligCollProtectionHist.CpaType =
          db.GetString(reader, 5);
        entities.ExistingObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 8);
        entities.ExistingObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ExistingObligCollProtectionHist.CpaType);

        return true;
      });
  }

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.Assert(import.PersistantObligor.Populated);
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PersistantObligor.Type1);
        db.SetString(command, "cspNumber", import.PersistantObligor.CspNumber);
        db.SetInt32(
          command, "obId",
          import.Group.Item.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.Group.Item.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ExistingObligation.LastCollAmt =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingObligation.LastCollDt = db.GetNullableDate(reader, 7);
        entities.ExistingObligation.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingObligation.OrderTypeCode = db.GetString(reader, 10);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingObligation.OrderTypeCode);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligationRln.Populated);
    entities.ExistingConcurrentObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId", entities.ExistingObligationRln.OtyFirstId);
          
        db.SetInt32(
          command, "obId", entities.ExistingObligationRln.ObgFGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ExistingObligationRln.CspFNumber);
        db.
          SetString(command, "cpaType", entities.ExistingObligationRln.CpaFType);
          
      },
      (db, reader) =>
      {
        entities.ExistingConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingConcurrentObligation.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingConcurrentObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingConcurrentObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.ExistingConcurrentObligation.LastCollAmt =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingConcurrentObligation.LastCollDt =
          db.GetNullableDate(reader, 6);
        entities.ExistingConcurrentObligation.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ExistingConcurrentObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingConcurrentObligation.OrderTypeCode =
          db.GetString(reader, 9);
        entities.ExistingConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.ExistingConcurrentObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingConcurrentObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingConcurrentObligation.OrderTypeCode);
      });
  }

  private bool ReadObligation3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligationRln.Populated);
    entities.ExistingConcurrentObligation.Populated = false;

    return Read("ReadObligation3",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ExistingObligationRln.OtySecondId);
        db.SetInt32(
          command, "obId", entities.ExistingObligationRln.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ExistingObligationRln.CspNumber);
        db.
          SetString(command, "cpaType", entities.ExistingObligationRln.CpaType);
          
      },
      (db, reader) =>
      {
        entities.ExistingConcurrentObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingConcurrentObligation.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingConcurrentObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingConcurrentObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingConcurrentObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.ExistingConcurrentObligation.LastCollAmt =
          db.GetNullableDecimal(reader, 5);
        entities.ExistingConcurrentObligation.LastCollDt =
          db.GetNullableDate(reader, 6);
        entities.ExistingConcurrentObligation.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ExistingConcurrentObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingConcurrentObligation.OrderTypeCode =
          db.GetString(reader, 9);
        entities.ExistingConcurrentObligation.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.ExistingConcurrentObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingConcurrentObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.ExistingConcurrentObligation.OrderTypeCode);
      });
  }

  private bool ReadObligationRln1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationRln.Populated = false;

    return Read("ReadObligationRln1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyFirstId", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspFNumber", entities.ExistingObligation.CspNumber);
        db.SetString(command, "cpaFType", entities.ExistingObligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          import.HardcodedJointSeveralObligationRlnRsn.
            SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligationRln.CpaType = db.GetString(reader, 2);
        entities.ExistingObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ExistingObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ExistingObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingObligationRln.CreatedTmst = db.GetDateTime(reader, 7);
        entities.ExistingObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ExistingObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ExistingObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType",
          entities.ExistingObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType",
          entities.ExistingObligationRln.CpaFType);
      });
  }

  private bool ReadObligationRln2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationRln.Populated = false;

    return Read("ReadObligationRln2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otySecondId", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetInt32(
          command, "orrGeneratedId",
          import.HardcodedJointSeveralObligationRlnRsn.
            SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligationRln.CpaType = db.GetString(reader, 2);
        entities.ExistingObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ExistingObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ExistingObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingObligationRln.CreatedTmst = db.GetDateTime(reader, 7);
        entities.ExistingObligationRln.OtySecondId = db.GetInt32(reader, 8);
        entities.ExistingObligationRln.OtyFirstId = db.GetInt32(reader, 9);
        entities.ExistingObligationRln.Populated = true;
        CheckValid<ObligationRln>("CpaType",
          entities.ExistingObligationRln.CpaType);
        CheckValid<ObligationRln>("CpaFType",
          entities.ExistingObligationRln.CpaFType);
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistantCashReceiptDetail.Populated);

    var lastUpdatedBy = import.UserId.Text8;
    var lastUpdatedTmst = Now();
    var distributedAmount =
      local.ForUpdateCashReceiptDetail.DistributedAmount.GetValueOrDefault();
    var collectionAmtFullyAppliedInd =
      local.ForUpdateCashReceiptDetail.CollectionAmtFullyAppliedInd ?? "";
    var overrideManualDistInd = "Y";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    import.PersistantCashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "distributedAmt", distributedAmount);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetNullableString(command, "ovrrdMnlDistInd", overrideManualDistInd);
        db.SetInt32(
          command, "crvIdentifier",
          import.PersistantCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.PersistantCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.PersistantCashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId",
          import.PersistantCashReceiptDetail.SequentialIdentifier);
      });

    import.PersistantCashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    import.PersistantCashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    import.PersistantCashReceiptDetail.DistributedAmount = distributedAmount;
    import.PersistantCashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    import.PersistantCashReceiptDetail.OverrideManualDistInd =
      overrideManualDistInd;
    import.PersistantCashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCashReceiptDetailStatHistory.Populated);

    var discontinueDate = import.Current.Date;

    entities.ExistingCashReceiptDetailStatHistory.Populated = false;
    Update("UpdateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier);
        db.SetInt32(
          command, "cdsIdentifier",
          entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
      discontinueDate;
    entities.ExistingCashReceiptDetailStatHistory.Populated = true;
  }

  private void UpdateObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);

    var lastCollAmt = local.ForUpdateObligation.LastCollAmt.GetValueOrDefault();
    var lastCollDt = import.PersistantCashReceiptDetail.CollectionDate;
    var lastUpdatedBy = import.UserId.Text8;
    var lastUpdateTmst = import.Current.Timestamp;

    entities.ExistingObligation.Populated = false;
    Update("UpdateObligation1",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "lastPymntAmt", lastCollAmt);
        db.SetNullableDate(command, "lastPymntDt", lastCollDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetInt32(
          command, "obId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ExistingObligation.DtyGeneratedId);
      });

    entities.ExistingObligation.LastCollAmt = lastCollAmt;
    entities.ExistingObligation.LastCollDt = lastCollDt;
    entities.ExistingObligation.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingObligation.LastUpdateTmst = lastUpdateTmst;
    entities.ExistingObligation.Populated = true;
  }

  private void UpdateObligation2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingConcurrentObligation.Populated);

    var lastCollAmt = local.ForUpdateObligation.LastCollAmt.GetValueOrDefault();
    var lastCollDt = import.PersistantCashReceiptDetail.CollectionDate;
    var lastUpdatedBy = import.UserId.Text8;
    var lastUpdateTmst = import.Current.Timestamp;

    entities.ExistingConcurrentObligation.Populated = false;
    Update("UpdateObligation2",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "lastPymntAmt", lastCollAmt);
        db.SetNullableDate(command, "lastPymntDt", lastCollDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(
          command, "cpaType", entities.ExistingConcurrentObligation.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.ExistingConcurrentObligation.CspNumber);
        db.SetInt32(
          command, "obId",
          entities.ExistingConcurrentObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ExistingConcurrentObligation.DtyGeneratedId);
      });

    entities.ExistingConcurrentObligation.LastCollAmt = lastCollAmt;
    entities.ExistingConcurrentObligation.LastCollDt = lastCollDt;
    entities.ExistingConcurrentObligation.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingConcurrentObligation.LastUpdateTmst = lastUpdateTmst;
    entities.ExistingConcurrentObligation.Populated = true;
  }

  private void UpdateObligor()
  {
    System.Diagnostics.Debug.Assert(import.PersistantObligor.Populated);

    var lastUpdatedBy = import.UserId.Text8;
    var lastUpdatedTmst = import.Current.Timestamp;
    var lastManualDistributionDate =
      local.ForUpdateObligor.LastManualDistributionDate;
    var lastCollAmt = local.ForUpdateObligor.LastCollAmt.GetValueOrDefault();
    var lastCollDt = import.PersistantCashReceiptDetail.CollectionDate;

    import.PersistantObligor.Populated = false;
    Update("UpdateObligor",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetNullableDate(command, "lastManDistDt", lastManualDistributionDate);
          
        db.SetNullableDecimal(command, "lastColAmt", lastCollAmt);
        db.SetNullableDate(command, "lastColDt", lastCollDt);
        db.SetString(command, "cspNumber", import.PersistantObligor.CspNumber);
        db.SetString(command, "type", import.PersistantObligor.Type1);
      });

    import.PersistantObligor.LastUpdatedBy = lastUpdatedBy;
    import.PersistantObligor.LastUpdatedTmst = lastUpdatedTmst;
    import.PersistantObligor.LastManualDistributionDate =
      lastManualDistributionDate;
    import.PersistantObligor.LastCollAmt = lastCollAmt;
    import.PersistantObligor.LastCollDt = lastCollDt;
    import.PersistantObligor.Populated = true;
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
        LegalDtlGroup.Capacity);

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

    /// <summary>
    /// A value of PersistantCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("persistantCashReceiptDetail")]
    public CashReceiptDetail PersistantCashReceiptDetail
    {
      get => persistantCashReceiptDetail ??= new();
      set => persistantCashReceiptDetail = value;
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
    /// A value of PersistantObligor.
    /// </summary>
    [JsonPropertyName("persistantObligor")]
    public CsePersonAccount PersistantObligor
    {
      get => persistantObligor ??= new();
      set => persistantObligor = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public CashReceiptDetail Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity);

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
    /// A value of Hardcoded718B.
    /// </summary>
    [JsonPropertyName("hardcoded718B")]
    public ObligationType Hardcoded718B
    {
      get => hardcoded718B ??= new();
      set => hardcoded718B = value;
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
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
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
    /// A value of HardcodedJointSeveralObligation.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveralObligation")]
    public Obligation HardcodedJointSeveralObligation
    {
      get => hardcodedJointSeveralObligation ??= new();
      set => hardcodedJointSeveralObligation = value;
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
    /// A value of HardcodedSType.
    /// </summary>
    [JsonPropertyName("hardcodedSType")]
    public CollectionType HardcodedSType
    {
      get => hardcodedSType ??= new();
      set => hardcodedSType = value;
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
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
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
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
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
    /// A value of HardcodedDeactivedStat.
    /// </summary>
    [JsonPropertyName("hardcodedDeactivedStat")]
    public DebtDetailStatusHistory HardcodedDeactivedStat
    {
      get => hardcodedDeactivedStat ??= new();
      set => hardcodedDeactivedStat = value;
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
    /// A value of HardcodedArrears.
    /// </summary>
    [JsonPropertyName("hardcodedArrears")]
    public Collection HardcodedArrears
    {
      get => hardcodedArrears ??= new();
      set => hardcodedArrears = value;
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
    /// A value of HardcodedAfType.
    /// </summary>
    [JsonPropertyName("hardcodedAfType")]
    public Program HardcodedAfType
    {
      get => hardcodedAfType ??= new();
      set => hardcodedAfType = value;
    }

    /// <summary>
    /// A value of HardcodedFcType.
    /// </summary>
    [JsonPropertyName("hardcodedFcType")]
    public Program HardcodedFcType
    {
      get => hardcodedFcType ??= new();
      set => hardcodedFcType = value;
    }

    /// <summary>
    /// A value of HardcodedNfType.
    /// </summary>
    [JsonPropertyName("hardcodedNfType")]
    public Program HardcodedNfType
    {
      get => hardcodedNfType ??= new();
      set => hardcodedNfType = value;
    }

    /// <summary>
    /// A value of HardcodedNcType.
    /// </summary>
    [JsonPropertyName("hardcodedNcType")]
    public Program HardcodedNcType
    {
      get => hardcodedNcType ??= new();
      set => hardcodedNcType = value;
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
    /// A value of ForMnlDistOnly.
    /// </summary>
    [JsonPropertyName("forMnlDistOnly")]
    public Collection ForMnlDistOnly
    {
      get => forMnlDistOnly ??= new();
      set => forMnlDistOnly = value;
    }

    private CashReceiptDetail persistantCashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CsePersonAccount persistantObligor;
    private CashReceiptDetail adjusted;
    private Collection autoOrManual;
    private CollectionType collectionType;
    private CashReceiptSourceType cashReceiptSourceType;
    private Array<GroupGroup> group;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private DateWorkArea current;
    private TextWorkArea userId;
    private ObligationType hardcoded718B;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private Obligation hardcodedPrimary;
    private Obligation hardcodedJointSeveralObligation;
    private CollectionType hardcodedFType;
    private CollectionType hardcodedSType;
    private CashReceiptType hardcodedCashType;
    private CashReceiptType hardcodedFcourtPmt;
    private CashReceiptType hardcodedFdirPmt;
    private Obligation hardcodedSecondary;
    private ObligationRlnRsn hardcodedJointSeveralObligationRlnRsn;
    private ObligationType hardcodedAccruingClass;
    private CashReceiptDetailStatus hardcodedDistributed;
    private DebtDetailStatusHistory hardcodedDeactivedStat;
    private Collection hardcodedCurrent;
    private Collection hardcodedArrears;
    private ObligationType hardcodedVolClass;
    private Program hardcodedAfType;
    private Program hardcodedFcType;
    private Program hardcodedNfType;
    private Program hardcodedNcType;
    private DprProgram hardcodedUk;
    private Collection forMnlDistOnly;
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
    /// <summary>
    /// A value of UpdateMatchingConcurrOb.
    /// </summary>
    [JsonPropertyName("updateMatchingConcurrOb")]
    public Common UpdateMatchingConcurrOb
    {
      get => updateMatchingConcurrOb ??= new();
      set => updateMatchingConcurrOb = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public DebtDetail Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentTimestamp.
    /// </summary>
    [JsonPropertyName("currentTimestamp")]
    public DateWorkArea CurrentTimestamp
    {
      get => currentTimestamp ??= new();
      set => currentTimestamp = value;
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
    /// A value of ForUpdateObligation.
    /// </summary>
    [JsonPropertyName("forUpdateObligation")]
    public Obligation ForUpdateObligation
    {
      get => forUpdateObligation ??= new();
      set => forUpdateObligation = value;
    }

    /// <summary>
    /// A value of UpdateAttempt.
    /// </summary>
    [JsonPropertyName("updateAttempt")]
    public Common UpdateAttempt
    {
      get => updateAttempt ??= new();
      set => updateAttempt = value;
    }

    /// <summary>
    /// A value of ForUpdateCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("forUpdateCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ForUpdateCashReceiptDetailStatHistory
    {
      get => forUpdateCashReceiptDetailStatHistory ??= new();
      set => forUpdateCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ForUpdateCollection.
    /// </summary>
    [JsonPropertyName("forUpdateCollection")]
    public Collection ForUpdateCollection
    {
      get => forUpdateCollection ??= new();
      set => forUpdateCollection = value;
    }

    /// <summary>
    /// A value of ForUpdateObligor.
    /// </summary>
    [JsonPropertyName("forUpdateObligor")]
    public CsePersonAccount ForUpdateObligor
    {
      get => forUpdateObligor ??= new();
      set => forUpdateObligor = value;
    }

    /// <summary>
    /// A value of ForUpdateDebtDetail.
    /// </summary>
    [JsonPropertyName("forUpdateDebtDetail")]
    public DebtDetail ForUpdateDebtDetail
    {
      get => forUpdateDebtDetail ??= new();
      set => forUpdateDebtDetail = value;
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
    /// A value of CollectionMonthStartDt.
    /// </summary>
    [JsonPropertyName("collectionMonthStartDt")]
    public DateWorkArea CollectionMonthStartDt
    {
      get => collectionMonthStartDt ??= new();
      set => collectionMonthStartDt = value;
    }

    /// <summary>
    /// A value of CollectionMonthEndDt.
    /// </summary>
    [JsonPropertyName("collectionMonthEndDt")]
    public DateWorkArea CollectionMonthEndDt
    {
      get => collectionMonthEndDt ??= new();
      set => collectionMonthEndDt = value;
    }

    /// <summary>
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of MaxDiscontinueDate.
    /// </summary>
    [JsonPropertyName("maxDiscontinueDate")]
    public DateWorkArea MaxDiscontinueDate
    {
      get => maxDiscontinueDate ??= new();
      set => maxDiscontinueDate = value;
    }

    /// <summary>
    /// A value of ForUpdateCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("forUpdateCashReceiptDetail")]
    public CashReceiptDetail ForUpdateCashReceiptDetail
    {
      get => forUpdateCashReceiptDetail ??= new();
      set => forUpdateCashReceiptDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of HardcodedSTypeMisc.
    /// </summary>
    [JsonPropertyName("hardcodedSTypeMisc")]
    public CollectionType HardcodedSTypeMisc
    {
      get => hardcodedSTypeMisc ??= new();
      set => hardcodedSTypeMisc = value;
    }

    /// <summary>
    /// A value of HardcodedSTypeUnemploy.
    /// </summary>
    [JsonPropertyName("hardcodedSTypeUnemploy")]
    public CollectionType HardcodedSTypeUnemploy
    {
      get => hardcodedSTypeUnemploy ??= new();
      set => hardcodedSTypeUnemploy = value;
    }

    /// <summary>
    /// A value of HardcodedSTypeKpers.
    /// </summary>
    [JsonPropertyName("hardcodedSTypeKpers")]
    public CollectionType HardcodedSTypeKpers
    {
      get => hardcodedSTypeKpers ??= new();
      set => hardcodedSTypeKpers = value;
    }

    /// <summary>
    /// A value of HardcodedSTypeRecovery.
    /// </summary>
    [JsonPropertyName("hardcodedSTypeRecovery")]
    public CollectionType HardcodedSTypeRecovery
    {
      get => hardcodedSTypeRecovery ??= new();
      set => hardcodedSTypeRecovery = value;
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
    /// A value of HardcodedFTypeSalary.
    /// </summary>
    [JsonPropertyName("hardcodedFTypeSalary")]
    public CollectionType HardcodedFTypeSalary
    {
      get => hardcodedFTypeSalary ??= new();
      set => hardcodedFTypeSalary = value;
    }

    /// <summary>
    /// A value of HardcodedFTypeRetire.
    /// </summary>
    [JsonPropertyName("hardcodedFTypeRetire")]
    public CollectionType HardcodedFTypeRetire
    {
      get => hardcodedFTypeRetire ??= new();
      set => hardcodedFTypeRetire = value;
    }

    /// <summary>
    /// A value of HardcodedFTypeVendor.
    /// </summary>
    [JsonPropertyName("hardcodedFTypeVendor")]
    public CollectionType HardcodedFTypeVendor
    {
      get => hardcodedFTypeVendor ??= new();
      set => hardcodedFTypeVendor = value;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    private Common updateMatchingConcurrOb;
    private DebtDetail tmp;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea currentTimestamp;
    private TextWorkArea userId;
    private Obligation forUpdateObligation;
    private Common updateAttempt;
    private CashReceiptDetailStatHistory forUpdateCashReceiptDetailStatHistory;
    private Collection forUpdateCollection;
    private CsePersonAccount forUpdateObligor;
    private DebtDetail forUpdateDebtDetail;
    private DateWorkArea collection;
    private DateWorkArea collectionMonthStartDt;
    private DateWorkArea collectionMonthEndDt;
    private DateWorkArea dueDate;
    private DateWorkArea maxDiscontinueDate;
    private CashReceiptDetail forUpdateCashReceiptDetail;
    private DateWorkArea current;
    private LegalAction legalAction;
    private LegalAction nullLegalAction;
    private CollectionType hardcodedSTypeMisc;
    private CollectionType hardcodedSTypeUnemploy;
    private CollectionType hardcodedSTypeKpers;
    private CollectionType hardcodedSTypeRecovery;
    private CollectionType hardcodedFType;
    private CollectionType hardcodedFTypeSalary;
    private CollectionType hardcodedFTypeRetire;
    private CollectionType hardcodedFTypeVendor;
    private CsenetStateTable csenetStateTable;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligorKeyOnly.
    /// </summary>
    [JsonPropertyName("existingObligorKeyOnly")]
    public CsePerson ExistingObligorKeyOnly
    {
      get => existingObligorKeyOnly ??= new();
      set => existingObligorKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnly.
    /// </summary>
    [JsonPropertyName("existingKeyOnly")]
    public ObligationType ExistingKeyOnly
    {
      get => existingKeyOnly ??= new();
      set => existingKeyOnly = value;
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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
    /// A value of ExistingSupportedKeyOnly.
    /// </summary>
    [JsonPropertyName("existingSupportedKeyOnly")]
    public CsePerson ExistingSupportedKeyOnly
    {
      get => existingSupportedKeyOnly ??= new();
      set => existingSupportedKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingSupported.
    /// </summary>
    [JsonPropertyName("existingSupported")]
    public CsePersonAccount ExistingSupported
    {
      get => existingSupported ??= new();
      set => existingSupported = value;
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

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailStatHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("existingObligationRlnRsn")]
    public ObligationRlnRsn ExistingObligationRlnRsn
    {
      get => existingObligationRlnRsn ??= new();
      set => existingObligationRlnRsn = value;
    }

    /// <summary>
    /// A value of ExistingObligationRln.
    /// </summary>
    [JsonPropertyName("existingObligationRln")]
    public ObligationRln ExistingObligationRln
    {
      get => existingObligationRln ??= new();
      set => existingObligationRln = value;
    }

    /// <summary>
    /// A value of ExistingConcurrentObligation.
    /// </summary>
    [JsonPropertyName("existingConcurrentObligation")]
    public Obligation ExistingConcurrentObligation
    {
      get => existingConcurrentObligation ??= new();
      set => existingConcurrentObligation = value;
    }

    /// <summary>
    /// A value of ExistingConcurrentDebt.
    /// </summary>
    [JsonPropertyName("existingConcurrentDebt")]
    public ObligationTransaction ExistingConcurrentDebt
    {
      get => existingConcurrentDebt ??= new();
      set => existingConcurrentDebt = value;
    }

    /// <summary>
    /// A value of ExistingConcurrentDebtDetail.
    /// </summary>
    [JsonPropertyName("existingConcurrentDebtDetail")]
    public DebtDetail ExistingConcurrentDebtDetail
    {
      get => existingConcurrentDebtDetail ??= new();
      set => existingConcurrentDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CaseRole ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    /// <summary>
    /// A value of ExistingApplicantRecipient.
    /// </summary>
    [JsonPropertyName("existingApplicantRecipient")]
    public CaseRole ExistingApplicantRecipient
    {
      get => existingApplicantRecipient ??= new();
      set => existingApplicantRecipient = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalActionCaseRole")]
    public LegalActionCaseRole ExistingLegalActionCaseRole
    {
      get => existingLegalActionCaseRole ??= new();
      set => existingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("existingInterstateRequestHistory")]
    public InterstateRequestHistory ExistingInterstateRequestHistory
    {
      get => existingInterstateRequestHistory ??= new();
      set => existingInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of ExistingObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("existingObligCollProtectionHist")]
    public ObligCollProtectionHist ExistingObligCollProtectionHist
    {
      get => existingObligCollProtectionHist ??= new();
      set => existingObligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public InterstateRequestObligation DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private CsePerson existingObligorKeyOnly;
    private ObligationType existingKeyOnly;
    private Obligation existingObligation;
    private LegalAction existingLegalAction;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private CsePerson existingSupportedKeyOnly;
    private CsePersonAccount existingSupported;
    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailStatHistory new1;
    private ObligationRlnRsn existingObligationRlnRsn;
    private ObligationRln existingObligationRln;
    private Obligation existingConcurrentObligation;
    private ObligationTransaction existingConcurrentDebt;
    private DebtDetail existingConcurrentDebtDetail;
    private Case1 existingCase;
    private CaseRole existingAbsentParent;
    private CaseRole existingChild;
    private InterstateRequest existingInterstateRequest;
    private CaseRole existingApplicantRecipient;
    private Tribunal existingTribunal;
    private Fips existingFips;
    private LegalActionCaseRole existingLegalActionCaseRole;
    private InterstateRequestHistory existingInterstateRequestHistory;
    private ObligCollProtectionHist existingObligCollProtectionHist;
    private InterstateRequestObligation delMe;
    private Fips fips;
  }
#endregion
}
