// Program: READ_CASH_RECEIPT_DELETE_REASON, ID: 371721489, model: 746.
// Short name: SWE01022
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: READ_CASH_RECEIPT_DELETE_REASON.
/// </para>
/// <para>
/// RESP: CASHMGMT					To read for a cash receipt delete reason to pass back to 
/// the prad.
/// </para>
/// </summary>
[Serializable]
public partial class ReadCashReceiptDeleteReason: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the READ_CASH_RECEIPT_DELETE_REASON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ReadCashReceiptDeleteReason(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ReadCashReceiptDeleteReason.
  /// </summary>
  public ReadCashReceiptDeleteReason(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // 06/07/99  J. Katz - SRG		Analyzed READ statements and changed
    // 				read property to Select Only where
    // 				appropriate.
    // ------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    UseFnHardcodedCashReceipting();
    ReadCashReceiptStatusHistoryCashReceiptStatus();

    if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
      .HardcodedDelete.SystemGeneratedIdentifier)
    {
      if (ReadCashReceiptDeleteReason1())
      {
        export.CashReceiptStatusHistory.
          Assign(entities.CashReceiptStatusHistory);
        MoveCashReceiptDeleteReason(entities.CashReceiptDeleteReason,
          export.CashReceiptDeleteReason);
      }
      else
      {
        ExitState = "FN0035_CASH_RCPT_DEL_RSN_NF";
      }
    }
    else
    {
      export.CashReceiptStatusHistory.Assign(local.Clear);
    }
  }

  private static void MoveCashReceiptDeleteReason(
    CashReceiptDeleteReason source, CashReceiptDeleteReason target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedDelete.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceiptDeleteReason1()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);
    entities.CashReceiptDeleteReason.Populated = false;

    return Read("ReadCashReceiptDeleteReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdeleteRsnId",
          entities.CashReceiptStatusHistory.CdrIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDeleteReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDeleteReason.Code = db.GetString(reader, 1);
        entities.CashReceiptDeleteReason.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    entities.CashReceiptStatus.Populated = false;
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
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
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
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
        entities.CashReceiptStatus.Populated = true;
        entities.CashReceiptStatusHistory.Populated = true;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private CashReceiptDeleteReason cashReceiptDeleteReason;
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
    /// A value of HardcodedDelete.
    /// </summary>
    [JsonPropertyName("hardcodedDelete")]
    public CashReceiptStatus HardcodedDelete
    {
      get => hardcodedDelete ??= new();
      set => hardcodedDelete = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CashReceiptStatusHistory Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    private DateWorkArea current;
    private CashReceiptStatus hardcodedDelete;
    private CashReceiptStatusHistory clear;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
  }
#endregion
}
