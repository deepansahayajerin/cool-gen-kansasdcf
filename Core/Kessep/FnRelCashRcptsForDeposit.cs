// Program: FN_REL_CASH_RCPTS_FOR_DEPOSIT, ID: 371725739, model: 746.
// Short name: SWE00595
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REL_CASH_RCPTS_FOR_DEPOSIT.
/// </summary>
[Serializable]
public partial class FnRelCashRcptsForDeposit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REL_CASH_RCPTS_FOR_DEPOSIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRelCashRcptsForDeposit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRelCashRcptsForDeposit.
  /// </summary>
  public FnRelCashRcptsForDeposit(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------------
    // AUTHOR		DATE		DESCRIPTION
    // ----------	--------	
    // -----------------------------------
    // SHERAZ MALIK	04/28/97	Change_current date
    // J. Katz		10/10/98	Remove edit that non-cash receipts
    // 				cannot be deposited.
    // J. Katz		10/21/98	Create a new deposit for each
    // 				business date even if an existing
    // 				deposit is open.
    // 				Set Last Update By and Last Updated
    // 				Timestamp when updating the Fund
    // 				Transaction record.
    // J. Katz		06/04/99	Analyzed READ statements and set
    // 				read property to Select Only
    // 				where appropriate.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // ----------------------  HARD CODE AREA  --------------------------
    // Need the cash receipt status identifiers equal to 'balanced',
    // 'deposited', and 'pending'.
    // Need the fund transaction type identifier equal to 'deposit'.
    // Need the fund transaction status identifier equal to 'active'.
    // Need the program cost account code equal to the
    // clearing fund (22140, CSE revenues).
    // Need the clearing fund identifier number set to '9069'.
    // ------------------------------------------------------------------
    UseFnHardcodedCashReceipting();
    UseHardcodedFundingInformation();

    // -------------------------------------------------------
    // Cash receipt must be a 'cash' category.
    // Removed edit restricting non-cash receipts from being
    // deposited per CRTB Screen Assessment approved
    // 09/17/98.      JLK   10/10/98
    // -------------------------------------------------------
    if (!ReadCashReceipt())
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    if (ReadCashReceiptStatusCashReceiptStatusHistory())
    {
      if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsPending.SystemGeneratedIdentifier && entities
        .ActiveCashReceiptStatus.SystemGeneratedIdentifier != local
        .HardcodedCrsBalanced.SystemGeneratedIdentifier)
      {
        ExitState = "FN0107_CASH_RCPT_STA_INV_4_RQ_RB";

        return;
      }
    }
    else
    {
      ExitState = "FILE_READ_ERROR_WITH_RB";

      return;
    }

    // -----------------------------------------------------------------
    // Passed all of the edits, now get ready to deposit.
    // Determine if a Deposit exists with an Open status for a Business
    // Date of current date.  If so, add the receipt to this Deposit.
    // If a Deposit does not exist with a Business Date of current date,
    // it must be created.
    // -----------------------------------------------------------------
    if (ReadFundTransaction1())
    {
      ReadFundTransactionStatusHistoryFundTransactionStatus();

      if (entities.FundTransactionStatus.SystemGeneratedIdentifier == local
        .HardcodedFtsClosed.SystemGeneratedIdentifier)
      {
        ExitState = "FN0307_CANT_ADD_CR_TO_CLOSED_DEP";

        return;
      }

      MoveFundTransaction1(entities.FundTransaction, export.New1);
    }
    else
    {
      // ---------------------------------------------------
      // Create new  Deposit (Fund) Transaction record.
      // ---------------------------------------------------
      local.NewFundTransaction.BusinessDate = local.Current.Date;
      local.HardcodedClearingFundProgramCostAccount.EffectiveDate =
        local.NewFundTransaction.BusinessDate;
      local.ForDepositNumber.Identifier = "DEPOSIT NUMBER";
      local.NewFundTransaction.DepositNumber = UseAccessControlTable();
      UseCreateFundTransaction();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (ReadFundTransaction2())
      {
        // ok, have currency set.
        MoveFundTransaction1(entities.FundTransaction, export.New1);
      }
      else
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }
    }

    // ------------------------------------------------
    // Now actually release the cash receipt.
    // ------------------------------------------------
    if (ReadCashReceiptStatus())
    {
      UseFnChangeCashRcptStatusHist();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      ExitState = "DEPOSIT_STATUS_NF_RB";

      return;
    }

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

    // ------------------------------------------------
    // Update the Fund Transaction with the new Amount.
    // ------------------------------------------------
    local.NewFundTransaction.Amount = entities.FundTransaction.Amount + entities
      .CashReceipt.ReceiptAmount;

    try
    {
      UpdateFundTransaction();
      MoveFundTransaction1(entities.FundTransaction, export.New1);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_FUND_TRANS_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_FUND_TRANS_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveFundTransaction1(FundTransaction source,
    FundTransaction target)
  {
    target.DepositNumber = source.DepositNumber;
    target.Amount = source.Amount;
  }

  private static void MoveFundTransaction2(FundTransaction source,
    FundTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.BusinessDate = source.BusinessDate;
  }

  private static void MoveProgramCostAccount(ProgramCostAccount source,
    ProgramCostAccount target)
  {
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
  }

  private int UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ForDepositNumber.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    return useExport.ControlTable.LastUsedNumber;
  }

  private void UseCreateFundTransaction()
  {
    var useImport = new CreateFundTransaction.Import();
    var useExport = new CreateFundTransaction.Export();

    useImport.FundTransaction.Assign(local.NewFundTransaction);
    useImport.FundTransactionStatusHistory.ReasonText =
      local.NewFundTransactionStatusHistory.ReasonText;
    useImport.FundTransactionType.SystemGeneratedIdentifier =
      local.HardcodedFttDeposit.SystemGeneratedIdentifier;
    MoveProgramCostAccount(local.HardcodedClearingFundProgramCostAccount,
      useImport.ProgramCostAccount);
    useImport.Fund.SystemGeneratedIdentifier =
      local.HardcodedClearingFundFund.SystemGeneratedIdentifier;

    Call(CreateFundTransaction.Execute, useImport, useExport);

    MoveFundTransaction2(useExport.FundTransaction, local.NewFundTransaction);
  }

  private void UseFnChangeCashRcptStatusHist()
  {
    var useImport = new FnChangeCashRcptStatusHist.Import();
    var useExport = new FnChangeCashRcptStatusHist.Export();

    useImport.NewPersistent.Assign(entities.Deposited);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;

    Call(FnChangeCashRcptStatusHist.Execute, useImport, useExport);

    MoveCashReceiptStatus(useImport.NewPersistent, entities.Deposited);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsBalanced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdBalanced.SystemGeneratedIdentifier;
    local.HardcodedCrsPending.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdPended.SystemGeneratedIdentifier;
    local.HardcodedCrsDeposited.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
  }

  private void UseHardcodedFundingInformation()
  {
    var useImport = new HardcodedFundingInformation.Import();
    var useExport = new HardcodedFundingInformation.Export();

    Call(HardcodedFundingInformation.Execute, useImport, useExport);

    local.HardcodedFttDeposit.SystemGeneratedIdentifier =
      useExport.Deposit.SystemGeneratedIdentifier;
    local.HardcodedFtsOpen.SystemGeneratedIdentifier =
      useExport.Open.SystemGeneratedIdentifier;
    local.HardcodedFtsClosed.SystemGeneratedIdentifier =
      useExport.Closed.SystemGeneratedIdentifier;
    MoveProgramCostAccount(useExport.ClearingFundRevenue,
      local.HardcodedClearingFundProgramCostAccount);
    local.HardcodedClearingFundFund.SystemGeneratedIdentifier =
      useExport.ClearingFund.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

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
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.DepositReleaseDate = db.GetNullableDate(reader, 5);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 7);
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

  private bool ReadCashReceiptStatus()
  {
    entities.Deposited.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsDeposited.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Deposited.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Deposited.Code = db.GetString(reader, 1);
        entities.Deposited.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusCashReceiptStatusHistory",
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
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadFundTransaction1()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction1",
      (db, command) =>
      {
        db.SetDate(
          command, "businessDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "fttIdentifier",
          local.HardcodedFttDeposit.SystemGeneratedIdentifier);
        db.SetString(
          command, "pcaCode",
          local.HardcodedClearingFundProgramCostAccount.Code);
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
        entities.FundTransaction.CreatedBy = db.GetString(reader, 8);
        entities.FundTransaction.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.FundTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.FundTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.FundTransaction.Populated = true;
      });
  }

  private bool ReadFundTransaction2()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransId",
          local.NewFundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "fttIdentifier",
          local.HardcodedFttDeposit.SystemGeneratedIdentifier);
        db.SetString(
          command, "pcaCode",
          local.HardcodedClearingFundProgramCostAccount.Code);
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
        entities.FundTransaction.CreatedBy = db.GetString(reader, 8);
        entities.FundTransaction.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.FundTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.FundTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.FundTransaction.Populated = true;
      });
  }

  private bool ReadFundTransactionStatusHistoryFundTransactionStatus()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.FundTransactionStatusHistory.Populated = false;
    entities.FundTransactionStatus.Populated = false;

    return Read("ReadFundTransactionStatusHistoryFundTransactionStatus",
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
        entities.FundTransactionStatusHistory.FtrIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FunIdentifier =
          db.GetInt32(reader, 1);
        entities.FundTransactionStatusHistory.PcaEffectiveDate =
          db.GetDate(reader, 2);
        entities.FundTransactionStatusHistory.PcaCode = db.GetString(reader, 3);
        entities.FundTransactionStatusHistory.FttIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransactionStatusHistory.FtsIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
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
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var depositReleaseDate = local.Current.Date;
    var fttIdentifier = entities.FundTransaction.FttIdentifier;
    var pcaCode = entities.FundTransaction.PcaCode;
    var pcaEffectiveDate = entities.FundTransaction.PcaEffectiveDate;
    var funIdentifier = entities.FundTransaction.FunIdentifier;
    var ftrIdentifier = entities.FundTransaction.SystemGeneratedIdentifier;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableInt32(command, "fttIdentifier", fttIdentifier);
        db.SetNullableString(command, "pcaCode", pcaCode);
        db.SetNullableDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetNullableInt32(command, "funIdentifier", funIdentifier);
        db.SetNullableInt32(command, "ftrIdentifier", ftrIdentifier);
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
    entities.CashReceipt.FttIdentifier = fttIdentifier;
    entities.CashReceipt.PcaCode = pcaCode;
    entities.CashReceipt.PcaEffectiveDate = pcaEffectiveDate;
    entities.CashReceipt.FunIdentifier = funIdentifier;
    entities.CashReceipt.FtrIdentifier = ftrIdentifier;
    entities.CashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateFundTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var amount = local.NewFundTransaction.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;

    entities.FundTransaction.Populated = false;
    Update("UpdateFundTransaction",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetString(command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fundTransId",
          entities.FundTransaction.SystemGeneratedIdentifier);
      });

    entities.FundTransaction.Amount = amount;
    entities.FundTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.FundTransaction.LastUpdatedTmst = lastUpdatedTmst;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public FundTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private FundTransaction new1;
    private CashReceipt cashReceipt;
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
    /// A value of ForDepositNumber.
    /// </summary>
    [JsonPropertyName("forDepositNumber")]
    public ControlTable ForDepositNumber
    {
      get => forDepositNumber ??= new();
      set => forDepositNumber = value;
    }

    /// <summary>
    /// A value of NewFundTransaction.
    /// </summary>
    [JsonPropertyName("newFundTransaction")]
    public FundTransaction NewFundTransaction
    {
      get => newFundTransaction ??= new();
      set => newFundTransaction = value;
    }

    /// <summary>
    /// A value of NewFundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("newFundTransactionStatusHistory")]
    public FundTransactionStatusHistory NewFundTransactionStatusHistory
    {
      get => newFundTransactionStatusHistory ??= new();
      set => newFundTransactionStatusHistory = value;
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
    /// A value of HardcodedCrsPending.
    /// </summary>
    [JsonPropertyName("hardcodedCrsPending")]
    public CashReceiptStatus HardcodedCrsPending
    {
      get => hardcodedCrsPending ??= new();
      set => hardcodedCrsPending = value;
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
    /// A value of HardcodedFttDeposit.
    /// </summary>
    [JsonPropertyName("hardcodedFttDeposit")]
    public FundTransactionType HardcodedFttDeposit
    {
      get => hardcodedFttDeposit ??= new();
      set => hardcodedFttDeposit = value;
    }

    /// <summary>
    /// A value of HardcodedFtsOpen.
    /// </summary>
    [JsonPropertyName("hardcodedFtsOpen")]
    public FundTransactionStatus HardcodedFtsOpen
    {
      get => hardcodedFtsOpen ??= new();
      set => hardcodedFtsOpen = value;
    }

    /// <summary>
    /// A value of HardcodedFtsClosed.
    /// </summary>
    [JsonPropertyName("hardcodedFtsClosed")]
    public FundTransactionStatus HardcodedFtsClosed
    {
      get => hardcodedFtsClosed ??= new();
      set => hardcodedFtsClosed = value;
    }

    /// <summary>
    /// A value of HardcodedClearingFundProgramCostAccount.
    /// </summary>
    [JsonPropertyName("hardcodedClearingFundProgramCostAccount")]
    public ProgramCostAccount HardcodedClearingFundProgramCostAccount
    {
      get => hardcodedClearingFundProgramCostAccount ??= new();
      set => hardcodedClearingFundProgramCostAccount = value;
    }

    /// <summary>
    /// A value of HardcodedClearingFundFund.
    /// </summary>
    [JsonPropertyName("hardcodedClearingFundFund")]
    public Fund HardcodedClearingFundFund
    {
      get => hardcodedClearingFundFund ??= new();
      set => hardcodedClearingFundFund = value;
    }

    private DateWorkArea current;
    private ControlTable forDepositNumber;
    private FundTransaction newFundTransaction;
    private FundTransactionStatusHistory newFundTransactionStatusHistory;
    private CashReceiptStatus hardcodedCrsBalanced;
    private CashReceiptStatus hardcodedCrsPending;
    private CashReceiptStatus hardcodedCrsDeposited;
    private FundTransactionType hardcodedFttDeposit;
    private FundTransactionStatus hardcodedFtsOpen;
    private FundTransactionStatus hardcodedFtsClosed;
    private ProgramCostAccount hardcodedClearingFundProgramCostAccount;
    private Fund hardcodedClearingFundFund;
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

    /// <summary>
    /// A value of PcaFundExplosionRule.
    /// </summary>
    [JsonPropertyName("pcaFundExplosionRule")]
    public PcaFundExplosionRule PcaFundExplosionRule
    {
      get => pcaFundExplosionRule ??= new();
      set => pcaFundExplosionRule = value;
    }

    /// <summary>
    /// A value of ProgramCostAccount.
    /// </summary>
    [JsonPropertyName("programCostAccount")]
    public ProgramCostAccount ProgramCostAccount
    {
      get => programCostAccount ??= new();
      set => programCostAccount = value;
    }

    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
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
    /// A value of Deposited.
    /// </summary>
    [JsonPropertyName("deposited")]
    public CashReceiptStatus Deposited
    {
      get => deposited ??= new();
      set => deposited = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus fundTransactionStatus;
    private PcaFundExplosionRule pcaFundExplosionRule;
    private ProgramCostAccount programCostAccount;
    private FundTransactionType fundTransactionType;
    private FundTransaction fundTransaction;
    private CashReceiptStatus deposited;
  }
#endregion
}
