// Program: OE_B467_PRINT_TOTALS, ID: 374473153, model: 746.
// Short name: SWE02709
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B467_PRINT_TOTALS.
/// </summary>
[Serializable]
public partial class OeB467PrintTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B467_PRINT_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB467PrintTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB467PrintTotals.
  /// </summary>
  public OeB467PrintTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Send.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Debt Details read                  " + NumberToString
      (export.CountsAndAmounts.NbrOfDebtDtlsRead.Count, 9, 7) + "  " + " ";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Non-Foster Care Debt Details read  " + NumberToString
      (export.CountsAndAmounts.NbrOfNonFcDebtDtls.Count, 9, 7) + "  " + " ";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ForFormatting.Number112 =
      export.CountsAndAmounts.AmtOfFcDebtDtlsRead.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "Foster Care Debt Details read      " + NumberToString
      (export.CountsAndAmounts.NbrOfFcDebtDtls.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ForFormatting.Number112 =
      export.CountsAndAmounts.AmtOfMoUrasCreated.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "Monthly URA Summaries created      " + NumberToString
      (export.CountsAndAmounts.NbrOfMoUrasCreated.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ForFormatting.Number112 =
      export.CountsAndAmounts.AmtOfMoUrasUpdated.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "Monthly URA Summaries updated      " + NumberToString
      (export.CountsAndAmounts.NbrOfMoUrasUpdated.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "IM Household records created       " + NumberToString
      (export.CountsAndAmounts.NbrOfImHhCreated.Count, 9, 7) + "  " + " ";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "Debt Details updated               " + NumberToString
      (export.CountsAndAmounts.NbrOfDebtDtlsUpdated.Count, 9, 7) + "  " + " ";
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.ForFormatting.Number112 =
      export.CountsAndAmounts.AmtOfErrors.TotalCurrency;
    UseCabFormat112AmtField();
    local.EabReportSend.RptDetail = "Number of errors                   " + NumberToString
      (export.CountsAndAmounts.NbrOfErrors.Count, 9, 7) + "  " + local
      .FormattedAmt.Text12;
    UseCabControlReport();

    if (!Equal(local.Receive.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Send.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Receive.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormat112AmtField()
  {
    var useImport = new CabFormat112AmtField.Import();
    var useExport = new CabFormat112AmtField.Export();

    useImport.Import112AmountField.Number112 = local.ForFormatting.Number112;

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
      /// A value of NbrOfDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsRead")]
      public Common NbrOfDebtDtlsRead
      {
        get => nbrOfDebtDtlsRead ??= new();
        set => nbrOfDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of NbrOfDebtDtlsUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfDebtDtlsUpdated")]
      public Common NbrOfDebtDtlsUpdated
      {
        get => nbrOfDebtDtlsUpdated ??= new();
        set => nbrOfDebtDtlsUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfFcDebtDtls")]
      public Common NbrOfFcDebtDtls
      {
        get => nbrOfFcDebtDtls ??= new();
        set => nbrOfFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfNonFcDebtDtls.
      /// </summary>
      [JsonPropertyName("nbrOfNonFcDebtDtls")]
      public Common NbrOfNonFcDebtDtls
      {
        get => nbrOfNonFcDebtDtls ??= new();
        set => nbrOfNonFcDebtDtls = value;
      }

      /// <summary>
      /// A value of NbrOfImHhCreated.
      /// </summary>
      [JsonPropertyName("nbrOfImHhCreated")]
      public Common NbrOfImHhCreated
      {
        get => nbrOfImHhCreated ??= new();
        set => nbrOfImHhCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasCreated")]
      public Common NbrOfMoUrasCreated
      {
        get => nbrOfMoUrasCreated ??= new();
        set => nbrOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfMoUrasUpdated")]
      public Common NbrOfMoUrasUpdated
      {
        get => nbrOfMoUrasUpdated ??= new();
        set => nbrOfMoUrasUpdated = value;
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
      /// A value of AmtOfFcDebtDtlsRead.
      /// </summary>
      [JsonPropertyName("amtOfFcDebtDtlsRead")]
      public Common AmtOfFcDebtDtlsRead
      {
        get => amtOfFcDebtDtlsRead ??= new();
        set => amtOfFcDebtDtlsRead = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasCreated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasCreated")]
      public Common AmtOfMoUrasCreated
      {
        get => amtOfMoUrasCreated ??= new();
        set => amtOfMoUrasCreated = value;
      }

      /// <summary>
      /// A value of AmtOfMoUrasUpdated.
      /// </summary>
      [JsonPropertyName("amtOfMoUrasUpdated")]
      public Common AmtOfMoUrasUpdated
      {
        get => amtOfMoUrasUpdated ??= new();
        set => amtOfMoUrasUpdated = value;
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

      private Common nbrOfDebtDtlsRead;
      private Common nbrOfDebtDtlsUpdated;
      private Common nbrOfFcDebtDtls;
      private Common nbrOfNonFcDebtDtls;
      private Common nbrOfImHhCreated;
      private Common nbrOfMoUrasCreated;
      private Common nbrOfMoUrasUpdated;
      private Common nbrOfErrors;
      private Common amtOfFcDebtDtlsRead;
      private Common amtOfMoUrasCreated;
      private Common amtOfMoUrasUpdated;
      private Common amtOfErrors;
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
    /// A value of Receive.
    /// </summary>
    [JsonPropertyName("receive")]
    public EabFileHandling Receive
    {
      get => receive ??= new();
      set => receive = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public EabFileHandling Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of ForFormatting.
    /// </summary>
    [JsonPropertyName("forFormatting")]
    public NumericWorkSet ForFormatting
    {
      get => forFormatting ??= new();
      set => forFormatting = value;
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
    private EabFileHandling receive;
    private EabFileHandling send;
    private NumericWorkSet forFormatting;
    private WorkArea formattedAmt;
  }
#endregion
}
