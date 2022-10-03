// Program: FN_REMOVE_CASH_RCPT_FROM_DEPOSIT, ID: 371725979, model: 746.
// Short name: SWE00597
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REMOVE_CASH_RCPT_FROM_DEPOSIT.
/// </summary>
[Serializable]
public partial class FnRemoveCashRcptFromDeposit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REMOVE_CASH_RCPT_FROM_DEPOSIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRemoveCashRcptFromDeposit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRemoveCashRcptFromDeposit.
  /// </summary>
  public FnRemoveCashRcptFromDeposit(IContext context, Import import,
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
    // --------------------------------------------------------------------------
    // Date	    Developer	 	Description
    // -----      ------------------	-------------------
    // 03/25/96    Holly Kennedy-MTW	Added logic to insure that the status
    // 				of the Deposit is Open before allowing
    // 				the user to remove it from the deposit.
    // 10/20/98    J. Katz - SRG	End date active Cash Receipt Status
    // 				History record before creating new
    // 				Status History record.
    // 10/24/98    J. Katz - SRG	Remove logic preventing non-CSE type
    // 				receipts from being removed from a
    // 				deposit.
    // 				Include SET statements for Last Updated
    // 				By and Last Updated Timestamp when
    // 				updating a Fund Transaction Record.
    // 02/27/99    J Katz - SRG	After removing a receipt from the
    // 				deposit, receipt should be placed in
    // 				REC status instead of BAL status.
    // 09/10/99	J Katz - SRG	Allow interface receipts to be removed
    // 				from the deposit so that the CRMI
    // 				Unmatch action can be implemented
    // 				per Heat problem report # H 00073153.
    // --------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // --------------------------------------------------------------
    // Set up local views including dates and hardcoded values.
    // --------------------------------------------------------------
    local.Low.Date = null;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    UseHardcodedFundingInformation();
    UseFnHardcodedCashReceipting();

    // --------------------------------------------------------------
    // Read Cash Receipt information.
    // --------------------------------------------------------------
    if (ReadCashReceipt())
    {
      // --->  ok to continue
    }
    else
    {
      ExitState = "FN0086_CASH_RCPT_NF_RB";

      return;
    }

    if (ReadCashReceiptEvent())
    {
      // --->  ok to continue
    }
    else
    {
      ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";

      return;
    }

    // --------------------------------------------------------------
    // Interface receipts can be removed from the deposit to allow
    //  the user to unmatch the receipt on CRMI.  JLK  09/10/99
    // --------------------------------------------------------------
    if (Lt(local.Low.Date, entities.ExistingCashReceiptEvent.SourceCreationDate))
      
    {
    }
    else
    {
      ReadCashReceiptDetail();

      if (local.CrDetails.Count > 0)
      {
        ExitState = "FN0000_DENY_ACTION_DET_EXIST_RB";

        return;
      }
    }

    if (!ReadCashReceiptStatus2())
    {
      ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

      return;
    }

    if (!ReadCashReceiptStatusHistory())
    {
      ExitState = "FN0103_CASH_RCPT_STAT_HIST_NF_RB";

      return;
    }

    if (ReadCashReceiptStatus1())
    {
      if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsDeposited.SystemGeneratedIdentifier)
      {
        ExitState = "FN0107_CASH_RCPT_STA_INV_4_RQ_RB";

        return;
      }
    }
    else
    {
      ExitState = "FN0103_CASH_RCPT_STAT_HIST_NF_RB";

      return;
    }

    // --------------------------------------------------------------
    // Read Deposit information.
    // --------------------------------------------------------------
    if (!ReadFundTransaction())
    {
      ExitState = "FN0000_FUND_TRANS_NF_RB";

      return;
    }

    ReadFundTransactionStatusFundTransactionStatusHistory();

    if (entities.FundTransactionStatus.SystemGeneratedIdentifier != local
      .FtsOpen.SystemGeneratedIdentifier)
    {
      ExitState = "FN0000_FUND_TRNS_STAT_NOT_OPN_RB";

      return;
    }

    // --------------------------------------------------------------
    // Remove current cash receipt from deposit and reduce the Fund
    // Transaction Amount by the amount of the cash receipt.
    // --------------------------------------------------------------
    local.New1.Amount = entities.FundTransaction.Amount - entities
      .CashReceipt.ReceiptAmount;

    try
    {
      UpdateFundTransaction();

      // O.k., continue.
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_FUND_TRANS_NU_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_FUND_TRANS_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // --------------------------------------------------------------
    // Set the Cash Receipt Deposit Release Date to null.
    // --------------------------------------------------------------
    try
    {
      UpdateCashReceipt();

      // O.k., continue.
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0089_CASH_RCPT_NU_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0091_CASH_RCPT_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // --------------------------------------------------------------
    // Change status of current cash receipt from DEP to BAL.
    // --------------------------------------------------------------
    try
    {
      UpdateCashReceiptStatusHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASH_RCPT_STATUS_HISTORY_NU_W_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASH_RCPT_STATUS_HISTORY_PV_W_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    UseFnCreateCashRcptStatHist();
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreateCashRcptStatHist()
  {
    var useImport = new FnCreateCashRcptStatHist.Import();
    var useExport = new FnCreateCashRcptStatHist.Export();

    useImport.PersistentCashReceiptStatus.Assign(entities.Receipted);
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;

    Call(FnCreateCashRcptStatHist.Execute, useImport, useExport);

    MoveCashReceiptStatus(useImport.PersistentCashReceiptStatus,
      entities.Receipted);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrsDeposited.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
  }

  private void UseHardcodedFundingInformation()
  {
    var useImport = new HardcodedFundingInformation.Import();
    var useExport = new HardcodedFundingInformation.Export();

    Call(HardcodedFundingInformation.Execute, useImport, useExport);

    local.FtsOpen.SystemGeneratedIdentifier =
      useExport.Open.SystemGeneratedIdentifier;
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
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 5);
        entities.CashReceipt.DepositReleaseDate = db.GetNullableDate(reader, 6);
        entities.CashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CashReceipt.FttIdentifier = db.GetNullableInt32(reader, 8);
        entities.CashReceipt.PcaCode = db.GetNullableString(reader, 9);
        entities.CashReceipt.PcaEffectiveDate = db.GetNullableDate(reader, 10);
        entities.CashReceipt.FunIdentifier = db.GetNullableInt32(reader, 11);
        entities.CashReceipt.FtrIdentifier = db.GetNullableInt32(reader, 12);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CashReceipt.Populated = true;
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
        local.CrDetails.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ExistingCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ActiveCashReceiptStatusHistory.Populated);
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.ActiveCashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus2()
  {
    entities.Receipted.Populated = false;

    return Read("ReadCashReceiptStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsReceipted.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Receipted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Receipted.Code = db.GetString(reader, 1);
        entities.Receipted.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
      });
  }

  private bool ReadFundTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransId",
          entities.CashReceipt.FtrIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier",
          entities.CashReceipt.FunIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "fttIdentifier",
          entities.CashReceipt.FttIdentifier.GetValueOrDefault());
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.CashReceipt.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "pcaCode", entities.CashReceipt.PcaCode ?? "");
      },
      (db, reader) =>
      {
        entities.FundTransaction.FttIdentifier = db.GetInt32(reader, 0);
        entities.FundTransaction.PcaCode = db.GetString(reader, 1);
        entities.FundTransaction.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.FundTransaction.FunIdentifier = db.GetInt32(reader, 3);
        entities.FundTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransaction.DepositNumber = db.GetNullableInt32(reader, 5);
        entities.FundTransaction.Amount = db.GetDecimal(reader, 6);
        entities.FundTransaction.BusinessDate = db.GetDate(reader, 7);
        entities.FundTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.FundTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.FundTransaction.Populated = true;
      });
  }

  private bool ReadFundTransactionStatusFundTransactionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.FundTransactionStatusHistory.Populated = false;
    entities.FundTransactionStatus.Populated = false;

    return Read("ReadFundTransactionStatusFundTransactionStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "pcaCode", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FtsIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FtrIdentifier =
          db.GetInt32(reader, 1);
        entities.FundTransactionStatusHistory.FunIdentifier =
          db.GetInt32(reader, 2);
        entities.FundTransactionStatusHistory.PcaEffectiveDate =
          db.GetDate(reader, 3);
        entities.FundTransactionStatusHistory.PcaCode = db.GetString(reader, 4);
        entities.FundTransactionStatusHistory.FttIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatusHistory.EffectiveTmst =
          db.GetDateTime(reader, 6);
        entities.FundTransactionStatusHistory.Populated = true;
        entities.FundTransactionStatus.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var depositReleaseDate = local.Clear.Date;
    var balancedTimestamp = local.Clear.Timestamp;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
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

    entities.CashReceipt.DepositReleaseDate = depositReleaseDate;
    entities.CashReceipt.BalancedTimestamp = balancedTimestamp;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.ActiveCashReceiptStatusHistory.Populated);

    var discontinueDate = local.Current.Date;

    entities.ActiveCashReceiptStatusHistory.Populated = false;
    Update("UpdateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ActiveCashReceiptStatusHistory.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrvIdentifier);
        db.SetInt32(
          command, "crsIdentifier",
          entities.ActiveCashReceiptStatusHistory.CrsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ActiveCashReceiptStatusHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.ActiveCashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.ActiveCashReceiptStatusHistory.Populated = true;
  }

  private void UpdateFundTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var amount = local.New1.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;

    entities.CashReceipt.Populated = false;
    entities.FundTransaction.Populated = false;

    bool exists;

    Update("UpdateFundTransaction#1",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "fttIdentifier1", entities.FundTransaction.FttIdentifier);
        db.SetString(command, "pcaCode1", entities.FundTransaction.PcaCode);
        db.SetDate(
          command, "pcaEffectiveDate1",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier1", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fundTransId",
          entities.FundTransaction.SystemGeneratedIdentifier);
      });

    Update("UpdateFundTransaction#2",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier1", crvIdentifier);
        db.SetInt32(command, "cstIdentifier1", cstIdentifier);
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    exists = Read("UpdateFundTransaction#3",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier2", crvIdentifier);
        db.SetInt32(command, "cstIdentifier2", cstIdentifier);
      },
      null);

    if (!exists)
    {
      Update("UpdateFundTransaction#4",
        (db, command) =>
        {
          db.SetInt32(command, "crvIdentifier2", crvIdentifier);
          db.SetInt32(command, "cstIdentifier2", cstIdentifier);
        });
    }

    entities.FundTransaction.Amount = amount;
    entities.FundTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.FundTransaction.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceipt.FttIdentifier = null;
    entities.CashReceipt.PcaCode = null;
    entities.CashReceipt.PcaEffectiveDate = null;
    entities.CashReceipt.FunIdentifier = null;
    entities.CashReceipt.FtrIdentifier = null;
    entities.CashReceipt.Populated = true;
    entities.FundTransaction.Populated = true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Low.
    /// </summary>
    [JsonPropertyName("low")]
    public DateWorkArea Low
    {
      get => low ??= new();
      set => low = value;
    }

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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of CrDetails.
    /// </summary>
    [JsonPropertyName("crDetails")]
    public Common CrDetails
    {
      get => crDetails ??= new();
      set => crDetails = value;
    }

    /// <summary>
    /// A value of FtsOpen.
    /// </summary>
    [JsonPropertyName("ftsOpen")]
    public FundTransactionStatus FtsOpen
    {
      get => ftsOpen ??= new();
      set => ftsOpen = value;
    }

    /// <summary>
    /// A value of HardcodedCrsReceipted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsReceipted")]
    public CashReceiptStatus HardcodedCrsReceipted
    {
      get => hardcodedCrsReceipted ??= new();
      set => hardcodedCrsReceipted = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeposited")]
    public CashReceiptStatus HardcodedCrsDeposited
    {
      get => hardcodedCrsDeposited ??= new();
      set => hardcodedCrsDeposited = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public FundTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private DateWorkArea low;
    private DateWorkArea current;
    private DateWorkArea maximum;
    private DateWorkArea clear;
    private Common crDetails;
    private FundTransactionStatus ftsOpen;
    private CashReceiptStatus hardcodedCrsReceipted;
    private CashReceiptStatus hardcodedCrsDeposited;
    private FundTransaction new1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Receipted.
    /// </summary>
    [JsonPropertyName("receipted")]
    public CashReceiptStatus Receipted
    {
      get => receipted ??= new();
      set => receipted = value;
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
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ActiveCashReceiptStatusHistory
    {
      get => activeCashReceiptStatusHistory ??= new();
      set => activeCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatus")]
    public CashReceiptStatus ActiveCashReceiptStatus
    {
      get => activeCashReceiptStatus ??= new();
      set => activeCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
    }

    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptStatus receipted;
    private CashReceipt cashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private FundTransaction fundTransaction;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus fundTransactionStatus;
  }
#endregion
}
