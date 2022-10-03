// Program: FN_CHANGE_CASH_RCPT_DTL_STAT_HIS, ID: 371770028, model: 746.
// Short name: SWE00310
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHANGE_CASH_RCPT_DTL_STAT_HIS.
/// </para>
/// <para>
/// RESP: FINANCE
/// DESC:  This action block will change  the status of a cash receipt by 
/// discontinuing the current status and creating the new status.	
/// </para>
/// </summary>
[Serializable]
public partial class FnChangeCashRcptDtlStatHis: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHANGE_CASH_RCPT_DTL_STAT_HIS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnChangeCashRcptDtlStatHis(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnChangeCashRcptDtlStatHis.
  /// </summary>
  public FnChangeCashRcptDtlStatHis(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // ??????	??????		Initial code
    // 123197	govind		If the existing status is the same as the new status for 
    // the same reason code , don't create a new cash receipt detail status
    // history record
    // ---------------------------------------------------------------------------------------
    UseCabSetMaximumDiscontinueDate();

    // *****  Default the discontinue date to the current date if the imported 
    // discontinue date is blank.
    if (Equal(import.OldDiscontinueDate.DiscontinueDate, local.Initialized.Date))
      
    {
      local.CashReceiptDetailStatHistory.DiscontinueDate = Now().Date;
    }
    else
    {
      local.CashReceiptDetailStatHistory.DiscontinueDate =
        import.OldDiscontinueDate.DiscontinueDate;
    }

    if (!import.Persistent.Populated)
    {
      if (ReadCashReceiptDetail())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0052_CASH_RCPT_DTL_NF";

        return;
      }
    }

    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      ++export.ImportNumberOfReads.Count;

      if (entities.Existing.SystemGeneratedIdentifier == import
        .CashReceiptDetailStatus.SystemGeneratedIdentifier && Equal
        (entities.CashReceiptDetailStatHistory.ReasonCodeId,
        import.New1.ReasonCodeId))
      {
        // --- the new status is the same as the existing one
        return;
      }

      try
      {
        UpdateCashReceiptDetailStatHistory();
        ++export.ImportNumberOfUpdates.Count;

        // continue on
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0065_CASH_RCPT_DTL_STAT_HST_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

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
      ExitState = "FN0064_CASH_RCPT_DTL_STAT_HST_NF";

      return;
    }

    if (ReadCashReceiptDetailStatus())
    {
      ++export.ImportNumberOfReads.Count;
      UseFnCreateCashRcptDtlStatHis();
    }
    else
    {
      ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";
    }
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaximumDiscontinue.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnCreateCashRcptDtlStatHis()
  {
    var useImport = new FnCreateCashRcptDtlStatHis.Import();
    var useExport = new FnCreateCashRcptDtlStatHis.Export();

    useImport.PersistentCashReceiptDetailStatus.Assign(
      entities.CashReceiptDetailStatus);
    useImport.PersistentCashReceiptDetail.Assign(import.Persistent);
    MoveCashReceiptDetailStatHistory(import.New1,
      useImport.CashReceiptDetailStatHistory);
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashRcptDtlStatHis.Execute, useImport, useExport);

    entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      useImport.PersistentCashReceiptDetailStatus.SystemGeneratedIdentifier;
    import.Persistent.Assign(useImport.PersistentCashReceiptDetail);
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private bool ReadCashReceiptDetail()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.Persistent.CrvIdentifier = db.GetInt32(reader, 0);
        import.Persistent.CstIdentifier = db.GetInt32(reader, 1);
        import.Persistent.CrtIdentifier = db.GetInt32(reader, 2);
        import.Persistent.SequentialIdentifier = db.GetInt32(reader, 3);
        import.Persistent.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.Existing.Populated = false;
    entities.CashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDiscontinue.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crdIdentifier", import.Persistent.SequentialIdentifier);
        db.SetInt32(command, "crvIdentifier", import.Persistent.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", import.Persistent.CstIdentifier);
        db.SetInt32(command, "crtIdentifier", import.Persistent.CrtIdentifier);
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
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.Existing.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.CashReceiptDetailStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailStatHistory.Populated);

    var discontinueDate = local.CashReceiptDetailStatHistory.DiscontinueDate;

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
    /// A value of OldDiscontinueDate.
    /// </summary>
    [JsonPropertyName("oldDiscontinueDate")]
    public CashReceiptDetailStatHistory OldDiscontinueDate
    {
      get => oldDiscontinueDate ??= new();
      set => oldDiscontinueDate = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailStatHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptDetail Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    private CashReceiptDetailStatHistory oldDiscontinueDate;
    private CashReceiptDetailStatHistory new1;
    private CashReceiptDetail persistent;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private DateWorkArea maximumDiscontinue;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CashReceiptDetailStatus Existing
    {
      get => existing ??= new();
      set => existing = value;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private CashReceiptDetailStatus existing;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
