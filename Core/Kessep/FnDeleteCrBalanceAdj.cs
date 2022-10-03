// Program: FN_DELETE_CR_BALANCE_ADJ, ID: 372342873, model: 746.
// Short name: SWE02441
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_CR_BALANCE_ADJ.
/// </summary>
[Serializable]
public partial class FnDeleteCrBalanceAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_CR_BALANCE_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteCrBalanceAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteCrBalanceAdj.
  /// </summary>
  public FnDeleteCrBalanceAdj(IContext context, Import import, Export export):
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
    // 10/20/99  J. Katz		Add logic to support two new
    // 				adjustment reason codes.
    // ---------------------------------------------------------------
    // ---------------------------------------------------------------
    // The initial exit state coming into the CAB should be ALL OK.
    // The imported Cash Receipt Sequential Number is mandatory.
    // ---------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------------
    // Read increase and decrease cash receipts.
    // ---------------------------------------------------------------
    if (ReadCashReceipt2())
    {
      if (ReadCashReceiptEvent2())
      {
        // ---> OK to continue
      }
      else
      {
        ExitState = "FN0000_CASH_RECEIPT_EVENT_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_INCR_CASH_RECEIPT_NF";

      return;
    }

    if (ReadCashReceipt1())
    {
      if (ReadCashReceiptEvent1())
      {
        // ---> OK to continue
      }
      else
      {
        ExitState = "FN0000_CASH_RECEIPT_EVENT_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_DECR_CASH_RECEIPT_NF";

      return;
    }

    // ---------------------------------------------------------------
    // Read Cash Receipt Balance Adjustment record to be deleted.
    // ---------------------------------------------------------------
    if (ReadCashReceiptBalanceAdjustment())
    {
      // ---> OK to continue
    }
    else
    {
      ExitState = "FN0031_CASH_RCPT_BAL_ADJ_NF";

      return;
    }

    // ---------------------------------------------------------------
    // Calculate Receipt Adj Amt for the Increase cash receipt.
    // Added new adjustment reason codes PROCCSTFEE and
    // NETINTFERR, both which are used in conjunction with
    // BOGUS type cash receipts.   JLK  10/20/99
    // ---------------------------------------------------------------
    local.Increase1CrAdjAmt.TotalCurrency = 0;
    local.Increase2CrAdjAmt.TotalCurrency = 0;
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2();
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn4();
    local.IncreaseCrAdjAmt.TotalCurrency =
      local.Increase1CrAdjAmt.TotalCurrency + local
      .Increase2CrAdjAmt.TotalCurrency - entities.ToBeDeleted.AdjustmentAmount;

    // ---------------------------------------------------------------
    // Calculate the Net Receipt Amount for the Increase cash receipt.
    // ---------------------------------------------------------------
    local.IncreaseNetReceiptAmt.TotalCurrency =
      entities.IncreaseCashReceipt.ReceiptAmount + local
      .IncreaseCrAdjAmt.TotalCurrency;

    // ---------------------------------------------------------------
    // Calculate Balance Due for the Increase cash receipt if the
    // receipt is an interface receipt.
    // ---------------------------------------------------------------
    if (Lt(local.Null1.Date,
      entities.IncreaseCashReceiptEvent.SourceCreationDate))
    {
      local.Increase.CashBalanceAmt =
        entities.IncreaseCashReceipt.CashDue.GetValueOrDefault() - local
        .IncreaseNetReceiptAmt.TotalCurrency;

      if (local.Increase.CashBalanceAmt.GetValueOrDefault() > 0)
      {
        local.Increase.CashBalanceReason = "UNDER";
      }
      else if (local.Increase.CashBalanceAmt.GetValueOrDefault() < 0)
      {
        local.Increase.CashBalanceReason = "OVER";
      }
      else
      {
        local.Increase.CashBalanceReason = "";
      }
    }

    // ---------------------------------------------------------------
    // Calculate Receipt Adj Amt for the Decrease cash receipt.
    // Added new adjustment reason codes PROCCSTFEE and
    // NETINTFERR, both which are used in conjunction with
    // BOGUS type cash receipts.   JLK  10/20/99
    // ---------------------------------------------------------------
    local.Decrease1CrAdjAmt.TotalCurrency = 0;
    local.Decrease2CrAdjAmt.TotalCurrency = 0;
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1();
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3();
    local.DecreaseCrAdjAmt.TotalCurrency =
      local.Decrease1CrAdjAmt.TotalCurrency + local
      .Decrease2CrAdjAmt.TotalCurrency + entities.ToBeDeleted.AdjustmentAmount;

    // ---------------------------------------------------------------
    // Calculate the Net Receipt Amount for the Decrease cash receipt.
    // ---------------------------------------------------------------
    local.DecreaseNetReceiptAmt.TotalCurrency =
      entities.DecreaseCashReceipt.ReceiptAmount + local
      .DecreaseCrAdjAmt.TotalCurrency;

    // ---------------------------------------------------------------
    // Calculate Balance Due for the Decrease cash receipt if the
    // receipt is an interface receipt.
    // ---------------------------------------------------------------
    if (Lt(local.Null1.Date,
      entities.DecreaseCashReceiptEvent.SourceCreationDate))
    {
      local.Decrease.CashBalanceAmt =
        entities.DecreaseCashReceipt.CashDue.GetValueOrDefault() - local
        .DecreaseNetReceiptAmt.TotalCurrency;

      if (local.Decrease.CashBalanceAmt.GetValueOrDefault() > 0)
      {
        local.Decrease.CashBalanceReason = "UNDER";
      }
      else if (local.Decrease.CashBalanceAmt.GetValueOrDefault() < 0)
      {
        local.Decrease.CashBalanceReason = "OVER";
      }
      else
      {
        local.Decrease.CashBalanceReason = "";
      }
    }

    // ---------------------------------------------------------------
    // Perform database actions:
    //   * Delete cash receipt balance adjustment record
    //   * Update Increase cash receipt with new balance due
    //   * Update Decrease cash receipt with new balance due
    // ---------------------------------------------------------------
    local.Current.Timestamp = Now();
    DeleteCashReceiptBalanceAdjustment();

    if (Lt(local.Null1.Date,
      entities.IncreaseCashReceiptEvent.SourceCreationDate))
    {
      try
      {
        UpdateCashReceipt2();

        // ---> OK to continue
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0032_CASH_RCPT_BAL_ADJ_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0033_CASH_RCPT_BAL_ADJ_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (Lt(local.Null1.Date,
      entities.DecreaseCashReceiptEvent.SourceCreationDate))
    {
      try
      {
        UpdateCashReceipt1();

        // ---> OK to continue
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0032_CASH_RCPT_BAL_ADJ_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0033_CASH_RCPT_BAL_ADJ_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ---------------------------------------------------------------
    // Populate export views.
    // ---------------------------------------------------------------
    export.Increase.Assign(entities.IncreaseCashReceipt);
    export.IncreaseCrAdjAmt.TotalCurrency =
      local.IncreaseCrAdjAmt.TotalCurrency;
    export.IncreaseNetReceiptAmt.TotalCurrency =
      local.IncreaseNetReceiptAmt.TotalCurrency;
    export.Decrease.Assign(entities.DecreaseCashReceipt);
    export.DecreaseCrAdjAmt.TotalCurrency =
      local.DecreaseCrAdjAmt.TotalCurrency;
    export.DecreaseNetReceiptAmt.TotalCurrency =
      local.DecreaseNetReceiptAmt.TotalCurrency;
  }

  private void DeleteCashReceiptBalanceAdjustment()
  {
    Update("DeleteCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.ToBeDeleted.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.ToBeDeleted.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.ToBeDeleted.CrvIdentifier);
          
        db.SetInt32(
          command, "crtIIdentifier", entities.ToBeDeleted.CrtIIdentifier);
        db.SetInt32(
          command, "cstIIdentifier", entities.ToBeDeleted.CstIIdentifier);
        db.SetInt32(
          command, "crvIIdentifier", entities.ToBeDeleted.CrvIIdentifier);
        db.
          SetInt32(command, "crrIdentifier", entities.ToBeDeleted.CrrIdentifier);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ToBeDeleted.CreatedTimestamp.GetValueOrDefault());
      });
  }

  private bool ReadCashReceipt1()
  {
    entities.DecreaseCashReceipt.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", import.Decrease.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.DecreaseCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.DecreaseCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.DecreaseCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.DecreaseCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.DecreaseCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.DecreaseCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 5);
        entities.DecreaseCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 6);
        entities.DecreaseCashReceipt.CashDue = db.GetNullableDecimal(reader, 7);
        entities.DecreaseCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DecreaseCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.DecreaseCashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.DecreaseCashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceipt2()
  {
    entities.IncreaseCashReceipt.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(command, "cashReceiptId", import.Increase.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.IncreaseCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.IncreaseCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.IncreaseCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.IncreaseCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.IncreaseCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.IncreaseCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 5);
        entities.IncreaseCashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 6);
        entities.IncreaseCashReceipt.CashDue = db.GetNullableDecimal(reader, 7);
        entities.IncreaseCashReceipt.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.IncreaseCashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.IncreaseCashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.IncreaseCashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);
    entities.ToBeDeleted.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetInt32(
          command, "crrIdentifier",
          import.CashReceiptRlnRsn.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIIdentifier",
          entities.IncreaseCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.IncreaseCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.IncreaseCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.DecreaseCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.DecreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.DecreaseCashReceipt.CrvIdentifier);
          
        db.SetDateTime(
          command, "createdTimestamp",
          import.CashReceiptBalanceAdjustment.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ToBeDeleted.CrtIdentifier = db.GetInt32(reader, 0);
        entities.ToBeDeleted.CstIdentifier = db.GetInt32(reader, 1);
        entities.ToBeDeleted.CrvIdentifier = db.GetInt32(reader, 2);
        entities.ToBeDeleted.CrtIIdentifier = db.GetInt32(reader, 3);
        entities.ToBeDeleted.CstIIdentifier = db.GetInt32(reader, 4);
        entities.ToBeDeleted.CrvIIdentifier = db.GetInt32(reader, 5);
        entities.ToBeDeleted.CrrIdentifier = db.GetInt32(reader, 6);
        entities.ToBeDeleted.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.ToBeDeleted.AdjustmentAmount = db.GetDecimal(reader, 8);
        entities.ToBeDeleted.Populated = true;
      });
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
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
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
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
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

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceipt increase;
    private CashReceipt decrease;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of DecreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("decreaseNetReceiptAmt")]
    public Common DecreaseNetReceiptAmt
    {
      get => decreaseNetReceiptAmt ??= new();
      set => decreaseNetReceiptAmt = value;
    }

    private CashReceipt increase;
    private Common increaseCrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceipt decrease;
    private Common decreaseCrAdjAmt;
    private Common decreaseNetReceiptAmt;
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
    /// A value of DecreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("decreaseNetReceiptAmt")]
    public Common DecreaseNetReceiptAmt
    {
      get => decreaseNetReceiptAmt ??= new();
      set => decreaseNetReceiptAmt = value;
    }

    private DateWorkArea current;
    private DateWorkArea null1;
    private CashReceipt increase;
    private Common increaseCrAdjAmt;
    private Common increase1CrAdjAmt;
    private Common increase2CrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceipt decrease;
    private Common decreaseCrAdjAmt;
    private Common decrease1CrAdjAmt;
    private Common decrease2CrAdjAmt;
    private Common decreaseNetReceiptAmt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ToBeDeleted.
    /// </summary>
    [JsonPropertyName("toBeDeleted")]
    public CashReceiptBalanceAdjustment ToBeDeleted
    {
      get => toBeDeleted ??= new();
      set => toBeDeleted = value;
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
    /// A value of TotalCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("totalCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment TotalCashReceiptBalanceAdjustment
    {
      get => totalCashReceiptBalanceAdjustment ??= new();
      set => totalCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of TotalCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("totalCashReceiptRlnRsn")]
    public CashReceiptRlnRsn TotalCashReceiptRlnRsn
    {
      get => totalCashReceiptRlnRsn ??= new();
      set => totalCashReceiptRlnRsn = value;
    }

    private CashReceiptBalanceAdjustment toBeDeleted;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceipt increaseCashReceipt;
    private CashReceiptEvent increaseCashReceiptEvent;
    private CashReceipt decreaseCashReceipt;
    private CashReceiptEvent decreaseCashReceiptEvent;
    private CashReceiptBalanceAdjustment totalCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn totalCashReceiptRlnRsn;
  }
#endregion
}
