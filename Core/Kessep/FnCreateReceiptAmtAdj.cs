// Program: FN_CREATE_RECEIPT_AMT_ADJ, ID: 372342872, model: 746.
// Short name: SWE02440
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_RECEIPT_AMT_ADJ.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block applies the incremental amount of a Cash Receipt 
/// Balance Adjustment to the increasing and decreasing Cash Receipts.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateReceiptAmtAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_RECEIPT_AMT_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateReceiptAmtAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateReceiptAmtAdj.
  /// </summary>
  public FnCreateReceiptAmtAdj(IContext context, Import import, Export export):
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
    // 10/20/99	J. Katz		Added logic to support two new
    // 				Cash Receipt Rln Rsn codes.
    // ---------------------------------------------------------------
    // ------------------------------------------------------------
    // Set up local views.
    // ------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // ------------------------------------------------------------
    // Read the Increase cash receipt.
    // ------------------------------------------------------------
    if (ReadCashReceiptCashReceiptType2())
    {
      if (!ReadCashReceiptEvent2())
      {
        ExitState = "FN0000_CASH_RECEIPT_EVENT_NF";

        return;
      }

      // ------------------------------------------------------------
      // The cash receipt type must have a Cash Indicator of Y.
      // ------------------------------------------------------------
      if (!Equal(entities.IncreaseCashReceipt.CheckType, "BOGUS"))
      {
        if (AsChar(entities.IncreaseCashReceiptType.CategoryIndicator) != AsChar
          (local.HardcodedCrtCatCash.CategoryIndicator))
        {
          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          return;
        }
        else
        {
          // --->  OK to continue.
        }
      }

      // ------------------------------------------------------------
      // The cash receipt cannot be in Deleted status.
      // ------------------------------------------------------------
      if (ReadCashReceiptStatusHistoryCashReceiptStatus2())
      {
        if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          return;
        }
        else
        {
          // --->  OK to continue.
        }
      }
      else
      {
        ExitState = "FN0000_CASH_RECEIPT_STAT_HIST_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_INCR_CASH_RECEIPT_NF";

      return;
    }

    // ------------------------------------------------------------
    // Read the Decrease cash receipt.
    // ------------------------------------------------------------
    if (ReadCashReceiptCashReceiptType1())
    {
      if (!ReadCashReceiptEvent1())
      {
        ExitState = "FN0000_CASH_RECEIPT_EVENT_NF";

        return;
      }

      // ------------------------------------------------------------
      // The cash receipt type must have a Cash Indicator of Y.
      // ------------------------------------------------------------
      if (!Equal(entities.DecreaseCashReceipt.CheckType, "BOGUS"))
      {
        if (AsChar(entities.DecreaseCashReceiptType.CategoryIndicator) != AsChar
          (local.HardcodedCrtCatCash.CategoryIndicator))
        {
          ExitState = "FN0000_INVALID_CR_TYP_4_REQ_ACTN";

          return;
        }
        else
        {
          // --->  OK to continue.
        }
      }

      // ------------------------------------------------------------
      // The cash receipt cannot be in Deleted status.
      // ------------------------------------------------------------
      if (ReadCashReceiptStatusHistoryCashReceiptStatus1())
      {
        if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          return;
        }
        else
        {
          // --->  OK to continue.
        }
      }
    }
    else
    {
      ExitState = "FN0000_DECR_CASH_RECEIPT_NF";

      return;
    }

    // ------------------------------------------------------------
    // Read the adjustment reason code.
    // ------------------------------------------------------------
    if (ReadCashReceiptRlnRsn())
    {
      // --->  OK to continue.
    }
    else
    {
      ExitState = "FN0093_CASH_RCPT_RLN_RSN_NF";

      return;
    }

    // ------------------------------------------------------------
    // Calculate Receipt Adj Amt for the Increase cash receipt.
    // Added logic to support reason codes PROCCSTFEE and NETINTFERR.
    // JLK  10/20/99
    // ------------------------------------------------------------
    local.Increase1CrAdjAmt.TotalCurrency = 0;
    local.Increase2CrAdjAmt.TotalCurrency = 0;
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2();
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn4();
    local.IncreaseCrAdjAmt.TotalCurrency =
      local.Increase1CrAdjAmt.TotalCurrency + local
      .Increase2CrAdjAmt.TotalCurrency + import
      .CashReceiptBalanceAdjustment.AdjustmentAmount;

    // ------------------------------------------------------------
    // Calculate the Net Receipt Amount for the Increase cash receipt.
    // ------------------------------------------------------------
    local.IncreaseNetReceiptAmt.TotalCurrency =
      entities.IncreaseCashReceipt.ReceiptAmount + local
      .IncreaseCrAdjAmt.TotalCurrency;

    // ------------------------------------------------------------
    // Calculate Balance Due for the Increase cash receipt.
    // ------------------------------------------------------------
    if (Lt(local.Null1.Date,
      entities.IncreaseCashReceiptEvent.SourceCreationDate))
    {
      local.Increase.CashBalanceAmt =
        entities.IncreaseCashReceipt.CashDue.GetValueOrDefault() - local
        .IncreaseNetReceiptAmt.TotalCurrency;

      if (local.Increase.CashBalanceAmt.GetValueOrDefault() > 0)
      {
        local.Increase.CashBalanceReason =
          local.HardcodedUnder.CashBalanceReason ?? "";
      }
      else if (local.Increase.CashBalanceAmt.GetValueOrDefault() < 0)
      {
        local.Increase.CashBalanceReason =
          local.HardcodedOver.CashBalanceReason ?? "";
      }
      else
      {
        local.Increase.CashBalanceReason = "";
      }
    }

    // ------------------------------------------------------------
    // Calculate Receipt Adj Amt for the Decrease cash receipt.
    // Added logic to support reason codes PROCCSTFEE and NETINTFERR.
    //  JLK  10/20/99
    // ------------------------------------------------------------
    local.Decrease1CrAdjAmt.TotalCurrency = 0;
    local.Decrease2CrAdjAmt.TotalCurrency = 0;
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1();
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3();
    local.DecreaseCrAdjAmt.TotalCurrency =
      local.Decrease1CrAdjAmt.TotalCurrency + local
      .Decrease2CrAdjAmt.TotalCurrency - import
      .CashReceiptBalanceAdjustment.AdjustmentAmount;

    // ------------------------------------------------------------
    // Calculate the Net Receipt Amount for the Decrease cash receipt.
    // ------------------------------------------------------------
    local.DecreaseNetAdjAmt.TotalCurrency =
      entities.DecreaseCashReceipt.ReceiptAmount + local
      .DecreaseCrAdjAmt.TotalCurrency;

    // ------------------------------------------------------------
    // Calculate Balance Due for the Increase cash receipt.
    // ------------------------------------------------------------
    if (Lt(local.Null1.Date,
      entities.DecreaseCashReceiptEvent.SourceCreationDate))
    {
      local.Decrease.CashBalanceAmt =
        entities.DecreaseCashReceipt.CashDue.GetValueOrDefault() - local
        .DecreaseNetAdjAmt.TotalCurrency;

      if (local.Decrease.CashBalanceAmt.GetValueOrDefault() > 0)
      {
        local.Decrease.CashBalanceReason =
          local.HardcodedUnder.CashBalanceReason ?? "";
      }
      else if (Lt(entities.DecreaseCashReceipt.CashBalanceAmt, 0))
      {
        local.Decrease.CashBalanceReason =
          local.HardcodedOver.CashBalanceReason ?? "";
      }
      else
      {
        local.Decrease.CashBalanceReason = "";
      }
    }

    // ------------------------------------------------------------
    // Create the balance adjustment record.
    // ------------------------------------------------------------
    try
    {
      CreateCashReceiptBalanceAdjustment();

      // --->  OK to continue.
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0030_CASH_RCPT_BAL_ADJ_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0033_CASH_RCPT_BAL_ADJ_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ------------------------------------------------------------
    // Update the increase Cash Receipt to reflect the adjustment.
    // ------------------------------------------------------------
    if (Lt(local.Null1.Date,
      entities.IncreaseCashReceiptEvent.SourceCreationDate))
    {
      try
      {
        UpdateCashReceipt2();

        // --->  OK to continue.
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
    }

    // ------------------------------------------------------------
    // Update the decrease Cash Receipt to reflect the adjustment.
    // ------------------------------------------------------------
    if (Lt(local.Null1.Date,
      entities.DecreaseCashReceiptEvent.SourceCreationDate))
    {
      try
      {
        UpdateCashReceipt1();

        // --->  OK to continue.
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
    }

    // ------------------------------------------------------------
    // Populate export views.
    // ------------------------------------------------------------
    MoveCashReceiptRlnRsn(entities.NewCashReceiptRlnRsn,
      export.CashReceiptRlnRsn);
    export.CashReceiptBalanceAdjustment.Assign(
      entities.NewCashReceiptBalanceAdjustment);
    export.Increase.Assign(entities.IncreaseCashReceipt);
    export.IncreaseCrAdjAmt.TotalCurrency =
      local.IncreaseCrAdjAmt.TotalCurrency;
    export.IncreaseNetReceiptAmt.TotalCurrency =
      local.IncreaseNetReceiptAmt.TotalCurrency;
    export.Decrease.Assign(entities.DecreaseCashReceipt);
    export.DecreaseCrAdjAmt.TotalCurrency =
      local.DecreaseCrAdjAmt.TotalCurrency;
    export.DecreaseNetAdjAmt.TotalCurrency =
      local.DecreaseNetAdjAmt.TotalCurrency;
  }

  private static void MoveCashReceiptRlnRsn(CashReceiptRlnRsn source,
    CashReceiptRlnRsn target)
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

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrtCatCash.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
    local.HardcodedOver.CashBalanceReason =
      useExport.CrCashBalanceReason.CrOver.CashBalanceReason;
    local.HardcodedUnder.CashBalanceReason =
      useExport.CrCashBalanceReason.CrUnder.CashBalanceReason;
  }

  private void CreateCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);

    var crtIdentifier = entities.DecreaseCashReceipt.CrtIdentifier;
    var cstIdentifier = entities.DecreaseCashReceipt.CstIdentifier;
    var crvIdentifier = entities.DecreaseCashReceipt.CrvIdentifier;
    var crtIIdentifier = entities.IncreaseCashReceipt.CrtIdentifier;
    var cstIIdentifier = entities.IncreaseCashReceipt.CstIdentifier;
    var crvIIdentifier = entities.IncreaseCashReceipt.CrvIdentifier;
    var crrIdentifier = entities.NewCashReceiptRlnRsn.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var adjustmentAmount = import.CashReceiptBalanceAdjustment.AdjustmentAmount;
    var createdBy = global.UserId;
    var description = import.CashReceiptBalanceAdjustment.Description ?? "";

    entities.NewCashReceiptBalanceAdjustment.Populated = false;
    Update("CreateCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crtIIdentifier", crtIIdentifier);
        db.SetInt32(command, "cstIIdentifier", cstIIdentifier);
        db.SetInt32(command, "crvIIdentifier", crvIIdentifier);
        db.SetInt32(command, "crrIdentifier", crrIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDecimal(command, "adjustmentAmount", adjustmentAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "description", description);
      });

    entities.NewCashReceiptBalanceAdjustment.CrtIdentifier = crtIdentifier;
    entities.NewCashReceiptBalanceAdjustment.CstIdentifier = cstIdentifier;
    entities.NewCashReceiptBalanceAdjustment.CrvIdentifier = crvIdentifier;
    entities.NewCashReceiptBalanceAdjustment.CrtIIdentifier = crtIIdentifier;
    entities.NewCashReceiptBalanceAdjustment.CstIIdentifier = cstIIdentifier;
    entities.NewCashReceiptBalanceAdjustment.CrvIIdentifier = crvIIdentifier;
    entities.NewCashReceiptBalanceAdjustment.CrrIdentifier = crrIdentifier;
    entities.NewCashReceiptBalanceAdjustment.CreatedTimestamp =
      createdTimestamp;
    entities.NewCashReceiptBalanceAdjustment.AdjustmentAmount =
      adjustmentAmount;
    entities.NewCashReceiptBalanceAdjustment.CreatedBy = createdBy;
    entities.NewCashReceiptBalanceAdjustment.Description = description;
    entities.NewCashReceiptBalanceAdjustment.Populated = true;
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1()
  {
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1",
      (db, command) =>
      {
        db.SetDecimal(
          command, "totalCurrency", local.Decrease1CrAdjAmt.TotalCurrency);
        db.SetInt32(
          command, "crtIIdentifier",
          entities.DecreaseCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.DecreaseCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.DecreaseCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        local.Decrease1CrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2",
      (db, command) =>
      {
        db.SetDecimal(
          command, "totalCurrency", local.Increase1CrAdjAmt.TotalCurrency);
        db.SetInt32(
          command, "crtIIdentifier",
          entities.IncreaseCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.IncreaseCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.IncreaseCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        local.Increase1CrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3()
  {
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3",
      (db, command) =>
      {
        db.SetDecimal(
          command, "totalCurrency", local.Decrease2CrAdjAmt.TotalCurrency);
        db.SetInt32(
          command, "crtIdentifier", entities.DecreaseCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.DecreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.DecreaseCashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        local.Decrease2CrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn4()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn4",
      (db, command) =>
      {
        db.SetDecimal(
          command, "totalCurrency", local.Increase2CrAdjAmt.TotalCurrency);
        db.SetInt32(
          command, "crtIdentifier", entities.IncreaseCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.IncreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.IncreaseCashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        local.Increase2CrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptCashReceiptType1()
  {
    entities.DecreaseCashReceipt.Populated = false;
    entities.DecreaseCashReceiptType.Populated = false;

    return Read("ReadCashReceiptCashReceiptType1",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", import.Decrease.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.DecreaseCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.DecreaseCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.DecreaseCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.DecreaseCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DecreaseCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.DecreaseCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.DecreaseCashReceipt.CheckType =
          db.GetNullableString(reader, 5);
        entities.DecreaseCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 6);
        entities.DecreaseCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 7);
        entities.DecreaseCashReceipt.CashDue = db.GetNullableDecimal(reader, 8);
        entities.DecreaseCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.DecreaseCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.DecreaseCashReceiptType.CategoryIndicator =
          db.GetString(reader, 11);
        entities.DecreaseCashReceipt.Populated = true;
        entities.DecreaseCashReceiptType.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.DecreaseCashReceipt.CashBalanceReason);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.DecreaseCashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptCashReceiptType2()
  {
    entities.IncreaseCashReceipt.Populated = false;
    entities.IncreaseCashReceiptType.Populated = false;

    return Read("ReadCashReceiptCashReceiptType2",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", import.Increase.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.IncreaseCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.IncreaseCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.IncreaseCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.IncreaseCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.IncreaseCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.IncreaseCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.IncreaseCashReceipt.CheckType =
          db.GetNullableString(reader, 5);
        entities.IncreaseCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 6);
        entities.IncreaseCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 7);
        entities.IncreaseCashReceipt.CashDue = db.GetNullableDecimal(reader, 8);
        entities.IncreaseCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.IncreaseCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.IncreaseCashReceiptType.CategoryIndicator =
          db.GetString(reader, 11);
        entities.IncreaseCashReceipt.Populated = true;
        entities.IncreaseCashReceiptType.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.IncreaseCashReceipt.CashBalanceReason);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.IncreaseCashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptEvent1()
  {
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);
    entities.DecreaseCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent1",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.DecreaseCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.DecreaseCashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.DecreaseCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.DecreaseCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DecreaseCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 2);
        entities.DecreaseCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent2()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);
    entities.IncreaseCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent2",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.IncreaseCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.IncreaseCashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.IncreaseCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.IncreaseCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.IncreaseCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 2);
        entities.IncreaseCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptRlnRsn()
  {
    entities.NewCashReceiptRlnRsn.Populated = false;

    return Read("ReadCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptRlnRsn.Code);
      },
      (db, reader) =>
      {
        entities.NewCashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NewCashReceiptRlnRsn.Code = db.GetString(reader, 1);
        entities.NewCashReceiptRlnRsn.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus1()
  {
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.DecreaseCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.DecreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.DecreaseCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus2()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.IncreaseCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.IncreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.IncreaseCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private void UpdateCashReceipt1()
  {
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);

    var cashBalanceAmt = local.Decrease.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = local.Decrease.CashBalanceReason ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.DecreaseCashReceipt.Populated = false;
    Update("UpdateCashReceipt1",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "crvIdentifier", entities.DecreaseCashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.DecreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crtIdentifier", entities.DecreaseCashReceipt.CrtIdentifier);
          
      });

    entities.DecreaseCashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.DecreaseCashReceipt.CashBalanceReason = cashBalanceReason;
    entities.DecreaseCashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.DecreaseCashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.DecreaseCashReceipt.Populated = true;
  }

  private void UpdateCashReceipt2()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);

    var cashBalanceAmt = local.Increase.CashBalanceAmt.GetValueOrDefault();
    var cashBalanceReason = local.Increase.CashBalanceReason ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.IncreaseCashReceipt.Populated = false;
    Update("UpdateCashReceipt2",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "crvIdentifier", entities.IncreaseCashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.IncreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crtIdentifier", entities.IncreaseCashReceipt.CrtIdentifier);
          
      });

    entities.IncreaseCashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.IncreaseCashReceipt.CashBalanceReason = cashBalanceReason;
    entities.IncreaseCashReceipt.LastUpdatedBy = lastUpdatedBy;
    entities.IncreaseCashReceipt.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncreaseCashReceipt.Populated = true;
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
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
    }

    /// <summary>
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
    }

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    private CashReceipt increase;
    private CashReceipt decrease;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
    }

    /// <summary>
    /// A value of IncreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("increaseCrAdjAmt")]
    public Common IncreaseCrAdjAmt
    {
      get => increaseCrAdjAmt ??= new();
      set => increaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of IncreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("increaseNetReceiptAmt")]
    public Common IncreaseNetReceiptAmt
    {
      get => increaseNetReceiptAmt ??= new();
      set => increaseNetReceiptAmt = value;
    }

    /// <summary>
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
    }

    /// <summary>
    /// A value of DecreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("decreaseCrAdjAmt")]
    public Common DecreaseCrAdjAmt
    {
      get => decreaseCrAdjAmt ??= new();
      set => decreaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of DecreaseNetAdjAmt.
    /// </summary>
    [JsonPropertyName("decreaseNetAdjAmt")]
    public Common DecreaseNetAdjAmt
    {
      get => decreaseNetAdjAmt ??= new();
      set => decreaseNetAdjAmt = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceipt increase;
    private Common increaseCrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceipt decrease;
    private Common decreaseCrAdjAmt;
    private Common decreaseNetAdjAmt;
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
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
    }

    /// <summary>
    /// A value of Increase1CrAdjAmt.
    /// </summary>
    [JsonPropertyName("increase1CrAdjAmt")]
    public Common Increase1CrAdjAmt
    {
      get => increase1CrAdjAmt ??= new();
      set => increase1CrAdjAmt = value;
    }

    /// <summary>
    /// A value of Increase2CrAdjAmt.
    /// </summary>
    [JsonPropertyName("increase2CrAdjAmt")]
    public Common Increase2CrAdjAmt
    {
      get => increase2CrAdjAmt ??= new();
      set => increase2CrAdjAmt = value;
    }

    /// <summary>
    /// A value of IncreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("increaseCrAdjAmt")]
    public Common IncreaseCrAdjAmt
    {
      get => increaseCrAdjAmt ??= new();
      set => increaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of IncreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("increaseNetReceiptAmt")]
    public Common IncreaseNetReceiptAmt
    {
      get => increaseNetReceiptAmt ??= new();
      set => increaseNetReceiptAmt = value;
    }

    /// <summary>
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
    }

    /// <summary>
    /// A value of Decrease1CrAdjAmt.
    /// </summary>
    [JsonPropertyName("decrease1CrAdjAmt")]
    public Common Decrease1CrAdjAmt
    {
      get => decrease1CrAdjAmt ??= new();
      set => decrease1CrAdjAmt = value;
    }

    /// <summary>
    /// A value of Decrease2CrAdjAmt.
    /// </summary>
    [JsonPropertyName("decrease2CrAdjAmt")]
    public Common Decrease2CrAdjAmt
    {
      get => decrease2CrAdjAmt ??= new();
      set => decrease2CrAdjAmt = value;
    }

    /// <summary>
    /// A value of DecreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("decreaseCrAdjAmt")]
    public Common DecreaseCrAdjAmt
    {
      get => decreaseCrAdjAmt ??= new();
      set => decreaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of DecreaseNetAdjAmt.
    /// </summary>
    [JsonPropertyName("decreaseNetAdjAmt")]
    public Common DecreaseNetAdjAmt
    {
      get => decreaseNetAdjAmt ??= new();
      set => decreaseNetAdjAmt = value;
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
    /// A value of HardcodedCrtCatCash.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCatCash")]
    public CashReceiptType HardcodedCrtCatCash
    {
      get => hardcodedCrtCatCash ??= new();
      set => hardcodedCrtCatCash = value;
    }

    /// <summary>
    /// A value of HardcodedOver.
    /// </summary>
    [JsonPropertyName("hardcodedOver")]
    public CashReceipt HardcodedOver
    {
      get => hardcodedOver ??= new();
      set => hardcodedOver = value;
    }

    /// <summary>
    /// A value of HardcodedUnder.
    /// </summary>
    [JsonPropertyName("hardcodedUnder")]
    public CashReceipt HardcodedUnder
    {
      get => hardcodedUnder ??= new();
      set => hardcodedUnder = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private CashReceipt increase;
    private Common increase1CrAdjAmt;
    private Common increase2CrAdjAmt;
    private Common increaseCrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceipt decrease;
    private Common decrease1CrAdjAmt;
    private Common decrease2CrAdjAmt;
    private Common decreaseCrAdjAmt;
    private Common decreaseNetAdjAmt;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptType hardcodedCrtCatCash;
    private CashReceipt hardcodedOver;
    private CashReceipt hardcodedUnder;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IncreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("increaseCashReceipt")]
    public CashReceipt IncreaseCashReceipt
    {
      get => increaseCashReceipt ??= new();
      set => increaseCashReceipt = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptEvent")]
    public CashReceiptEvent IncreaseCashReceiptEvent
    {
      get => increaseCashReceiptEvent ??= new();
      set => increaseCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceiptType.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptType")]
    public CashReceiptType IncreaseCashReceiptType
    {
      get => increaseCashReceiptType ??= new();
      set => increaseCashReceiptType = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("decreaseCashReceipt")]
    public CashReceipt DecreaseCashReceipt
    {
      get => decreaseCashReceipt ??= new();
      set => decreaseCashReceipt = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptEvent")]
    public CashReceiptEvent DecreaseCashReceiptEvent
    {
      get => decreaseCashReceiptEvent ??= new();
      set => decreaseCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptType.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptType")]
    public CashReceiptType DecreaseCashReceiptType
    {
      get => decreaseCashReceiptType ??= new();
      set => decreaseCashReceiptType = value;
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
    /// A value of TotalAdjCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("totalAdjCashReceiptRlnRsn")]
    public CashReceiptRlnRsn TotalAdjCashReceiptRlnRsn
    {
      get => totalAdjCashReceiptRlnRsn ??= new();
      set => totalAdjCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of TotalAdjCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("totalAdjCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment TotalAdjCashReceiptBalanceAdjustment
    {
      get => totalAdjCashReceiptBalanceAdjustment ??= new();
      set => totalAdjCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of NewCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("newCashReceiptRlnRsn")]
    public CashReceiptRlnRsn NewCashReceiptRlnRsn
    {
      get => newCashReceiptRlnRsn ??= new();
      set => newCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of NewCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("newCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment NewCashReceiptBalanceAdjustment
    {
      get => newCashReceiptBalanceAdjustment ??= new();
      set => newCashReceiptBalanceAdjustment = value;
    }

    private CashReceipt increaseCashReceipt;
    private CashReceiptEvent increaseCashReceiptEvent;
    private CashReceiptType increaseCashReceiptType;
    private CashReceipt decreaseCashReceipt;
    private CashReceiptEvent decreaseCashReceiptEvent;
    private CashReceiptType decreaseCashReceiptType;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private CashReceiptRlnRsn totalAdjCashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment totalAdjCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn newCashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment newCashReceiptBalanceAdjustment;
  }
#endregion
}
