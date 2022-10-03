// Program: UPDATE_CASH_RECEIPT, ID: 371721895, model: 746.
// Short name: SWE01466
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_CASH_RECEIPT.
/// </summary>
[Serializable]
public partial class UpdateCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCashReceipt.
  /// </summary>
  public UpdateCashReceipt(IContext context, Import import, Export export):
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
    //                         MODIFICATION LOG
    // J Katz		09/23/98	Add in-code documentation.
    // 				Use local current date view in
    // 				Create and Update actions.
    // 				Set exit states for error conditions.
    // 				Removed unused views.
    // J Katz	06/03/99		Analyzed READ statements and
    // 				changed property to Select Only
    // 				where appropriate.
    // -----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Timestamp = Now();

    // ---------------------------------------------------
    // Read and Update Cash Receipt Event with imported
    // Received Date.
    // ---------------------------------------------------
    if (ReadCashReceiptEventCashReceiptSourceType())
    {
      ++export.ImportNumberOfReads.Count;

      if (!Equal(entities.CashReceiptEvent.ReceivedDate,
        import.CashReceiptEvent.ReceivedDate))
      {
        // ----------------------------------------------------------
        // Validate that the Source Type Code and the new Cash
        // Receipt Event Received Date are unique for the interface
        // receipt.    JLK  10/04/98
        // ----------------------------------------------------------
        if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) == 'Y'
          && Lt
          (local.Clear.Date, entities.CashReceiptEvent.SourceCreationDate))
        {
          // ---------------------------------------------------------
          // Read property set to Cursor Only.
          // If any record is found, do not allow the Cash Receipt
          // Event Received Date to be changed.
          // If no record is found, okay to continue processing.
          // JLK  06/03/99
          // ---------------------------------------------------------
          if (ReadCashReceiptEvent())
          {
            ExitState = "FN0000_INTF_AE_4_SOURCE_RCVD_DT";

            return;
          }
          else
          {
            // ---------------------------------------------------------
            // Interface Record does not exist for Source Code and
            // Received Date requested.  OK to update.   JLK  10/03/98
            // ---------------------------------------------------------
          }
        }

        try
        {
          UpdateCashReceiptEvent();
          ++export.ImportNumberOfUpdates.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0079_CASH_RCPT_EVENT_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_CASH_RECEIPT_EVENT_NF";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "FN0077_CASH_RCPT_EVENT_NF";

      return;
    }

    // ------------------------------------------------------------------
    // If import persistent Cash Receipt is not active, read Cash Receipt.
    // ------------------------------------------------------------------
    var condition = !import.Persistent.Populated;

    if (!condition)
    {
      // IS LOCKED expression is used.
      // Entity is considered to be locked during the call.
      condition = !true;
    }

    if (condition)
    {
      if (import.CashReceipt.SequentialNumber == 0)
      {
        // -----------------------------------------
        // Read receipt by the physical key.
        // -----------------------------------------
        if (ReadCashReceipt1())
        {
          ++export.ImportNumberOfReads.Count;
        }
        else
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }
      }
      else
      {
        // -----------------------------------------------------
        // Read receipt by the Cash Receipt Sequential Number.
        // -----------------------------------------------------
        if (ReadCashReceipt2())
        {
          ++export.ImportNumberOfReads.Count;
        }
        else
        {
          ExitState = "FN0084_CASH_RCPT_NF";

          return;
        }
      }
    }

    // *****
    // Save the prior amounts to determine if a Cash Receipt Audit
    // record must be made.
    // *****
    local.CashReceipt.ReceiptAmount = import.Persistent.ReceiptAmount;
    local.CashReceipt.TotalCashTransactionAmount =
      import.Persistent.TotalCashTransactionAmount;
    local.CashReceipt.TotalNoncashTransactionAmount =
      import.Persistent.TotalNoncashTransactionAmount;

    if (IsEmpty(import.Persistent.LastUpdatedBy))
    {
      local.CashReceipt.LastUpdatedBy = import.Persistent.CreatedBy;
      local.CashReceipt.LastUpdatedTimestamp =
        import.Persistent.CreatedTimestamp;
    }
    else
    {
      local.CashReceipt.LastUpdatedBy = import.Persistent.LastUpdatedBy;
      local.CashReceipt.LastUpdatedTimestamp =
        import.Persistent.LastUpdatedTimestamp;
    }

    // -----------------------------------------------------
    // Update Cash Receipt with imported values.
    // -----------------------------------------------------
    try
    {
      UpdateCashReceipt1();
      ++export.ImportNumberOfUpdates.Count;
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

    // -----------------------------------------------------
    // Create Cash Receipt Audit record, if necessary.
    // -----------------------------------------------------
    if (!Equal(import.Persistent.TotalCashTransactionAmount,
      local.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault()) || !
      Equal(import.Persistent.TotalNoncashTransactionAmount,
      local.CashReceipt.TotalNoncashTransactionAmount.GetValueOrDefault()) || import
      .Persistent.ReceiptAmount != local.CashReceipt.ReceiptAmount)
    {
      try
      {
        CreateCashReceiptAudit();
        ++export.ImportNumberOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0027_CASH_RCPT_AUDIT_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0029_CASH_RCPT_AUDIT_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateCashReceiptAudit()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var receiptAmount = local.CashReceipt.ReceiptAmount;
    var lastUpdatedTmst = local.CashReceipt.LastUpdatedTimestamp;
    var lastUpdatedBy = local.CashReceipt.LastUpdatedBy ?? "";
    var priorTransactionAmount =
      local.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault();
    var priorAdjustmentAmount =
      local.CashReceipt.TotalNoncashTransactionAmount.GetValueOrDefault();
    var crvIdentifier = import.Persistent.CrvIdentifier;
    var cstIdentifier = import.Persistent.CstIdentifier;
    var crtIdentifier = import.Persistent.CrtIdentifier;

    entities.CashReceiptAudit.Populated = false;
    Update("CreateCashReceiptAudit",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDecimal(command, "priorTransnAmt", priorTransactionAmount);
          
        db.SetDecimal(command, "priorAdjAmt", priorAdjustmentAmount);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
      });

    entities.CashReceiptAudit.ReceiptAmount = receiptAmount;
    entities.CashReceiptAudit.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptAudit.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptAudit.PriorTransactionAmount = priorTransactionAmount;
    entities.CashReceiptAudit.PriorAdjustmentAmount = priorAdjustmentAmount;
    entities.CashReceiptAudit.CrvIdentifier = crvIdentifier;
    entities.CashReceiptAudit.CstIdentifier = cstIdentifier;
    entities.CashReceiptAudit.CrtIdentifier = crtIdentifier;
    entities.CashReceiptAudit.Populated = true;
  }

  private bool ReadCashReceipt1()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceipt1",
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
        import.Persistent.CrvIdentifier = db.GetInt32(reader, 0);
        import.Persistent.CstIdentifier = db.GetInt32(reader, 1);
        import.Persistent.CrtIdentifier = db.GetInt32(reader, 2);
        import.Persistent.ReceiptAmount = db.GetDecimal(reader, 3);
        import.Persistent.SequentialNumber = db.GetInt32(reader, 4);
        import.Persistent.ReceiptDate = db.GetDate(reader, 5);
        import.Persistent.CheckType = db.GetNullableString(reader, 6);
        import.Persistent.CheckNumber = db.GetNullableString(reader, 7);
        import.Persistent.CheckDate = db.GetNullableDate(reader, 8);
        import.Persistent.ReceivedDate = db.GetDate(reader, 9);
        import.Persistent.DepositReleaseDate = db.GetNullableDate(reader, 10);
        import.Persistent.ReferenceNumber = db.GetNullableString(reader, 11);
        import.Persistent.PayorOrganization = db.GetNullableString(reader, 12);
        import.Persistent.PayorFirstName = db.GetNullableString(reader, 13);
        import.Persistent.PayorMiddleName = db.GetNullableString(reader, 14);
        import.Persistent.PayorLastName = db.GetNullableString(reader, 15);
        import.Persistent.ForwardedToName = db.GetNullableString(reader, 16);
        import.Persistent.ForwardedStreet1 = db.GetNullableString(reader, 17);
        import.Persistent.ForwardedStreet2 = db.GetNullableString(reader, 18);
        import.Persistent.ForwardedCity = db.GetNullableString(reader, 19);
        import.Persistent.ForwardedState = db.GetNullableString(reader, 20);
        import.Persistent.ForwardedZip5 = db.GetNullableString(reader, 21);
        import.Persistent.ForwardedZip4 = db.GetNullableString(reader, 22);
        import.Persistent.ForwardedZip3 = db.GetNullableString(reader, 23);
        import.Persistent.BalancedTimestamp =
          db.GetNullableDateTime(reader, 24);
        import.Persistent.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 25);
        import.Persistent.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 26);
        import.Persistent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 27);
        import.Persistent.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 28);
        import.Persistent.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 29);
        import.Persistent.CreatedBy = db.GetString(reader, 30);
        import.Persistent.CreatedTimestamp = db.GetDateTime(reader, 31);
        import.Persistent.CashBalanceAmt = db.GetNullableDecimal(reader, 32);
        import.Persistent.CashBalanceReason = db.GetNullableString(reader, 33);
        import.Persistent.CashDue = db.GetNullableDecimal(reader, 34);
        import.Persistent.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 35);
        import.Persistent.LastUpdatedBy = db.GetNullableString(reader, 36);
        import.Persistent.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 37);
        import.Persistent.Note = db.GetNullableString(reader, 38);
        import.Persistent.PayorSocialSecurityNumber =
          db.GetNullableString(reader, 39);
        import.Persistent.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          import.Persistent.CashBalanceReason);
      });
  }

  private bool ReadCashReceipt2()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.Persistent.CrvIdentifier = db.GetInt32(reader, 0);
        import.Persistent.CstIdentifier = db.GetInt32(reader, 1);
        import.Persistent.CrtIdentifier = db.GetInt32(reader, 2);
        import.Persistent.ReceiptAmount = db.GetDecimal(reader, 3);
        import.Persistent.SequentialNumber = db.GetInt32(reader, 4);
        import.Persistent.ReceiptDate = db.GetDate(reader, 5);
        import.Persistent.CheckType = db.GetNullableString(reader, 6);
        import.Persistent.CheckNumber = db.GetNullableString(reader, 7);
        import.Persistent.CheckDate = db.GetNullableDate(reader, 8);
        import.Persistent.ReceivedDate = db.GetDate(reader, 9);
        import.Persistent.DepositReleaseDate = db.GetNullableDate(reader, 10);
        import.Persistent.ReferenceNumber = db.GetNullableString(reader, 11);
        import.Persistent.PayorOrganization = db.GetNullableString(reader, 12);
        import.Persistent.PayorFirstName = db.GetNullableString(reader, 13);
        import.Persistent.PayorMiddleName = db.GetNullableString(reader, 14);
        import.Persistent.PayorLastName = db.GetNullableString(reader, 15);
        import.Persistent.ForwardedToName = db.GetNullableString(reader, 16);
        import.Persistent.ForwardedStreet1 = db.GetNullableString(reader, 17);
        import.Persistent.ForwardedStreet2 = db.GetNullableString(reader, 18);
        import.Persistent.ForwardedCity = db.GetNullableString(reader, 19);
        import.Persistent.ForwardedState = db.GetNullableString(reader, 20);
        import.Persistent.ForwardedZip5 = db.GetNullableString(reader, 21);
        import.Persistent.ForwardedZip4 = db.GetNullableString(reader, 22);
        import.Persistent.ForwardedZip3 = db.GetNullableString(reader, 23);
        import.Persistent.BalancedTimestamp =
          db.GetNullableDateTime(reader, 24);
        import.Persistent.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 25);
        import.Persistent.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 26);
        import.Persistent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 27);
        import.Persistent.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 28);
        import.Persistent.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 29);
        import.Persistent.CreatedBy = db.GetString(reader, 30);
        import.Persistent.CreatedTimestamp = db.GetDateTime(reader, 31);
        import.Persistent.CashBalanceAmt = db.GetNullableDecimal(reader, 32);
        import.Persistent.CashBalanceReason = db.GetNullableString(reader, 33);
        import.Persistent.CashDue = db.GetNullableDecimal(reader, 34);
        import.Persistent.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 35);
        import.Persistent.LastUpdatedBy = db.GetNullableString(reader, 36);
        import.Persistent.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 37);
        import.Persistent.Note = db.GetNullableString(reader, 38);
        import.Persistent.PayorSocialSecurityNumber =
          db.GetNullableString(reader, 39);
        import.Persistent.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          import.Persistent.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptEvent()
  {
    entities.ExistingInterface.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "receivedDate",
          import.CashReceiptEvent.ReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInterface.CstIdentifier = db.GetInt32(reader, 0);
        entities.ExistingInterface.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingInterface.ReceivedDate = db.GetDate(reader, 2);
        entities.ExistingInterface.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptEvent.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.CashReceiptEvent.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 6);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private void UpdateCashReceipt1()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var receiptAmount = import.CashReceipt.ReceiptAmount;
    var receiptDate = import.CashReceipt.ReceiptDate;
    var checkType = import.CashReceipt.CheckType ?? "";
    var checkNumber = import.CashReceipt.CheckNumber ?? "";
    var checkDate = import.CashReceipt.CheckDate;
    var receivedDate = import.CashReceipt.ReceivedDate;
    var depositReleaseDate = import.CashReceipt.DepositReleaseDate;
    var referenceNumber = import.CashReceipt.ReferenceNumber ?? "";
    var payorOrganization = import.CashReceipt.PayorOrganization ?? "";
    var payorFirstName = import.CashReceipt.PayorFirstName ?? "";
    var payorMiddleName = import.CashReceipt.PayorMiddleName ?? "";
    var payorLastName = import.CashReceipt.PayorLastName ?? "";
    var forwardedToName = import.CashReceipt.ForwardedToName ?? "";
    var forwardedStreet1 = import.CashReceipt.ForwardedStreet1 ?? "";
    var forwardedStreet2 = import.CashReceipt.ForwardedStreet2 ?? "";
    var forwardedCity = import.CashReceipt.ForwardedCity ?? "";
    var forwardedState = import.CashReceipt.ForwardedState ?? "";
    var forwardedZip5 = import.CashReceipt.ForwardedZip5 ?? "";
    var forwardedZip4 = import.CashReceipt.ForwardedZip4 ?? "";
    var forwardedZip3 = import.CashReceipt.ForwardedZip3 ?? "";
    var balancedTimestamp = import.CashReceipt.BalancedTimestamp;
    var totalCashTransactionAmount =
      import.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault();
    var totalNoncashTransactionAmount =
      import.CashReceipt.TotalNoncashTransactionAmount.GetValueOrDefault();
    var totalCashTransactionCount =
      import.CashReceipt.TotalCashTransactionCount.GetValueOrDefault();
    var totalNoncashTransactionCount =
      import.CashReceipt.TotalNoncashTransactionCount.GetValueOrDefault();
    var totalDetailAdjustmentCount =
      import.CashReceipt.TotalDetailAdjustmentCount.GetValueOrDefault();
    var cashBalanceAmt = import.CashReceipt.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = import.CashReceipt.CashBalanceReason ?? "";
    var cashDue = import.CashReceipt.CashDue.GetValueOrDefault();
    var totalNonCashFeeAmount =
      import.CashReceipt.TotalNonCashFeeAmount.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var note = import.CashReceipt.Note ?? "";
    var payorSocialSecurityNumber =
      import.CashReceipt.PayorSocialSecurityNumber ?? "";

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    import.Persistent.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDate(command, "receiptDate", receiptDate);
        db.SetNullableString(command, "checkType", checkType);
        db.SetNullableString(command, "checkNumber", checkNumber);
        db.SetNullableDate(command, "checkDate", checkDate);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetNullableString(command, "payorOrganization", payorOrganization);
        db.SetNullableString(command, "payorFirstName", payorFirstName);
        db.SetNullableString(command, "payorMiddleName", payorMiddleName);
        db.SetNullableString(command, "payorLastName", payorLastName);
        db.SetNullableString(command, "frwrdToName", forwardedToName);
        db.SetNullableString(command, "frwrdStreet1", forwardedStreet1);
        db.SetNullableString(command, "frwrdStreet2", forwardedStreet2);
        db.SetNullableString(command, "frwrdCity", forwardedCity);
        db.SetNullableString(command, "frwrdState", forwardedState);
        db.SetNullableString(command, "frwrdZip5", forwardedZip5);
        db.SetNullableString(command, "frwrdZip4", forwardedZip4);
        db.SetNullableString(command, "frwrdZip3", forwardedZip3);
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetNullableDecimal(
          command, "totalCashTransac", totalCashTransactionAmount);
        db.SetNullableDecimal(
          command, "totNoncshTrnAmt", totalNoncashTransactionAmount);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableInt32(
          command, "totNocshTranCnt", totalNoncashTransactionCount);
        db.SetNullableInt32(
          command, "totDetailAdjCnt", totalDetailAdjustmentCount);
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableDecimal(command, "cashDue", cashDue);
        db.SetNullableDecimal(command, "totalNcFeeAmt", totalNonCashFeeAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "payorSsn", payorSocialSecurityNumber);
        db.SetInt32(command, "crvIdentifier", import.Persistent.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", import.Persistent.CstIdentifier);
        db.SetInt32(command, "crtIdentifier", import.Persistent.CrtIdentifier);
      });

    import.Persistent.ReceiptAmount = receiptAmount;
    import.Persistent.ReceiptDate = receiptDate;
    import.Persistent.CheckType = checkType;
    import.Persistent.CheckNumber = checkNumber;
    import.Persistent.CheckDate = checkDate;
    import.Persistent.ReceivedDate = receivedDate;
    import.Persistent.DepositReleaseDate = depositReleaseDate;
    import.Persistent.ReferenceNumber = referenceNumber;
    import.Persistent.PayorOrganization = payorOrganization;
    import.Persistent.PayorFirstName = payorFirstName;
    import.Persistent.PayorMiddleName = payorMiddleName;
    import.Persistent.PayorLastName = payorLastName;
    import.Persistent.ForwardedToName = forwardedToName;
    import.Persistent.ForwardedStreet1 = forwardedStreet1;
    import.Persistent.ForwardedStreet2 = forwardedStreet2;
    import.Persistent.ForwardedCity = forwardedCity;
    import.Persistent.ForwardedState = forwardedState;
    import.Persistent.ForwardedZip5 = forwardedZip5;
    import.Persistent.ForwardedZip4 = forwardedZip4;
    import.Persistent.ForwardedZip3 = forwardedZip3;
    import.Persistent.BalancedTimestamp = balancedTimestamp;
    import.Persistent.TotalCashTransactionAmount = totalCashTransactionAmount;
    import.Persistent.TotalNoncashTransactionAmount =
      totalNoncashTransactionAmount;
    import.Persistent.TotalCashTransactionCount = totalCashTransactionCount;
    import.Persistent.TotalNoncashTransactionCount =
      totalNoncashTransactionCount;
    import.Persistent.TotalDetailAdjustmentCount = totalDetailAdjustmentCount;
    import.Persistent.CashBalanceAmt = cashBalanceAmt;
    import.Persistent.CashBalanceReason = cashBalanceReason;
    import.Persistent.CashDue = cashDue;
    import.Persistent.TotalNonCashFeeAmount = totalNonCashFeeAmount;
    import.Persistent.LastUpdatedBy = lastUpdatedBy;
    import.Persistent.LastUpdatedTimestamp = lastUpdatedTimestamp;
    import.Persistent.Note = note;
    import.Persistent.PayorSocialSecurityNumber = payorSocialSecurityNumber;
    import.Persistent.Populated = true;
  }

  private void UpdateCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);

    var receivedDate = import.CashReceiptEvent.ReceivedDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;

    entities.CashReceiptEvent.Populated = false;
    Update("UpdateCashReceiptEvent",
      (db, command) =>
      {
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptEvent.CstIdentifier);
        db.SetInt32(
          command, "creventId",
          entities.CashReceiptEvent.SystemGeneratedIdentifier);
      });

    entities.CashReceiptEvent.ReceivedDate = receivedDate;
    entities.CashReceiptEvent.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptEvent.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptEvent.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceipt Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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

    private CashReceipt persistent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public DateWorkArea Clear
    {
      get => clear ??= new();
      set => clear = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public CashReceipt Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DateWorkArea current;
    private DateWorkArea clear;
    private CashReceipt cashReceipt;
    private CashReceipt initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptAudit.
    /// </summary>
    [JsonPropertyName("cashReceiptAudit")]
    public CashReceiptAudit CashReceiptAudit
    {
      get => cashReceiptAudit ??= new();
      set => cashReceiptAudit = value;
    }

    /// <summary>
    /// A value of ExistingInterface.
    /// </summary>
    [JsonPropertyName("existingInterface")]
    public CashReceiptEvent ExistingInterface
    {
      get => existingInterface ??= new();
      set => existingInterface = value;
    }

    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptAudit cashReceiptAudit;
    private CashReceiptEvent existingInterface;
  }
#endregion
}
