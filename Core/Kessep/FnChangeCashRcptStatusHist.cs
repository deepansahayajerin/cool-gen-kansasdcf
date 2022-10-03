// Program: FN_CHANGE_CASH_RCPT_STATUS_HIST, ID: 371721897, model: 746.
// Short name: SWE00311
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHANGE_CASH_RCPT_STATUS_HIST.
/// </para>
/// <para>
/// RESP: Finance
/// This action block will change  the status of a cash receipt by discontinuing
/// the current status and creating the new status.
/// </para>
/// </summary>
[Serializable]
public partial class FnChangeCashRcptStatusHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHANGE_CASH_RCPT_STATUS_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnChangeCashRcptStatusHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnChangeCashRcptStatusHist.
  /// </summary>
  public FnChangeCashRcptStatusHist(IContext context, Import import,
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
    // -------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // ??????	??????		Initial code
    // 020398	govind		If the cash receipt is already in the required status, 
    // don't create new cash receipt status history.
    // 			Set the persistent view as unlocked.
    // 06/04/99  J. Katz	Analyzed READ statements and changed read
    // 			property to Select Only where appropriate.
    // -------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();

    if (ReadCashReceipt())
    {
      if (ReadCashReceiptStatusHistory())
      {
        if (ReadCashReceiptStatus())
        {
          if (entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier == import
            .NewPersistent.SystemGeneratedIdentifier)
          {
            // ----------------------------------------------------------
            // There is no change in the Cash Receipt Status. So return.
            // ----------------------------------------------------------
            return;
          }
        }
        else
        {
          ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

          return;
        }

        UpdateCashReceiptStatusHistory();

        // continue on
      }
      else
      {
        ExitState = "FN0103_CASH_RCPT_STAT_HIST_NF_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0086_CASH_RCPT_NF_RB";

      return;
    }

    UseFnCreateCashRcptStatHist();
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaximumDiscontinue.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnCreateCashRcptStatHist()
  {
    var useImport = new FnCreateCashRcptStatHist.Import();
    var useExport = new FnCreateCashRcptStatHist.Export();

    useImport.PersistentCashReceiptStatus.Assign(import.NewPersistent);
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptStatusHistory.ReasonText =
      import.CashReceiptStatusHistory.ReasonText;

    Call(FnCreateCashRcptStatHist.Execute, useImport, useExport);

    import.NewPersistent.Assign(useImport.PersistentCashReceiptStatus);
    export.CashReceiptStatusHistory.Assign(useExport.CashReceiptStatusHistory);
  }

  private bool ReadCashReceipt()
  {
    entities.ExistingCashReceipt.Populated = false;

    return Read("ReadCashReceipt",
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
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCashReceiptStatusHistory.Populated);
    entities.ExistingCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.ExistingCashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptStatus.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingCashReceiptStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.ExistingCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingCashReceiptStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceiptStatusHistory.Populated = true;
      });
  }

  private void UpdateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCashReceiptStatusHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.ExistingCashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.ExistingCashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingCashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ExistingCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.ExistingCashReceiptStatusHistory.Populated = true;
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
    /// A value of NewPersistent.
    /// </summary>
    [JsonPropertyName("newPersistent")]
    public CashReceiptStatus NewPersistent
    {
      get => newPersistent ??= new();
      set => newPersistent = value;
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

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatus newPersistent;
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
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ExistingCashReceiptStatusHistory
    {
      get => existingCashReceiptStatusHistory ??= new();
      set => existingCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatus")]
    public CashReceiptStatus ExistingCashReceiptStatus
    {
      get => existingCashReceiptStatus ??= new();
      set => existingCashReceiptStatus = value;
    }

    private CashReceipt existingCashReceipt;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptType existingCashReceiptType;
    private CashReceiptStatusHistory existingCashReceiptStatusHistory;
    private CashReceiptStatus existingCashReceiptStatus;
  }
#endregion
}
