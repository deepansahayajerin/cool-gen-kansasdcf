// Program: FN_REIP_UPDATE_PAYMENT_HISTORY, ID: 372418908, model: 746.
// Short name: SWE02037
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REIP_UPDATE_PAYMENT_HISTORY.
/// </summary>
[Serializable]
public partial class FnReipUpdatePaymentHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_UPDATE_PAYMENT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipUpdatePaymentHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipUpdatePaymentHistory.
  /// </summary>
  public FnReipUpdatePaymentHistory(IContext context, Import import,
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
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Syed Hasan,MTW  01/31/98  PR# 36367
    // - Modified code to correctly perform update function as required
    //   on problem report 36367. Previous code was not selecting the
    //   right occurance for update.
    // J Katz, SRG  03/21/99
    // Modified validation logic to prevent update if the payment
    // history record was adjusted or partially distributed or
    // refunded.
    // J Katz, SRG  06/08/99
    // Analyzed READ statements and changed read property to
    // Select Only where appropriate.
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // 01/04/00  P. Phinney  H00082731  Do NOT allow collection date prior
    // to 01/01/1960 or Before Effective Date.
    // ---------------------------------------------------------------------
    // --------------------------------------------------------------------
    // Set up initial values for local views.
    // --------------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // --------------------------------------------------------------------
    // Read cash receipt to be updated.
    // --------------------------------------------------------------------
    if (ReadCashReceipt())
    {
      // -->  CONTINUE
    }
    else
    {
      ExitState = "FN0086_CASH_RCPT_NF_RB";

      return;
    }

    // --------------------------------------------------------------------
    // Determine if attempts were made to edit identifying values.
    // --------------------------------------------------------------------
    if (ReadCashReceiptEventCashReceiptSourceType())
    {
      if (ReadCashReceiptSourceType())
      {
        // ------------------------------------------------------------
        // Validate that the cash receipt source type was not changed.
        // ------------------------------------------------------------
        if (entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier !=
          entities.NewCashReceiptSourceType.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_SRC_CODE_CHANGED_W_RB";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_CASH_RCPT_SRC_TYP_NF_W_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RCPT_SRC_TYP_NF_W_RB";

      return;
    }

    if (ReadCashReceiptType())
    {
      // ------------------------------------------------------------
      // Validate that the cash receipt type was not changed.
      // ------------------------------------------------------------
      if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtFcourtPmt.SystemGeneratedIdentifier)
      {
        if (AsChar(import.CrOrDp.SelectChar) == 'D')
        {
          ExitState = "FN0000_CANNOT_CHG_CR_TYPE_W_RB";

          return;
        }
      }
      else if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
      {
        if (AsChar(import.CrOrDp.SelectChar) == 'C')
        {
          ExitState = "FN0000_CANNOT_CHG_CR_TYPE_W_RB";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_INVALD_PMT_HIST_CR_TYP_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0114_CASH_RCPT_TYPE_NF_RB";

      return;
    }

    if (ReadCashReceiptStatusHistoryCashReceiptStatus())
    {
      // --------------------------------------------------------------------
      // The active status of the cash receipt must be REC.
      // Any other status is invalid for the update action.
      // --------------------------------------------------------------------
      if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsRecorded.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

        return;
      }
    }
    else
    {
      if (!ReadCashReceiptStatus())
      {
        ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

        return;
      }

      try
      {
        CreateCashReceiptStatusHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0101_CASH_RCPT_STAT_HIST_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0105_CASH_RCPT_STAT_HIST_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // --------------------------------------------------------------------
    // Read cash receipt detail information.  If the detail has a
    // Distributed Amount or Refunded Amount greater than zero,
    // record cannot be changed.    JLK  03/21/99
    // --------------------------------------------------------------------
    if (ReadCashReceiptDetail())
    {
      if (Lt(0, entities.CashReceiptDetail.DistributedAmount) || Lt
        (0, entities.CashReceiptDetail.RefundedAmount))
      {
        ExitState = "FN0000_COLL_OR_REF_EXIST_NO_UD";

        return;
      }
      else
      {
        // -->  CONTINUE
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_DETAIL_NF_RB";

      return;
    }

    if (!ReadCollectionType1())
    {
      // --------------------------------------------------------------------
      // A collection type should always be found, however, if it is
      // not found due to data integrity problem, allow the user to
      // update the record with a valid collection type.  JLK  01/22/99
      // --------------------------------------------------------------------
    }

    // --------------------------------------------------------------------
    // Validate that active status is REC, REL, or SUSP.
    // The cash receipt detail must be in one of these statuses for
    // the update action.
    // Validation logic changed.  Record cannot be updated if
    // active detail status is ADJ.    03/21/99
    // --------------------------------------------------------------------
    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      if (entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsAdjusted.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_PMT_HIST_ADJ_CANT_UPD_DEL";

        return;
      }
    }
    else
    {
      ExitState = "FN0064_CSH_RCPT_DTL_ST_HST_NF_RB";

      return;
    }

    // -----------------------------------------------------------
    // Determine if any changes were made to the data.
    // -----------------------------------------------------------
    if (entities.CashReceiptDetail.CollectionAmount == import
      .CashReceiptDetail.CollectionAmount && Equal
      (entities.CashReceiptDetail.CollectionDate,
      import.CashReceiptDetail.CollectionDate) && Equal
      (entities.ExistingCollectionType.Code, import.New1.Code))
    {
      // --- Nothing to update
      return;
    }

    // -----------------------------------------------------------
    // Process changes to payment history information.
    // -----------------------------------------------------------
    if (!Equal(entities.ExistingCollectionType.Code, import.New1.Code))
    {
      if (ReadCollectionType2())
      {
        // -->  CONTINUE
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF_RB";

        return;
      }
    }

    // -----------------------------------------------------------------
    // Create Cash Receipt Audit record if the Collection Amount changed.
    // -----------------------------------------------------------------
    if (entities.CashReceipt.ReceiptAmount != import
      .CashReceiptDetail.CollectionAmount)
    {
      if (Equal(entities.CashReceipt.LastUpdatedTimestamp, local.Null1.Timestamp))
        
      {
        local.LastUpdated.Timestamp = entities.CashReceipt.CreatedTimestamp;
      }
      else
      {
        local.LastUpdated.Timestamp = entities.CashReceipt.LastUpdatedTimestamp;
      }

      if (IsEmpty(entities.CashReceipt.LastUpdatedBy))
      {
        local.LastUpdatedBy.Text8 = entities.CashReceipt.CreatedBy;
      }
      else
      {
        local.LastUpdatedBy.Text8 = entities.CashReceipt.LastUpdatedBy ?? Spaces
          (8);
      }

      try
      {
        CreateCashReceiptAudit();

        // -->  CONTINUE
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0027_CASH_RCPT_AUDIT_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0029_CASH_RCPT_AUDIT_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // -----------------------------------------------------------------
    // Update Cash Receipt Event and Cash Receipt if the Collection
    // Date or Collection Amount was changed.
    // -----------------------------------------------------------------
    // 01/04/00  P. Phinney  H00082731  Do NOT allow invalid collection date
    if (ReadCollectionType1())
    {
      if (Lt(import.CashReceiptDetail.CollectionDate,
        entities.ExistingCollectionType.EffectiveDate))
      {
        ExitState = "FN0000_COLL_DATE_NOT_VALID";

        return;
      }
    }
    else if (Lt(import.CashReceiptDetail.CollectionDate,
      new DateTime(1960, 1, 1)))
    {
      ExitState = "FN0000_COLL_DATE_NOT_VALID";

      return;
    }

    if (!Equal(entities.ExistingCashReceiptEvent.ReceivedDate,
      import.CashReceiptDetail.CollectionDate))
    {
      try
      {
        UpdateCashReceiptEvent();

        // -->  CONTINUE
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0079_CASH_RCPT_EVENT_NU_W_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0080_CASH_RCPT_EVENT_PV_W_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!Equal(entities.CashReceipt.ReceivedDate,
      import.CashReceiptDetail.CollectionDate) || entities
      .CashReceipt.ReceiptAmount != import.CashReceiptDetail.CollectionAmount)
    {
      try
      {
        UpdateCashReceipt();

        // -->  CONTINUE
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0089_CASH_RCPT_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0091_CASH_RCPT_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ----------------------------------------------------------------------
    // Create new Cash Receipt Detail History and update Cash Receipt Detail.
    // ----------------------------------------------------------------------
    if (Equal(entities.CashReceiptDetail.LastUpdatedTmst, local.Null1.Timestamp))
      
    {
      local.LastUpdated.Timestamp = entities.CashReceiptDetail.CreatedTmst;
    }
    else
    {
      local.LastUpdated.Timestamp = entities.CashReceiptDetail.LastUpdatedTmst;
    }

    if (IsEmpty(entities.CashReceiptDetail.LastUpdatedBy))
    {
      local.LastUpdatedBy.Text8 = entities.CashReceiptDetail.CreatedBy;
    }
    else
    {
      local.LastUpdatedBy.Text8 = entities.CashReceiptDetail.LastUpdatedBy ?? Spaces
        (8);
    }

    try
    {
      CreateCashReceiptDetailHistory();

      // -->  CONTINUE
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0049_CASH_RCPT_DTL_HIST_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0050_CASH_RCPT_DTL_HIST_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      UpdateCashReceiptDetail();

      if (!Equal(entities.ExistingCollectionType.Code, import.New1.Code))
      {
        if (!IsEmpty(entities.ExistingCollectionType.Code))
        {
          DisassociateCashReceiptDetail();
        }

        AssociateCollectionType();
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsRecorded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrtFcourtPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
    local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedCrdsAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private void AssociateCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var cltIdentifier = entities.NewCollectionType.SequentialIdentifier;

    entities.CashReceiptDetail.Populated = false;
    Update("AssociateCollectionType",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Populated = true;
  }

  private void CreateCashReceiptAudit()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var receiptAmount = entities.CashReceipt.ReceiptAmount;
    var lastUpdatedTmst = local.LastUpdated.Timestamp;
    var lastUpdatedBy = local.LastUpdatedBy.Text8;
    var param = 0M;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crtIdentifier = entities.CashReceipt.CrtIdentifier;

    entities.CashReceiptAudit.Populated = false;
    Update("CreateCashReceiptAudit",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDecimal(command, "priorTransnAmt", param);
        db.SetDecimal(command, "priorAdjAmt", param);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
      });

    entities.CashReceiptAudit.ReceiptAmount = receiptAmount;
    entities.CashReceiptAudit.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptAudit.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptAudit.PriorAdjustmentAmount = param;
    entities.CashReceiptAudit.CrvIdentifier = crvIdentifier;
    entities.CashReceiptAudit.CstIdentifier = cstIdentifier;
    entities.CashReceiptAudit.CrtIdentifier = crtIdentifier;
    entities.CashReceiptAudit.Populated = true;
  }

  private void CreateCashReceiptDetailHistory()
  {
    var lastUpdatedTmst = local.LastUpdated.Timestamp;
    var sequentialIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var collectionTypeIdentifier =
      entities.ExistingCollectionType.SequentialIdentifier;
    var cashReceiptEventNumber =
      entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
    var cashReceiptNumber = entities.CashReceipt.SequentialNumber;
    var collectionDate = entities.CashReceiptDetail.CollectionDate;
    var obligorPersonNumber = entities.CashReceiptDetail.ObligorPersonNumber;
    var courtOrderNumber = entities.CashReceiptDetail.CourtOrderNumber ?? Substring
      (entities.CashReceiptDetail.CourtOrderNumber, 1, 17);
    var obligorFirstName = entities.CashReceiptDetail.ObligorFirstName;
    var obligorLastName = entities.CashReceiptDetail.ObligorLastName;
    var obligorMiddleName = entities.CashReceiptDetail.ObligorMiddleName;
    var obligorSocialSecurityNumber =
      entities.CashReceiptDetail.ObligorSocialSecurityNumber;
    var receivedAmount = entities.CashReceiptDetail.ReceivedAmount;
    var collectionAmount = entities.CashReceiptDetail.CollectionAmount;
    var createdBy = entities.CashReceiptDetail.CreatedBy;
    var createdTmst = entities.CashReceiptDetail.CreatedTmst;
    var lastUpdatedBy = local.LastUpdatedBy.Text8;
    var cashReceiptType1 =
      entities.ExistingCashReceiptType.SystemGeneratedIdentifier;
    var cashReceiptSourceType1 =
      entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier;
    var notes = entities.CashReceiptDetail.Notes;

    entities.CashReceiptDetailHistory.Populated = false;
    Update("CreateCashReceiptDetailHistory",
      (db, command) =>
      {
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "interfaceTransId", "");
        db.SetNullableInt32(command, "offsetTaxid", 0);
        db.SetNullableString(
          command, "jointReturnInd", GetImplicitValue<CashReceiptDetailHistory,
          string>("JointReturnInd"));
        db.SetNullableString(command, "jointReturnName", "");
        db.SetNullableDecimal(command, "refundedAmount", 0M);
        db.SetNullableString(command, "adjustmentInd", "");
        db.SetNullableInt32(command, "crdetailHistId", sequentialIdentifier);
        db.SetNullableString(command, "suppPrsnFn2", "");
        db.SetNullableString(command, "suppPrsnMn2", "");
        db.SetInt32(command, "collctTypeId", collectionTypeIdentifier);
        db.SetInt32(command, "creventNbrId", cashReceiptEventNumber);
        db.SetInt32(command, "crNbrId", cashReceiptNumber);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "oblgorPersNbrId", obligorPersonNumber);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", "");
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMiddleNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", "");
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableInt32(command, "offsetTaxYear", 0);
        db.SetNullableString(
          command, "dfltCllctnDtInd", GetImplicitValue<CashReceiptDetailHistory,
          string>("DefaultedCollectionDateInd"));
        db.SetNullableString(
          command, "multiPayor", GetImplicitValue<CashReceiptDetailHistory,
          string>("MultiPayor"));
        db.SetNullableDecimal(command, "receivedAmount", receivedAmount);
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(command, "cashRecType", cashReceiptType1);
        db.SetInt32(command, "cashRecSrcType", cashReceiptSourceType1);
        db.SetNullableString(command, "referenc", "");
        db.SetNullableString(command, "notes", notes);
      });

    entities.CashReceiptDetailHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetailHistory.SequentialIdentifier =
      sequentialIdentifier;
    entities.CashReceiptDetailHistory.CollectionTypeIdentifier =
      collectionTypeIdentifier;
    entities.CashReceiptDetailHistory.CashReceiptEventNumber =
      cashReceiptEventNumber;
    entities.CashReceiptDetailHistory.CashReceiptNumber = cashReceiptNumber;
    entities.CashReceiptDetailHistory.CollectionDate = collectionDate;
    entities.CashReceiptDetailHistory.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetailHistory.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetailHistory.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetailHistory.ObligorLastName = obligorLastName;
    entities.CashReceiptDetailHistory.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetailHistory.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetailHistory.CollectionAmount = collectionAmount;
    entities.CashReceiptDetailHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailHistory.CreatedTmst = createdTmst;
    entities.CashReceiptDetailHistory.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetailHistory.CashReceiptType = cashReceiptType1;
    entities.CashReceiptDetailHistory.CashReceiptSourceType =
      cashReceiptSourceType1;
    entities.CashReceiptDetailHistory.Notes = notes;
    entities.CashReceiptDetailHistory.Populated = true;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var crsIdentifier =
      entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;

    entities.ActiveCashReceiptStatusHistory.Populated = false;
    Update("CreateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crsIdentifier", crsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.ActiveCashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.ActiveCashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.ActiveCashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.ActiveCashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.ActiveCashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.ActiveCashReceiptStatusHistory.CreatedBy = createdBy;
    entities.ActiveCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.ActiveCashReceiptStatusHistory.Populated = true;
  }

  private void DisassociateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetail.Populated = false;
    Update("DisassociateCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.CltIdentifier = null;
    entities.CashReceiptDetail.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 5);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 6);
        entities.CashReceipt.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 11);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 13);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 14);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 16);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 17);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 18);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ActiveCashReceiptDetailStatHistory.Populated = false;
    entities.ActiveCashReceiptDetailStatus.Populated = false;

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
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ActiveCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ActiveCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ActiveCashReceiptDetailStatHistory.Populated = true;
        entities.ActiveCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ExistingCashReceiptEvent.Populated = false;
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.ExistingCashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ExistingCashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 4);
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.NewCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.NewCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NewCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.NewCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsRecorded.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ExistingCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptType.Populated = true;
      });
  }

  private bool ReadCollectionType1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ExistingCollectionType.Populated = false;

    return Read("ReadCollectionType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollectionType.Code = db.GetString(reader, 1);
        entities.ExistingCollectionType.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCollectionType.Populated = true;
      });
  }

  private bool ReadCollectionType2()
  {
    entities.NewCollectionType.Populated = false;

    return Read("ReadCollectionType2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "code", import.New1.Code);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.NewCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.NewCollectionType.Code = db.GetString(reader, 1);
        entities.NewCollectionType.EffectiveDate = db.GetDate(reader, 2);
        entities.NewCollectionType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.NewCollectionType.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var receiptAmount = import.CashReceiptDetail.CollectionAmount;
    var receivedDate = import.CashReceiptDetail.CollectionDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.ReceiptAmount = receiptAmount;
    entities.CashReceipt.ReceivedDate = receivedDate;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var receivedAmount = import.CashReceiptDetail.CollectionAmount;
    var collectionDate = import.CashReceiptDetail.CollectionDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;

    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetDecimal(command, "receivedAmount", receivedAmount);
        db.SetDecimal(command, "collectionAmount", receivedAmount);
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionDate = collectionDate;
    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptEvent()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptEvent.Populated);

    var receivedDate = import.CashReceiptDetail.CollectionDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;

    entities.ExistingCashReceiptEvent.Populated = false;
    Update("UpdateCashReceiptEvent",
      (db, command) =>
      {
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptEvent.CstIdentifier);
        db.SetInt32(
          command, "creventId",
          entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier);
      });

    entities.ExistingCashReceiptEvent.ReceivedDate = receivedDate;
    entities.ExistingCashReceiptEvent.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCashReceiptEvent.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingCashReceiptEvent.Populated = true;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CollectionType New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of CrOrDp.
    /// </summary>
    [JsonPropertyName("crOrDp")]
    public Common CrOrDp
    {
      get => crOrDp ??= new();
      set => crOrDp = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType new1;
    private Common crOrDp;
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
    /// A value of LastUpdatedBy.
    /// </summary>
    [JsonPropertyName("lastUpdatedBy")]
    public TextWorkArea LastUpdatedBy
    {
      get => lastUpdatedBy ??= new();
      set => lastUpdatedBy = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of LastUpdated.
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public DateWorkArea LastUpdated
    {
      get => lastUpdated ??= new();
      set => lastUpdated = value;
    }

    /// <summary>
    /// A value of HardcodedCrsRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedCrsRecorded")]
    public CashReceiptStatus HardcodedCrsRecorded
    {
      get => hardcodedCrsRecorded ??= new();
      set => hardcodedCrsRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFcourtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFcourtPmt")]
    public CashReceiptType HardcodedCrtFcourtPmt
    {
      get => hardcodedCrtFcourtPmt ??= new();
      set => hardcodedCrtFcourtPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFdirPmt")]
    public CashReceiptType HardcodedCrtFdirPmt
    {
      get => hardcodedCrtFdirPmt ??= new();
      set => hardcodedCrtFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsAdjusted")]
    public CashReceiptDetailStatus HardcodedCrdsAdjusted
    {
      get => hardcodedCrdsAdjusted ??= new();
      set => hardcodedCrdsAdjusted = value;
    }

    /// <summary>
    /// A value of CollectionTypeFound.
    /// </summary>
    [JsonPropertyName("collectionTypeFound")]
    public Common CollectionTypeFound
    {
      get => collectionTypeFound ??= new();
      set => collectionTypeFound = value;
    }

    private TextWorkArea lastUpdatedBy;
    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private DateWorkArea lastUpdated;
    private CashReceiptStatus hardcodedCrsRecorded;
    private CashReceiptType hardcodedCrtFcourtPmt;
    private CashReceiptType hardcodedCrtFdirPmt;
    private CashReceiptDetailStatus hardcodedCrdsAdjusted;
    private Common collectionTypeFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptAudit.
    /// </summary>
    [JsonPropertyName("cashReceiptAudit")]
    public CashReceiptAudit CashReceiptAudit
    {
      get => cashReceiptAudit ??= new();
      set => cashReceiptAudit = value;
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
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
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
    /// A value of NewCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("newCashReceiptSourceType")]
    public CashReceiptSourceType NewCashReceiptSourceType
    {
      get => newCashReceiptSourceType ??= new();
      set => newCashReceiptSourceType = value;
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
    /// A value of ActiveCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ActiveCashReceiptStatusHistory
    {
      get => activeCashReceiptStatusHistory ??= new();
      set => activeCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatus")]
    public CashReceiptStatus ActiveCashReceiptStatus
    {
      get => activeCashReceiptStatus ??= new();
      set => activeCashReceiptStatus = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of NewCollectionType.
    /// </summary>
    [JsonPropertyName("newCollectionType")]
    public CollectionType NewCollectionType
    {
      get => newCollectionType ??= new();
      set => newCollectionType = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("activeCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ActiveCashReceiptDetailStatHistory
    {
      get => activeCashReceiptDetailStatHistory ??= new();
      set => activeCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ActiveCashReceiptDetailStatus
    {
      get => activeCashReceiptDetailStatus ??= new();
      set => activeCashReceiptDetailStatus = value;
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

    private CashReceiptAudit cashReceiptAudit;
    private CashReceipt cashReceipt;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptSourceType newCashReceiptSourceType;
    private CashReceiptType existingCashReceiptType;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private CashReceiptDetailHistory cashReceiptDetailHistory;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType existingCollectionType;
    private CollectionType newCollectionType;
    private CashReceiptDetailStatHistory activeCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus activeCashReceiptDetailStatus;
    private CollectionType collectionType;
  }
#endregion
}
