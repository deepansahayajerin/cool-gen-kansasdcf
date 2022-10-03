// Program: FN_CREATE_CASH_RCPT_DTL_STAT_HIS, ID: 371770911, model: 746.
// Short name: SWE00343
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RCPT_DTL_STAT_HIS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will create a new cash receipt detail status history.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashRcptDtlStatHis: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RCPT_DTL_STAT_HIS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashRcptDtlStatHis(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashRcptDtlStatHis.
  /// </summary>
  public FnCreateCashRcptDtlStatHis(IContext context, Import import,
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
    UseCabSetMaximumDiscontinueDate();

    // *****  Make sure currency has not been lost.
    if (!import.PersistentCashReceiptDetailStatus.Populated)
    {
      if (ReadCashReceiptDetailStatus())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

        return;
      }
    }

    if (!import.PersistentCashReceiptDetail.Populated)
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

    try
    {
      CreateCashReceiptDetailStatHistory();
      ++export.ImportNumberOfUpdates.Count;
      export.CashReceiptDetailStatHistory.Assign(
        entities.CashReceiptDetailStatHistory);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0063_CASH_RCPT_DTL_STAT_HST_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

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

    local.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistentCashReceiptDetail.Populated);

    var crdIdentifier = import.PersistentCashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = import.PersistentCashReceiptDetail.CrvIdentifier;
    var cstIdentifier = import.PersistentCashReceiptDetail.CstIdentifier;
    var crtIdentifier = import.PersistentCashReceiptDetail.CrtIdentifier;
    var cdsIdentifier =
      import.PersistentCashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var reasonCodeId = import.CashReceiptDetailStatHistory.ReasonCodeId ?? "";
    var createdBy = global.UserId;
    var discontinueDate = local.Maximum.Date;
    var reasonText = import.CashReceiptDetailStatHistory.ReasonText ?? "";

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
        db.SetNullableString(command, "reasonCodeId", reasonCodeId);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.CashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.CashReceiptDetailStatHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailStatHistory.ReasonCodeId = reasonCodeId;
    entities.CashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.ReasonText = reasonText;
    entities.CashReceiptDetailStatHistory.Populated = true;
  }

  private bool ReadCashReceiptDetail()
  {
    import.PersistentCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        import.PersistentCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        import.PersistentCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        import.PersistentCashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    import.PersistentCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.CashReceiptDetailStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.PersistentCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentCashReceiptDetailStatus.Populated = true;
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
    /// A value of PersistentCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("persistentCashReceiptDetailStatus")]
    public CashReceiptDetailStatus PersistentCashReceiptDetailStatus
    {
      get => persistentCashReceiptDetailStatus ??= new();
      set => persistentCashReceiptDetailStatus = value;
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
    /// A value of PersistentCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("persistentCashReceiptDetail")]
    public CashReceiptDetail PersistentCashReceiptDetail
    {
      get => persistentCashReceiptDetail ??= new();
      set => persistentCashReceiptDetail = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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

    private CashReceiptDetailStatus persistentCashReceiptDetailStatus;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail persistentCashReceiptDetail;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
  }
#endregion
}
