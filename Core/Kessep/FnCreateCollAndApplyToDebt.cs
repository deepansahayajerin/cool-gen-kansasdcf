// Program: FN_CREATE_COLL_AND_APPLY_TO_DEBT, ID: 372280614, model: 746.
// Short name: SWE02254
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_COLL_AND_APPLY_TO_DEBT.
/// </summary>
[Serializable]
public partial class FnCreateCollAndApplyToDebt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_COLL_AND_APPLY_TO_DEBT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCollAndApplyToDebt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCollAndApplyToDebt.
  /// </summary>
  public FnCreateCollAndApplyToDebt(IContext context, Import import,
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
    // Date	  Developer	Request #	Description
    // --------  ------------	------------	
    // ---------------------------------------------------------------------------------------
    // ?	  ?		?		Original Development.
    // 09/29/09  GVandy	CQ13297		Save the current collection adjustment date and
    // the date the collection
    // 					is unadjusted for processing in OCSE34.
    // -------------------------------------------------------------------------------------------------------------------------------
    // : Ensure that the require persistant entity views are populated and 
    // locked.
    if (!import.PersistantCashReceiptDetail.Populated)
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    if (!import.PersistantDebt.Populated)
    {
      ExitState = "FN0000_OBLIG_TRANS_NF_RB";

      return;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      ExitState = "FN0000_OBLIG_TRANS_NF_RB";

      return;
    }

    if (!import.PersistantDebtDetail.Populated)
    {
      ExitState = "FN0000_DEBT_DETAIL_NF_RB";

      return;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      ExitState = "FN0000_DEBT_DETAIL_NF_RB";

      return;
    }

    if (!ReadObligation())
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";

      return;
    }

    if (!ReadCsePerson())
    {
      ExitState = "FN0000_OBLIGOR_NF_RB";

      return;
    }

    local.ForUpdate.Assign(import.ForUpdateDebtDetail);
    local.Current.Date = Now().Date;
    local.ApplyUpdates.Flag = "Y";

    // : Read for a previously adjusted collection.  If one is found and it 
    // matches the new collection perfectly (the CRD redistributed exactly the
    // same), then mark the collection as NOT BEING ADJUSTED.
    if (ReadCollection())
    {
      if (ReadCollectionAdjustmentReason())
      {
        DisassociateCollection();

        // -- 09/29/2009  GVandy  CQ13297  Save the current collection 
        // adjustment date and the date the collection is unadjusted for
        // processing in OCSE34.
        local.Collection.PreviousCollectionAdjDate =
          entities.ExistingCollection.CollectionAdjustmentDt;
        local.Collection.UnadjustedDate = import.CurrentTimestamp.Date;

        try
        {
          UpdateCollection();

          // : Apply AF & FC Collections to URA.
          if (Equal(import.ForUpdateCollection.ProgramAppliedTo, "AF") || Equal
            (import.ForUpdateCollection.ProgramAppliedTo, "FC"))
          {
            if (AsChar(import.ForUpdateCollection.ConcurrentInd) == 'Y')
            {
              goto Test1;
            }

            if (Equal(import.ForUpdateCollection.DistPgmStateAppldTo,
              import.HardcodedUk.ProgramState))
            {
              goto Test1;
            }

            if (AsChar(import.Obligation.PrimarySecondaryCode) == AsChar
              (import.HardcodedSecondary.PrimarySecondaryCode))
            {
              goto Test1;
            }

            UseFnApplyCollectionToUra1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

Test1:
          ;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_COLLECTION_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_COLLECTION_PV_RB";

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
        ExitState = "FN0000_COLL_ADJUST_REASON_NF_RB";

        return;
      }
    }
    else
    {
      // : Create a Collection.
      for(local.UpdateAttempt.Count = 1; local.UpdateAttempt.Count <= 5; ++
        local.UpdateAttempt.Count)
      {
        try
        {
          CreateCollection();

          // : Apply AF & FC Collections to URA.
          if (Equal(import.ForUpdateCollection.ProgramAppliedTo, "AF") || Equal
            (import.ForUpdateCollection.ProgramAppliedTo, "FC"))
          {
            if (AsChar(import.ForUpdateCollection.ConcurrentInd) == 'Y')
            {
              goto Test2;
            }

            if (Equal(import.ForUpdateCollection.DistPgmStateAppldTo,
              import.HardcodedUk.ProgramState))
            {
              goto Test2;
            }

            if (AsChar(import.Obligation.PrimarySecondaryCode) == AsChar
              (import.HardcodedSecondary.PrimarySecondaryCode))
            {
              goto Test2;
            }

            UseFnApplyCollectionToUra2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

Test2:

          break;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              if (local.UpdateAttempt.Count >= 5)
              {
                ExitState = "FN0000_COLLECTION_AE_RB";

                return;
              }
              else
              {
                continue;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_COLLECTION_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    // : Update the Debt Detail.
    if (AsChar(import.ObligationType.Classification) == AsChar
      (import.HardcodedVolClass.Classification))
    {
      try
      {
        UpdateDebtDetail2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0214_DEBT_DETAIL_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0218_DEBT_DETAIL_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else if (AsChar(import.ForUpdateCollection.AppliedToCode) == 'G')
    {
      try
      {
        UpdateDebtDetail2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0214_DEBT_DETAIL_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0218_DEBT_DETAIL_PV_RB";

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
      if (local.ForUpdate.BalanceDueAmt == 0 && local
        .ForUpdate.InterestBalanceDueAmt.GetValueOrDefault() == 0)
      {
        local.ForUpdate.RetiredDt =
          import.PersistantCashReceiptDetail.CollectionDate;

        if (ReadDebtDetailStatusHistory())
        {
          try
          {
            UpdateDebtDetailStatusHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DEBT_DETL_STAT_HIST_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DEBT_DETL_STAT_HIST_PV_RB";

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
          ExitState = "FN0000_DEBT_DETL_STAT_HIST_NF_RB";

          return;
        }

        for(local.UpdateAttempt.Count = 1; local.UpdateAttempt.Count <= 50; ++
          local.UpdateAttempt.Count)
        {
          try
          {
            CreateDebtDetailStatusHistory();

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (local.UpdateAttempt.Count >= 50)
                {
                  ExitState = "FN0000_DEBT_DETL_STAT_HIST_AE_R";

                  return;
                }
                else
                {
                  continue;
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DEBT_DETL_STAT_HIST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      try
      {
        UpdateDebtDetail1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0214_DEBT_DETAIL_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0218_DEBT_DETAIL_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    try
    {
      UpdateDebt();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_OBLIG_TRANS_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_OBLIG_TRANS_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveHhHist1(FnApplyCollectionToUra.Import.
    HhHistGroup source, Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(Import.HhHistGroup source,
    FnApplyCollectionToUra.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl2);
  }

  private static void MoveHhHistDtl1(FnApplyCollectionToUra.Import.
    HhHistDtlGroup source, Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(Import.HhHistDtlGroup source,
    FnApplyCollectionToUra.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveLegal(Import.LegalGroup source,
    FnApplyCollectionToUra.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl);
  }

  private static void MoveLegalDtl(Import.LegalDtlGroup source,
    FnApplyCollectionToUra.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void UseFnApplyCollectionToUra1()
  {
    var useImport = new FnApplyCollectionToUra.Import();
    var useExport = new FnApplyCollectionToUra.Export();

    useImport.Obligor.Number = entities.ExistingObligorKeyOnly.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ExistingKeyOnlyObligation.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      import.PersistantDebt.SystemGeneratedIdentifier;
    useImport.ApplyUpdates.Flag = local.ApplyUpdates.Flag;
    MoveCollection(entities.ExistingCollection, useImport.Collection2);
    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    useImport.SuppPrsn.Number = import.SuppPrsn.Number;
    useImport.Collection1.Date = import.Collection.Date;
    MoveObligationType(import.ObligationType, useImport.ObligationType);
    useImport.DebtDetail.DueDt = import.PersistantDebtDetail.DueDt;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    import.Legal.CopyTo(useImport.Legal, MoveLegal);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);

    Call(FnApplyCollectionToUra.Execute, useImport, useExport);

    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist1);
  }

  private void UseFnApplyCollectionToUra2()
  {
    var useImport = new FnApplyCollectionToUra.Import();
    var useExport = new FnApplyCollectionToUra.Export();

    useImport.Obligor.Number = entities.ExistingObligorKeyOnly.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ExistingKeyOnlyObligation.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      import.PersistantDebt.SystemGeneratedIdentifier;
    useImport.ApplyUpdates.Flag = local.ApplyUpdates.Flag;
    MoveCollection(entities.NewCollection, useImport.Collection2);
    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    useImport.SuppPrsn.Number = import.SuppPrsn.Number;
    useImport.Collection1.Date = import.Collection.Date;
    MoveObligationType(import.ObligationType, useImport.ObligationType);
    useImport.DebtDetail.DueDt = import.PersistantDebtDetail.DueDt;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      import.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      import.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      import.HardcodedMcType.SystemGeneratedIdentifier;
    import.Legal.CopyTo(useImport.Legal, MoveLegal);
    import.HhHist.CopyTo(useImport.HhHist, MoveHhHist2);

    Call(FnApplyCollectionToUra.Execute, useImport, useExport);

    useImport.HhHist.CopyTo(import.HhHist, MoveHhHist1);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCollection()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistantCashReceiptDetail.Populated);
    System.Diagnostics.Debug.Assert(import.PersistantDebt.Populated);

    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var appliedToCode = import.ForUpdateCollection.AppliedToCode;
    var collectionDt = import.ForUpdateCollection.CollectionDt;
    var adjustedInd = import.ForUpdateCollection.AdjustedInd ?? "";
    var concurrentInd = import.ForUpdateCollection.ConcurrentInd;
    var disbursementAdjProcessDate = local.Null1.Date;
    var crtType = import.PersistantCashReceiptDetail.CrtIdentifier;
    var cstId = import.PersistantCashReceiptDetail.CstIdentifier;
    var crvId = import.PersistantCashReceiptDetail.CrvIdentifier;
    var crdId = import.PersistantCashReceiptDetail.SequentialIdentifier;
    var obgId = import.PersistantDebt.ObgGeneratedId;
    var cspNumber = import.PersistantDebt.CspNumber;
    var cpaType = import.PersistantDebt.CpaType;
    var otrId = import.PersistantDebt.SystemGeneratedIdentifier;
    var otrType = import.PersistantDebt.Type1;
    var otyId = import.PersistantDebt.OtyType;
    var createdBy = import.UserId.Text8;
    var createdTmst = import.CurrentTimestamp.Timestamp;
    var amount = import.ForUpdateCollection.Amount;
    var disbursementProcessingNeedInd =
      import.ForUpdateCollection.DisbursementProcessingNeedInd ?? "";
    var distributionMethod = import.ForUpdateCollection.DistributionMethod;
    var programAppliedTo = import.ForUpdateCollection.ProgramAppliedTo;
    var appliedToOrderTypeCode =
      import.ForUpdateCollection.AppliedToOrderTypeCode;
    var courtNoticeReqInd = import.ForUpdateCollection.CourtNoticeReqInd ?? "";
    var disburseToArInd = import.ForUpdateCollection.DisburseToArInd ?? "";
    var manualDistributionReasonText =
      import.ForUpdateCollection.ManualDistributionReasonText ?? "";
    var courtOrderAppliedTo =
      import.ForUpdateCollection.CourtOrderAppliedTo ?? "";
    var appliedToFuture = import.ForUpdateCollection.AppliedToFuture;
    var csenetOutboundReqInd = import.ForUpdateCollection.CsenetOutboundReqInd;
    var distPgmStateAppldTo =
      import.ForUpdateCollection.DistPgmStateAppldTo ?? "";

    CheckValid<Collection>("AppliedToCode", appliedToCode);
    CheckValid<Collection>("AdjustedInd", adjustedInd);
    CheckValid<Collection>("ConcurrentInd", concurrentInd);
    CheckValid<Collection>("CpaType", cpaType);
    CheckValid<Collection>("OtrType", otrType);
    CheckValid<Collection>("DisbursementProcessingNeedInd",
      disbursementProcessingNeedInd);
    CheckValid<Collection>("DistributionMethod", distributionMethod);
    CheckValid<Collection>("ProgramAppliedTo", programAppliedTo);
    CheckValid<Collection>("AppliedToOrderTypeCode", appliedToOrderTypeCode);
    CheckValid<Collection>("AppliedToFuture", appliedToFuture);
    CheckValid<Collection>("CsenetOutboundReqInd", csenetOutboundReqInd);
    entities.NewCollection.Populated = false;
    Update("CreateCollection",
      (db, command) =>
      {
        db.SetInt32(command, "collId", systemGeneratedIdentifier);
        db.SetString(command, "appliedToCd", appliedToCode);
        db.SetDate(command, "collDt", collectionDt);
        db.SetNullableDate(command, "disbDt", null);
        db.SetNullableString(command, "adjInd", adjustedInd);
        db.SetString(command, "concurrentInd", concurrentInd);
        db.SetDate(command, "disbAdjProcDate", disbursementAdjProcessDate);
        db.SetInt32(command, "crtType", crtType);
        db.SetInt32(command, "cstId", cstId);
        db.SetInt32(command, "crvId", crvId);
        db.SetInt32(command, "crdId", crdId);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "otrType", otrType);
        db.SetNullableDate(command, "prevCollAdjDt", default(DateTime));
        db.SetInt32(command, "otyId", otyId);
        db.SetDate(command, "collAdjDt", disbursementAdjProcessDate);
        db.SetDate(command, "collAdjProcDate", disbursementAdjProcessDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetNullableString(
          command, "disbProcNeedInd", disbursementProcessingNeedInd);
        db.SetString(command, "distMtd", distributionMethod);
        db.SetString(command, "pgmAppldTo", programAppliedTo);
        db.SetString(command, "applToOrdTypCd", appliedToOrderTypeCode);
        db.SetNullableString(command, "crtNoticeReqInd", courtNoticeReqInd);
        db.SetNullableDate(command, "crtNoticeProcDt", null);
        db.SetNullableDecimal(command, "balBefColl", 0M);
        db.SetNullableString(command, "disbToArInd", disburseToArInd);
        db.SetNullableString(
          command, "mnlDistRsnTxt", manualDistributionReasonText);
        db.SetNullableString(command, "colAdjRsnTxt", "");
        db.SetNullableString(command, "ctOrdAppliedTo", courtOrderAppliedTo);
        db.SetString(command, "appliedToFutInd", appliedToFuture);
        db.SetString(command, "csenetObReqInd", csenetOutboundReqInd);
        db.SetNullableDate(command, "csenetObAdjPDt", null);
        db.SetDate(command, "crtNtcAdjPrcDt", null);
        db.SetNullableString(command, "pgmStAppldTo", distPgmStateAppldTo);
        db.SetNullableString(command, "arNumber", "");
      });

    entities.NewCollection.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewCollection.AppliedToCode = appliedToCode;
    entities.NewCollection.CollectionDt = collectionDt;
    entities.NewCollection.DisbursementDt = null;
    entities.NewCollection.AdjustedInd = adjustedInd;
    entities.NewCollection.ConcurrentInd = concurrentInd;
    entities.NewCollection.DisbursementAdjProcessDate =
      disbursementAdjProcessDate;
    entities.NewCollection.CrtType = crtType;
    entities.NewCollection.CstId = cstId;
    entities.NewCollection.CrvId = crvId;
    entities.NewCollection.CrdId = crdId;
    entities.NewCollection.ObgId = obgId;
    entities.NewCollection.CspNumber = cspNumber;
    entities.NewCollection.CpaType = cpaType;
    entities.NewCollection.OtrId = otrId;
    entities.NewCollection.OtrType = otrType;
    entities.NewCollection.OtyId = otyId;
    entities.NewCollection.CollectionAdjustmentDt = disbursementAdjProcessDate;
    entities.NewCollection.CollectionAdjProcessDate =
      disbursementAdjProcessDate;
    entities.NewCollection.CreatedBy = createdBy;
    entities.NewCollection.CreatedTmst = createdTmst;
    entities.NewCollection.LastUpdatedBy = "";
    entities.NewCollection.LastUpdatedTmst = null;
    entities.NewCollection.Amount = amount;
    entities.NewCollection.DisbursementProcessingNeedInd =
      disbursementProcessingNeedInd;
    entities.NewCollection.DistributionMethod = distributionMethod;
    entities.NewCollection.ProgramAppliedTo = programAppliedTo;
    entities.NewCollection.AppliedToOrderTypeCode = appliedToOrderTypeCode;
    entities.NewCollection.CourtNoticeReqInd = courtNoticeReqInd;
    entities.NewCollection.CourtNoticeProcessedDate = null;
    entities.NewCollection.DisburseToArInd = disburseToArInd;
    entities.NewCollection.ManualDistributionReasonText =
      manualDistributionReasonText;
    entities.NewCollection.CollectionAdjustmentReasonTxt = "";
    entities.NewCollection.CourtOrderAppliedTo = courtOrderAppliedTo;
    entities.NewCollection.AppliedToFuture = appliedToFuture;
    entities.NewCollection.CsenetOutboundReqInd = csenetOutboundReqInd;
    entities.NewCollection.CsenetOutboundAdjProjDt = null;
    entities.NewCollection.CourtNoticeAdjProcessDate = null;
    entities.NewCollection.DistPgmStateAppldTo = distPgmStateAppldTo;
    entities.NewCollection.Populated = true;
  }

  private void CreateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebtDetail.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDt = local.Current.Date;
    var discontinueDt = import.MaxDiscontinueDate.Date;
    var createdBy = import.UserId.Text8;
    var createdTmst = import.CurrentTimestamp.Timestamp;
    var otrType = import.PersistantDebtDetail.OtrType;
    var otrId = import.PersistantDebtDetail.OtrGeneratedId;
    var cpaType = import.PersistantDebtDetail.CpaType;
    var cspNumber = import.PersistantDebtDetail.CspNumber;
    var obgId = import.PersistantDebtDetail.ObgGeneratedId;
    var code = import.HardcodedDeactivedStatu.Code;
    var otyType = import.PersistantDebtDetail.OtyType;

    CheckValid<DebtDetailStatusHistory>("OtrType", otrType);
    CheckValid<DebtDetailStatusHistory>("CpaType", cpaType);
    entities.NewDebtDetailStatusHistory.Populated = false;
    Update("CreateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnStatHstId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "obTrnStCd", code);
        db.SetInt32(command, "otyType", otyType);
        db.SetNullableString(command, "rsnTxt", "");
      });

    entities.NewDebtDetailStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewDebtDetailStatusHistory.EffectiveDt = effectiveDt;
    entities.NewDebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.NewDebtDetailStatusHistory.CreatedBy = createdBy;
    entities.NewDebtDetailStatusHistory.CreatedTmst = createdTmst;
    entities.NewDebtDetailStatusHistory.OtrType = otrType;
    entities.NewDebtDetailStatusHistory.OtrId = otrId;
    entities.NewDebtDetailStatusHistory.CpaType = cpaType;
    entities.NewDebtDetailStatusHistory.CspNumber = cspNumber;
    entities.NewDebtDetailStatusHistory.ObgId = obgId;
    entities.NewDebtDetailStatusHistory.Code = code;
    entities.NewDebtDetailStatusHistory.OtyType = otyType;
    entities.NewDebtDetailStatusHistory.ReasonTxt = "";
    entities.NewDebtDetailStatusHistory.Populated = true;
  }

  private void DisassociateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCollection.Populated = false;
    Update("DisassociateCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.ExistingCollection.CrtType);
        db.SetInt32(command, "cstId", entities.ExistingCollection.CstId);
        db.SetInt32(command, "crvId", entities.ExistingCollection.CrvId);
        db.SetInt32(command, "crdId", entities.ExistingCollection.CrdId);
        db.SetInt32(command, "obgId", entities.ExistingCollection.ObgId);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.SetInt32(command, "otrId", entities.ExistingCollection.OtrId);
        db.SetString(command, "otrType", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "otyId", entities.ExistingCollection.OtyId);
      });

    entities.ExistingCollection.CarId = null;
    entities.ExistingCollection.Populated = true;
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistantCashReceiptDetail.Populated);
    System.Diagnostics.Debug.Assert(import.PersistantDebt.Populated);
    entities.ExistingCollection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          import.PersistantCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvId", import.PersistantCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstId", import.PersistantCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtType", import.PersistantCashReceiptDetail.CrtIdentifier);
          
        db.SetInt32(command, "otyId", import.PersistantDebt.OtyType);
        db.SetString(command, "otrType", import.PersistantDebt.Type1);
        db.SetInt32(
          command, "otrId", import.PersistantDebt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.PersistantDebt.CpaType);
        db.SetString(command, "cspNumber", import.PersistantDebt.CspNumber);
        db.SetInt32(command, "obgId", import.PersistantDebt.ObgGeneratedId);
        db.SetDecimal(command, "obTrnAmt", import.ForUpdateCollection.Amount);
        db.SetString(
          command, "appliedToCd", import.ForUpdateCollection.AppliedToCode);
        db.SetString(
          command, "applToOrdTypCd",
          import.ForUpdateCollection.AppliedToOrderTypeCode);
        db.SetNullableString(
          command, "disbToArInd",
          import.ForUpdateCollection.DisburseToArInd ?? "");
        db.SetString(
          command, "distMtd", import.ForUpdateCollection.DistributionMethod);
        db.SetString(
          command, "pgmAppldTo", import.ForUpdateCollection.ProgramAppliedTo);
        db.SetNullableString(
          command, "pgmStAppldTo",
          import.ForUpdateCollection.DistPgmStateAppldTo ?? "");
        db.SetDate(
          command, "collDt",
          import.ForUpdateCollection.CollectionDt.GetValueOrDefault());
        db.SetNullableString(
          command, "ctOrdAppliedTo",
          import.ForUpdateCollection.CourtOrderAppliedTo ?? "");
        db.SetString(
          command, "appliedToFutInd",
          import.ForUpdateCollection.AppliedToFuture);
        db.SetString(
          command, "concurrentInd", import.ForUpdateCollection.ConcurrentInd);
        db.SetString(
          command, "csenetObReqInd",
          import.ForUpdateCollection.CsenetOutboundReqInd);
        db.SetNullableString(
          command, "crtNoticeReqInd",
          import.ForUpdateCollection.CourtNoticeReqInd ?? "");
        db.SetNullableString(
          command, "disbProcNeedInd",
          import.ForUpdateCollection.DisbursementProcessingNeedInd ?? "");
        db.SetDate(
          command, "disbAdjProcDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AppliedToCode = db.GetString(reader, 1);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 2);
        entities.ExistingCollection.DisbursementDt =
          db.GetNullableDate(reader, 3);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 5);
        entities.ExistingCollection.DisbursementAdjProcessDate =
          db.GetDate(reader, 6);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 7);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 8);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 9);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 10);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 11);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 12);
        entities.ExistingCollection.CpaType = db.GetString(reader, 13);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 14);
        entities.ExistingCollection.OtrType = db.GetString(reader, 15);
        entities.ExistingCollection.PreviousCollectionAdjDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingCollection.CarId = db.GetNullableInt32(reader, 17);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 18);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 19);
        entities.ExistingCollection.CollectionAdjProcessDate =
          db.GetDate(reader, 20);
        entities.ExistingCollection.CreatedBy = db.GetString(reader, 21);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 22);
        entities.ExistingCollection.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 25);
        entities.ExistingCollection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 26);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 27);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 28);
        entities.ExistingCollection.AppliedToOrderTypeCode =
          db.GetString(reader, 29);
        entities.ExistingCollection.CourtNoticeReqInd =
          db.GetNullableString(reader, 30);
        entities.ExistingCollection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 31);
        entities.ExistingCollection.DisburseToArInd =
          db.GetNullableString(reader, 32);
        entities.ExistingCollection.ManualDistributionReasonText =
          db.GetNullableString(reader, 33);
        entities.ExistingCollection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 34);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 35);
        entities.ExistingCollection.AppliedToFuture = db.GetString(reader, 36);
        entities.ExistingCollection.CsenetOutboundReqInd =
          db.GetString(reader, 37);
        entities.ExistingCollection.CsenetOutboundAdjProjDt =
          db.GetNullableDate(reader, 38);
        entities.ExistingCollection.CourtNoticeAdjProcessDate =
          db.GetDate(reader, 39);
        entities.ExistingCollection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 40);
        entities.ExistingCollection.UnadjustedDate =
          db.GetNullableDate(reader, 41);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.ExistingCollection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
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
        CheckValid<Collection>("CsenetOutboundReqInd",
          entities.ExistingCollection.CsenetOutboundReqInd);
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
        entities.ExistingCollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingKeyOnlyObligation.Populated);
    entities.ExistingObligorKeyOnly.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ExistingKeyOnlyObligation.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingObligorKeyOnly.Number = db.GetString(reader, 0);
        entities.ExistingObligorKeyOnly.Populated = true;
      });
  }

  private bool ReadDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebtDetail.Populated);
    entities.ExistingDebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.PersistantDebtDetail.OtyType);
        db.
          SetInt32(command, "obgId", import.PersistantDebtDetail.ObgGeneratedId);
          
        db.
          SetString(command, "cspNumber", import.PersistantDebtDetail.CspNumber);
          
        db.SetString(command, "cpaType", import.PersistantDebtDetail.CpaType);
        db.
          SetInt32(command, "otrId", import.PersistantDebtDetail.OtrGeneratedId);
          
        db.SetString(command, "otrType", import.PersistantDebtDetail.OtrType);
        db.SetNullableDate(
          command, "discontinueDt",
          import.MaxDiscontinueDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 1);
        entities.ExistingDebtDetailStatusHistory.OtrType =
          db.GetString(reader, 2);
        entities.ExistingDebtDetailStatusHistory.OtrId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetailStatusHistory.CpaType =
          db.GetString(reader, 4);
        entities.ExistingDebtDetailStatusHistory.CspNumber =
          db.GetString(reader, 5);
        entities.ExistingDebtDetailStatusHistory.ObgId = db.GetInt32(reader, 6);
        entities.ExistingDebtDetailStatusHistory.Code = db.GetString(reader, 7);
        entities.ExistingDebtDetailStatusHistory.OtyType =
          db.GetInt32(reader, 8);
        entities.ExistingDebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.ExistingDebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.ExistingDebtDetailStatusHistory.CpaType);
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebt.Populated);
    entities.ExistingKeyOnlyObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", import.PersistantDebt.OtyType);
        db.SetInt32(command, "obId", import.PersistantDebt.ObgGeneratedId);
        db.SetString(command, "cspNumber", import.PersistantDebt.CspNumber);
        db.SetString(command, "cpaType", import.PersistantDebt.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingKeyOnlyObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingKeyOnlyObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingKeyOnlyObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.ExistingKeyOnlyObligation.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.ExistingKeyOnlyObligation.CpaType);
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);

    var adjustedInd = "N";
    var previousCollectionAdjDate = local.Collection.PreviousCollectionAdjDate;
    var collectionAdjustmentDt = local.Null1.Date;
    var lastUpdatedBy = import.UserId.Text8;
    var lastUpdatedTmst = import.CurrentTimestamp.Timestamp;
    var collectionAdjustmentReasonTxt = Spaces(240);
    var unadjustedDate = local.Collection.UnadjustedDate;

    CheckValid<Collection>("AdjustedInd", adjustedInd);
    entities.ExistingCollection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "adjInd", adjustedInd);
        db.SetNullableDate(command, "prevCollAdjDt", previousCollectionAdjDate);
        db.SetDate(command, "collAdjDt", collectionAdjustmentDt);
        db.SetDate(command, "collAdjProcDate", collectionAdjustmentDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "colAdjRsnTxt", collectionAdjustmentReasonTxt);
        db.SetNullableDate(command, "unadjustedDt", unadjustedDate);
        db.SetInt32(
          command, "collId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.ExistingCollection.CrtType);
        db.SetInt32(command, "cstId", entities.ExistingCollection.CstId);
        db.SetInt32(command, "crvId", entities.ExistingCollection.CrvId);
        db.SetInt32(command, "crdId", entities.ExistingCollection.CrdId);
        db.SetInt32(command, "obgId", entities.ExistingCollection.ObgId);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.SetInt32(command, "otrId", entities.ExistingCollection.OtrId);
        db.SetString(command, "otrType", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "otyId", entities.ExistingCollection.OtyId);
      });

    entities.ExistingCollection.AdjustedInd = adjustedInd;
    entities.ExistingCollection.PreviousCollectionAdjDate =
      previousCollectionAdjDate;
    entities.ExistingCollection.CollectionAdjustmentDt = collectionAdjustmentDt;
    entities.ExistingCollection.CollectionAdjProcessDate =
      collectionAdjustmentDt;
    entities.ExistingCollection.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCollection.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingCollection.CollectionAdjustmentReasonTxt =
      collectionAdjustmentReasonTxt;
    entities.ExistingCollection.UnadjustedDate = unadjustedDate;
    entities.ExistingCollection.Populated = true;
  }

  private void UpdateDebt()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebt.Populated);

    var lastUpdatedBy = import.UserId.Text8;
    var lastUpdatedTmst = import.CurrentTimestamp.Timestamp;

    import.PersistantDebt.Populated = false;
    Update("UpdateDebt",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "obgGeneratedId", import.PersistantDebt.ObgGeneratedId);
        db.SetString(command, "cspNumber", import.PersistantDebt.CspNumber);
        db.SetString(command, "cpaType", import.PersistantDebt.CpaType);
        db.SetInt32(
          command, "obTrnId", import.PersistantDebt.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", import.PersistantDebt.Type1);
        db.SetInt32(command, "otyType", import.PersistantDebt.OtyType);
      });

    import.PersistantDebt.LastUpdatedBy = lastUpdatedBy;
    import.PersistantDebt.LastUpdatedTmst = lastUpdatedTmst;
    import.PersistantDebt.Populated = true;
  }

  private void UpdateDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebtDetail.Populated);

    var balanceDueAmt = local.ForUpdate.BalanceDueAmt;
    var interestBalanceDueAmt =
      local.ForUpdate.InterestBalanceDueAmt.GetValueOrDefault();
    var retiredDt = local.ForUpdate.RetiredDt;
    var lastUpdatedTmst = import.CurrentTimestamp.Timestamp;
    var lastUpdatedBy = import.UserId.Text8;

    import.PersistantDebtDetail.Populated = false;
    Update("UpdateDebtDetail1",
      (db, command) =>
      {
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDecimal(command, "intBalDueAmt", interestBalanceDueAmt);
        db.SetNullableDate(command, "retiredDt", retiredDt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId",
          import.PersistantDebtDetail.ObgGeneratedId);
        db.
          SetString(command, "cspNumber", import.PersistantDebtDetail.CspNumber);
          
        db.SetString(command, "cpaType", import.PersistantDebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          import.PersistantDebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", import.PersistantDebtDetail.OtyType);
        db.SetString(command, "otrType", import.PersistantDebtDetail.OtrType);
      });

    import.PersistantDebtDetail.BalanceDueAmt = balanceDueAmt;
    import.PersistantDebtDetail.InterestBalanceDueAmt = interestBalanceDueAmt;
    import.PersistantDebtDetail.RetiredDt = retiredDt;
    import.PersistantDebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    import.PersistantDebtDetail.LastUpdatedBy = lastUpdatedBy;
    import.PersistantDebtDetail.Populated = true;
  }

  private void UpdateDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(import.PersistantDebtDetail.Populated);

    var lastUpdatedTmst = import.CurrentTimestamp.Timestamp;
    var lastUpdatedBy = import.UserId.Text8;

    import.PersistantDebtDetail.Populated = false;
    Update("UpdateDebtDetail2",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId",
          import.PersistantDebtDetail.ObgGeneratedId);
        db.
          SetString(command, "cspNumber", import.PersistantDebtDetail.CspNumber);
          
        db.SetString(command, "cpaType", import.PersistantDebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          import.PersistantDebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", import.PersistantDebtDetail.OtyType);
        db.SetString(command, "otrType", import.PersistantDebtDetail.OtrType);
      });

    import.PersistantDebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    import.PersistantDebtDetail.LastUpdatedBy = lastUpdatedBy;
    import.PersistantDebtDetail.Populated = true;
  }

  private void UpdateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDebtDetailStatusHistory.Populated);

    var discontinueDt = local.Current.Date;

    entities.ExistingDebtDetailStatusHistory.Populated = false;
    Update("UpdateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetInt32(
          command, "obTrnStatHstId",
          entities.ExistingDebtDetailStatusHistory.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType", entities.ExistingDebtDetailStatusHistory.OtrType);
          
        db.SetInt32(
          command, "otrId", entities.ExistingDebtDetailStatusHistory.OtrId);
        db.SetString(
          command, "cpaType", entities.ExistingDebtDetailStatusHistory.CpaType);
          
        db.SetString(
          command, "cspNumber",
          entities.ExistingDebtDetailStatusHistory.CspNumber);
        db.SetInt32(
          command, "obgId", entities.ExistingDebtDetailStatusHistory.ObgId);
        db.SetString(
          command, "obTrnStCd", entities.ExistingDebtDetailStatusHistory.Code);
        db.SetInt32(
          command, "otyType", entities.ExistingDebtDetailStatusHistory.OtyType);
          
      });

    entities.ExistingDebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.ExistingDebtDetailStatusHistory.Populated = true;
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
    /// A value of SuppPrsn.
    /// </summary>
    [JsonPropertyName("suppPrsn")]
    public CsePerson SuppPrsn
    {
      get => suppPrsn ??= new();
      set => suppPrsn = value;
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
    /// A value of ForUpdateCollection.
    /// </summary>
    [JsonPropertyName("forUpdateCollection")]
    public Collection ForUpdateCollection
    {
      get => forUpdateCollection ??= new();
      set => forUpdateCollection = value;
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
    /// A value of PersistantCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("persistantCashReceiptDetail")]
    public CashReceiptDetail PersistantCashReceiptDetail
    {
      get => persistantCashReceiptDetail ??= new();
      set => persistantCashReceiptDetail = value;
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
    /// A value of PersistantDebt.
    /// </summary>
    [JsonPropertyName("persistantDebt")]
    public ObligationTransaction PersistantDebt
    {
      get => persistantDebt ??= new();
      set => persistantDebt = value;
    }

    /// <summary>
    /// A value of PersistantDebtDetail.
    /// </summary>
    [JsonPropertyName("persistantDebtDetail")]
    public DebtDetail PersistantDebtDetail
    {
      get => persistantDebtDetail ??= new();
      set => persistantDebtDetail = value;
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
    /// A value of HardcodedDeactivedStatu.
    /// </summary>
    [JsonPropertyName("hardcodedDeactivedStatu")]
    public DebtDetailStatusHistory HardcodedDeactivedStatu
    {
      get => hardcodedDeactivedStatu ??= new();
      set => hardcodedDeactivedStatu = value;
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
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
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
    /// A value of CurrentTimestamp.
    /// </summary>
    [JsonPropertyName("currentTimestamp")]
    public DateWorkArea CurrentTimestamp
    {
      get => currentTimestamp ??= new();
      set => currentTimestamp = value;
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
    /// A value of HardcodedUk.
    /// </summary>
    [JsonPropertyName("hardcodedUk")]
    public DprProgram HardcodedUk
    {
      get => hardcodedUk ??= new();
      set => hardcodedUk = value;
    }

    private Obligation obligation;
    private LegalAction legalAction;
    private CsePerson suppPrsn;
    private DateWorkArea collection;
    private Collection forUpdateCollection;
    private DebtDetail forUpdateDebtDetail;
    private CashReceiptDetail persistantCashReceiptDetail;
    private ObligationType obligationType;
    private ObligationTransaction persistantDebt;
    private DebtDetail persistantDebtDetail;
    private DateWorkArea maxDiscontinueDate;
    private ObligationType hardcoded718B;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private DebtDetailStatusHistory hardcodedDeactivedStatu;
    private ObligationType hardcodedVolClass;
    private Obligation hardcodedSecondary;
    private TextWorkArea userId;
    private DateWorkArea currentTimestamp;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private DprProgram hardcodedUk;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public DebtDetail ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
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
    /// A value of ApplyUpdates.
    /// </summary>
    [JsonPropertyName("applyUpdates")]
    public Common ApplyUpdates
    {
      get => applyUpdates ??= new();
      set => applyUpdates = value;
    }

    private Collection collection;
    private DateWorkArea null1;
    private DateWorkArea current;
    private DebtDetail forUpdate;
    private Common updateAttempt;
    private Common applyUpdates;
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
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePersonAccount ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
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
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
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
    /// A value of NewCollection.
    /// </summary>
    [JsonPropertyName("newCollection")]
    public Collection NewCollection
    {
      get => newCollection ??= new();
      set => newCollection = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("existingDebtDetailStatusHistory")]
    public DebtDetailStatusHistory ExistingDebtDetailStatusHistory
    {
      get => existingDebtDetailStatusHistory ??= new();
      set => existingDebtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of NewDebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("newDebtDetailStatusHistory")]
    public DebtDetailStatusHistory NewDebtDetailStatusHistory
    {
      get => newDebtDetailStatusHistory ??= new();
      set => newDebtDetailStatusHistory = value;
    }

    private CsePerson existingObligorKeyOnly;
    private CsePersonAccount existingKeyOnlyObligor;
    private Obligation existingKeyOnlyObligation;
    private Collection existingCollection;
    private CollectionAdjustmentReason existingCollectionAdjustmentReason;
    private Collection newCollection;
    private DebtDetailStatusHistory existingDebtDetailStatusHistory;
    private DebtDetailStatusHistory newDebtDetailStatusHistory;
  }
#endregion
}
