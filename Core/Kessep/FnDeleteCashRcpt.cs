// Program: FN_DELETE_CASH_RCPT, ID: 371721490, model: 746.
// Short name: SWE00395
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DELETE_CASH_RCPT.
/// </para>
/// <para>
/// RESP: CASHMGMT									  This action block allows for the deletion of a cash
/// receipt.
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteCashRcpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_CASH_RCPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteCashRcpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteCashRcpt.
  /// </summary>
  public FnDeleteCashRcpt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // 04/29/97     JeHoward	Current date fix.
    // ??/??/??     ???????	Removed logic that prevented deletion of Non-CSE
    // 			cash receipts (problem rpt # 26549
    // 09/17/98     J Katz	Add delete validation logic.
    // 			Delete unused views.
    // 02/27/99     J Katz	Add edit to prevent cash receipt from being
    // 			deleted if Cash Receipt Balance Adjustments
    // 			exist for the receipt to be deleted.
    // 06/07/99  J. Katz 	Analyzed READ statements and changed read
    // 			property to Select Only where appropriate.
    // 06/21/99  J. Katz	Add logic to support new Cash Receipt Rln
    // 			Rsn codes.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.CurrentTs.Timestamp = Now();
    export.CashReceiptStatusHistory.ReasonText =
      import.CashReceiptStatusHistory.ReasonText;
    UseCabSetMaximumDiscontinueDate();

    // *****   hardcode area  ******
    UseFnHardcodedCashReceipting();

    // ---------------------------------------------------------
    // The following Cash Receipt Types are not included in the
    // Hardcoded Cash Receipting CAB.  These local views will be
    // matched in the USE statement above when the CAB is
    // updated.         JLK  09/17/98
    // ---------------------------------------------------------
    local.ToBeHardcodedCrtCsenet.SystemGeneratedIdentifier = 8;
    local.HardcodedCrtRdirPmt.SystemGeneratedIdentifier = 11;
    local.ToBeHardcodedCrtManint.SystemGeneratedIdentifier = 12;
    local.ToBeHardcodedCrtRecap.SystemGeneratedIdentifier = 13;

    // *********************************************************
    // Read current Cash Receipt information for the Cash Receipt
    // that is to be deleted.
    // *********************************************************
    if (ReadCashReceiptCashReceiptTypeCashReceiptSourceType())
    {
      ReadCashReceiptStatusHistoryCashReceiptStatus();

      if (!IsEmpty(entities.CashReceiptStatusHistory.CreatedBy))
      {
        // ----------------------------------
        // Active status record was found.
        // Fall thru to validation logic.
        // ----------------------------------
      }
      else
      {
        ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // *************************************************
    // Delete Validation Edits
    // JLK  09/17/98
    // *************************************************
    // ------------------------------------------------
    // Cannot delete receipts already deleted!
    // ------------------------------------------------
    if (entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier == local
      .HardcodedDelete.SystemGeneratedIdentifier)
    {
      export.CashReceiptStatusHistory.Assign(entities.CashReceiptStatusHistory);
      ExitState = "FN0000_CASH_RECEIPT_ALREADY_DEL";

      return;
    }

    // ------------------------------------------------
    // Cannot delete a Cash Receipt if it has Details!
    // JLK    09/28/98
    // ------------------------------------------------
    ReadCashReceiptDetail();

    if (local.CrDetails.Count > 0)
    {
      ExitState = "FN0304_CR_HAS_DETAILS_CANNOT_DEL";

      return;
    }

    // ------------------------------------------------
    // Validation Edits Based on Cash Receipt Category
    // of Cash or Non-cash.
    // ------------------------------------------------
    if (AsChar(entities.CashReceiptType.CategoryIndicator) == AsChar
      (local.HardcodedCrtCashCat.CategoryIndicator))
    {
      if (entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedRecorded.SystemGeneratedIdentifier)
      {
        // --------------------------------------------------------
        // Can only delete a Cash Receipt with a Source Type
        // Interface Indicator of Yes if the Cash Receipt Type is
        // MANINT.
        // --------------------------------------------------------
        if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) == 'Y'
          && entities.CashReceiptType.SystemGeneratedIdentifier != local
          .ToBeHardcodedCrtManint.SystemGeneratedIdentifier && Lt
          (local.Null1.Date, entities.CashReceiptEvent.SourceCreationDate))
        {
          ExitState = "FN0000_CR_FOR_INTF_CANNOT_BE_DEL";

          return;
        }

        // --------------------------------------------------------
        // Can only delete a CASH type Cash Receipt in REC status
        // if the receipt date is the same as the current date.
        // --------------------------------------------------------
        if (Equal(entities.CashReceipt.ReceiptDate, local.Current.Date))
        {
          // ----------------------------------------
          // OK to delete.
          // ----------------------------------------
        }
        else
        {
          ExitState = "FN0303_CR_CANNOT_BE_DELETED";

          return;
        }
      }
      else if (entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedIntf.SystemGeneratedIdentifier)
      {
        // --------------------------------------------------------
        // Can only delete a  Cash Receipt in INTF status when
        // the cash receipt type is MANINT and the receipt date
        // is the same as the current date.
        // --------------------------------------------------------
        if (entities.CashReceiptType.SystemGeneratedIdentifier == local
          .ToBeHardcodedCrtManint.SystemGeneratedIdentifier)
        {
          if (Equal(entities.CashReceipt.ReceiptDate, local.Current.Date))
          {
            // ----------------------------------------
            // OK to delete.
            // ----------------------------------------
          }
          else
          {
            ExitState = "FN0303_CR_CANNOT_BE_DELETED";

            return;
          }
        }
        else
        {
          ExitState = "FN0107_CASH_RCPT_STA_INV_4_RQ";

          return;
        }
      }
      else
      {
        ExitState = "FN0107_CASH_RCPT_STA_INV_4_RQ";

        return;
      }
    }
    else if (AsChar(entities.CashReceiptType.CategoryIndicator) == AsChar
      (local.HardcodedCrtNonCashCat.CategoryIndicator))
    {
      // --------------------------------------------------------
      // Can only delete a non-cash type Cash Receipt in REC
      // status.
      // --------------------------------------------------------
      if (entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedRecorded.SystemGeneratedIdentifier)
      {
        // --------------------------------------------------------
        // Can only delete a Cash Receipt with a non-cash type code
        // of CSENet, RDIR PMT, or RECAP in REC status when the
        // receipt date is the same as the current date.
        // --------------------------------------------------------
        if (entities.CashReceiptType.SystemGeneratedIdentifier == local
          .ToBeHardcodedCrtCsenet.SystemGeneratedIdentifier || entities
          .CashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedCrtRdirPmt.SystemGeneratedIdentifier || entities
          .CashReceiptType.SystemGeneratedIdentifier == local
          .ToBeHardcodedCrtRecap.SystemGeneratedIdentifier)
        {
          if (Equal(entities.CashReceipt.ReceiptDate, local.Current.Date))
          {
            // --------------------------------------------------------
            // No details exist for this receipt and receipt date is same
            // as current date.  OK to delete.
            // --------------------------------------------------------
          }
          else
          {
            ExitState = "FN0303_CR_CANNOT_BE_DELETED";

            return;
          }
        }
        else
        {
          // -------------------------------------------------------
          // Non-Cash Receipt is in receipted status and no details
          // exist.   No date restrictions apply.
          // Okay to delete!
          // -------------------------------------------------------
        }
      }
      else
      {
        ExitState = "FN0107_CASH_RCPT_STA_INV_4_RQ";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_INVALID_CR_TYPE_CAT_IND";

      return;
    }

    // --------------------------------------------------------
    // Can only delete a Cash Receipt with a Source Type
    // Interface Indicator of Yes if the receipt does not
    // participate in receipt amount adjustments.
    // JLK  02/27/99
    // Added two new Cash Receipt Rln Rsn codes for receipt
    // amount adjustments -- PROCCSTFEE and NETINTFERR.
    // JLK  06/21/99
    // --------------------------------------------------------
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn();

    if (local.RcptAmtAdj.Count > 0)
    {
      ExitState = "FN0000_CR_ADJ_EXIST_CANNOT_DEL";

      return;
    }

    // **********************************************************
    // Perform Delete actions which include end dating the current
    // Cash Receipt Status History record and creating a new
    // Status History record with a Status Code of DEL.
    // JLK    09/17/98
    // **********************************************************
    // ----------------------------------------------
    // Read Cash Receipt Status for DEL record.
    // Read Cash Receipt Delete Rason Code.
    // ----------------------------------------------
    if (ReadCashReceiptStatus())
    {
      // ok, go on
    }
    else
    {
      ExitState = "FN0108_CASH_RCPT_STAT_NF";

      return;
    }

    if (ReadCashReceiptDeleteReason())
    {
      // ok, go on
    }
    else
    {
      ExitState = "FN0035_CASH_RCPT_DEL_RSN_NF";

      return;
    }

    // ----------------------------------------------
    // Discontinue Current Status History (04/03/96)
    // ----------------------------------------------
    try
    {
      UpdateCashReceiptStatusHistory();

      // ok. Continue
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0110_CASH_RCPT_STAT_NU_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0111_CASH_RCPT_STAT_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // --------------------------------
    // Add new Status History
    // --------------------------------
    try
    {
      CreateCashReceiptStatusHistory();
      export.CashReceiptStatusHistory.Assign(entities.CashReceiptStatusHistory);
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

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedIntf.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier;
    local.HardcodedDelete.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrtCashCat.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
    local.HardcodedCrtNonCashCat.CategoryIndicator =
      useExport.CrtCategory.CrtCatNonCash.CategoryIndicator;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var crsIdentifier = entities.New1.SystemGeneratedIdentifier;
    var createdTimestamp = local.CurrentTs.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;
    var reasonText = import.CashReceiptStatusHistory.ReasonText ?? "";
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

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIIdentifier", entities.CashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier", entities.CashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier", entities.CashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        local.RcptAmtAdj.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceiptCashReceiptTypeCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptCashReceiptTypeCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtypeId",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 3);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 4);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 7);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptDeleteReason()
  {
    entities.CashReceiptDeleteReason.Populated = false;

    return Read("ReadCashReceiptDeleteReason",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptDeleteReason.Code);
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

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

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
        local.CrDetails.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.New1.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedDelete.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.New1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.New1.EffectiveDate = db.GetDate(reader, 1);
        entities.New1.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.New1.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;
    entities.ExistingCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
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
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier =
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
        entities.CashReceiptStatusHistory.Populated = true;
        entities.ExistingCashReceiptStatus.Populated = true;
      });
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
    /// A value of CashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("cashReceiptDeleteReason")]
    public CashReceiptDeleteReason CashReceiptDeleteReason
    {
      get => cashReceiptDeleteReason ??= new();
      set => cashReceiptDeleteReason = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
    }

    private CashReceiptStatusHistory cashReceiptStatusHistory;
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
    /// A value of CurrentTs.
    /// </summary>
    [JsonPropertyName("currentTs")]
    public DateWorkArea CurrentTs
    {
      get => currentTs ??= new();
      set => currentTs = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of CrDetails.
    /// </summary>
    [JsonPropertyName("crDetails")]
    public Common CrDetails
    {
      get => crDetails ??= new();
      set => crDetails = value;
    }

    /// <summary>
    /// A value of RcptAmtAdj.
    /// </summary>
    [JsonPropertyName("rcptAmtAdj")]
    public Common RcptAmtAdj
    {
      get => rcptAmtAdj ??= new();
      set => rcptAmtAdj = value;
    }

    /// <summary>
    /// A value of HardcodedRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedRecorded")]
    public CashReceiptStatus HardcodedRecorded
    {
      get => hardcodedRecorded ??= new();
      set => hardcodedRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedIntf.
    /// </summary>
    [JsonPropertyName("hardcodedIntf")]
    public CashReceiptStatus HardcodedIntf
    {
      get => hardcodedIntf ??= new();
      set => hardcodedIntf = value;
    }

    /// <summary>
    /// A value of HardcodedDelete.
    /// </summary>
    [JsonPropertyName("hardcodedDelete")]
    public CashReceiptStatus HardcodedDelete
    {
      get => hardcodedDelete ??= new();
      set => hardcodedDelete = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCashCat.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCashCat")]
    public CashReceiptType HardcodedCrtCashCat
    {
      get => hardcodedCrtCashCat ??= new();
      set => hardcodedCrtCashCat = value;
    }

    /// <summary>
    /// A value of HardcodedCrtNonCashCat.
    /// </summary>
    [JsonPropertyName("hardcodedCrtNonCashCat")]
    public CashReceiptType HardcodedCrtNonCashCat
    {
      get => hardcodedCrtNonCashCat ??= new();
      set => hardcodedCrtNonCashCat = value;
    }

    /// <summary>
    /// A value of ToBeHardcodedCrtManint.
    /// </summary>
    [JsonPropertyName("toBeHardcodedCrtManint")]
    public CashReceiptType ToBeHardcodedCrtManint
    {
      get => toBeHardcodedCrtManint ??= new();
      set => toBeHardcodedCrtManint = value;
    }

    /// <summary>
    /// A value of ToBeHardcodedCrtCsenet.
    /// </summary>
    [JsonPropertyName("toBeHardcodedCrtCsenet")]
    public CashReceiptType ToBeHardcodedCrtCsenet
    {
      get => toBeHardcodedCrtCsenet ??= new();
      set => toBeHardcodedCrtCsenet = value;
    }

    /// <summary>
    /// A value of HardcodedCrtRdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtRdirPmt")]
    public CashReceiptType HardcodedCrtRdirPmt
    {
      get => hardcodedCrtRdirPmt ??= new();
      set => hardcodedCrtRdirPmt = value;
    }

    /// <summary>
    /// A value of ToBeHardcodedCrtRecap.
    /// </summary>
    [JsonPropertyName("toBeHardcodedCrtRecap")]
    public CashReceiptType ToBeHardcodedCrtRecap
    {
      get => toBeHardcodedCrtRecap ??= new();
      set => toBeHardcodedCrtRecap = value;
    }

    private DateWorkArea current;
    private DateWorkArea currentTs;
    private DateWorkArea null1;
    private DateWorkArea max;
    private Common crDetails;
    private Common rcptAmtAdj;
    private CashReceiptStatus hardcodedRecorded;
    private CashReceiptStatus hardcodedIntf;
    private CashReceiptStatus hardcodedDelete;
    private CashReceiptType hardcodedCrtCashCat;
    private CashReceiptType hardcodedCrtNonCashCat;
    private CashReceiptType toBeHardcodedCrtManint;
    private CashReceiptType toBeHardcodedCrtCsenet;
    private CashReceiptType hardcodedCrtRdirPmt;
    private CashReceiptType toBeHardcodedCrtRecap;
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
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatus")]
    public CashReceiptStatus ExistingCashReceiptStatus
    {
      get => existingCashReceiptStatus ??= new();
      set => existingCashReceiptStatus = value;
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
    public CashReceiptStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus existingCashReceiptStatus;
    private CashReceiptBalanceAdjustment existingCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn existingCashReceiptRlnRsn;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptStatus new1;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
  }
#endregion
}
