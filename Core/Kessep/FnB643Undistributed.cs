// Program: FN_B643_UNDISTRIBUTED, ID: 372683793, model: 746.
// Short name: SWE02405
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_UNDISTRIBUTED.
/// </summary>
[Serializable]
public partial class FnB643Undistributed: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_UNDISTRIBUTED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643Undistributed(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643Undistributed.
  /// </summary>
  public FnB643Undistributed(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************************
    // * PR NUM     DATE     NAME      DESCRIPTION                        *
    // * ------  ----------  --------  
    // ----------------------------------
    // *
    // * #78285  11-15-1999  Ed Lyman  Do not print source of refund,     *
    // *
    // 
    // simply print the word REFUND.
    // *
    // ********************************************************************
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;
    local.EabFileHandling.Action = "WRITE";
    local.UndistributedFound.Flag = "N";
    local.UndistributedAmount.AverageCurrency = 0;
    local.RefundFound.Flag = "N";
    local.RefundedAmount.AverageCurrency = 0;

    // **************************************************************
    // Read Each Cash Receipt Detail received prior to the end of the
    // statement period, that has not been fully distributed.
    // **************************************************************
    foreach(var item in ReadCashReceiptDetail1())
    {
      local.UndistributedFound.Flag = "Y";

      // **************************************************************
      // ACCUMULATE UNDISTRIBUTED DETAIL
      // **************************************************************
      local.UndistributedAmount.AverageCurrency += entities.CashReceiptDetail.
        CollectionAmount - entities
        .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
        .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
    }

    if (AsChar(local.UndistributedFound.Flag) == 'Y')
    {
      // **************************************************************
      // WRITE UNDISTRIBUTED HEADER (REC TYPE = 4)
      // **************************************************************
      local.RecordType.ActionEntry = "04";
      ++export.VendorFileRecordCount.Count;
      ++export.SortSequenceNumber.Count;
      local.VariableLine1.RptDetail = "UNDISTRIBUTED";
      local.CollectionDate.Text10 = import.StmtEndTextWorkArea.Text10;
      UseEabCreateVendorFile2();
    }

    // **************************************************************
    // Read Each Cash Receipt Detail received during the statement
    // period, that resulted in a refund.
    // **************************************************************
    foreach(var item in ReadCashReceiptDetail2())
    {
      foreach(var item1 in ReadReceiptRefund())
      {
        local.RefundedAmount.AverageCurrency = entities.ReceiptRefund.Amount;
        local.ActivityDate.Text10 =
          NumberToString(Month(entities.ReceiptRefund.RequestDate), 14, 2) + "/"
          + NumberToString(Day(entities.ReceiptRefund.RequestDate), 14, 2) + "/"
          + NumberToString(Year(entities.ReceiptRefund.RequestDate), 12, 4);

        if (ReadPaymentRequest())
        {
          local.RefundedAmount.AverageCurrency = entities.PaymentRequest.Amount;
          local.ActivityDate.Text10 =
            NumberToString(Month(entities.PaymentRequest.ProcessDate), 14, 2) +
            "/" + NumberToString
            (Day(entities.PaymentRequest.ProcessDate), 14, 2) + "/" + NumberToString
            (Year(entities.PaymentRequest.ProcessDate), 12, 4);
          local.VariableLine1.RptDetail = "REFUND";
        }
        else
        {
          local.VariableLine1.RptDetail = "REFUND PENDING";

          // *** Nothing has been refunded against this request
        }

        // **************************************************************
        // WRITE REFUNDED HEADER (REC TYPE = 4)
        // **************************************************************
        local.RecordType.ActionEntry = "04";
        ++export.VendorFileRecordCount.Count;
        ++export.SortSequenceNumber.Count;
        local.CollectionDate.Text10 = local.ActivityDate.Text10;
        UseEabCreateVendorFile1();
      }
    }
  }

  private void UseEabCreateVendorFile1()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.AmountOne.AverageCurrency = local.RefundedAmount.AverageCurrency;
    useImport.DateOne.Text10 = local.CollectionDate.Text10;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile2()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.AmountOne.AverageCurrency =
      local.UndistributedAmount.AverageCurrency;
    useImport.DateOne.Text10 = local.CollectionDate.Text10;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCashReceiptDetail1()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.CsePerson.Number);
        db.SetDate(
          command, "collectionDate",
          import.StmtEndDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 10);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 11);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail2()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.CsePerson.Number);
        db.SetDate(
          command, "collectionDate",
          import.StmtEndDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 10);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 11);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PaymentRequest.Classification = db.GetString(reader, 4);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 5);
        entities.PaymentRequest.Type1 = db.GetString(reader, 6);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 7);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private IEnumerable<bool> ReadReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.StmtEndDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 1);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 2);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 6);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 7);
        entities.ReceiptRefund.Populated = true;

        return true;
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
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of StmtBegin.
    /// </summary>
    [JsonPropertyName("stmtBegin")]
    public DateWorkArea StmtBegin
    {
      get => stmtBegin ??= new();
      set => stmtBegin = value;
    }

    /// <summary>
    /// A value of StmtEndDateWorkArea.
    /// </summary>
    [JsonPropertyName("stmtEndDateWorkArea")]
    public DateWorkArea StmtEndDateWorkArea
    {
      get => stmtEndDateWorkArea ??= new();
      set => stmtEndDateWorkArea = value;
    }

    /// <summary>
    /// A value of StmtEndTextWorkArea.
    /// </summary>
    [JsonPropertyName("stmtEndTextWorkArea")]
    public TextWorkArea StmtEndTextWorkArea
    {
      get => stmtEndTextWorkArea ??= new();
      set => stmtEndTextWorkArea = value;
    }

    /// <summary>
    /// A value of StmtNumber.
    /// </summary>
    [JsonPropertyName("stmtNumber")]
    public Common StmtNumber
    {
      get => stmtNumber ??= new();
      set => stmtNumber = value;
    }

    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
    private CsePerson csePerson;
    private DateWorkArea stmtBegin;
    private DateWorkArea stmtEndDateWorkArea;
    private TextWorkArea stmtEndTextWorkArea;
    private Common stmtNumber;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
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

    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RefundFound.
    /// </summary>
    [JsonPropertyName("refundFound")]
    public Common RefundFound
    {
      get => refundFound ??= new();
      set => refundFound = value;
    }

    /// <summary>
    /// A value of RefundedAmount.
    /// </summary>
    [JsonPropertyName("refundedAmount")]
    public Common RefundedAmount
    {
      get => refundedAmount ??= new();
      set => refundedAmount = value;
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
    /// A value of UndistributedFound.
    /// </summary>
    [JsonPropertyName("undistributedFound")]
    public Common UndistributedFound
    {
      get => undistributedFound ??= new();
      set => undistributedFound = value;
    }

    /// <summary>
    /// A value of UndistributedAmount.
    /// </summary>
    [JsonPropertyName("undistributedAmount")]
    public Common UndistributedAmount
    {
      get => undistributedAmount ??= new();
      set => undistributedAmount = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of VariableLine1.
    /// </summary>
    [JsonPropertyName("variableLine1")]
    public EabReportSend VariableLine1
    {
      get => variableLine1 ??= new();
      set => variableLine1 = value;
    }

    /// <summary>
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    public TextWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

    /// <summary>
    /// A value of ActivityDate.
    /// </summary>
    [JsonPropertyName("activityDate")]
    public TextWorkArea ActivityDate
    {
      get => activityDate ??= new();
      set => activityDate = value;
    }

    /// <summary>
    /// A value of AmountDue.
    /// </summary>
    [JsonPropertyName("amountDue")]
    public Common AmountDue
    {
      get => amountDue ??= new();
      set => amountDue = value;
    }

    /// <summary>
    /// A value of AmountReceived.
    /// </summary>
    [JsonPropertyName("amountReceived")]
    public Common AmountReceived
    {
      get => amountReceived ??= new();
      set => amountReceived = value;
    }

    private Common refundFound;
    private Common refundedAmount;
    private EabFileHandling eabFileHandling;
    private Common undistributedFound;
    private Common undistributedAmount;
    private Common recordType;
    private EabReportSend variableLine1;
    private TextWorkArea collectionDate;
    private TextWorkArea activityDate;
    private Common amountDue;
    private Common amountReceived;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private ReceiptRefund receiptRefund;
    private CashReceiptDetail cashReceiptDetail;
    private PaymentRequest paymentRequest;
  }
#endregion
}
