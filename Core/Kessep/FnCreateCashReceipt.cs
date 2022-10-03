// Program: FN_CREATE_CASH_RECEIPT, ID: 371721894, model: 746.
// Short name: SWE00352
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RECEIPT.
/// </para>
/// <para>
/// RESP:  FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashReceipt.
  /// </summary>
  public FnCreateCashReceipt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **** A cash receipt must be in a status at all times.  When a cash 
    // receipt is entered it will automatically be placed in a "RECORDED" status
    // (Cash Receipt Status ID 1) unless it is from an interface where the
    // check lags behind the electronic processing of the detail file.  These
    // cash receipts are set up with a status of INTERFACE (cash receipt status
    // id 3) when the file is processed and changed to recorded when the actual
    // check is receipted and matched to the preliminary electronic interfaced
    // cash receipt.
    // ---------------------------------------------------------------------
    // Modification Log
    // ---------------------------------------------------------------------
    // 06/03/99  J. Katz	Analyzed READ statements and changed read
    // 			property to Select Only where appropriate.
    // ---------------------------------------------------------------------
    // **** HARDCODE area ****
    local.Current.Timestamp = Now();
    UseCabSetMaximumDiscontinueDate();

    if (ReadCashReceiptSourceType())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

      return;
    }

    if (import.CashReceiptEvent.SystemGeneratedIdentifier > 0)
    {
      if (ReadCashReceiptEvent())
      {
        export.CashReceiptEvent.SystemGeneratedIdentifier =
          entities.CashReceiptEvent.SystemGeneratedIdentifier;
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0077_CASH_RCPT_EVENT_NF";

        return;
      }
    }
    else
    {
      do
      {
        try
        {
          CreateCashReceiptEvent();
          export.CashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          ExitState = "ACO_NN0000_ALL_OK";
          ++export.ImportNumberOfUpdates.Count;
          local.RetryCount.Count = 6;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.RetryCount.Count;
              ExitState = "FN0076_CASH_RCPT_EVENT_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0080_CASH_RCPT_EVENT_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      while(local.RetryCount.Count <= 5);

      if (IsExitState("FN0076_CASH_RCPT_EVENT_AE"))
      {
        return;
      }
    }

    if (ReadCashReceiptType())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0113_CASH_RCPT_TYPE_NF";

      return;
    }

    do
    {
      // ------------------------------------------------------
      // The next sequential cash receipt number is retrieved
      // from the access control table identifier equal to
      // "CASH RECEIPT" and updated.
      // ------------------------------------------------------
      local.ControlTable.Identifier = "CASH RECEIPT";
      UseAccessControlTable();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++export.ImportNumberOfReads.Count;
        ++export.ImportNumberOfUpdates.Count;
      }
      else
      {
        return;
      }

      // --------------------------------------------------------------------------
      // A Cash Receipt will never have forwarding information at
      // the time the receipt is created.  All SET statements for
      // attributes related to Forwarding a receipt are set to spaces.
      // These attributes were not removed from the views in this
      // CAB to minimize the impact on other action blocks using
      // this CAB.
      // JLK     09/29/98
      // -------------------------------------------------------------------------
      try
      {
        CreateCashReceipt();
        export.CashReceipt.SequentialNumber =
          entities.CashReceipt.SequentialNumber;
        ExitState = "ACO_NN0000_ALL_OK";
        ++export.ImportNumberOfUpdates.Count;
        local.RetryCount.Count = 6;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0126_CASH_RCPT_AE_W_RB";
            ++local.RetryCount.Count;

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0090_CASH_RCPT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(local.RetryCount.Count <= 5);

    if (IsExitState("FN0126_CASH_RCPT_AE_W_RB"))
    {
      return;
    }

    if (ReadCashReceiptStatus())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0108_CASH_RCPT_STAT_NF";

      return;
    }

    try
    {
      CreateCashReceiptStatusHistory();
      ++export.ImportNumberOfUpdates.Count;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "ZD_FN0099_CASH_RCPT_STAT_HT_AERB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0104_CASH_RCPT_STAT_HIST_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaximumDiscontinue.Date = useExport.DateWorkArea.Date;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);

    var crvIdentifier = entities.CashReceiptEvent.SystemGeneratedIdentifier;
    var cstIdentifier = entities.CashReceiptEvent.CstIdentifier;
    var crtIdentifier = entities.CashReceiptType.SystemGeneratedIdentifier;
    var receiptAmount = import.CashReceipt.ReceiptAmount;
    var sequentialNumber = local.ControlTable.LastUsedNumber;
    var receiptDate = import.CashReceipt.ReceiptDate;
    var checkType = import.CashReceipt.CheckType ?? "";
    var checkNumber = import.CashReceipt.CheckNumber ?? "";
    var checkDate = import.CashReceipt.CheckDate;
    var receivedDate = import.CashReceipt.ReceivedDate;
    var referenceNumber = import.CashReceipt.ReferenceNumber ?? "";
    var payorOrganization = import.CashReceipt.PayorOrganization ?? "";
    var payorFirstName = import.CashReceipt.PayorFirstName ?? "";
    var payorMiddleName = import.CashReceipt.PayorMiddleName ?? "";
    var payorLastName = import.CashReceipt.PayorLastName ?? "";
    var totalCashTransactionAmount =
      import.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault();
    var totalNoncashTransactionAmount =
      import.CashReceipt.TotalNoncashTransactionAmount.GetValueOrDefault();
    var totalCashTransactionCount =
      import.CashReceipt.TotalCashTransactionCount.GetValueOrDefault();
    var totalDetailAdjustmentCount =
      import.CashReceipt.TotalDetailAdjustmentCount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var cashBalanceAmt = import.CashReceipt.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = import.CashReceipt.CashBalanceReason ?? "";
    var cashDue = import.CashReceipt.CashDue.GetValueOrDefault();
    var note = import.CashReceipt.Note ?? "";
    var payorSocialSecurityNumber =
      import.CashReceipt.PayorSocialSecurityNumber ?? "";

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.CashReceipt.Populated = false;
    Update("CreateCashReceipt",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetInt32(command, "cashReceiptId", sequentialNumber);
        db.SetDate(command, "receiptDate", receiptDate);
        db.SetNullableString(command, "checkType", checkType);
        db.SetNullableString(command, "checkNumber", checkNumber);
        db.SetNullableDate(command, "checkDate", checkDate);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "depositRlseDt", default(DateTime));
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetNullableString(command, "payorOrganization", payorOrganization);
        db.SetNullableString(command, "payorFirstName", payorFirstName);
        db.SetNullableString(command, "payorMiddleName", payorMiddleName);
        db.SetNullableString(command, "payorLastName", payorLastName);
        db.SetNullableString(command, "frwrdToName", "");
        db.SetNullableString(command, "frwrdStreet1", "");
        db.SetNullableString(command, "frwrdStreet2", "");
        db.SetNullableString(command, "frwrdCity", "");
        db.SetNullableString(command, "frwrdState", "");
        db.SetNullableString(command, "frwrdZip5", "");
        db.SetNullableString(command, "frwrdZip4", "");
        db.SetNullableString(command, "frwrdZip3", "");
        db.SetNullableDateTime(command, "balTmst", default(DateTime));
        db.SetNullableDecimal(
          command, "totalCashTransac", totalCashTransactionAmount);
        db.SetNullableDecimal(
          command, "totNoncshTrnAmt", totalNoncashTransactionAmount);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableInt32(command, "totNocshTranCnt", 0);
        db.SetNullableInt32(
          command, "totDetailAdjCnt", totalDetailAdjustmentCount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableDecimal(command, "cashDue", cashDue);
        db.SetNullableDecimal(command, "totalNcFeeAmt", 0M);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "payorSsn", payorSocialSecurityNumber);
      });

    entities.CashReceipt.CrvIdentifier = crvIdentifier;
    entities.CashReceipt.CstIdentifier = cstIdentifier;
    entities.CashReceipt.CrtIdentifier = crtIdentifier;
    entities.CashReceipt.ReceiptAmount = receiptAmount;
    entities.CashReceipt.SequentialNumber = sequentialNumber;
    entities.CashReceipt.ReceiptDate = receiptDate;
    entities.CashReceipt.CheckType = checkType;
    entities.CashReceipt.CheckNumber = checkNumber;
    entities.CashReceipt.CheckDate = checkDate;
    entities.CashReceipt.ReceivedDate = receivedDate;
    entities.CashReceipt.ReferenceNumber = referenceNumber;
    entities.CashReceipt.PayorOrganization = payorOrganization;
    entities.CashReceipt.PayorFirstName = payorFirstName;
    entities.CashReceipt.PayorMiddleName = payorMiddleName;
    entities.CashReceipt.PayorLastName = payorLastName;
    entities.CashReceipt.ForwardedToName = "";
    entities.CashReceipt.ForwardedStreet1 = "";
    entities.CashReceipt.ForwardedStreet2 = "";
    entities.CashReceipt.ForwardedCity = "";
    entities.CashReceipt.ForwardedState = "";
    entities.CashReceipt.ForwardedZip5 = "";
    entities.CashReceipt.ForwardedZip4 = "";
    entities.CashReceipt.ForwardedZip3 = "";
    entities.CashReceipt.TotalCashTransactionAmount =
      totalCashTransactionAmount;
    entities.CashReceipt.TotalNoncashTransactionAmount =
      totalNoncashTransactionAmount;
    entities.CashReceipt.TotalCashTransactionCount = totalCashTransactionCount;
    entities.CashReceipt.TotalDetailAdjustmentCount =
      totalDetailAdjustmentCount;
    entities.CashReceipt.CreatedBy = createdBy;
    entities.CashReceipt.CreatedTimestamp = createdTimestamp;
    entities.CashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.CashReceipt.CashBalanceReason = cashBalanceReason;
    entities.CashReceipt.CashDue = cashDue;
    entities.CashReceipt.Note = note;
    entities.CashReceipt.PayorSocialSecurityNumber = payorSocialSecurityNumber;
    entities.CashReceipt.Populated = true;
  }

  private void CreateCashReceiptEvent()
  {
    var cstIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var receivedDate = import.CashReceiptEvent.ReceivedDate;
    var sourceCreationDate = import.CashReceiptEvent.SourceCreationDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var anticipatedCheckAmt =
      import.CashReceiptEvent.AnticipatedCheckAmt.GetValueOrDefault();
    var totalCashAmt = import.CashReceiptEvent.TotalCashAmt.GetValueOrDefault();

    entities.CashReceiptEvent.Populated = false;
    Update("CreateCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "creventId", systemGeneratedIdentifier);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "sourceCreationDt", sourceCreationDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableInt32(command, "totNonCshtrnCnt", 0);
        db.SetNullableDecimal(command, "totCashFeeAmt", 0M);
        db.SetNullableDecimal(command, "anticCheckAmt", anticipatedCheckAmt);
        db.SetNullableDecimal(command, "totalCashAmt", totalCashAmt);
      });

    entities.CashReceiptEvent.CstIdentifier = cstIdentifier;
    entities.CashReceiptEvent.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptEvent.ReceivedDate = receivedDate;
    entities.CashReceiptEvent.SourceCreationDate = sourceCreationDate;
    entities.CashReceiptEvent.CreatedBy = createdBy;
    entities.CashReceiptEvent.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptEvent.AnticipatedCheckAmt = anticipatedCheckAmt;
    entities.CashReceiptEvent.TotalCashAmt = totalCashAmt;
    entities.CashReceiptEvent.Populated = true;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var crsIdentifier = entities.CashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.MaximumDiscontinue.Date;

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
        db.SetNullableString(command, "reasonText", "");
      });

    entities.CashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.CashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptStatusHistory.CreatedBy = createdBy;
    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private bool ReadCashReceiptEvent()
  {
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptEvent.CreatedBy = db.GetString(reader, 4);
        entities.CashReceiptEvent.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CashReceiptEvent.AnticipatedCheckAmt =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceiptEvent.TotalCashAmt =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
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

  private bool ReadCashReceiptType()
  {
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
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
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of RetryCount.
    /// </summary>
    [JsonPropertyName("retryCount")]
    public Common RetryCount
    {
      get => retryCount ??= new();
      set => retryCount = value;
    }

    private DateWorkArea current;
    private DateWorkArea maximumDiscontinue;
    private ControlTable controlTable;
    private Common retryCount;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
  }
#endregion
}
