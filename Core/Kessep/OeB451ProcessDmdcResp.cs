// Program: OE_B451_PROCESS_DMDC_RESP, ID: 371306815, model: 746.
// Short name: SWEE451B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B451_PROCESS_DMDC_RESP.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB451ProcessDmdcResp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B451_PROCESS_DMDC_RESP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB451ProcessDmdcResp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB451ProcessDmdcResp.
  /// </summary>
  public OeB451ProcessDmdcResp(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 03/01/2006      DDupree   	Initial Creation
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB451Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    do
    {
      // ************************************************
      // *Call external to READ the driver file.        *
      // ************************************************
      local.PassArea.FileInstruction = "READ";
      UseOeEabReceiveFplsDmdcRespon();

      if (Equal(local.PassArea.TextReturnCode, "EF"))
      {
        break;
      }

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      UseOeB451ProcessDmdcResponse();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // ************************************************
      // *Check the number of reads, and updates that   *
      // *have occurred since the last checkpoint.      *
      // ************************************************
      if (local.DmdcProcessed.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.DmdcProcessed.Count = 0;

        // ************************************************
        // *Call an external that does a DB2 commit using *
        // *a Cobol program.
        // 
        // *
        // ************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          break;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Commit Taken: " + NumberToString
          (TimeToInt(Time(Now())), 15);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB451Close();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseOeB451Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveDmdcProMatchResponse(DmdcProMatchResponse source,
    DmdcProMatchResponse target)
  {
    target.RecordId = source.RecordId;
    target.TransmitterStateCode = source.TransmitterStateCode;
    target.FipsCountyCode = source.FipsCountyCode;
    target.CaseId = source.CaseId;
    target.OrderIndicator = source.OrderIndicator;
    target.ChFirstName = source.ChFirstName;
    target.ChMiddleName = source.ChMiddleName;
    target.ChLastName = source.ChLastName;
    target.ChSsn = source.ChSsn;
    target.ChSsnVerifiedInd = source.ChSsnVerifiedInd;
    target.ChMemberId = source.ChMemberId;
    target.ChDeathIndicator = source.ChDeathIndicator;
    target.ChMedicalCoverageIndicator = source.ChMedicalCoverageIndicator;
    target.ChMedicalCoverageSponsorCode = source.ChMedicalCoverageSponsorCode;
    target.ChMedicalCoverageBeginDate = source.ChMedicalCoverageBeginDate;
    target.ChMedicalCoverageEndDate = source.ChMedicalCoverageEndDate;
    target.NcpFirstName = source.NcpFirstName;
    target.NcpMiddleName = source.NcpMiddleName;
    target.NcpLastName = source.NcpLastName;
    target.NcpSsn = source.NcpSsn;
    target.NcpSsnVerifiedIndicator = source.NcpSsnVerifiedIndicator;
    target.NcpMemberId = source.NcpMemberId;
    target.NcpDeathIndicator = source.NcpDeathIndicator;
    target.NcpInTheMilitaryIndicator = source.NcpInTheMilitaryIndicator;
    target.PfFirstName = source.PfFirstName;
    target.PfMiddleName = source.PfMiddleName;
    target.PfLastName = source.PfLastName;
    target.PfSsn = source.PfSsn;
    target.PfSsnVerifiedIndicator = source.PfSsnVerifiedIndicator;
    target.PfMemberId = source.PfMemberId;
    target.PfInTheMilitaryIndicator = source.PfInTheMilitaryIndicator;
    target.CpFirstName = source.CpFirstName;
    target.CpMiddleName = source.CpMiddleName;
    target.CpLastName = source.CpLastName;
    target.CpSsn = source.CpSsn;
    target.CpSsnVerifiedIndicator = source.CpSsnVerifiedIndicator;
    target.CpMemberId = source.CpMemberId;
    target.CpDeathIndicator = source.CpDeathIndicator;
    target.CpInTheMilitaryIndicator = source.CpInTheMilitaryIndicator;
    target.ChSponsorRelCode = source.ChSponsorRelCode;
    target.ChSponsorSsn = source.ChSponsorSsn;
    target.ChSponsorLastName = source.ChSponsorLastName;
    target.ChSponsorFirstName = source.ChSponsorFirstName;
    target.ChSponsorMiddleName = source.ChSponsorMiddleName;
    target.ChSponsorLastNameSuffix = source.ChSponsorLastNameSuffix;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeB451Close()
  {
    var useImport = new OeB451Close.Import();
    var useExport = new OeB451Close.Export();

    useImport.HistoryRecordsCreated.Count = local.HistoryRecordsCreated.Count;
    useImport.DmdcProcessed.Count = local.DmdcProcessed.Count;

    Call(OeB451Close.Execute, useImport, useExport);
  }

  private void UseOeB451Housekeeping()
  {
    var useImport = new OeB451Housekeeping.Import();
    var useExport = new OeB451Housekeeping.Export();

    Call(OeB451Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeB451ProcessDmdcResponse()
  {
    var useImport = new OeB451ProcessDmdcResponse.Import();
    var useExport = new OeB451ProcessDmdcResponse.Export();

    MoveDmdcProMatchResponse(local.DmdcProMatchResponse,
      useImport.DmdcProMatchResponse);
    useImport.HistoryRecordsCreated.Count = local.HistoryRecordsCreated.Count;
    useImport.DmdcProcessed.Count = local.DmdcProcessed.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(OeB451ProcessDmdcResponse.Execute, useImport, useExport);

    local.HistoryRecordsCreated.Count = useExport.HistoryRecordsCreated.Count;
    local.DmdcProcessed.Count = useExport.DmdcProcessed.Count;
  }

  private void UseOeEabReceiveFplsDmdcRespon()
  {
    var useImport = new OeEabReceiveFplsDmdcRespon.Import();
    var useExport = new OeEabReceiveFplsDmdcRespon.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    MoveDmdcProMatchResponse(local.DmdcProMatchResponse,
      useExport.DmdcProMatchResponse);
    useExport.External.Assign(local.PassArea);

    Call(OeEabReceiveFplsDmdcRespon.Execute, useImport, useExport);

    local.DmdcProMatchResponse.Assign(useExport.DmdcProMatchResponse);
    local.PassArea.Assign(useExport.External);
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
    /// <summary>
    /// A value of ExternalFplsResponse.
    /// </summary>
    [JsonPropertyName("externalFplsResponse")]
    public ExternalFplsResponse ExternalFplsResponse
    {
      get => externalFplsResponse ??= new();
      set => externalFplsResponse = value;
    }

    private ExternalFplsResponse externalFplsResponse;
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
    /// A value of DmdcProMatchResponse.
    /// </summary>
    [JsonPropertyName("dmdcProMatchResponse")]
    public DmdcProMatchResponse DmdcProMatchResponse
    {
      get => dmdcProMatchResponse ??= new();
      set => dmdcProMatchResponse = value;
    }

    /// <summary>
    /// A value of HistoryRecordsCreated.
    /// </summary>
    [JsonPropertyName("historyRecordsCreated")]
    public Common HistoryRecordsCreated
    {
      get => historyRecordsCreated ??= new();
      set => historyRecordsCreated = value;
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
    /// A value of FplsRequestsCreatDelete.
    /// </summary>
    [JsonPropertyName("fplsRequestsCreatDelete")]
    public Common FplsRequestsCreatDelete
    {
      get => fplsRequestsCreatDelete ??= new();
      set => fplsRequestsCreatDelete = value;
    }

    /// <summary>
    /// A value of DmdcProcessed.
    /// </summary>
    [JsonPropertyName("dmdcProcessed")]
    public Common DmdcProcessed
    {
      get => dmdcProcessed ??= new();
      set => dmdcProcessed = value;
    }

    /// <summary>
    /// A value of FplsResponsesCrDelete.
    /// </summary>
    [JsonPropertyName("fplsResponsesCrDelete")]
    public Common FplsResponsesCrDelete
    {
      get => fplsResponsesCrDelete ??= new();
      set => fplsResponsesCrDelete = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private DmdcProMatchResponse dmdcProMatchResponse;
    private Common historyRecordsCreated;
    private ExitStateWorkArea exitStateWorkArea;
    private Common fplsRequestsCreatDelete;
    private Common dmdcProcessed;
    private Common fplsResponsesCrDelete;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
