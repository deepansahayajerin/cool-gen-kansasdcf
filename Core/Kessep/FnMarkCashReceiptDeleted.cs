// Program: FN_MARK_CASH_RECEIPT_DELETED, ID: 372347435, model: 746.
// Short name: SWE02188
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_MARK_CASH_RECEIPT_DELETED.
/// </para>
/// <para>
/// RESP: Finance
/// This action block would logically delete a cash receipt by making it as 
/// deleted.
/// </para>
/// </summary>
[Serializable]
public partial class FnMarkCashReceiptDeleted: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MARK_CASH_RECEIPT_DELETED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMarkCashReceiptDeleted(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMarkCashReceiptDeleted.
  /// </summary>
  public FnMarkCashReceiptDeleted(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------
    //                          Change Log
    // ---------------------------------------------------------------
    // Date      Developer		Description
    // ---------------------------------------------------------------
    // 06/08/99  J. Katz		Analyzed READ statements and
    // 				changed read property to Select
    // 				Only where appropriate.
    // ---------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadCashReceipt())
    {
      if (ReadCashReceiptStatusHistory())
      {
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

    if (!ReadCashReceiptStatus())
    {
      ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

      return;
    }

    if (!ReadCashReceiptDeleteReason())
    {
      ExitState = "FN0000_CR_DELETE_REASON_NF_RB";

      return;
    }

    try
    {
      CreateCashReceiptStatusHistory();
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);

    var crtIdentifier = entities.ExistingCashReceipt.CrtIdentifier;
    var cstIdentifier = entities.ExistingCashReceipt.CstIdentifier;
    var crvIdentifier = entities.ExistingCashReceipt.CrvIdentifier;
    var crsIdentifier = entities.CashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;
    var reasonText = import.ReasonForDiscontinue.ReasonText ?? "";
    var cdrIdentifier =
      entities.CashReceiptDeleteReason.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
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
        db.SetNullableInt32(command, "cdrIdentifier", cdrIdentifier);
      });

    entities.New1.CrtIdentifier = crtIdentifier;
    entities.New1.CstIdentifier = cstIdentifier;
    entities.New1.CrvIdentifier = crvIdentifier;
    entities.New1.CrsIdentifier = crsIdentifier;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.CreatedBy = createdBy;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.ReasonText = reasonText;
    entities.New1.CdrIdentifier = cdrIdentifier;
    entities.New1.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    entities.ExistingCashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
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

  private bool ReadCashReceiptDeleteReason()
  {
    entities.CashReceiptDeleteReason.Populated = false;

    return Read("ReadCashReceiptDeleteReason",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptDeleteReason.Code);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDeleteReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDeleteReason.Code = db.GetString(reader, 1);
        entities.CashReceiptDeleteReason.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptDeleteReason.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptDeleteReason.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          import.CashReceiptStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Populated = true;
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
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.ExistingCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
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
    /// A value of CashReceiptDeleteReason.
    /// </summary>
    [JsonPropertyName("cashReceiptDeleteReason")]
    public CashReceiptDeleteReason CashReceiptDeleteReason
    {
      get => cashReceiptDeleteReason ??= new();
      set => cashReceiptDeleteReason = value;
    }

    /// <summary>
    /// A value of ReasonForDiscontinue.
    /// </summary>
    [JsonPropertyName("reasonForDiscontinue")]
    public CashReceiptStatusHistory ReasonForDiscontinue
    {
      get => reasonForDiscontinue ??= new();
      set => reasonForDiscontinue = value;
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

    private CashReceipt cashReceipt;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
    private CashReceiptStatusHistory reasonForDiscontinue;
    private CashReceiptStatus cashReceiptStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
    }

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
    /// A value of ExistingCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ExistingCashReceiptStatusHistory
    {
      get => existingCashReceiptStatusHistory ??= new();
      set => existingCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptStatusHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CashReceiptDeleteReason cashReceiptDeleteReason;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceipt existingCashReceipt;
    private CashReceiptStatusHistory existingCashReceiptStatusHistory;
    private CashReceiptStatusHistory new1;
  }
#endregion
}
