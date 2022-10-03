// Program: FN_DISPLAY_RCPT_AMT_ADJ_FOR_CR, ID: 372338781, model: 746.
// Short name: SWE02443
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISPLAY_RCPT_AMT_ADJ_FOR_CR.
/// </summary>
[Serializable]
public partial class FnDisplayRcptAmtAdjForCr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_RCPT_AMT_ADJ_FOR_CR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayRcptAmtAdjForCr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayRcptAmtAdjForCr.
  /// </summary>
  public FnDisplayRcptAmtAdjForCr(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------
    //                          Change Log
    // ---------------------------------------------------------------
    // Date      Developer		Description
    // ---------------------------------------------------------------
    // 06/08/99  J. Katz		Analyzed READ statements and
    // 				changed read property to Select
    // 				Only where appropriate.
    // 10/20/99	J. Katz		Added logic to support new
    // 				Cash Receipt Rln Rsn codes.
    // ---------------------------------------------------------------
    // ---------------------------------------------------------------
    // The initial exit state coming into the CAB should be ALL OK.
    // The imported Cash Receipt Sequential Number is mandatory.
    // ---------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;

      return;
    }

    // ------------------------------------------------------------------
    // Read selected cash receipt for which adjustments are to be listed.
    // ------------------------------------------------------------------
    if (ReadCashReceipt())
    {
      // --->  ok to continue
    }
    else
    {
      export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    if (ReadCashReceiptEventCashReceiptSourceType())
    {
      // --------------------------------------------------------------
      // Populate export views with information about selected
      // cash receipt.
      // --------------------------------------------------------------
      export.CashReceipt.Assign(entities.SelectedCashReceipt);
      export.CashReceiptEvent.Assign(entities.SelectedCashReceiptEvent);
      MoveCashReceiptSourceType(entities.SelectedCashReceiptSourceType,
        export.CashReceiptSourceType);

      // --------------------------------------------------------------
      // Only CR Source Types that submit the Cash Receipts
      // electronically via an interface can be selected.
      // --------------------------------------------------------------
      if (AsChar(entities.SelectedCashReceiptSourceType.InterfaceIndicator) != 'Y'
        )
      {
        export.CashReceiptEvent.ReceivedDate = local.Null1.Date;
        ExitState = "FN0000_CR_SRC_TYPE_NOT_INTERFACE";

        return;
      }
    }
    else
    {
      export.CashReceipt.Assign(entities.SelectedCashReceipt);
      ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

      return;
    }

    // ------------------------------------------------------------------
    // Calculate total cash receipt adjustments for selected cash receipt.
    // Added hardcoding for new Cash Receipt Rln Rsn codes
    // JLK  10/20/99
    // ------------------------------------------------------------------
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1();
    ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2();

    // ------------------------------------------------------------------
    // Calculate Net Receipt Amount for selected cash receipt.
    // ------------------------------------------------------------------
    local.NetReceiptAmt.TotalCurrency =
      entities.SelectedCashReceipt.ReceiptAmount + local
      .IncrCrAdjAmt.TotalCurrency + local.DecrCrAdjAmt.TotalCurrency;
    export.NetReceiptAmt.TotalCurrency = local.NetReceiptAmt.TotalCurrency;

    // ------------------------------------------------------------------
    // Build list of adjustments for the group view.
    // Added hardcoding for new Cash Receipt Rln Rsn codes.
    // JLK  10/20/99
    // ------------------------------------------------------------------
    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3())
    {
      if (!ReadCashReceiptCashReceiptEvent2())
      {
        ExitState = "FN0000_INCR_CASH_RECEIPT_NF";
        export.Export1.Next();

        return;
      }

      if (!ReadCashReceiptCashReceiptEvent1())
      {
        ExitState = "FN0000_DECR_CASH_RECEIPT_NF";
        export.Export1.Next();

        return;
      }

      // ------------------------------------------------------------------
      // Populate group export view.
      // ------------------------------------------------------------------
      export.Export1.Update.MemberIncrCashReceipt.Assign(
        entities.ExistingIncrCashReceipt);
      MoveCashReceiptEvent(entities.ExistingIncrCashReceiptEvent,
        export.Export1.Update.MemberIncrCashReceiptEvent);
      MoveCashReceiptBalanceAdjustment(entities.
        ExistingCashReceiptBalanceAdjustment,
        export.Export1.Update.MemberIncrCashReceiptBalanceAdjustment);
      export.Export1.Update.MemberDecrCashReceipt.Assign(
        entities.ExistingDecrCashReceipt);
      MoveCashReceiptEvent(entities.ExistingDecrCashReceiptEvent,
        export.Export1.Update.MemberDecrCashReceiptEvent);
      MoveCashReceiptBalanceAdjustment(entities.
        ExistingCashReceiptBalanceAdjustment,
        export.Export1.Update.MemberDecrCashReceiptBalanceAdjustment);
      export.Export1.Update.MemberDecrCashReceiptBalanceAdjustment.
        AdjustmentAmount =
          -entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
      MoveCashReceiptRlnRsn(entities.ExistingCashReceiptRlnRsn,
        export.Export1.Update.MemberCashReceiptRlnRsn);
      export.Export1.Next();
    }
  }

  private static void MoveCashReceiptBalanceAdjustment(
    CashReceiptBalanceAdjustment source, CashReceiptBalanceAdjustment target)
  {
    target.AdjustmentAmount = source.AdjustmentAmount;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SourceCreationDate = source.SourceCreationDate;
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

  private bool ReadCashReceipt()
  {
    entities.SelectedCashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.SelectedCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.SelectedCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.SelectedCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.SelectedCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.SelectedCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.SelectedCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 5);
        entities.SelectedCashReceipt.CashDue = db.GetNullableDecimal(reader, 6);
        entities.SelectedCashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1()
  {
    System.Diagnostics.Debug.Assert(entities.SelectedCashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn1",
      (db, command) =>
      {
        db.
          SetDecimal(command, "totalCurrency", local.IncrCrAdjAmt.TotalCurrency);
          
        db.SetInt32(
          command, "crtIIdentifier",
          entities.SelectedCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.SelectedCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.SelectedCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        local.IncrCrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2()
  {
    System.Diagnostics.Debug.Assert(entities.SelectedCashReceipt.Populated);

    return Read("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn2",
      (db, command) =>
      {
        db.
          SetDecimal(command, "totalCurrency", local.DecrCrAdjAmt.TotalCurrency);
          
        db.SetInt32(
          command, "crtIdentifier", entities.SelectedCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.SelectedCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.SelectedCashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        local.DecrCrAdjAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3()
  {
    System.Diagnostics.Debug.Assert(entities.SelectedCashReceipt.Populated);

    return ReadEach("ReadCashReceiptBalanceAdjustmentCashReceiptRlnRsn3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIIdentifier",
          entities.SelectedCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.SelectedCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.SelectedCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

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
        entities.ExistingCashReceiptRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptRlnRsn.Code = db.GetString(reader, 9);
        entities.ExistingCashReceiptBalanceAdjustment.Populated = true;
        entities.ExistingCashReceiptRlnRsn.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptCashReceiptEvent1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCashReceiptBalanceAdjustment.Populated);
    entities.ExistingDecrCashReceipt.Populated = false;
    entities.ExistingDecrCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptCashReceiptEvent1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptBalanceAdjustment.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptBalanceAdjustment.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptBalanceAdjustment.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDecrCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDecrCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDecrCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingDecrCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingDecrCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingDecrCashReceipt.ReceiptAmount =
          db.GetDecimal(reader, 3);
        entities.ExistingDecrCashReceipt.SequentialNumber =
          db.GetInt32(reader, 4);
        entities.ExistingDecrCashReceipt.CheckDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingDecrCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingDecrCashReceipt.Populated = true;
        entities.ExistingDecrCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptCashReceiptEvent2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingCashReceiptBalanceAdjustment.Populated);
    entities.ExistingIncrCashReceipt.Populated = false;
    entities.ExistingIncrCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptCashReceiptEvent2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptBalanceAdjustment.CrtIIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptBalanceAdjustment.CstIIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptBalanceAdjustment.CrvIIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingIncrCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingIncrCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingIncrCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingIncrCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingIncrCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingIncrCashReceipt.ReceiptAmount =
          db.GetDecimal(reader, 3);
        entities.ExistingIncrCashReceipt.SequentialNumber =
          db.GetInt32(reader, 4);
        entities.ExistingIncrCashReceipt.CheckDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingIncrCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingIncrCashReceipt.Populated = true;
        entities.ExistingIncrCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.SelectedCashReceipt.Populated);
    entities.SelectedCashReceiptEvent.Populated = false;
    entities.SelectedCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.SelectedCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.SelectedCashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.SelectedCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.SelectedCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.SelectedCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.SelectedCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.SelectedCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.SelectedCashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 4);
        entities.SelectedCashReceiptSourceType.Code = db.GetString(reader, 5);
        entities.SelectedCashReceiptEvent.Populated = true;
        entities.SelectedCashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.SelectedCashReceiptSourceType.InterfaceIndicator);
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
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of MemberCommon.
      /// </summary>
      [JsonPropertyName("memberCommon")]
      public Common MemberCommon
      {
        get => memberCommon ??= new();
        set => memberCommon = value;
      }

      /// <summary>
      /// A value of MemberIncrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("memberIncrCashReceiptEvent")]
      public CashReceiptEvent MemberIncrCashReceiptEvent
      {
        get => memberIncrCashReceiptEvent ??= new();
        set => memberIncrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MemberIncrCashReceipt.
      /// </summary>
      [JsonPropertyName("memberIncrCashReceipt")]
      public CashReceipt MemberIncrCashReceipt
      {
        get => memberIncrCashReceipt ??= new();
        set => memberIncrCashReceipt = value;
      }

      /// <summary>
      /// A value of MemberIncrCashReceiptBalanceAdjustment.
      /// </summary>
      [JsonPropertyName("memberIncrCashReceiptBalanceAdjustment")]
      public CashReceiptBalanceAdjustment MemberIncrCashReceiptBalanceAdjustment
      {
        get => memberIncrCashReceiptBalanceAdjustment ??= new();
        set => memberIncrCashReceiptBalanceAdjustment = value;
      }

      /// <summary>
      /// A value of MemberDecrCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("memberDecrCashReceiptEvent")]
      public CashReceiptEvent MemberDecrCashReceiptEvent
      {
        get => memberDecrCashReceiptEvent ??= new();
        set => memberDecrCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MemberDecrCashReceipt.
      /// </summary>
      [JsonPropertyName("memberDecrCashReceipt")]
      public CashReceipt MemberDecrCashReceipt
      {
        get => memberDecrCashReceipt ??= new();
        set => memberDecrCashReceipt = value;
      }

      /// <summary>
      /// A value of MemberDecrCashReceiptBalanceAdjustment.
      /// </summary>
      [JsonPropertyName("memberDecrCashReceiptBalanceAdjustment")]
      public CashReceiptBalanceAdjustment MemberDecrCashReceiptBalanceAdjustment
      {
        get => memberDecrCashReceiptBalanceAdjustment ??= new();
        set => memberDecrCashReceiptBalanceAdjustment = value;
      }

      /// <summary>
      /// A value of MemberCashReceiptRlnRsn.
      /// </summary>
      [JsonPropertyName("memberCashReceiptRlnRsn")]
      public CashReceiptRlnRsn MemberCashReceiptRlnRsn
      {
        get => memberCashReceiptRlnRsn ??= new();
        set => memberCashReceiptRlnRsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common memberCommon;
      private CashReceiptEvent memberIncrCashReceiptEvent;
      private CashReceipt memberIncrCashReceipt;
      private CashReceiptBalanceAdjustment memberIncrCashReceiptBalanceAdjustment;
        
      private CashReceiptEvent memberDecrCashReceiptEvent;
      private CashReceipt memberDecrCashReceipt;
      private CashReceiptBalanceAdjustment memberDecrCashReceiptBalanceAdjustment;
        
      private CashReceiptRlnRsn memberCashReceiptRlnRsn;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private Common netReceiptAmt;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    /// <summary>
    /// A value of NetReceiptAmt.
    /// </summary>
    [JsonPropertyName("netReceiptAmt")]
    public Common NetReceiptAmt
    {
      get => netReceiptAmt ??= new();
      set => netReceiptAmt = value;
    }

    private DateWorkArea null1;
    private Common incrCrAdjAmt;
    private Common decrCrAdjAmt;
    private Common netReceiptAmt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SelectedCashReceipt.
    /// </summary>
    [JsonPropertyName("selectedCashReceipt")]
    public CashReceipt SelectedCashReceipt
    {
      get => selectedCashReceipt ??= new();
      set => selectedCashReceipt = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptEvent")]
    public CashReceiptEvent SelectedCashReceiptEvent
    {
      get => selectedCashReceiptEvent ??= new();
      set => selectedCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptSourceType")]
    public CashReceiptSourceType SelectedCashReceiptSourceType
    {
      get => selectedCashReceiptSourceType ??= new();
      set => selectedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of TotalCrAdjAmtCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("totalCrAdjAmtCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment TotalCrAdjAmtCashReceiptBalanceAdjustment
      
    {
      get => totalCrAdjAmtCashReceiptBalanceAdjustment ??= new();
      set => totalCrAdjAmtCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of TotalCrAdjAmtCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("totalCrAdjAmtCashReceiptRlnRsn")]
    public CashReceiptRlnRsn TotalCrAdjAmtCashReceiptRlnRsn
    {
      get => totalCrAdjAmtCashReceiptRlnRsn ??= new();
      set => totalCrAdjAmtCashReceiptRlnRsn = value;
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
    /// A value of ExistingIncrCashReceipt.
    /// </summary>
    [JsonPropertyName("existingIncrCashReceipt")]
    public CashReceipt ExistingIncrCashReceipt
    {
      get => existingIncrCashReceipt ??= new();
      set => existingIncrCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingIncrCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingIncrCashReceiptEvent")]
    public CashReceiptEvent ExistingIncrCashReceiptEvent
    {
      get => existingIncrCashReceiptEvent ??= new();
      set => existingIncrCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingDecrCashReceipt.
    /// </summary>
    [JsonPropertyName("existingDecrCashReceipt")]
    public CashReceipt ExistingDecrCashReceipt
    {
      get => existingDecrCashReceipt ??= new();
      set => existingDecrCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingDecrCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingDecrCashReceiptEvent")]
    public CashReceiptEvent ExistingDecrCashReceiptEvent
    {
      get => existingDecrCashReceiptEvent ??= new();
      set => existingDecrCashReceiptEvent = value;
    }

    private CashReceipt selectedCashReceipt;
    private CashReceiptEvent selectedCashReceiptEvent;
    private CashReceiptSourceType selectedCashReceiptSourceType;
    private CashReceiptBalanceAdjustment totalCrAdjAmtCashReceiptBalanceAdjustment;
      
    private CashReceiptRlnRsn totalCrAdjAmtCashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment existingCashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn existingCashReceiptRlnRsn;
    private CashReceipt existingIncrCashReceipt;
    private CashReceiptEvent existingIncrCashReceiptEvent;
    private CashReceipt existingDecrCashReceipt;
    private CashReceiptEvent existingDecrCashReceiptEvent;
  }
#endregion
}
