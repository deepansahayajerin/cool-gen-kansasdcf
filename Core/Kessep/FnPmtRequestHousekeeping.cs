// Program: FN_PMT_REQUEST_HOUSEKEEPING, ID: 372673982, model: 746.
// Short name: SWE02386
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PMT_REQUEST_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnPmtRequestHousekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PMT_REQUEST_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPmtRequestHousekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPmtRequestHousekeeping.
  /// </summary>
  public FnPmtRequestHousekeeping(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	DEscription
    // 4/13/00  Fangman         PR 93146  Changed code cancel warrants and deny 
    // recoveries if the amount is less than one dollar.
    // ---------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail = "";
    export.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.ProgramProcessingInfo.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
      local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    }
    else
    {
      local.EabReportSend.ProcessDate = Now().Date;
      local.EabReportSend.ProgramName = global.UserId;
    }

    local.EabFileHandling.Action = "OPEN";

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Check the exit state coming back from the call to get the pgm processing 
    // info.
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.ExitStateMsg.Text80 = UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail =
        "Error reading program processing info for " + global.UserId + ": " + local
        .ExitStateMsg.Text80;
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.ExitStateMsg.Text80 = UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Error opening Control Report: " + local
        .ExitStateMsg.Text80;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // ***** Interrogate the parameter list.
    if (CharAt(local.ProgramProcessingInfo.ParameterList, 1) == 'Y')
    {
      export.DisplayInd.Flag = "Y";
    }
    else
    {
      export.DisplayInd.Flag = "N";
    }

    if (!IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 2, 10)))
    {
      export.TestPayee.Number =
        TrimEnd(Substring(local.ProgramProcessingInfo.ParameterList, 2, 10));
      local.EabReportSend.RptDetail = "Run only for payee # " + export
        .TestPayee.Number;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail = "";
    }

    if (CharAt(local.ProgramProcessingInfo.ParameterList, 12) == 'Y')
    {
      export.AbendForTestInd.Flag = "Y";
    }
    else
    {
      export.AbendForTestInd.Flag = "N";
    }

    // ***** Get the DB2 commit frequency counts.
    export.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ExitStateMsg.Text80 = UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail =
        "Error reading program checkpoint restart info for " + global.UserId + ": " +
        local.ExitStateMsg.Text80;
    }

    if (!ReadDisbursementStatus())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"PROCESSED\" Disbursement Status with ID of 2.";
        
    }

    if (!ReadPaymentStatus4())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"REQ\" Payment Status with ID of 1.";
    }

    if (!ReadPaymentStatus3())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"RCVPOT\" Payment Status with ID of 27.";
    }

    if (!ReadPaymentStatus1())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"CAN\" Payment Status with ID of 6.";
    }

    if (!ReadPaymentStatus2())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"DENIED\" Payment Status with ID of 23.";
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      UseCabErrorReport3();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
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

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadDisbursementStatus()
  {
    export.PersistentProcessed.Populated = false;

    return Read("ReadDisbursementStatus",
      null,
      (db, reader) =>
      {
        export.PersistentProcessed.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentProcessed.Populated = true;
      });
  }

  private bool ReadPaymentStatus1()
  {
    export.PersistentCanceled.Populated = false;

    return Read("ReadPaymentStatus1",
      null,
      (db, reader) =>
      {
        export.PersistentCanceled.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentCanceled.Populated = true;
      });
  }

  private bool ReadPaymentStatus2()
  {
    export.PersistentDenied.Populated = false;

    return Read("ReadPaymentStatus2",
      null,
      (db, reader) =>
      {
        export.PersistentDenied.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentDenied.Populated = true;
      });
  }

  private bool ReadPaymentStatus3()
  {
    export.PersistentRcvpot.Populated = false;

    return Read("ReadPaymentStatus3",
      null,
      (db, reader) =>
      {
        export.PersistentRcvpot.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentRcvpot.Populated = true;
      });
  }

  private bool ReadPaymentStatus4()
  {
    export.PersistentRequested.Populated = false;

    return Read("ReadPaymentStatus4",
      null,
      (db, reader) =>
      {
        export.PersistentRequested.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        export.PersistentRequested.Populated = true;
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
    /// A value of PersistentProcessed.
    /// </summary>
    [JsonPropertyName("persistentProcessed")]
    public DisbursementStatus PersistentProcessed
    {
      get => persistentProcessed ??= new();
      set => persistentProcessed = value;
    }

    /// <summary>
    /// A value of PersistentRequested.
    /// </summary>
    [JsonPropertyName("persistentRequested")]
    public PaymentStatus PersistentRequested
    {
      get => persistentRequested ??= new();
      set => persistentRequested = value;
    }

    /// <summary>
    /// A value of PersistentRcvpot.
    /// </summary>
    [JsonPropertyName("persistentRcvpot")]
    public PaymentStatus PersistentRcvpot
    {
      get => persistentRcvpot ??= new();
      set => persistentRcvpot = value;
    }

    /// <summary>
    /// A value of PersistentCanceled.
    /// </summary>
    [JsonPropertyName("persistentCanceled")]
    public PaymentStatus PersistentCanceled
    {
      get => persistentCanceled ??= new();
      set => persistentCanceled = value;
    }

    /// <summary>
    /// A value of PersistentDenied.
    /// </summary>
    [JsonPropertyName("persistentDenied")]
    public PaymentStatus PersistentDenied
    {
      get => persistentDenied ??= new();
      set => persistentDenied = value;
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
    /// A value of TestPayee.
    /// </summary>
    [JsonPropertyName("testPayee")]
    public CsePerson TestPayee
    {
      get => testPayee ??= new();
      set => testPayee = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of AbendForTestInd.
    /// </summary>
    [JsonPropertyName("abendForTestInd")]
    public Common AbendForTestInd
    {
      get => abendForTestInd ??= new();
      set => abendForTestInd = value;
    }

    private DisbursementStatus persistentProcessed;
    private PaymentStatus persistentRequested;
    private PaymentStatus persistentRcvpot;
    private PaymentStatus persistentCanceled;
    private PaymentStatus persistentDenied;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson testPayee;
    private DateWorkArea maximum;
    private Common displayInd;
    private Common abendForTestInd;
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
    /// A value of ExitStateMsg.
    /// </summary>
    [JsonPropertyName("exitStateMsg")]
    public WorkArea ExitStateMsg
    {
      get => exitStateMsg ??= new();
      set => exitStateMsg = value;
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
    private WorkArea exitStateMsg;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
