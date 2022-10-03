// Program: FN_SWEFR001_DUP_PMT_RPT_DTL, ID: 372953913, model: 746.
// Short name: SWE02499
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_SWEFR001_DUP_PMT_RPT_DTL.
/// </summary>
[Serializable]
public partial class FnSwefr001DupPmtRptDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SWEFR001_DUP_PMT_RPT_DTL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSwefr001DupPmtRptDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSwefr001DupPmtRptDtl.
  /// </summary>
  public FnSwefr001DupPmtRptDtl(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // 12/10/10   RMathews    CQ22192          Expand amount fields from 5.2 to 
    // 6.2
    ++export.ImpExpCrdsWithDupPmt.Count;
    local.EabFileHandling.Action = "WRITE";
    local.CrdCreatedDate.Text8 =
      NumberToString(DateToInt(Date(import.CashReceiptDetail.CreatedTmst)), 8);
    local.CrdCollDate.Text8 =
      NumberToString(DateToInt(import.CashReceiptDetail.CollectionDate), 8);
    local.CrdAmtTxt.Text10 =
      NumberToString((long)import.CashReceiptDetail.CollectionAmount, 10, 6) + "."
      + NumberToString
      ((long)(import.CashReceiptDetail.CollectionAmount * 100), 14, 2);
    local.PotRcvCrTxt.Text10 =
      NumberToString((long)import.PotRcvForCr.TotalCurrency, 10, 6) + "." + NumberToString
      ((long)(import.PotRcvForCr.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail = import.Obligee.Number + "  " + (
      import.Disbursement.ReferenceNumber ?? "") + " " + local
      .CrdCreatedDate.Text8 + "  " + local.CrdCollDate.Text8 + "  " + local
      .CrdAmtTxt.Text10 + " " + local.PotRcvCrTxt.Text10;
    UseCabErrorReport();
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
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
    /// A value of PotRcvForCr.
    /// </summary>
    [JsonPropertyName("potRcvForCr")]
    public Common PotRcvForCr
    {
      get => potRcvForCr ??= new();
      set => potRcvForCr = value;
    }

    private CsePerson obligee;
    private DisbursementTransaction disbursement;
    private CashReceiptDetail cashReceiptDetail;
    private Common potRcvForCr;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImpExpCrdsWithDupPmt.
    /// </summary>
    [JsonPropertyName("impExpCrdsWithDupPmt")]
    public Common ImpExpCrdsWithDupPmt
    {
      get => impExpCrdsWithDupPmt ??= new();
      set => impExpCrdsWithDupPmt = value;
    }

    private Common impExpCrdsWithDupPmt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CrdCreatedDate.
    /// </summary>
    [JsonPropertyName("crdCreatedDate")]
    public TextWorkArea CrdCreatedDate
    {
      get => crdCreatedDate ??= new();
      set => crdCreatedDate = value;
    }

    /// <summary>
    /// A value of CrdCollDate.
    /// </summary>
    [JsonPropertyName("crdCollDate")]
    public TextWorkArea CrdCollDate
    {
      get => crdCollDate ??= new();
      set => crdCollDate = value;
    }

    /// <summary>
    /// A value of CrdAmtTxt.
    /// </summary>
    [JsonPropertyName("crdAmtTxt")]
    public TextWorkArea CrdAmtTxt
    {
      get => crdAmtTxt ??= new();
      set => crdAmtTxt = value;
    }

    /// <summary>
    /// A value of PotRcvCrTxt.
    /// </summary>
    [JsonPropertyName("potRcvCrTxt")]
    public TextWorkArea PotRcvCrTxt
    {
      get => potRcvCrTxt ??= new();
      set => potRcvCrTxt = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private TextWorkArea crdCreatedDate;
    private TextWorkArea crdCollDate;
    private TextWorkArea crdAmtTxt;
    private TextWorkArea potRcvCrTxt;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
