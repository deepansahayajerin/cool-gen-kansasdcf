// Program: FN_PROCESS_A_DIST_COLL_TRAN, ID: 372543907, model: 746.
// Short name: SWE00516
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_PROCESS_A_DIST_COLL_TRAN.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action diagram will process a single collection, determining the 
/// obligee that it relates to and creating a credit disbursement_transaction
/// associated to that obligee.
/// </para>
/// </summary>
[Serializable]
public partial class FnProcessADistCollTran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_A_DIST_COLL_TRAN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessADistCollTran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessADistCollTran.
  /// </summary>
  public FnProcessADistCollTran(IContext context, Import import, Export export):
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
    // ---------------------------------------------
    // 120697	govind		Set Reference Number to CR-CRD combo no
    // 121197	govind		Set Designated Payee
    // 122397	govind		Fixed bug: cse person currency was not obtained
    // 010298	govind		Removed the check to see if an adjusted record already 
    // exists. The collection reversal process now automatically creates
    // reversal disb tran recs.
    // 			Create OBLIGEE if not found
    // 022698	govind		Associate with Interstate Request for interstate debts
    // 090799  Fangman         Set attributes on a create statement (create 
    // obligee).
    // 091799  Fangman     Restructured to support:
    //                PR 74155 - Displaying more info on errors.
    //                PR 73627 - Changing the way the Obligee is determined.
    //                PR 74069 - Not creating disbursements when the applied to
    // is "NC".
    //                PR 83163 - Added view for Cash Receipt Type.
    // 050400  Fangman  PRWORA   Added code to set the excess URA ind.
    // 090800  Fangman  103323  Changed code to not create a suppression rule if
    // one just like it already exists.  Changed code to not create a
    // suppression rule that is already discontinued.
    // ---------------------------------------------
    // ****************************************************************
    // Phase II changes.  rk 4/29/99
    // 1. Took out Designated Payee code, 641 does.
    // 2. Changed interstate Request read
    // SWSRKXD - 7/21/99
    // PR# 302 - Interstate Request changes.
    // ****************************************************************
    // -----------------------------------------------------------------
    // 10/22/99 - SWSRKXD PR#77874
    // NC collections will never be disbursed.
    // 06/06/01  Fangman  SR 010507
    //      Update the AR Number on the Collection table.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 10/01/02  Fangman  WR 020120
    //      Added code to call new archive action block that returns a unique 
    // disbursement transaction ID by checking both the prod & archive tables to
    // ensure that the ID is unique in both tables.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // 06/23/03  Fangman  WR 302055
    //      Made changes for passing data to the action block that determines 
    // teh AR.  Also commented out changes for archiving.  The archiving changes
    // can go back in after this project.
    // -----------------------------------------------------------------
    export.Abort.Flag = "N";
    UseFnDetObligeeForObligColl();

    if (AsChar(export.AppliedApCollToRcvInd.Flag) == 'Y')
    {
      return;
    }

    // -----------------------------------------------------------------
    // 10/22/99 - SWSRKXD PR#77874
    // CAB fn_det_obligee_for_oblig_coll will never supply
    // export_state_collection_ind. This code has been disabled
    // and views left here just incase business change their mind.
    // -----------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (ReadObligee())
    {
      // Continue
    }
    else if (ReadCsePerson())
    {
      try
      {
        CreateObligee();

        // Continue
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGEE_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGEE_PV";

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
      ExitState = "FN0000_OBLIGEE_CSE_PERSON_NF";

      return;
    }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // USE ar_get_disb_tran_id
    //         WHICH IMPORTS: Entity View import cash_receipt_detail TO Entity 
    // View import cash_receipt_detail
    //                        Entity View import cash_receipt  TO Entity View 
    // import cash_receipt
    //         WHICH EXPORTS: Work View  local crd_cr_combo_no  FROM Work View  
    // export crd_cr_combo_no
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    UseFnGetDisbTranId();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseFnAbConcatCrAndCrd();
    local.ForCreate.ReferenceNumber = local.CrdCrComboNo.CrdCrCombo;

    if (AsChar(import.PerCollection.AdjustedInd) == 'Y')
    {
      // -------------------------------------------------
      // If processing an adjustment then set the amount negative.
      // -------------------------------------------------
      local.ForCreate.Amount = -import.PerCollection.Amount;
    }
    else
    {
      local.ForCreate.Amount = import.PerCollection.Amount;
    }

    if (Equal(import.PerCollection.DistPgmStateAppldTo, "UD") || Equal
      (import.PerCollection.DistPgmStateAppldTo, "UP"))
    {
      local.ForCreate.ExcessUraInd = "Y";
    }
    else
    {
      local.ForCreate.ExcessUraInd = "";
    }

    // ---------------------------------------------------------------------------------------
    // For interstate debts, get the currency on Interstate Request
    // ---------------------------------------------------------------------------------------
    local.ForCreate.InterstateInd = "N";

    if (AsChar(import.PerCollection.AppliedToOrderTypeCode) == 'I')
    {
      if (ReadInterstateRequest())
      {
        local.ForCreate.InterstateInd = "Y";
      }
      else
      {
        ExitState = "FN0000_CURRENT_INTER_ST_RQST_NF";

        return;
      }
    }

    try
    {
      CreateDisbursementTransaction();

      if (AsChar(local.ForCreate.InterstateInd) == 'Y')
      {
        AssociateDisbursementTransaction();
      }

      // Update the AR Number on the 
      // Collection row
      // --------------------------------
      // ***************************************************
      // Mark the Collection as having been processed.
      // ***************************************************
      try
      {
        UpdateCollection();
        ExitState = "ACO_NN0000_ALL_OK";
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_COLLECTION_NU";
            export.Abort.Flag = "Y";

            return;
          case ErrorCode.PermittedValueViolation:
            export.Abort.Flag = "Y";
            ExitState = "FN0000_COLLECTION_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DISB_TRANS_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_TRANS_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(import.PerCollection.AppliedToCode) == 'C' && Equal
      (import.PerCollection.ProgramAppliedTo, "NA") && AsChar
      (import.PerCollection.AppliedToFuture) == 'Y' && AsChar
      (import.PerCollection.AdjustedInd) != 'Y')
    {
      UseFnCheckForAfInvolvement();

      if (AsChar(local.AfInvolvementInd.Flag) == 'Y')
      {
        if (Equal(export.DebtDetail.DueDt, local.Initialized.Date))
        {
          if (ReadDebtDetail())
          {
            export.DebtDetail.DueDt = entities.DebtDetail.DueDt;
          }
          else
          {
            ExitState = "FN0000_DEBT_DETAIL_NF_RB";

            return;
          }
        }

        local.Numeric.NumDate = DateToInt(export.DebtDetail.DueDt);
        local.Text.TextDate = NumberToString(local.Numeric.NumDate, 8);
        local.Text2.TextDate =
          Substring(local.Text.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) +
          "01";
        local.Numeric.NumDate = (int)StringToNumber(local.Text2.TextDate);
        local.DiscontinueDate.Date = IntToDate(local.Numeric.NumDate);

        if (!Lt(import.ProgramProcessingInfo.ProcessDate,
          local.DiscontinueDate.Date))
        {
          // Do not create suppression rules that have already discontinued.
          return;
        }

        if (ReadDisbSuppressionStatusHistory())
        {
          // This suppression rule already exists - do not need to create 
          // another one.
        }
        else
        {
          try
          {
            CreateDisbSuppressionStatusHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_SUPP_STAT_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_SUPP_STAT_PV";

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

  private static void MoveHardcode(Import.HardcodeGroup source,
    FnDetObligeeForObligColl.Import.HardcodeGroup target)
  {
    target.HardcodeSpousalSupport.SystemGeneratedIdentifier =
      source.HardcodeSpousalSupport.SystemGeneratedIdentifier;
    target.HardcodeSpArrearsJudgmt.SystemGeneratedIdentifier =
      source.HardcodeSpArrearsJudgemt.SystemGeneratedIdentifier;
    target.HardcodeVoluntary.SystemGeneratedIdentifier =
      source.HardcodeVoluntary.SystemGeneratedIdentifier;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    local.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private int UseFnAssignDisbSuppIdV2()
  {
    var useImport = new FnAssignDisbSuppIdV2.Import();
    var useExport = new FnAssignDisbSuppIdV2.Export();

    useImport.Obligee.Number = export.Obligee.Number;

    Call(FnAssignDisbSuppIdV2.Execute, useImport, useExport);

    return useExport.DisbSuppressionStatusHistory.SystemGeneratedIdentifier;
  }

  private void UseFnCheckForAfInvolvement()
  {
    var useImport = new FnCheckForAfInvolvement.Import();
    var useExport = new FnCheckForAfInvolvement.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Ar.Number = export.Obligee.Number;
    useImport.Child.Number = export.Child.Number;

    Call(FnCheckForAfInvolvement.Execute, useImport, useExport);

    local.AfInvolvementInd.Flag = useExport.AfInvolvementInd.Flag;
  }

  private void UseFnDetObligeeForObligColl()
  {
    var useImport = new FnDetObligeeForObligColl.Import();
    var useExport = new FnDetObligeeForObligColl.Export();

    useImport.ExpCollsEligForRcvCnt.Count = import.ExpCollsEligForRcvCnt.Count;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.DisplayInd.Flag = import.DisplayInd.Flag;
    useImport.ApplyApCollToRcvInd.Flag = import.ApplyApCollToRcvInd.Flag;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveHardcode(import.Hardcode, useImport.Hardcode);
    useImport.PerCollection.Assign(import.PerCollection);
    useImport.PerDebt.Assign(import.PerDebt);
    useImport.ObligationType.Assign(import.ObligationType);
    useImport.Obligor.Number = import.Obligor.Number;
    useImport.PerCashReceiptType.Assign(import.PerCashReceiptType);

    Call(FnDetObligeeForObligColl.Execute, useImport, useExport);

    import.ExpCollsEligForRcvCnt.Count = useImport.ExpCollsEligForRcvCnt.Count;
    import.PerCollection.Assign(useImport.PerCollection);
    import.PerDebt.Assign(useImport.PerDebt);
    import.PerCashReceiptType.Assign(useImport.PerCashReceiptType);
    export.AppliedApCollToRcvInd.Flag = useExport.AppliedApCollToRcvInd.Flag;
    export.Obligee.Number = useExport.Obligee.Number;
    export.Child.Number = useExport.Child.Number;
    export.Case1.Number = useExport.Case1.Number;
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    export.StateCollecionInd.Flag = useExport.StateCollectionInd.Flag;
    export.Abort.Flag = useExport.Abort.Flag;
  }

  private void UseFnGetDisbTranId()
  {
    var useImport = new FnGetDisbTranId.Import();
    var useExport = new FnGetDisbTranId.Export();

    useImport.CsePerson.Number = export.Obligee.Number;

    Call(FnGetDisbTranId.Execute, useImport, useExport);

    local.ForCreate.SystemGeneratedIdentifier =
      useExport.DisbursementTransaction.SystemGeneratedIdentifier;
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var intInterId = entities.InterstateRequest.IntHGeneratedId;

    entities.DisbursementTransaction.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);

    var cpaType = entities.Obligee1.Type1;
    var cspNumber = entities.Obligee1.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignDisbSuppIdV2();
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.DiscontinueDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = "A";
    var reasonText =
      "Current NA Collection has been Automatically suppressed due to AF involvement in last 90 days.";
      

    CheckValid<DisbSuppressionStatusHistory>("CpaType", cpaType);
    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("CreateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "dssGeneratedId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "personDisbFiller", "");
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.DisbSuppressionStatusHistory.CpaType = cpaType;
    entities.DisbSuppressionStatusHistory.CspNumber = cspNumber;
    entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbSuppressionStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.CreatedBy = createdBy;
    entities.DisbSuppressionStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbSuppressionStatusHistory.Type1 = type1;
    entities.DisbSuppressionStatusHistory.ReasonText = reasonText;
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private void CreateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(import.PerCollection.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);

    var cpaType = entities.Obligee1.Type1;
    var cspNumber = entities.Obligee1.CspNumber;
    var systemGeneratedIdentifier = local.ForCreate.SystemGeneratedIdentifier;
    var type1 = "C";
    var amount = local.ForCreate.Amount;
    var processDate = local.Initialized.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var collectionDate = import.PerCollection.CollectionDt;
    var collectionProcessDate = import.ProgramProcessingInfo.ProcessDate;
    var otyId = import.PerCollection.OtyId;
    var otrTypeDisb = import.PerCollection.OtrType;
    var otrId = import.PerCollection.OtrId;
    var cpaTypeDisb = import.PerCollection.CpaType;
    var cspNumberDisb = import.PerCollection.CspNumber;
    var obgId = import.PerCollection.ObgId;
    var crdId = import.PerCollection.CrdId;
    var crvId = import.PerCollection.CrvId;
    var cstId = import.PerCollection.CstId;
    var crtId = import.PerCollection.CrtType;
    var colId = import.PerCollection.SystemGeneratedIdentifier;
    var interstateInd = local.ForCreate.InterstateInd ?? "";
    var referenceNumber = local.ForCreate.ReferenceNumber ?? "";
    var excessUraInd = local.ForCreate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("OtrTypeDisb", otrTypeDisb);
    CheckValid<DisbursementTransaction>("CpaTypeDisb", cpaTypeDisb);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "disbTranId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetNullableDate(command, "disbursementDate", default(DateTime));
        db.SetString(
          command, "cashNonCashInd", GetImplicitValue<DisbursementTransaction,
          string>("CashNonCashInd"));
        db.SetString(command, "recapturedInd", "");
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", collectionProcessDate);
        db.SetNullableInt32(command, "otyId", otyId);
        db.SetNullableString(command, "otrTypeDisb", otrTypeDisb);
        db.SetNullableInt32(command, "otrId", otrId);
        db.SetNullableString(command, "cpaTypeDisb", cpaTypeDisb);
        db.SetNullableString(command, "cspNumberDisb", cspNumberDisb);
        db.SetNullableInt32(command, "obgId", obgId);
        db.SetNullableInt32(command, "crdId", crdId);
        db.SetNullableInt32(command, "crvId", crvId);
        db.SetNullableInt32(command, "cstId", cstId);
        db.SetNullableInt32(command, "crtId", crtId);
        db.SetNullableInt32(command, "colId", colId);
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.CollectionProcessDate =
      collectionProcessDate;
    entities.DisbursementTransaction.OtyId = otyId;
    entities.DisbursementTransaction.OtrTypeDisb = otrTypeDisb;
    entities.DisbursementTransaction.OtrId = otrId;
    entities.DisbursementTransaction.CpaTypeDisb = cpaTypeDisb;
    entities.DisbursementTransaction.CspNumberDisb = cspNumberDisb;
    entities.DisbursementTransaction.ObgId = obgId;
    entities.DisbursementTransaction.CrdId = crdId;
    entities.DisbursementTransaction.CrvId = crvId;
    entities.DisbursementTransaction.CstId = cstId;
    entities.DisbursementTransaction.CrtId = crtId;
    entities.DisbursementTransaction.ColId = colId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateObligee()
  {
    var cspNumber = entities.Obligee2.Number;
    var type1 = "E";
    var createdBy = global.UserId;
    var createdTmst = Now();

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.Obligee1.Populated = false;
    Update("CreateObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.Obligee1.CspNumber = cspNumber;
    entities.Obligee1.Type1 = type1;
    entities.Obligee1.CreatedBy = createdBy;
    entities.Obligee1.CreatedTmst = createdTmst;
    entities.Obligee1.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.Obligee2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.Number = db.GetString(reader, 0);
        entities.Obligee2.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.PerDebt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.PerDebt.OtyType);
        db.SetInt32(command, "obgGeneratedId", import.PerDebt.ObgGeneratedId);
        db.SetString(command, "otrType", import.PerDebt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", import.PerDebt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.PerDebt.CpaType);
        db.SetString(command, "cspNumber", import.PerDebt.CspNumber);
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
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          local.DiscontinueDate.Date.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(import.PerObligation.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.PerObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.PerObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.PerObligation.CspNumber);
        db.SetString(command, "cpaType", import.PerObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee1.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee1.CspNumber = db.GetString(reader, 0);
        entities.Obligee1.Type1 = db.GetString(reader, 1);
        entities.Obligee1.CreatedBy = db.GetString(reader, 2);
        entities.Obligee1.CreatedTmst = db.GetDateTime(reader, 3);
        entities.Obligee1.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee1.Type1);
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(import.PerCollection.Populated);

    var disbursementDt = import.CollectionUpdateValues.DisbursementDt;
    var disbursementAdjProcessDate =
      import.CollectionUpdateValues.DisbursementAdjProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var arNumber = export.Obligee.Number;

    import.PerCollection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableDate(command, "disbDt", disbursementDt);
        db.SetDate(command, "disbAdjProcDate", disbursementAdjProcessDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "arNumber", arNumber);
        db.SetInt32(
          command, "collId", import.PerCollection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", import.PerCollection.CrtType);
        db.SetInt32(command, "cstId", import.PerCollection.CstId);
        db.SetInt32(command, "crvId", import.PerCollection.CrvId);
        db.SetInt32(command, "crdId", import.PerCollection.CrdId);
        db.SetInt32(command, "obgId", import.PerCollection.ObgId);
        db.SetString(command, "cspNumber", import.PerCollection.CspNumber);
        db.SetString(command, "cpaType", import.PerCollection.CpaType);
        db.SetInt32(command, "otrId", import.PerCollection.OtrId);
        db.SetString(command, "otrType", import.PerCollection.OtrType);
        db.SetInt32(command, "otyId", import.PerCollection.OtyId);
      });

    import.PerCollection.DisbursementDt = disbursementDt;
    import.PerCollection.DisbursementAdjProcessDate =
      disbursementAdjProcessDate;
    import.PerCollection.LastUpdatedBy = lastUpdatedBy;
    import.PerCollection.LastUpdatedTmst = lastUpdatedTmst;
    import.PerCollection.ArNumber = arNumber;
    import.PerCollection.Populated = true;
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
    /// <summary>A HardcodeGroup group.</summary>
    [Serializable]
    public class HardcodeGroup
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

    /// <summary>
    /// A value of ExpCollsEligForRcvCnt.
    /// </summary>
    [JsonPropertyName("expCollsEligForRcvCnt")]
    public Common ExpCollsEligForRcvCnt
    {
      get => expCollsEligForRcvCnt ??= new();
      set => expCollsEligForRcvCnt = value;
    }

    /// <summary>
    /// A value of PerCashReceiptType.
    /// </summary>
    [JsonPropertyName("perCashReceiptType")]
    public CashReceiptType PerCashReceiptType
    {
      get => perCashReceiptType ??= new();
      set => perCashReceiptType = value;
    }

    /// <summary>
    /// A value of PerCollection.
    /// </summary>
    [JsonPropertyName("perCollection")]
    public Collection PerCollection
    {
      get => perCollection ??= new();
      set => perCollection = value;
    }

    /// <summary>
    /// A value of PerDebt.
    /// </summary>
    [JsonPropertyName("perDebt")]
    public ObligationTransaction PerDebt
    {
      get => perDebt ??= new();
      set => perDebt = value;
    }

    /// <summary>
    /// A value of PerObligation.
    /// </summary>
    [JsonPropertyName("perObligation")]
    public Obligation PerObligation
    {
      get => perObligation ??= new();
      set => perObligation = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CollectionUpdateValues.
    /// </summary>
    [JsonPropertyName("collectionUpdateValues")]
    public Collection CollectionUpdateValues
    {
      get => collectionUpdateValues ??= new();
      set => collectionUpdateValues = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// Gets a value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public HardcodeGroup Hardcode
    {
      get => hardcode ?? (hardcode = new());
      set => hardcode = value;
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
    /// A value of ApplyApCollToRcvInd.
    /// </summary>
    [JsonPropertyName("applyApCollToRcvInd")]
    public Common ApplyApCollToRcvInd
    {
      get => applyApCollToRcvInd ??= new();
      set => applyApCollToRcvInd = value;
    }

    private Common expCollsEligForRcvCnt;
    private CashReceiptType perCashReceiptType;
    private Collection perCollection;
    private ObligationTransaction perDebt;
    private Obligation perObligation;
    private ObligationType obligationType;
    private CsePerson obligor;
    private Collection collectionUpdateValues;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private ProgramProcessingInfo programProcessingInfo;
    private HardcodeGroup hardcode;
    private Common displayInd;
    private Common applyApCollToRcvInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of StateCollecionInd.
    /// </summary>
    [JsonPropertyName("stateCollecionInd")]
    public Common StateCollecionInd
    {
      get => stateCollecionInd ??= new();
      set => stateCollecionInd = value;
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
    /// A value of AppliedApCollToRcvInd.
    /// </summary>
    [JsonPropertyName("appliedApCollToRcvInd")]
    public Common AppliedApCollToRcvInd
    {
      get => appliedApCollToRcvInd ??= new();
      set => appliedApCollToRcvInd = value;
    }

    private CsePerson obligee;
    private CsePerson child;
    private Case1 case1;
    private DebtDetail debtDetail;
    private Common stateCollecionInd;
    private Common abort;
    private Common appliedApCollToRcvInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AfInvolvementInd.
    /// </summary>
    [JsonPropertyName("afInvolvementInd")]
    public Common AfInvolvementInd
    {
      get => afInvolvementInd ??= new();
      set => afInvolvementInd = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public DisbursementTransaction ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// A value of Numeric.
    /// </summary>
    [JsonPropertyName("numeric")]
    public BatchTimestampWorkArea Numeric
    {
      get => numeric ??= new();
      set => numeric = value;
    }

    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public DateWorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    /// <summary>
    /// A value of Text2.
    /// </summary>
    [JsonPropertyName("text2")]
    public DateWorkArea Text2
    {
      get => text2 ??= new();
      set => text2 = value;
    }

    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public DateWorkArea DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common afInvolvementInd;
    private CrdCrComboNo crdCrComboNo;
    private DateWorkArea initialized;
    private DisbursementTransaction forCreate;
    private BatchTimestampWorkArea numeric;
    private DateWorkArea text;
    private DateWorkArea text2;
    private DateWorkArea discontinueDate;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
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

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private InterstateRequestObligation interstateRequestObligation;
    private InterstateRequest interstateRequest;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
    private DisbursementTransaction disbursementTransaction;
    private DebtDetail debtDetail;
  }
#endregion
}
