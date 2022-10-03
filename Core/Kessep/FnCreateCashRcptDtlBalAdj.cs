// Program: FN_CREATE_CASH_RCPT_DTL_BAL_ADJ, ID: 372532811, model: 746.
// Short name: SWE00340
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RCPT_DTL_BAL_ADJ.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block creates an occurrence of the Cash Receipt Detail 
/// Balance Adjustment entity and associates it to the imported Cash Receipt
/// Details and the Relationship Reason.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashRcptDtlBalAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RCPT_DTL_BAL_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashRcptDtlBalAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashRcptDtlBalAdj.
  /// </summary>
  public FnCreateCashRcptDtlBalAdj(IContext context, Import import,
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
    local.Current.Timestamp = Now();

    if (Equal(import.ProgramProcessingInfo.ProcessDate, local.Clear.Date))
    {
      local.AdjustmentDateDateWorkArea.Date = Now().Date;
    }
    else
    {
      local.AdjustmentDateDateWorkArea.Date =
        import.ProgramProcessingInfo.ProcessDate;
    }

    if (!import.Persistent.Populated)
    {
      if (ReadCashReceiptDetailRlnRsn())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0059_CASH_RCPT_DTL_RLN_RSN_NF";

        return;
      }
    }

    if (!import.PersistentAdjusting.Populated)
    {
      if (ReadCashReceiptDetail2())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0052_CASH_RCPT_DTL_NF";

        return;
      }
    }

    if (!import.PersistentAdjusted.Populated)
    {
      if (ReadCashReceiptDetail1())
      {
        ++export.ImportNumberOfReads.Count;

        // ------------------------------------------------------------
        // Set up text to update the NOTES attribute for the adjusted
        // detail record.  A statement will be added to NOTES
        // indicating that the 'Cash Receipt Detail was adjusted on
        // [process date] by Receipt # [adjusting receipt sequential
        // number].'    JLK  04/09/99
        // ------------------------------------------------------------
        local.AdjustmentDateDateWorkArea.Day =
          Day(local.AdjustmentDateDateWorkArea.Date);
        local.AdjustmentDateDateWorkArea.Month =
          Month(local.AdjustmentDateDateWorkArea.Date);
        local.AdjustmentDateDateWorkArea.Year =
          Year(local.AdjustmentDateDateWorkArea.Date);
        local.AdjustmentDateTextWorkArea.Text10 =
          NumberToString(local.AdjustmentDateDateWorkArea.Month, 14, 2) + "/"
          + NumberToString(local.AdjustmentDateDateWorkArea.Day, 14, 2) + "/"
          + NumberToString(local.AdjustmentDateDateWorkArea.Year, 12, 4);
        local.AdjustingCrNumber.Text10 =
          NumberToString(import.AdjustingCashReceipt.SequentialNumber, 7, 9);
        local.Adjusted.Notes = TrimEnd(import.PersistentAdjusted.Notes) + "  CASH RECEIPT DETAIL WAS ADJUSTED ON " +
          local.AdjustmentDateTextWorkArea.Text10 + " BY RECEIPT # " + TrimEnd
          (local.AdjustingCrNumber.Text10) + ".";

        // ------------------------------------------------------------
        // If the adjusted detail has refunded dollars, include a
        // comment in the NOTE field indicating that money was
        // refunded and may need to be manually recovered.
        // JLK  05/18/99
        // ------------------------------------------------------------
        if (import.AdjustedCashReceiptDetail.RefundedAmount.
          GetValueOrDefault() > 0)
        {
          local.Adjusted.Notes = (local.Adjusted.Notes ?? "") + "  ALSO REFUNDED MONEY WAS ADJUSTED AND MAY NEED TO BE MANUALLY RECOVERED.";
            
        }

        try
        {
          UpdateCashReceiptDetail();
          ++export.ImportNumberOfUpdates.Count;
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
      else
      {
        ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

        return;
      }
    }

    try
    {
      CreateCashReceiptDetailBalanceAdj();
      ++export.ImportNumberOfUpdates.Count;
      MoveCashReceiptDetailBalanceAdj(entities.CashReceiptDetailBalanceAdj,
        export.CashReceiptDetailBalanceAdj);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0042_CASH_RCPT_DTL_BAL_ADJ_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0045_CASH_RCPT_DTL_BAL_ADJ_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveCashReceiptDetailBalanceAdj(
    CashReceiptDetailBalanceAdj source, CashReceiptDetailBalanceAdj target)
  {
    target.Description = source.Description;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void CreateCashReceiptDetailBalanceAdj()
  {
    System.Diagnostics.Debug.Assert(import.PersistentAdjusted.Populated);
    System.Diagnostics.Debug.Assert(import.PersistentAdjusting.Populated);

    var crdIdentifier = import.PersistentAdjusted.SequentialIdentifier;
    var crvIdentifier = import.PersistentAdjusted.CrvIdentifier;
    var cstIdentifier = import.PersistentAdjusted.CstIdentifier;
    var crtIdentifier = import.PersistentAdjusted.CrtIdentifier;
    var crdSIdentifier = import.PersistentAdjusting.SequentialIdentifier;
    var crvSIdentifier = import.PersistentAdjusting.CrvIdentifier;
    var cstSIdentifier = import.PersistentAdjusting.CstIdentifier;
    var crtSIdentifier = import.PersistentAdjusting.CrtIdentifier;
    var crnIdentifier = import.Persistent.SequentialIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var description = import.CashReceiptDetailBalanceAdj.Description ?? "";

    entities.CashReceiptDetailBalanceAdj.Populated = false;
    Update("CreateCashReceiptDetailBalanceAdj",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "crdSIdentifier", crdSIdentifier);
        db.SetInt32(command, "crvSIdentifier", crvSIdentifier);
        db.SetInt32(command, "cstSIdentifier", cstSIdentifier);
        db.SetInt32(command, "crtSIdentifier", crtSIdentifier);
        db.SetInt32(command, "crnIdentifier", crnIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "description", description);
      });

    entities.CashReceiptDetailBalanceAdj.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailBalanceAdj.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailBalanceAdj.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailBalanceAdj.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailBalanceAdj.CrdSIdentifier = crdSIdentifier;
    entities.CashReceiptDetailBalanceAdj.CrvSIdentifier = crvSIdentifier;
    entities.CashReceiptDetailBalanceAdj.CstSIdentifier = cstSIdentifier;
    entities.CashReceiptDetailBalanceAdj.CrtSIdentifier = crtSIdentifier;
    entities.CashReceiptDetailBalanceAdj.CrnIdentifier = crnIdentifier;
    entities.CashReceiptDetailBalanceAdj.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailBalanceAdj.CreatedBy = createdBy;
    entities.CashReceiptDetailBalanceAdj.Description = description;
    entities.CashReceiptDetailBalanceAdj.Populated = true;
  }

  private bool ReadCashReceiptDetail1()
  {
    import.PersistentAdjusted.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          import.AdjustedCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId",
          import.AdjustedCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentAdjusted.CrvIdentifier = db.GetInt32(reader, 0);
        import.PersistentAdjusted.CstIdentifier = db.GetInt32(reader, 1);
        import.PersistentAdjusted.CrtIdentifier = db.GetInt32(reader, 2);
        import.PersistentAdjusted.SequentialIdentifier = db.GetInt32(reader, 3);
        import.PersistentAdjusted.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        import.PersistentAdjusted.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        import.PersistentAdjusted.Notes = db.GetNullableString(reader, 6);
        import.PersistentAdjusted.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    import.PersistentAdjusting.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          import.AdjustingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId",
          import.AdjustingCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentAdjusting.CrvIdentifier = db.GetInt32(reader, 0);
        import.PersistentAdjusting.CstIdentifier = db.GetInt32(reader, 1);
        import.PersistentAdjusting.CrtIdentifier = db.GetInt32(reader, 2);
        import.PersistentAdjusting.SequentialIdentifier =
          db.GetInt32(reader, 3);
        import.PersistentAdjusting.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailRlnRsn()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceiptDetailRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdtlRlnRsnId",
          import.CashReceiptDetailRlnRsn.SequentialIdentifier);
      },
      (db, reader) =>
      {
        import.Persistent.SequentialIdentifier = db.GetInt32(reader, 0);
        import.Persistent.Populated = true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(import.PersistentAdjusted.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;
    var notes = local.Adjusted.Notes ?? "";

    import.PersistentAdjusted.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "notes", notes);
        db.SetInt32(
          command, "crvIdentifier", import.PersistentAdjusted.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", import.PersistentAdjusted.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", import.PersistentAdjusted.CrtIdentifier);
        db.SetInt32(
          command, "crdId", import.PersistentAdjusted.SequentialIdentifier);
      });

    import.PersistentAdjusted.LastUpdatedBy = lastUpdatedBy;
    import.PersistentAdjusted.LastUpdatedTmst = lastUpdatedTmst;
    import.PersistentAdjusted.Notes = notes;
    import.PersistentAdjusted.Populated = true;
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
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptDetailRlnRsn Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
    }

    /// <summary>
    /// A value of PersistentAdjusted.
    /// </summary>
    [JsonPropertyName("persistentAdjusted")]
    public CashReceiptDetail PersistentAdjusted
    {
      get => persistentAdjusted ??= new();
      set => persistentAdjusted = value;
    }

    /// <summary>
    /// A value of AdjustedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustedCashReceiptDetail")]
    public CashReceiptDetail AdjustedCashReceiptDetail
    {
      get => adjustedCashReceiptDetail ??= new();
      set => adjustedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of AdjustedCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustedCashReceipt")]
    public CashReceipt AdjustedCashReceipt
    {
      get => adjustedCashReceipt ??= new();
      set => adjustedCashReceipt = value;
    }

    /// <summary>
    /// A value of PersistentAdjusting.
    /// </summary>
    [JsonPropertyName("persistentAdjusting")]
    public CashReceiptDetail PersistentAdjusting
    {
      get => persistentAdjusting ??= new();
      set => persistentAdjusting = value;
    }

    /// <summary>
    /// A value of AdjustingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustingCashReceiptDetail")]
    public CashReceiptDetail AdjustingCashReceiptDetail
    {
      get => adjustingCashReceiptDetail ??= new();
      set => adjustingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of AdjustingCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustingCashReceipt")]
    public CashReceipt AdjustingCashReceipt
    {
      get => adjustingCashReceipt ??= new();
      set => adjustingCashReceipt = value;
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

    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CashReceiptDetailRlnRsn persistent;
    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
    private CashReceiptDetail persistentAdjusted;
    private CashReceiptDetail adjustedCashReceiptDetail;
    private CashReceipt adjustedCashReceipt;
    private CashReceiptDetail persistentAdjusting;
    private CashReceiptDetail adjustingCashReceiptDetail;
    private CashReceipt adjustingCashReceipt;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
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

    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public DateWorkArea Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of AdjustmentDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("adjustmentDateDateWorkArea")]
    public DateWorkArea AdjustmentDateDateWorkArea
    {
      get => adjustmentDateDateWorkArea ??= new();
      set => adjustmentDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of AdjustmentDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("adjustmentDateTextWorkArea")]
    public TextWorkArea AdjustmentDateTextWorkArea
    {
      get => adjustmentDateTextWorkArea ??= new();
      set => adjustmentDateTextWorkArea = value;
    }

    /// <summary>
    /// A value of AdjustingCrNumber.
    /// </summary>
    [JsonPropertyName("adjustingCrNumber")]
    public TextWorkArea AdjustingCrNumber
    {
      get => adjustingCrNumber ??= new();
      set => adjustingCrNumber = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public CashReceiptDetail Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    private DateWorkArea current;
    private DateWorkArea clear;
    private DateWorkArea adjustmentDateDateWorkArea;
    private TextWorkArea adjustmentDateTextWorkArea;
    private TextWorkArea adjustingCrNumber;
    private CashReceiptDetail adjusted;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public CashReceipt Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    /// <summary>
    /// A value of Adjusting.
    /// </summary>
    [JsonPropertyName("adjusting")]
    public CashReceipt Adjusting
    {
      get => adjusting ??= new();
      set => adjusting = value;
    }

    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CashReceipt adjusted;
    private CashReceipt adjusting;
  }
#endregion
}
