// Program: FN_PRINT_PMT_REQ_CNTRL_TOTALS, ID: 372673978, model: 746.
// Short name: SWE02118
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_PRINT_PMT_REQ_CNTRL_TOTALS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block creates/updates the program control totals for the batch 
/// procedure SWEFB641 that creates warrants and potential recoveries
/// </para>
/// </summary>
[Serializable]
public partial class FnPrintPmtReqCntrlTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PRINT_PMT_REQ_CNTRL_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPrintPmtReqCntrlTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPrintPmtReqCntrlTotals.
  /// </summary>
  public FnPrintPmtReqCntrlTotals(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Number and amount of disb transactions read. . . . . . .";
    local.TextWorkArea.Text8 =
      NumberToString(import.Counts.NbrOfDisbTransRead.Count, 8);
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField2();
    local.EabReportSend.RptDetail =
      Substring(local.EabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 57) + local.TextWorkArea.Text8 + "   " +
      local.Formatted112AmtField.Text12;
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Number and amount of disb transactions updated . . . . .";
    local.TextWorkArea.Text8 =
      NumberToString(import.Counts.NbrOfDisbTransUpdated.Count, 8);
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField2();
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField3();
    local.EabReportSend.RptDetail =
      Substring(local.EabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 57) + local.TextWorkArea.Text8 + "   " +
      local.Formatted112AmtField.Text12;
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Number and amount of warrants created. . . . . . . . . .";
    local.TextWorkArea.Text8 =
      NumberToString(import.Counts.NbrOfWarrantsCreated.Count, 8);
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField4();
    local.EabReportSend.RptDetail =
      Substring(local.EabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 57) + local.TextWorkArea.Text8 + "   " +
      local.Formatted112AmtField.Text12;
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Number and amount of EFTs created. . . . . . . . . . . .";
    local.TextWorkArea.Text8 =
      NumberToString(import.Counts.NbrOfEftsCreated.Count, 8);
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField5();
    local.EabReportSend.RptDetail =
      Substring(local.EabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 57) + local.TextWorkArea.Text8 + "   " +
      local.Formatted112AmtField.Text12;
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Number and amount of recaptures created. . . . . . . . .";
    local.TextWorkArea.Text8 =
      NumberToString(import.Counts.NbrOfRecapturesCreated.Count, 8);
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField6();
    local.EabReportSend.RptDetail =
      Substring(local.EabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 57) + local.TextWorkArea.Text8 + "   " +
      local.Formatted112AmtField.Text12;
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Number and amount of potential recoveries created. . . .";
    local.TextWorkArea.Text8 =
      NumberToString(import.Counts.NbrOfRecoveriesCreated.Count, 8);
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField7();
    local.EabReportSend.RptDetail =
      Substring(local.EabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 57) + local.TextWorkArea.Text8 + "   " +
      local.Formatted112AmtField.Text12;
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Total number and amount of payment requests created. . .";
    local.TotNbrOfPmtReqCreated.Count =
      import.Counts.NbrOfWarrantsCreated.Count + import
      .Counts.NbrOfEftsCreated.Count + import
      .Counts.NbrOfRecapturesCreated.Count + import
      .Counts.NbrOfRecoveriesCreated.Count;
    local.TextWorkArea.Text8 =
      NumberToString(local.TotNbrOfPmtReqCreated.Count, 8);
    local.TotAmtOfPmtRequests.Number112 =
      import.Counts.AmtOfWarrantsCreated.Number112 + import
      .Counts.AmtOfEftsCreated.Number112 + import
      .Counts.AmtOfRecapturesCreated.Number112 + import
      .Counts.AmtOfRecoveriesCreated.Number112;
    local.Formatted112AmtField.Text12 = UseCabFormat112AmtField1();
    local.EabReportSend.RptDetail =
      Substring(local.EabReportSend.RptDetail,
      EabReportSend.RptDetail_MaxLength, 1, 57) + local.TextWorkArea.Text8 + "   " +
      local.Formatted112AmtField.Text12;
    UseCabControlReport();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private string UseCabFormat112AmtField1()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 =
      local.TotAmtOfPmtRequests.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    return useExport.Formatted112AmtField.Text12;
  }

  private string UseCabFormat112AmtField2()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 =
      import.Counts.AmtOfDisbTransRead.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    return useExport.Formatted112AmtField.Text12;
  }

  private string UseCabFormat112AmtField3()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 =
      import.Counts.AmtOfDisbTransUpdated.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    return useExport.Formatted112AmtField.Text12;
  }

  private string UseCabFormat112AmtField4()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 =
      import.Counts.AmtOfWarrantsCreated.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    return useExport.Formatted112AmtField.Text12;
  }

  private string UseCabFormat112AmtField5()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 =
      import.Counts.AmtOfEftsCreated.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    return useExport.Formatted112AmtField.Text12;
  }

  private string UseCabFormat112AmtField6()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 =
      import.Counts.AmtOfRecapturesCreated.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    return useExport.Formatted112AmtField.Text12;
  }

  private string UseCabFormat112AmtField7()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 =
      import.Counts.AmtOfRecoveriesCreated.Number112;

    Call(CabFormat112AmtField.Execute, useImport, useExport);

    return useExport.Formatted112AmtField.Text12;
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
    /// <summary>A CountsGroup group.</summary>
    [Serializable]
    public class CountsGroup
    {
      /// <summary>
      /// A value of NbrOfDisbTransRead.
      /// </summary>
      [JsonPropertyName("nbrOfDisbTransRead")]
      public Common NbrOfDisbTransRead
      {
        get => nbrOfDisbTransRead ??= new();
        set => nbrOfDisbTransRead = value;
      }

      /// <summary>
      /// A value of AmtOfDisbTransRead.
      /// </summary>
      [JsonPropertyName("amtOfDisbTransRead")]
      public NumericWorkSet AmtOfDisbTransRead
      {
        get => amtOfDisbTransRead ??= new();
        set => amtOfDisbTransRead = value;
      }

      /// <summary>
      /// A value of NbrOfDisbTransUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfDisbTransUpdated")]
      public Common NbrOfDisbTransUpdated
      {
        get => nbrOfDisbTransUpdated ??= new();
        set => nbrOfDisbTransUpdated = value;
      }

      /// <summary>
      /// A value of AmtOfDisbTransUpdated.
      /// </summary>
      [JsonPropertyName("amtOfDisbTransUpdated")]
      public NumericWorkSet AmtOfDisbTransUpdated
      {
        get => amtOfDisbTransUpdated ??= new();
        set => amtOfDisbTransUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfWarrantsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfWarrantsCreated")]
      public Common NbrOfWarrantsCreated
      {
        get => nbrOfWarrantsCreated ??= new();
        set => nbrOfWarrantsCreated = value;
      }

      /// <summary>
      /// A value of AmtOfWarrantsCreated.
      /// </summary>
      [JsonPropertyName("amtOfWarrantsCreated")]
      public NumericWorkSet AmtOfWarrantsCreated
      {
        get => amtOfWarrantsCreated ??= new();
        set => amtOfWarrantsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfEftsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfEftsCreated")]
      public Common NbrOfEftsCreated
      {
        get => nbrOfEftsCreated ??= new();
        set => nbrOfEftsCreated = value;
      }

      /// <summary>
      /// A value of AmtOfEftsCreated.
      /// </summary>
      [JsonPropertyName("amtOfEftsCreated")]
      public NumericWorkSet AmtOfEftsCreated
      {
        get => amtOfEftsCreated ??= new();
        set => amtOfEftsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfRecapturesCreated.
      /// </summary>
      [JsonPropertyName("nbrOfRecapturesCreated")]
      public Common NbrOfRecapturesCreated
      {
        get => nbrOfRecapturesCreated ??= new();
        set => nbrOfRecapturesCreated = value;
      }

      /// <summary>
      /// A value of AmtOfRecapturesCreated.
      /// </summary>
      [JsonPropertyName("amtOfRecapturesCreated")]
      public NumericWorkSet AmtOfRecapturesCreated
      {
        get => amtOfRecapturesCreated ??= new();
        set => amtOfRecapturesCreated = value;
      }

      /// <summary>
      /// A value of NbrOfRecoveriesCreated.
      /// </summary>
      [JsonPropertyName("nbrOfRecoveriesCreated")]
      public Common NbrOfRecoveriesCreated
      {
        get => nbrOfRecoveriesCreated ??= new();
        set => nbrOfRecoveriesCreated = value;
      }

      /// <summary>
      /// A value of AmtOfRecoveriesCreated.
      /// </summary>
      [JsonPropertyName("amtOfRecoveriesCreated")]
      public NumericWorkSet AmtOfRecoveriesCreated
      {
        get => amtOfRecoveriesCreated ??= new();
        set => amtOfRecoveriesCreated = value;
      }

      private Common nbrOfDisbTransRead;
      private NumericWorkSet amtOfDisbTransRead;
      private Common nbrOfDisbTransUpdated;
      private NumericWorkSet amtOfDisbTransUpdated;
      private Common nbrOfWarrantsCreated;
      private NumericWorkSet amtOfWarrantsCreated;
      private Common nbrOfEftsCreated;
      private NumericWorkSet amtOfEftsCreated;
      private Common nbrOfRecapturesCreated;
      private NumericWorkSet amtOfRecapturesCreated;
      private Common nbrOfRecoveriesCreated;
      private NumericWorkSet amtOfRecoveriesCreated;
    }

    /// <summary>
    /// Gets a value of Counts.
    /// </summary>
    [JsonPropertyName("counts")]
    public CountsGroup Counts
    {
      get => counts ?? (counts = new());
      set => counts = value;
    }

    private CountsGroup counts;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Formatted112AmtField.
    /// </summary>
    [JsonPropertyName("formatted112AmtField")]
    public WorkArea Formatted112AmtField
    {
      get => formatted112AmtField ??= new();
      set => formatted112AmtField = value;
    }

    /// <summary>
    /// A value of TotNbrOfPmtReqCreated.
    /// </summary>
    [JsonPropertyName("totNbrOfPmtReqCreated")]
    public Common TotNbrOfPmtReqCreated
    {
      get => totNbrOfPmtReqCreated ??= new();
      set => totNbrOfPmtReqCreated = value;
    }

    /// <summary>
    /// A value of TotAmtOfPmtRequests.
    /// </summary>
    [JsonPropertyName("totAmtOfPmtRequests")]
    public NumericWorkSet TotAmtOfPmtRequests
    {
      get => totAmtOfPmtRequests ??= new();
      set => totAmtOfPmtRequests = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private TextWorkArea textWorkArea;
    private WorkArea formatted112AmtField;
    private Common totNbrOfPmtReqCreated;
    private NumericWorkSet totAmtOfPmtRequests;
  }
#endregion
}
