// Program: FN_SWEFR001_PRINT_DISB, ID: 372954039, model: 746.
// Short name: SWE02500
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_SWEFR001_PRINT_DISB.
/// </summary>
[Serializable]
public partial class FnSwefr001PrintDisb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SWEFR001_PRINT_DISB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSwefr001PrintDisb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSwefr001PrintDisb.
  /// </summary>
  public FnSwefr001PrintDisb(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // 12/10/10  RMathews    CQ22192        Expanded amount fields from 5.2 to 
    // 6.2
    local.EabFileHandling.Action = "WRITE";
    local.CrdAmtTxt.Text10 =
      NumberToString((long)import.CashReceiptDetail.CollectionAmount, 10, 6) + "."
      + NumberToString
      ((long)(import.CashReceiptDetail.CollectionAmount * 100), 14, 2);
    local.TotDisbForPerRefNbrTxt.Text10 =
      NumberToString((long)import.TotDisbForPerRefNbr.TotalCurrency, 10, 6) + "."
      + NumberToString
      ((long)(import.TotDisbForPerRefNbr.TotalCurrency * 100), 14, 2);
    local.TotPosForProcessDateTxt.Text10 =
      NumberToString((long)import.TotDisbForPerRefNbr.TotalCurrency, 10, 6) + "."
      + NumberToString
      ((long)(import.TotDisbForPerRefNbr.TotalCurrency * 100), 14, 2);
    local.DisbCreated.Text8 =
      NumberToString(DateToInt(Date(import.Disbursement.CreatedTimestamp)), 8);
    local.DisbProcessDate.Text8 =
      NumberToString(DateToInt(import.Disbursement.ProcessDate), 8);

    if (import.Disbursement.Amount < 0)
    {
      local.DisbSign.Text1 = "-";
    }
    else
    {
      local.DisbSign.Text1 = "";
    }

    local.DisbAmtTxt.Text10 =
      NumberToString((long)import.Disbursement.Amount, 10, 6) + "." + NumberToString
      ((long)(import.Disbursement.Amount * 100), 14, 2);
    local.EabReportSend.RptDetail = import.Obligee.Number + "  " + (
      import.Disbursement.ReferenceNumber ?? "") + "  " + import
      .DisbursementType.Code + "  " + local.DisbCreated.Text8 + "  " + local
      .DisbProcessDate.Text8 + " " + local.DisbSign.Text1 + local
      .DisbAmtTxt.Text10 + "  " + local.TotDisbForPerRefNbrTxt.Text10 + "   " +
      local.CrdAmtTxt.Text10 + "   " + local.TotPosForProcessDateTxt.Text10;
    UseCabErrorReport();
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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
    /// A value of TotPosForProcessDate.
    /// </summary>
    [JsonPropertyName("totPosForProcessDate")]
    public DisbursementTransaction TotPosForProcessDate
    {
      get => totPosForProcessDate ??= new();
      set => totPosForProcessDate = value;
    }

    /// <summary>
    /// A value of TotDisbForPerRefNbr.
    /// </summary>
    [JsonPropertyName("totDisbForPerRefNbr")]
    public Common TotDisbForPerRefNbr
    {
      get => totDisbForPerRefNbr ??= new();
      set => totDisbForPerRefNbr = value;
    }

    private CsePerson obligee;
    private DisbursementTransaction disbursement;
    private DisbursementType disbursementType;
    private CashReceiptDetail cashReceiptDetail;
    private DisbursementTransaction totPosForProcessDate;
    private Common totDisbForPerRefNbr;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of DisbCreated.
    /// </summary>
    [JsonPropertyName("disbCreated")]
    public TextWorkArea DisbCreated
    {
      get => disbCreated ??= new();
      set => disbCreated = value;
    }

    /// <summary>
    /// A value of DisbProcessDate.
    /// </summary>
    [JsonPropertyName("disbProcessDate")]
    public TextWorkArea DisbProcessDate
    {
      get => disbProcessDate ??= new();
      set => disbProcessDate = value;
    }

    /// <summary>
    /// A value of DisbSign.
    /// </summary>
    [JsonPropertyName("disbSign")]
    public TextWorkArea DisbSign
    {
      get => disbSign ??= new();
      set => disbSign = value;
    }

    /// <summary>
    /// A value of DisbAmtTxt.
    /// </summary>
    [JsonPropertyName("disbAmtTxt")]
    public TextWorkArea DisbAmtTxt
    {
      get => disbAmtTxt ??= new();
      set => disbAmtTxt = value;
    }

    /// <summary>
    /// A value of TotDisbForPerRefNbrTxt.
    /// </summary>
    [JsonPropertyName("totDisbForPerRefNbrTxt")]
    public TextWorkArea TotDisbForPerRefNbrTxt
    {
      get => totDisbForPerRefNbrTxt ??= new();
      set => totDisbForPerRefNbrTxt = value;
    }

    /// <summary>
    /// A value of TotPosForProcessDateTxt.
    /// </summary>
    [JsonPropertyName("totPosForProcessDateTxt")]
    public TextWorkArea TotPosForProcessDateTxt
    {
      get => totPosForProcessDateTxt ??= new();
      set => totPosForProcessDateTxt = value;
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

    private EabFileHandling eabFileHandling;
    private TextWorkArea crdAmtTxt;
    private TextWorkArea disbCreated;
    private TextWorkArea disbProcessDate;
    private TextWorkArea disbSign;
    private TextWorkArea disbAmtTxt;
    private TextWorkArea totDisbForPerRefNbrTxt;
    private TextWorkArea totPosForProcessDateTxt;
    private EabReportSend eabReportSend;
  }
#endregion
}
