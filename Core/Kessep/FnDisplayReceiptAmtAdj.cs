// Program: FN_DISPLAY_RECEIPT_AMT_ADJ, ID: 372342874, model: 746.
// Short name: SWE02442
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISPLAY_RECEIPT_AMT_ADJ.
/// </summary>
[Serializable]
public partial class FnDisplayReceiptAmtAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_RECEIPT_AMT_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayReceiptAmtAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayReceiptAmtAdj.
  /// </summary>
  public FnDisplayReceiptAmtAdj(IContext context, Import import, Export export):
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
    // 10/20/99  J. Katz		Added logic to support two new
    // 				adjustment reason codes.
    // ---------------------------------------------------------------
    local.MaximumDiscontinue.Date = UseCabSetMaximumDiscontinueDate();

    // ---------------------------------------------------------------
    // Read for the Adjustment Reason Code.
    // ---------------------------------------------------------------
    if (!IsEmpty(import.CashReceiptRlnRsn.Code))
    {
      if (ReadCashReceiptRlnRsn())
      {
        MoveCashReceiptRlnRsn(entities.CashReceiptRlnRsn,
          export.CashReceiptRlnRsn);
      }
      else
      {
        MoveCashReceiptRlnRsn(import.CashReceiptRlnRsn, export.CashReceiptRlnRsn);
          
        ExitState = "FN0093_CASH_RCPT_RLN_RSN_NF";
      }
    }

    // ---------------------------------------------------------------
    // Retrieve information for Increase cash receipt.
    // ---------------------------------------------------------------
    if (import.Increase.SequentialNumber > 0)
    {
      if (ReadCashReceipt2())
      {
        export.IncreaseCashReceipt.Assign(entities.IncreaseCashReceipt);

        if (ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.IncreaseCashReceiptEvent.Assign(
            entities.IncreaseCashReceiptEvent);
          MoveCashReceiptSourceType(entities.IncreaseCashReceiptSourceType,
            export.IncreaseCashReceiptSourceType);
          local.Increase1CrAdjAmt.Flag = "Y";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
        }
      }
      else
      {
        export.IncreaseCashReceipt.SequentialNumber =
          import.Increase.SequentialNumber;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_INCR_CASH_RECEIPT_NF";
        }
      }
    }

    // ---------------------------------------------------------------
    // Retrieve information for Decrease cash receipt.
    // ---------------------------------------------------------------
    if (import.Decrease.SequentialNumber > 0)
    {
      if (ReadCashReceipt1())
      {
        export.DecreaseCashReceipt.Assign(entities.DecreaseCashReceipt);

        if (ReadCashReceiptEventCashReceiptSourceType())
        {
          export.DecreaseCashReceiptEvent.Assign(
            entities.DecreaseCashReceiptEvent);
          MoveCashReceiptSourceType(entities.DecreaseCashReceiptSourceType,
            export.DecreaseCashReceiptSourceType);
          local.Decrease1CrAdjAmt.Flag = "Y";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
        }

        // ---------------------------------------------------------------
        // Retrieve the current status for the Decrease receipt.
        // The status is necessary for a new edit requiring a status of DEP for 
        // the ADDPMT action.
        // JLK  07/23/99
        // ---------------------------------------------------------------
        if (ReadCashReceiptStatus())
        {
          export.DecreaseCashReceiptStatus.SystemGeneratedIdentifier =
            entities.DecreaseCashReceiptStatus.SystemGeneratedIdentifier;
        }
        else
        {
          // --->  No action required.  This receipt will be rejected for
          //       the ADDPMT action since the status is not DEP.
        }
      }
      else
      {
        export.DecreaseCashReceipt.SequentialNumber =
          import.Decrease.SequentialNumber;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_DECR_CASH_RECEIPT_NF";
        }
        else if (IsExitState("FN0000_INCR_CASH_RECEIPT_NF"))
        {
          ExitState = "FN0000_CASH_RECEIPT_NF";
        }
        else
        {
          // ---> exit state remains the same ... continue
        }
      }
    }

    // ---------------------------------------------------------------
    // Calculate the Receipt Adj Amt as the sum of all existing
    // Cash Receipt Balance Adjustments that increase or
    // decrease the Receipt Amount of the INCREASE cash receipt.
    // Added new adjustment reason codes PROCCSTFEE and
    // NETINTFERR, both which are used in conjunction with
    // BOGUS type cash receipts.   JLK  10/20/99
    // ---------------------------------------------------------------
    if (AsChar(local.Increase1CrAdjAmt.Flag) == 'Y')
    {
      local.Increase1CrAdjAmt.TotalCurrency = 0;
      local.Increase2CrAdjAmt.TotalCurrency = 0;
      ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2();
      ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn4();
      export.IncreaseCrAdjAmt.TotalCurrency =
        local.Increase1CrAdjAmt.TotalCurrency + local
        .Increase2CrAdjAmt.TotalCurrency;

      // ---------------------------------------------------------------
      // Calculate the Net Receipt Amount for the INCREASE cash receipt.
      // ---------------------------------------------------------------
      export.IncreaseNetReceiptAmt.TotalCurrency =
        export.IncreaseCashReceipt.ReceiptAmount + export
        .IncreaseCrAdjAmt.TotalCurrency;
    }

    // ---------------------------------------------------------------
    // Calculate the Receipt Adj Amt as the sum of all existing
    // Cash Receipt Balance Adjustments that increase or
    // decrease the Receipt Amount of the DECREASE cash receipt.
    // Added new adjustment reason codes PROCCSTFEE and
    // NETINTFERR, both which are used in conjunction with
    // BOGUS type cash receipts.   JLK  10/20/99
    // ---------------------------------------------------------------
    if (AsChar(local.Decrease1CrAdjAmt.Flag) == 'Y')
    {
      local.Decrease1CrAdjAmt.TotalCurrency = 0;
      local.Decrease2CrAdjAmt.TotalCurrency = 0;
      ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1();
      ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3();
      export.DecreaseCrAdjAmt.TotalCurrency =
        local.Decrease1CrAdjAmt.TotalCurrency + local
        .Decrease2CrAdjAmt.TotalCurrency;

      // ---------------------------------------------------------------
      // Calculate the Net Receipt Amount for the Decrease cash receipt.
      // ---------------------------------------------------------------
      export.DecreaseNetReceiptAmt.TotalCurrency =
        export.DecreaseCashReceipt.ReceiptAmount + export
        .DecreaseCrAdjAmt.TotalCurrency;
    }

    // ---------------------------------------------------------------
    // If the exit state is all ok but the Adjustment Reason Code is
    // spaces or the increase receipt number is zero or the
    // decrease receipt number is zero, then the exit state is reset
    // to partial display ok.    JLK  05/29/99
    // ---------------------------------------------------------------
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsEmpty(import.CashReceiptRlnRsn.Code) || import
        .Increase.SequentialNumber == 0 || import.Decrease.SequentialNumber == 0
        )
      {
        ExitState = "FN0000_PARTIAL_DISPLAY_OK";

        return;
      }
    }
    else
    {
      return;
    }

    // ---------------------------------------------------------------
    // Read the Cash Receipt Balance Adjustment information.
    // ---------------------------------------------------------------
    if (Equal(import.CashReceiptBalanceAdjustment.CreatedTimestamp,
      local.Null1.Timestamp))
    {
      // ----------------------------------------------------------
      // A Cash Receipt Balance Adjustment Reason was not
      // selected from the list screen.  See if an adjustment
      // exists for these Cash Receipts and this Relationship
      // Reason.  If more than one exists, make the user select
      // an adjustment from the list screen.
      // ----------------------------------------------------------
      local.Common.Count = 0;

      foreach(var item in ReadCashReceiptBalanceAdjustment2())
      {
        ++local.Common.Count;
      }

      switch(local.Common.Count)
      {
        case 0:
          ExitState = "ACO_NI0000_NO_ITEMS_FOUND";

          break;
        case 1:
          export.CashReceiptBalanceAdjustment.Assign(
            entities.CashReceiptBalanceAdjustment);

          break;
        default:
          ExitState = "FN0000_MULTIPLE_CR_BALANCE_ADJ";

          break;
      }
    }
    else
    {
      // Read and display the imported cash receipt adjustment.
      // -------------------------------------------------------
      if (ReadCashReceiptBalanceAdjustment1())
      {
        export.CashReceiptBalanceAdjustment.Assign(
          entities.CashReceiptBalanceAdjustment);
      }
      else
      {
        ExitState = "FN0031_CASH_RCPT_BAL_ADJ_NF";
      }
    }
  }

  private static void MoveCashReceiptRlnRsn(CashReceiptRlnRsn source,
    CashReceiptRlnRsn target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
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
        entities.DecreaseCashReceipt.CheckType =
          db.GetNullableString(reader, 5);
        entities.DecreaseCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 6);
        entities.DecreaseCashReceipt.CashDue = db.GetNullableDecimal(reader, 7);
        entities.DecreaseCashReceipt.Populated = true;
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
        entities.IncreaseCashReceipt.CheckType =
          db.GetNullableString(reader, 5);
        entities.IncreaseCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 6);
        entities.IncreaseCashReceipt.CashDue = db.GetNullableDecimal(reader, 7);
        entities.IncreaseCashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptBalanceAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);
    entities.CashReceiptBalanceAdjustment.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustment1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.CashReceiptBalanceAdjustment.CreatedTimestamp.
            GetValueOrDefault());
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
          
        db.SetInt32(
          command, "crrIdentifier",
          entities.CashReceiptRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptBalanceAdjustment.AdjustmentAmount =
          db.GetDecimal(reader, 8);
        entities.CashReceiptBalanceAdjustment.CreatedBy =
          db.GetString(reader, 9);
        entities.CashReceiptBalanceAdjustment.Description =
          db.GetNullableString(reader, 10);
        entities.CashReceiptBalanceAdjustment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptBalanceAdjustment2()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);
    entities.CashReceiptBalanceAdjustment.Populated = false;

    return ReadEach("ReadCashReceiptBalanceAdjustment2",
      (db, command) =>
      {
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
          
        db.SetInt32(
          command, "crrIdentifier",
          entities.CashReceiptRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptBalanceAdjustment.AdjustmentAmount =
          db.GetDecimal(reader, 8);
        entities.CashReceiptBalanceAdjustment.CreatedBy =
          db.GetString(reader, 9);
        entities.CashReceiptBalanceAdjustment.Description =
          db.GetNullableString(reader, 10);
        entities.CashReceiptBalanceAdjustment.Populated = true;

        return true;
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

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);
    entities.DecreaseCashReceiptSourceType.Populated = false;
    entities.DecreaseCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
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
        entities.DecreaseCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DecreaseCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DecreaseCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.DecreaseCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.DecreaseCashReceiptSourceType.Code = db.GetString(reader, 4);
        entities.DecreaseCashReceiptSourceType.Populated = true;
        entities.DecreaseCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptRlnRsn()
  {
    entities.CashReceiptRlnRsn.Populated = false;

    return Read("ReadCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptRlnRsn.Code);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptRlnRsn.Code = db.GetString(reader, 1);
        entities.CashReceiptRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CashReceiptRlnRsn.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceTypeCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.IncreaseCashReceipt.Populated);
    entities.IncreaseCashReceiptSourceType.Populated = false;
    entities.IncreaseCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptSourceTypeCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.IncreaseCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.IncreaseCashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.IncreaseCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.IncreaseCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.IncreaseCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.IncreaseCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.IncreaseCashReceiptEvent.ReceivedDate = db.GetDate(reader, 3);
        entities.IncreaseCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 4);
        entities.IncreaseCashReceiptSourceType.Populated = true;
        entities.IncreaseCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.DecreaseCashReceipt.Populated);
    entities.DecreaseCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.DecreaseCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.DecreaseCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.DecreaseCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DecreaseCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DecreaseCashReceiptStatus.Populated = true;
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
    /// A value of IncreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptSourceType")]
    public CashReceiptSourceType IncreaseCashReceiptSourceType
    {
      get => increaseCashReceiptSourceType ??= new();
      set => increaseCashReceiptSourceType = value;
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
    /// A value of IncreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("increaseCashReceipt")]
    public CashReceipt IncreaseCashReceipt
    {
      get => increaseCashReceipt ??= new();
      set => increaseCashReceipt = value;
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
    /// A value of DecreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptSourceType")]
    public CashReceiptSourceType DecreaseCashReceiptSourceType
    {
      get => decreaseCashReceiptSourceType ??= new();
      set => decreaseCashReceiptSourceType = value;
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
    /// A value of DecreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("decreaseCashReceipt")]
    public CashReceipt DecreaseCashReceipt
    {
      get => decreaseCashReceipt ??= new();
      set => decreaseCashReceipt = value;
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

    /// <summary>
    /// A value of DecreaseCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptStatus")]
    public CashReceiptStatus DecreaseCashReceiptStatus
    {
      get => decreaseCashReceiptStatus ??= new();
      set => decreaseCashReceiptStatus = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptSourceType increaseCashReceiptSourceType;
    private CashReceiptEvent increaseCashReceiptEvent;
    private CashReceipt increaseCashReceipt;
    private Common increaseCrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceiptSourceType decreaseCashReceiptSourceType;
    private CashReceiptEvent decreaseCashReceiptEvent;
    private CashReceipt decreaseCashReceipt;
    private Common decreaseCrAdjAmt;
    private Common decreaseNetReceiptAmt;
    private CashReceiptStatus decreaseCashReceiptStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private DateWorkArea maximumDiscontinue;
    private DateWorkArea null1;
    private Common common;
    private Common increase1CrAdjAmt;
    private Common increase2CrAdjAmt;
    private Common decrease1CrAdjAmt;
    private Common decrease2CrAdjAmt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of IncreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptSourceType")]
    public CashReceiptSourceType IncreaseCashReceiptSourceType
    {
      get => increaseCashReceiptSourceType ??= new();
      set => increaseCashReceiptSourceType = value;
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
    /// A value of IncreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("increaseCashReceipt")]
    public CashReceipt IncreaseCashReceipt
    {
      get => increaseCashReceipt ??= new();
      set => increaseCashReceipt = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptSourceType")]
    public CashReceiptSourceType DecreaseCashReceiptSourceType
    {
      get => decreaseCashReceiptSourceType ??= new();
      set => decreaseCashReceiptSourceType = value;
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
    /// A value of DecreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("decreaseCashReceipt")]
    public CashReceipt DecreaseCashReceipt
    {
      get => decreaseCashReceipt ??= new();
      set => decreaseCashReceipt = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptStatusHistory")]
    public CashReceiptStatusHistory DecreaseCashReceiptStatusHistory
    {
      get => decreaseCashReceiptStatusHistory ??= new();
      set => decreaseCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptStatus")]
    public CashReceiptStatus DecreaseCashReceiptStatus
    {
      get => decreaseCashReceiptStatus ??= new();
      set => decreaseCashReceiptStatus = value;
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
    private CashReceiptSourceType increaseCashReceiptSourceType;
    private CashReceiptEvent increaseCashReceiptEvent;
    private CashReceipt increaseCashReceipt;
    private CashReceiptSourceType decreaseCashReceiptSourceType;
    private CashReceiptEvent decreaseCashReceiptEvent;
    private CashReceipt decreaseCashReceipt;
    private CashReceiptStatusHistory decreaseCashReceiptStatusHistory;
    private CashReceiptStatus decreaseCashReceiptStatus;
    private CashReceiptBalanceAdjustment totalCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn totalCashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
  }
#endregion
}
