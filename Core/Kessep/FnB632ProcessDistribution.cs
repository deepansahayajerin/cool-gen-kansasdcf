// Program: FN_B632_PROCESS_DISTRIBUTION, ID: 372279318, model: 746.
// Short name: SWEF632B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B632_PROCESS_DISTRIBUTION.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This PrAD distributes all of the undistributed or partially 
/// distributed cash receipts to the eligible debts, as determined by the
/// distribution policy rule.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB632ProcessDistribution: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B632_PROCESS_DISTRIBUTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB632ProcessDistribution(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB632ProcessDistribution.
  /// </summary>
  public FnB632ProcessDistribution(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.ApplRunMode.Text8 = "BATCH";
    local.PrworaDateOfConversion.Date = new DateTime(2000, 10, 1);
    UseFnB632Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // : Override I Type Processing to "N".
    local.AllowITypeProcInd.Flag = "N";
    local.ItypeWindow.Count = 0;
    local.ProcessDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "PARAMETERS AS FOLLOWS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "Starting Cash Receipt . . . : " + NumberToString
      (local.StartCashReceipt.SequentialNumber, 15) + "-" + NumberToString
      (local.StartCashReceiptDetail.SequentialIdentifier, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "Ending Cash Receipt . . . . : " + NumberToString
      (local.EndCashReceipt.SequentialNumber, 15) + "-" + NumberToString
      (local.EndCashReceiptDetail.SequentialIdentifier, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseFnAutoDistributeCrdsToDebts();
    local.EabFileHandling.Action = "WRITE";

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail =
        "Auto Distribution has ABENDed due to an unrecoverable error.  See Error Report for more details.";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (IsEmpty(local.ExitStateWorkArea.Message))
      {
        local.NeededToWrite.RptDetail =
          "Unknown Error Has Occurred - An ABEND has been issued.";
      }
      else
      {
        local.NeededToWrite.RptDetail = "Exit State : " + local
          .ExitStateWorkArea.Message;
      }

      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseFnAbConcatCrAndCrd();
      local.NeededToWrite.RptDetail = "Cash Receipt/Detail Number . . : " + local
        .CrdCrComboNo.CrdCrCombo;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.NeededToWrite.RptDetail =
        "------------------------------------------------------------------------------------------------------------------------------------";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.NeededToWrite.RptDetail = "RUN RESULTS AS FOLLOWS:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total Records Read . . . . . . . . . . . . : " + NumberToString
      (local.TotalCrdRead.TotalInteger, 15) + "  -  $ " + NumberToString
      ((long)(local.TotalAmtAttempted.TotalCurrency * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total Records Suspended. . . . . . . . . . : " + NumberToString
      (local.TotalCrdSuspended.TotalInteger, 15) + "  -  $ " + NumberToString
      ((long)(local.TotalAmtSuspended.TotalCurrency * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total Records Processed. . . . . . . . . . : " + NumberToString
      (local.TotalCrdProcessed.TotalInteger, 15) + "  -  $ " + NumberToString
      ((long)(local.TotalAmtProcessed.TotalCurrency * 100), 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Events Raised. . . . . . . . . . . . . . . : " + NumberToString
      (local.TotalEventsRaised.TotalInteger, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Commits Taken  . . . . . . . . . . . . . . : " + NumberToString
      (local.TotalCommitsTaken.TotalInteger, 15);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
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

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
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

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      local.LastProcessedCashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      local.LastProcessedCashReceipt.SequentialNumber;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    local.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnAutoDistributeCrdsToDebts()
  {
    var useImport = new FnAutoDistributeCrdsToDebts.Import();
    var useExport = new FnAutoDistributeCrdsToDebts.Export();

    useImport.StartCashReceipt.SequentialNumber =
      local.StartCashReceipt.SequentialNumber;
    useImport.StartCashReceiptDetail.SequentialIdentifier =
      local.StartCashReceiptDetail.SequentialIdentifier;
    useImport.EndCashReceipt.SequentialNumber =
      local.EndCashReceipt.SequentialNumber;
    useImport.EndCashReceiptDetail.SequentialIdentifier =
      local.EndCashReceiptDetail.SequentialIdentifier;
    useImport.ProgramCheckpointRestart.UpdateFrequencyCount =
      local.ProgramCheckpointRestart.UpdateFrequencyCount;
    useImport.ProcessDate.Date = local.ProcessDate.Date;
    useImport.ApplRunMode.Text8 = local.ApplRunMode.Text8;
    useImport.PrworaDateOfConversion.Date = local.PrworaDateOfConversion.Date;
    useImport.AllowITypeProcInd.Flag = local.AllowITypeProcInd.Flag;
    useImport.ItypeWindow.Count = local.ItypeWindow.Count;

    Call(FnAutoDistributeCrdsToDebts.Execute, useImport, useExport);

    local.TotalCrdRead.TotalInteger = useExport.TotalCrdRead.TotalInteger;
    local.TotalCrdSuspended.TotalInteger =
      useExport.TotalCrdSuspended.TotalInteger;
    local.TotalCrdProcessed.TotalInteger =
      useExport.TotalCrdProcessed.TotalInteger;
    local.TotalAmtAttempted.TotalCurrency =
      useExport.TotalAmtAttempted.TotalCurrency;
    local.TotalAmtSuspended.TotalCurrency =
      useExport.TotalAmtSuspended.TotalCurrency;
    local.TotalAmtProcessed.TotalCurrency =
      useExport.TotalAmtProcessed.TotalCurrency;
    local.TotalEventsRaised.TotalInteger =
      useExport.TotalEventsRaised.TotalInteger;
    local.TotalCommitsTaken.TotalInteger =
      useExport.TotalCommitsTaken.TotalInteger;
    local.LastProcessedCashReceiptDetail.SequentialIdentifier =
      useExport.LastProcessedCashReceiptDetail.SequentialIdentifier;
    local.LastProcessedCashReceipt.SequentialNumber =
      useExport.LastProcessedCashReceipt.SequentialNumber;
  }

  private void UseFnB632Housekeeping()
  {
    var useImport = new FnB632Housekeeping.Import();
    var useExport = new FnB632Housekeeping.Export();

    Call(FnB632Housekeeping.Execute, useImport, useExport);

    local.StartCashReceipt.SequentialNumber =
      useExport.StartCashReceipt.SequentialNumber;
    local.StartCashReceiptDetail.SequentialIdentifier =
      useExport.StartCashReceiptDetail.SequentialIdentifier;
    local.EndCashReceipt.SequentialNumber =
      useExport.EndCashReceipt.SequentialNumber;
    local.EndCashReceiptDetail.SequentialIdentifier =
      useExport.EndCashReceiptDetail.SequentialIdentifier;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    local.OpmtWaitPeriod.Count = useExport.OpmtWaitPeriod.Count;
    local.AllowITypeProcInd.Flag = useExport.AllowITypeProcInd.Flag;
    local.ItypeWindow.Count = useExport.ItypeWindow.Count;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PrworaDateOfConversion.
    /// </summary>
    [JsonPropertyName("prworaDateOfConversion")]
    public DateWorkArea PrworaDateOfConversion
    {
      get => prworaDateOfConversion ??= new();
      set => prworaDateOfConversion = value;
    }

    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of ApplRunMode.
    /// </summary>
    [JsonPropertyName("applRunMode")]
    public TextWorkArea ApplRunMode
    {
      get => applRunMode ??= new();
      set => applRunMode = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of TotalAmtProcessed.
    /// </summary>
    [JsonPropertyName("totalAmtProcessed")]
    public Common TotalAmtProcessed
    {
      get => totalAmtProcessed ??= new();
      set => totalAmtProcessed = value;
    }

    /// <summary>
    /// A value of TotalAmtSuspended.
    /// </summary>
    [JsonPropertyName("totalAmtSuspended")]
    public Common TotalAmtSuspended
    {
      get => totalAmtSuspended ??= new();
      set => totalAmtSuspended = value;
    }

    /// <summary>
    /// A value of TotalAmtAttempted.
    /// </summary>
    [JsonPropertyName("totalAmtAttempted")]
    public Common TotalAmtAttempted
    {
      get => totalAmtAttempted ??= new();
      set => totalAmtAttempted = value;
    }

    /// <summary>
    /// A value of LastProcessedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("lastProcessedCashReceiptDetail")]
    public CashReceiptDetail LastProcessedCashReceiptDetail
    {
      get => lastProcessedCashReceiptDetail ??= new();
      set => lastProcessedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of LastProcessedCashReceipt.
    /// </summary>
    [JsonPropertyName("lastProcessedCashReceipt")]
    public CashReceipt LastProcessedCashReceipt
    {
      get => lastProcessedCashReceipt ??= new();
      set => lastProcessedCashReceipt = value;
    }

    /// <summary>
    /// A value of StartCashReceipt.
    /// </summary>
    [JsonPropertyName("startCashReceipt")]
    public CashReceipt StartCashReceipt
    {
      get => startCashReceipt ??= new();
      set => startCashReceipt = value;
    }

    /// <summary>
    /// A value of StartCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("startCashReceiptDetail")]
    public CashReceiptDetail StartCashReceiptDetail
    {
      get => startCashReceiptDetail ??= new();
      set => startCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of EndCashReceipt.
    /// </summary>
    [JsonPropertyName("endCashReceipt")]
    public CashReceipt EndCashReceipt
    {
      get => endCashReceipt ??= new();
      set => endCashReceipt = value;
    }

    /// <summary>
    /// A value of EndCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("endCashReceiptDetail")]
    public CashReceiptDetail EndCashReceiptDetail
    {
      get => endCashReceiptDetail ??= new();
      set => endCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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

    /// <summary>
    /// A value of TotalCommitsTaken.
    /// </summary>
    [JsonPropertyName("totalCommitsTaken")]
    public Common TotalCommitsTaken
    {
      get => totalCommitsTaken ??= new();
      set => totalCommitsTaken = value;
    }

    /// <summary>
    /// A value of TotalEventsRaised.
    /// </summary>
    [JsonPropertyName("totalEventsRaised")]
    public Common TotalEventsRaised
    {
      get => totalEventsRaised ??= new();
      set => totalEventsRaised = value;
    }

    /// <summary>
    /// A value of TotalCrdSuspended.
    /// </summary>
    [JsonPropertyName("totalCrdSuspended")]
    public Common TotalCrdSuspended
    {
      get => totalCrdSuspended ??= new();
      set => totalCrdSuspended = value;
    }

    /// <summary>
    /// A value of TotalCrdProcessed.
    /// </summary>
    [JsonPropertyName("totalCrdProcessed")]
    public Common TotalCrdProcessed
    {
      get => totalCrdProcessed ??= new();
      set => totalCrdProcessed = value;
    }

    /// <summary>
    /// A value of TotalCrdRead.
    /// </summary>
    [JsonPropertyName("totalCrdRead")]
    public Common TotalCrdRead
    {
      get => totalCrdRead ??= new();
      set => totalCrdRead = value;
    }

    /// <summary>
    /// A value of OpmtWaitPeriod.
    /// </summary>
    [JsonPropertyName("opmtWaitPeriod")]
    public Common OpmtWaitPeriod
    {
      get => opmtWaitPeriod ??= new();
      set => opmtWaitPeriod = value;
    }

    /// <summary>
    /// A value of AllowITypeProcInd.
    /// </summary>
    [JsonPropertyName("allowITypeProcInd")]
    public Common AllowITypeProcInd
    {
      get => allowITypeProcInd ??= new();
      set => allowITypeProcInd = value;
    }

    /// <summary>
    /// A value of ItypeWindow.
    /// </summary>
    [JsonPropertyName("itypeWindow")]
    public Common ItypeWindow
    {
      get => itypeWindow ??= new();
      set => itypeWindow = value;
    }

    private DateWorkArea prworaDateOfConversion;
    private CrdCrComboNo crdCrComboNo;
    private TextWorkArea applRunMode;
    private DateWorkArea processDate;
    private Common totalAmtProcessed;
    private Common totalAmtSuspended;
    private Common totalAmtAttempted;
    private CashReceiptDetail lastProcessedCashReceiptDetail;
    private CashReceipt lastProcessedCashReceipt;
    private CashReceipt startCashReceipt;
    private CashReceiptDetail startCashReceiptDetail;
    private CashReceipt endCashReceipt;
    private CashReceiptDetail endCashReceiptDetail;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalCommitsTaken;
    private Common totalEventsRaised;
    private Common totalCrdSuspended;
    private Common totalCrdProcessed;
    private Common totalCrdRead;
    private Common opmtWaitPeriod;
    private Common allowITypeProcInd;
    private Common itypeWindow;
  }
#endregion
}
