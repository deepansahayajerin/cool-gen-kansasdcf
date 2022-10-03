// Program: FN_UNMATCH_INTERFACE_RECEIPT, ID: 372346722, model: 746.
// Short name: SWE02445
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UNMATCH_INTERFACE_RECEIPT.
/// </summary>
[Serializable]
public partial class FnUnmatchInterfaceReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UNMATCH_INTERFACE_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUnmatchInterfaceReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUnmatchInterfaceReceipt.
  /// </summary>
  public FnUnmatchInterfaceReceipt(IContext context, Import import,
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
    // ------------------------------------------------------------------
    //                          Change Log
    // ------------------------------------------------------------------
    // Date      Developer		Description
    // ------------------------------------------------------------------
    // 06/08/99  J. Katz		Analyzed READ statements and
    // 				changed read property to Select
    // 				Only where appropriate.
    // ------------------------------------------------------------------
    if (import.CashReceipt.SequentialNumber == 0)
    {
      ExitState = "FN0000_CASH_RECEIPT_NF";

      return;
    }

    // ------------------------------------------------------------------
    // Set up local views.
    // ------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();
    local.HardcodedCrtEft.SystemGeneratedIdentifier = 6;

    // ------------------------------------------------------------------
    // Read imported Cash Receipt and Cash Receipt Type.
    // BOGUS type receipts are not valid for the unmatch action.
    // ------------------------------------------------------------------
    if (ReadCashReceiptCashReceiptType())
    {
      // --------------------------------------------------------------
      // EFT receipts cannot be unmatched.
      // --------------------------------------------------------------
      if (entities.ExistingCashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtEft.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

        return;
      }

      // --------------------------------------------------------------
      // BOGUS receipts cannot be unmatched.
      // --------------------------------------------------------------
      if (Equal(entities.ExistingCashReceiptType.Code, "BOGUS"))
      {
        ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_NF";

      return;
    }

    // ------------------------------------------------------------------
    // Read active status history and status code.
    // Active cash receipt status must be REC.
    // ------------------------------------------------------------------
    if (ReadCashReceiptStatusHistoryCashReceiptStatus())
    {
      if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsReceipted.SystemGeneratedIdentifier)
      {
        ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_STAT_HIST_NF";

      return;
    }

    // ------------------------------------------------------------------
    // Determine if the receipt participates in any Cash Receipt
    // adjustments with a reason code of ADDPMT, NETPMT, or
    // REFUND.  If adjustments exist, unmatch action is denied.
    // Added two new Cash Receipt Rln Rsn codes for receipt
    // amount adjustments -- PROCCSTFEE and NETINTFERR.
    // JLK  06/21/99
    // ------------------------------------------------------------------
    if (ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn())
    {
      ExitState = "FN0000_CR_ADJ_EXIST_NO_MTC_UNMTC";

      return;
    }

    // ------------------------------------------------------------------
    // Read new cash receipt status value for positioning.
    // ------------------------------------------------------------------
    if (!ReadCashReceiptStatus())
    {
      ExitState = "FN0000_CASH_RECEIPT_STATUS_NF";

      return;
    }

    // ------------------------------------------------------------------
    // Determine value for Last Updated By and Last Updated
    // Timestamp for the audit record.
    // ------------------------------------------------------------------
    if (IsEmpty(entities.ExistingCashReceipt.LastUpdatedBy))
    {
      local.CashReceipt.LastUpdatedBy = entities.ExistingCashReceipt.CreatedBy;
    }
    else
    {
      local.CashReceipt.LastUpdatedBy =
        entities.ExistingCashReceipt.LastUpdatedBy;
    }

    if (Equal(entities.ExistingCashReceipt.LastUpdatedTimestamp,
      local.Null1.Timestamp))
    {
      local.CashReceipt.LastUpdatedTimestamp =
        entities.ExistingCashReceipt.CreatedTimestamp;
    }
    else
    {
      local.CashReceipt.LastUpdatedTimestamp =
        entities.ExistingCashReceipt.LastUpdatedTimestamp;
    }

    // ------------------------------------------------------------------
    // Create a Cash Receipt Audit record to capture the change in
    // the receipt amount.
    // ------------------------------------------------------------------
    try
    {
      CreateCashReceiptAudit();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0025_CASH_RCPT_AUDIT_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_CASH_RCPT_AUDIT_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ------------------------------------------------------------------
    // Update the imported Cash Receipt to only reflect the
    // interface receipt data.
    // ------------------------------------------------------------------
    try
    {
      UpdateCashReceipt();
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

    // ------------------------------------------------------------------
    // End date the active status history record and create a new
    // status history record to place the receipt in INTF status.
    // ------------------------------------------------------------------
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

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrsInterface.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
  }

  private void CreateCashReceiptAudit()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);

    var receiptAmount = entities.ExistingCashReceipt.ReceiptAmount;
    var lastUpdatedTmst = local.CashReceipt.LastUpdatedTimestamp;
    var lastUpdatedBy = local.CashReceipt.LastUpdatedBy ?? "";
    var priorTransactionAmount =
      entities.ExistingCashReceipt.TotalCashTransactionAmount;
    var priorAdjustmentAmount = 0M;
    var crvIdentifier = entities.ExistingCashReceipt.CrvIdentifier;
    var cstIdentifier = entities.ExistingCashReceipt.CstIdentifier;
    var crtIdentifier = entities.ExistingCashReceipt.CrtIdentifier;

    entities.NewCashReceiptAudit.Populated = false;
    Update("CreateCashReceiptAudit",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDecimal(command, "priorTransnAmt", priorTransactionAmount);
          
        db.SetDecimal(command, "priorAdjAmt", priorAdjustmentAmount);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
      });

    entities.NewCashReceiptAudit.ReceiptAmount = receiptAmount;
    entities.NewCashReceiptAudit.LastUpdatedTmst = lastUpdatedTmst;
    entities.NewCashReceiptAudit.LastUpdatedBy = lastUpdatedBy;
    entities.NewCashReceiptAudit.PriorTransactionAmount =
      priorTransactionAmount;
    entities.NewCashReceiptAudit.PriorAdjustmentAmount = priorAdjustmentAmount;
    entities.NewCashReceiptAudit.CrvIdentifier = crvIdentifier;
    entities.NewCashReceiptAudit.CstIdentifier = cstIdentifier;
    entities.NewCashReceiptAudit.CrtIdentifier = crtIdentifier;
    entities.NewCashReceiptAudit.Populated = true;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);

    var crtIdentifier = entities.ExistingCashReceipt.CrtIdentifier;
    var cstIdentifier = entities.ExistingCashReceipt.CstIdentifier;
    var crvIdentifier = entities.ExistingCashReceipt.CrvIdentifier;
    var crsIdentifier = entities.NewCashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;

    entities.NewCashReceiptStatusHistory.Populated = false;
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

    entities.NewCashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.NewCashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.NewCashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.NewCashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.NewCashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.NewCashReceiptStatusHistory.CreatedBy = createdBy;
    entities.NewCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.NewCashReceiptStatusHistory.Populated = true;
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptBalanceAdjustment.Populated = false;
    entities.ExistingCashReceiptRlnRsn.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIIdentifier",
          entities.ExistingCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.ExistingCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.ExistingCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingCashReceiptRlnRsn.Code = db.GetString(reader, 8);
        entities.ExistingCashReceiptBalanceAdjustment.Populated = true;
        entities.ExistingCashReceiptRlnRsn.Populated = true;
      });
  }

  private bool ReadCashReceiptCashReceiptType()
  {
    entities.ExistingCashReceipt.Populated = false;
    entities.ExistingCashReceiptType.Populated = false;

    return Read("ReadCashReceiptCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.ExistingCashReceipt.CheckType =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceipt.CheckNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceipt.CheckDate = db.GetNullableDate(reader, 8);
        entities.ExistingCashReceipt.ReceivedDate = db.GetDate(reader, 9);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingCashReceipt.PayorOrganization =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceipt.PayorFirstName =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceipt.PayorMiddleName =
          db.GetNullableString(reader, 13);
        entities.ExistingCashReceipt.PayorLastName =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 17);
        entities.ExistingCashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 18);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 19);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.ExistingCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 21);
        entities.ExistingCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 22);
        entities.ExistingCashReceipt.CashDue =
          db.GetNullableDecimal(reader, 23);
        entities.ExistingCashReceipt.TotalCashFeeAmount =
          db.GetNullableDecimal(reader, 24);
        entities.ExistingCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 25);
        entities.ExistingCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 26);
        entities.ExistingCashReceipt.Note = db.GetNullableString(reader, 27);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 28);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.ExistingCashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.NewCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsInterface.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.NewCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NewCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
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
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);

    var receiptAmount = import.CashReceipt.ReceiptAmount;
    var receiptDate = import.CashReceipt.ReceiptDate;
    var checkNumber = import.CashReceipt.CheckNumber ?? "";
    var checkDate = import.CashReceipt.CheckDate;
    var receivedDate = import.CashReceipt.ReceivedDate;
    var depositReleaseDate = import.CashReceipt.DepositReleaseDate;
    var payorOrganization = import.CashReceipt.PayorOrganization ?? "";
    var payorFirstName = import.CashReceipt.PayorFirstName ?? "";
    var payorMiddleName = import.CashReceipt.PayorMiddleName ?? "";
    var payorLastName = import.CashReceipt.PayorLastName ?? "";
    var balancedTimestamp = import.CashReceipt.BalancedTimestamp;
    var cashBalanceAmt = import.CashReceipt.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = import.CashReceipt.CashBalanceReason ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var note = import.CashReceipt.Note ?? "";

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.ExistingCashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDate(command, "receiptDate", receiptDate);
        db.SetNullableString(command, "checkNumber", checkNumber);
        db.SetNullableDate(command, "checkDate", checkDate);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableString(command, "payorOrganization", payorOrganization);
        db.SetNullableString(command, "payorFirstName", payorFirstName);
        db.SetNullableString(command, "payorMiddleName", payorMiddleName);
        db.SetNullableString(command, "payorLastName", payorLastName);
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "note", note);
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
      });

    entities.ExistingCashReceipt.ReceiptAmount = receiptAmount;
    entities.ExistingCashReceipt.ReceiptDate = receiptDate;
    entities.ExistingCashReceipt.CheckNumber = checkNumber;
    entities.ExistingCashReceipt.CheckDate = checkDate;
    entities.ExistingCashReceipt.ReceivedDate = receivedDate;
    entities.ExistingCashReceipt.DepositReleaseDate = depositReleaseDate;
    entities.ExistingCashReceipt.PayorOrganization = payorOrganization;
    entities.ExistingCashReceipt.PayorFirstName = payorFirstName;
    entities.ExistingCashReceipt.PayorMiddleName = payorMiddleName;
    entities.ExistingCashReceipt.PayorLastName = payorLastName;
    entities.ExistingCashReceipt.BalancedTimestamp = balancedTimestamp;
    entities.ExistingCashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.ExistingCashReceipt.CashBalanceReason = cashBalanceReason;
    entities.ExistingCashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCashReceipt.Note = note;
    entities.ExistingCashReceipt.Populated = true;
  }

  private void UpdateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ActiveCashReceiptStatusHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.ActiveCashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ActiveCashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ActiveCashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ActiveCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.ActiveCashReceiptStatusHistory.Populated = true;
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

    private CashReceipt cashReceipt;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of HardcodedCrsReceipted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsReceipted")]
    public CashReceiptStatus HardcodedCrsReceipted
    {
      get => hardcodedCrsReceipted ??= new();
      set => hardcodedCrsReceipted = value;
    }

    /// <summary>
    /// A value of HardcodedCrsInterface.
    /// </summary>
    [JsonPropertyName("hardcodedCrsInterface")]
    public CashReceiptStatus HardcodedCrsInterface
    {
      get => hardcodedCrsInterface ??= new();
      set => hardcodedCrsInterface = value;
    }

    /// <summary>
    /// A value of HardcodedCrtEft.
    /// </summary>
    [JsonPropertyName("hardcodedCrtEft")]
    public CashReceiptType HardcodedCrtEft
    {
      get => hardcodedCrtEft ??= new();
      set => hardcodedCrtEft = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private CashReceipt cashReceipt;
    private CashReceiptStatus hardcodedCrsReceipted;
    private CashReceiptStatus hardcodedCrsInterface;
    private CashReceiptType hardcodedCrtEft;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("existingCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment ExistingCashReceiptBalanceAdjustment
    {
      get => existingCashReceiptBalanceAdjustment ??= new();
      set => existingCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("existingCashReceiptRlnRsn")]
    public CashReceiptRlnRsn ExistingCashReceiptRlnRsn
    {
      get => existingCashReceiptRlnRsn ??= new();
      set => existingCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of NewCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("newCashReceiptStatusHistory")]
    public CashReceiptStatusHistory NewCashReceiptStatusHistory
    {
      get => newCashReceiptStatusHistory ??= new();
      set => newCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of NewCashReceiptAudit.
    /// </summary>
    [JsonPropertyName("newCashReceiptAudit")]
    public CashReceiptAudit NewCashReceiptAudit
    {
      get => newCashReceiptAudit ??= new();
      set => newCashReceiptAudit = value;
    }

    /// <summary>
    /// A value of NewCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("newCashReceiptStatus")]
    public CashReceiptStatus NewCashReceiptStatus
    {
      get => newCashReceiptStatus ??= new();
      set => newCashReceiptStatus = value;
    }

    private CashReceipt existingCashReceipt;
    private CashReceiptType existingCashReceiptType;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private CashReceiptBalanceAdjustment existingCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn existingCashReceiptRlnRsn;
    private CashReceiptStatusHistory newCashReceiptStatusHistory;
    private CashReceiptAudit newCashReceiptAudit;
    private CashReceiptStatus newCashReceiptStatus;
  }
#endregion
}
