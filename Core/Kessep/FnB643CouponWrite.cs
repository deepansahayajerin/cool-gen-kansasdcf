// Program: FN_B643_COUPON_WRITE, ID: 372764842, model: 746.
// Short name: SWE02464
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_COUPON_WRITE.
/// </summary>
[Serializable]
public partial class FnB643CouponWrite: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_COUPON_WRITE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643CouponWrite(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643CouponWrite.
  /// </summary>
  public FnB643CouponWrite(IContext context, Import import, Export export):
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
    // * #KPC    11-02-2000  Ed Lyman  Use standard no, not court_case_no *
    // ********************************************************************
    export.CouponsCreated.Count = import.CouponsCreated.Count;
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;

    // **************************************************************
    // WRITE COUPON ADDRESS & FINANCIAL INFORMATION (REC TYPE = "10")
    // **************************************************************
    ++export.CouponsCreated.Count;
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;
    local.RecordType.ActionEntry = "10";
    local.EabFileHandling.Action = "WRITE";
    local.VariableLine1.RptDetail = import.CpnReportingMonthYear.Text30;

    // *************************** KPC
    // 
    // *******************************
    if (CharAt(import.LegalAction.StandardNumber, 6) == '*')
    {
      local.VariableLine2.RptDetail =
        Substring(import.LegalAction.StandardNumber,
        LegalAction.StandardNumber_MaxLength, 1, 5) + " " + Substring
        (import.LegalAction.StandardNumber,
        LegalAction.StandardNumber_MaxLength, 7, 14);
    }
    else
    {
      local.VariableLine2.RptDetail = import.LegalAction.StandardNumber ?? Spaces
        (132);
    }

    // *************************** KPC
    // 
    // *******************************
    local.GlobalStatementMessage.TextArea = import.PayToName.Text30;
    UseEabCreateVendorFile1();

    // **************************************************************
    // WRITE COUPON DETAILS (REC TYPE = "11")
    // **************************************************************
    local.RecordType.ActionEntry = "11";
    local.EabFileHandling.Action = "WRITE";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;

    // ***** The future global coupon message will be placed in the next two 
    // lines. ********
    local.VariableLine1.RptDetail = "";
    local.VariableLine2.RptDetail = "";
    UseEabCreateVendorFile2();
  }

  private void UseEabCreateVendorFile1()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.CsePersonAddress.Assign(import.PaymentAddress);
    useImport.AmountOne.AverageCurrency = import.PastDueAmt.AverageCurrency;
    useImport.DateOne.Text10 = import.PastDueAsOf.Text10;
    useImport.AmountTwo.AverageCurrency = import.AmtDue.AverageCurrency;
    useImport.DateTwo.Text10 = import.DueBy.Text10;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.CsePerson.Number = import.Client.Number;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.VariableLine2.RptDetail = local.VariableLine2.RptDetail;
    useImport.GlobalStatementMessage.TextArea =
      local.GlobalStatementMessage.TextArea;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile2()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.AmountOne.AverageCurrency = import.PastDueAmt.AverageCurrency;
    useImport.DateOne.Text10 = import.PastDueAsOf.Text10;
    useImport.AmountTwo.AverageCurrency = import.AmtDue.AverageCurrency;
    useImport.DateTwo.Text10 = import.DueBy.Text10;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.CsePerson.Number = import.Client.Number;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.VariableLine2.RptDetail = local.VariableLine2.RptDetail;
    useImport.GlobalStatementMessage.TextArea =
      local.GlobalStatementMessage.TextArea;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of PaymentAddress.
    /// </summary>
    [JsonPropertyName("paymentAddress")]
    public CsePersonAddress PaymentAddress
    {
      get => paymentAddress ??= new();
      set => paymentAddress = value;
    }

    /// <summary>
    /// A value of PayToName.
    /// </summary>
    [JsonPropertyName("payToName")]
    public TextWorkArea PayToName
    {
      get => payToName ??= new();
      set => payToName = value;
    }

    /// <summary>
    /// A value of PastDueAmt.
    /// </summary>
    [JsonPropertyName("pastDueAmt")]
    public Common PastDueAmt
    {
      get => pastDueAmt ??= new();
      set => pastDueAmt = value;
    }

    /// <summary>
    /// A value of PastDueAsOf.
    /// </summary>
    [JsonPropertyName("pastDueAsOf")]
    public TextWorkArea PastDueAsOf
    {
      get => pastDueAsOf ??= new();
      set => pastDueAsOf = value;
    }

    /// <summary>
    /// A value of AmtDue.
    /// </summary>
    [JsonPropertyName("amtDue")]
    public Common AmtDue
    {
      get => amtDue ??= new();
      set => amtDue = value;
    }

    /// <summary>
    /// A value of DueBy.
    /// </summary>
    [JsonPropertyName("dueBy")]
    public TextWorkArea DueBy
    {
      get => dueBy ??= new();
      set => dueBy = value;
    }

    /// <summary>
    /// A value of CouponBegin.
    /// </summary>
    [JsonPropertyName("couponBegin")]
    public DateWorkArea CouponBegin
    {
      get => couponBegin ??= new();
      set => couponBegin = value;
    }

    /// <summary>
    /// A value of CpnReportingMonthYear.
    /// </summary>
    [JsonPropertyName("cpnReportingMonthYear")]
    public TextWorkArea CpnReportingMonthYear
    {
      get => cpnReportingMonthYear ??= new();
      set => cpnReportingMonthYear = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of StmtEndDate.
    /// </summary>
    [JsonPropertyName("stmtEndDate")]
    public TextWorkArea StmtEndDate
    {
      get => stmtEndDate ??= new();
      set => stmtEndDate = value;
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

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of CouponsCreated.
    /// </summary>
    [JsonPropertyName("couponsCreated")]
    public Common CouponsCreated
    {
      get => couponsCreated ??= new();
      set => couponsCreated = value;
    }

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

    private CsePersonAddress paymentAddress;
    private TextWorkArea payToName;
    private Common pastDueAmt;
    private TextWorkArea pastDueAsOf;
    private Common amtDue;
    private TextWorkArea dueBy;
    private DateWorkArea couponBegin;
    private TextWorkArea cpnReportingMonthYear;
    private LegalAction legalAction;
    private TextWorkArea stmtEndDate;
    private Common stmtNumber;
    private CsePerson client;
    private Common couponsCreated;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CouponsCreated.
    /// </summary>
    [JsonPropertyName("couponsCreated")]
    public Common CouponsCreated
    {
      get => couponsCreated ??= new();
      set => couponsCreated = value;
    }

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

    private Common couponsCreated;
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
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of VariableLine1.
    /// </summary>
    [JsonPropertyName("variableLine1")]
    public EabReportSend VariableLine1
    {
      get => variableLine1 ??= new();
      set => variableLine1 = value;
    }

    /// <summary>
    /// A value of VariableLine2.
    /// </summary>
    [JsonPropertyName("variableLine2")]
    public EabReportSend VariableLine2
    {
      get => variableLine2 ??= new();
      set => variableLine2 = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    private Common recordType;
    private EabFileHandling eabFileHandling;
    private EabReportSend variableLine1;
    private EabReportSend variableLine2;
    private GlobalStatementMessage globalStatementMessage;
  }
#endregion
}
