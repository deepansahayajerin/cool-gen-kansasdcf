// Program: OE_B450_LOAD_FPLS_RESPONSES, ID: 372364128, model: 746.
// Short name: SWEE450B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B450_LOAD_FPLS_RESPONSES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB450LoadFplsResponses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B450_LOAD_FPLS_RESPONSES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB450LoadFplsResponses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB450LoadFplsResponses.
  /// </summary>
  public OeB450LoadFplsResponses(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------
    //   DATE		Developer	Description
    // 01/13/1996     	T.O.Redmond	Initial Creation
    // 08/07/1996      R. Welborn      Revise for Header Record
    // 01/03/1997	Sid		Create Infrastructure Event for FPLS Response received.
    // 11/26/1997	RCG		concat Agency ID into Infra detail text
    // 12/12/1997 	RCG		remove reference to control table/sit no
    // 05/18/2001      V Madhira       PR# 114110
    // The edit -  to eliminate duplication of responses if a response has been 
    // received for a request from the same agency within the past 7 days, then
    // no update will occur.- will be changed . SME wants to drop the 7 day time
    // frame.
    // 10/01/2001      E Lyman         PR #126409   Too many alerts being 
    // generated.
    // 02/01/2006      DDupree         WR258947 Added new views for a 'A03' 
    // agency.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeB450Housekeeping();

    do
    {
      // ************************************************
      // *Call external to READ the driver file.        *
      // ************************************************
      local.PassArea.FileInstruction = "READ";
      UseOeEabReceiveFplsResponse();

      if (Equal(local.PassArea.TextReturnCode, "EF"))
      {
        break;
      }

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      UseOeB450ProcessFplsResponse();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // ************************************************
      // *Check the number of reads, and updates that   *
      // *have occurred since the last checkpoint.      *
      // ************************************************
      if (local.DatabaseActivity.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.DatabaseActivity.Count = 0;

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
      UseOeB450Close();
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

      UseOeB450Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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

  private void UseOeB450Close()
  {
    var useImport = new OeB450Close.Import();
    var useExport = new OeB450Close.Export();

    useImport.AlertsCreated.Count = local.FplsAlertsCreated.Count;
    useImport.ResponsesSkipped.Count = local.FplsResponsesSkipped.Count;
    useImport.RequestsCreated.Count = local.FplsRequestsCreated.Count;
    useImport.RequestsUpdated.Count = local.FplsRequestsUpdated.Count;
    useImport.ResponsesCreated.Count = local.FplsResponsesCreated.Count;

    Call(OeB450Close.Execute, useImport, useExport);
  }

  private void UseOeB450Housekeeping()
  {
    var useImport = new OeB450Housekeeping.Import();
    var useExport = new OeB450Housekeeping.Export();

    Call(OeB450Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseOeB450ProcessFplsResponse()
  {
    var useImport = new OeB450ProcessFplsResponse.Import();
    var useExport = new OeB450ProcessFplsResponse.Export();

    useImport.DateOfDeathIndicator.Text1 = local.DateOfDeathIndicator.Text1;
    useImport.FplsAlertsCreated.Count = local.FplsAlertsCreated.Count;
    useImport.FplsResponsesSkipped.Count = local.FplsResponsesSkipped.Count;
    useImport.DatabaseActivity.Count = local.DatabaseActivity.Count;
    useImport.FplsRequestsCreated.Count = local.FplsRequestsCreated.Count;
    useImport.FplsRequestsUpdated.Count = local.FplsRequestsUpdated.Count;
    useImport.FplsResponsesCreated.Count = local.FplsResponsesCreated.Count;
    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ExternalFplsResponse.Assign(local.ExternalFplsResponse);

    Call(OeB450ProcessFplsResponse.Execute, useImport, useExport);

    local.FplsAlertsCreated.Count = useExport.FplsAlertsCreated.Count;
    local.FplsResponsesSkipped.Count = useExport.FplsResponsesSkipped.Count;
    local.DatabaseActivity.Count = useExport.DatabaseActivity.Count;
    local.FplsRequestsCreated.Count = useExport.FplsRequestsCreated.Count;
    local.FplsRequestsUpdated.Count = useExport.FplsRequestsUpdated.Count;
    local.FplsResponsesCreated.Count = useExport.FplsResponsesCreated.Count;
  }

  private void UseOeEabReceiveFplsResponse()
  {
    var useImport = new OeEabReceiveFplsResponse.Import();
    var useExport = new OeEabReceiveFplsResponse.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.ExternalFplsResponse.Assign(local.ExternalFplsResponse);
    useExport.DateOfDeathIndicator.Text1 = local.DateOfDeathIndicator.Text1;
    useExport.External.Assign(local.PassArea);

    Call(OeEabReceiveFplsResponse.Execute, useImport, useExport);

    local.ExternalFplsResponse.Assign(useExport.ExternalFplsResponse);
    local.DateOfDeathIndicator.Text1 = useExport.DateOfDeathIndicator.Text1;
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
    /// A value of DateOfDeathIndicator.
    /// </summary>
    [JsonPropertyName("dateOfDeathIndicator")]
    public TextWorkArea DateOfDeathIndicator
    {
      get => dateOfDeathIndicator ??= new();
      set => dateOfDeathIndicator = value;
    }

    /// <summary>
    /// A value of FplsAlertsCreated.
    /// </summary>
    [JsonPropertyName("fplsAlertsCreated")]
    public Common FplsAlertsCreated
    {
      get => fplsAlertsCreated ??= new();
      set => fplsAlertsCreated = value;
    }

    /// <summary>
    /// A value of FplsResponsesSkipped.
    /// </summary>
    [JsonPropertyName("fplsResponsesSkipped")]
    public Common FplsResponsesSkipped
    {
      get => fplsResponsesSkipped ??= new();
      set => fplsResponsesSkipped = value;
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
    /// A value of DatabaseActivity.
    /// </summary>
    [JsonPropertyName("databaseActivity")]
    public Common DatabaseActivity
    {
      get => databaseActivity ??= new();
      set => databaseActivity = value;
    }

    /// <summary>
    /// A value of FplsRequestsCreated.
    /// </summary>
    [JsonPropertyName("fplsRequestsCreated")]
    public Common FplsRequestsCreated
    {
      get => fplsRequestsCreated ??= new();
      set => fplsRequestsCreated = value;
    }

    /// <summary>
    /// A value of FplsRequestsUpdated.
    /// </summary>
    [JsonPropertyName("fplsRequestsUpdated")]
    public Common FplsRequestsUpdated
    {
      get => fplsRequestsUpdated ??= new();
      set => fplsRequestsUpdated = value;
    }

    /// <summary>
    /// A value of FplsResponsesCreated.
    /// </summary>
    [JsonPropertyName("fplsResponsesCreated")]
    public Common FplsResponsesCreated
    {
      get => fplsResponsesCreated ??= new();
      set => fplsResponsesCreated = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
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
    /// A value of ExternalFplsResponse.
    /// </summary>
    [JsonPropertyName("externalFplsResponse")]
    public ExternalFplsResponse ExternalFplsResponse
    {
      get => externalFplsResponse ??= new();
      set => externalFplsResponse = value;
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

    private TextWorkArea dateOfDeathIndicator;
    private Common fplsAlertsCreated;
    private Common fplsResponsesSkipped;
    private ExitStateWorkArea exitStateWorkArea;
    private Common databaseActivity;
    private Common fplsRequestsCreated;
    private Common fplsRequestsUpdated;
    private Common fplsResponsesCreated;
    private FplsLocateResponse fplsLocateResponse;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private ExternalFplsResponse externalFplsResponse;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
