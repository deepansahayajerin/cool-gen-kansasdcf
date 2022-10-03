// Program: FN_B676_REPORT_ERROR, ID: 372742951, model: 746.
// Short name: SWE02459
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B676_REPORT_ERROR.
/// </summary>
[Serializable]
public partial class FnB676ReportError: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B676_REPORT_ERROR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB676ReportError(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB676ReportError.
  /// </summary>
  public FnB676ReportError(IContext context, Import import, Export export):
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
    UseEabExtractExitStateMessage();
    local.NeededToWrite.RptDetail = "Exit State Message: " + local
      .ExitStateWorkArea.Message;
    ExitState = "ACO_NN0000_ALL_OK";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "Obligor Person Number : " + import
      .Obligor.Number;
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "Supported Person Number : " + import
      .SuppPrsn.Number;
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : Get the Cash Rcpt ID & Cash Rcpt Dtl ID.
    if (ReadCashReceiptCashReceiptDetail())
    {
      local.NeededToWrite.RptDetail = "CR & CRD ID: " + NumberToString
        (entities.ExistingCashReceipt.SequentialNumber, 15) + "-" + NumberToString
        (entities.ExistingCashReceiptDetail.SequentialIdentifier, 15);
    }
    else
    {
      local.NeededToWrite.RptDetail = "ERROR : Unable to read the CR & CRD ID.";
    }

    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "------------------------------------------------------------------------------------------------------------------------------------";
      
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

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

  private bool ReadCashReceiptCashReceiptDetail()
  {
    entities.ExistingCashReceipt.Populated = false;
    entities.ExistingCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId", import.Persistant.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of SuppPrsn.
    /// </summary>
    [JsonPropertyName("suppPrsn")]
    public CsePerson SuppPrsn
    {
      get => suppPrsn ??= new();
      set => suppPrsn = value;
    }

    /// <summary>
    /// A value of Persistant.
    /// </summary>
    [JsonPropertyName("persistant")]
    public Collection Persistant
    {
      get => persistant ??= new();
      set => persistant = value;
    }

    private CsePerson obligor;
    private CsePerson suppPrsn;
    private Collection persistant;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    private Collection collection;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
  }
#endregion
}
