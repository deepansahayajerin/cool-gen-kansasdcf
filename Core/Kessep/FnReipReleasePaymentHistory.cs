// Program: FN_REIP_RELEASE_PAYMENT_HISTORY, ID: 372418907, model: 746.
// Short name: SWE02434
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REIP_RELEASE_PAYMENT_HISTORY.
/// </summary>
[Serializable]
public partial class FnReipReleasePaymentHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_RELEASE_PAYMENT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipReleasePaymentHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipReleasePaymentHistory.
  /// </summary>
  public FnReipReleasePaymentHistory(IContext context, Import import,
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
    // ----------------------------------------------------------------------
    // The active status on all payment history records will be changed from
    // REC to REL.  If any records are found with an active status of SUSP,
    // processing will be discontinued.
    // ----------------------------------------------------------------------
    // ---------------------------------------------------------------------
    //                              Change Log
    // ---------------------------------------------------------------------
    // Date		Developer	Description
    // ---------------------------------------------------------------------
    // 06/08/99	J. Katz		Analyzed READ statements and changed
    // 				read property to Select Only where
    // 				appropriate.
    // 08/26/99	J. Katz		Set Override Manual Dist Ind to Y to
    // 				bypass manual distribution on payment
    // 				history.  [Ref:  PR #238]
    // ---------------------------------------------------------------------
    // 01/21/00 	P. Phinney      H00084490
    // Allow Release of Suspended Payments
    // 				When the Suspense reason is anything
    // 				EXCEPT manual Distribution.
    // 01/07/02 	P. Phinney      I 010561
    // ByPass REIPDELETE CRD's
    // ---------------------------------------------------------------------
    // ----------------------------------------------------------------------
    // The AP/Payor CSE Person Number and Court Order Number must be
    // imported.
    // ----------------------------------------------------------------------
    if (IsEmpty(import.CsePerson.Number) || IsEmpty
      (import.LegalAction.StandardNumber))
    {
      ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

      return;
    }

    // ----------------------------------------------------------------------
    // Set up local views.
    // ----------------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // 01/07/02 	P. Phinney      I 010561
    // ByPass REIPDELETE CRD's
    local.HardcodedCrdsReipdelete.SystemGeneratedIdentifier = 8;

    // ----------------------------------------------------------------------
    // Read the new RELEASED Cash Receipt Detail Status
    // ----------------------------------------------------------------------
    if (!ReadCashReceiptDetailStatus())
    {
      ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

      return;
    }

    // ----------------------------------------------------------------------
    // Find all payment history records for imported AP/Payor and Court Order.
    // ----------------------------------------------------------------------
    foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType())
    {
      // -----------------------------------------------------------------
      // Retrieve the active detail status.
      // -----------------------------------------------------------------
      if (!ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
      {
        ExitState = "FN0064_CSH_RCPT_DTL_ST_HST_NF_RB";

        return;
      }

      if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsRecorded.SystemGeneratedIdentifier)
      {
        // -----------------------------------------------------------
        // The active status is REC, change the status to REL.
        // -----------------------------------------------------------
        try
        {
          UpdateCashReceiptDetailStatHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0065_CSH_RCPT_DTL_ST_HST_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          CreateCashReceiptDetailStatHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0067_CSH_RCPT_DTL_ST_HS_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // -------------------------------------------------------------
        // Bypass manual distribution when releasing payment
        // history records per PR #238.
        // No Cash Receipt Detail History record is required since the
        // Override Manual Dist Ind is not part of that table.
        // JLK  08/26/99
        // -------------------------------------------------------------
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
      }
      else if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsSuspended.SystemGeneratedIdentifier)
      {
        // 01/21/00	P. Phinney      H00084490
        // -----------------------------------------------------------
        // The active status is SUSP, change the status to REL.
        // -----------------------------------------------------------
        try
        {
          UpdateCashReceiptDetailStatHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0065_CSH_RCPT_DTL_ST_HST_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          CreateCashReceiptDetailStatHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0067_CSH_RCPT_DTL_ST_HS_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // -------------------------------------------------------------
        // Bypass manual distribution when releasing payment
        // history records per PR #238.
        // No Cash Receipt Detail History record is required since the
        // Override Manual Dist Ind is not part of that table.
        // JLK  08/26/99
        // -------------------------------------------------------------
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
      }
      else if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsPended.SystemGeneratedIdentifier)
      {
        // 01/21/00	P. Phinney      H00084490
        // Removed Suspended  and changed Exit_State
        // -----------------------------------------------------------
        // The active status of one or more payment history records
        // is SUSP or PEND.  Payment history cannot be released.
        // Rollback all processing.    JLK  03/21/99
        // -----------------------------------------------------------
        ExitState = "FN0000_PMT_HIST_RECORDS_PENDED";

        return;
      }
      else if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsReleased.SystemGeneratedIdentifier || entities
        .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsDistributed.SystemGeneratedIdentifier || entities
        .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsRefunded.SystemGeneratedIdentifier || entities
        .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsAdjusted.SystemGeneratedIdentifier || entities
        .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedCrdsReipdelete.SystemGeneratedIdentifier)
      {
        // 01/07/02 	P. Phinney      I 010561
        // ByPass REIPDELETE CRD's
        // -----------------------------------------------------------
        // This payment history record has already been released or
        // processed (distributed, refunded, or adjusted).
        // No processing necessary.    JLK  03/21/99
        // -----------------------------------------------------------
        continue;
      }
      else
      {
        // -----------------------------------------------------------
        // Invalid status for the Release action.
        // Rollback all processing.
        // -----------------------------------------------------------
        ExitState = "FN0000_INVALID_STAT_4_REQ_ACT_RB";

        return;
      }

      ++local.ReleasedCrdtl.Count;
    }

    if (local.ReleasedCrdtl.Count > 0)
    {
      ExitState = "FN0000_COLL_RELEASE_SUCCESSFUL";
    }
    else
    {
      ExitState = "FN0000_NO_CRD_AVAIL_FOR_RELEASE";
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

    local.HardcodedCrtFcourtPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
    local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedCrdsRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrdsReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedCrdsSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedCrdsPended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
    local.HardcodedCrdsDistributed.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    local.HardcodedCrdsAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
    local.HardcodedCrdsRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier = entities.NewReleased.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;

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
        db.SetNullableString(command, "reasonText", "");
      });

    entities.CashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.CashReceiptDetailStatHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.Populated = true;
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType()
  {
    entities.CashReceiptType.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.CsePerson.Number);
        db.SetNullableString(
          command, "courtOrderNumber", import.LegalAction.StandardNumber ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedCrtFcourtPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 7);
        entities.CashReceiptType.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
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
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.NewReleased.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          local.HardcodedCrdsReleased.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NewReleased.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.NewReleased.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.NewReleased.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var overrideManualDistInd = "Y";

    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "ovrrdMnlDistInd", overrideManualDistInd);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.OverrideManualDistInd = overrideManualDistInd;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailStatHistory.Populated);

    var discontinueDate = local.Current.Date;

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private LegalAction legalAction;
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
    /// A value of ReleasedCrdtl.
    /// </summary>
    [JsonPropertyName("releasedCrdtl")]
    public Common ReleasedCrdtl
    {
      get => releasedCrdtl ??= new();
      set => releasedCrdtl = value;
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
    /// A value of HardcodedCrdsRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsRecorded")]
    public CashReceiptDetailStatus HardcodedCrdsRecorded
    {
      get => hardcodedCrdsRecorded ??= new();
      set => hardcodedCrdsRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsReleased.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsReleased")]
    public CashReceiptDetailStatus HardcodedCrdsReleased
    {
      get => hardcodedCrdsReleased ??= new();
      set => hardcodedCrdsReleased = value;
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
    /// A value of HardcodedCrdsPended.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsPended")]
    public CashReceiptDetailStatus HardcodedCrdsPended
    {
      get => hardcodedCrdsPended ??= new();
      set => hardcodedCrdsPended = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsDistributed.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsDistributed")]
    public CashReceiptDetailStatus HardcodedCrdsDistributed
    {
      get => hardcodedCrdsDistributed ??= new();
      set => hardcodedCrdsDistributed = value;
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
    /// A value of HardcodedCrdsRefunded.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsRefunded")]
    public CashReceiptDetailStatus HardcodedCrdsRefunded
    {
      get => hardcodedCrdsRefunded ??= new();
      set => hardcodedCrdsRefunded = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsReipdelete.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsReipdelete")]
    public CashReceiptDetailStatus HardcodedCrdsReipdelete
    {
      get => hardcodedCrdsReipdelete ??= new();
      set => hardcodedCrdsReipdelete = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Common releasedCrdtl;
    private CashReceiptType hardcodedCrtFcourtPmt;
    private CashReceiptType hardcodedCrtFdirPmt;
    private CashReceiptDetailStatus hardcodedCrdsRecorded;
    private CashReceiptDetailStatus hardcodedCrdsReleased;
    private CashReceiptDetailStatus hardcodedCrdsSuspended;
    private CashReceiptDetailStatus hardcodedCrdsPended;
    private CashReceiptDetailStatus hardcodedCrdsDistributed;
    private CashReceiptDetailStatus hardcodedCrdsAdjusted;
    private CashReceiptDetailStatus hardcodedCrdsRefunded;
    private CashReceiptDetailStatus hardcodedCrdsReipdelete;
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
    /// A value of NewReleased.
    /// </summary>
    [JsonPropertyName("newReleased")]
    public CashReceiptDetailStatus NewReleased
    {
      get => newReleased ??= new();
      set => newReleased = value;
    }

    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatus newReleased;
  }
#endregion
}
