// Program: FN_BF11_WRITE_TOTALS, ID: 371040733, model: 746.
// Short name: SWE02723
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF11_WRITE_TOTALS.
/// </summary>
[Serializable]
public partial class FnBf11WriteTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF11_WRITE_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf11WriteTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf11WriteTotals.
  /// </summary>
  public FnBf11WriteTotals(IContext context, Import import, Export export):
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
    // 2000-11-27  PR 108247  Fangman - New AB to display totals for Monthly CR 
    // Fee tbl Fix run.
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (export.CountsAndAmounts.AmtOfCrFeeDisbRead.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail =
      "Disbursement Cost Recovery Fees read       " + NumberToString
      (export.CountsAndAmounts.NbrOfCrFeeDisbRead.Count, 10, 6) + "  " + local
      .Sign.Text1 + NumberToString
      ((long)export.CountsAndAmounts.AmtOfCrFeeDisbRead.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(export.CountsAndAmounts.AmtOfCrFeeDisbRead.TotalCurrency * 100),
      14, 2);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (export.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail =
      "Monthly Cost Recovery Fees deleted         " + NumberToString
      (export.CountsAndAmounts.NbrOfMoCrFeesDeleted.Count, 10, 6) + "  " + local
      .Sign.Text1 + NumberToString
      ((long)export.CountsAndAmounts.AmtOfMoCrFeesDeleted.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(export.CountsAndAmounts.AmtOfMoCrFeesDeleted.TotalCurrency * 100),
      14, 2);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (export.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail =
      "Monthly Cost Recovery Fees created         " + NumberToString
      (export.CountsAndAmounts.NbrOfMoCrFeesCreated.Count, 10, 6) + "  " + local
      .Sign.Text1 + NumberToString
      ((long)export.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(export.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency * 100),
      14, 2);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      "Number of ARs                              " + NumberToString
      (export.CountsAndAmounts.NbrOfArs.Count, 10, 6);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      "Number of ARs w/ total CR Fee diferrences  " + NumberToString
      (export.CountsAndAmounts.NbrOfErrors.Count, 10, 6);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfCrFeeDisbRead.
      /// </summary>
      [JsonPropertyName("nbrOfCrFeeDisbRead")]
      public Common NbrOfCrFeeDisbRead
      {
        get => nbrOfCrFeeDisbRead ??= new();
        set => nbrOfCrFeeDisbRead = value;
      }

      /// <summary>
      /// A value of NbrOfMoCrFeesDeleted.
      /// </summary>
      [JsonPropertyName("nbrOfMoCrFeesDeleted")]
      public Common NbrOfMoCrFeesDeleted
      {
        get => nbrOfMoCrFeesDeleted ??= new();
        set => nbrOfMoCrFeesDeleted = value;
      }

      /// <summary>
      /// A value of NbrOfMoCrFeesCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoCrFeesCreated")]
      public Common NbrOfMoCrFeesCreated
      {
        get => nbrOfMoCrFeesCreated ??= new();
        set => nbrOfMoCrFeesCreated = value;
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
      /// A value of NbrOfErrors.
      /// </summary>
      [JsonPropertyName("nbrOfErrors")]
      public Common NbrOfErrors
      {
        get => nbrOfErrors ??= new();
        set => nbrOfErrors = value;
      }

      /// <summary>
      /// A value of AmtOfCrFeeDisbRead.
      /// </summary>
      [JsonPropertyName("amtOfCrFeeDisbRead")]
      public Common AmtOfCrFeeDisbRead
      {
        get => amtOfCrFeeDisbRead ??= new();
        set => amtOfCrFeeDisbRead = value;
      }

      /// <summary>
      /// A value of AmtOfMoCrFeesDeleted.
      /// </summary>
      [JsonPropertyName("amtOfMoCrFeesDeleted")]
      public Common AmtOfMoCrFeesDeleted
      {
        get => amtOfMoCrFeesDeleted ??= new();
        set => amtOfMoCrFeesDeleted = value;
      }

      /// <summary>
      /// A value of AmtOfMoCrFeesCreated.
      /// </summary>
      [JsonPropertyName("amtOfMoCrFeesCreated")]
      public Common AmtOfMoCrFeesCreated
      {
        get => amtOfMoCrFeesCreated ??= new();
        set => amtOfMoCrFeesCreated = value;
      }

      private Common nbrOfCrFeeDisbRead;
      private Common nbrOfMoCrFeesDeleted;
      private Common nbrOfMoCrFeesCreated;
      private Common nbrOfArs;
      private Common nbrOfErrors;
      private Common amtOfCrFeeDisbRead;
      private Common amtOfMoCrFeesDeleted;
      private Common amtOfMoCrFeesCreated;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    /// <summary>
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public WorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
    }

    /// <summary>
    /// A value of UnformattedAmt.
    /// </summary>
    [JsonPropertyName("unformattedAmt")]
    public NumericWorkSet UnformattedAmt
    {
      get => unformattedAmt ??= new();
      set => unformattedAmt = value;
    }

    /// <summary>
    /// A value of FormattedAmt.
    /// </summary>
    [JsonPropertyName("formattedAmt")]
    public WorkArea FormattedAmt
    {
      get => formattedAmt ??= new();
      set => formattedAmt = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private WorkArea sign;
    private NumericWorkSet unformattedAmt;
    private WorkArea formattedAmt;
  }
#endregion
}
