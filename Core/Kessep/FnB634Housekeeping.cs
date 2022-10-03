// Program: FN_B634_HOUSEKEEPING, ID: 372264739, model: 746.
// Short name: SWE02343
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B634_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB634Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B634_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB634Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB634Housekeeping.
  /// </summary>
  public FnB634Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWEFB634";
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

      // **********************************************************
      // OPEN OUTPUT BUSINESS REPORT 01
      // **********************************************************
      local.EabFileHandling.Action = "OPEN";
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
      // **** ok, continue processing.  ****
    }
    else
    {
      ExitState = "PROGRAM_CHECKPOINT_NF_RB";

      return;
    }

    export.RetroPgmChange.Code = "RETPCHG";

    if (ReadCollectionAdjustmentReason3())
    {
      export.RetroPgmChange.Assign(entities.CollectionAdjustmentReason);
    }
    else
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF";

      return;
    }

    export.RetroUraAdj.Code = "RETURAA";

    if (ReadCollectionAdjustmentReason4())
    {
      export.RetroUraAdj.Assign(entities.CollectionAdjustmentReason);
    }
    else
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF";

      return;
    }

    export.RetroCourtOrderNumber.Code = "RETCONC";

    if (ReadCollectionAdjustmentReason1())
    {
      export.RetroCourtOrderNumber.Assign(entities.CollectionAdjustmentReason);
    }
    else
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF";

      return;
    }

    export.RetroDeactCollProtect.Code = "RETDCOP";

    if (ReadCollectionAdjustmentReason2())
    {
      export.RetroDeactCollProtect.Assign(entities.CollectionAdjustmentReason);
    }
    else
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF";

      return;
    }

    UseFnHardcodedCashReceipting();
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
    target.ColHeading2 = source.ColHeading2;
    target.ColHeading3 = source.ColHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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

    export.ReleasedStatus.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    export.DistributedStatus.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    export.Refunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    export.SuspendedStatus.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
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

  private bool ReadCollectionAdjustmentReason1()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason1",
      (db, command) =>
      {
        db.
          SetString(command, "obTrnRlnRsnCd", export.RetroCourtOrderNumber.Code);
          
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

  private bool ReadCollectionAdjustmentReason2()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason2",
      (db, command) =>
      {
        db.
          SetString(command, "obTrnRlnRsnCd", export.RetroDeactCollProtect.Code);
          
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

  private bool ReadCollectionAdjustmentReason3()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason3",
      (db, command) =>
      {
        db.SetString(command, "obTrnRlnRsnCd", export.RetroPgmChange.Code);
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

  private bool ReadCollectionAdjustmentReason4()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason4",
      (db, command) =>
      {
        db.SetString(command, "obTrnRlnRsnCd", export.RetroUraAdj.Code);
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CashReceiptDetail Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of RetroCourtOrderNumber.
    /// </summary>
    [JsonPropertyName("retroCourtOrderNumber")]
    public CollectionAdjustmentReason RetroCourtOrderNumber
    {
      get => retroCourtOrderNumber ??= new();
      set => retroCourtOrderNumber = value;
    }

    /// <summary>
    /// A value of RetroPgmChange.
    /// </summary>
    [JsonPropertyName("retroPgmChange")]
    public CollectionAdjustmentReason RetroPgmChange
    {
      get => retroPgmChange ??= new();
      set => retroPgmChange = value;
    }

    /// <summary>
    /// A value of RetroUraAdj.
    /// </summary>
    [JsonPropertyName("retroUraAdj")]
    public CollectionAdjustmentReason RetroUraAdj
    {
      get => retroUraAdj ??= new();
      set => retroUraAdj = value;
    }

    /// <summary>
    /// A value of RetroDeactCollProtect.
    /// </summary>
    [JsonPropertyName("retroDeactCollProtect")]
    public CollectionAdjustmentReason RetroDeactCollProtect
    {
      get => retroDeactCollProtect ??= new();
      set => retroDeactCollProtect = value;
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
    /// A value of ReleasedStatus.
    /// </summary>
    [JsonPropertyName("releasedStatus")]
    public CashReceiptDetailStatus ReleasedStatus
    {
      get => releasedStatus ??= new();
      set => releasedStatus = value;
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
    /// A value of Refunded.
    /// </summary>
    [JsonPropertyName("refunded")]
    public CashReceiptDetailStatus Refunded
    {
      get => refunded ??= new();
      set => refunded = value;
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
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    private CashReceiptDetail restart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CollectionAdjustmentReason retroCourtOrderNumber;
    private CollectionAdjustmentReason retroPgmChange;
    private CollectionAdjustmentReason retroUraAdj;
    private CollectionAdjustmentReason retroDeactCollProtect;
    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private CashReceiptDetailStatus releasedStatus;
    private CashReceiptDetailStatus distributedStatus;
    private CashReceiptDetailStatus refunded;
    private CashReceiptDetailStatus suspendedStatus;
    private CashReceiptDetail ppiParameter;
    private Common reportNeeded;
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
