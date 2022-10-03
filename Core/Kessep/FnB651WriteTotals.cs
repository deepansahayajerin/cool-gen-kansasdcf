// Program: FN_B651_WRITE_TOTALS, ID: 371004907, model: 746.
// Short name: SWE02693
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_WRITE_TOTALS.
/// </summary>
[Serializable]
public partial class FnB651WriteTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_WRITE_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651WriteTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651WriteTotals.
  /// </summary>
  public FnB651WriteTotals(IContext context, Import import, Export export):
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
    // 2000-09-27  PR 98039   Fangman - As part of the project to prevent 
    // duplicate payments I changed added a rollback count & suppression counts.
    // 2001-04-23  PR 118495  Fangman - As part of this project the size of the 
    // amount field on the Suppression report lines was increased.
    // 2001-10-25  PR 118495  Fangman - Added count for number of collections 
    // that require duplicate payment processing.
    // 2019-07-02  CQ65423  GVandy - Added count for system (Y & Z) 
    // suppressions.
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (export.CountsAndAmounts.AmtOfDisbRead.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Disbursement Credits read    " + NumberToString
      (export.CountsAndAmounts.NbrOfDisbRead.Count, 10, 6) + "  " + local
      .Sign.Text1 + NumberToString
      ((long)export.CountsAndAmounts.AmtOfDisbRead.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(export.CountsAndAmounts.AmtOfDisbRead.TotalCurrency * 100), 14, 2);
      
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (export.CountsAndAmounts.AmtOfDisbCreated.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Disbursement Debits created  " + NumberToString
      (export.CountsAndAmounts.NbrOfDisbCreated.Count, 10, 6) + "  " + local
      .Sign.Text1 + NumberToString
      ((long)export.CountsAndAmounts.AmtOfDisbCreated.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(export.CountsAndAmounts.AmtOfDisbCreated.TotalCurrency * 100), 14,
      2);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (export.CountsAndAmounts.AmtOfErrors.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else
    {
      local.Sign.Text1 = "";
    }

    local.EabReportSend.RptDetail = "Number of Errors             " + NumberToString
      (export.CountsAndAmounts.NbrOfErrors.Count, 10, 6) + "  " + local
      .Sign.Text1 + NumberToString
      ((long)export.CountsAndAmounts.AmtOfErrors.TotalCurrency, 9, 7) + "." + NumberToString
      ((long)(export.CountsAndAmounts.AmtOfErrors.TotalCurrency * 100), 14, 2);
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

    local.EabReportSend.RptDetail =
      "Number of Collections that require potential duplicate check  " + NumberToString
      (export.CountsAndAmounts.NbrOfDupChecks.Count, 10, 6);
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

    local.EabReportSend.RptDetail = "Suppressed disbursements read:      " + NumberToString
      (export.SupprCounts.DtlSupprRead.Count, 9, 7);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Suppressed disbursements released:  " + NumberToString
      (export.SupprCounts.DtlSupprReleased.Count, 9, 7);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Suppressed disbursements extended:  " + NumberToString
      (export.SupprCounts.DtlSupprExtended.Count, 9, 7);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Suppressed disbursements reduced:   " + NumberToString
      (export.SupprCounts.DtlSupprReduced.Count, 9, 7);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Suppressed disbursements no change: " + NumberToString
      (export.SupprCounts.DtlSupprNoChange.Count, 9, 7);
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

    local.UnformattedAmt.Number112 =
      export.SupprCounts.DtlPSupprAmt.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "P suppressions: " + NumberToString
      (export.SupprCounts.DtlPSuppr.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.UnformattedAmt.Number112 =
      export.SupprCounts.DtlCSupprAmt.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "C suppressions: " + NumberToString
      (export.SupprCounts.DtlCSuppr.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12 + " (Not including FDSO C suppressions).";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.UnformattedAmt.Number112 =
      export.SupprCounts.DtlASupprAmt.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "A suppressions: " + NumberToString
      (export.SupprCounts.DtlASuppr.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.UnformattedAmt.Number112 =
      export.SupprCounts.DtlXSupprAmt.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "X suppressions: " + NumberToString
      (export.SupprCounts.DtlXSuppr.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.UnformattedAmt.Number112 =
      export.SupprCounts.DtlDSupprAmt.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "D suppressions: " + NumberToString
      (export.SupprCounts.DtlDSuppr.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.UnformattedAmt.Number112 =
      export.SupprCounts.DtlYSupprAmt.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "Y suppressions: " + NumberToString
      (export.SupprCounts.DtlYSuppr.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.UnformattedAmt.Number112 =
      export.SupprCounts.DtlZSupprAmt.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "Z suppressions: " + NumberToString
      (export.SupprCounts.DtlZSuppr.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
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

  private void UseCabFormat112AmtField()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 = local.UnformattedAmt.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    local.FormattedAmt.Text12 = useExport.Formatted112AmtField.Text12;
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
      /// A value of NbrOfDisbRead.
      /// </summary>
      [JsonPropertyName("nbrOfDisbRead")]
      public Common NbrOfDisbRead
      {
        get => nbrOfDisbRead ??= new();
        set => nbrOfDisbRead = value;
      }

      /// <summary>
      /// A value of NbrOfDisbCreated.
      /// </summary>
      [JsonPropertyName("nbrOfDisbCreated")]
      public Common NbrOfDisbCreated
      {
        get => nbrOfDisbCreated ??= new();
        set => nbrOfDisbCreated = value;
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
      /// A value of NbrOfDupChecks.
      /// </summary>
      [JsonPropertyName("nbrOfDupChecks")]
      public Common NbrOfDupChecks
      {
        get => nbrOfDupChecks ??= new();
        set => nbrOfDupChecks = value;
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
      /// A value of AmtOfDisbCreated.
      /// </summary>
      [JsonPropertyName("amtOfDisbCreated")]
      public Common AmtOfDisbCreated
      {
        get => amtOfDisbCreated ??= new();
        set => amtOfDisbCreated = value;
      }

      /// <summary>
      /// A value of AmtOfErrors.
      /// </summary>
      [JsonPropertyName("amtOfErrors")]
      public Common AmtOfErrors
      {
        get => amtOfErrors ??= new();
        set => amtOfErrors = value;
      }

      private Common nbrOfDisbRead;
      private Common nbrOfDisbCreated;
      private Common nbrOfErrors;
      private Common nbrOfDupChecks;
      private Common amtOfDisbRead;
      private Common amtOfDisbCreated;
      private Common amtOfErrors;
    }

    /// <summary>A SupprCountsGroup group.</summary>
    [Serializable]
    public class SupprCountsGroup
    {
      /// <summary>
      /// A value of DtlSupprRead.
      /// </summary>
      [JsonPropertyName("dtlSupprRead")]
      public Common DtlSupprRead
      {
        get => dtlSupprRead ??= new();
        set => dtlSupprRead = value;
      }

      /// <summary>
      /// A value of DtlSupprReleased.
      /// </summary>
      [JsonPropertyName("dtlSupprReleased")]
      public Common DtlSupprReleased
      {
        get => dtlSupprReleased ??= new();
        set => dtlSupprReleased = value;
      }

      /// <summary>
      /// A value of DtlSupprExtended.
      /// </summary>
      [JsonPropertyName("dtlSupprExtended")]
      public Common DtlSupprExtended
      {
        get => dtlSupprExtended ??= new();
        set => dtlSupprExtended = value;
      }

      /// <summary>
      /// A value of DtlSupprReduced.
      /// </summary>
      [JsonPropertyName("dtlSupprReduced")]
      public Common DtlSupprReduced
      {
        get => dtlSupprReduced ??= new();
        set => dtlSupprReduced = value;
      }

      /// <summary>
      /// A value of DtlSupprNoChange.
      /// </summary>
      [JsonPropertyName("dtlSupprNoChange")]
      public Common DtlSupprNoChange
      {
        get => dtlSupprNoChange ??= new();
        set => dtlSupprNoChange = value;
      }

      /// <summary>
      /// A value of DtlPSuppr.
      /// </summary>
      [JsonPropertyName("dtlPSuppr")]
      public Common DtlPSuppr
      {
        get => dtlPSuppr ??= new();
        set => dtlPSuppr = value;
      }

      /// <summary>
      /// A value of DtlCSuppr.
      /// </summary>
      [JsonPropertyName("dtlCSuppr")]
      public Common DtlCSuppr
      {
        get => dtlCSuppr ??= new();
        set => dtlCSuppr = value;
      }

      /// <summary>
      /// A value of DtlASuppr.
      /// </summary>
      [JsonPropertyName("dtlASuppr")]
      public Common DtlASuppr
      {
        get => dtlASuppr ??= new();
        set => dtlASuppr = value;
      }

      /// <summary>
      /// A value of DtlXSuppr.
      /// </summary>
      [JsonPropertyName("dtlXSuppr")]
      public Common DtlXSuppr
      {
        get => dtlXSuppr ??= new();
        set => dtlXSuppr = value;
      }

      /// <summary>
      /// A value of DtlDSuppr.
      /// </summary>
      [JsonPropertyName("dtlDSuppr")]
      public Common DtlDSuppr
      {
        get => dtlDSuppr ??= new();
        set => dtlDSuppr = value;
      }

      /// <summary>
      /// A value of DtlPSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlPSupprAmt")]
      public Common DtlPSupprAmt
      {
        get => dtlPSupprAmt ??= new();
        set => dtlPSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlCSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlCSupprAmt")]
      public Common DtlCSupprAmt
      {
        get => dtlCSupprAmt ??= new();
        set => dtlCSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlASupprAmt.
      /// </summary>
      [JsonPropertyName("dtlASupprAmt")]
      public Common DtlASupprAmt
      {
        get => dtlASupprAmt ??= new();
        set => dtlASupprAmt = value;
      }

      /// <summary>
      /// A value of DtlXSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlXSupprAmt")]
      public Common DtlXSupprAmt
      {
        get => dtlXSupprAmt ??= new();
        set => dtlXSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlDSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlDSupprAmt")]
      public Common DtlDSupprAmt
      {
        get => dtlDSupprAmt ??= new();
        set => dtlDSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlYSuppr.
      /// </summary>
      [JsonPropertyName("dtlYSuppr")]
      public Common DtlYSuppr
      {
        get => dtlYSuppr ??= new();
        set => dtlYSuppr = value;
      }

      /// <summary>
      /// A value of DtlZSuppr.
      /// </summary>
      [JsonPropertyName("dtlZSuppr")]
      public Common DtlZSuppr
      {
        get => dtlZSuppr ??= new();
        set => dtlZSuppr = value;
      }

      /// <summary>
      /// A value of DtlYSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlYSupprAmt")]
      public Common DtlYSupprAmt
      {
        get => dtlYSupprAmt ??= new();
        set => dtlYSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlZSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlZSupprAmt")]
      public Common DtlZSupprAmt
      {
        get => dtlZSupprAmt ??= new();
        set => dtlZSupprAmt = value;
      }

      private Common dtlSupprRead;
      private Common dtlSupprReleased;
      private Common dtlSupprExtended;
      private Common dtlSupprReduced;
      private Common dtlSupprNoChange;
      private Common dtlPSuppr;
      private Common dtlCSuppr;
      private Common dtlASuppr;
      private Common dtlXSuppr;
      private Common dtlDSuppr;
      private Common dtlPSupprAmt;
      private Common dtlCSupprAmt;
      private Common dtlASupprAmt;
      private Common dtlXSupprAmt;
      private Common dtlDSupprAmt;
      private Common dtlYSuppr;
      private Common dtlZSuppr;
      private Common dtlYSupprAmt;
      private Common dtlZSupprAmt;
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

    /// <summary>
    /// Gets a value of SupprCounts.
    /// </summary>
    [JsonPropertyName("supprCounts")]
    public SupprCountsGroup SupprCounts
    {
      get => supprCounts ?? (supprCounts = new());
      set => supprCounts = value;
    }

    private CountsAndAmountsGroup countsAndAmounts;
    private SupprCountsGroup supprCounts;
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
