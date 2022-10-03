// Program: FN_B656_PRINT_CONTROL_TOTALS, ID: 372720554, model: 746.
// Short name: SWE02331
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B656_PRINT_CONTROL_TOTALS.
/// </summary>
[Serializable]
public partial class FnB656PrintControlTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B656_PRINT_CONTROL_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB656PrintControlTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB656PrintControlTotals.
  /// </summary>
  public FnB656PrintControlTotals(IContext context, Import import, Export export)
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
    // ***********************************************************
    // Initial Version - ??????
    // 10/12/00 SWSRKXD PR#105240
    // Add AE Income records totals.
    // **********************************************************
    local.NoOfPymntReqsInError.Count = import.NoOfPaymentRequestRead.Count - import
      .NoOfPaymentsProcessed.Count;
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "RUN RESULTS AS FOLLOWS. . . . . . . . .";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Payment_Request records read. . . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.NoOfPaymentRequestRead.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Payment_Request records processed . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.NoOfPaymentsProcessed.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Payment_Request records in error. . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (local.NoOfPymntReqsInError.Count, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "AE Income records created. . . . . . . :";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (import.NoAeRecordsCreated.Count, 15);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "  -  $" +
      NumberToString
      ((long)(import.NoAeRecordsCreated.TotalCurrency * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(import.CloseInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "CLOSE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

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
    /// <summary>
    /// A value of NoAeRecordsCreated.
    /// </summary>
    [JsonPropertyName("noAeRecordsCreated")]
    public Common NoAeRecordsCreated
    {
      get => noAeRecordsCreated ??= new();
      set => noAeRecordsCreated = value;
    }

    /// <summary>
    /// A value of NoOfPaymentRequestRead.
    /// </summary>
    [JsonPropertyName("noOfPaymentRequestRead")]
    public Common NoOfPaymentRequestRead
    {
      get => noOfPaymentRequestRead ??= new();
      set => noOfPaymentRequestRead = value;
    }

    /// <summary>
    /// A value of NoOfPaymentsProcessed.
    /// </summary>
    [JsonPropertyName("noOfPaymentsProcessed")]
    public Common NoOfPaymentsProcessed
    {
      get => noOfPaymentsProcessed ??= new();
      set => noOfPaymentsProcessed = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    private Common noAeRecordsCreated;
    private Common noOfPaymentRequestRead;
    private Common noOfPaymentsProcessed;
    private Common closeInd;
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
    /// A value of NoOfPymntReqsInError.
    /// </summary>
    [JsonPropertyName("noOfPymntReqsInError")]
    public Common NoOfPymntReqsInError
    {
      get => noOfPymntReqsInError ??= new();
      set => noOfPymntReqsInError = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common noOfPymntReqsInError;
  }
#endregion
}
