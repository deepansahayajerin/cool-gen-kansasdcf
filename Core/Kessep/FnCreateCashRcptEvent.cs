// Program: FN_CREATE_CASH_RCPT_EVENT, ID: 372430609, model: 746.
// Short name: SWE00345
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RCPT_EVENT.
/// </para>
/// <para>
/// RESP:  FINANCE
/// This cab will create a cash receipt event for a supplied cash receipt source
/// type.
/// Used primarily by the batch interface processes such as court interface and 
/// setoffs.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashRcptEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RCPT_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashRcptEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashRcptEvent.
  /// </summary>
  public FnCreateCashRcptEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!import.Persistent.Populated)
    {
      if (ReadCashReceiptSourceType())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

        return;
      }
    }

    do
    {
      try
      {
        CreateCashReceiptEvent();
        ++export.ImportNumberOfUpdates.Count;
        export.CashReceiptEvent.Assign(entities.CashReceiptEvent);

        // ****  Continue
        local.RetryCount.Count = 6;
        ExitState = "ACO_NN0000_ALL_OK";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ++local.RetryCount.Count;
            ExitState = "FN0076_CASH_RCPT_EVENT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0080_CASH_RCPT_EVENT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(local.RetryCount.Count < 5);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCashReceiptEvent()
  {
    var cstIdentifier = import.Persistent.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var receivedDate = import.CashReceiptEvent.ReceivedDate;
    var sourceCreationDate = import.CashReceiptEvent.SourceCreationDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var totalNonCashTransactionCount =
      import.CashReceiptEvent.TotalNonCashTransactionCount.GetValueOrDefault();
    var totalAdjustmentCount =
      import.CashReceiptEvent.TotalAdjustmentCount.GetValueOrDefault();
    var totalCashFeeAmt =
      import.CashReceiptEvent.TotalCashFeeAmt.GetValueOrDefault();
    var totalNonCashFeeAmt =
      import.CashReceiptEvent.TotalNonCashFeeAmt.GetValueOrDefault();
    var anticipatedCheckAmt =
      import.CashReceiptEvent.AnticipatedCheckAmt.GetValueOrDefault();
    var totalCashAmt = import.CashReceiptEvent.TotalCashAmt.GetValueOrDefault();
    var totalCashTransactionCount =
      import.CashReceiptEvent.TotalCashTransactionCount.GetValueOrDefault();
    var totalNonCashAmt =
      import.CashReceiptEvent.TotalNonCashAmt.GetValueOrDefault();

    entities.CashReceiptEvent.Populated = false;
    Update("CreateCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "creventId", systemGeneratedIdentifier);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "sourceCreationDt", sourceCreationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableInt32(
          command, "totNonCshtrnCnt", totalNonCashTransactionCount);
        db.SetNullableInt32(command, "totAdjustmentCnt", totalAdjustmentCount);
        db.SetNullableDecimal(command, "totCashFeeAmt", totalCashFeeAmt);
        db.SetNullableDecimal(command, "totNoncshFeeAmt", totalNonCashFeeAmt);
        db.SetNullableDecimal(command, "anticCheckAmt", anticipatedCheckAmt);
        db.SetNullableDecimal(command, "totalCashAmt", totalCashAmt);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableDecimal(command, "totNonCashAmt", totalNonCashAmt);
      });

    entities.CashReceiptEvent.CstIdentifier = cstIdentifier;
    entities.CashReceiptEvent.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptEvent.ReceivedDate = receivedDate;
    entities.CashReceiptEvent.SourceCreationDate = sourceCreationDate;
    entities.CashReceiptEvent.CreatedBy = createdBy;
    entities.CashReceiptEvent.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptEvent.LastUpdatedBy = "";
    entities.CashReceiptEvent.LastUpdatedTmst = null;
    entities.CashReceiptEvent.TotalNonCashTransactionCount =
      totalNonCashTransactionCount;
    entities.CashReceiptEvent.TotalAdjustmentCount = totalAdjustmentCount;
    entities.CashReceiptEvent.TotalCashFeeAmt = totalCashFeeAmt;
    entities.CashReceiptEvent.TotalNonCashFeeAmt = totalNonCashFeeAmt;
    entities.CashReceiptEvent.AnticipatedCheckAmt = anticipatedCheckAmt;
    entities.CashReceiptEvent.TotalCashAmt = totalCashAmt;
    entities.CashReceiptEvent.TotalCashTransactionCount =
      totalCashTransactionCount;
    entities.CashReceiptEvent.TotalNonCashAmt = totalNonCashAmt;
    entities.CashReceiptEvent.Populated = true;
  }

  private bool ReadCashReceiptSourceType()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.Persistent.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        import.Persistent.Populated = true;
      });
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptSourceType Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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

    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType persistent;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private CashReceiptEvent cashReceiptEvent;
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
    /// A value of RetryCount.
    /// </summary>
    [JsonPropertyName("retryCount")]
    public Common RetryCount
    {
      get => retryCount ??= new();
      set => retryCount = value;
    }

    private Common retryCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private CashReceiptEvent cashReceiptEvent;
  }
#endregion
}
