// Program: FN_B656_PRINT_ERROR_N_ABEND_DATA, ID: 371019344, model: 746.
// Short name: SWE02814
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B656_PRINT_ERROR_N_ABEND_DATA.
/// </summary>
[Serializable]
public partial class FnB656PrintErrorNAbendData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B656_PRINT_ERROR_N_ABEND_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB656PrintErrorNAbendData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB656PrintErrorNAbendData.
  /// </summary>
  public FnB656PrintErrorNAbendData(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();

      if (IsEmpty(local.ExitStateWorkArea.Message))
      {
        local.EabReportSend.RptDetail =
          "Exit State: Unknown Error - Exit State Message is Blank.";
      }
      else
      {
        local.EabReportSend.RptDetail = "Exit State: " + local
          .ExitStateWorkArea.Message;
      }

      ExitState = "ACO_NN0000_ALL_OK";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail = "ADABAS abend information follows :";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring("ABEND Type............................................", 1,
        30) + import.AbendData.Type1;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring("ADABAS file action .....................................", 1,
        30) + import.AbendData.AdabasFileAction;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring("ADABAS file number .....................................", 1,
        30) + import.AbendData.AdabasFileNumber;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring("ADABAS response code .....................................",
        1, 30) + import.AbendData.AdabasResponseCd;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (!IsEmpty(import.AbendData.CicsFunctionCd))
      {
        local.EabReportSend.RptDetail =
          Substring(
            "CICS function code............................................", 1,
          30) + import.AbendData.CicsFunctionCd;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      if (!IsEmpty(import.AbendData.CicsResourceNm))
      {
        local.EabReportSend.RptDetail =
          Substring(
            "CICS resource name............................................", 1,
          30) + import.AbendData.CicsResourceNm;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      if (!IsEmpty(import.AbendData.CicsResponseCd))
      {
        local.EabReportSend.RptDetail =
          Substring(
            "CICS response code............................................", 1,
          30) + import.AbendData.CicsResponseCd;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      local.EabReportSend.RptDetail = "";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (import.PaymentRequest.SystemGeneratedIdentifier != 0)
      {
        if (IsEmpty(import.PaymentRequest.Classification))
        {
          local.EabReportSend.RptDetail = "Payment Request ID : " + NumberToString
            (import.PaymentRequest.SystemGeneratedIdentifier, 15) + " - Classification : " +
            "** INVALID VALUE **";
        }
        else
        {
          local.EabReportSend.RptDetail = "Payment Request ID : " + NumberToString
            (import.PaymentRequest.SystemGeneratedIdentifier, 15) + " - Classification : " +
            import.PaymentRequest.Classification;
        }

        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (IsEmpty(import.PaymentRequest.CsePersonNumber))
        {
          local.EabReportSend.RptDetail =
            "Payee Number : **** PAYEE NUMBER IS BLANK ****";
        }
        else
        {
          local.EabReportSend.RptDetail = "Payee Number : " + (
            import.PaymentRequest.CsePersonNumber ?? "");
        }

        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (!Equal(import.PaymentRequest.CsePersonNumber,
          import.PaymentRequest.DesignatedPayeeCsePersonNo) && !
          IsEmpty(import.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          local.EabReportSend.RptDetail = "Designated Payee Number : " + (
            import.PaymentRequest.DesignatedPayeeCsePersonNo ?? "");
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      local.EabReportSend.RptDetail =
        "------------------------------------------------------------------------------------------------------------------------------------";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    if (AsChar(import.CloseInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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

    private AbendData abendData;
    private PaymentRequest paymentRequest;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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

    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
