// Program: FN_B635_HOUSEKEEPING, ID: 372270114, model: 746.
// Short name: SWE02353
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B635_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB635Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B635_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB635Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB635Housekeeping.
  /// </summary>
  public FnB635Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.ProgramProcessingInfo.Name = "SWEFB635";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // LOOK AT PPI PARMLIST TO SEE IF AN OBLIGOR WAS SUPPLIED
    // If obligor supplied, only that obligor will be processed.
    // Otherwise all obligors will be processed.
    // **********************************************************
    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 1, 10)))
    {
      export.PpiParameter.ObligorPersonNumber =
        Substring(export.ProgramProcessingInfo.ParameterList, 1, 10);

      // **********************************************************
      // WRITE TO CONTROL REPORT 98
      // **********************************************************
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "The PPI Parmlist requested processing for only obligor number: " + (
          export.PpiParameter.ObligorPersonNumber ?? "") + "   ";
      UseCabControlReport1();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // **********************************************************
    // LOOK AT PPI PARMLIST TO SEE IF REPORT NEEDED INDICATOR
    // is supplied.
    // **********************************************************
    if (CharAt(export.ProgramProcessingInfo.ParameterList, 12) == 'Y')
    {
      export.ReportNeeded.Flag = "Y";

      // **********************************************************
      // WRITE TO CONTROL REPORT 98
      // **********************************************************
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "The PPI Parmlist requested a report to be produced." + "" + "   ";
      UseCabControlReport1();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabFileHandling.Action = "OPEN";

      // **********************************************************
      // OPEN OUTPUT BUSINESS REPORT 01
      // **********************************************************
      local.EabReportSend.RptHeading3 = "";
      local.EabReportSend.NumberOfColHeadings = 3;
      local.EabReportSend.ColHeading1 =
        "   CRD        PERSON     COLLECTION    CRD  CASH RECEIPT   COLLECTION   DISTRIBUTED   REFUNDED       COURT";
        
      local.EabReportSend.ColHeading2 =
        "  STATUS      NUMBER        DATE       SEQ     AMOUNT        AMOUNT       AMOUNT       AMOUNT        ORDER";
        
      local.EabReportSend.ColHeading3 =
        "----------  ----------   ----------    ---  ------------  ------------  -----------  ----------   ------------";
        
      local.EabReportSend.BlankLineAfterHeading = "Y";
      local.EabReportSend.BlankLineAfterColHead = "N";
      UseCabBusinessReport01();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      export.ReportNeeded.Flag = "N";
    }

    // **********************************************************
    // GET COMMIT FREQUENCY AND SEE IF THIS IS A RESTART
    // **********************************************************
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // **** ok, continue processing. Pick up the last Cash Receipt Detail 
      // processed, so that during restart we can skip it ****
      if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y' && IsEmpty
        (export.ProgramProcessingInfo.ParameterList))
      {
        export.Restart.ObligorPersonNumber =
          export.ProgramCheckpointRestart.RestartInfo ?? "";

        // **********************************************************
        // WRITE TO CONTROL REPORT 98
        // **********************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "This program is being restarted after obligor number: " + (
            export.Restart.ObligorPersonNumber ?? "") + "   ";
        UseCabControlReport1();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabControlReport1();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_CHECKPOINT_NF_RB";
    }

    export.RetroCollAdjustment.Code = "RETCOLL";

    if (ReadCollectionAdjustmentReason())
    {
      export.RetroCollAdjustment.Assign(entities.CollectionAdjustmentReason);
    }
    else
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF";

      return;
    }

    if (IsEmpty(export.RetroCollAdjustment.Description))
    {
      export.RetroCollAdjustment.Description = export.RetroCollAdjustment.Name;
    }

    UseFnHardcodedCashReceipting();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.EabReportSend);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    export.SuspendedStatus.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    export.RefundedStatus.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    export.DistributedStatus.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    export.ReleasedStatus.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCollectionAdjustmentReason()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetString(command, "obTrnRlnRsnCd", export.RetroCollAdjustment.Code);
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.Description =
          db.GetNullableString(reader, 3);
        entities.CollectionAdjustmentReason.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    /// <summary>
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CashReceiptDetail Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of SuspendedStatus.
    /// </summary>
    [JsonPropertyName("suspendedStatus")]
    public CashReceiptDetailStatus SuspendedStatus
    {
      get => suspendedStatus ??= new();
      set => suspendedStatus = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of RefundedStatus.
    /// </summary>
    [JsonPropertyName("refundedStatus")]
    public CashReceiptDetailStatus RefundedStatus
    {
      get => refundedStatus ??= new();
      set => refundedStatus = value;
    }

    /// <summary>
    /// A value of DistributedStatus.
    /// </summary>
    [JsonPropertyName("distributedStatus")]
    public CashReceiptDetailStatus DistributedStatus
    {
      get => distributedStatus ??= new();
      set => distributedStatus = value;
    }

    /// <summary>
    /// A value of ReleasedStatus.
    /// </summary>
    [JsonPropertyName("releasedStatus")]
    public CashReceiptDetailStatus ReleasedStatus
    {
      get => releasedStatus ??= new();
      set => releasedStatus = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of RetroCollAdjustment.
    /// </summary>
    [JsonPropertyName("retroCollAdjustment")]
    public CollectionAdjustmentReason RetroCollAdjustment
    {
      get => retroCollAdjustment ??= new();
      set => retroCollAdjustment = value;
    }

    private Common reportNeeded;
    private CashReceiptDetail ppiParameter;
    private CashReceiptDetail restart;
    private CashReceiptDetailStatus suspendedStatus;
    private DateWorkArea max;
    private CashReceiptDetailStatus refundedStatus;
    private CashReceiptDetailStatus distributedStatus;
    private CashReceiptDetailStatus releasedStatus;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private CollectionAdjustmentReason retroCollAdjustment;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    private CollectionAdjustmentReason collectionAdjustmentReason;
  }
#endregion
}
