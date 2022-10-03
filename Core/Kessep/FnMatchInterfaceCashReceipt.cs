// Program: FN_MATCH_INTERFACE_CASH_RECEIPT, ID: 372346723, model: 746.
// Short name: SWE00504
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_MATCH_INTERFACE_CASH_RECEIPT.
/// </para>
/// <para>
/// RESP: CASHMGMNT		
/// This action block will update the status of a cash receipt because the check
/// has arrived from the interface.  The amount may update too in case of an
/// error along the way where we are expecting a different amount than what they
/// sent.
/// </para>
/// </summary>
[Serializable]
public partial class FnMatchInterfaceCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MATCH_INTERFACE_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMatchInterfaceCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMatchInterfaceCashReceipt.
  /// </summary>
  public FnMatchInterfaceCashReceipt(IContext context, Import import,
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
    // ------------------------------------------------------------------
    //                          Change Log
    // ------------------------------------------------------------------
    // Date      Developer		Description
    // ------------------------------------------------------------------
    // 06/08/99  J. Katz		Analyzed READ statements and
    // 				changed read property to Select
    // 				Only where appropriate.
    // ------------------------------------------------------------------
    local.Current.Timestamp = Now();
    UseFnHardcodedCashReceipting();

    // ------------------------------------------------------------------
    // If the imported Cash Receipt has a valid Receipt Number,
    // read the receipt to retrieve the balanced timestamp and
    // deposit release date.
    // ------------------------------------------------------------------
    if (import.ReceiptedCashReceipt.SequentialNumber > 0)
    {
      if (ReadCashReceipt1())
      {
        // ------------------------------------------------------------
        // A cash receipt cannot be matched to an interface receipt if
        // the cash receipt already participates in one or more
        // adjustments.
        // Added two new Cash Receipt Rln Rsn codes for receipt
        // amount adjustments -- PROCCSTFEE and NETINTFERR.
        // JLK  06/21/99
        // ------------------------------------------------------------
        if (ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn())
        {
          MoveCashReceipt1(import.ReceiptedCashReceipt, export.Matched);
          ExitState = "FN0000_CR_ADJ_EXIST_NO_MTC_UNMTC";

          return;
        }

        local.CashReceipt.BalancedTimestamp =
          entities.ExistingReceipted.BalancedTimestamp;
        local.CashReceipt.DepositReleaseDate =
          entities.ExistingReceipted.DepositReleaseDate;
      }
      else
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }
    }

    // ------------------------------------------------------------------
    // Read the existing interface receipt.
    // ------------------------------------------------------------------
    if (ReadCashReceipt2())
    {
      // ---> continue
    }
    else
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // ------------------------------------------------------------------
    // Create Cash Receipt Audit record if receipted Receipt Amount
    // is not equal to the interface Receipt Amount.
    // ------------------------------------------------------------------
    if (import.ReceiptedCashReceipt.ReceiptAmount != entities
      .InterfaceCashReceipt.ReceiptAmount)
    {
      // ------------------------------------------------------------------
      // Set up last updated values for Cash Receipt Audit record.
      // ------------------------------------------------------------------
      if (IsEmpty(entities.InterfaceCashReceipt.LastUpdatedBy))
      {
        local.CashReceipt.LastUpdatedBy =
          entities.InterfaceCashReceipt.CreatedBy;
      }
      else
      {
        local.CashReceipt.LastUpdatedBy =
          entities.InterfaceCashReceipt.LastUpdatedBy;
      }

      if (Equal(entities.InterfaceCashReceipt.LastUpdatedTimestamp,
        local.Null1.Timestamp))
      {
        local.CashReceipt.LastUpdatedTimestamp =
          entities.InterfaceCashReceipt.CreatedTimestamp;
      }
      else
      {
        local.CashReceipt.LastUpdatedTimestamp =
          entities.InterfaceCashReceipt.LastUpdatedTimestamp;
      }

      try
      {
        CreateCashReceiptAudit();

        // ok
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0025_CASH_RCPT_AUDIT_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ------------------------------------------------------------------
    // Determine interface receipt balance due and balance reason.
    // ------------------------------------------------------------------
    local.CashReceipt.CashBalanceAmt =
      entities.InterfaceCashReceipt.CashDue.GetValueOrDefault() - import
      .ReceiptedCashReceipt.ReceiptAmount;

    if (local.CashReceipt.CashBalanceAmt.GetValueOrDefault() > 0)
    {
      local.CashReceipt.CashBalanceReason = "UNDER";
    }
    else if (local.CashReceipt.CashBalanceAmt.GetValueOrDefault() < 0)
    {
      local.CashReceipt.CashBalanceReason = "OVER";
    }
    else
    {
      local.CashReceipt.CashBalanceReason = "";
    }

    // ------------------------------------------------------------------
    // Populate the local cash receipt view with a Note describing
    // which existing receipt number is being matched to the
    // interface.    JLK  02/24/99
    // ------------------------------------------------------------------
    if (import.ReceiptedCashReceipt.SequentialNumber > 0)
    {
      local.CashReceipt.Note =
        "This interface receipt was matched to Cash Receipt # " + NumberToString
        (entities.ExistingReceipted.SequentialNumber, 15);
    }

    // ------------------------------------------------------------------
    // Update the interface receipt with the cash receipt information.
    // ------------------------------------------------------------------
    try
    {
      UpdateCashReceipt();
      MoveCashReceipt2(entities.InterfaceCashReceipt, export.Matched);
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

    // ------------------------------------------------------------------
    // Create new status history record for interface receipt.
    // ------------------------------------------------------------------
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
      ExitState = "FN0108_CASH_RCPT_STAT_NF";

      return;
    }

    // ------------------------------------------------------------------
    // If status of existing receipted Cash Receipt was DEP, remove the
    // receipted Cash Receipt from the Deposit and associate the interface
    // Cash Receipt with the Deposit.
    // ------------------------------------------------------------------
    if (import.ReceiptedCashReceiptStatus.SystemGeneratedIdentifier == local
      .HardcodedCrsDeposited.SystemGeneratedIdentifier)
    {
      if (ReadFundTransaction())
      {
        DisassociateCashReceipt();
        AssociateCashReceipt();
      }
      else
      {
        ExitState = "FN0000_FUND_TRANS_NF";

        return;
      }
    }

    // ------------------------------------------------------------------
    // Change the status of the existing receipted Cash Receipt
    // that was matched to the interface to DEL.
    // ------------------------------------------------------------------
    if (import.ReceiptedCashReceipt.SequentialNumber > 0)
    {
      local.CashReceiptDeleteReason.Code = "CK HOLD";
      local.ReasonForDiscontinue.ReasonText =
        "This Cash Receipt has been matched to interface Receipt # " + NumberToString
        (entities.InterfaceCashReceipt.SequentialNumber, 15);
      UseFnMarkCashReceiptDeleted();
    }
  }

  private static void MoveCashReceipt1(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CheckDate = source.CheckDate;
    target.ReceivedDate = source.ReceivedDate;
    target.Note = source.Note;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorFirstName = source.PayorFirstName;
    target.PayorMiddleName = source.PayorMiddleName;
    target.PayorLastName = source.PayorLastName;
  }

  private static void MoveCashReceipt2(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CheckDate = source.CheckDate;
    target.ReceivedDate = source.ReceivedDate;
    target.Note = source.Note;
    target.PayorOrganization = source.PayorOrganization;
    target.PayorFirstName = source.PayorFirstName;
    target.PayorMiddleName = source.PayorMiddleName;
    target.PayorLastName = source.PayorLastName;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.CashDue = source.CashDue;
    target.CashBalanceAmt = source.CashBalanceAmt;
    target.CashBalanceReason = source.CashBalanceReason;
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseFnChangeCashRcptStatusHist()
  {
    var useImport = new FnChangeCashRcptStatusHist.Import();
    var useExport = new FnChangeCashRcptStatusHist.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.InterfaceCashReceipt.SequentialNumber;
    useImport.NewPersistent.Assign(entities.NewInterface);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.InterfaceCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.InterfaceCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.InterfaceCashReceiptType.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptStatusHist.Execute, useImport, useExport);

    MoveCashReceiptStatus(useImport.NewPersistent, entities.NewInterface);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsDeposited.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
  }

  private void UseFnMarkCashReceiptDeleted()
  {
    var useImport = new FnMarkCashReceiptDeleted.Import();
    var useExport = new FnMarkCashReceiptDeleted.Export();

    useImport.CashReceipt.SequentialNumber =
      import.ReceiptedCashReceipt.SequentialNumber;
    useImport.CashReceiptDeleteReason.Code = local.CashReceiptDeleteReason.Code;
    useImport.ReasonForDiscontinue.ReasonText =
      local.ReasonForDiscontinue.ReasonText;
    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      local.HardcodedCrsDeleted.SystemGeneratedIdentifier;

    Call(FnMarkCashReceiptDeleted.Execute, useImport, useExport);

    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useImport.CashReceiptStatus.SystemGeneratedIdentifier;
  }

  private void AssociateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.InterfaceCashReceipt.Populated);
    System.Diagnostics.Debug.Assert(entities.ReceiptedDeposit.Populated);

    var fttIdentifier = entities.ReceiptedDeposit.FttIdentifier;
    var pcaCode = entities.ReceiptedDeposit.PcaCode;
    var pcaEffectiveDate = entities.ReceiptedDeposit.PcaEffectiveDate;
    var funIdentifier = entities.ReceiptedDeposit.FunIdentifier;
    var ftrIdentifier = entities.ReceiptedDeposit.SystemGeneratedIdentifier;

    entities.InterfaceCashReceipt.Populated = false;
    Update("AssociateCashReceipt",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fttIdentifier", fttIdentifier);
        db.SetNullableString(command, "pcaCode", pcaCode);
        db.SetNullableDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetNullableInt32(command, "funIdentifier", funIdentifier);
        db.SetNullableInt32(command, "ftrIdentifier", ftrIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.InterfaceCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.InterfaceCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.InterfaceCashReceipt.CrtIdentifier);
      });

    entities.InterfaceCashReceipt.FttIdentifier = fttIdentifier;
    entities.InterfaceCashReceipt.PcaCode = pcaCode;
    entities.InterfaceCashReceipt.PcaEffectiveDate = pcaEffectiveDate;
    entities.InterfaceCashReceipt.FunIdentifier = funIdentifier;
    entities.InterfaceCashReceipt.FtrIdentifier = ftrIdentifier;
    entities.InterfaceCashReceipt.Populated = true;
  }

  private void CreateCashReceiptAudit()
  {
    System.Diagnostics.Debug.Assert(entities.InterfaceCashReceipt.Populated);

    var receiptAmount = entities.InterfaceCashReceipt.ReceiptAmount;
    var lastUpdatedTmst = local.CashReceipt.LastUpdatedTimestamp;
    var lastUpdatedBy = local.CashReceipt.LastUpdatedBy ?? "";
    var priorTransactionAmount =
      entities.InterfaceCashReceipt.TotalCashTransactionAmount;
    var priorAdjustmentAmount = 0M;
    var crvIdentifier = entities.InterfaceCashReceipt.CrvIdentifier;
    var cstIdentifier = entities.InterfaceCashReceipt.CstIdentifier;
    var crtIdentifier = entities.InterfaceCashReceipt.CrtIdentifier;

    entities.InterfaceCashReceiptAudit.Populated = false;
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

    entities.InterfaceCashReceiptAudit.ReceiptAmount = receiptAmount;
    entities.InterfaceCashReceiptAudit.LastUpdatedTmst = lastUpdatedTmst;
    entities.InterfaceCashReceiptAudit.LastUpdatedBy = lastUpdatedBy;
    entities.InterfaceCashReceiptAudit.PriorTransactionAmount =
      priorTransactionAmount;
    entities.InterfaceCashReceiptAudit.PriorAdjustmentAmount =
      priorAdjustmentAmount;
    entities.InterfaceCashReceiptAudit.CrvIdentifier = crvIdentifier;
    entities.InterfaceCashReceiptAudit.CstIdentifier = cstIdentifier;
    entities.InterfaceCashReceiptAudit.CrtIdentifier = crtIdentifier;
    entities.InterfaceCashReceiptAudit.Populated = true;
  }

  private void DisassociateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingReceipted.Populated);

    var crvIdentifier = entities.ExistingReceipted.CrvIdentifier;
    var cstIdentifier = entities.ExistingReceipted.CstIdentifier;

    entities.ExistingReceipted.Populated = false;

    bool exists;

    Update("DisassociateCashReceipt#1",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier1", crvIdentifier);
        db.SetInt32(command, "cstIdentifier1", cstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingReceipted.CrtIdentifier);
      });

    exists = Read("DisassociateCashReceipt#2",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier2", crvIdentifier);
        db.SetInt32(command, "cstIdentifier2", cstIdentifier);
      },
      null);

    if (!exists)
    {
      Update("DisassociateCashReceipt#3",
        (db, command) =>
        {
          db.SetInt32(command, "crvIdentifier2", crvIdentifier);
          db.SetInt32(command, "cstIdentifier2", cstIdentifier);
        });
    }

    entities.ExistingReceipted.FttIdentifier = null;
    entities.ExistingReceipted.PcaCode = null;
    entities.ExistingReceipted.PcaEffectiveDate = null;
    entities.ExistingReceipted.FunIdentifier = null;
    entities.ExistingReceipted.FtrIdentifier = null;
    entities.ExistingReceipted.Populated = true;
  }

  private bool ReadCashReceipt1()
  {
    entities.ExistingReceipted.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId",
          import.ReceiptedCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.ExistingReceipted.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingReceipted.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingReceipted.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingReceipted.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingReceipted.DepositReleaseDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingReceipted.BalancedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingReceipted.FttIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingReceipted.PcaCode = db.GetNullableString(reader, 7);
        entities.ExistingReceipted.PcaEffectiveDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingReceipted.FunIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.ExistingReceipted.FtrIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingReceipted.Populated = true;
      });
  }

  private bool ReadCashReceipt2()
  {
    entities.InterfaceCashReceipt.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.InterfaceCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.InterfaceCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.InterfaceCashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.InterfaceCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.InterfaceCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.InterfaceCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.InterfaceCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.InterfaceCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.InterfaceCashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.InterfaceCashReceipt.CheckType =
          db.GetNullableString(reader, 6);
        entities.InterfaceCashReceipt.CheckNumber =
          db.GetNullableString(reader, 7);
        entities.InterfaceCashReceipt.CheckDate = db.GetNullableDate(reader, 8);
        entities.InterfaceCashReceipt.ReceivedDate = db.GetDate(reader, 9);
        entities.InterfaceCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 10);
        entities.InterfaceCashReceipt.PayorOrganization =
          db.GetNullableString(reader, 11);
        entities.InterfaceCashReceipt.PayorFirstName =
          db.GetNullableString(reader, 12);
        entities.InterfaceCashReceipt.PayorMiddleName =
          db.GetNullableString(reader, 13);
        entities.InterfaceCashReceipt.PayorLastName =
          db.GetNullableString(reader, 14);
        entities.InterfaceCashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.InterfaceCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 16);
        entities.InterfaceCashReceipt.CreatedBy = db.GetString(reader, 17);
        entities.InterfaceCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 18);
        entities.InterfaceCashReceipt.FttIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.InterfaceCashReceipt.PcaCode =
          db.GetNullableString(reader, 20);
        entities.InterfaceCashReceipt.PcaEffectiveDate =
          db.GetNullableDate(reader, 21);
        entities.InterfaceCashReceipt.FunIdentifier =
          db.GetNullableInt32(reader, 22);
        entities.InterfaceCashReceipt.FtrIdentifier =
          db.GetNullableInt32(reader, 23);
        entities.InterfaceCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 24);
        entities.InterfaceCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 25);
        entities.InterfaceCashReceipt.CashDue =
          db.GetNullableDecimal(reader, 26);
        entities.InterfaceCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 27);
        entities.InterfaceCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 28);
        entities.InterfaceCashReceipt.Note = db.GetNullableString(reader, 29);
        entities.InterfaceCashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.InterfaceCashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingReceipted.Populated);
    entities.ExistingCashReceiptBalanceAdjustment.Populated = false;
    entities.ExistingCashReceiptRlnRsn.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIIdentifier", entities.ExistingReceipted.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier", entities.ExistingReceipted.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier", entities.ExistingReceipted.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingCashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ExistingCashReceiptRlnRsn.Code = db.GetString(reader, 9);
        entities.ExistingCashReceiptBalanceAdjustment.Populated = true;
        entities.ExistingCashReceiptRlnRsn.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.NewInterface.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          import.ReceiptedCashReceiptStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.NewInterface.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NewInterface.Code = db.GetString(reader, 1);
        entities.NewInterface.Populated = true;
      });
  }

  private bool ReadFundTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingReceipted.Populated);
    entities.ReceiptedDeposit.Populated = false;

    return Read("ReadFundTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "fundTransId",
          entities.ExistingReceipted.FtrIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier",
          entities.ExistingReceipted.FunIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "fttIdentifier",
          entities.ExistingReceipted.FttIdentifier.GetValueOrDefault());
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.ExistingReceipted.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "pcaCode", entities.ExistingReceipted.PcaCode ?? "");
      },
      (db, reader) =>
      {
        entities.ReceiptedDeposit.FttIdentifier = db.GetInt32(reader, 0);
        entities.ReceiptedDeposit.PcaCode = db.GetString(reader, 1);
        entities.ReceiptedDeposit.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.ReceiptedDeposit.FunIdentifier = db.GetInt32(reader, 3);
        entities.ReceiptedDeposit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ReceiptedDeposit.Populated = true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.InterfaceCashReceipt.Populated);

    var receiptAmount = import.ReceiptedCashReceipt.ReceiptAmount;
    var receiptDate = import.ReceiptedCashReceipt.ReceiptDate;
    var checkNumber = import.ReceiptedCashReceipt.CheckNumber ?? "";
    var checkDate = import.ReceiptedCashReceipt.CheckDate;
    var receivedDate = import.ReceiptedCashReceipt.ReceivedDate;
    var depositReleaseDate = local.CashReceipt.DepositReleaseDate;
    var payorOrganization = import.ReceiptedCashReceipt.PayorOrganization ?? "";
    var payorFirstName = import.ReceiptedCashReceipt.PayorFirstName ?? "";
    var payorMiddleName = import.ReceiptedCashReceipt.PayorMiddleName ?? "";
    var payorLastName = import.ReceiptedCashReceipt.PayorLastName ?? "";
    var balancedTimestamp = local.CashReceipt.BalancedTimestamp;
    var createdBy = import.ReceiptedCashReceipt.CreatedBy;
    var cashBalanceAmt = local.CashReceipt.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = local.CashReceipt.CashBalanceReason ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var note = local.CashReceipt.Note ?? "";

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.InterfaceCashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetDate(command, "receiptDate", receiptDate);
        db.SetNullableString(command, "checkNumber", checkNumber);
        db.SetNullableDate(command, "checkDate", checkDate);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableString(command, "payorOrganization", payorOrganization);
        db.SetNullableString(command, "payorFirstName", payorFirstName);
        db.SetNullableString(command, "payorMiddleName", payorMiddleName);
        db.SetNullableString(command, "payorLastName", payorLastName);
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "note", note);
        db.SetInt32(
          command, "crvIdentifier",
          entities.InterfaceCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.InterfaceCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.InterfaceCashReceipt.CrtIdentifier);
      });

    entities.InterfaceCashReceipt.ReceiptAmount = receiptAmount;
    entities.InterfaceCashReceipt.ReceiptDate = receiptDate;
    entities.InterfaceCashReceipt.CheckNumber = checkNumber;
    entities.InterfaceCashReceipt.CheckDate = checkDate;
    entities.InterfaceCashReceipt.ReceivedDate = receivedDate;
    entities.InterfaceCashReceipt.DepositReleaseDate = depositReleaseDate;
    entities.InterfaceCashReceipt.PayorOrganization = payorOrganization;
    entities.InterfaceCashReceipt.PayorFirstName = payorFirstName;
    entities.InterfaceCashReceipt.PayorMiddleName = payorMiddleName;
    entities.InterfaceCashReceipt.PayorLastName = payorLastName;
    entities.InterfaceCashReceipt.BalancedTimestamp = balancedTimestamp;
    entities.InterfaceCashReceipt.CreatedBy = createdBy;
    entities.InterfaceCashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.InterfaceCashReceipt.CashBalanceReason = cashBalanceReason;
    entities.InterfaceCashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.InterfaceCashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterfaceCashReceipt.Note = note;
    entities.InterfaceCashReceipt.Populated = true;
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
    /// A value of InterfaceCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptSourceType")]
    public CashReceiptSourceType InterfaceCashReceiptSourceType
    {
      get => interfaceCashReceiptSourceType ??= new();
      set => interfaceCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptEvent")]
    public CashReceiptEvent InterfaceCashReceiptEvent
    {
      get => interfaceCashReceiptEvent ??= new();
      set => interfaceCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptType.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptType")]
    public CashReceiptType InterfaceCashReceiptType
    {
      get => interfaceCashReceiptType ??= new();
      set => interfaceCashReceiptType = value;
    }

    /// <summary>
    /// A value of ReceiptedCashReceipt.
    /// </summary>
    [JsonPropertyName("receiptedCashReceipt")]
    public CashReceipt ReceiptedCashReceipt
    {
      get => receiptedCashReceipt ??= new();
      set => receiptedCashReceipt = value;
    }

    /// <summary>
    /// A value of ReceiptedCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("receiptedCashReceiptStatus")]
    public CashReceiptStatus ReceiptedCashReceiptStatus
    {
      get => receiptedCashReceiptStatus ??= new();
      set => receiptedCashReceiptStatus = value;
    }

    private CashReceiptSourceType interfaceCashReceiptSourceType;
    private CashReceiptEvent interfaceCashReceiptEvent;
    private CashReceiptType interfaceCashReceiptType;
    private CashReceipt receiptedCashReceipt;
    private CashReceiptStatus receiptedCashReceiptStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Matched.
    /// </summary>
    [JsonPropertyName("matched")]
    public CashReceipt Matched
    {
      get => matched ??= new();
      set => matched = value;
    }

    private CashReceipt matched;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of HardcodedCrsDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeposited")]
    public CashReceiptStatus HardcodedCrsDeposited
    {
      get => hardcodedCrsDeposited ??= new();
      set => hardcodedCrsDeposited = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeleted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeleted")]
    public CashReceiptStatus HardcodedCrsDeleted
    {
      get => hardcodedCrsDeleted ??= new();
      set => hardcodedCrsDeleted = value;
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

    private DateWorkArea current;
    private DateWorkArea null1;
    private CashReceipt cashReceipt;
    private CashReceiptStatus hardcodedCrsDeposited;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptDeleteReason cashReceiptDeleteReason;
    private CashReceiptStatusHistory reasonForDiscontinue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingReceipted.
    /// </summary>
    [JsonPropertyName("existingReceipted")]
    public CashReceipt ExistingReceipted
    {
      get => existingReceipted ??= new();
      set => existingReceipted = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("existingCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment ExistingCashReceiptBalanceAdjustment
    {
      get => existingCashReceiptBalanceAdjustment ??= new();
      set => existingCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("existingCashReceiptRlnRsn")]
    public CashReceiptRlnRsn ExistingCashReceiptRlnRsn
    {
      get => existingCashReceiptRlnRsn ??= new();
      set => existingCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptAudit.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptAudit")]
    public CashReceiptAudit InterfaceCashReceiptAudit
    {
      get => interfaceCashReceiptAudit ??= new();
      set => interfaceCashReceiptAudit = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptSourceType")]
    public CashReceiptSourceType InterfaceCashReceiptSourceType
    {
      get => interfaceCashReceiptSourceType ??= new();
      set => interfaceCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptEvent")]
    public CashReceiptEvent InterfaceCashReceiptEvent
    {
      get => interfaceCashReceiptEvent ??= new();
      set => interfaceCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceiptType.
    /// </summary>
    [JsonPropertyName("interfaceCashReceiptType")]
    public CashReceiptType InterfaceCashReceiptType
    {
      get => interfaceCashReceiptType ??= new();
      set => interfaceCashReceiptType = value;
    }

    /// <summary>
    /// A value of InterfaceCashReceipt.
    /// </summary>
    [JsonPropertyName("interfaceCashReceipt")]
    public CashReceipt InterfaceCashReceipt
    {
      get => interfaceCashReceipt ??= new();
      set => interfaceCashReceipt = value;
    }

    /// <summary>
    /// A value of NewInterface.
    /// </summary>
    [JsonPropertyName("newInterface")]
    public CashReceiptStatus NewInterface
    {
      get => newInterface ??= new();
      set => newInterface = value;
    }

    /// <summary>
    /// A value of ReceiptedDeposit.
    /// </summary>
    [JsonPropertyName("receiptedDeposit")]
    public FundTransaction ReceiptedDeposit
    {
      get => receiptedDeposit ??= new();
      set => receiptedDeposit = value;
    }

    private CashReceipt existingReceipted;
    private CashReceiptBalanceAdjustment existingCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn existingCashReceiptRlnRsn;
    private CashReceiptAudit interfaceCashReceiptAudit;
    private CashReceiptSourceType interfaceCashReceiptSourceType;
    private CashReceiptEvent interfaceCashReceiptEvent;
    private CashReceiptType interfaceCashReceiptType;
    private CashReceipt interfaceCashReceipt;
    private CashReceiptStatus newInterface;
    private FundTransaction receiptedDeposit;
  }
#endregion
}
