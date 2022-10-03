// Program: FN_B694_ADDRESS_CHANGES, ID: 374413918, model: 746.
// Short name: SWEF694B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B694_ADDRESS_CHANGES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB694AddressChanges: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B694_ADDRESS_CHANGES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB694AddressChanges(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB694AddressChanges.
  /// </summary>
  public FnB694AddressChanges(IContext context, Import import, Export export):
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
    // Initial Version - 04/14/00 SWTTREM
    // ************************************************************
    // *******************************************************************
    // 06/26/01 Tom Bobb WR - 010560
    // In the read for cse_person, added a check to see if there
    // is avlid date in the KPC_Date field. If there is a valid date,
    // this means the court order has been sent to KPC.
    // Also change to timstamp compare in the read of
    // cse_person_address to only look at the date portion
    // only.
    // *******************************************************************
    // *******************************************************************
    // 08/17/01 Mark Ashworth WR - 010560
    // Extract Address changes the same way as Court Order Extract for KPC.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB694Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.LowDate.Date = new DateTime(1901, 1, 1);
    local.FileCount.Count = 1;
    local.BatchTimestampWorkArea.TextDateYyyy =
      NumberToString(Year(local.LastRun.Date), 12, 4);
    local.BatchTimestampWorkArea.TextDateMm =
      NumberToString(Month(local.LastRun.Date), 14, 2);
    local.BatchTimestampWorkArea.TestDateDd =
      NumberToString(Day(local.LastRun.Date), 14, 2);
    local.BatchTimestampWorkArea.IefTimestamp =
      Timestamp(local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
      .BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd);

    // *******************************
    // Set header record attributes
    // *******************************
    local.HeaderRecord.RecordType = "1";
    local.HeaderRecord.TransactionDate =
      NumberToString(DateToInt(local.ProcessDate.Date), 8, 8);
    local.HeaderRecord.ActionCode = "UADD";
    local.HeaderRecord.Userid = "SWEFB694";
    local.HeaderRecord.Timestamp = local.BatchTimestampWorkArea.TextDateYyyy + "-"
      + local.BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd + "-" + local.Header.TestTimeHh + "."
      + local.Header.TextTimeMm + "." + local.Header.TextTimeSs;

    // ***********************************************************
    // Main Processing
    // ************************************************************
    UseFnB694CabProcessAddrChanges();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************
      // UPDATE PROCESSING DATA
      // ****************************
      local.ProgramProcessingInfo.ParameterList = "N" + NumberToString
        (DateToInt(local.ProcessDate.Date), 8, 8);
      UseUpdatePgmProcessingInfo();
    }

    // ****************************
    // CLOSE OUTPUT FILE
    // ****************************
    local.KpcExternalParms.Parm1 = "CF";
    UseFnExtWriteInterfaceFile();
    local.CloseInd.Flag = "Y";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : Close the Error Report.
      UseFnB694PrintErrorLine();
      UseFnB694PrintControlReport();
    }
    else
    {
      // : Report the error that occurred and close the Error Report.
      //   ABEND the procedure once reporting is complete.
      UseFnB694PrintErrorLine();
      UseFnB694PrintControlReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    UseSiCloseAdabas();
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseFnB694CabProcessAddrChanges()
  {
    var useImport = new FnB694CabProcessAddrChanges.Import();
    var useExport = new FnB694CabProcessAddrChanges.Export();

    useImport.FileCount.Count = local.FileCount.Count;
    useImport.LastRun.Date = local.LastRun.Date;
    useImport.ProcessDate.Date = local.ProcessDate.Date;
    useImport.LowDate.Date = local.LowDate.Date;
    useImport.HeaderRecord.Assign(local.HeaderRecord);

    Call(FnB694CabProcessAddrChanges.Execute, useImport, useExport);

    local.AddressesRead.Count = useExport.AddressesRead.Count;
    local.AddressesProcessed.Count = useExport.TotalWritten.Count;
  }

  private void UseFnB694Housekeeping()
  {
    var useImport = new FnB694Housekeeping.Import();
    var useExport = new FnB694Housekeeping.Export();

    Call(FnB694Housekeeping.Execute, useImport, useExport);

    local.RunInTestMode.Flag = useExport.RunInTestMode.Flag;
    local.LastRun.Date = useExport.LastRun.Date;
    local.ProcessDate.Date = useExport.CurrentRun.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseFnB694PrintControlReport()
  {
    var useImport = new FnB694PrintControlReport.Import();
    var useExport = new FnB694PrintControlReport.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;
    useImport.AddressRead.Count = local.AddressesRead.Count;
    useImport.AddressProcessed.Count = local.AddressesProcessed.Count;

    Call(FnB694PrintControlReport.Execute, useImport, useExport);
  }

  private void UseFnB694PrintErrorLine()
  {
    var useImport = new FnB694PrintErrorLine.Import();
    var useExport = new FnB694PrintErrorLine.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB694PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnExtWriteInterfaceFile()
  {
    var useImport = new FnExtWriteInterfaceFile.Import();
    var useExport = new FnExtWriteInterfaceFile.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteInterfaceFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseUpdatePgmProcessingInfo()
  {
    var useImport = new UpdatePgmProcessingInfo.Import();
    var useExport = new UpdatePgmProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdatePgmProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// A value of FileCount.
    /// </summary>
    [JsonPropertyName("fileCount")]
    public Common FileCount
    {
      get => fileCount ??= new();
      set => fileCount = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public Common Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public BatchTimestampWorkArea Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of RunInTestMode.
    /// </summary>
    [JsonPropertyName("runInTestMode")]
    public Common RunInTestMode
    {
      get => runInTestMode ??= new();
      set => runInTestMode = value;
    }

    /// <summary>
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
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
    /// A value of LowDate.
    /// </summary>
    [JsonPropertyName("lowDate")]
    public DateWorkArea LowDate
    {
      get => lowDate ??= new();
      set => lowDate = value;
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

    /// <summary>
    /// A value of AddressesRead.
    /// </summary>
    [JsonPropertyName("addressesRead")]
    public Common AddressesRead
    {
      get => addressesRead ??= new();
      set => addressesRead = value;
    }

    /// <summary>
    /// A value of AddressesProcessed.
    /// </summary>
    [JsonPropertyName("addressesProcessed")]
    public Common AddressesProcessed
    {
      get => addressesProcessed ??= new();
      set => addressesProcessed = value;
    }

    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    /// <summary>
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public HeaderRecord HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
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

    private Common fileCount;
    private Common process;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private BatchTimestampWorkArea header;
    private Common runInTestMode;
    private DateWorkArea lastRun;
    private DateWorkArea processDate;
    private DateWorkArea lowDate;
    private Common closeInd;
    private Common addressesRead;
    private Common addressesProcessed;
    private KpcExternalParms kpcExternalParms;
    private HeaderRecord headerRecord;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
