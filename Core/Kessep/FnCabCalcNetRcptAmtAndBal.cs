// Program: FN_CAB_CALC_NET_RCPT_AMT_AND_BAL, ID: 372341713, model: 746.
// Short name: SWE02435
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_CALC_NET_RCPT_AMT_AND_BAL.
/// </summary>
[Serializable]
public partial class FnCabCalcNetRcptAmtAndBal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CALC_NET_RCPT_AMT_AND_BAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCalcNetRcptAmtAndBal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCalcNetRcptAmtAndBal.
  /// </summary>
  public FnCabCalcNetRcptAmtAndBal(IContext context, Import import,
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
    // ---------------------------------------------------------------
    // This CAB will calculate the Receipt Adj Amt and the Net
    // Receipt Amount for a specified cash receipt.
    // The initial exit state coming into the CAB should be ALL OK.
    // The imported Cash Receipt Sequential Number is mandatory.
    // 06/03/99  J. Katz	Analyzed READ statements and changed
    // 			read property to Select Only where
    // 			appropriate.
    // ---------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (import.CashReceipt.SequentialNumber == 0)
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // ---------------------------------------------------------------
    // Read the imported cash receipt.
    // ---------------------------------------------------------------
    if (ReadCashReceipt())
    {
      // --->  OK to continue
    }
    else
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // ---------------------------------------------------------------
    // Determine if receipt amount adjustments exist.
    // Added hardcoding for new Cash Receipt Rln Rsn codes.
    // JLK  10/20/99
    // ---------------------------------------------------------------
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1();

    // ---------------------------------------------------------------
    // Calculate the total adjustments that increase the receipt
    // amount of the selected cash receipt.
    // Added hardcoding for new Cash Receipt Rln Rsn codes.
    // JLK  10/20/99
    // ---------------------------------------------------------------
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2();

    // ---------------------------------------------------------------
    // Calculate the total adjustments that decrease the receipt
    // amount of the selected cash receipt.
    // Added hardcoding for new Cash Receipt Rln Rsn codes.
    // JLK  10/20/99
    // ---------------------------------------------------------------
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3();

    // ---------------------------------------------------------------
    // Calculate values for export views.
    // ---------------------------------------------------------------
    export.TotalCrAdjAmt.TotalCurrency = local.IncrCrAdjAmt.TotalCurrency + local
      .DecrCrAdjAmt.TotalCurrency;
    export.NetReceiptAmt.TotalCurrency = import.CashReceipt.ReceiptAmount + local
      .IncrCrAdjAmt.TotalCurrency + local.DecrCrAdjAmt.TotalCurrency;
    export.NewBal.CashBalanceAmt =
      entities.Existing.CashDue.GetValueOrDefault() - export
      .NetReceiptAmt.TotalCurrency;

    // ---------------------------------------------------------------
    // Determine value of cash balance reason.
    // ---------------------------------------------------------------
    if (export.NewBal.CashBalanceAmt.GetValueOrDefault() > 0)
    {
      export.NewBal.CashBalanceReason = "UNDER";
    }
    else if (export.NewBal.CashBalanceAmt.GetValueOrDefault() < 0)
    {
      export.NewBal.CashBalanceReason = "OVER";
    }
    else
    {
      export.NewBal.CashBalanceReason = "";
    }
  }

  private bool ReadCashReceipt()
  {
    entities.Existing.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.Existing.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Existing.CstIdentifier = db.GetInt32(reader, 1);
        entities.Existing.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Existing.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.Existing.SequentialNumber = db.GetInt32(reader, 4);
        entities.Existing.CashDue = db.GetNullableDecimal(reader, 5);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1",
      (db, command) =>
      {
        db.SetInt32(command, "crtIIdentifier", entities.Existing.CrtIdentifier);
        db.SetInt32(command, "cstIIdentifier", entities.Existing.CstIdentifier);
        db.SetInt32(command, "crvIIdentifier", entities.Existing.CrvIdentifier);
      },
      (db, reader) =>
      {
        export.QtyReceiptAmtAdj.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2",
      (db, command) =>
      {
        db.
          SetDecimal(command, "totalCurrency", local.IncrCrAdjAmt.TotalCurrency);
          
        db.SetInt32(command, "crtIIdentifier", entities.Existing.CrtIdentifier);
        db.SetInt32(command, "cstIIdentifier", entities.Existing.CstIdentifier);
        db.SetInt32(command, "crvIIdentifier", entities.Existing.CrvIdentifier);
      },
      (db, reader) =>
      {
        local.IncrCrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3",
      (db, command) =>
      {
        db.
          SetDecimal(command, "totalCurrency", local.DecrCrAdjAmt.TotalCurrency);
          
        db.SetInt32(command, "crtIdentifier", entities.Existing.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", entities.Existing.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", entities.Existing.CrvIdentifier);
      },
      (db, reader) =>
      {
        local.DecrCrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
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
    /// <summary>
    /// A value of QtyReceiptAmtAdj.
    /// </summary>
    [JsonPropertyName("qtyReceiptAmtAdj")]
    public Common QtyReceiptAmtAdj
    {
      get => qtyReceiptAmtAdj ??= new();
      set => qtyReceiptAmtAdj = value;
    }

    /// <summary>
    /// A value of NewBal.
    /// </summary>
    [JsonPropertyName("newBal")]
    public CashReceipt NewBal
    {
      get => newBal ??= new();
      set => newBal = value;
    }

    /// <summary>
    /// A value of TotalCrAdjAmt.
    /// </summary>
    [JsonPropertyName("totalCrAdjAmt")]
    public Common TotalCrAdjAmt
    {
      get => totalCrAdjAmt ??= new();
      set => totalCrAdjAmt = value;
    }

    /// <summary>
    /// A value of NetReceiptAmt.
    /// </summary>
    [JsonPropertyName("netReceiptAmt")]
    public Common NetReceiptAmt
    {
      get => netReceiptAmt ??= new();
      set => netReceiptAmt = value;
    }

    private Common qtyReceiptAmtAdj;
    private CashReceipt newBal;
    private Common totalCrAdjAmt;
    private Common netReceiptAmt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of IncrCrAdjAmt.
    /// </summary>
    [JsonPropertyName("incrCrAdjAmt")]
    public Common IncrCrAdjAmt
    {
      get => incrCrAdjAmt ??= new();
      set => incrCrAdjAmt = value;
    }

    /// <summary>
    /// A value of DecrCrAdjAmt.
    /// </summary>
    [JsonPropertyName("decrCrAdjAmt")]
    public Common DecrCrAdjAmt
    {
      get => decrCrAdjAmt ??= new();
      set => decrCrAdjAmt = value;
    }

    private Common incrCrAdjAmt;
    private Common decrCrAdjAmt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CashReceipt Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of TotalReceiptAmtCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("totalReceiptAmtCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment TotalReceiptAmtCashReceiptBalanceAdjustment
      
    {
      get => totalReceiptAmtCashReceiptBalanceAdjustment ??= new();
      set => totalReceiptAmtCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of TotalReceiptAmtCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("totalReceiptAmtCashReceiptRlnRsn")]
    public CashReceiptRlnRsn TotalReceiptAmtCashReceiptRlnRsn
    {
      get => totalReceiptAmtCashReceiptRlnRsn ??= new();
      set => totalReceiptAmtCashReceiptRlnRsn = value;
    }

    private CashReceipt existing;
    private CashReceiptBalanceAdjustment totalReceiptAmtCashReceiptBalanceAdjustment;
      
    private CashReceiptRlnRsn totalReceiptAmtCashReceiptRlnRsn;
  }
#endregion
}
