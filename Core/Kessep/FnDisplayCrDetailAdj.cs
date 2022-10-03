// Program: FN_DISPLAY_CR_DETAIL_ADJ, ID: 372341435, model: 746.
// Short name: SWE02436
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISPLAY_CR_DETAIL_ADJ.
/// </summary>
[Serializable]
public partial class FnDisplayCrDetailAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_CR_DETAIL_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayCrDetailAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayCrDetailAdj.
  /// </summary>
  public FnDisplayCrDetailAdj(IContext context, Import import, Export export):
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
    // ---------------------------------------------------------------
    // The initial exit state coming into the CAB should be ALL OK.
    // The imported Cash Receipt Sequential Number is mandatory.
    // ---------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (import.CashReceipt.SequentialNumber == 0)
    {
      ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

      return;
    }

    // ---------------------------------------------------------------
    // Read the cash receipt information for the adjusting receipt.
    // ---------------------------------------------------------------
    if (ReadCashReceipt())
    {
      export.CashReceipt.Assign(entities.AdjustingCashReceipt);
    }
    else
    {
      export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
      ExitState = "FN0000_CASH_RECEIPT_NF";

      return;
    }

    if (ReadCashReceiptEventCashReceiptSourceType())
    {
      export.CashReceiptEvent.Assign(entities.AdjustingCashReceiptEvent);
      MoveCashReceiptSourceType(entities.CashReceiptSourceType,
        export.CashReceiptSourceType);

      if (AsChar(entities.CashReceiptSourceType.InterfaceIndicator) != 'Y')
      {
        ExitState = "FN0000_INTF_SOURCE_CD_REQ";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_SOURCE_TYPE_NF";

      return;
    }

    // ---------------------------------------------------------------
    // Determine the Net Receipt Amount and Balance for the
    // adjusting receipt.
    // ---------------------------------------------------------------
    UseFnCabCalcNetRcptAmtAndBal();

    // ---------------------------------------------------------------
    // Generate the list of adjustments for the adjusting receipt.
    // ---------------------------------------------------------------
    export.CseDtlAdj.Count = 0;

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadCashReceiptDetail())
    {
      if (ReadCashReceiptDetailBalanceAdjCashReceiptDetailRlnRsn())
      {
        ++export.CseDtlAdj.Count;
      }
      else
      {
        ExitState = "FN0043_CASH_RCPT_DTL_BAL_ADJ_NF";
        export.Export1.Next();

        return;
      }

      if (!ReadCashReceiptCashReceiptDetail())
      {
        ExitState = "FN0000_ADJUSTED_CR_NF";
        export.Export1.Next();

        return;
      }

      if (!ReadCashReceiptEvent())
      {
        ExitState = "FN0000_CASH_RECEIPT_EVENT_NF";
        export.Export1.Next();

        return;
      }

      // ---------------------------------------------------------------
      // Populate the export group view with the adjustment information.
      // ---------------------------------------------------------------
      MoveCashReceiptDetailRlnRsn(entities.CashReceiptDetailRlnRsn,
        export.Export1.Update.MbrCashReceiptDetailRlnRsn);
      MoveCashReceiptEvent(entities.AdjustedCashReceiptEvent,
        export.Export1.Update.MbrAdjustedCashReceiptEvent);
      export.Export1.Update.MbrAdjustedCashReceipt.SequentialNumber =
        entities.AdjustedCashReceipt.SequentialNumber;
      export.Export1.Update.MbrAdjustedCashReceiptDetail.Assign(
        entities.AdjustedCashReceiptDetail);
      MoveCashReceiptEvent(entities.AdjustingCashReceiptEvent,
        export.Export1.Update.MbrAdjustingCashReceiptEvent);
      export.Export1.Update.MbrAdjustingCashReceipt.SequentialNumber =
        entities.AdjustingCashReceipt.SequentialNumber;
      export.Export1.Update.MbrAdjustingCashReceiptDetail.Assign(
        entities.AdjustingCashReceiptDetail);
      export.Export1.Update.MbrAdjustingCashReceiptDetail.CollectionAmount =
        -export.Export1.Item.MbrAdjustingCashReceiptDetail.CollectionAmount;

      // ---------------------------------------------------------------
      // Format the cash receipt numbers for display purposes.
      // ---------------------------------------------------------------
      UseFnAbConcatCrAndCrd1();
      UseFnAbConcatCrAndCrd2();
      export.Export1.Next();
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
  }

  private static void MoveCashReceiptDetailRlnRsn(
    CashReceiptDetailRlnRsn source, CashReceiptDetailRlnRsn target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseFnAbConcatCrAndCrd1()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.AdjustingCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.AdjustingCashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    export.Export1.Update.MbrAdjustingCrdCrComboNo.CrdCrCombo =
      useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnAbConcatCrAndCrd2()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.AdjustedCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.AdjustedCashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    export.Export1.Update.MbrAdjustedCrdCrComboNo.CrdCrCombo =
      useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnCabCalcNetRcptAmtAndBal()
  {
    var useImport = new FnCabCalcNetRcptAmtAndBal.Import();
    var useExport = new FnCabCalcNetRcptAmtAndBal.Export();

    MoveCashReceipt(entities.AdjustingCashReceipt, useImport.CashReceipt);

    Call(FnCabCalcNetRcptAmtAndBal.Execute, useImport, useExport);

    export.NetRcptAmt.TotalCurrency = useExport.NetReceiptAmt.TotalCurrency;
  }

  private bool ReadCashReceipt()
  {
    entities.AdjustingCashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.AdjustingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.AdjustingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.AdjustingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.AdjustingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.AdjustingCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.AdjustingCashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 5);
        entities.AdjustingCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 6);
        entities.AdjustingCashReceipt.CashDue =
          db.GetNullableDecimal(reader, 7);
        entities.AdjustingCashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailBalanceAdj.Populated);
    entities.AdjustedCashReceipt.Populated = false;
    entities.AdjustedCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetailBalanceAdj.CrdIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptDetailBalanceAdj.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptDetailBalanceAdj.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptDetailBalanceAdj.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.AdjustedCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.AdjustedCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustedCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.AdjustedCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustedCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.AdjustedCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustedCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.AdjustedCashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 4);
        entities.AdjustedCashReceipt.CashBalanceAmt =
          db.GetNullableDecimal(reader, 5);
        entities.AdjustedCashReceipt.CashDue = db.GetNullableDecimal(reader, 6);
        entities.AdjustedCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.AdjustedCashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 8);
        entities.AdjustedCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 9);
        entities.AdjustedCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 10);
        entities.AdjustedCashReceipt.Populated = true;
        entities.AdjustedCashReceiptDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.AdjustingCashReceipt.Populated);

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier",
          entities.AdjustingCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.AdjustingCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.AdjustingCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.AdjustingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.AdjustingCashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.AdjustingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.AdjustingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 6);
        entities.AdjustingCashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailBalanceAdjCashReceiptDetailRlnRsn()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdjustingCashReceiptDetail.Populated);
    entities.CashReceiptDetailBalanceAdj.Populated = false;
    entities.CashReceiptDetailRlnRsn.Populated = false;

    return Read("ReadCashReceiptDetailBalanceAdjCashReceiptDetailRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdSIdentifier",
          entities.AdjustingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvSIdentifier",
          entities.AdjustingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstSIdentifier",
          entities.AdjustingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtSIdentifier",
          entities.AdjustingCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailRlnRsn.SequentialIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailRlnRsn.Code = db.GetString(reader, 10);
        entities.CashReceiptDetailBalanceAdj.Populated = true;
        entities.CashReceiptDetailRlnRsn.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.AdjustedCashReceipt.Populated);
    entities.AdjustedCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.AdjustedCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.AdjustedCashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.AdjustedCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustedCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustedCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.AdjustedCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.AdjustedCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.AdjustingCashReceipt.Populated);
    entities.CashReceiptSourceType.Populated = false;
    entities.AdjustingCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.AdjustingCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.AdjustingCashReceipt.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.AdjustingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustingCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.AdjustingCashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 4);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
        entities.AdjustingCashReceiptEvent.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
      /// A value of MbrCommon.
      /// </summary>
      [JsonPropertyName("mbrCommon")]
      public Common MbrCommon
      {
        get => mbrCommon ??= new();
        set => mbrCommon = value;
      }

      /// <summary>
      /// A value of MbrCashReceiptDetailRlnRsn.
      /// </summary>
      [JsonPropertyName("mbrCashReceiptDetailRlnRsn")]
      public CashReceiptDetailRlnRsn MbrCashReceiptDetailRlnRsn
      {
        get => mbrCashReceiptDetailRlnRsn ??= new();
        set => mbrCashReceiptDetailRlnRsn = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCrdCrComboNo")]
      public CrdCrComboNo MbrAdjustedCrdCrComboNo
      {
        get => mbrAdjustedCrdCrComboNo ??= new();
        set => mbrAdjustedCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceiptEvent")]
      public CashReceiptEvent MbrAdjustedCashReceiptEvent
      {
        get => mbrAdjustedCashReceiptEvent ??= new();
        set => mbrAdjustedCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceipt")]
      public CashReceipt MbrAdjustedCashReceipt
      {
        get => mbrAdjustedCashReceipt ??= new();
        set => mbrAdjustedCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrAdjustedCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("mbrAdjustedCashReceiptDetail")]
      public CashReceiptDetail MbrAdjustedCashReceiptDetail
      {
        get => mbrAdjustedCashReceiptDetail ??= new();
        set => mbrAdjustedCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCrdCrComboNo")]
      public CrdCrComboNo MbrAdjustingCrdCrComboNo
      {
        get => mbrAdjustingCrdCrComboNo ??= new();
        set => mbrAdjustingCrdCrComboNo = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceiptEvent")]
      public CashReceiptEvent MbrAdjustingCashReceiptEvent
      {
        get => mbrAdjustingCashReceiptEvent ??= new();
        set => mbrAdjustingCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceipt.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceipt")]
      public CashReceipt MbrAdjustingCashReceipt
      {
        get => mbrAdjustingCashReceipt ??= new();
        set => mbrAdjustingCashReceipt = value;
      }

      /// <summary>
      /// A value of MbrAdjustingCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("mbrAdjustingCashReceiptDetail")]
      public CashReceiptDetail MbrAdjustingCashReceiptDetail
      {
        get => mbrAdjustingCashReceiptDetail ??= new();
        set => mbrAdjustingCashReceiptDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common mbrCommon;
      private CashReceiptDetailRlnRsn mbrCashReceiptDetailRlnRsn;
      private CrdCrComboNo mbrAdjustedCrdCrComboNo;
      private CashReceiptEvent mbrAdjustedCashReceiptEvent;
      private CashReceipt mbrAdjustedCashReceipt;
      private CashReceiptDetail mbrAdjustedCashReceiptDetail;
      private CrdCrComboNo mbrAdjustingCrdCrComboNo;
      private CashReceiptEvent mbrAdjustingCashReceiptEvent;
      private CashReceipt mbrAdjustingCashReceipt;
      private CashReceiptDetail mbrAdjustingCashReceiptDetail;
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
    /// A value of NetRcptAmt.
    /// </summary>
    [JsonPropertyName("netRcptAmt")]
    public Common NetRcptAmt
    {
      get => netRcptAmt ??= new();
      set => netRcptAmt = value;
    }

    /// <summary>
    /// A value of CseDtlAdj.
    /// </summary>
    [JsonPropertyName("cseDtlAdj")]
    public Common CseDtlAdj
    {
      get => cseDtlAdj ??= new();
      set => cseDtlAdj = value;
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
    private Common netRcptAmt;
    private Common cseDtlAdj;
    private Array<ExportGroup> export1;
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
    /// A value of AdjustingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("adjustingCashReceiptEvent")]
    public CashReceiptEvent AdjustingCashReceiptEvent
    {
      get => adjustingCashReceiptEvent ??= new();
      set => adjustingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of AdjustingCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustingCashReceipt")]
    public CashReceipt AdjustingCashReceipt
    {
      get => adjustingCashReceipt ??= new();
      set => adjustingCashReceipt = value;
    }

    /// <summary>
    /// A value of AdjustingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustingCashReceiptDetail")]
    public CashReceiptDetail AdjustingCashReceiptDetail
    {
      get => adjustingCashReceiptDetail ??= new();
      set => adjustingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
    }

    /// <summary>
    /// A value of AdjustedCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("adjustedCashReceiptEvent")]
    public CashReceiptEvent AdjustedCashReceiptEvent
    {
      get => adjustedCashReceiptEvent ??= new();
      set => adjustedCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of AdjustedCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustedCashReceipt")]
    public CashReceipt AdjustedCashReceipt
    {
      get => adjustedCashReceipt ??= new();
      set => adjustedCashReceipt = value;
    }

    /// <summary>
    /// A value of AdjustedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustedCashReceiptDetail")]
    public CashReceiptDetail AdjustedCashReceiptDetail
    {
      get => adjustedCashReceiptDetail ??= new();
      set => adjustedCashReceiptDetail = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent adjustingCashReceiptEvent;
    private CashReceipt adjustingCashReceipt;
    private CashReceiptDetail adjustingCashReceiptDetail;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
    private CashReceiptEvent adjustedCashReceiptEvent;
    private CashReceipt adjustedCashReceipt;
    private CashReceiptDetail adjustedCashReceiptDetail;
  }
#endregion
}
