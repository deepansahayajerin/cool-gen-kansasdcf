// Program: FN_CREATE_CASH_RCPT_STAT_HIST, ID: 371722240, model: 746.
// Short name: SWE00349
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RCPT_STAT_HIST.
/// </para>
/// <para>
/// RESP: FINANCE
/// This common action block will create a new status history for a cash 
/// receipt.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashRcptStatHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RCPT_STAT_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashRcptStatHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashRcptStatHist.
  /// </summary>
  public FnCreateCashRcptStatHist(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------
    // 06/04/99	J. Katz		Analyzed READ statements and
    // 				changed read property to
    // 				Select Only where appropriate.
    // -----------------------------------------------------------------
    // *****
    // Make sure currency has not been lost.  If it has, reread
    // the entities.
    // *****
    if (!import.PersistentCashReceipt.Populated)
    {
      if (!ReadCashReceipt())
      {
        ExitState = "FN0086_CASH_RCPT_NF_RB";

        return;
      }
    }

    if (!import.PersistentCashReceiptStatus.Populated)
    {
      if (!ReadCashReceiptStatus())
      {
        ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

        return;
      }
    }

    // ----------------------------------------------------------------
    // Set up local views to handle current date and maximum
    // discontinue date.    JLK  09/22/98
    // ----------------------------------------------------------------
    local.Current.Timestamp = Now();
    UseCabSetMaximumDiscontinueDate();

    // ----------------------------------------------------------------
    // Create new Cash Receipt Status History record.
    // Added export view to pass new status history information to
    // calling action block.       JLK  09/23/98
    // ----------------------------------------------------------------
    try
    {
      CreateCashReceiptStatusHistory();
      export.CashReceiptStatusHistory.Assign(entities.CashReceiptStatusHistory);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0101_CASH_RCPT_STAT_HIST_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0105_CASH_RCPT_STAT_HIST_PV_RB";

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

    local.MaximumDiscontinue.Date = useExport.DateWorkArea.Date;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.PersistentCashReceipt.Populated);

    var crtIdentifier = import.PersistentCashReceipt.CrtIdentifier;
    var cstIdentifier = import.PersistentCashReceipt.CstIdentifier;
    var crvIdentifier = import.PersistentCashReceipt.CrvIdentifier;
    var crsIdentifier =
      import.PersistentCashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.MaximumDiscontinue.Date;
    var reasonText = import.CashReceiptStatusHistory.ReasonText ?? "";

    entities.CashReceiptStatusHistory.Populated = false;
    Update("CreateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crsIdentifier", crsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.CashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.CashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptStatusHistory.CreatedBy = createdBy;
    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.ReasonText = reasonText;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    import.PersistentCashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        import.PersistentCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        import.PersistentCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        import.PersistentCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        import.PersistentCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        import.PersistentCashReceipt.ReceiptDate = db.GetDate(reader, 5);
        import.PersistentCashReceipt.CheckType =
          db.GetNullableString(reader, 6);
        import.PersistentCashReceipt.CheckNumber =
          db.GetNullableString(reader, 7);
        import.PersistentCashReceipt.CheckDate = db.GetNullableDate(reader, 8);
        import.PersistentCashReceipt.ReceivedDate = db.GetDate(reader, 9);
        import.PersistentCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 10);
        import.PersistentCashReceipt.ReferenceNumber =
          db.GetNullableString(reader, 11);
        import.PersistentCashReceipt.PayorOrganization =
          db.GetNullableString(reader, 12);
        import.PersistentCashReceipt.PayorFirstName =
          db.GetNullableString(reader, 13);
        import.PersistentCashReceipt.PayorMiddleName =
          db.GetNullableString(reader, 14);
        import.PersistentCashReceipt.PayorLastName =
          db.GetNullableString(reader, 15);
        import.PersistentCashReceipt.ForwardedToName =
          db.GetNullableString(reader, 16);
        import.PersistentCashReceipt.ForwardedStreet1 =
          db.GetNullableString(reader, 17);
        import.PersistentCashReceipt.ForwardedStreet2 =
          db.GetNullableString(reader, 18);
        import.PersistentCashReceipt.ForwardedCity =
          db.GetNullableString(reader, 19);
        import.PersistentCashReceipt.ForwardedState =
          db.GetNullableString(reader, 20);
        import.PersistentCashReceipt.ForwardedZip5 =
          db.GetNullableString(reader, 21);
        import.PersistentCashReceipt.ForwardedZip4 =
          db.GetNullableString(reader, 22);
        import.PersistentCashReceipt.ForwardedZip3 =
          db.GetNullableString(reader, 23);
        import.PersistentCashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 24);
        import.PersistentCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 25);
        import.PersistentCashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 26);
        import.PersistentCashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 27);
        import.PersistentCashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 28);
        import.PersistentCashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 29);
        import.PersistentCashReceipt.CreatedBy = db.GetString(reader, 30);
        import.PersistentCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 31);
        import.PersistentCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 32);
        import.PersistentCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 33);
        import.PersistentCashReceipt.CashDue =
          db.GetNullableDecimal(reader, 34);
        import.PersistentCashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 35);
        import.PersistentCashReceipt.TotalCashFeeAmount =
          db.GetNullableDecimal(reader, 36);
        import.PersistentCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 37);
        import.PersistentCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 38);
        import.PersistentCashReceipt.Note = db.GetNullableString(reader, 39);
        import.PersistentCashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          import.PersistentCashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptStatus()
  {
    import.PersistentCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          import.CashReceiptStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.PersistentCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentCashReceiptStatus.Code = db.GetString(reader, 1);
        import.PersistentCashReceiptStatus.Populated = true;
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
    /// A value of PersistentCashReceipt.
    /// </summary>
    [JsonPropertyName("persistentCashReceipt")]
    public CashReceipt PersistentCashReceipt
    {
      get => persistentCashReceipt ??= new();
      set => persistentCashReceipt = value;
    }

    /// <summary>
    /// A value of PersistentCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("persistentCashReceiptStatus")]
    public CashReceiptStatus PersistentCashReceiptStatus
    {
      get => persistentCashReceiptStatus ??= new();
      set => persistentCashReceiptStatus = value;
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

    private CashReceipt persistentCashReceipt;
    private CashReceiptStatus persistentCashReceiptStatus;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
    }

    private DateWorkArea current;
    private DateWorkArea maximumDiscontinue;
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
