// Program: FN_B650_WRITE_TOTALS, ID: 372896791, model: 746.
// Short name: SWE02492
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B650_WRITE_TOTALS.
/// </summary>
[Serializable]
public partial class FnB650WriteTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B650_WRITE_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB650WriteTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB650WriteTotals.
  /// </summary>
  public FnB650WriteTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------
    // 10/22/99 - SWSRKXD PR#77874
    // NC collections are errored off by B650 and money is never
    // disbursed!  Disable code that writes record count and
    // total amount disbursed to State. This was only reqd for NC.
    // 11/12/99 - Fangman  PR78745
    // Add new counts for Collections not fully applied (these collections will 
    // not be disbursed.)
    // 09/12/00 - Fangman  103323
    // Added new counts for errors.  This was put in with the changes to fix the
    // disb suppr with past discontinue dates.
    // 11/26/02 - Fangman  161935
    // Add new counts to break up the Cash counts into TAF and Non-TAF.
    // -----------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtOfCollRead.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtOfCollRead.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail = "Collection records read:            " + NumberToString
      (import.Totals.NbrOfCollRead.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10 + "  For " + NumberToString
      (import.Totals.NbrOfAps.Count, 10, 6) + " APs.";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtOfCollBackedOff.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(import.Totals.AmtOfCollBackedOff.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail = "Collections backed off (same day):  " + NumberToString
      (import.Totals.NbrOfCollBackedOff.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtOfErrorsCreated.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(import.Totals.AmtOfErrorsCreated.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail = "Collection records errored out:     " + NumberToString
      (import.Totals.NbrOfErrorsCreated.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------  Cash NA error subtotals  ---------
    local.EabReportSend.RptDetail = "  Cash Errors";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "   Non-TAF";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCNaKsAr.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCNaKsAr.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    State of KS as AR                                     " + NumberToString
      (import.Totals.NbrCNaKsAr.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCNaJjAr.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCNaJjAr.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Juvenile Justice as AR                                " + NumberToString
      (import.Totals.NbrCNaJjAr.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCNaNotDeterm.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(import.Totals.AmtCNaNotDeterm.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Obligee could not be determined                       " + NumberToString
      (import.Totals.NbrCNaNotDeterm.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCNaCaseNf.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCNaCaseNf.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Case not found                                        " + NumberToString
      (import.Totals.NbrCNaCaseNf.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCNaInterSt.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCNaInterSt.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Interstate Request not found                          " + NumberToString
      (import.Totals.NbrCNaInterSt.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCNaAllOther.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCNaAllOther.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    All other errors                                      " + NumberToString
      (import.Totals.NbrCNaAllOther.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------  Cash AF error subtotals  ---------
    local.EabReportSend.RptDetail = "   TAF";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCAfKsAr.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCAfKsAr.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    State of KS as AR                                     " + NumberToString
      (import.Totals.NbrCAfKsAr.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCAfJjAr.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCAfJjAr.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Juvenile Justice as AR                                " + NumberToString
      (import.Totals.NbrCAfJjAr.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCAfNotDeterm.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(import.Totals.AmtCAfNotDeterm.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Obligee could not be determined                       " + NumberToString
      (import.Totals.NbrCAfNotDeterm.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCAfCaseNf.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCAfCaseNf.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Case not found                                        " + NumberToString
      (import.Totals.NbrCAfCaseNf.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCAfInterSt.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCAfInterSt.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Interstate Request not found                          " + NumberToString
      (import.Totals.NbrCAfInterSt.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtCAfAllOther.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtCAfAllOther.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    All other errors                                      " + NumberToString
      (import.Totals.NbrCAfAllOther.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------  Non-Cash error subtotals  ---------
    local.EabReportSend.RptDetail = "  Non-Cash Errors";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtNKsAr.TotalCurrency, 9, 7) + "." + NumberToString
      ((long)(import.Totals.AmtNKsAr.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    State of KS as AR                                     " + NumberToString
      (import.Totals.NbrNKsAr.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtNJjAr.TotalCurrency, 9, 7) + "." + NumberToString
      ((long)(import.Totals.AmtNJjAr.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Juvenile Justice as AR                                " + NumberToString
      (import.Totals.NbrNJjAr.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtNNotDeterm.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtNNotDeterm.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Obligee could not be determined                       " + NumberToString
      (import.Totals.NbrNNotDeterm.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtNCaseNf.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtNCaseNf.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Case not found                                        " + NumberToString
      (import.Totals.NbrNCaseNf.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtNInterSt.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtNInterSt.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    Interstate Request not found                          " + NumberToString
      (import.Totals.NbrNInterSt.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtNAllOther.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(import.Totals.AmtNAllOther.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "    All other errors                                      " + NumberToString
      (import.Totals.NbrNAllOther.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtOfCollNotFulApl.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(import.Totals.AmtOfCollNotFulApl.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail = "Collections not fully applied:      " + NumberToString
      (import.Totals.NbrOfCollNotFulApl.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10 + "  For " + NumberToString
      (import.Totals.NbrOfApWoCollFullyAppl.Count, 10, 6) + " APs.";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.FormattedAmount.Text10 =
      NumberToString((long)import.Totals.AmtOfCreditsCreated.TotalCurrency, 9, 7)
      + "." + NumberToString
      ((long)(import.Totals.AmtOfCreditsCreated.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail = "Disbursement credits created:       " + NumberToString
      (import.Totals.NbrOfCreditsCreated.Count, 10, 6) + "    " + local
      .FormattedAmount.Text10;
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
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
    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of NbrOfCollRead.
      /// </summary>
      [JsonPropertyName("nbrOfCollRead")]
      public Common NbrOfCollRead
      {
        get => nbrOfCollRead ??= new();
        set => nbrOfCollRead = value;
      }

      /// <summary>
      /// A value of AmtOfCollRead.
      /// </summary>
      [JsonPropertyName("amtOfCollRead")]
      public Common AmtOfCollRead
      {
        get => amtOfCollRead ??= new();
        set => amtOfCollRead = value;
      }

      /// <summary>
      /// A value of NbrOfAps.
      /// </summary>
      [JsonPropertyName("nbrOfAps")]
      public Common NbrOfAps
      {
        get => nbrOfAps ??= new();
        set => nbrOfAps = value;
      }

      /// <summary>
      /// A value of NbrOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("nbrOfCollBackedOff")]
      public Common NbrOfCollBackedOff
      {
        get => nbrOfCollBackedOff ??= new();
        set => nbrOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of AmtOfCollBackedOff.
      /// </summary>
      [JsonPropertyName("amtOfCollBackedOff")]
      public Common AmtOfCollBackedOff
      {
        get => amtOfCollBackedOff ??= new();
        set => amtOfCollBackedOff = value;
      }

      /// <summary>
      /// A value of NbrOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfErrorsCreated")]
      public Common NbrOfErrorsCreated
      {
        get => nbrOfErrorsCreated ??= new();
        set => nbrOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of AmtOfErrorsCreated.
      /// </summary>
      [JsonPropertyName("amtOfErrorsCreated")]
      public Common AmtOfErrorsCreated
      {
        get => amtOfErrorsCreated ??= new();
        set => amtOfErrorsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfCollNotFulApl.
      /// </summary>
      [JsonPropertyName("nbrOfCollNotFulApl")]
      public Common NbrOfCollNotFulApl
      {
        get => nbrOfCollNotFulApl ??= new();
        set => nbrOfCollNotFulApl = value;
      }

      /// <summary>
      /// A value of AmtOfCollNotFulApl.
      /// </summary>
      [JsonPropertyName("amtOfCollNotFulApl")]
      public Common AmtOfCollNotFulApl
      {
        get => amtOfCollNotFulApl ??= new();
        set => amtOfCollNotFulApl = value;
      }

      /// <summary>
      /// A value of NbrOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfCreditsCreated")]
      public Common NbrOfCreditsCreated
      {
        get => nbrOfCreditsCreated ??= new();
        set => nbrOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of AmtOfCreditsCreated.
      /// </summary>
      [JsonPropertyName("amtOfCreditsCreated")]
      public Common AmtOfCreditsCreated
      {
        get => amtOfCreditsCreated ??= new();
        set => amtOfCreditsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfApWoCollFullyAppl.
      /// </summary>
      [JsonPropertyName("nbrOfApWoCollFullyAppl")]
      public Common NbrOfApWoCollFullyAppl
      {
        get => nbrOfApWoCollFullyAppl ??= new();
        set => nbrOfApWoCollFullyAppl = value;
      }

      /// <summary>
      /// A value of NbrCNaKsAr.
      /// </summary>
      [JsonPropertyName("nbrCNaKsAr")]
      public Common NbrCNaKsAr
      {
        get => nbrCNaKsAr ??= new();
        set => nbrCNaKsAr = value;
      }

      /// <summary>
      /// A value of AmtCNaKsAr.
      /// </summary>
      [JsonPropertyName("amtCNaKsAr")]
      public Common AmtCNaKsAr
      {
        get => amtCNaKsAr ??= new();
        set => amtCNaKsAr = value;
      }

      /// <summary>
      /// A value of NbrCNaJjAr.
      /// </summary>
      [JsonPropertyName("nbrCNaJjAr")]
      public Common NbrCNaJjAr
      {
        get => nbrCNaJjAr ??= new();
        set => nbrCNaJjAr = value;
      }

      /// <summary>
      /// A value of AmtCNaJjAr.
      /// </summary>
      [JsonPropertyName("amtCNaJjAr")]
      public Common AmtCNaJjAr
      {
        get => amtCNaJjAr ??= new();
        set => amtCNaJjAr = value;
      }

      /// <summary>
      /// A value of NbrCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("nbrCNaNotDeterm")]
      public Common NbrCNaNotDeterm
      {
        get => nbrCNaNotDeterm ??= new();
        set => nbrCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of AmtCNaNotDeterm.
      /// </summary>
      [JsonPropertyName("amtCNaNotDeterm")]
      public Common AmtCNaNotDeterm
      {
        get => amtCNaNotDeterm ??= new();
        set => amtCNaNotDeterm = value;
      }

      /// <summary>
      /// A value of NbrCNaCaseNf.
      /// </summary>
      [JsonPropertyName("nbrCNaCaseNf")]
      public Common NbrCNaCaseNf
      {
        get => nbrCNaCaseNf ??= new();
        set => nbrCNaCaseNf = value;
      }

      /// <summary>
      /// A value of AmtCNaCaseNf.
      /// </summary>
      [JsonPropertyName("amtCNaCaseNf")]
      public Common AmtCNaCaseNf
      {
        get => amtCNaCaseNf ??= new();
        set => amtCNaCaseNf = value;
      }

      /// <summary>
      /// A value of NbrCNaInterSt.
      /// </summary>
      [JsonPropertyName("nbrCNaInterSt")]
      public Common NbrCNaInterSt
      {
        get => nbrCNaInterSt ??= new();
        set => nbrCNaInterSt = value;
      }

      /// <summary>
      /// A value of AmtCNaInterSt.
      /// </summary>
      [JsonPropertyName("amtCNaInterSt")]
      public Common AmtCNaInterSt
      {
        get => amtCNaInterSt ??= new();
        set => amtCNaInterSt = value;
      }

      /// <summary>
      /// A value of NbrCNaAllOther.
      /// </summary>
      [JsonPropertyName("nbrCNaAllOther")]
      public Common NbrCNaAllOther
      {
        get => nbrCNaAllOther ??= new();
        set => nbrCNaAllOther = value;
      }

      /// <summary>
      /// A value of AmtCNaAllOther.
      /// </summary>
      [JsonPropertyName("amtCNaAllOther")]
      public Common AmtCNaAllOther
      {
        get => amtCNaAllOther ??= new();
        set => amtCNaAllOther = value;
      }

      /// <summary>
      /// A value of NbrCAfKsAr.
      /// </summary>
      [JsonPropertyName("nbrCAfKsAr")]
      public Common NbrCAfKsAr
      {
        get => nbrCAfKsAr ??= new();
        set => nbrCAfKsAr = value;
      }

      /// <summary>
      /// A value of AmtCAfKsAr.
      /// </summary>
      [JsonPropertyName("amtCAfKsAr")]
      public Common AmtCAfKsAr
      {
        get => amtCAfKsAr ??= new();
        set => amtCAfKsAr = value;
      }

      /// <summary>
      /// A value of NbrCAfJjAr.
      /// </summary>
      [JsonPropertyName("nbrCAfJjAr")]
      public Common NbrCAfJjAr
      {
        get => nbrCAfJjAr ??= new();
        set => nbrCAfJjAr = value;
      }

      /// <summary>
      /// A value of AmtCAfJjAr.
      /// </summary>
      [JsonPropertyName("amtCAfJjAr")]
      public Common AmtCAfJjAr
      {
        get => amtCAfJjAr ??= new();
        set => amtCAfJjAr = value;
      }

      /// <summary>
      /// A value of NbrCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("nbrCAfNotDeterm")]
      public Common NbrCAfNotDeterm
      {
        get => nbrCAfNotDeterm ??= new();
        set => nbrCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of AmtCAfNotDeterm.
      /// </summary>
      [JsonPropertyName("amtCAfNotDeterm")]
      public Common AmtCAfNotDeterm
      {
        get => amtCAfNotDeterm ??= new();
        set => amtCAfNotDeterm = value;
      }

      /// <summary>
      /// A value of NbrCAfCaseNf.
      /// </summary>
      [JsonPropertyName("nbrCAfCaseNf")]
      public Common NbrCAfCaseNf
      {
        get => nbrCAfCaseNf ??= new();
        set => nbrCAfCaseNf = value;
      }

      /// <summary>
      /// A value of AmtCAfCaseNf.
      /// </summary>
      [JsonPropertyName("amtCAfCaseNf")]
      public Common AmtCAfCaseNf
      {
        get => amtCAfCaseNf ??= new();
        set => amtCAfCaseNf = value;
      }

      /// <summary>
      /// A value of NbrCAfInterSt.
      /// </summary>
      [JsonPropertyName("nbrCAfInterSt")]
      public Common NbrCAfInterSt
      {
        get => nbrCAfInterSt ??= new();
        set => nbrCAfInterSt = value;
      }

      /// <summary>
      /// A value of AmtCAfInterSt.
      /// </summary>
      [JsonPropertyName("amtCAfInterSt")]
      public Common AmtCAfInterSt
      {
        get => amtCAfInterSt ??= new();
        set => amtCAfInterSt = value;
      }

      /// <summary>
      /// A value of NbrCAfAllOther.
      /// </summary>
      [JsonPropertyName("nbrCAfAllOther")]
      public Common NbrCAfAllOther
      {
        get => nbrCAfAllOther ??= new();
        set => nbrCAfAllOther = value;
      }

      /// <summary>
      /// A value of AmtCAfAllOther.
      /// </summary>
      [JsonPropertyName("amtCAfAllOther")]
      public Common AmtCAfAllOther
      {
        get => amtCAfAllOther ??= new();
        set => amtCAfAllOther = value;
      }

      /// <summary>
      /// A value of NbrNKsAr.
      /// </summary>
      [JsonPropertyName("nbrNKsAr")]
      public Common NbrNKsAr
      {
        get => nbrNKsAr ??= new();
        set => nbrNKsAr = value;
      }

      /// <summary>
      /// A value of AmtNKsAr.
      /// </summary>
      [JsonPropertyName("amtNKsAr")]
      public Common AmtNKsAr
      {
        get => amtNKsAr ??= new();
        set => amtNKsAr = value;
      }

      /// <summary>
      /// A value of NbrNJjAr.
      /// </summary>
      [JsonPropertyName("nbrNJjAr")]
      public Common NbrNJjAr
      {
        get => nbrNJjAr ??= new();
        set => nbrNJjAr = value;
      }

      /// <summary>
      /// A value of AmtNJjAr.
      /// </summary>
      [JsonPropertyName("amtNJjAr")]
      public Common AmtNJjAr
      {
        get => amtNJjAr ??= new();
        set => amtNJjAr = value;
      }

      /// <summary>
      /// A value of NbrNNotDeterm.
      /// </summary>
      [JsonPropertyName("nbrNNotDeterm")]
      public Common NbrNNotDeterm
      {
        get => nbrNNotDeterm ??= new();
        set => nbrNNotDeterm = value;
      }

      /// <summary>
      /// A value of AmtNNotDeterm.
      /// </summary>
      [JsonPropertyName("amtNNotDeterm")]
      public Common AmtNNotDeterm
      {
        get => amtNNotDeterm ??= new();
        set => amtNNotDeterm = value;
      }

      /// <summary>
      /// A value of NbrNCaseNf.
      /// </summary>
      [JsonPropertyName("nbrNCaseNf")]
      public Common NbrNCaseNf
      {
        get => nbrNCaseNf ??= new();
        set => nbrNCaseNf = value;
      }

      /// <summary>
      /// A value of AmtNCaseNf.
      /// </summary>
      [JsonPropertyName("amtNCaseNf")]
      public Common AmtNCaseNf
      {
        get => amtNCaseNf ??= new();
        set => amtNCaseNf = value;
      }

      /// <summary>
      /// A value of NbrNInterSt.
      /// </summary>
      [JsonPropertyName("nbrNInterSt")]
      public Common NbrNInterSt
      {
        get => nbrNInterSt ??= new();
        set => nbrNInterSt = value;
      }

      /// <summary>
      /// A value of AmtNInterSt.
      /// </summary>
      [JsonPropertyName("amtNInterSt")]
      public Common AmtNInterSt
      {
        get => amtNInterSt ??= new();
        set => amtNInterSt = value;
      }

      /// <summary>
      /// A value of NbrNAllOther.
      /// </summary>
      [JsonPropertyName("nbrNAllOther")]
      public Common NbrNAllOther
      {
        get => nbrNAllOther ??= new();
        set => nbrNAllOther = value;
      }

      /// <summary>
      /// A value of AmtNAllOther.
      /// </summary>
      [JsonPropertyName("amtNAllOther")]
      public Common AmtNAllOther
      {
        get => amtNAllOther ??= new();
        set => amtNAllOther = value;
      }

      private Common nbrOfCollRead;
      private Common amtOfCollRead;
      private Common nbrOfAps;
      private Common nbrOfCollBackedOff;
      private Common amtOfCollBackedOff;
      private Common nbrOfErrorsCreated;
      private Common amtOfErrorsCreated;
      private Common nbrOfCollNotFulApl;
      private Common amtOfCollNotFulApl;
      private Common nbrOfCreditsCreated;
      private Common amtOfCreditsCreated;
      private Common nbrOfApWoCollFullyAppl;
      private Common nbrCNaKsAr;
      private Common amtCNaKsAr;
      private Common nbrCNaJjAr;
      private Common amtCNaJjAr;
      private Common nbrCNaNotDeterm;
      private Common amtCNaNotDeterm;
      private Common nbrCNaCaseNf;
      private Common amtCNaCaseNf;
      private Common nbrCNaInterSt;
      private Common amtCNaInterSt;
      private Common nbrCNaAllOther;
      private Common amtCNaAllOther;
      private Common nbrCAfKsAr;
      private Common amtCAfKsAr;
      private Common nbrCAfJjAr;
      private Common amtCAfJjAr;
      private Common nbrCAfNotDeterm;
      private Common amtCAfNotDeterm;
      private Common nbrCAfCaseNf;
      private Common amtCAfCaseNf;
      private Common nbrCAfInterSt;
      private Common amtCAfInterSt;
      private Common nbrCAfAllOther;
      private Common amtCAfAllOther;
      private Common nbrNKsAr;
      private Common amtNKsAr;
      private Common nbrNJjAr;
      private Common amtNJjAr;
      private Common nbrNNotDeterm;
      private Common amtNNotDeterm;
      private Common nbrNCaseNf;
      private Common amtNCaseNf;
      private Common nbrNInterSt;
      private Common amtNInterSt;
      private Common nbrNAllOther;
      private Common amtNAllOther;
    }

    /// <summary>
    /// Gets a value of Totals.
    /// </summary>
    [JsonPropertyName("totals")]
    public TotalsGroup Totals
    {
      get => totals ?? (totals = new());
      set => totals = value;
    }

    private TotalsGroup totals;
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
    /// A value of FormattedAmount.
    /// </summary>
    [JsonPropertyName("formattedAmount")]
    public TextWorkArea FormattedAmount
    {
      get => formattedAmount ??= new();
      set => formattedAmount = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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

    private TextWorkArea formattedAmount;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
