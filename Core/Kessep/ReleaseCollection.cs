// Program: RELEASE_COLLECTION, ID: 371773602, model: 746.
// Short name: SWE01059
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: RELEASE_COLLECTION.
/// </summary>
[Serializable]
public partial class ReleaseCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RELEASE_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ReleaseCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ReleaseCollection.
  /// </summary>
  public ReleaseCollection(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************
    // MAINTENANCE LOG:
    // 04/30/97  JF. Caillouet		Change Current Date
    // ********************************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnHardcodedCashReceipting();
    UseCabSetMaximumDiscontinueDate();

    if (!ReadCashReceipt())
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    if (!ReadCashReceiptDetail())
    {
      ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

      return;
    }

    if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
    {
      // --- If status is 'suspended' or 'recorded' release is possible
      if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == 1 || entities
        .CashReceiptDetailStatus.SystemGeneratedIdentifier == 3)
      {
      }
      else
      {
        export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          import.CashReceiptDetailStatus.SystemGeneratedIdentifier;
        ExitState = "FN0000_COLLECT_STAT_IMPROPER_RB";

        return;
      }
    }
    else
    {
      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

      return;
    }

    UseFnChangeCashRcptDtlStatHis();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.CashReceiptDetailStatus.Code = "REL";
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

    local.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    MoveCashReceiptDetailStatHistory(import.CashReceiptDetailStatHistory,
      useImport.New1);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.Released.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.Released.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.Recorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.Adjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
    local.Suspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.Pended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetailStatHistory.Populated = false;

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
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
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
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 10);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
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

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    private CashReceiptDetailStatus cashReceiptDetailStatus;
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
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public CashReceiptDetailStatus Released
    {
      get => released ??= new();
      set => released = value;
    }

    /// <summary>
    /// A value of Recorded.
    /// </summary>
    [JsonPropertyName("recorded")]
    public CashReceiptDetailStatus Recorded
    {
      get => recorded ??= new();
      set => recorded = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public CashReceiptDetailStatus Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
    }

    /// <summary>
    /// A value of FoundAStatus.
    /// </summary>
    [JsonPropertyName("foundAStatus")]
    public Common FoundAStatus
    {
      get => foundAStatus ??= new();
      set => foundAStatus = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DateWorkArea current;
    private CashReceiptDetailStatus released;
    private CashReceiptDetailStatus recorded;
    private CashReceiptDetailStatus adjusted;
    private CashReceiptDetailStatus suspended;
    private CashReceiptDetailStatus pended;
    private Common foundAStatus;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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

    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
