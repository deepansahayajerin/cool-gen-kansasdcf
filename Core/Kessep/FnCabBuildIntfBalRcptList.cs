// Program: FN_CAB_BUILD_INTF_BAL_RCPT_LIST, ID: 372340032, model: 746.
// Short name: SWE02437
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_BUILD_INTF_BAL_RCPT_LIST.
/// </summary>
[Serializable]
public partial class FnCabBuildIntfBalRcptList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_BUILD_INTF_BAL_RCPT_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabBuildIntfBalRcptList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabBuildIntfBalRcptList.
  /// </summary>
  public FnCabBuildIntfBalRcptList(IContext context, Import import,
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
    //                          Change Log
    // ---------------------------------------------------------------
    // Date      Developer		Description
    // ---------------------------------------------------------------
    // 06/08/99  J. Katz		Analyzed READ statements and
    // 				changed read property to Select
    // 				Only where appropriate.
    // ---------------------------------------------------------------
    // 10/20/99  P Phinney    H00077900        Add two New
    // Cash_Receipt_Rln_Rsn
    // PROCCSTFEE and NETINTFERR
    // for use with creating
    // Receipt Amount Adjustments
    // on CRIA
    // -------------------------------------------------------------------
    // ---------------------------------------------------------------
    // The initial exit state coming into the CAB should be ALL OK.
    // The imported Cash Receipt Sequential Number is mandatory.
    // ---------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------------------------------------------------------
    // Set up local views
    // ----------------------------------------------------------------
    local.Low.Date = new DateTime(1, 1, 1);
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // ----------------------------------------------------------------
    // If the "TO" date is null or 0001-01-01, use the max date for
    // the end of the date range.
    // Validate the "FROM" and "TO" date range.
    // ----------------------------------------------------------------
    if (Lt(local.Low.Date, import.To.Date))
    {
      local.ToDate.Date = import.To.Date;
    }
    else
    {
      local.ToDate.Date = local.Max.Date;
    }

    if (Lt(local.ToDate.Date, import.From.Date))
    {
      ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

      return;
    }

    // ----------------------------------------------------------------
    // Validate Source Code
    // ----------------------------------------------------------------
    if (ReadCashReceiptSourceType())
    {
      if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) != 'Y')
      {
        ExitState = "FN0000_CR_SRC_TYPE_NOT_INTERFACE";

        return;
      }
    }
    else
    {
      ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

      return;
    }

    // ----------------------------------------------------------------
    // Find receipts for the imported source code that are interface
    // receipts [source creation date not null] within the specified
    // date range.
    // ----------------------------------------------------------------
    export.List.Index = -1;

    foreach(var item in ReadCashReceiptCashReceiptEvent())
    {
      // ------------------------------------------------------------
      // Only retrieve Cash Receipts that are a "Cash" category.
      // ------------------------------------------------------------
      if (ReadCashReceiptType())
      {
        if (AsChar(entities.CashReceiptType.CategoryIndicator) != 'C')
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      // ------------------------------------------------------------
      // Skip the receipt if the active status is DEL.
      // JLK  05/15/99
      // ------------------------------------------------------------
      if (ReadCashReceiptStatusHistoryCashReceiptStatus())
      {
        if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier)
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      // ------------------------------------------------------------
      // Skip to the next receipt if only out of balance receipts are
      // to be listed and the balance due on this receipt is zero.
      // ------------------------------------------------------------
      if (AsChar(import.OutOfBalanceOnly.OneChar) == 'Y' && Equal
        (entities.CashReceipt.CashBalanceAmt, 0))
      {
        continue;
      }

      // ------------------------------------------------------------
      // Accumulate Total Balance Due for the entire account.
      // ------------------------------------------------------------
      local.TotalBalanceDue.TotalCurrency += entities.CashReceipt.
        CashBalanceAmt.GetValueOrDefault();

      if (export.List.Index + 1 < Export.ListGroup.Capacity && !
        Lt(entities.CashReceiptEvent.ReceivedDate, import.From.Date) && !
        Lt(local.ToDate.Date, entities.CashReceiptEvent.ReceivedDate))
      {
        // ---------------------------------------------------------
        // Calculate the total adjustments that apply to the Receipt
        // Amount for the current Cash Receipt.
        // ---------------------------------------------------------
        ++export.List.Index;
        export.List.CheckSize();

        local.IncreaseRcptAdjAmt.TotalCurrency = 0;
        local.DecreaseRcptAdjAmt.TotalCurrency = 0;

        // 10/20/99  P Phinney    H00077900        Add two New
        // Cash_Receipt_Rln_Rsn
        // PROCCSTFEE and NETINTFERR
        ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1();

        // 10/20/99  P Phinney    H00077900        Add two New
        // Cash_Receipt_Rln_Rsn
        // PROCCSTFEE and NETINTFERR
        ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2();

        // --------------------------------------------------------
        // Populate the group view.
        // --------------------------------------------------------
        MoveCashReceiptEvent(entities.CashReceiptEvent,
          export.List.Update.MbrCashReceiptEvent);
        export.List.Update.MbrCashReceipt.Assign(entities.CashReceipt);
        MoveCashReceiptType(entities.CashReceiptType,
          export.List.Update.MbrHidden);

        // --------------------------------------------------------
        // Perform calculations for work views.
        // Modified calculation for interface adjustment amount to
        // display negative adjustments as negative amounts per
        // PR # H00075873.    JLK  10/05/99
        // --------------------------------------------------------
        export.List.Update.MbrIntfAdjAmt.TotalCurrency =
          entities.CashReceipt.CashDue.GetValueOrDefault() - entities
          .CashReceipt.TotalCashTransactionAmount.GetValueOrDefault();
        export.List.Update.MbrRcptAdjAmt.TotalCurrency =
          local.IncreaseRcptAdjAmt.TotalCurrency + local
          .DecreaseRcptAdjAmt.TotalCurrency;
        export.List.Update.MbrNetRcptAmt.TotalCurrency =
          entities.CashReceipt.ReceiptAmount + local
          .IncreaseRcptAdjAmt.TotalCurrency + local
          .DecreaseRcptAdjAmt.TotalCurrency;

        // --------------------------------------------------------
        // Accumulate Total Balance Due for the records displayed.
        // --------------------------------------------------------
        local.TotalDisplayBalanceDue.TotalCurrency += entities.CashReceipt.
          CashBalanceAmt.GetValueOrDefault();
      }
    }

    export.TotalBalDue.TotalCurrency = local.TotalBalanceDue.TotalCurrency;
    export.TotalDispBalDue.TotalCurrency =
      local.TotalDisplayBalanceDue.TotalCurrency;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
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
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1",
      (db, command) =>
      {
        db.SetDecimal(
          command, "totalCurrency", local.IncreaseRcptAdjAmt.TotalCurrency);
        db.SetInt32(
          command, "crtIIdentifier", entities.CashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier", entities.CashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier", entities.CashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        local.IncreaseRcptAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2",
      (db, command) =>
      {
        db.SetDecimal(
          command, "totalCurrency", local.DecreaseRcptAdjAmt.TotalCurrency);
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        local.DecreaseRcptAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptEvent()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return ReadEach("ReadCashReceiptCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "sourceCreationDt", local.Low.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.CheckDate = db.GetNullableDate(reader, 5);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 7);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 8);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 9);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 10);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptEvent.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
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
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptStatusHistory.Populated = true;
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of OutOfBalanceOnly.
    /// </summary>
    [JsonPropertyName("outOfBalanceOnly")]
    public Standard OutOfBalanceOnly
    {
      get => outOfBalanceOnly ??= new();
      set => outOfBalanceOnly = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private DateWorkArea from;
    private DateWorkArea to;
    private Standard outOfBalanceOnly;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of MbrCommon.
      /// </summary>
      [JsonPropertyName("mbrCommon")]
      public Common MbrCommon
      {
        get => mbrCommon ??= new();
        set => mbrCommon = value;
      }

      /// <summary>
      /// A value of MbrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrCashReceiptEvent")]
      public CashReceiptEvent MbrCashReceiptEvent
      {
        get => mbrCashReceiptEvent ??= new();
        set => mbrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrCashReceipt")]
      public CashReceipt MbrCashReceipt
      {
        get => mbrCashReceipt ??= new();
        set => mbrCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrIntfAdjAmt.
      /// </summary>
      [JsonPropertyName("mbrIntfAdjAmt")]
      public Common MbrIntfAdjAmt
      {
        get => mbrIntfAdjAmt ??= new();
        set => mbrIntfAdjAmt = value;
      }

      /// <summary>
      /// A value of MbrRcptAdjAmt.
      /// </summary>
      [JsonPropertyName("mbrRcptAdjAmt")]
      public Common MbrRcptAdjAmt
      {
        get => mbrRcptAdjAmt ??= new();
        set => mbrRcptAdjAmt = value;
      }

      /// <summary>
      /// A value of MbrNetRcptAmt.
      /// </summary>
      [JsonPropertyName("mbrNetRcptAmt")]
      public Common MbrNetRcptAmt
      {
        get => mbrNetRcptAmt ??= new();
        set => mbrNetRcptAmt = value;
      }

      /// <summary>
      /// A value of MbrHidden.
      /// </summary>
      [JsonPropertyName("mbrHidden")]
      public CashReceiptType MbrHidden
      {
        get => mbrHidden ??= new();
        set => mbrHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common mbrCommon;
      private CashReceiptEvent mbrCashReceiptEvent;
      private CashReceipt mbrCashReceipt;
      private Common mbrIntfAdjAmt;
      private Common mbrRcptAdjAmt;
      private Common mbrNetRcptAmt;
      private CashReceiptType mbrHidden;
    }

    /// <summary>
    /// A value of TotalBalDue.
    /// </summary>
    [JsonPropertyName("totalBalDue")]
    public Common TotalBalDue
    {
      get => totalBalDue ??= new();
      set => totalBalDue = value;
    }

    /// <summary>
    /// A value of TotalDispBalDue.
    /// </summary>
    [JsonPropertyName("totalDispBalDue")]
    public Common TotalDispBalDue
    {
      get => totalDispBalDue ??= new();
      set => totalDispBalDue = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    private Common totalBalDue;
    private Common totalDispBalDue;
    private Array<ListGroup> list;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of ToDate.
    /// </summary>
    [JsonPropertyName("toDate")]
    public DateWorkArea ToDate
    {
      get => toDate ??= new();
      set => toDate = value;
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
    /// A value of IncreaseRcptAdjAmt.
    /// </summary>
    [JsonPropertyName("increaseRcptAdjAmt")]
    public Common IncreaseRcptAdjAmt
    {
      get => increaseRcptAdjAmt ??= new();
      set => increaseRcptAdjAmt = value;
    }

    /// <summary>
    /// A value of DecreaseRcptAdjAmt.
    /// </summary>
    [JsonPropertyName("decreaseRcptAdjAmt")]
    public Common DecreaseRcptAdjAmt
    {
      get => decreaseRcptAdjAmt ??= new();
      set => decreaseRcptAdjAmt = value;
    }

    /// <summary>
    /// A value of TotalBalanceDue.
    /// </summary>
    [JsonPropertyName("totalBalanceDue")]
    public Common TotalBalanceDue
    {
      get => totalBalanceDue ??= new();
      set => totalBalanceDue = value;
    }

    /// <summary>
    /// A value of TotalDisplayBalanceDue.
    /// </summary>
    [JsonPropertyName("totalDisplayBalanceDue")]
    public Common TotalDisplayBalanceDue
    {
      get => totalDisplayBalanceDue ??= new();
      set => totalDisplayBalanceDue = value;
    }

    private DateWorkArea low;
    private DateWorkArea max;
    private DateWorkArea toDate;
    private CashReceiptStatus hardcodedCrsDeleted;
    private Common increaseRcptAdjAmt;
    private Common decreaseRcptAdjAmt;
    private Common totalBalanceDue;
    private Common totalDisplayBalanceDue;
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
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
  }
#endregion
}
