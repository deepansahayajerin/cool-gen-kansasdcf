// Program: FN_REVERSE_A_DISTRIBUTED_COLL, ID: 372257992, model: 746.
// Short name: SWE02098
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_REVERSE_A_DISTRIBUTED_COLL.
/// </para>
/// <para>
/// RESP: FINANCE
/// This common action block reverses a distributed collection. This has been 
/// created from the elementary process action block 'FN BACKOFF DISTRIBUTED
/// COLL which should no longer be used.
/// This cab must be used wherever a given collection needs to be reversed.
/// </para>
/// </summary>
[Serializable]
public partial class FnReverseADistributedColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REVERSE_A_DISTRIBUTED_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReverseADistributedColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReverseADistributedColl.
  /// </summary>
  public FnReverseADistributedColl(IContext context, Import import,
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
    // DATE      DEVELOPER NAME          REQUEST #  DESCRIPTION
    // 09/02/97  Govind - MTW			Cloned from FN BACKOFF DISTRIBUTED COLL. 
    // Rewritten to reverse collection online; removed the code that updated
    // buckets
    // 10/22/97  Govind				Added code to roll up the interest balance and the 
    // cumulative interest fields in the Collection records.
    // 11/24/97  Govind				Don't update Debt Detail balance and don't recompute 
    // interest fields in Collection for Voluntary and Gift. Instead create a
    // negative debt adjustment to nullify the positive debt adjustment created
    // at the time of distribution. (Otherwise the system will show that Gift
    // amount is 'Due').
    // 12/05/97  govind	Don't change the distributed amount for collections 
    // distributed to concurrent debts.
    // 12/30/97  govind	Create reversals for the disbursement transactions
    // 01/06/98  govind	Set court notice req ind/ date for the reversal
    // 3/19/1999 - B Adams  -  Read properties set
    // ----------------------------------------------------------------
    // : Feb 1, 2000, M Brown, PR#86727 - remove code to update the disbursement
    // processing indicator.
    // : May 29, 2002, M Brown, PR#142517 - fixed handling of gift/voluntary.
    // ***************************************************
    // *                 HARD CODE AREA                  *
    // ***************************************************
    UseFnHardcodedDebtDistribution();
    UseFnHardcodedCashReceipting();

    if (Equal(import.Current.Date, local.Zero.Date))
    {
      local.Current.Date = Now().Date;
      local.Current.Timestamp = Now();
    }
    else
    {
      local.Current.Date = import.Current.Date;
      local.Current.Timestamp = import.Current.Timestamp;
    }

    local.Current.YearMonth = UseCabGetYearMonthFromDate();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!Lt(local.InitializedViews.InitializedDateWorkArea.Date,
      import.ProgramProcessingInfo.ProcessDate))
    {
      local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
    }
    else
    {
      local.ProgramProcessingInfo.ProcessDate =
        import.ProgramProcessingInfo.ProcessDate;
    }

    if (ReadCashReceiptDetail1())
    {
      MoveCashReceiptDetail1(entities.CashReceiptDetail, local.CashReceiptDetail);
        
    }
    else
    {
      ExitState = "FN0000_CRD_NF_ASSOC_WITH_COLL_RB";

      return;
    }

    // *****  Update the Cash Receipt Detail.
    local.CashReceiptDetail.CollectionAmtFullyAppliedInd = "";

    if (AsChar(import.Persistent.ConcurrentInd) == 'Y')
    {
      // ---------------------------------------------
      // For collection distributed to concurrent debt, the cash receipt detail 
      // distributed amount is not updated.
      // ---------------------------------------------
    }
    else
    {
      if (local.CashReceiptDetail.DistributedAmount.GetValueOrDefault() < import
        .Persistent.Amount)
      {
        ExitState = "FN0000_CRD_DIST_AMT_LT_COLL_AMT";

        return;
      }

      local.CashReceiptDetail.DistributedAmount =
        local.CashReceiptDetail.DistributedAmount.GetValueOrDefault() - import
        .Persistent.Amount;
    }

    local.CashReceiptDetail.LastUpdatedBy = global.UserId;
    local.CashReceiptDetail.LastUpdatedTmst = local.Current.Timestamp;
    UseFnUpdateCrDetailViaColl();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // ***** Set the Cash Receipt Detail Status to "Released".  The current 
    // Status History must be inactivated and a new Status History must be
    // created.  Use CABs to set the system generated identifier and the maximum
    // discontinued date.
    if (!ReadCashReceiptDetail2())
    {
      ExitState = "FN0000_CRD_NF_ASSOC_WITH_COLL_RB";

      return;
    }

    if (Equal(import.CollectionAdjustmentReason.Code, "BAD CK") || Equal
      (import.CollectionAdjustmentReason.Code, "ST PMT"))
    {
      local.WorkCashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodeAdjusted.SystemGeneratedIdentifier;
    }
    else if (!IsEmpty(import.CollectionAdjustmentReason.Code))
    {
      local.WorkCashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodeReleased.SystemGeneratedIdentifier;
    }

    UseFnChangeCashRcptDtlStatHis();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** Read and update debt and debt details balances.
    UseFnReadDebtAndDtlViaColl();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (ReadDebtObligationObligorCsePersonObligationType())
    {
      local.ObligationType.SupportedPersonReqInd =
        entities.KeyObligationType.SupportedPersonReqInd;
    }
    else
    {
      ExitState = "FN0000_DEBT_DET_ASSOC_COLL_NF_RB";

      return;
    }

    // : May 29, 2002, M Brown, PR#142517 - changed this next IF to look at 
    // obligation type classification for voluntary, and collection applied to
    // for gift.  It used to look at the obligation type code, and there is no
    // such thing as a gift type obligation.
    if (AsChar(entities.KeyObligationType.Classification) != 'V' && AsChar
      (import.Persistent.AppliedToCode) != 'G')
    {
      // *** Increase debt detail balance by the amount of the collection
      if (AsChar(import.Persistent.AppliedToCode) == 'I')
      {
        local.DebtDetail.InterestBalanceDueAmt =
          local.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() + import
          .Persistent.Amount;
      }
      else
      {
        local.DebtDetail.BalanceDueAmt += import.Persistent.Amount;
        local.DebtDetail.RetiredDt =
          local.InitializedViews.InitializedDateWorkArea.Date;
      }

      UseFnUpdateDebtAndDtlViaColl();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // ***** If the Debt Detail Status is 'D' (deactivated), reactivate the 
      // debt.
      if (AsChar(local.DebtDetailStatusHistory.Code) == AsChar
        (local.HardcodedViews.HardcodeDeactivatedStat.Code))
      {
        local.DebtDetailStatusHistory.Assign(
          local.InitializedViews.InitializedDebtDetailStatusHistory);
        local.DebtDetailStatusHistory.Code =
          local.HardcodedViews.HardcodeActivatedStatus.Code;
        local.DebtDetailStatusHistory.EffectiveDt = local.Current.Date;
        local.DebtDetailStatusHistory.ReasonTxt =
          import.Updated.CollectionAdjustmentReasonTxt ?? "";
        UseFnChangeDebtDetailStatHist();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
    }
    else
    {
      // : May 29, 2002, M Brown, PR#142517 - removed code that sets up a 
      // negative debt adjustment if the collection is gift or the obligation is
      // voluntary.
    }

    if (import.CollectionAdjustmentReason.SystemGeneratedIdentifier > 0)
    {
      if (!ReadCollectionAdjustmentReason2())
      {
        ExitState = "FN0000_COLL_ADJUST_REASON_NF_RB";
      }
    }
    else if (!IsEmpty(import.CollectionAdjustmentReason.Code))
    {
      if (!ReadCollectionAdjustmentReason1())
      {
        ExitState = "FN0000_COLL_ADJUST_REASON_NF_RB";
      }
    }
    else
    {
      ExitState = "FN0000_COLA_REASON_NOT_SUPPLD_RB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (ReadCashReceiptType())
    {
      local.CashReceiptType.CategoryIndicator =
        entities.CashReceiptType.CategoryIndicator;
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_TYPE_NF_RB";

      return;
    }

    try
    {
      UpdateCollection();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_COLLECTION_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_COLLECTION_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : PRWORA - update URA amount on URA Collection Application.
    if (AsChar(import.Persistent.ConcurrentInd) == 'Y')
    {
      return;
    }

    if (Equal(import.Persistent.ProgramAppliedTo, "FC") || Equal
      (import.Persistent.ProgramAppliedTo, "AF"))
    {
      UseFnUpdateUraAmount();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.InterfaceTransId = source.InterfaceTransId;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.DefaultedCollectionDateInd = source.DefaultedCollectionDateInd;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
    target.PayeeFirstName = source.PayeeFirstName;
    target.PayeeMiddleName = source.PayeeMiddleName;
    target.PayeeLastName = source.PayeeLastName;
    target.Reference = source.Reference;
    target.Notes = source.Notes;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
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

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.Persistent.Assign(entities.Key2);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.WorkCashReceiptDetailStatus.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    entities.Key2.Assign(useImport.Persistent);
  }

  private void UseFnChangeDebtDetailStatHist()
  {
    var useImport = new FnChangeDebtDetailStatHist.Import();
    var useExport = new FnChangeDebtDetailStatHist.Export();

    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.Max.Date = local.Max.Date;
    MoveObligationTransaction(local.Debt, useImport.Debt);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.KeyObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.KeyObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = entities.KeyObligor2.Number;
    useImport.DebtDetailStatusHistory.Assign(local.DebtDetailStatusHistory);

    Call(FnChangeDebtDetailStatHist.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodeCrtCatCash.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
    local.HardcodeCrtCategoryCash.CashNonCashInd =
      useExport.CollectionType.CtCash.CashNonCashInd;
    local.HardcodedViews.HardcodeReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodeAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedViews.HardcodeActivatedStatus.Code =
      useExport.DdshActiveStatus.Code;
    local.HardcodedViews.HardcodeDeactivatedStat.Code =
      useExport.DdshDeactivedStatus.Code;
  }

  private void UseFnReadDebtAndDtlViaColl()
  {
    var useImport = new FnReadDebtAndDtlViaColl.Import();
    var useExport = new FnReadDebtAndDtlViaColl.Export();

    useImport.Persistent.Assign(import.Persistent);

    Call(FnReadDebtAndDtlViaColl.Execute, useImport, useExport);

    local.Debt.Assign(useExport.Debt);
    local.DebtDetail.Assign(useExport.DebtDetail);
    local.DebtDetailStatusHistory.Assign(useExport.DebtDetailStatusHistory);
  }

  private void UseFnUpdateCrDetailViaColl()
  {
    var useImport = new FnUpdateCrDetailViaColl.Import();
    var useExport = new FnUpdateCrDetailViaColl.Export();

    useImport.Persistent.Assign(import.Persistent);
    MoveCashReceiptDetail2(local.CashReceiptDetail, useImport.CashReceiptDetail);
      

    Call(FnUpdateCrDetailViaColl.Execute, useImport, useExport);
  }

  private void UseFnUpdateDebtAndDtlViaColl()
  {
    var useImport = new FnUpdateDebtAndDtlViaColl.Import();
    var useExport = new FnUpdateDebtAndDtlViaColl.Export();

    useImport.Persistent.Assign(import.Persistent);
    useImport.DebtDetail.Assign(local.DebtDetail);
    useImport.Current.Timestamp = import.Current.Timestamp;

    Call(FnUpdateDebtAndDtlViaColl.Execute, useImport, useExport);
  }

  private void UseFnUpdateUraAmount()
  {
    var useImport = new FnUpdateUraAmount.Import();
    var useExport = new FnUpdateUraAmount.Export();

    useImport.CsePerson.Number = entities.KeyObligor2.Number;
    MoveCollection(import.Persistent, useImport.Collection);

    Call(FnUpdateUraAmount.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", import.Persistent.CrdId);
        db.SetInt32(command, "crvIdentifier", import.Persistent.CrvId);
        db.SetInt32(command, "cstIdentifier", import.Persistent.CstId);
        db.SetInt32(command, "crtIdentifier", import.Persistent.CrtType);
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
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 26);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 28);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 29);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 30);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 31);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 32);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 33);
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

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.Key2.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", import.Persistent.CrdId);
        db.SetInt32(command, "crvIdentifier", import.Persistent.CrvId);
        db.SetInt32(command, "cstIdentifier", import.Persistent.CstId);
        db.SetInt32(command, "crtIdentifier", import.Persistent.CrtType);
      },
      (db, reader) =>
      {
        entities.Key2.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Key2.CstIdentifier = db.GetInt32(reader, 1);
        entities.Key2.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Key2.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.Key2.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.Key2.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.Key2.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 1);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCollectionAdjustmentReason1()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason1",
      (db, command) =>
      {
        db.SetString(
          command, "obTrnRlnRsnCd", import.CollectionAdjustmentReason.Code);
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCollectionAdjustmentReason2()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason2",
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
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadDebtObligationObligorCsePersonObligationType()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.KeyDebt.Populated = false;
    entities.KeyObligation.Populated = false;
    entities.KeyObligor1.Populated = false;
    entities.KeyObligor2.Populated = false;
    entities.KeyObligationType.Populated = false;

    return Read("ReadDebtObligationObligorCsePersonObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Persistent.OtyId);
        db.SetString(command, "obTrnTyp", import.Persistent.OtrType);
        db.SetInt32(command, "obTrnId", import.Persistent.OtrId);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetInt32(command, "obgGeneratedId", import.Persistent.ObgId);
      },
      (db, reader) =>
      {
        entities.KeyDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.KeyObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.KeyDebt.CspNumber = db.GetString(reader, 1);
        entities.KeyObligation.CspNumber = db.GetString(reader, 1);
        entities.KeyObligor1.CspNumber = db.GetString(reader, 1);
        entities.KeyObligor2.Number = db.GetString(reader, 1);
        entities.KeyObligor2.Number = db.GetString(reader, 1);
        entities.KeyDebt.CpaType = db.GetString(reader, 2);
        entities.KeyObligation.CpaType = db.GetString(reader, 2);
        entities.KeyObligor1.Type1 = db.GetString(reader, 2);
        entities.KeyDebt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.KeyDebt.Type1 = db.GetString(reader, 4);
        entities.KeyDebt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.KeyDebt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.KeyDebt.CpaSupType = db.GetNullableString(reader, 7);
        entities.KeyDebt.OtyType = db.GetInt32(reader, 8);
        entities.KeyObligation.DtyGeneratedId = db.GetInt32(reader, 8);
        entities.KeyObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.KeyObligation.Description = db.GetNullableString(reader, 9);
        entities.KeyObligation.HistoryInd = db.GetNullableString(reader, 10);
        entities.KeyObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 11);
        entities.KeyObligation.PreConversionDebtNumber =
          db.GetNullableInt32(reader, 12);
        entities.KeyObligation.PreConversionCaseNumber =
          db.GetNullableString(reader, 13);
        entities.KeyObligation.AsOfDtNadArrBal =
          db.GetNullableDecimal(reader, 14);
        entities.KeyObligation.AsOfDtNadIntBal =
          db.GetNullableDecimal(reader, 15);
        entities.KeyObligation.AsOfDtAdcArrBal =
          db.GetNullableDecimal(reader, 16);
        entities.KeyObligation.AsOfDtAdcIntBal =
          db.GetNullableDecimal(reader, 17);
        entities.KeyObligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 18);
        entities.KeyObligation.AsOdDtRecIntBal =
          db.GetNullableDecimal(reader, 19);
        entities.KeyObligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 20);
        entities.KeyObligation.AsOfDtFeeIntBal =
          db.GetNullableDecimal(reader, 21);
        entities.KeyObligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 22);
        entities.KeyObligation.TillDtCsCollCurrArr =
          db.GetNullableDecimal(reader, 23);
        entities.KeyObligation.TillDtSpCollCurrArr =
          db.GetNullableDecimal(reader, 24);
        entities.KeyObligation.TillDtMsCollCurrArr =
          db.GetNullableDecimal(reader, 25);
        entities.KeyObligation.TillDtNadArrColl =
          db.GetNullableDecimal(reader, 26);
        entities.KeyObligation.TillDtNadIntColl =
          db.GetNullableDecimal(reader, 27);
        entities.KeyObligation.TillDtAdcArrColl =
          db.GetNullableDecimal(reader, 28);
        entities.KeyObligation.TillDtAdcIntColl =
          db.GetNullableDecimal(reader, 29);
        entities.KeyObligation.AsOfDtTotRecColl =
          db.GetNullableDecimal(reader, 30);
        entities.KeyObligation.AsOfDtTotRecIntColl =
          db.GetNullableDecimal(reader, 31);
        entities.KeyObligation.AsOfDtTotFeeColl =
          db.GetNullableDecimal(reader, 32);
        entities.KeyObligation.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 33);
        entities.KeyObligation.AsOfDtTotCollAll =
          db.GetNullableDecimal(reader, 34);
        entities.KeyObligation.LastCollAmt = db.GetNullableDecimal(reader, 35);
        entities.KeyObligation.AsOfDtTotGiftColl =
          db.GetNullableDecimal(reader, 36);
        entities.KeyObligation.LastCollDt = db.GetNullableDate(reader, 37);
        entities.KeyObligation.CreatedBy = db.GetString(reader, 38);
        entities.KeyObligation.CreatedTmst = db.GetDateTime(reader, 39);
        entities.KeyObligation.LastUpdatedBy = db.GetNullableString(reader, 40);
        entities.KeyObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 41);
        entities.KeyObligation.OrderTypeCode = db.GetString(reader, 42);
        entities.KeyObligor1.CreatedBy = db.GetString(reader, 43);
        entities.KeyObligor1.CreatedTmst = db.GetDateTime(reader, 44);
        entities.KeyObligor1.LastUpdatedBy = db.GetNullableString(reader, 45);
        entities.KeyObligor1.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 46);
        entities.KeyObligor1.AsOfDtTotGiftColl =
          db.GetNullableDecimal(reader, 47);
        entities.KeyObligor1.OverpaymentLetterSendDate =
          db.GetNullableDate(reader, 48);
        entities.KeyObligor1.LastManualDistributionDate =
          db.GetNullableDate(reader, 49);
        entities.KeyObligor1.AsOfDtNadArrBal =
          db.GetNullableDecimal(reader, 50);
        entities.KeyObligor1.AsOfDtNadIntBal =
          db.GetNullableDecimal(reader, 51);
        entities.KeyObligor1.AsOfDtAdcArrBal =
          db.GetNullableDecimal(reader, 52);
        entities.KeyObligor1.AsOfDtAdcIntBal =
          db.GetNullableDecimal(reader, 53);
        entities.KeyObligor1.AsOfDtRecBal = db.GetNullableDecimal(reader, 54);
        entities.KeyObligor1.AsOfDtTotRecIntBal =
          db.GetNullableDecimal(reader, 55);
        entities.KeyObligor1.AsOfDtTotFeeBal =
          db.GetNullableDecimal(reader, 56);
        entities.KeyObligor1.AsOfDtTotFeeIntBal =
          db.GetNullableDecimal(reader, 57);
        entities.KeyObligor1.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 58);
        entities.KeyObligor1.TillDtCsCollCurrArr =
          db.GetNullableDecimal(reader, 59);
        entities.KeyObligor1.TillDtSpCollCurrArr =
          db.GetNullableDecimal(reader, 60);
        entities.KeyObligor1.TillDtMsCollCurrArr =
          db.GetNullableDecimal(reader, 61);
        entities.KeyObligor1.TillDtNadArrColl =
          db.GetNullableDecimal(reader, 62);
        entities.KeyObligor1.TillDtNadIntColl =
          db.GetNullableDecimal(reader, 63);
        entities.KeyObligor1.TillDtAdcArrColl =
          db.GetNullableDecimal(reader, 64);
        entities.KeyObligor1.TillDtAdcIntColl =
          db.GetNullableDecimal(reader, 65);
        entities.KeyObligor1.AsOfDtTotRecColl =
          db.GetNullableDecimal(reader, 66);
        entities.KeyObligor1.AsOfDtTotRecIntColl =
          db.GetNullableDecimal(reader, 67);
        entities.KeyObligor1.AsOfDtTotFeeColl =
          db.GetNullableDecimal(reader, 68);
        entities.KeyObligor1.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 69);
        entities.KeyObligor1.AsOfDtTotCollAll =
          db.GetNullableDecimal(reader, 70);
        entities.KeyObligor1.LastCollAmt = db.GetNullableDecimal(reader, 71);
        entities.KeyObligor1.LastCollDt = db.GetNullableDate(reader, 72);
        entities.KeyObligationType.Code = db.GetString(reader, 73);
        entities.KeyObligationType.Classification = db.GetString(reader, 74);
        entities.KeyObligationType.SupportedPersonReqInd =
          db.GetString(reader, 75);
        entities.KeyDebt.Populated = true;
        entities.KeyObligation.Populated = true;
        entities.KeyObligor1.Populated = true;
        entities.KeyObligor2.Populated = true;
        entities.KeyObligationType.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.KeyDebt.CpaType);
        CheckValid<Obligation>("CpaType", entities.KeyObligation.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.KeyObligor1.Type1);
        CheckValid<ObligationTransaction>("Type1", entities.KeyDebt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.KeyDebt.CpaSupType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.KeyObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.KeyObligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.KeyObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.KeyObligationType.SupportedPersonReqInd);
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var adjustedInd = "Y";
    var carId = entities.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    var collectionAdjustmentDt = local.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;
    var collectionAdjustmentReasonTxt =
      import.Updated.CollectionAdjustmentReasonTxt ?? "";

    CheckValid<Collection>("AdjustedInd", adjustedInd);
    import.Persistent.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "adjInd", adjustedInd);
        db.SetNullableInt32(command, "carId", carId);
        db.SetDate(command, "collAdjDt", collectionAdjustmentDt);
        db.SetDate(command, "collAdjProcDate", collectionAdjustmentDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "colAdjRsnTxt", collectionAdjustmentReasonTxt);
        db.SetInt32(
          command, "collId", import.Persistent.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", import.Persistent.CrtType);
        db.SetInt32(command, "cstId", import.Persistent.CstId);
        db.SetInt32(command, "crvId", import.Persistent.CrvId);
        db.SetInt32(command, "crdId", import.Persistent.CrdId);
        db.SetInt32(command, "obgId", import.Persistent.ObgId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(command, "otrId", import.Persistent.OtrId);
        db.SetString(command, "otrType", import.Persistent.OtrType);
        db.SetInt32(command, "otyId", import.Persistent.OtyId);
      });

    import.Persistent.AdjustedInd = adjustedInd;
    import.Persistent.CarId = carId;
    import.Persistent.CollectionAdjustmentDt = collectionAdjustmentDt;
    import.Persistent.CollectionAdjProcessDate = collectionAdjustmentDt;
    import.Persistent.LastUpdatedBy = lastUpdatedBy;
    import.Persistent.LastUpdatedTmst = lastUpdatedTmst;
    import.Persistent.CollectionAdjustmentReasonTxt =
      collectionAdjustmentReasonTxt;
    import.Persistent.Populated = true;
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
    /// A value of Updated.
    /// </summary>
    [JsonPropertyName("updated")]
    public Collection Updated
    {
      get => updated ??= new();
      set => updated = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Collection updated;
    private ProgramProcessingInfo programProcessingInfo;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection persistent;
    private DateWorkArea current;
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
    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodeReleased.
      /// </summary>
      [JsonPropertyName("hardcodeReleased")]
      public CashReceiptDetailStatus HardcodeReleased
      {
        get => hardcodeReleased ??= new();
        set => hardcodeReleased = value;
      }

      /// <summary>
      /// A value of HardcodeAdjusted.
      /// </summary>
      [JsonPropertyName("hardcodeAdjusted")]
      public CashReceiptDetailStatus HardcodeAdjusted
      {
        get => hardcodeAdjusted ??= new();
        set => hardcodeAdjusted = value;
      }

      /// <summary>
      /// A value of HardcodeActivatedStatus.
      /// </summary>
      [JsonPropertyName("hardcodeActivatedStatus")]
      public DebtDetailStatusHistory HardcodeActivatedStatus
      {
        get => hardcodeActivatedStatus ??= new();
        set => hardcodeActivatedStatus = value;
      }

      /// <summary>
      /// A value of HardcodeDeactivatedStat.
      /// </summary>
      [JsonPropertyName("hardcodeDeactivatedStat")]
      public DebtDetailStatusHistory HardcodeDeactivatedStat
      {
        get => hardcodeDeactivatedStat ??= new();
        set => hardcodeDeactivatedStat = value;
      }

      /// <summary>
      /// A value of HardcodeCollBackedOff.
      /// </summary>
      [JsonPropertyName("hardcodeCollBackedOff")]
      public ProgramControlTotal HardcodeCollBackedOff
      {
        get => hardcodeCollBackedOff ??= new();
        set => hardcodeCollBackedOff = value;
      }

      /// <summary>
      /// A value of HardcodeCpaRead.
      /// </summary>
      [JsonPropertyName("hardcodeCpaRead")]
      public ProgramControlTotal HardcodeCpaRead
      {
        get => hardcodeCpaRead ??= new();
        set => hardcodeCpaRead = value;
      }

      /// <summary>
      /// A value of HardcodeCpaUpdated.
      /// </summary>
      [JsonPropertyName("hardcodeCpaUpdated")]
      public ProgramControlTotal HardcodeCpaUpdated
      {
        get => hardcodeCpaUpdated ??= new();
        set => hardcodeCpaUpdated = value;
      }

      /// <summary>
      /// A value of HardcodeObligRead.
      /// </summary>
      [JsonPropertyName("hardcodeObligRead")]
      public ProgramControlTotal HardcodeObligRead
      {
        get => hardcodeObligRead ??= new();
        set => hardcodeObligRead = value;
      }

      /// <summary>
      /// A value of HardcodeObligUpdated.
      /// </summary>
      [JsonPropertyName("hardcodeObligUpdated")]
      public ProgramControlTotal HardcodeObligUpdated
      {
        get => hardcodeObligUpdated ??= new();
        set => hardcodeObligUpdated = value;
      }

      /// <summary>
      /// A value of HardcodeMosUpdated.
      /// </summary>
      [JsonPropertyName("hardcodeMosUpdated")]
      public ProgramControlTotal HardcodeMosUpdated
      {
        get => hardcodeMosUpdated ??= new();
        set => hardcodeMosUpdated = value;
      }

      private CashReceiptDetailStatus hardcodeReleased;
      private CashReceiptDetailStatus hardcodeAdjusted;
      private DebtDetailStatusHistory hardcodeActivatedStatus;
      private DebtDetailStatusHistory hardcodeDeactivatedStat;
      private ProgramControlTotal hardcodeCollBackedOff;
      private ProgramControlTotal hardcodeCpaRead;
      private ProgramControlTotal hardcodeCpaUpdated;
      private ProgramControlTotal hardcodeObligRead;
      private ProgramControlTotal hardcodeObligUpdated;
      private ProgramControlTotal hardcodeMosUpdated;
    }

    /// <summary>A InitializedViewsGroup group.</summary>
    [Serializable]
    public class InitializedViewsGroup
    {
      /// <summary>
      /// A value of InitializedDateWorkArea.
      /// </summary>
      [JsonPropertyName("initializedDateWorkArea")]
      public DateWorkArea InitializedDateWorkArea
      {
        get => initializedDateWorkArea ??= new();
        set => initializedDateWorkArea = value;
      }

      /// <summary>
      /// A value of InitializedDebtDetailStatusHistory.
      /// </summary>
      [JsonPropertyName("initializedDebtDetailStatusHistory")]
      public DebtDetailStatusHistory InitializedDebtDetailStatusHistory
      {
        get => initializedDebtDetailStatusHistory ??= new();
        set => initializedDebtDetailStatusHistory = value;
      }

      private DateWorkArea initializedDateWorkArea;
      private DebtDetailStatusHistory initializedDebtDetailStatusHistory;
    }

    /// <summary>
    /// A value of DisbProcNeeded.
    /// </summary>
    [JsonPropertyName("disbProcNeeded")]
    public Collection DisbProcNeeded
    {
      get => disbProcNeeded ??= new();
      set => disbProcNeeded = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of HardcodeCrtCatCash.
    /// </summary>
    [JsonPropertyName("hardcodeCrtCatCash")]
    public CashReceiptType HardcodeCrtCatCash
    {
      get => hardcodeCrtCatCash ??= new();
      set => hardcodeCrtCatCash = value;
    }

    /// <summary>
    /// A value of HardcodeCrtCategoryCash.
    /// </summary>
    [JsonPropertyName("hardcodeCrtCategoryCash")]
    public CollectionType HardcodeCrtCategoryCash
    {
      get => hardcodeCrtCategoryCash ??= new();
      set => hardcodeCrtCategoryCash = value;
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
    /// A value of Reversal.
    /// </summary>
    [JsonPropertyName("reversal")]
    public Collection Reversal
    {
      get => reversal ??= new();
      set => reversal = value;
    }

    /// <summary>
    /// A value of ReverseDebtAdjCreated.
    /// </summary>
    [JsonPropertyName("reverseDebtAdjCreated")]
    public Common ReverseDebtAdjCreated
    {
      get => reverseDebtAdjCreated ??= new();
      set => reverseDebtAdjCreated = value;
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
    /// A value of RefreshIntFieldsFromDt.
    /// </summary>
    [JsonPropertyName("refreshIntFieldsFromDt")]
    public DateWorkArea RefreshIntFieldsFromDt
    {
      get => refreshIntFieldsFromDt ??= new();
      set => refreshIntFieldsFromDt = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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

    /// <summary>
    /// Gets a value of InitializedViews.
    /// </summary>
    [JsonPropertyName("initializedViews")]
    public InitializedViewsGroup InitializedViews
    {
      get => initializedViews ?? (initializedViews = new());
      set => initializedViews = value;
    }

    /// <summary>
    /// A value of ZdelLocalProcessingProgramTy.
    /// </summary>
    [JsonPropertyName("zdelLocalProcessingProgramTy")]
    public Common ZdelLocalProcessingProgramTy
    {
      get => zdelLocalProcessingProgramTy ??= new();
      set => zdelLocalProcessingProgramTy = value;
    }

    /// <summary>
    /// A value of WorkCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("workCashReceiptDetail")]
    public CashReceiptDetail WorkCashReceiptDetail
    {
      get => workCashReceiptDetail ??= new();
      set => workCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of WorkCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("workCashReceiptDetailStatus")]
    public CashReceiptDetailStatus WorkCashReceiptDetailStatus
    {
      get => workCashReceiptDetailStatus ??= new();
      set => workCashReceiptDetailStatus = value;
    }

    private Collection disbProcNeeded;
    private ObligationType obligationType;
    private CashReceiptType cashReceiptType;
    private CashReceiptType hardcodeCrtCatCash;
    private CollectionType hardcodeCrtCategoryCash;
    private DateWorkArea max;
    private Collection reversal;
    private Common reverseDebtAdjCreated;
    private DateWorkArea zero;
    private DateWorkArea refreshIntFieldsFromDt;
    private CsePersonAccount csePersonAccount;
    private ProgramProcessingInfo programProcessingInfo;
    private HardcodedViewsGroup hardcodedViews;
    private DateWorkArea current;
    private CashReceiptDetail cashReceiptDetail;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private InitializedViewsGroup initializedViews;
    private Common zdelLocalProcessingProgramTy;
    private CashReceiptDetail workCashReceiptDetail;
    private CashReceiptDetailStatus workCashReceiptDetailStatus;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CashReceipt KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Obligation Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of PreviousDebt.
    /// </summary>
    [JsonPropertyName("previousDebt")]
    public ObligationTransaction PreviousDebt
    {
      get => previousDebt ??= new();
      set => previousDebt = value;
    }

    /// <summary>
    /// A value of PreviousCollection.
    /// </summary>
    [JsonPropertyName("previousCollection")]
    public Collection PreviousCollection
    {
      get => previousCollection ??= new();
      set => previousCollection = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Key2.
    /// </summary>
    [JsonPropertyName("key2")]
    public CashReceiptDetail Key2
    {
      get => key2 ??= new();
      set => key2 = value;
    }

    /// <summary>
    /// A value of KeyDebt.
    /// </summary>
    [JsonPropertyName("keyDebt")]
    public ObligationTransaction KeyDebt
    {
      get => keyDebt ??= new();
      set => keyDebt = value;
    }

    /// <summary>
    /// A value of KeyObligation.
    /// </summary>
    [JsonPropertyName("keyObligation")]
    public Obligation KeyObligation
    {
      get => keyObligation ??= new();
      set => keyObligation = value;
    }

    /// <summary>
    /// A value of KeyObligor1.
    /// </summary>
    [JsonPropertyName("keyObligor1")]
    public CsePersonAccount KeyObligor1
    {
      get => keyObligor1 ??= new();
      set => keyObligor1 = value;
    }

    /// <summary>
    /// A value of KeyObligor2.
    /// </summary>
    [JsonPropertyName("keyObligor2")]
    public CsePerson KeyObligor2
    {
      get => keyObligor2 ??= new();
      set => keyObligor2 = value;
    }

    /// <summary>
    /// A value of KeyObligationType.
    /// </summary>
    [JsonPropertyName("keyObligationType")]
    public ObligationType KeyObligationType
    {
      get => keyObligationType ??= new();
      set => keyObligationType = value;
    }

    private CashReceiptType cashReceiptType;
    private CashReceipt keyOnly;
    private CsePerson supported1;
    private CsePersonAccount supported2;
    private Obligation prev;
    private ObligationTransaction previousDebt;
    private Collection previousCollection;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetail key2;
    private ObligationTransaction keyDebt;
    private Obligation keyObligation;
    private CsePersonAccount keyObligor1;
    private CsePerson keyObligor2;
    private ObligationType keyObligationType;
  }
#endregion
}
