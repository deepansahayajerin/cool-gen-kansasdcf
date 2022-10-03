// Program: FN_B777_OCSE34_CRD_SUSPENSE, ID: 371229929, model: 746.
// Short name: SWEB777P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B777_OCSE34_CRD_SUSPENSE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB777Ocse34CrdSuspense: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B777_OCSE34_CRD_SUSPENSE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB777Ocse34CrdSuspense(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB777Ocse34CrdSuspense.
  /// </summary>
  public FnB777Ocse34CrdSuspense(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // ----------------------------------------------
    //                                                                                                                                
    // 12/27/2004 E.SHIRK  	PR224067	OCSE34 One-shot CRD suspense calculation.	
    // ------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.EndDate.Date = new DateTime(2004, 12, 30);
    local.Local2Date.Date = AddDays(local.EndDate.Date, -2);
    local.Local30Date.Date = AddDays(local.EndDate.Date, -30);
    local.Local180Date.Date = AddDays(local.EndDate.Date, -180);
    local.Local365Date.Date = AddDays(local.EndDate.Date, -365);
    local.Local1095Date.Date = AddDays(local.EndDate.Date, -1095);
    local.Local1825Date.Date = AddDays(local.EndDate.Date, -1825);
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      return;
    }

    foreach(var item in ReadCashReceiptDetailCashReceipt())
    {
      if (!ReadCollectionType())
      {
        continue;
      }

      foreach(var item1 in ReadCashReceiptDetailStatHistory())
      {
        if (!Equal(entities.CashReceiptDetailStatHistory.DiscontinueDate,
          local.MaxDate.Date))
        {
          local.CompareDate.Date =
            AddDays(entities.CashReceiptDetailStatHistory.DiscontinueDate, 1);

          break;
        }
      }

      ++local.TotCrdCnt.Count;

      if (Lt(local.CompareDate.Date, local.Local1825Date.Date))
      {
        local.Gt1825UdAmt.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount - (
            entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault()) + local
          .Gt1825UdAmt.TotalCurrency;
        ++local.Gt1825Cnt.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Local1095Date.Date))
      {
        local.Gt1095UdAmt.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount - (
            entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault()) + local
          .Gt1095UdAmt.TotalCurrency;
        ++local.Gt1095Cnt.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Local365Date.Date))
      {
        local.Gt365UdAmt.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount - (
            entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault()) + local
          .Gt365UdAmt.TotalCurrency;
        ++local.Gt365Cnt.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Local180Date.Date))
      {
        local.Gt180UdAmt.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount - (
            entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault()) + local
          .Gt180UdAmt.TotalCurrency;
        ++local.Gt180Cnt.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Local30Date.Date))
      {
        local.Gt30UdAmt.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount - (
            entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault()) + local
          .Gt30UdAmt.TotalCurrency;
        ++local.Gt30Cnt.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Local2Date.Date))
      {
        local.Gt2UdAmt.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount - (
            entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault()) + local
          .Gt2UdAmt.TotalCurrency;
        ++local.Gt2Cnt.Count;

        continue;
      }

      local.LbdUdAmt.TotalCurrency =
        entities.CashReceiptDetail.CollectionAmount - (
          entities.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + entities
        .CashReceiptDetail.RefundedAmount.GetValueOrDefault()) + local
        .LbdUdAmt.TotalCurrency;
      ++local.LbdCnt.Count;
    }

    local.EabFileHandling.Action = "WRITE";

    for(local.LoopCount.Count = 1; local.LoopCount.Count <= 17; ++
      local.LoopCount.Count)
    {
      switch(local.LoopCount.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "*******  OCSE34 CRD Suspense Summary  *******";

          break;
        case 2:
          local.EabReportSend.RptDetail = "Total CRD Count: " + NumberToString
            (local.TotCrdCnt.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Total LBD Count: " + NumberToString
            (local.LbdCnt.Count, 15);

          break;
        case 4:
          local.RoundedUdcAmt.Count =
            (int)Math.Round(
              local.LbdUdAmt.TotalCurrency, MidpointRounding.AwayFromZero);
          local.EabReportSend.RptDetail = "Total LBD Amount: " + NumberToString
            (local.RoundedUdcAmt.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail = "Total GT 2 Day Count: " + NumberToString
            (local.Gt2Cnt.Count, 15);

          break;
        case 6:
          local.RoundedUdcAmt.Count =
            (int)Math.Round(
              local.Gt2UdAmt.TotalCurrency, MidpointRounding.AwayFromZero);
          local.EabReportSend.RptDetail = "Total GT 2 Day Amount: " + NumberToString
            (local.RoundedUdcAmt.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail = "Total GT 30 Day Count: " + NumberToString
            (local.Gt30Cnt.Count, 15);

          break;
        case 8:
          local.RoundedUdcAmt.Count =
            (int)Math.Round(
              local.Gt30UdAmt.TotalCurrency, MidpointRounding.AwayFromZero);
          local.EabReportSend.RptDetail = "Total GT 30 Day Amount: " + NumberToString
            (local.RoundedUdcAmt.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail = "Total GT 180 Day Count: " + NumberToString
            (local.Gt180Cnt.Count, 15);

          break;
        case 10:
          local.RoundedUdcAmt.Count =
            (int)Math.Round(
              local.Gt180UdAmt.TotalCurrency, MidpointRounding.AwayFromZero);
          local.EabReportSend.RptDetail = "Total GT 180 Day Amount: " + NumberToString
            (local.RoundedUdcAmt.Count, 15);

          break;
        case 11:
          local.EabReportSend.RptDetail = "Total GT 365 Day Count: " + NumberToString
            (local.Gt365Cnt.Count, 15);

          break;
        case 12:
          local.RoundedUdcAmt.Count =
            (int)Math.Round(
              local.Gt365UdAmt.TotalCurrency, MidpointRounding.AwayFromZero);
          local.EabReportSend.RptDetail = "Total GT 365 Day Amount: " + NumberToString
            (local.RoundedUdcAmt.Count, 15);

          break;
        case 13:
          local.EabReportSend.RptDetail = "Total GT 1095 Day Count: " + NumberToString
            (local.Gt1095Cnt.Count, 15);

          break;
        case 14:
          local.RoundedUdcAmt.Count =
            (int)Math.Round(
              local.Gt1095UdAmt.TotalCurrency, MidpointRounding.AwayFromZero);
          local.EabReportSend.RptDetail = "Total GT 1095 Day Amount: " + NumberToString
            (local.RoundedUdcAmt.Count, 15);

          break;
        case 15:
          local.EabReportSend.RptDetail = "Total GT 1825 Day Count: " + NumberToString
            (local.Gt1825Cnt.Count, 15);

          break;
        case 16:
          local.RoundedUdcAmt.Count =
            (int)Math.Round(
              local.Gt1825UdAmt.TotalCurrency, MidpointRounding.AwayFromZero);
          local.EabReportSend.RptDetail = "Total GT 1825 Day Amount: " + NumberToString
            (local.RoundedUdcAmt.Count, 15);

          break;
        case 17:
          local.EabReportSend.RptDetail =
            "*************************************************************";

          break;
        default:
          break;
      }

      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceipt()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetDate(
          command, "receiptDate", local.EndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 8);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 12);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 13);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;

    return ReadEach("ReadCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CashReceiptDetailStatHistory.Populated = true;

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
    /// A value of ReportingPeriodEndDate.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndDate")]
    public DateWorkArea ReportingPeriodEndDate
    {
      get => reportingPeriodEndDate ??= new();
      set => reportingPeriodEndDate = value;
    }

    private DateWorkArea reportingPeriodEndDate;
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
    /// A value of RoundedUdcAmt.
    /// </summary>
    [JsonPropertyName("roundedUdcAmt")]
    public Common RoundedUdcAmt
    {
      get => roundedUdcAmt ??= new();
      set => roundedUdcAmt = value;
    }

    /// <summary>
    /// A value of LoopCount.
    /// </summary>
    [JsonPropertyName("loopCount")]
    public Common LoopCount
    {
      get => loopCount ??= new();
      set => loopCount = value;
    }

    /// <summary>
    /// A value of LbdCnt.
    /// </summary>
    [JsonPropertyName("lbdCnt")]
    public Common LbdCnt
    {
      get => lbdCnt ??= new();
      set => lbdCnt = value;
    }

    /// <summary>
    /// A value of Gt2Cnt.
    /// </summary>
    [JsonPropertyName("gt2Cnt")]
    public Common Gt2Cnt
    {
      get => gt2Cnt ??= new();
      set => gt2Cnt = value;
    }

    /// <summary>
    /// A value of Gt30Cnt.
    /// </summary>
    [JsonPropertyName("gt30Cnt")]
    public Common Gt30Cnt
    {
      get => gt30Cnt ??= new();
      set => gt30Cnt = value;
    }

    /// <summary>
    /// A value of Gt180Cnt.
    /// </summary>
    [JsonPropertyName("gt180Cnt")]
    public Common Gt180Cnt
    {
      get => gt180Cnt ??= new();
      set => gt180Cnt = value;
    }

    /// <summary>
    /// A value of Gt365Cnt.
    /// </summary>
    [JsonPropertyName("gt365Cnt")]
    public Common Gt365Cnt
    {
      get => gt365Cnt ??= new();
      set => gt365Cnt = value;
    }

    /// <summary>
    /// A value of Gt1095Cnt.
    /// </summary>
    [JsonPropertyName("gt1095Cnt")]
    public Common Gt1095Cnt
    {
      get => gt1095Cnt ??= new();
      set => gt1095Cnt = value;
    }

    /// <summary>
    /// A value of Gt1825Cnt.
    /// </summary>
    [JsonPropertyName("gt1825Cnt")]
    public Common Gt1825Cnt
    {
      get => gt1825Cnt ??= new();
      set => gt1825Cnt = value;
    }

    /// <summary>
    /// A value of TotCrdCnt.
    /// </summary>
    [JsonPropertyName("totCrdCnt")]
    public Common TotCrdCnt
    {
      get => totCrdCnt ??= new();
      set => totCrdCnt = value;
    }

    /// <summary>
    /// A value of LbdUdAmt.
    /// </summary>
    [JsonPropertyName("lbdUdAmt")]
    public Common LbdUdAmt
    {
      get => lbdUdAmt ??= new();
      set => lbdUdAmt = value;
    }

    /// <summary>
    /// A value of Gt2UdAmt.
    /// </summary>
    [JsonPropertyName("gt2UdAmt")]
    public Common Gt2UdAmt
    {
      get => gt2UdAmt ??= new();
      set => gt2UdAmt = value;
    }

    /// <summary>
    /// A value of Gt30UdAmt.
    /// </summary>
    [JsonPropertyName("gt30UdAmt")]
    public Common Gt30UdAmt
    {
      get => gt30UdAmt ??= new();
      set => gt30UdAmt = value;
    }

    /// <summary>
    /// A value of Gt180UdAmt.
    /// </summary>
    [JsonPropertyName("gt180UdAmt")]
    public Common Gt180UdAmt
    {
      get => gt180UdAmt ??= new();
      set => gt180UdAmt = value;
    }

    /// <summary>
    /// A value of Gt365UdAmt.
    /// </summary>
    [JsonPropertyName("gt365UdAmt")]
    public Common Gt365UdAmt
    {
      get => gt365UdAmt ??= new();
      set => gt365UdAmt = value;
    }

    /// <summary>
    /// A value of Gt1095UdAmt.
    /// </summary>
    [JsonPropertyName("gt1095UdAmt")]
    public Common Gt1095UdAmt
    {
      get => gt1095UdAmt ??= new();
      set => gt1095UdAmt = value;
    }

    /// <summary>
    /// A value of Gt1825UdAmt.
    /// </summary>
    [JsonPropertyName("gt1825UdAmt")]
    public Common Gt1825UdAmt
    {
      get => gt1825UdAmt ??= new();
      set => gt1825UdAmt = value;
    }

    /// <summary>
    /// A value of Local2Date.
    /// </summary>
    [JsonPropertyName("local2Date")]
    public DateWorkArea Local2Date
    {
      get => local2Date ??= new();
      set => local2Date = value;
    }

    /// <summary>
    /// A value of Local30Date.
    /// </summary>
    [JsonPropertyName("local30Date")]
    public DateWorkArea Local30Date
    {
      get => local30Date ??= new();
      set => local30Date = value;
    }

    /// <summary>
    /// A value of Local180Date.
    /// </summary>
    [JsonPropertyName("local180Date")]
    public DateWorkArea Local180Date
    {
      get => local180Date ??= new();
      set => local180Date = value;
    }

    /// <summary>
    /// A value of Local365Date.
    /// </summary>
    [JsonPropertyName("local365Date")]
    public DateWorkArea Local365Date
    {
      get => local365Date ??= new();
      set => local365Date = value;
    }

    /// <summary>
    /// A value of Local1095Date.
    /// </summary>
    [JsonPropertyName("local1095Date")]
    public DateWorkArea Local1095Date
    {
      get => local1095Date ??= new();
      set => local1095Date = value;
    }

    /// <summary>
    /// A value of Local1825Date.
    /// </summary>
    [JsonPropertyName("local1825Date")]
    public DateWorkArea Local1825Date
    {
      get => local1825Date ??= new();
      set => local1825Date = value;
    }

    /// <summary>
    /// A value of CompareDate.
    /// </summary>
    [JsonPropertyName("compareDate")]
    public DateWorkArea CompareDate
    {
      get => compareDate ??= new();
      set => compareDate = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private Common roundedUdcAmt;
    private Common loopCount;
    private Common lbdCnt;
    private Common gt2Cnt;
    private Common gt30Cnt;
    private Common gt180Cnt;
    private Common gt365Cnt;
    private Common gt1095Cnt;
    private Common gt1825Cnt;
    private Common totCrdCnt;
    private Common lbdUdAmt;
    private Common gt2UdAmt;
    private Common gt30UdAmt;
    private Common gt180UdAmt;
    private Common gt365UdAmt;
    private Common gt1095UdAmt;
    private Common gt1825UdAmt;
    private DateWorkArea local2Date;
    private DateWorkArea local30Date;
    private DateWorkArea local180Date;
    private DateWorkArea local365Date;
    private DateWorkArea local1095Date;
    private DateWorkArea local1825Date;
    private DateWorkArea compareDate;
    private DateWorkArea endDate;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private DateWorkArea maxDate;
    private External external;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CollectionType collectionType;
  }
#endregion
}
