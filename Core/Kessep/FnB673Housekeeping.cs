// Program: FN_B673_HOUSEKEEPING, ID: 372405146, model: 746.
// Short name: SWE02415
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B673_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB673Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B673_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB673Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB673Housekeeping.
  /// </summary>
  public FnB673Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.Process.Date = local.ProgramProcessingInfo.ProcessDate;
      ++export.NbrOfReads.Count;
      export.TraceIndicator.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);
      export.RollbackForTestInd.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList, 2, 1);
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.Process.Date;
    local.EabReportSend.ProgramName = global.UserId;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.EabReportSend.RptDetail =
        "Critical Error: Not Found reading program processing info for " + local
        .ProgramProcessingInfo.Name;

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }
    else
    {
      if (AsChar(export.TraceIndicator.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Trace Ind = " + export
          .TraceIndicator.Flag;
        UseCabControlReport2();
      }

      if (AsChar(export.RollbackForTestInd.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Rollback for Test Ind = " + export
          .RollbackForTestInd.Flag;
        UseCabControlReport2();
      }
    }

    // Get the DB2 commit frequency counts.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ++export.NbrOfReads.Count;
    }
    else
    {
      return;
    }

    UseFnHardcodedCashReceipting();
    UseFnHardcodedEft();

    // The following types and statuses are not implemented in the hardcoded 
    // action block.
    export.HardcodedEmployer.SystemGeneratedIdentifier = 212;
    export.HardcodedFdso.SystemGeneratedIdentifier = 1;
    export.HardcodedMisc.SystemGeneratedIdentifier = 4;
    local.HardcodedEft.SystemGeneratedIdentifier = 6;
    export.HardcodedRegular.SequentialIdentifier = 1;
    export.HardcodedStateOffset.SequentialIdentifier = 4;
    export.HardcodedIwo.SequentialIdentifier = 6;
    local.HardcodedIntCostRecov.SystemGeneratedIdentifier = 17;
    export.HardcodedDeposited.SystemGeneratedIdentifier = 1;

    // In the next section we will read to get currency on a number of entities 
    // that we expect to use each time this process runs.
    // Get currency for the "EFT" Cash Receipt Type.
    if (ReadCashReceiptType())
    {
      ++export.NbrOfReads.Count;
    }
    else
    {
      export.EabReportSend.RptDetail =
        "Critical Error: Not Found reading EFT Cash Receipt Type =" + NumberToString
        (local.HardcodedEft.SystemGeneratedIdentifier, 15);
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Get currency for the "Deposited" Cash Receipt Status.
    if (ReadCashReceiptStatus())
    {
      ++export.NbrOfReads.Count;
    }
    else
    {
      export.EabReportSend.RptDetail =
        "Critical Error: Not Found reading deposited Cash Receipt Status =" + NumberToString
        (local.HardcodedDeposited.SystemGeneratedIdentifier, 15);
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Get currency for the "Released" Cash_Receipt_Detail_Status.
    if (ReadCashReceiptDetailStatus2())
    {
      ++export.NbrOfReads.Count;
    }
    else
    {
      export.EabReportSend.RptDetail =
        "Critical Error: Not Found reading released Cash Receipt Detail Status =" +
        NumberToString(export.HardcodedReleased.SystemGeneratedIdentifier, 15);
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Get currency for the "Suspend" Cash_Receipt_Detail_Status.
    if (ReadCashReceiptDetailStatus3())
    {
      ++export.NbrOfReads.Count;
    }
    else
    {
      export.EabReportSend.RptDetail =
        "Critical Error: Not Found reading suspend Cash Receipt Detail Status =" +
        NumberToString
        (export.HardcodedSuspended.SystemGeneratedIdentifier, 15);
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Get currency for the "Pended" Cash_Receipt_Detail_Status.
    if (ReadCashReceiptDetailStatus1())
    {
      ++export.NbrOfReads.Count;
    }
    else
    {
      export.EabReportSend.RptDetail =
        "Critical Error: Not Found reading pended Cash Receipt Detail Status =" +
        NumberToString(export.HardcodedPended.SystemGeneratedIdentifier, 15);
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Get currency for the "Cost Recovery Fee INT (Interstate)" 
    // Cash_Receipt_Detail_Fee.
    if (ReadCashReceiptDetailFeeType())
    {
      ++export.NbrOfReads.Count;
    }
    else
    {
      export.EabReportSend.RptDetail =
        "Critical Error: Not Found reading interstate cost recovery fee Cash Receipt Detail Fee =" +
        NumberToString
        (local.HardcodedIntCostRecov.SystemGeneratedIdentifier, 15);
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedDeposited.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
    export.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    export.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    export.HardcodedPended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedEft()
  {
    var useImport = new FnHardcodedEft.Import();
    var useExport = new FnHardcodedEft.Export();

    Call(FnHardcodedEft.Execute, useImport, useExport);

    local.Eft.SystemGeneratedIdentifier =
      useExport.Eft.SystemGeneratedIdentifier;
    local.Inbound.TransmissionType = useExport.Inbound.TransmissionType;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

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

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCashReceiptDetailFeeType()
  {
    export.PersistentIntCostRecov.Populated = false;

    return Read("ReadCashReceiptDetailFeeType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdtlFeeTypeId",
          local.HardcodedIntCostRecov.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        export.PersistentIntCostRecov.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentIntCostRecov.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus1()
  {
    export.PersistentPended.Populated = false;

    return Read("ReadCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          export.HardcodedPended.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        export.PersistentPended.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentPended.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus2()
  {
    export.PersistentReleased.Populated = false;

    return Read("ReadCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          export.HardcodedReleased.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        export.PersistentReleased.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentReleased.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus3()
  {
    export.PersistentSuspended.Populated = false;

    return Read("ReadCashReceiptDetailStatus3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          export.HardcodedSuspended.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        export.PersistentSuspended.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentSuspended.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    export.PersistentDeposited.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedDeposited.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        export.PersistentDeposited.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentDeposited.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    export.PersistentEft.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", local.HardcodedEft.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        export.PersistentEft.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        export.PersistentEft.Populated = true;
      });
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of HardcodedEmployer.
    /// </summary>
    [JsonPropertyName("hardcodedEmployer")]
    public CashReceiptSourceType HardcodedEmployer
    {
      get => hardcodedEmployer ??= new();
      set => hardcodedEmployer = value;
    }

    /// <summary>
    /// A value of HardcodedFdso.
    /// </summary>
    [JsonPropertyName("hardcodedFdso")]
    public CashReceiptSourceType HardcodedFdso
    {
      get => hardcodedFdso ??= new();
      set => hardcodedFdso = value;
    }

    /// <summary>
    /// A value of HardcodedMisc.
    /// </summary>
    [JsonPropertyName("hardcodedMisc")]
    public CashReceiptSourceType HardcodedMisc
    {
      get => hardcodedMisc ??= new();
      set => hardcodedMisc = value;
    }

    /// <summary>
    /// A value of PersistentEft.
    /// </summary>
    [JsonPropertyName("persistentEft")]
    public CashReceiptType PersistentEft
    {
      get => persistentEft ??= new();
      set => persistentEft = value;
    }

    /// <summary>
    /// A value of PersistentDeposited.
    /// </summary>
    [JsonPropertyName("persistentDeposited")]
    public CashReceiptStatus PersistentDeposited
    {
      get => persistentDeposited ??= new();
      set => persistentDeposited = value;
    }

    /// <summary>
    /// A value of HardcodedRegular.
    /// </summary>
    [JsonPropertyName("hardcodedRegular")]
    public CollectionType HardcodedRegular
    {
      get => hardcodedRegular ??= new();
      set => hardcodedRegular = value;
    }

    /// <summary>
    /// A value of HardcodedIwo.
    /// </summary>
    [JsonPropertyName("hardcodedIwo")]
    public CollectionType HardcodedIwo
    {
      get => hardcodedIwo ??= new();
      set => hardcodedIwo = value;
    }

    /// <summary>
    /// A value of HardcodedStateOffset.
    /// </summary>
    [JsonPropertyName("hardcodedStateOffset")]
    public CollectionType HardcodedStateOffset
    {
      get => hardcodedStateOffset ??= new();
      set => hardcodedStateOffset = value;
    }

    /// <summary>
    /// A value of HardcodedReleased.
    /// </summary>
    [JsonPropertyName("hardcodedReleased")]
    public CashReceiptDetailStatus HardcodedReleased
    {
      get => hardcodedReleased ??= new();
      set => hardcodedReleased = value;
    }

    /// <summary>
    /// A value of HardcodedSuspended.
    /// </summary>
    [JsonPropertyName("hardcodedSuspended")]
    public CashReceiptDetailStatus HardcodedSuspended
    {
      get => hardcodedSuspended ??= new();
      set => hardcodedSuspended = value;
    }

    /// <summary>
    /// A value of HardcodedPended.
    /// </summary>
    [JsonPropertyName("hardcodedPended")]
    public CashReceiptDetailStatus HardcodedPended
    {
      get => hardcodedPended ??= new();
      set => hardcodedPended = value;
    }

    /// <summary>
    /// A value of PersistentReleased.
    /// </summary>
    [JsonPropertyName("persistentReleased")]
    public CashReceiptDetailStatus PersistentReleased
    {
      get => persistentReleased ??= new();
      set => persistentReleased = value;
    }

    /// <summary>
    /// A value of PersistentSuspended.
    /// </summary>
    [JsonPropertyName("persistentSuspended")]
    public CashReceiptDetailStatus PersistentSuspended
    {
      get => persistentSuspended ??= new();
      set => persistentSuspended = value;
    }

    /// <summary>
    /// A value of PersistentPended.
    /// </summary>
    [JsonPropertyName("persistentPended")]
    public CashReceiptDetailStatus PersistentPended
    {
      get => persistentPended ??= new();
      set => persistentPended = value;
    }

    /// <summary>
    /// A value of PersistentIntCostRecov.
    /// </summary>
    [JsonPropertyName("persistentIntCostRecov")]
    public CashReceiptDetailFeeType PersistentIntCostRecov
    {
      get => persistentIntCostRecov ??= new();
      set => persistentIntCostRecov = value;
    }

    /// <summary>
    /// A value of HardcodedDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedDeposited")]
    public FundTransactionType HardcodedDeposited
    {
      get => hardcodedDeposited ??= new();
      set => hardcodedDeposited = value;
    }

    /// <summary>
    /// A value of NbrOfReads.
    /// </summary>
    [JsonPropertyName("nbrOfReads")]
    public Common NbrOfReads
    {
      get => nbrOfReads ??= new();
      set => nbrOfReads = value;
    }

    /// <summary>
    /// A value of TraceIndicator.
    /// </summary>
    [JsonPropertyName("traceIndicator")]
    public Common TraceIndicator
    {
      get => traceIndicator ??= new();
      set => traceIndicator = value;
    }

    /// <summary>
    /// A value of RollbackForTestInd.
    /// </summary>
    [JsonPropertyName("rollbackForTestInd")]
    public Common RollbackForTestInd
    {
      get => rollbackForTestInd ??= new();
      set => rollbackForTestInd = value;
    }

    private EabReportSend eabReportSend;
    private DateWorkArea process;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CashReceiptSourceType hardcodedEmployer;
    private CashReceiptSourceType hardcodedFdso;
    private CashReceiptSourceType hardcodedMisc;
    private CashReceiptType persistentEft;
    private CashReceiptStatus persistentDeposited;
    private CollectionType hardcodedRegular;
    private CollectionType hardcodedIwo;
    private CollectionType hardcodedStateOffset;
    private CashReceiptDetailStatus hardcodedReleased;
    private CashReceiptDetailStatus hardcodedSuspended;
    private CashReceiptDetailStatus hardcodedPended;
    private CashReceiptDetailStatus persistentReleased;
    private CashReceiptDetailStatus persistentSuspended;
    private CashReceiptDetailStatus persistentPended;
    private CashReceiptDetailFeeType persistentIntCostRecov;
    private FundTransactionType hardcodedDeposited;
    private Common nbrOfReads;
    private Common traceIndicator;
    private Common rollbackForTestInd;
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
    /// A value of HardcodedEft.
    /// </summary>
    [JsonPropertyName("hardcodedEft")]
    public CashReceiptType HardcodedEft
    {
      get => hardcodedEft ??= new();
      set => hardcodedEft = value;
    }

    /// <summary>
    /// A value of HardcodedDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedDeposited")]
    public CashReceiptStatus HardcodedDeposited
    {
      get => hardcodedDeposited ??= new();
      set => hardcodedDeposited = value;
    }

    /// <summary>
    /// A value of HardcodedIntCostRecov.
    /// </summary>
    [JsonPropertyName("hardcodedIntCostRecov")]
    public CashReceiptDetailFeeType HardcodedIntCostRecov
    {
      get => hardcodedIntCostRecov ??= new();
      set => hardcodedIntCostRecov = value;
    }

    /// <summary>
    /// A value of Inbound.
    /// </summary>
    [JsonPropertyName("inbound")]
    public ElectronicFundTransmission Inbound
    {
      get => inbound ??= new();
      set => inbound = value;
    }

    /// <summary>
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public CashReceiptSourceType Eft
    {
      get => eft ??= new();
      set => eft = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CashReceiptType hardcodedEft;
    private CashReceiptStatus hardcodedDeposited;
    private CashReceiptDetailFeeType hardcodedIntCostRecov;
    private ElectronicFundTransmission inbound;
    private CashReceiptSourceType eft;
  }
#endregion
}
