// Program: FN_PROCESS_CT_INT_TOTALS_RECORD, ID: 372565698, model: 746.
// Short name: SWE00522
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_PROCESS_CT_INT_TOTALS_RECORD.
/// </para>
/// <para>
/// resp: finclmgmnt
/// This cab is used by the court interface program to process the totals record
/// (type=9) that is sent as the trailer on the file.
/// </para>
/// </summary>
[Serializable]
public partial class FnProcessCtIntTotalsRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_CT_INT_TOTALS_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessCtIntTotalsRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessCtIntTotalsRecord.
  /// </summary>
  public FnProcessCtIntTotalsRecord(IContext context, Import import,
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
    // *****************************************************************
    // RICH GALICHON - 2/99
    // Unit test and fixes.
    // 04/10/99	J. Katz		Modified logic to update the
    // Cash Receipt total values based on the court interface input
    // file.  Also modified code to eliminate all processing for court
    // disbursed non-cash receipts.
    // *****************************************************************
    // ------------------------------------------------------------------
    // If the cash receipt for this court interface file has no details,
    // delete the Cash Receipt and the Cash Receipt Event.
    // ------------------------------------------------------------------
    if (ReadCashReceiptCashReceiptEvent())
    {
      ReadCashReceiptDetail();

      if (!entities.KeyCashReceiptDetail.Populated)
      {
        DeleteCashReceipt();
        DeleteCashReceiptEvent();
        export.IsCashReceiptDeleted.Flag = "Y";
        export.ImportNumberOfUpdates.Count += 2;
      }
      else
      {
        ++local.ValidCashReceipts.Count;
      }
    }
    else
    {
      ExitState = "FN0086_CASH_RCPT_NF_RB";

      return;
    }

    // ------------------------------------------------------------------
    // Update the Cash Receipt Event with the Input Total Record.
    // This statement assumes that the Received Date and the
    // Source Creation Date are imported from the PrAD.
    // ------------------------------------------------------------------
    if (AsChar(export.IsCashReceiptDeleted.Flag) != 'Y')
    {
      MoveCashReceiptEvent1(import.CashReceiptEvent, local.CashReceiptEvent);
      MoveCashReceiptEvent2(import.TotalRecord.Total, local.CashReceiptEvent);
      UseFnUpdateCashRcptEvent();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // ------------------------------------------------------------------
      // Update the Cash Receipt with the interface totals and calculate
      // the balance for this court interface.
      // First populate the local view with the totals from the input file.
      // ------------------------------------------------------------------
      MoveCashReceipt(import.Check, local.CashReceipt);
      local.CashReceipt.TotalCashTransactionAmount =
        import.TotalRecord.Total.TotalCashAmt.GetValueOrDefault();
      local.CashReceipt.TotalCashTransactionCount =
        import.TotalRecord.Total.TotalCashTransactionCount.GetValueOrDefault();
      local.CashReceipt.TotalNoncashTransactionAmount =
        import.TotalRecord.Total.TotalNonCashAmt.GetValueOrDefault();
      local.CashReceipt.TotalNoncashTransactionCount =
        import.TotalRecord.Total.TotalNonCashTransactionCount.
          GetValueOrDefault();
      local.CashReceipt.TotalNonCashFeeAmount =
        import.TotalRecord.Total.TotalCashFeeAmt.GetValueOrDefault();
      local.CashReceipt.TotalDetailAdjustmentCount =
        import.TotalRecord.Total.TotalAdjustmentCount.GetValueOrDefault();
      local.CashReceipt.CashDue =
        import.TotalRecord.Total.AnticipatedCheckAmt.GetValueOrDefault();

      // ------------------------------------------------------------------
      // Calculate the Balance Amount as the cash due less the receipt
      // amount.  This value will generally be positive unless the court
      // transmitted more adjustment dollars than payment collections.
      // If the balance amount is positive indicating an underpayment,
      // the Balance Reason is UNDER;
      // If the balance amount is negative indicating an overpayment,
      // the Balance Reason is OVER.
      // Otherwise, this court interface is in balance and the Balance
      // Reason is blank.    JLK  04/10/99
      // ------------------------------------------------------------------
      local.CashReceipt.CashBalanceAmt =
        local.CashReceipt.CashDue.GetValueOrDefault() - local
        .CashReceipt.ReceiptAmount;

      if (local.CashReceipt.CashBalanceAmt.GetValueOrDefault() > 0)
      {
        local.CashReceipt.CashBalanceReason = "UNDER";
      }
      else if (local.CashReceipt.CashBalanceAmt.GetValueOrDefault() < 0)
      {
        local.CashReceipt.CashBalanceReason = "OVER";
      }
      else
      {
        local.CashReceipt.CashBalanceReason = "";
      }

      // ------------------------------------------------------------------
      // Update the Cash Receipt
      // ------------------------------------------------------------------
      try
      {
        UpdateCashReceipt();
        ++export.ImportNumberOfUpdates.Count;
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

    // *****************************************************************
    // Compare the totals sent by the Court to the totals calculated
    // by the PrAD and report any discrepancies.
    // *****************************************************************
    if (import.TotalRecord.Total.TotalCashAmt.GetValueOrDefault() != import
      .Swefb610CashAmt.TotalCurrency || import
      .TotalRecord.Total.TotalCashTransactionCount.GetValueOrDefault() != import
      .Swefb610CashCount.Count || import
      .TotalRecord.Total.TotalNonCashAmt.GetValueOrDefault() != import
      .Swefb610NonCashAmt.TotalCurrency || import
      .TotalRecord.Total.TotalNonCashTransactionCount.GetValueOrDefault() != import
      .Swefb610NonCash.Count || import
      .TotalRecord.Total.TotalAdjustmentCount.GetValueOrDefault() != import
      .Swefb610AdjustmentCount.Count)
    {
      export.Errors.Index = export.Errors.Count;
      export.Errors.CheckSize();

      export.Errors.Update.ErrorsDetailProgramError.KeyInfo =
        TrimEnd(export.Errors.Item.ErrorsDetailProgramError.KeyInfo) + "System Calculated: Cash-Amt = " +
        NumberToString((long)import.Swefb610CashAmt.TotalCurrency, 15);
      export.Errors.Update.ErrorsDetailProgramError.KeyInfo =
        TrimEnd(export.Errors.Item.ErrorsDetailProgramError.KeyInfo) + " Cash-Count = " +
        NumberToString(import.Swefb610CashCount.Count, 15);
      export.Errors.Update.ErrorsDetailProgramError.KeyInfo =
        TrimEnd(export.Errors.Item.ErrorsDetailProgramError.KeyInfo) + " Adjustment Count = " +
        NumberToString(import.Swefb610AdjustmentCount.Count, 15);
      export.Errors.Update.ErrorsDetailProgramError.SystemGeneratedIdentifier =
        export.Errors.Index + 1;

      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.Errors.Update.ErrorsDetailProgramError.KeyInfo =
        TrimEnd(export.Errors.Item.ErrorsDetailProgramError.KeyInfo) + "     Total Record: Cash-Amt = " +
        NumberToString
        ((long)import.TotalRecord.Total.TotalCashAmt.GetValueOrDefault(), 15);
      export.Errors.Update.ErrorsDetailProgramError.KeyInfo =
        TrimEnd(export.Errors.Item.ErrorsDetailProgramError.KeyInfo) + " Cash-Count = " +
        NumberToString
        (import.TotalRecord.Total.TotalCashTransactionCount.GetValueOrDefault(),
        15);
      export.Errors.Update.ErrorsDetailProgramError.KeyInfo =
        TrimEnd(export.Errors.Item.ErrorsDetailProgramError.KeyInfo) + " Adjustment Count = " +
        NumberToString
        (import.TotalRecord.Total.TotalAdjustmentCount.GetValueOrDefault(), 15);
        
      export.Errors.Update.ErrorsDetailProgramError.SystemGeneratedIdentifier =
        export.Errors.Index + 1;
      ExitState = "FN0000_OUT_OF_BALANCE";
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.TotalNonCashFeeAmount = source.TotalNonCashFeeAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
    target.CashDue = source.CashDue;
  }

  private static void MoveCashReceiptEvent1(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
    target.SourceCreationDate = source.SourceCreationDate;
  }

  private static void MoveCashReceiptEvent2(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.TotalNonCashTransactionCount = source.TotalNonCashTransactionCount;
    target.TotalAdjustmentCount = source.TotalAdjustmentCount;
    target.TotalCashFeeAmt = source.TotalCashFeeAmt;
    target.TotalNonCashFeeAmt = source.TotalNonCashFeeAmt;
    target.AnticipatedCheckAmt = source.AnticipatedCheckAmt;
    target.TotalCashAmt = source.TotalCashAmt;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalNonCashAmt = source.TotalNonCashAmt;
  }

  private void UseFnUpdateCashRcptEvent()
  {
    var useImport = new FnUpdateCashRcptEvent.Import();
    var useExport = new FnUpdateCashRcptEvent.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.Assign(local.CashReceiptEvent);
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnUpdateCashRcptEvent.Execute, useImport, useExport);

    local.CashReceiptEvent.Assign(useExport.CashReceiptEvent);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void DeleteCashReceipt()
  {
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    bool exists;

    Update("DeleteCashReceipt#1",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier1", crvIdentifier);
        db.SetInt32(command, "cstIdentifier1", cstIdentifier);
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    exists = Read("DeleteCashReceipt#2",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier2", crvIdentifier);
        db.SetInt32(command, "cstIdentifier2", cstIdentifier);
      },
      null);

    if (!exists)
    {
      Update("DeleteCashReceipt#3",
        (db, command) =>
        {
          db.SetInt32(command, "crvIdentifier2", crvIdentifier);
          db.SetInt32(command, "cstIdentifier2", cstIdentifier);
        });
    }
  }

  private void DeleteCashReceiptEvent()
  {
    bool exists;

    exists = Read("DeleteCashReceiptEvent#1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier", entities.KeyCashReceiptEvent.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier",
          entities.KeyCashReceiptEvent.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_CASH_RECEIPT\".",
        "50001");
    }

    Update("DeleteCashReceiptEvent#2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier", entities.KeyCashReceiptEvent.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier",
          entities.KeyCashReceiptEvent.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCashReceiptCashReceiptEvent()
  {
    entities.KeyCashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.KeyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.KeyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 7);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 8);
        entities.CashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 9);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 10);
        entities.CashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 11);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 12);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 13);
        entities.KeyCashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.KeyCashReceiptDetail.Populated = false;

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
        entities.KeyCashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.KeyCashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.KeyCashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.KeyCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.KeyCashReceiptDetail.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var totalCashTransactionAmount =
      local.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault();
    var totalNoncashTransactionAmount =
      local.CashReceipt.TotalNoncashTransactionAmount.GetValueOrDefault();
    var totalCashTransactionCount =
      local.CashReceipt.TotalCashTransactionCount.GetValueOrDefault();
    var totalNoncashTransactionCount =
      local.CashReceipt.TotalNoncashTransactionCount.GetValueOrDefault();
    var totalDetailAdjustmentCount =
      local.CashReceipt.TotalDetailAdjustmentCount.GetValueOrDefault();
    var cashBalanceAmt = local.CashReceipt.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = local.CashReceipt.CashBalanceReason ?? "";
    var cashDue = local.CashReceipt.CashDue.GetValueOrDefault();
    var totalNonCashFeeAmount =
      local.CashReceipt.TotalNonCashFeeAmount.GetValueOrDefault();

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "totalCashTransac", totalCashTransactionAmount);
        db.SetNullableDecimal(
          command, "totNoncshTrnAmt", totalNoncashTransactionAmount);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableInt32(
          command, "totNocshTranCnt", totalNoncashTransactionCount);
        db.SetNullableInt32(
          command, "totDetailAdjCnt", totalDetailAdjustmentCount);
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableDecimal(command, "cashDue", cashDue);
        db.SetNullableDecimal(command, "totalNcFeeAmt", totalNonCashFeeAmount);
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.TotalCashTransactionAmount =
      totalCashTransactionAmount;
    entities.CashReceipt.TotalNoncashTransactionAmount =
      totalNoncashTransactionAmount;
    entities.CashReceipt.TotalCashTransactionCount = totalCashTransactionCount;
    entities.CashReceipt.TotalNoncashTransactionCount =
      totalNoncashTransactionCount;
    entities.CashReceipt.TotalDetailAdjustmentCount =
      totalDetailAdjustmentCount;
    entities.CashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.CashReceipt.CashBalanceReason = cashBalanceReason;
    entities.CashReceipt.CashDue = cashDue;
    entities.CashReceipt.TotalNonCashFeeAmount = totalNonCashFeeAmount;
    entities.CashReceipt.Populated = true;
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
    /// <summary>A TotalRecordGroup group.</summary>
    [Serializable]
    public class TotalRecordGroup
    {
      /// <summary>
      /// A value of Total.
      /// </summary>
      [JsonPropertyName("total")]
      public CashReceiptEvent Total
      {
        get => total ??= new();
        set => total = value;
      }

      private CashReceiptEvent total;
    }

    /// <summary>
    /// A value of Swefb610AdjustmentCount.
    /// </summary>
    [JsonPropertyName("swefb610AdjustmentCount")]
    public Common Swefb610AdjustmentCount
    {
      get => swefb610AdjustmentCount ??= new();
      set => swefb610AdjustmentCount = value;
    }

    /// <summary>
    /// A value of Swefb610NonCash.
    /// </summary>
    [JsonPropertyName("swefb610NonCash")]
    public Common Swefb610NonCash
    {
      get => swefb610NonCash ??= new();
      set => swefb610NonCash = value;
    }

    /// <summary>
    /// A value of Swefb610CashCount.
    /// </summary>
    [JsonPropertyName("swefb610CashCount")]
    public Common Swefb610CashCount
    {
      get => swefb610CashCount ??= new();
      set => swefb610CashCount = value;
    }

    /// <summary>
    /// A value of Swefb610NonCashAmt.
    /// </summary>
    [JsonPropertyName("swefb610NonCashAmt")]
    public Common Swefb610NonCashAmt
    {
      get => swefb610NonCashAmt ??= new();
      set => swefb610NonCashAmt = value;
    }

    /// <summary>
    /// A value of Swefb610CashAmt.
    /// </summary>
    [JsonPropertyName("swefb610CashAmt")]
    public Common Swefb610CashAmt
    {
      get => swefb610CashAmt ??= new();
      set => swefb610CashAmt = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public CashReceipt Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of ZdelTbdImportCtDisbursed.
    /// </summary>
    [JsonPropertyName("zdelTbdImportCtDisbursed")]
    public CashReceipt ZdelTbdImportCtDisbursed
    {
      get => zdelTbdImportCtDisbursed ??= new();
      set => zdelTbdImportCtDisbursed = value;
    }

    /// <summary>
    /// A value of SourceCreation.
    /// </summary>
    [JsonPropertyName("sourceCreation")]
    public DateWorkArea SourceCreation
    {
      get => sourceCreation ??= new();
      set => sourceCreation = value;
    }

    /// <summary>
    /// Gets a value of TotalRecord.
    /// </summary>
    [JsonPropertyName("totalRecord")]
    public TotalRecordGroup TotalRecord
    {
      get => totalRecord ?? (totalRecord = new());
      set => totalRecord = value;
    }

    private Common swefb610AdjustmentCount;
    private Common swefb610NonCash;
    private Common swefb610CashCount;
    private Common swefb610NonCashAmt;
    private Common swefb610CashAmt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt check;
    private CashReceipt zdelTbdImportCtDisbursed;
    private DateWorkArea sourceCreation;
    private TotalRecordGroup totalRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of ErrorsDetailProgramError.
      /// </summary>
      [JsonPropertyName("errorsDetailProgramError")]
      public ProgramError ErrorsDetailProgramError
      {
        get => errorsDetailProgramError ??= new();
        set => errorsDetailProgramError = value;
      }

      /// <summary>
      /// A value of ErrorsDetailStandard.
      /// </summary>
      [JsonPropertyName("errorsDetailStandard")]
      public Standard ErrorsDetailStandard
      {
        get => errorsDetailStandard ??= new();
        set => errorsDetailStandard = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private ProgramError errorsDetailProgramError;
      private Standard errorsDetailStandard;
    }

    /// <summary>
    /// A value of IsCashReceiptDeleted.
    /// </summary>
    [JsonPropertyName("isCashReceiptDeleted")]
    public Common IsCashReceiptDeleted
    {
      get => isCashReceiptDeleted ??= new();
      set => isCashReceiptDeleted = value;
    }

    /// <summary>
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private Common isCashReceiptDeleted;
    private Array<ErrorsGroup> errors;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ValidCashReceipts.
    /// </summary>
    [JsonPropertyName("validCashReceipts")]
    public Common ValidCashReceipts
    {
      get => validCashReceipts ??= new();
      set => validCashReceipts = value;
    }

    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private Common validCashReceipts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("keyCashReceiptSourceType")]
    public CashReceiptSourceType KeyCashReceiptSourceType
    {
      get => keyCashReceiptSourceType ??= new();
      set => keyCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of KeyCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("keyCashReceiptEvent")]
    public CashReceiptEvent KeyCashReceiptEvent
    {
      get => keyCashReceiptEvent ??= new();
      set => keyCashReceiptEvent = value;
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
    /// A value of KeyCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("keyCashReceiptDetail")]
    public CashReceiptDetail KeyCashReceiptDetail
    {
      get => keyCashReceiptDetail ??= new();
      set => keyCashReceiptDetail = value;
    }

    private CashReceiptSourceType keyCashReceiptSourceType;
    private CashReceiptEvent keyCashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail keyCashReceiptDetail;
  }
#endregion
}
