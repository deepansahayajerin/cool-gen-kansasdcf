// Program: FN_PROCESS_FDSO_INT_TOTAL_RECORD, ID: 372529958, model: 746.
// Short name: SWE01526
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCESS_FDSO_INT_TOTAL_RECORD.
/// </summary>
[Serializable]
public partial class FnProcessFdsoIntTotalRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_FDSO_INT_TOTAL_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessFdsoIntTotalRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessFdsoIntTotalRecord.
  /// </summary>
  public FnProcessFdsoIntTotalRecord(IContext context, Import import,
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
    // ************************************************************
    // 03/24/98	Siraj Konkader		ZDEL Cleanup
    // ************************************************************
    // 01/22/99        SWSRPDP	            	Check PROPER fields for
    // Out-of-Balance condition.
    // 01/26/99	SWSRPDP      		Change Totals to go to NON-Cash.
    // 09/11/99    H00073496  pphinney net amount  FIX to update Cash_Receipt 
    // and Cash_Receipt_Event correctly if Adjustment amount is GREATER than
    // Collection Amount.
    // ************************************************************
    for(export.Errors.Index = 0; export.Errors.Index < export.Errors.Count; ++
      export.Errors.Index)
    {
      if (!export.Errors.CheckSize())
      {
        break;
      }

      if (Equal(export.Errors.Item.ErrorsDetailStandard.Command, "ROLLBACK"))
      {
        return;
      }
    }

    export.Errors.CheckIndex();

    // *****
    // Update the Cash Receipt Event with the Input Total Record.  This 
    // statement assumes that the Received_Date and the Source_Creation_Date are
    // imported in the Import Cash_Receipt_Event entity view from the PrAD.
    // *****
    MoveCashReceiptEvent1(import.CashReceiptEvent, local.CashReceiptEvent);
    local.CashReceiptEvent.SourceCreationDate = Now().Date;

    // USE Values from Input file - NOT Calculated Values
    local.CashReceiptEvent.TotalCashTransactionCount =
      import.TotalRecord.Detail.TotalNoncashTransactionCount.
        GetValueOrDefault();
    local.CashReceiptEvent.TotalCashAmt =
      import.TotalRecord.Detail.ReceiptAmount;
    local.CashReceiptEvent.TotalAdjustmentCount =
      import.TotalRecord.DetailAdjCount.TotalAdjustmentCount.
        GetValueOrDefault();

    // 09/11/99    H00073496  pphinney net amount  FIX will be here?
    // * * * * * * * * * * * * * * * * *
    if (import.TotalRecord.Detail.ReceiptAmount - import
      .TotalRecord.DetailAdjAmt.TotalCurrency > 0)
    {
      local.CashReceiptEvent.AnticipatedCheckAmt =
        import.TotalRecord.NetAmount.TotalCurrency;
    }
    else
    {
      local.CashReceiptEvent.AnticipatedCheckAmt =
        import.TotalRecord.Detail.ReceiptAmount - import
        .TotalRecord.DetailAdjAmt.TotalCurrency;
    }

    // * * * * * * * * * * * * * * * * *
    if (ReadCashReceiptEvent())
    {
      UseFnUpdateCashRcptEvent();
    }
    else
    {
      ExitState = "FN0077_CASH_RCPT_EVENT_NF";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Update the Cash Receipt with the accumulated amounts and counts.
    // *****
    if (ReadCashReceipt())
    {
      ++export.ImportNumberOfReads.Count;

      // *****
      // Initialize the local view.
      // *****
      MoveCashReceipt1(local.Initialized, local.CashReceipt);

      // *****
      // If there is not at least one Cash Receipt Detail associated to this 
      // Cash Receipt, delete the Cash Receipt.
      // *****
      if (ReadCashReceiptDetail())
      {
        ++export.ImportNumberOfReads.Count;
      }

      if (!entities.KeyCashReceiptDetail.Populated)
      {
        DeleteCashReceipt();
        export.ImportNumberOfUpdates.Count = 0;

        return;
      }

      // *****
      // Set the attributes to be updated.
      // *****
      local.CashReceipt.Assign(entities.CashReceipt);

      // USE Values from Input file - NOT Calculated Values
      local.CashReceipt.TotalCashTransactionCount =
        import.TotalRecord.Detail.TotalNoncashTransactionCount.
          GetValueOrDefault();
      local.CashReceipt.TotalCashTransactionAmount =
        import.TotalRecord.Detail.ReceiptAmount;
      local.CashReceipt.TotalDetailAdjustmentCount =
        import.TotalRecord.DetailAdjCount.TotalAdjustmentCount.
          GetValueOrDefault();

      // * * * * * * * * * * * * * * * * *
      // 09/11/99    H00073496  pphinney net amount  FIX will be here?
      local.CashReceipt.CashDue = import.TotalRecord.Detail.ReceiptAmount - import
        .TotalRecord.DetailAdjAmt.TotalCurrency;

      // * * * * * * * * * * * * * * * * *
      // HAVE NO RECEIPT AMOUNT AT THIS TIME SO set TO cash_due amt.
      local.CashReceipt.CashBalanceAmt =
        local.CashReceipt.CashDue.GetValueOrDefault();

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

      try
      {
        UpdateCashReceipt();
        ++export.ImportNumberOfUpdates.Count;
        ++local.ValidCashReceipts.Count;

        // H00075922    Sept. 30, 1999
        MoveCashReceipt2(entities.CashReceipt, export.ImportCashReceipt);
        MoveCashReceipt3(local.CashReceipt, export.ImportCashReceipt);
        export.ImportCashReceipt.CreatedBy = global.UserId;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0088_CASH_RCPT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0090_CASH_RCPT_PV";

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
      ExitState = "FN0084_CASH_RCPT_NF";
    }
  }

  private static void MoveCashReceipt1(CashReceipt source, CashReceipt target)
  {
    target.TotalNonCashFeeAmount = source.TotalNonCashFeeAmount;
    target.TotalNoncashTransactionAmount = source.TotalNoncashTransactionAmount;
    target.TotalNoncashTransactionCount = source.TotalNoncashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
  }

  private static void MoveCashReceipt2(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalNoncashTransactionAmount = source.TotalNoncashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalNoncashTransactionCount = source.TotalNoncashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
    target.CashDue = source.CashDue;
  }

  private static void MoveCashReceipt3(CashReceipt source, CashReceipt target)
  {
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.TotalNoncashTransactionAmount = source.TotalNoncashTransactionAmount;
    target.TotalCashTransactionCount = source.TotalCashTransactionCount;
    target.TotalNoncashTransactionCount = source.TotalNoncashTransactionCount;
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
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
    target.SourceCreationDate = source.SourceCreationDate;
    target.TotalNonCashTransactionCount = source.TotalNonCashTransactionCount;
    target.TotalAdjustmentCount = source.TotalAdjustmentCount;
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
    MoveCashReceiptEvent2(local.CashReceiptEvent, useImport.CashReceiptEvent);
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

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
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
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
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
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
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

  private bool ReadCashReceiptEvent()
  {
    entities.Persistent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Persistent.CstIdentifier = db.GetInt32(reader, 0);
        entities.Persistent.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Persistent.ReceivedDate = db.GetDate(reader, 2);
        entities.Persistent.SourceCreationDate = db.GetNullableDate(reader, 3);
        entities.Persistent.CreatedBy = db.GetString(reader, 4);
        entities.Persistent.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Persistent.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Persistent.LastUpdatedTmst = db.GetNullableDateTime(reader, 7);
        entities.Persistent.TotalNonCashTransactionCount =
          db.GetNullableInt32(reader, 8);
        entities.Persistent.TotalAdjustmentCount =
          db.GetNullableInt32(reader, 9);
        entities.Persistent.TotalCashFeeAmt = db.GetNullableDecimal(reader, 10);
        entities.Persistent.TotalNonCashFeeAmt =
          db.GetNullableDecimal(reader, 11);
        entities.Persistent.AnticipatedCheckAmt =
          db.GetNullableDecimal(reader, 12);
        entities.Persistent.TotalCashAmt = db.GetNullableDecimal(reader, 13);
        entities.Persistent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 14);
        entities.Persistent.TotalNonCashAmt = db.GetNullableDecimal(reader, 15);
        entities.Persistent.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var totalCashTransactionAmount =
      local.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault();
    var totalCashTransactionCount =
      local.CashReceipt.TotalCashTransactionCount.GetValueOrDefault();
    var totalDetailAdjustmentCount =
      local.CashReceipt.TotalDetailAdjustmentCount.GetValueOrDefault();
    var cashBalanceAmt = local.CashReceipt.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = local.CashReceipt.CashBalanceReason ?? "";
    var cashDue = local.CashReceipt.CashDue.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "totalCashTransac", totalCashTransactionAmount);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableInt32(
          command, "totDetailAdjCnt", totalDetailAdjustmentCount);
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableDecimal(command, "cashDue", cashDue);
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

    entities.CashReceipt.TotalCashTransactionAmount =
      totalCashTransactionAmount;
    entities.CashReceipt.TotalCashTransactionCount = totalCashTransactionCount;
    entities.CashReceipt.TotalDetailAdjustmentCount =
      totalDetailAdjustmentCount;
    entities.CashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.CashReceipt.CashBalanceReason = cashBalanceReason;
    entities.CashReceipt.CashDue = cashDue;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
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
      /// A value of DetailAdjAmt.
      /// </summary>
      [JsonPropertyName("detailAdjAmt")]
      public Common DetailAdjAmt
      {
        get => detailAdjAmt ??= new();
        set => detailAdjAmt = value;
      }

      /// <summary>
      /// A value of DetailAdjCount.
      /// </summary>
      [JsonPropertyName("detailAdjCount")]
      public CashReceiptEvent DetailAdjCount
      {
        get => detailAdjCount ??= new();
        set => detailAdjCount = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CashReceipt Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of NetAmount.
      /// </summary>
      [JsonPropertyName("netAmount")]
      public Common NetAmount
      {
        get => netAmount ??= new();
        set => netAmount = value;
      }

      private Common detailAdjAmt;
      private CashReceiptEvent detailAdjCount;
      private CashReceipt detail;
      private Common netAmount;
    }

    /// <summary>
    /// A value of Swefb612AdjError.
    /// </summary>
    [JsonPropertyName("swefb612AdjError")]
    public Common Swefb612AdjError
    {
      get => swefb612AdjError ??= new();
      set => swefb612AdjError = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionCount.
    /// </summary>
    [JsonPropertyName("swefb612CollectionCount")]
    public Common Swefb612CollectionCount
    {
      get => swefb612CollectionCount ??= new();
      set => swefb612CollectionCount = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustmentCount.
    /// </summary>
    [JsonPropertyName("swefb612AdjustmentCount")]
    public Common Swefb612AdjustmentCount
    {
      get => swefb612AdjustmentCount ??= new();
      set => swefb612AdjustmentCount = value;
    }

    /// <summary>
    /// A value of Swefb612NetAmt.
    /// </summary>
    [JsonPropertyName("swefb612NetAmt")]
    public Common Swefb612NetAmt
    {
      get => swefb612NetAmt ??= new();
      set => swefb612NetAmt = value;
    }

    /// <summary>
    /// A value of Swefb612AdjustmentAmt.
    /// </summary>
    [JsonPropertyName("swefb612AdjustmentAmt")]
    public Common Swefb612AdjustmentAmt
    {
      get => swefb612AdjustmentAmt ??= new();
      set => swefb612AdjustmentAmt = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionAmt.
    /// </summary>
    [JsonPropertyName("swefb612CollectionAmt")]
    public Common Swefb612CollectionAmt
    {
      get => swefb612CollectionAmt ??= new();
      set => swefb612CollectionAmt = value;
    }

    /// <summary>
    /// A value of AdjustmentCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("adjustmentCashReceiptEvent")]
    public CashReceiptEvent AdjustmentCashReceiptEvent
    {
      get => adjustmentCashReceiptEvent ??= new();
      set => adjustmentCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of AdjustmentCommon.
    /// </summary>
    [JsonPropertyName("adjustmentCommon")]
    public Common AdjustmentCommon
    {
      get => adjustmentCommon ??= new();
      set => adjustmentCommon = value;
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

    /// <summary>
    /// A value of Interfund.
    /// </summary>
    [JsonPropertyName("interfund")]
    public CashReceipt Interfund
    {
      get => interfund ??= new();
      set => interfund = value;
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

    private Common swefb612AdjError;
    private Common swefb612CollectionCount;
    private Common swefb612AdjustmentCount;
    private Common swefb612NetAmt;
    private Common swefb612AdjustmentAmt;
    private Common swefb612CollectionAmt;
    private CashReceiptEvent adjustmentCashReceiptEvent;
    private Common adjustmentCommon;
    private TotalRecordGroup totalRecord;
    private CashReceipt interfund;
    private DateWorkArea sourceCreation;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
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
      public const int Capacity = 1;

      private ProgramError errorsDetailProgramError;
      private Standard errorsDetailStandard;
    }

    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    /// <summary>
    /// A value of ImportNet.
    /// </summary>
    [JsonPropertyName("importNet")]
    public Common ImportNet
    {
      get => importNet ??= new();
      set => importNet = value;
    }

    /// <summary>
    /// A value of ImportAdjustment.
    /// </summary>
    [JsonPropertyName("importAdjustment")]
    public Common ImportAdjustment
    {
      get => importAdjustment ??= new();
      set => importAdjustment = value;
    }

    /// <summary>
    /// A value of ImportCashReceipt.
    /// </summary>
    [JsonPropertyName("importCashReceipt")]
    public CashReceipt ImportCashReceipt
    {
      get => importCashReceipt ??= new();
      set => importCashReceipt = value;
    }

    /// <summary>
    /// A value of ImportCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("importCashReceiptDetail")]
    public CashReceiptDetail ImportCashReceiptDetail
    {
      get => importCashReceiptDetail ??= new();
      set => importCashReceiptDetail = value;
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

    private ProgramError programError;
    private Common importNet;
    private Common importAdjustment;
    private CashReceipt importCashReceipt;
    private CashReceiptDetail importCashReceiptDetail;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public ProgramError Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public CashReceipt Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private ProgramError temp;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt initialized;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptEvent Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of KeyCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("keyCashReceiptDetail")]
    public CashReceiptDetail KeyCashReceiptDetail
    {
      get => keyCashReceiptDetail ??= new();
      set => keyCashReceiptDetail = value;
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
    /// A value of KeyCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("keyCashReceiptSourceType")]
    public CashReceiptSourceType KeyCashReceiptSourceType
    {
      get => keyCashReceiptSourceType ??= new();
      set => keyCashReceiptSourceType = value;
    }

    private CashReceiptEvent persistent;
    private CashReceiptEvent keyCashReceiptEvent;
    private CashReceiptDetail keyCashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType keyCashReceiptSourceType;
  }
#endregion
}
