// Program: FN_UPDATE_CASH_RCPT_EVENT, ID: 372532868, model: 746.
// Short name: SWE00631
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_CASH_RCPT_EVENT.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block updates an occurrence of the Cash Receipt Event 
/// entity.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateCashRcptEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_CASH_RCPT_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateCashRcptEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateCashRcptEvent.
  /// </summary>
  public FnUpdateCashRcptEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    var condition = !import.Persistent.Populated;

    if (!condition)
    {
      // IS LOCKED expression is used.
      // Entity is considered to be locked during the call.
      condition = !true;
    }

    if (condition)
    {
      if (ReadCashReceiptEvent())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0077_CASH_RCPT_EVENT_NF";

        return;
      }
    }

    try
    {
      UpdateCashReceiptEvent();
      ++export.ImportNumberOfUpdates.Count;
      export.CashReceiptEvent.Assign(import.Persistent);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0079_CASH_RCPT_EVENT_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0080_CASH_RCPT_EVENT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadCashReceiptEvent()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.Persistent.CstIdentifier = db.GetInt32(reader, 0);
        import.Persistent.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        import.Persistent.ReceivedDate = db.GetDate(reader, 2);
        import.Persistent.SourceCreationDate = db.GetNullableDate(reader, 3);
        import.Persistent.CreatedBy = db.GetString(reader, 4);
        import.Persistent.CreatedTimestamp = db.GetDateTime(reader, 5);
        import.Persistent.LastUpdatedBy = db.GetNullableString(reader, 6);
        import.Persistent.LastUpdatedTmst = db.GetNullableDateTime(reader, 7);
        import.Persistent.TotalNonCashTransactionCount =
          db.GetNullableInt32(reader, 8);
        import.Persistent.TotalAdjustmentCount = db.GetNullableInt32(reader, 9);
        import.Persistent.TotalCashFeeAmt = db.GetNullableDecimal(reader, 10);
        import.Persistent.TotalNonCashFeeAmt =
          db.GetNullableDecimal(reader, 11);
        import.Persistent.AnticipatedCheckAmt =
          db.GetNullableDecimal(reader, 12);
        import.Persistent.TotalCashAmt = db.GetNullableDecimal(reader, 13);
        import.Persistent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 14);
        import.Persistent.TotalNonCashAmt = db.GetNullableDecimal(reader, 15);
        import.Persistent.Populated = true;
      });
  }

  private void UpdateCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var receivedDate = import.CashReceiptEvent.ReceivedDate;
    var sourceCreationDate = import.CashReceiptEvent.SourceCreationDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
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

    import.Persistent.Populated = false;
    Update("UpdateCashReceiptEvent",
      (db, command) =>
      {
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "sourceCreationDt", sourceCreationDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
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
        db.SetInt32(command, "cstIdentifier", import.Persistent.CstIdentifier);
        db.SetInt32(
          command, "creventId", import.Persistent.SystemGeneratedIdentifier);
      });

    import.Persistent.ReceivedDate = receivedDate;
    import.Persistent.SourceCreationDate = sourceCreationDate;
    import.Persistent.LastUpdatedBy = lastUpdatedBy;
    import.Persistent.LastUpdatedTmst = lastUpdatedTmst;
    import.Persistent.TotalNonCashTransactionCount =
      totalNonCashTransactionCount;
    import.Persistent.TotalAdjustmentCount = totalAdjustmentCount;
    import.Persistent.TotalCashFeeAmt = totalCashFeeAmt;
    import.Persistent.TotalNonCashFeeAmt = totalNonCashFeeAmt;
    import.Persistent.AnticipatedCheckAmt = anticipatedCheckAmt;
    import.Persistent.TotalCashAmt = totalCashAmt;
    import.Persistent.TotalCashTransactionCount = totalCashTransactionCount;
    import.Persistent.TotalNonCashAmt = totalNonCashAmt;
    import.Persistent.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptEvent Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent persistent;
    private CashReceiptEvent cashReceiptEvent;
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

    private CashReceiptSourceType cashReceiptSourceType;
  }
#endregion
}
