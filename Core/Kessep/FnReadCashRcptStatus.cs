// Program: FN_READ_CASH_RCPT_STATUS, ID: 371722977, model: 746.
// Short name: SWE00535
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_CASH_RCPT_STATUS.
/// </para>
/// <para>
/// RESP:  FINANCE
/// This CAB will return a CASH_RECEIPT STATUS.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCashRcptStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CASH_RCPT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCashRcptStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCashRcptStatus.
  /// </summary>
  public FnReadCashRcptStatus(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******
    // This function will abend if any not found is encountered. These are data 
    // base errors and should not occur. This is per last review.
    // ******
    local.Maximum.DiscontinueDate = UseCabSetMaximumDiscontinueDate();

    if (ReadCashReceiptStatusHistory())
    {
      ReadCashReceiptStatus();
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCashReceiptStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);
    export.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.CashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        export.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.CashReceiptStatus.Code = db.GetString(reader, 1);
        export.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", import.CashReceipt.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.CashReceipt.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.CashReceipt.CrvIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.Maximum.DiscontinueDate.GetValueOrDefault());
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
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceipt cashReceipt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
    }

    private CashReceiptStatus cashReceiptStatus;
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
    public CashReceiptStatus Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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

    private CashReceiptStatus maximum;
    private DateWorkArea max;
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

    private CashReceiptStatusHistory cashReceiptStatusHistory;
  }
#endregion
}
