// Program: SP_B307_SET_EP_PARM_FOR_END_RUN, ID: 372236811, model: 746.
// Short name: SWEP307B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B307_SET_EP_PARM_FOR_END_RUN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB307SetEpParmForEndRun: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B307_SET_EP_PARM_FOR_END_RUN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB307SetEpParmForEndRun(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB307SetEpParmForEndRun.
  /// </summary>
  public SpB307SetEpParmForEndRun(IContext context, Import import, Export export)
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
    // ***********************************************************************************************
    // * Date          Developer       Request #      Description
    // ***********************************************************************************************
    // * 07 Mar 99     John C Crook    -              Intital Dev
    // *
    // * 19 Oct 10     GVandy		CQ966 		Modify format of event processor parms
    // ***********************************************************************************************
    // *****************************************************************
    // Purpose of this procedure is to set the parm of the
    // Program_Processing_Info Table so that the Event Processor will
    // execute a END Run.
    // ********************************************
    // Crook  07 Mar 99 ***
    ExitState = "ACO_NN0000_ALL_OK";
    local.Common.SelectChar = "O";
    local.ProgramProcessingInfo.Name = "SWEPB301";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = local.ProgramProcessingInfo.Name + ": IN  " +
      (local.ProgramProcessingInfo.ParameterList ?? "");
    UseCabControlReport();

    // *****************************************************************
    // Program_Processing_Information (Parameter_List)
    // Refer to Event Processor
    // ********************************************
    // Crook  17 Mar 99 ***
    local.ProgramProcessingInfo.ParameterList =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 33) + "END    " +
      Substring(local.ProgramProcessingInfo.ParameterList, 41, 199);
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = local.ProgramProcessingInfo.Name + ": OUT " +
      (local.ProgramProcessingInfo.ParameterList ?? "");
    UseCabControlReport();
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();
    UseUpdateProgramProcessingInfo();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateProgramProcessingInfo()
  {
    var useImport = new UpdateProgramProcessingInfo.Import();
    var useExport = new UpdateProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    Call(UpdateProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
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
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public ProgramProcessingInfo Work
    {
      get => work ??= new();
      set => work = value;
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

    private WorkArea formattedDate;
    private DateWorkArea date;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ProgramRun programRun;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramProcessingInfo work;
    private TextWorkArea waitSeconds;
    private External passArea;
  }
#endregion
}
