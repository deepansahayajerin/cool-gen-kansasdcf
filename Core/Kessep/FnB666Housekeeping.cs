// Program: FN_B666_HOUSEKEEPING, ID: 372727187, model: 746.
// Short name: SWE02421
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B666_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB666Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B666_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB666Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB666Housekeeping.
  /// </summary>
  public FnB666Housekeeping(IContext context, Import import, Export export):
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
    // 6/13/01  Fangman       WR 000235  Changed code to reset the rpt detail 
    // field before checking for errors.
    // ---------------------------------------------------------------------------------------
    // **********************************************************
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.ProgramProcessingInfo.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;

      // ***** Interrogate the parameter list.
      if (CharAt(local.ProgramProcessingInfo.ParameterList, 1) == 'Y')
      {
        export.DisplayInd.Flag = "Y";
      }
      else
      {
        export.DisplayInd.Flag = "N";
      }

      if (CharAt(local.ProgramProcessingInfo.ParameterList, 12) == 'Y')
      {
        export.AbendForTestInd.Flag = "Y";
      }
      else
      {
        export.AbendForTestInd.Flag = "N";
      }
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport1();

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error opening the control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail =
        "Error getting program processing info:  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
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
    }

    if (!IsEmpty(export.TestPayee.Number) || AsChar(export.DisplayInd.Flag) == 'Y'
      || AsChar(export.AbendForTestInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    UseCabSetMaximumDiscontinueDate();

    // ***** Get the DB2 commit frequency counts.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
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
        "Error: Not found reading \"REQ\" Payment Status with ID of 6.";
    }

    if (!ReadPaymentStatus2())
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading \"RCVPOT\" Payment Status with ID of 23.";
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      UseCabErrorReport2();
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
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    export.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.ReadFrequencyCount =
      useExport.ProgramCheckpointRestart.ReadFrequencyCount;
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
    /// A value of TestPayee.
    /// </summary>
    [JsonPropertyName("testPayee")]
    public CsePerson TestPayee
    {
      get => testPayee ??= new();
      set => testPayee = value;
    }

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
    /// A value of AbendForTestInd.
    /// </summary>
    [JsonPropertyName("abendForTestInd")]
    public Common AbendForTestInd
    {
      get => abendForTestInd ??= new();
      set => abendForTestInd = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea maximum;
    private Common displayInd;
    private CsePerson testPayee;
    private DisbursementStatus persistentProcessed;
    private PaymentStatus persistentRequested;
    private PaymentStatus persistentRcvpot;
    private Common abendForTestInd;
    private PaymentStatus persistentCanceled;
    private PaymentStatus persistentDenied;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ExitStateWorkArea exitStateWorkArea;
  }
#endregion
}
