// Program: FN_FORWARD_CASH_RCPT, ID: 371725653, model: 746.
// Short name: SWE00460
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_FORWARD_CASH_RCPT.
/// </summary>
[Serializable]
public partial class FnForwardCashRcpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_FORWARD_CASH_RCPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnForwardCashRcpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnForwardCashRcpt.
  /// </summary>
  public FnForwardCashRcpt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE		DESCRIPTION
    // ---------	---------     -----------------------
    // Ty Hill-MTW	04/29/97	Change Current_date
    // J Katz		09/22/98	Modify edits to allow receipts
    // 				with receipt types of check,
    // 				Money Order, or Interfund
    // 					to be forwarded.
    // 				Add logic to disallow forwarding
    // 				action if the imported receipt
    // 				has details.
    // 				Removed unused views and
    // 				duplicate code.
    // J Katz		06/08/99	Analyze READ statements and
    // 				change read property to Select
    // 				Only where appropriate.
    // ************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // --------------------------------------------------
    // Get cash receipt status hard code values for
    // recorded and forwarded.
    // Get cash receipt type hard code value for check
    // --------------------------------------------------
    UseFnHardcodedCashReceipting();

    // ------------------------------------------------------------------
    // The following statements are required for additional cash
    // receipting hardcoded values which are not currently in the
    // FN Hardcoded Cash Receipting CAB.
    // ------------------------------------------------------------------
    local.HardcodedCrtMoneyOrder.SystemGeneratedIdentifier = 3;
    local.HardcodedCrtInterfund.SystemGeneratedIdentifier = 10;

    // ------------------------------------------------------------
    // Read Cash Receipt record to be forwarded or updated
    // ------------------------------------------------------------
    if (ReadCashReceiptCashReceiptType())
    {
      // ------------------------------------------------------------
      // Can only forward receipts paid by check or money order
      // (problem # 25203)
      // Can also forward interfund type receipts per Lori Glissman.
      // JLK  09/21/98
      // ------------------------------------------------------------
      if (entities.CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtCheck.SystemGeneratedIdentifier || entities
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtMoneyOrder.SystemGeneratedIdentifier || entities
        .CashReceiptType.SystemGeneratedIdentifier == local
        .HardcodedCrtInterfund.SystemGeneratedIdentifier)
      {
        // --------------------------------------------------------
        // Cash receipt type is valid for Forwarding action.
        // --------------------------------------------------------
      }
      else
      {
        ExitState = "FN0000_INVALID_CR_TYPE_4_FWDING";

        return;
      }
    }
    else
    {
      ExitState = "FN0084_CASH_RCPT_NF";
    }

    // --------------------------------------------------------
    // Determine if active Cash Receipt status is valid for
    // forwarding (Bal) or updating (Fwd).
    // --------------------------------------------------------
    ReadCashReceiptStatusHistory();

    if (ReadCashReceiptStatus2())
    {
      MoveCashReceiptStatus(entities.CashReceiptStatus, export.CashReceiptStatus);
        

      if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
        .HardcodedCrsForwarded.SystemGeneratedIdentifier)
      {
        export.CashReceiptStatusHistory.
          Assign(entities.CashReceiptStatusHistory);
      }
    }
    else
    {
      ExitState = "FN0108_CASH_RCPT_STAT_NF";

      return;
    }

    // -----------------------------------------------------------
    // Receipt must have a status of BALANCED to forward or
    // FORWARDED to update forwarding information.
    // A Receipt with a status of FORWARDED which was created
    // on the current date can update the forwarding information.
    // The existing Cash Receipt Satus History record is updated
    // with the Note text and the existing Cash Receipt record is
    // updated with the changes to Forwarding Address information.
    // -----------------------------------------------------------
    if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
      .HardcodedCrsBalanced.SystemGeneratedIdentifier)
    {
      // --------------------------------------------------------
      // Cannot forward a cash receipt if the receipt has details.
      // --------------------------------------------------------
      ReadCashReceiptDetail();

      if (local.CrDetailsExist.Count > 0)
      {
        ExitState = "FN0000_CANNOT_FWD_CR_WITH_DETAIL";

        return;
      }

      // --------------------------------------------------------
      // Update Cash Receipt with forwarding information and
      // maintain the Cash Receipt Status History records.
      // --------------------------------------------------------
      try
      {
        UpdateCashReceipt2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0088_CASH_RCPT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0090_CASH_RCPT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // --------------------------------------------------------
      // End date the active Cash Receipt Status History record.
      // --------------------------------------------------------
      try
      {
        UpdateCashReceiptStatusHistory1();

        if (ReadCashReceiptStatus1())
        {
          // --------------------------------------------------------
          // Create a new active Cash Receipt Status History record
          // with a status of FWD.
          // --------------------------------------------------------
          UseFnCreateCashRcptStatHist();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveCashReceiptStatus(entities.CashReceiptStatus,
              export.CashReceiptStatus);
            ExitState = "FN0000_FORWARD_SUCCESSFUL";
          }
        }
        else
        {
          ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0106_CASH_RCPT_STAT_HIST_NU_RB";

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
    else if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
      .HardcodedCrsForwarded.SystemGeneratedIdentifier)
    {
      // --------------------------------------------------------
      // Update Cash Receipt with forwarding information and
      // maintain the Cash Receipt Status History records.
      // Removed edit requiring that Update actions only occur on
      // the same date that the Cash Receipt was placed in FWD
      // status.      JLK    11/03/98
      // --------------------------------------------------------
      try
      {
        UpdateCashReceipt1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0088_CASH_RCPT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0090_CASH_RCPT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        UpdateCashReceiptStatusHistory2();
        export.CashReceiptStatusHistory.
          Assign(entities.CashReceiptStatusHistory);
        export.ForwardedAddress.LastUpdatedTimestamp =
          entities.CashReceipt.LastUpdatedTimestamp;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASH_RCPT_STATUS_HISTORY_NU_W_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASH_RCPT_STATUS_HISTORY_PV_W_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "INVALID_STATUS_FOR_FORWARDING";
    }
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseFnCreateCashRcptStatHist()
  {
    var useImport = new FnCreateCashRcptStatHist.Import();
    var useExport = new FnCreateCashRcptStatHist.Export();

    useImport.PersistentCashReceiptStatus.Assign(entities.CashReceiptStatus);
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptStatusHistory.ReasonText =
      import.CashReceiptStatusHistory.ReasonText;

    Call(FnCreateCashRcptStatHist.Execute, useImport, useExport);

    MoveCashReceiptStatus(useImport.PersistentCashReceiptStatus,
      entities.CashReceiptStatus);
    export.CashReceiptStatusHistory.Assign(useExport.CashReceiptStatusHistory);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsBalanced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdBalanced.SystemGeneratedIdentifier;
    local.HardcodedCrsForwarded.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdForwarded.SystemGeneratedIdentifier;
    local.HardcodedCrtCheck.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdCheck.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceiptCashReceiptType()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptCashReceiptType",
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
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ForwardedToName = db.GetNullableString(reader, 4);
        entities.CashReceipt.ForwardedStreet1 = db.GetNullableString(reader, 5);
        entities.CashReceipt.ForwardedStreet2 = db.GetNullableString(reader, 6);
        entities.CashReceipt.ForwardedCity = db.GetNullableString(reader, 7);
        entities.CashReceipt.ForwardedState = db.GetNullableString(reader, 8);
        entities.CashReceipt.ForwardedZip5 = db.GetNullableString(reader, 9);
        entities.CashReceipt.ForwardedZip4 = db.GetNullableString(reader, 10);
        entities.CashReceipt.ForwardedZip3 = db.GetNullableString(reader, 11);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        local.CrDetailsExist.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceiptStatus1()
  {
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsForwarded.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus2()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.CashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
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
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.CashReceiptStatusHistory.Populated = true;
      });
  }

  private void UpdateCashReceipt1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var forwardedToName = import.CashReceipt.ForwardedToName ?? "";
    var forwardedStreet1 = import.CashReceipt.ForwardedStreet1 ?? "";
    var forwardedStreet2 = import.CashReceipt.ForwardedStreet2 ?? "";
    var forwardedCity = import.CashReceipt.ForwardedCity ?? "";
    var forwardedState = import.CashReceipt.ForwardedState ?? "";
    var forwardedZip5 = import.CashReceipt.ForwardedZip5 ?? "";
    var forwardedZip4 = import.CashReceipt.ForwardedZip4 ?? "";
    var forwardedZip3 = entities.CashReceipt.ForwardedZip3;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt1",
      (db, command) =>
      {
        db.SetNullableString(command, "frwrdToName", forwardedToName);
        db.SetNullableString(command, "frwrdStreet1", forwardedStreet1);
        db.SetNullableString(command, "frwrdStreet2", forwardedStreet2);
        db.SetNullableString(command, "frwrdCity", forwardedCity);
        db.SetNullableString(command, "frwrdState", forwardedState);
        db.SetNullableString(command, "frwrdZip5", forwardedZip5);
        db.SetNullableString(command, "frwrdZip4", forwardedZip4);
        db.SetNullableString(command, "frwrdZip3", forwardedZip3);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.ForwardedToName = forwardedToName;
    entities.CashReceipt.ForwardedStreet1 = forwardedStreet1;
    entities.CashReceipt.ForwardedStreet2 = forwardedStreet2;
    entities.CashReceipt.ForwardedCity = forwardedCity;
    entities.CashReceipt.ForwardedState = forwardedState;
    entities.CashReceipt.ForwardedZip5 = forwardedZip5;
    entities.CashReceipt.ForwardedZip4 = forwardedZip4;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateCashReceipt2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var forwardedToName = import.CashReceipt.ForwardedToName ?? "";
    var forwardedStreet1 = import.CashReceipt.ForwardedStreet1 ?? "";
    var forwardedStreet2 = import.CashReceipt.ForwardedStreet2 ?? "";
    var forwardedCity = import.CashReceipt.ForwardedCity ?? "";
    var forwardedState = import.CashReceipt.ForwardedState ?? "";
    var forwardedZip5 = import.CashReceipt.ForwardedZip5 ?? "";
    var forwardedZip4 = import.CashReceipt.ForwardedZip4 ?? "";
    var forwardedZip3 = import.CashReceipt.ForwardedZip3 ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt2",
      (db, command) =>
      {
        db.SetNullableString(command, "frwrdToName", forwardedToName);
        db.SetNullableString(command, "frwrdStreet1", forwardedStreet1);
        db.SetNullableString(command, "frwrdStreet2", forwardedStreet2);
        db.SetNullableString(command, "frwrdCity", forwardedCity);
        db.SetNullableString(command, "frwrdState", forwardedState);
        db.SetNullableString(command, "frwrdZip5", forwardedZip5);
        db.SetNullableString(command, "frwrdZip4", forwardedZip4);
        db.SetNullableString(command, "frwrdZip3", forwardedZip3);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.ForwardedToName = forwardedToName;
    entities.CashReceipt.ForwardedStreet1 = forwardedStreet1;
    entities.CashReceipt.ForwardedStreet2 = forwardedStreet2;
    entities.CashReceipt.ForwardedCity = forwardedCity;
    entities.CashReceipt.ForwardedState = forwardedState;
    entities.CashReceipt.ForwardedZip5 = forwardedZip5;
    entities.CashReceipt.ForwardedZip4 = forwardedZip4;
    entities.CashReceipt.ForwardedZip3 = forwardedZip3;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateCashReceiptStatusHistory1()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.CashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory1",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.CashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private void UpdateCashReceiptStatusHistory2()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);

    var createdBy = global.UserId;
    var reasonText = import.CashReceiptStatusHistory.ReasonText ?? "";

    entities.CashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory2",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.CashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptStatusHistory.CreatedBy = createdBy;
    entities.CashReceiptStatusHistory.ReasonText = reasonText;
    entities.CashReceiptStatusHistory.Populated = true;
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
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
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

    private CashReceipt cashReceipt;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
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
    /// A value of ForwardedAddress.
    /// </summary>
    [JsonPropertyName("forwardedAddress")]
    public CashReceipt ForwardedAddress
    {
      get => forwardedAddress ??= new();
      set => forwardedAddress = value;
    }

    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceipt forwardedAddress;
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
    /// A value of HardcodedCrsBalanced.
    /// </summary>
    [JsonPropertyName("hardcodedCrsBalanced")]
    public CashReceiptStatus HardcodedCrsBalanced
    {
      get => hardcodedCrsBalanced ??= new();
      set => hardcodedCrsBalanced = value;
    }

    /// <summary>
    /// A value of HardcodedCrsForwarded.
    /// </summary>
    [JsonPropertyName("hardcodedCrsForwarded")]
    public CashReceiptStatus HardcodedCrsForwarded
    {
      get => hardcodedCrsForwarded ??= new();
      set => hardcodedCrsForwarded = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCheck.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCheck")]
    public CashReceiptType HardcodedCrtCheck
    {
      get => hardcodedCrtCheck ??= new();
      set => hardcodedCrtCheck = value;
    }

    /// <summary>
    /// A value of HardcodedCrtMoneyOrder.
    /// </summary>
    [JsonPropertyName("hardcodedCrtMoneyOrder")]
    public CashReceiptType HardcodedCrtMoneyOrder
    {
      get => hardcodedCrtMoneyOrder ??= new();
      set => hardcodedCrtMoneyOrder = value;
    }

    /// <summary>
    /// A value of HardcodedCrtInterfund.
    /// </summary>
    [JsonPropertyName("hardcodedCrtInterfund")]
    public CashReceiptType HardcodedCrtInterfund
    {
      get => hardcodedCrtInterfund ??= new();
      set => hardcodedCrtInterfund = value;
    }

    /// <summary>
    /// A value of CrDetailsExist.
    /// </summary>
    [JsonPropertyName("crDetailsExist")]
    public Common CrDetailsExist
    {
      get => crDetailsExist ??= new();
      set => crDetailsExist = value;
    }

    private DateWorkArea current;
    private CashReceiptStatus hardcodedCrsBalanced;
    private CashReceiptStatus hardcodedCrsForwarded;
    private CashReceiptType hardcodedCrtCheck;
    private CashReceiptType hardcodedCrtMoneyOrder;
    private CashReceiptType hardcodedCrtInterfund;
    private Common crDetailsExist;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}
