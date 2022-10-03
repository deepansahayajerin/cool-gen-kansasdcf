// Program: FN_REIP_DELETE_PAYMENT_HISTORY, ID: 372418917, model: 746.
// Short name: SWE02053
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_REIP_DELETE_PAYMENT_HISTORY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This acblk deletes a cash receipt and cash receipt detail entered through 
/// REIP
/// </para>
/// </summary>
[Serializable]
public partial class FnReipDeletePaymentHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_DELETE_PAYMENT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipDeletePaymentHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipDeletePaymentHistory.
  /// </summary>
  public FnReipDeletePaymentHistory(IContext context, Import import,
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
    // ---------------------------------------------------------------------
    // Date	By	IDCR #	Description
    // ---------------------------------------------------------------------
    // 061797	govind		Initial creation
    // 03/21/99  J. Katz	Add edits to disallow the delete action if
    // 			there are existing collections or receipt
    // 			refunds or if the detail status is ADJ.
    // 06/08/99  J. Katz	Analyzed READ statements and changed read
    // 			property to Select Only where appropriate.
    // ---------------------------------------------------------------------
    // 01/12/00 P Phinney H00084289 If distribution has run
    // and full amount is suspended
    // DO NOT allow delete of Cash_Receipt_Detail
    // ---------------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "INITIAL_EXIT_STATE_NOT_ALL_OK";

      return;
    }

    // --------------------------------------------------------------------
    // Set up local views.
    // --------------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Day = Day(local.Current.Date);
    local.Current.Month = Month(local.Current.Date);
    local.Current.Year = Year(local.Current.Date);
    local.CurrentDateText.Text10 =
      NumberToString(local.Current.Month, 14, 2) + "/" + NumberToString
      (local.Current.Day, 14, 2) + "/" + NumberToString
      (local.Current.Year, 12, 4);

    // --------------------------------------------------------------------
    // Read the Cash Receipt and Cash Receipt Detail records to be deleted.
    // --------------------------------------------------------------------
    if (!ReadCashReceiptDetailCashReceipt())
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    // -----------------------------------------------------------------
    // Payment history record cannot be deleted if the detail has
    // associated collections.    JLK  03/21/99
    // -----------------------------------------------------------------
    // H00080972   12/07/99  pphinney   Fix -811
    if (ReadCollection())
    {
      ExitState = "FN0000_COLLECTION_EXIST_CANT_DEL";

      return;
    }

    // -----------------------------------------------------------------
    // Payment history record cannot be deleted if the detail has
    // associated refunds.    JLK  03/21/99
    // A detail record may have more than one refund.  If even one
    // refund is found, record cannot be deleted.  Read property is
    // set to Cursor Only to reflect this business rule.
    // JLK 06/08/99
    // -----------------------------------------------------------------
    if (ReadReceiptRefund())
    {
      ExitState = "FN0000_REFUNDS_EXIST_CANT_DELETE";

      return;
    }
    else
    {
      // --->  Continue
    }

    // -----------------------------------------------------------------
    // Active cash receipt status must be REC to execute the
    // delete action.
    // -----------------------------------------------------------------
    UseFnHardcodedCashReceipting();

    if (ReadCashReceiptStatusHistoryCashReceiptStatus())
    {
      if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsRecorded.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_INVALD_STAT_4_REQ_ACT_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_STATUS_NF";
    }

    // -----------------------------------------------------------------
    // Active cash receipt detao; status cannot be ADJ to execute the
    // delete action.    JLK  03/21/99
    // -----------------------------------------------------------------
    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      if (entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsAdjusted.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_PMT_HIST_ADJ_CANT_UPD_DEL";

        return;
      }
      else
      {
        // --->  Continue
      }

      // -----------------------------------------------------------
      // 01/12/00 H00084289 P.Phinney If distribution has run
      // and full amount is suspended
      // DO NOT allow delete of Cash_Receipt_Detail
      // -----------------------------------------------------------
      if (entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsSuspended.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_REIP_FULL_SUSP_NO_DELETE";

        return;
      }
      else
      {
        // --->  Continue
      }
    }
    else
    {
      // ------------------------------------------------------------
      // Even though this is a data integrity problem, allow the
      // delete.  JLK  01/22/99
      // ------------------------------------------------------------
    }

    // ---------------------------------------------------------------
    // Delete the payment history record.
    // First delete the cash receipt detail.
    // ---------------------------------------------------------------
    // WR010561  pphinney  07/09/01  Delete changed
    if (IsExitState("FN0000_CASH_RECEIPT_STATUS_NF"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      // ------------------------------------------------------------
      // End date the current cash receipt status history record.
      // ------------------------------------------------------------
      try
      {
        UpdateCashReceiptStatusHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0106_CASH_RCPT_STAT_HIST_NU_RB";

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

    // *****************************************************************
    // pphinney 07/09/01
    if (ReadCashReceiptDetailStatus())
    {
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier;
    }
    else
    {
      ExitState = "FN0000_CASH_RCPT_DTL_S_HST_NF_RB";

      return;
    }

    local.CashReceiptDetailStatHistory.ReasonCodeId = "REIPDELETE";

    // *****************************************************************
    UseFnChangeCashRcptDtlStatHis();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      ExitState = "FN0000_CASH_RCPT_DTL_S_HST_NF_RB";

      return;
    }

    // *****************************************************************
    // This FLAG will make sure Distribution Ignores this Record
    // *****************************************************************
    local.ForUpdate.CollectionAmtFullyAppliedInd = "Y";
    local.ForUpdate.LastUpdatedTmst = Now();
    local.ForUpdate.LastUpdatedBy = global.UserId;
    local.ForUpdate.Notes = TrimEnd(local.ForUpdate.Notes) + " --- Cash Receipt Detail DELETED by REIP on " +
      local.CurrentDateText.Text10;

    try
    {
      UpdateCashReceiptDetail();

      // *****************************************************************
      // This FLAG will make sure Distribution Ignores this Record
      // *****************************************************************
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

    // ---------------------------------------------------------------
    // Read the DELETED cash receipt status record.
    // Read the appropriate cash receipt delete reason.
    // ---------------------------------------------------------------
    if (!ReadCashReceiptStatus())
    {
      ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

      return;
    }

    if (!ReadCashReceiptDeleteReason())
    {
      ExitState = "FN0000_CR_DELETE_REASON_NF_RB";

      return;
    }

    // ---------------------------------------------------------------
    // Create a new active cash receipt status of DEL.
    // ---------------------------------------------------------------
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

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0105_CASH_RCPT_STAT_HIST_PV_RB";

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

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.New1.ReasonCodeId =
      local.CashReceiptDetailStatHistory.ReasonCodeId;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsRecorded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrdsAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
    local.HardcodedCrdsSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var crsIdentifier = entities.NewDeleted.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;
    var reasonText = "Payment History deleted on REIP.";
    var cdrIdentifier =
      entities.CashReceiptDeleteReason.SystemGeneratedIdentifier;

    entities.CashReceiptStatusHistory.Populated = false;
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
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableInt32(command, "cdrIdentifier", cdrIdentifier);
      });

    entities.CashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.CashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptStatusHistory.CreatedBy = createdBy;
    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.ReasonText = reasonText;
    entities.CashReceiptStatusHistory.CdrIdentifier = cdrIdentifier;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private bool ReadCashReceiptDeleteReason()
  {
    entities.CashReceiptDeleteReason.Populated = false;

    return Read("ReadCashReceiptDeleteReason",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDeleteReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDeleteReason.Code = db.GetString(reader, 1);
        entities.CashReceiptDeleteReason.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptDeleteReason.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptDeleteReason.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceipt()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 9);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 10);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
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
        entities.ActiveCashReceiptDetailStatus.Code = db.GetString(reader, 7);
        entities.ActiveCashReceiptDetailStatHistory.Populated = true;
        entities.ActiveCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.ActiveCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      null,
      (db, reader) =>
      {
        entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.ActiveCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.NewDeleted.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsDeleted.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.NewDeleted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.NewDeleted.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ActiveCashReceiptStatus.Populated = false;
    entities.CashReceiptStatusHistory.Populated = false;

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
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.CashReceiptStatusHistory.CdrIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ActiveCashReceiptStatus.Populated = true;
        entities.CashReceiptStatusHistory.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

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
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private bool ReadReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Existing.Populated = false;

    return Read("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Existing.CrvIdentifier = db.GetNullableInt32(reader, 1);
        entities.Existing.CrdIdentifier = db.GetNullableInt32(reader, 2);
        entities.Existing.CrtIdentifier = db.GetNullableInt32(reader, 3);
        entities.Existing.CstIdentifier = db.GetNullableInt32(reader, 4);
        entities.Existing.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = local.ForUpdate.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.ForUpdate.LastUpdatedTmst;
    var collectionAmtFullyAppliedInd =
      local.ForUpdate.CollectionAmtFullyAppliedInd ?? "";
    var notes = local.ForUpdate.Notes ?? "";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetNullableString(command, "notes", notes);
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
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.CashReceiptDetail.Notes = notes;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.CashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.CashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.Populated = true;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    /// <summary>
    /// A value of TbdexportImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("tbdexportImportNumberOfReads")]
    public Common TbdexportImportNumberOfReads
    {
      get => tbdexportImportNumberOfReads ??= new();
      set => tbdexportImportNumberOfReads = value;
    }

    private Common importNumberOfUpdates;
    private Common tbdexportImportNumberOfReads;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CurrentDateText.
    /// </summary>
    [JsonPropertyName("currentDateText")]
    public TextWorkArea CurrentDateText
    {
      get => currentDateText ??= new();
      set => currentDateText = value;
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
    /// A value of HardcodedCrsRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedCrsRecorded")]
    public CashReceiptStatus HardcodedCrsRecorded
    {
      get => hardcodedCrsRecorded ??= new();
      set => hardcodedCrsRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeleted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeleted")]
    public CashReceiptStatus HardcodedCrsDeleted
    {
      get => hardcodedCrsDeleted ??= new();
      set => hardcodedCrsDeleted = value;
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
    /// A value of HardcodedCrdsSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsSuspended")]
    public CashReceiptDetailStatus HardcodedCrdsSuspended
    {
      get => hardcodedCrdsSuspended ??= new();
      set => hardcodedCrdsSuspended = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public CashReceiptDetail ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    private DateWorkArea current;
    private TextWorkArea currentDateText;
    private DateWorkArea max;
    private CashReceiptStatus hardcodedCrsRecorded;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptDetailStatus hardcodedCrdsAdjusted;
    private CashReceiptDetailStatus hardcodedCrdsSuspended;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail forUpdate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public ReceiptRefund Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of ActiveCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatus")]
    public CashReceiptStatus ActiveCashReceiptStatus
    {
      get => activeCashReceiptStatus ??= new();
      set => activeCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of NewDeleted.
    /// </summary>
    [JsonPropertyName("newDeleted")]
    public CashReceiptStatus NewDeleted
    {
      get => newDeleted ??= new();
      set => newDeleted = value;
    }

    /// <summary>
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("cashReceiptDeleteReason")]
    public CashReceiptDeleteReason CashReceiptDeleteReason
    {
      get => cashReceiptDeleteReason ??= new();
      set => cashReceiptDeleteReason = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private ReceiptRefund existing;
    private CashReceiptDetailStatHistory activeCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus activeCashReceiptDetailStatus;
    private CashReceiptStatus activeCashReceiptStatus;
    private CashReceiptStatus newDeleted;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
  }
#endregion
}
