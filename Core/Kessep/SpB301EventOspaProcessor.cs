// Program: SP_B301_EVENT_OSPA_PROCESSOR, ID: 372067170, model: 746.
// Short name: SWEP301B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_B301_EVENT_OSPA_PROCESSOR.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB301EventOspaProcessor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B301_EVENT_OSPA_PROCESSOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB301EventOspaProcessor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB301EventOspaProcessor.
  /// </summary>
  public SpB301EventOspaProcessor(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #       Description
    // --------  ----------	-------------   
    // -------------------------------------------------------------------------------
    // 12/23/96  Siraj Konkader		Initial Dev
    //           HBD - Brother
    // 01/14/97  Alan Samuels			Addition of Part B
    // 05/05/97  Siraj Konkader		Return group error
    // 11/30/99  Claudia Ketterhange		Addition to error processing for first run
    // 08/01/00  PMcElderry	PR # 99841	(- 906 abends)  Eliminated READs of 
    // PROGRAM_RUN - no need for read
    // 					after persistent views were removed from receiving AB 
    // CREATE_PROGRAM_RUN.
    // 04/16/01  SWSRPRM	WR # 225 	Prevent certain event/activity combinations 
    // from appearing on error report
    // 01/17/03  EShirk	Prodfix		Added logic that exits to part B of this 
    // process when all 'Q' status rows
    // 					have been processed under a run mode = 'END'.  Also added
    // 					logic that exits entire process when a END mode and all 'O' status
    // 					rows have been processed.
    // 02/11/03  EShirk	Prodfix		Removed logic that reset the runmode = FULL 
    // when the process completed
    // 					part B of this process.
    // 05/18/07  GVandy	PR297018	Quick and dirty fix to help shutdown times.  A 
    // more permanent fix
    // 					will be implemented when changes are made for PR244532.
    // 04/22/09  GVandy	CQ9788		Re-design/re-write alert optimization logic to 
    // improve performance.
    // 04/14/10  GVandy	CQ966		Use new Infrastructure indexing strategy and 
    // completely restructure the program.
    // -----------------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------------
    // This procedure is divided into two parts, Batch Event Processor and Batch
    // OSP Alert Optimizer.  This was done so that
    // both parts could be run consecutively.  These two parts are essentially 
    // treated as two jobs.  All program run info,
    // checkpoint info, and error processing are handled independently.
    // -----------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -- Format of the PPI parameter is as follows:
    // --
    // -- WaitSecs=xxxx,LogTim=xxx,RunMode=xxxxxxx,Debug=x
    // --
    // -- Bytes 10-13 WaitSecs - The number of Seconds to pause between 
    // executions when
    // -- the Infrastructure queue is empty.
    // --
    // -- Bytes 22-24 LogTim - Processing time threshold which when exceeded 
    // causes
    // -- the event to be logged to the control report.  LogTime is in Tenths of
    // a
    // -- second (e.g. 010 = 1 second, 005 = half second)
    // --
    // -- Bytes 34-40 RunMode - Controls processing flow.  Three valid values 
    // below.
    // --  FULL - continuously process starting with event processing then alert
    // optimization,
    // --  ALERTS - continuously process starting with alert optimization then 
    // event processing,
    // --  END - stop processing
    // --
    // -- Byte 48 Debug - Y will cause info for every processed infrastructure 
    // record to be logged to the contol report
    // --
    // -- The following example WaitSecs is 180 seconds, LogTim is 2.5 seconds,
    // -- and RunMode is FULL
    // --
    // -- WaitSecs=0180,LogTim=025,RunMode=FULL   ,Debug=N
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // -- Read PPI record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB301";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Open the control report.
    // -------------------------------------------------------------------------------------
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_CNTRL_RPT_A";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Log parameters to the control report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";

    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "PPI Parameters: " + TrimEnd
            (local.ProgramProcessingInfo.ParameterList);

          break;
        case 3:
          local.BatchTimestampWorkArea.TextTimestamp = "";
          local.BatchTimestampWorkArea.IefTimestamp = Now();
          UseLeCabConvertTimestamp();
          local.EabReportSend.RptDetail =
            local.BatchTimestampWorkArea.TextTimestamp + "  Program Starting";

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    if (Equal(local.ProgramProcessingInfo.ParameterList, 34, 7, "ALERTS "))
    {
      // -- parm is set indicating we should start with alert optimization 
      // first.
      local.ProcessAlertsFirst.Flag = "Y";
    }

    // -------------------------------------------------------------------------------------
    // -- Continuously process events and alerts until parameter indicates to 
    // END.
    // -------------------------------------------------------------------------------------
    while(!Equal(local.ProgramProcessingInfo.ParameterList, 34, 7, "END    "))
    {
      // -------------------------------------------------------------------------------------
      // -- Call Event Processor CAB to process all infrastructure records in "
      // Q" status.
      // -------------------------------------------------------------------------------------
      if (AsChar(local.ProcessAlertsFirst.Flag) != 'Y')
      {
        UseSpEventProcessor();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -------------------------------------------------------------------------------------
      // -- Call Alert Optimization CAB to process all infrastructure records in
      // "O" status.
      // -------------------------------------------------------------------------------------
      UseSpCabOptimizeAlerts();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // -------------------------------------------------------------------------------------
      // -- If no events and no alerts found to process then SLEEP for specified
      // amount of time.
      // -------------------------------------------------------------------------------------
      if (AsChar(local.ProcessAlertsFirst.Flag) == 'Y')
      {
        // -- We only processed alerts so far.  We don't want to sleep yet.
        //    Go back and see if there are events to be processed.
        local.ProcessAlertsFirst.Flag = "";
      }
      else if (local.EventsProcessed.Count == 0 && local
        .AlertsProcessed.Count == 0)
      {
        // -- If Parameter WaitSecs is NOT NUMERIC default to 0060 seconds (1 
        // Minute)
        local.WaitSeconds.Text10 = "000000" + Substring
          (local.ProgramProcessingInfo.ParameterList, 10, 4);

        if (Verify(local.WaitSeconds.Text10, "0123456789") > 0)
        {
          local.WaitSeconds.Text10 = "0000000060";
        }

        // -- COMMIT must be done before  S L E E P  because threads will be 
        // held during sleep period preventing update to parms
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        // S L E E P
        UseSpEabWait();
      }

      // -------------------------------------------------------------------------------------
      // -- Read PPI.  This is done to determine if the END parameter has been 
      // set since we last read the PPI record.
      // -------------------------------------------------------------------------------------
      UseReadProgramProcessingInfo();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // -- Log end time to the control report.
    // -------------------------------------------------------------------------------------
    local.BatchTimestampWorkArea.TextTimestamp = "";
    local.BatchTimestampWorkArea.IefTimestamp = Now();
    UseLeCabConvertTimestamp();
    local.EabReportSend.RptDetail =
      local.BatchTimestampWorkArea.TextTimestamp + "  Program Ending";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Close the control report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpCabOptimizeAlerts()
  {
    var useImport = new SpCabOptimizeAlerts.Import();
    var useExport = new SpCabOptimizeAlerts.Export();

    Call(SpCabOptimizeAlerts.Execute, useImport, useExport);

    local.AlertsProcessed.Count = useExport.AlertsProcessed.Count;
  }

  private void UseSpEabWait()
  {
    var useImport = new SpEabWait.Import();
    var useExport = new SpEabWait.Export();

    useImport.WaitSeconds.Text10 = local.WaitSeconds.Text10;

    Call(SpEabWait.Execute, useImport, useExport);
  }

  private void UseSpEventProcessor()
  {
    var useImport = new SpEventProcessor.Import();
    var useExport = new SpEventProcessor.Export();

    Call(SpEventProcessor.Execute, useImport, useExport);

    local.EventsProcessed.Count = useExport.EventsProcessed.Count;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of ProcessAlertsFirst.
    /// </summary>
    [JsonPropertyName("processAlertsFirst")]
    public Common ProcessAlertsFirst
    {
      get => processAlertsFirst ??= new();
      set => processAlertsFirst = value;
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
    /// A value of EventsProcessed.
    /// </summary>
    [JsonPropertyName("eventsProcessed")]
    public Common EventsProcessed
    {
      get => eventsProcessed ??= new();
      set => eventsProcessed = value;
    }

    /// <summary>
    /// A value of AlertsProcessed.
    /// </summary>
    [JsonPropertyName("alertsProcessed")]
    public Common AlertsProcessed
    {
      get => alertsProcessed ??= new();
      set => alertsProcessed = value;
    }

    /// <summary>
    /// A value of WaitSeconds.
    /// </summary>
    [JsonPropertyName("waitSeconds")]
    public TextWorkArea WaitSeconds
    {
      get => waitSeconds ??= new();
      set => waitSeconds = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common common;
    private Common processAlertsFirst;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private Common eventsProcessed;
    private Common alertsProcessed;
    private TextWorkArea waitSeconds;
    private External passArea;
  }
#endregion
}
