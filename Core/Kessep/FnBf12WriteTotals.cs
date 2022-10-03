// Program: FN_BF12_WRITE_TOTALS, ID: 373335537, model: 746.
// Short name: SWE02727
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF12_WRITE_TOTALS.
/// </summary>
[Serializable]
public partial class FnBf12WriteTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF12_WRITE_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf12WriteTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf12WriteTotals.
  /// </summary>
  public FnBf12WriteTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2001-02-26  WR 000235  Fangman - New AB to display totals for Psum 
    // Redesign Conversion process.
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Disbursements read " + NumberToString
      (import.CountsAndAmounts.NbrOfDisbRead.Count, 6, 10) + "  " + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfDisbRead.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of ARs " + NumberToString
      (import.CountsAndAmounts.NbrOfArs.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of Mo Sum rows updated      " + NumberToString
      (import.CountsAndAmounts.NbrOfMoSumRowsUpdated.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of Mo Sum rows created      " + NumberToString
      (import.CountsAndAmounts.NbrOfMoSumRowsCreated.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of Mo Sum rows deleted      " + NumberToString
      (import.CountsAndAmounts.NbrOfMoSumRowsDeleted.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Number of Mo Sum rows not matching " + NumberToString
      (import.CountsAndAmounts.NbrOfRowsNotMatching.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Nbr of Mo Sum rows with neg nbrs   " + NumberToString
      (import.CountsAndAmounts.NbrOfRowsWithNegNbr.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Coll count    " + NumberToString
      (import.CountsAndAmounts.NbrOfColl.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfColl.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of Coll  " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfColl.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfAf.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of AF    " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfAf.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfNa.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of NA    " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfNa.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfFees.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of Fees  " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfFees.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfSuppr.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of Suppr " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfSuppr.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfRecap.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of Recap " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfRecap.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfPt.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of P/T   " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfPt.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (import.CountsAndAmounts.AmtOfXUra.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Amt of X URA " + local.Sign.Text1 + NumberToString
      ((long)(import.CountsAndAmounts.AmtOfXUra.TotalCurrency * 100), 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
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
    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDisbRead.
      /// </summary>
      [JsonPropertyName("nbrOfDisbRead")]
      public Common NbrOfDisbRead
      {
        get => nbrOfDisbRead ??= new();
        set => nbrOfDisbRead = value;
      }

      /// <summary>
      /// A value of AmtOfDisbRead.
      /// </summary>
      [JsonPropertyName("amtOfDisbRead")]
      public Common AmtOfDisbRead
      {
        get => amtOfDisbRead ??= new();
        set => amtOfDisbRead = value;
      }

      /// <summary>
      /// A value of NbrOfArs.
      /// </summary>
      [JsonPropertyName("nbrOfArs")]
      public Common NbrOfArs
      {
        get => nbrOfArs ??= new();
        set => nbrOfArs = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsUpdated")]
      public Common NbrOfMoSumRowsUpdated
      {
        get => nbrOfMoSumRowsUpdated ??= new();
        set => nbrOfMoSumRowsUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsCreated")]
      public Common NbrOfMoSumRowsCreated
      {
        get => nbrOfMoSumRowsCreated ??= new();
        set => nbrOfMoSumRowsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsDeleted.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsDeleted")]
      public Common NbrOfMoSumRowsDeleted
      {
        get => nbrOfMoSumRowsDeleted ??= new();
        set => nbrOfMoSumRowsDeleted = value;
      }

      /// <summary>
      /// A value of NbrOfRowsNotMatching.
      /// </summary>
      [JsonPropertyName("nbrOfRowsNotMatching")]
      public Common NbrOfRowsNotMatching
      {
        get => nbrOfRowsNotMatching ??= new();
        set => nbrOfRowsNotMatching = value;
      }

      /// <summary>
      /// A value of NbrOfRowsWithNegNbr.
      /// </summary>
      [JsonPropertyName("nbrOfRowsWithNegNbr")]
      public Common NbrOfRowsWithNegNbr
      {
        get => nbrOfRowsWithNegNbr ??= new();
        set => nbrOfRowsWithNegNbr = value;
      }

      /// <summary>
      /// A value of NbrOfColl.
      /// </summary>
      [JsonPropertyName("nbrOfColl")]
      public Common NbrOfColl
      {
        get => nbrOfColl ??= new();
        set => nbrOfColl = value;
      }

      /// <summary>
      /// A value of AmtOfColl.
      /// </summary>
      [JsonPropertyName("amtOfColl")]
      public Common AmtOfColl
      {
        get => amtOfColl ??= new();
        set => amtOfColl = value;
      }

      /// <summary>
      /// A value of AmtOfAf.
      /// </summary>
      [JsonPropertyName("amtOfAf")]
      public Common AmtOfAf
      {
        get => amtOfAf ??= new();
        set => amtOfAf = value;
      }

      /// <summary>
      /// A value of AmtOfNa.
      /// </summary>
      [JsonPropertyName("amtOfNa")]
      public Common AmtOfNa
      {
        get => amtOfNa ??= new();
        set => amtOfNa = value;
      }

      /// <summary>
      /// A value of AmtOfFees.
      /// </summary>
      [JsonPropertyName("amtOfFees")]
      public Common AmtOfFees
      {
        get => amtOfFees ??= new();
        set => amtOfFees = value;
      }

      /// <summary>
      /// A value of AmtOfSuppr.
      /// </summary>
      [JsonPropertyName("amtOfSuppr")]
      public Common AmtOfSuppr
      {
        get => amtOfSuppr ??= new();
        set => amtOfSuppr = value;
      }

      /// <summary>
      /// A value of AmtOfRecap.
      /// </summary>
      [JsonPropertyName("amtOfRecap")]
      public Common AmtOfRecap
      {
        get => amtOfRecap ??= new();
        set => amtOfRecap = value;
      }

      /// <summary>
      /// A value of AmtOfPt.
      /// </summary>
      [JsonPropertyName("amtOfPt")]
      public Common AmtOfPt
      {
        get => amtOfPt ??= new();
        set => amtOfPt = value;
      }

      /// <summary>
      /// A value of AmtOfXUra.
      /// </summary>
      [JsonPropertyName("amtOfXUra")]
      public Common AmtOfXUra
      {
        get => amtOfXUra ??= new();
        set => amtOfXUra = value;
      }

      private Common nbrOfDisbRead;
      private Common amtOfDisbRead;
      private Common nbrOfArs;
      private Common nbrOfMoSumRowsUpdated;
      private Common nbrOfMoSumRowsCreated;
      private Common nbrOfMoSumRowsDeleted;
      private Common nbrOfRowsNotMatching;
      private Common nbrOfRowsWithNegNbr;
      private Common nbrOfColl;
      private Common amtOfColl;
      private Common amtOfAf;
      private Common amtOfNa;
      private Common amtOfFees;
      private Common amtOfSuppr;
      private Common amtOfRecap;
      private Common amtOfPt;
      private Common amtOfXUra;
    }

    /// <summary>
    /// Gets a value of CountsAndAmounts.
    /// </summary>
    [JsonPropertyName("countsAndAmounts")]
    public CountsAndAmountsGroup CountsAndAmounts
    {
      get => countsAndAmounts ?? (countsAndAmounts = new());
      set => countsAndAmounts = value;
    }

    private CountsAndAmountsGroup countsAndAmounts;
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
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public TextWorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
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

    private TextWorkArea sign;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
