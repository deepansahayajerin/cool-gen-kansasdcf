// Program: FN_B632_HOUSEKEEPING, ID: 372279517, model: 746.
// Short name: SWE02320
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B632_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB632Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B632_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB632Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB632Housekeeping.
  /// </summary>
  public FnB632Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(export.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    export.StartCashReceipt.SequentialNumber =
      (int)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 1, 9));
    export.StartCashReceiptDetail.SequentialIdentifier =
      (int)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 11, 4));
    export.EndCashReceipt.SequentialNumber =
      (int)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 16, 9));
    export.EndCashReceiptDetail.SequentialIdentifier =
      (int)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 26, 4));

    if (export.EndCashReceipt.SequentialNumber == 0)
    {
      export.EndCashReceipt.SequentialNumber = 999999999;
    }

    if (export.EndCashReceiptDetail.SequentialIdentifier == 0)
    {
      export.EndCashReceiptDetail.SequentialIdentifier = 9999;
    }

    export.OpmtWaitPeriod.Count =
      (int)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 31, 2));

    if (export.OpmtWaitPeriod.Count == 0)
    {
      export.OpmtWaitPeriod.Count = 1;
    }

    export.AllowITypeProcInd.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 34, 1);

    if (IsEmpty(export.AllowITypeProcInd.Flag))
    {
      export.AllowITypeProcInd.Flag = "Y";
    }

    export.ItypeWindow.Count =
      (int)StringToNumber(Substring(
        export.ProgramProcessingInfo.ParameterList, 36, 2));

    if (export.ItypeWindow.Count == 0)
    {
      export.ItypeWindow.Count = 10;
    }

    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = export.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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

    private CashReceipt startCashReceipt;
    private CashReceiptDetail startCashReceiptDetail;
    private CashReceipt endCashReceipt;
    private CashReceiptDetail endCashReceiptDetail;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common opmtWaitPeriod;
    private Common allowITypeProcInd;
    private Common itypeWindow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
  }
#endregion
}
